using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;

using EvolutionaryRoseTree.Constraints;
namespace EvolutionaryRoseTree.DataStructures
{
    class ConstrainedBayesionBinaryTree : ConstrainedRoseTree
    {
        public static int[] GTClusterNumber = new int[] { 4, 17 };

        public ConstrainedBayesionBinaryTree(int dataset_index,   //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET                             
            int algorithm_index,                        //BRT,KNN_BRT,SPILLTREE_BRT
            int experiment_index,                       //0 (ROSETREE_PRECISION)
            int random_projection_algorithm_index,      //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
            int model_index,                            //DCM,VMF,BERNOULLI
            int projectdimension,                       //projectdimensions[1]:50
            int k,                                      //k nearest neighbour
            LoadFeatureVectors lfv,                     //load feature vector
            double alpha, double gamma,                 //parameters, see top of this file
            double tau, double kappa, double R_0,       //parameters, see top of this file
            string outputpath,
            double sizePunishMinRatio, double sizePunishMaxRatio) :
            base(dataset_index, algorithm_index, experiment_index, random_projection_algorithm_index, model_index, projectdimension, k, lfv, alpha, gamma, tau, kappa, R_0, outputpath, sizePunishMinRatio, sizePunishMaxRatio)
        {
        }

#if !UNSORTED_CACHE
        public override void CacheNearestNeighbors(RoseTreeNode newnode, int[] nearestneighborlist)
        {
            //try
            {
                int leafnum = this.lfv.featurevectors.Length;
                double basetag = 8 * cacheNNindex++ * leafnum;
                //if (cacheNNindex % 1000 == 0)
                //    Console.WriteLine("~~~cacheNNindex {0}", cacheNNindex);
                //if (newnode.MergeTreeIndex == 3585)
                //    Console.Write("");
                Dictionary<CacheKey, CacheValue> newnodecachedict = newnode.CacheMergePairs;

                for (int i = 0; i < nearestneighborlist.Length; i++)
                {
                    if (nearestneighborlist[i] < 0) continue;
                    RoseTreeNode nearestneighbor = nodearray[nearestneighborlist[i]];

                    //if (nearestneighbor.MergeTreeIndex == 2979)
                    //    Console.Write("");

                    double cache_valuearray_plus_alpha;
                    double[] log_likelihood_part1 = new double[1];
                    double[] log_likelihood_part2 = new double[1];
                    CacheKey[] ck = new CacheKey[1];
                    CacheValue[] cv = new CacheValue[1];

                    double logf = GetLogF(newnode, nearestneighbor, out cache_valuearray_plus_alpha);
#if NEW_CONSTRAINT_MODEL                   
                    double[] log_treeprobability = new double[1];
                    double join_log_treeprobability_ratio = constraint.GetLogJoinTreeProbabilityRatio(newnode, nearestneighbor);
                    log_treeprobability[0] = join_log_treeprobability_ratio + newnode.LogTreeProbability + nearestneighbor.LogTreeProbability;

                    double join_log_likelihood_ratio = PosterierJoinLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, join_log_treeprobability_ratio, out log_likelihood_part1[0], out log_likelihood_part2[0]) - (newnode.log_likelihood_posterior + nearestneighbor.log_likelihood_posterior);
                    for (int r = 0; r < 1; r++)
                        cv[r] = new ConstrainedCacheValue(newnode, nearestneighbor, r, cache_valuearray_plus_alpha, logf, log_likelihood_part1[r], log_likelihood_part2[r], log_treeprobability[r]);
                    int[] depth_difference;
                    double similarity;
                    double[] secondarykey = GetSecondaryKey(cv, out similarity, out depth_difference);
                    ck[0] = new ConstrainedCacheKey(join_log_likelihood_ratio, join_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex, depth_difference[0], similarity, secondarykey[0]);

#else
                    double[] log_treeprobability_ratio = new double[1];
                    double join_log_likelihood_ratio = newnode.JoinLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[0], out log_likelihood_part2[0]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);

                    ConstraintTree.bayesFactor = log_likelihood_part1[0] - log_likelihood_part2[0];
                    double join_log_treeprobability_ratio = constraint.GetLogJoinTreeProbabilityRatio(newnode, nearestneighbor);
                    log_treeprobability_ratio[0] = join_log_treeprobability_ratio;
                    for (int r = 0; r < 1; r++)
                        cv[r] = new ConstrainedCacheValue(newnode, nearestneighbor, r, cache_valuearray_plus_alpha, logf, log_likelihood_part1[r], log_likelihood_part2[r], log_treeprobability_ratio[r]);
                    int[] depth_difference;
                    double similarity;
                    GetSimilarityAndDepthDifference(cv, out similarity, out depth_difference);
                    ck[0] = new ConstrainedCacheKey(join_log_likelihood_ratio + join_log_treeprobability_ratio, join_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex, depth_difference[0], similarity);
#endif

                    for (int r = 0; r < 1; r++)
                    {
                        this.cachedict.Insert(ck[r], cv[r]);
                        newnodecachedict.Add(ck[r], cv[r]);
                        nearestneighbor.CacheMergePairs.Add(ck[r], cv[r]);
                    }

                    if (CacheValueRecord != null) WriteNewlyCachedValue(ck);
                }
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }
#else
        /// UNSORTED_CACHE
        ///------------------Not Implemented-----------------///
#endif

        protected override void GetSimilarityAndDepthDifference(CacheValue[] cv, out double similarity, out int[] depth_difference)
        {
            RoseTreeNode newnode = cv[0].node1;
            RoseTreeNode nearestneighbor = cv[0].node2;

            similarity = newnode.data.Cosine(newnode.data, nearestneighbor.data);
            depth_difference = new int[1]
            {
                Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth),
            };

            //int sizepunishtoosmall0 = 0, sizepunishtoosmall1 = 0;

            //double[] secondarykey = new double[1]
            //{
            //    ConstrainedCacheKey.GetSecondaryKey(cv[0], similarity, depth_difference[0], sizepunishtoosmall0 + sizepunishtoosmall1),
            //};

            //return secondarykey;
        }

    }
}
