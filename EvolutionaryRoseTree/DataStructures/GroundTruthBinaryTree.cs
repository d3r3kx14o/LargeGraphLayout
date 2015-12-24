using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Constants;

using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.Accuracy;
namespace EvolutionaryRoseTree.DataStructures
{
    class GroundTruthBinaryTree : GroundTruthRoseTree
    {
        public GroundTruthBinaryTree(int dataset_index,                          //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET                             
            int algorithm_index,                        //BRT,KNN_BRT,SPILLTREE_BRT
            int experiment_index,                       //0 (ROSETREE_PRECISION)
            int random_projection_algorithm_index,      //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
            int model_index,                            //DCM,VMF,BERNOULLI
            int projectdimension,                       //projectdimensions[1]:50
            int k,                                      //k nearest neighbour
            LoadFeatureVectors lfv,                     //load feature vector
            double alpha, double gamma,                 //parameters, see top of this file
            double tau, double kappa, double R_0,       //parameters, see top of this file
            string outputpath) :
            base(dataset_index, algorithm_index, experiment_index, random_projection_algorithm_index, model_index, projectdimension, k, lfv, alpha, gamma, tau, kappa, R_0, outputpath)
        {
        }

        public override void Run(
            int interval,               //Constant.intervals[0]:30
            out int depth,
            out double log_likelihood)
        {
            mergedtreepointer = lfv.featurevectors.Length;

            CacheCacheClass();              //Cache log values as dictionaries
            if (model_index == Constant.VMF)
                InitializeNodesvMF();
            else
                InitializeNodes();

            MergeLoop(interval);
            FindRoot();
            LabelTreeIndices(out depth);
            UpdateDepthInTree();

            this.spilltree = null;

            log_likelihood = this.root.log_likelihood;
        }

        StreamWriter sw;
        public override void MergeLoop(int interval)
        {
            //calculate labels
            GTMergeOrder = new GroundTruthMergeOrder(lfv);
            sw = InitializeMergeRecordWriter();
            RoseTreeNode[] levelnodes = new RoseTreeNode[nodearray.Length / 2];
            Array.Copy(nodearray, levelnodes, levelnodes.Length);
            int[] levellabels = GTMergeOrder.GetLabels(2);

            for (int level = 2; level >= 0; level--)
            {
                Dictionary<int, List<RoseTreeNode>> clusters = GetClusters(levelnodes, levellabels);

                int nextlevelnodecnt = clusters.Count;
                levelnodes = new RoseTreeNode[nextlevelnodecnt];
                levellabels = new int[nextlevelnodecnt];
                Dictionary<int, int> nextlevelhash = GTMergeOrder.GetLevelHash(level);

                int icluster = 0;
                foreach (KeyValuePair<int, List<RoseTreeNode>> kvp in clusters)
                {
                    int label = kvp.Key;
                    List<RoseTreeNode> clusternodes = kvp.Value;

                    RoseTreeNode clusterroot = MergeClusterNodes(clusternodes);
                    levelnodes[icluster] = clusterroot;
                    levellabels[icluster] = nextlevelhash[label];

                    icluster++;
                }
            }


            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }

        private RoseTreeNode MergeClusterNodes(List<RoseTreeNode> clusternodes)
        {
            if (clusternodes.Count == 1)
                return clusternodes[0];

            List<RoseTreeNode> nextlevelclusternodes;
            RoseTreeNode leftnode = null;
            int ilevel = 0;
            bool leftnodeend = true;
            while (true)
            {
                //remove node: left node
                if (clusternodes.Count % 2 == 1)
                {
                    if (ilevel % 2 == 0)
                    {
                        //remove end
                        leftnode = clusternodes[clusternodes.Count - 1];
                        clusternodes.RemoveAt(clusternodes.Count - 1);
                        leftnodeend = true;
                    }
                    else
                    {
                        //remove front
                        leftnode = clusternodes[0];
                        clusternodes.RemoveAt(0);
                        leftnodeend = false;
                    }
                }
                else
                    leftnode = null;

                //Get new nodes
                nextlevelclusternodes = MergeClusterNodesOneLevel(clusternodes);
                //add left node
                if (leftnode != null)
                {
                    if (leftnodeend)
                        nextlevelclusternodes.Add(leftnode);
                    else
                        nextlevelclusternodes.Insert(0, leftnode);
                }                

                if (nextlevelclusternodes.Count == 1)
                    return nextlevelclusternodes[0];

                clusternodes = nextlevelclusternodes;
                ilevel++;
            }
        }

        private List<RoseTreeNode> MergeClusterNodesOneLevel(List<RoseTreeNode> clusternodes)
        {
            if (clusternodes.Count % 2 != 0)
                throw new Exception("Error! length of clusternodes should be 2n");

            List<RoseTreeNode> newclusternodes = new List<RoseTreeNode>();
            for (int i = 0; i < clusternodes.Count / 2; i++)
            {
                RoseTreeNode newnode = MergeTwoNodes(clusternodes[2 * i], clusternodes[2 * i + 1]);
                newclusternodes.Add(newnode);
            }

            return newclusternodes;
        }

        private Dictionary<int, List<RoseTreeNode>> GetClusters(RoseTreeNode[] levelnodes, int[] levellabels)
        {
            if (levelnodes.Length != levellabels.Length)
                throw new Exception("Error! Label and nodes length do not match!");

            Dictionary<int, List<RoseTreeNode>> clusters = new Dictionary<int, List<RoseTreeNode>>();
            for (int i = 0; i < levellabels.Length; i++)
            {
                RoseTreeNode node = levelnodes[i];
                int label = levellabels[i];
                if (!clusters.ContainsKey(label))
                    clusters.Add(label, new List<RoseTreeNode>());
                clusters[label].Add(node);
            }

            return clusters;
        }

        RoseTreeNode MergeTwoNodes(RoseTreeNode node1, RoseTreeNode node2)
        {
            int m = 0;

            /// Calculate logf and loglikelihood ///
            double log_likelihood_part1, log_likelihood_part2;
            double cache_valuearray_plus_alpha, log_likelihood_ratio;

            double logf = GetLogF(node1, node2, out cache_valuearray_plus_alpha);
             log_likelihood_ratio = node1.JoinLogLikelihood(this.cacheclass, node1, node2, logf, out log_likelihood_part1, out log_likelihood_part2) - (node1.log_likelihood + node2.log_likelihood);

            /// Merge single step ///
            OutputMergeRecord(sw, m, node1, node2, log_likelihood_ratio, log_likelihood_ratio);
            RoseTreeNode newnode = MergeSingleStep(node1, node2, m, log_likelihood_ratio + (node1.log_likelihood + node2.log_likelihood), logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
            UpdateLeafCount(newnode);

            return newnode;
        }
    }
}
