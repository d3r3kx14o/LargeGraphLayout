using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.DrawTree;
using System.IO;

namespace RoseTreeTaxonomy.Algorithms
{
    public class RoseTree
    {
        public int initial_clusternum;
        public int clusternum;
        public int dataset_index;
        public int algorithm_index;
        public int experiment_index;
        public int random_projection_algorithm_index;
        public int model_index;
        public int nodecounter = 0;
        public int cachestamp = 0;
        public int projectdimension; 
        public int k;
        public int rebuild_times_num = 0;

        public RandomProjection projection;
        protected CacheClass cacheclass = new CacheClass();
        protected CacheSortedDictionary cachedict = new CacheSortedDictionary();
        protected kNearestNeighbor kNN = new kNearestNeighbor();

        protected SpillTree spilltree;

        protected RoseTreeNode[] nodearray;
        public string[] querystrings;

        public int[] sample_array;

        public RoseTreeNode root;

        public LoadFeatureVectors lfv;

        public double alpha, gamma, tau, kappa;
        //public double[] mu_0;
        //public double[] idf;//Xiting
        public Dictionary<int,double> idf;
        public double mu_0_each_dimension, mu_0_each_dimension_sqr, R_0;

        public string outputpath;

        public RoseTree(int dataset_index, int algorithm_index, 
                                   int experiment_index, int random_projection_algorithm_index, 
                                   int model_index, int projectdimension, int k, LoadFeatureVectors lfv, 
                                   double alpha, double gamma, double tau, double kappa, double R_0, string outputpath, 
                                   int[] sample_array)
        {
            this.dataset_index = dataset_index;
            this.algorithm_index = algorithm_index;
            this.experiment_index = experiment_index;
            this.random_projection_algorithm_index = random_projection_algorithm_index;
            this.model_index = model_index;
            this.lfv = lfv;
            this.alpha = alpha;
            this.gamma = gamma;
            this.tau = tau;
            this.projectdimension = projectdimension;
            this.k = k;
            this.outputpath = outputpath;
            this.sample_array = sample_array;
            this.kappa = kappa;
            this.R_0 = R_0;
            this.mu_0_each_dimension = R_0 / Math.Sqrt(lfv.lexiconsize);
            this.mu_0_each_dimension_sqr = this.mu_0_each_dimension * this.mu_0_each_dimension;
            this.idf = lfv.idf;
        }

        public RoseTree(
            int dataset_index,                          //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET                             
            int algorithm_index,                        //BRT,KNN_BRT,SPILLTREE_BRT
            int experiment_index,                       //0 (ROSETREE_PRECISION)
            int random_projection_algorithm_index,      //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
            int model_index,                            //DCM,VMF,BERNOULLI
            int projectdimension,                       //projectdimensions[1]:50
            int k,                                      //k nearest neighbour
            LoadFeatureVectors lfv,                     //load feature vector
            double alpha, double gamma,                 //parameters, see top of this file
            double tau, double kappa, double R_0,       //parameters, see top of this file
            string outputpath)
        {
            this.dataset_index = dataset_index;
            this.algorithm_index = algorithm_index;
            this.experiment_index = experiment_index;
            this.random_projection_algorithm_index = random_projection_algorithm_index;
            this.model_index = model_index;
            this.lfv = lfv;
            this.alpha = alpha;
            this.gamma = gamma;
            this.tau = tau;
            this.projectdimension = projectdimension;
            this.k = k;
            this.outputpath = outputpath;
            this.kappa = kappa;
            this.R_0 = R_0;
            this.mu_0_each_dimension = R_0 / Math.Sqrt(lfv.lexiconsize);
            this.mu_0_each_dimension_sqr = this.mu_0_each_dimension * this.mu_0_each_dimension;
            this.idf = lfv.idf;
        }

        public void Run(int interval, int relevant_nearest_neighbor_k, out Dictionary<int, int[]> relevant_nearest_neighbors_array, out int depth)
        {
            CacheCacheClass();
            Initialize();

            if (experiment_index == Constant.ROSETREE_PRECISION)
                relevant_nearest_neighbors_array = RelevantNearestNeighbors(relevant_nearest_neighbor_k);
            else
                relevant_nearest_neighbors_array = null;

            DateTime dt1 = DateTime.Now;
            CacheNearestNeighborsForAll();
            DateTime dt2 = DateTime.Now;
            MergeLoop(interval);
            DateTime dt3 = DateTime.Now;
            FindRoot();
            LabelTreeIndices(out depth);
            
            this.spilltree = null;
            this.cacheclass = null;
        }

        public void Run(int interval, out int depth, out TimeSpan timespan_preprocess, out TimeSpan timespan_merge, out TimeSpan timespan_total)
        {
            CacheCacheClass();
            Initialize();

            DateTime dt1 = DateTime.Now;
            CacheNearestNeighborsForAll();
            DateTime dt2 = DateTime.Now;
            MergeLoop(interval);
            DateTime dt3 = DateTime.Now;
            FindRoot();
            LabelTreeIndices(out depth);

            timespan_preprocess = dt2 - dt1;
            timespan_merge = dt3 - dt2;
            timespan_total = dt3 - dt1;

            this.spilltree = null;
            this.cacheclass = null;
        }

        public void Run(int interval, out int depth)
        {
            CacheCacheClass();
            Initialize();

            CacheNearestNeighborsForAll();
            MergeLoop(interval);
            FindRoot();
            LabelTreeIndices(out depth);

            this.spilltree = null;
            this.cacheclass = null;
        }

        public virtual void Run(
            int interval,               //Constant.intervals[0]:30
            out int depth, 
            out double log_likelihood)
        {
            CacheCacheClass();              //Cache log values as dictionaries
            Initialize();                   //Initialize (random projection,) nodes, (spill tree)

            CacheNearestNeighborsForAll();
            MergeLoop(interval);
            FindRoot();
            LabelTreeIndices(out depth);

            this.spilltree = null;
            //this.cacheclass = null;

            log_likelihood = this.root.log_likelihood;
        }

        public void Initialize()
        {   
            if (this.algorithm_index == Constant.SPILLTREE_BRT)
            {
#if PrintDetailedProcess
                Console.WriteLine("Random Projection start");
#endif
                InitializeRandomProjection();
            }
            if (model_index == Constant.VMF)
                InitializeNodesvMF();
            else
               InitializeNodes();              //Calculate log likelyhood log(f(D|alpha)) for each node
            
            if (this.algorithm_index == Constant.SPILLTREE_BRT)
                InitializeSpillTree();
        }

        public void InitializeSpillTree()
        {
#if PrintDetailedProcess
            Console.WriteLine("Preparing to build the Spill Tree");
#endif
            this.spilltree = new SpillTree(this.k, this.projectdimension, this.tau);
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            //for (int i = 0; i < clusternum; i++)
            //    nodelist.Add(this.nodearray[i]);
            //Modified by Xiting: disable TransferNodes() because it causes troubles (nodearray is changed).
            for (int i = 0; i < nodearray.Length; i++)
                if (nodearray[i] != null && nodearray[i].valid)
                    nodelist.Add(this.nodearray[i]);
            this.spilltree.Build(nodelist);
#if PrintDetailedProcess
            Console.WriteLine("Done with building the Spill Tree");
#endif

            //DrawSpillTree drawtree = new DrawSpillTree(spilltree, outputpath);
            //drawtree.Run();
        }

        public void CacheCacheClass()
        {
            this.cacheclass = new CacheClass(this.alpha, this.gamma, this.lfv.maxdimensionvalue, this.lfv.wordnum, this.lfv.featurevectors.Length, this.lfv.lexiconsize);
            this.cacheclass.Cache();
            //if (model_index == Constant.VMF)
            //    for (int i = 0; i < lfv.lexiconsize; i++)
            //        mu_0[i] *= R_0;
        }

        public void InitializeNodes()
        {
            this.clusternum = this.lfv.featurevectors.Length;
            this.initial_clusternum = clusternum;

            this.nodearray = new RoseTreeNode[2 * this.clusternum];
            this.querystrings = new string[this.clusternum];

            string[] sampleDocIds = this.lfv.GetSampleDocIds();
            if (sampleDocIds == null)
            {
                sampleDocIds = new string[this.clusternum];
                //throw new Exception("No Doc ID! Disable this exception if doc id is not needed!");
            }
            for (int i = 0; i < clusternum; i++)
            {
#if PrintDetailedProcess
                if (i % 10000 == 0)
                    Console.WriteLine("Initializing the " + i + "th node");
#endif
                RoseTreeNode newnode = new RoseTreeNode(null, this.lfv.featurevectors[i],
                    (this.algorithm_index == Constant.SPILLTREE_BRT) ?
                        projection.GenerateProjectData(this.lfv.featurevectors[i]) //RandMat*FeatureVector
                            : null,
                    i, sampleDocIds[i]);
                newnode.indices.initial_index = i;

                double cache_valuearray_plus_alpha;
                double logf_part1 = cacheclass.GetLogFactorials(this.lfv.featurevectors[i].valuearray_sum);
                double part = 0;
                for (int j = 0; j < this.lfv.featurevectors[i].count; j++)
                    part += cacheclass.GetLogFactorials(this.lfv.featurevectors[i].valuearray[j]);
                logf_part1 -= part;
                double log_likelihood = LogF(logf_part1, this.lfv.featurevectors[i].valuearray_sum, null, null, this.lfv.featurevectors[i], 1, null, out cache_valuearray_plus_alpha, this.lfv.featurevectors[i].count);
                CacheNodeValues(newnode, log_likelihood, log_likelihood, logf_part1, 1, 1, log_likelihood, 0);
                newnode.data.cache_valuearray_plus_alpha = cache_valuearray_plus_alpha;

                this.nodearray[i] = newnode;
                this.querystrings[i] = this.lfv.featurevectors[i].querystring;
            }

            this.nodecounter = clusternum;
            //this.lfv.Nullify();
        }


        public void InitializeNodesvMF()
        {
            this.clusternum = this.lfv.featurevectors.Length;
            this.initial_clusternum = clusternum;

            this.nodearray = new RoseTreeNode[2 * this.clusternum];
            this.querystrings = new string[this.clusternum];

            string[] sampleDocIds = this.lfv.GetSampleDocIds();
            if (sampleDocIds == null)
            {
                sampleDocIds = new string[this.clusternum];
                //throw new Exception("No Doc ID! Disable this exception if doc id is not needed!");
            }
            for (int i = 0; i < clusternum; i++)
            {
#if PrintDetailedProcess
                if (i % 10000 == 0)
                    Console.WriteLine("Initializing the " + i + "th node");
#endif
                RoseTreeNode newnode = new RoseTreeNode(null, this.lfv.featurevectors[i],
                    (this.algorithm_index == Constant.SPILLTREE_BRT) ?
                        projection.GenerateProjectData(this.lfv.featurevectors[i]) //RandMat*FeatureVector
                            : null,
                    i, sampleDocIds[i]);
                newnode.indices.initial_index = i;

                double cache_valuearray_plus_alpha = double.MinValue;
                double log_likelihood = LogvMF(1, this.lfv.featurevectors[i], this.lfv.featurevectors[i].count);

                CacheNodeValues(newnode, log_likelihood, log_likelihood, double.MinValue, 1, 1, log_likelihood, 0);
                newnode.data.cache_valuearray_plus_alpha = cache_valuearray_plus_alpha;

                this.nodearray[i] = newnode;
                this.querystrings[i] = this.lfv.featurevectors[i].querystring;
            }

            this.nodecounter = clusternum;
            //this.lfv.Nullify();
        }

        public Dictionary<int, int[]> RelevantNearestNeighbors(int relevant_nearest_neighbor_k)
        {
            RoseTreeNode[] new_nodearray = new RoseTreeNode[clusternum];
            for (int i = 0; i < clusternum; i++)
                new_nodearray[i] = this.nodearray[i];
            Dictionary<int, int[]> relevant_nearest_neighbor_array = new Dictionary<int, int[]>();
            for (int i = 0; i < this.sample_array.Length; i++)
            {
                relevant_nearest_neighbor_array.Add(this.sample_array[i], this.kNN.SearchTree(this.nodearray[this.sample_array[i]], new_nodearray, relevant_nearest_neighbor_k, this.nodearray));
#if PrintDetailedProcess
                Console.WriteLine("Searching the relevant neighbor for the " + i + "th sample");
#endif
            }
            return relevant_nearest_neighbor_array;
        }

        public void TransferNodes()
        {
            RoseTreeNode[] new_nodearray = new RoseTreeNode[2 * clusternum];
            int new_nodearray_index = 0;
            
            for(int i = 0; i < this.nodearray.Length; i++)
                if (this.nodearray[i] != null && this.nodearray[i].valid == true)
                {
                    new_nodearray[new_nodearray_index] = this.nodearray[i];
                    new_nodearray[new_nodearray_index].indices.array_index = new_nodearray_index;
                    new_nodearray_index++;
                }
            this.nodearray = new_nodearray;
            this.nodecounter = clusternum;
            //this.lfv.Nullify();
        }

        public void InitializeRandomProjection()
        {
            this.projection = new RandomProjection(
                this.lfv.lexiconsize,                       //lexiconsize: 47989
                this.projectdimension,                      //projectdimensions[1]:50
                this.random_projection_algorithm_index,     //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                this.lfv.samplepath);                       //outputdebugpath
            projection.GenerateRandomMatrix();              //generate & write a projectiondim*datadim random matrix
            projection.ReadRandomMatrix();                  //read in this random matrix
        }

        public void CacheNearestNeighborsForAll()
        {
            int[][] nearestneighborlists = new int[this.nodearray.Length][];
            
            int length = this.nodearray.Length;
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < this.nodearray.Length; i++)
            //System.Threading.Tasks.Parallel.For(0, length, i =>
            {
                if (this.nodearray[i] != null && this.nodearray[i].valid == true)
                {
                    int[] nearestneighborlist = SearchNearestNeighbors(i);
                    List<int> newNearestneighborlist = new List<int>();

                    for (int j = 0; j < nearestneighborlist.Length; j++)
                    {
                        if (nearestneighborlist[j] >= i
                            || !nearestneighborlists[nearestneighborlist[j]].Contains(i))
                            newNearestneighborlist.Add(nearestneighborlist[j]);
                    }

                    nearestneighborlists[i] = newNearestneighborlist.ToArray();
                    CacheNearestNeighbors(this.nodearray[i], nearestneighborlists[i]);

                    if (i % 100 == 0)
                        Util.Write("Caching Nearest Neighbor for the " + i + "th node", i, length, DateTime.Now - startTime);
                }
            };
        }

        public virtual void MergeLoop(int interval)
        {
            StreamWriter sw = InitializeMergeRecordWriter();

            while (clusternum > 1)
            {
//#if PrintDetailedProcess
                if (clusternum % 100 == 0)
                    Console.WriteLine("In the " + clusternum + "th cluster");
//#endif
                RoseTreeNode node1, node2;
                int m;
                double log_likelihood_ratio, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2;
                double log_total_ratio;

                log_total_ratio = this.cachedict.getTopOne(out node1, out node2, out m, out log_likelihood_ratio, out logf, out cache_valuearray_plus_alpha, out log_likelihood_part1, out log_likelihood_part2);

                if (m == -1)
                {
                    this.cachedict = new CacheSortedDictionary();
                    CacheNearestNeighborsForAll();
                    log_total_ratio = this.cachedict.getTopOne(out node1, out node2, out m, out log_likelihood_ratio, out logf, out cache_valuearray_plus_alpha, out log_likelihood_part1, out log_likelihood_part2);
                }

                //if (clusternum == 100)
                //{
                    #region outputlikelihood(commented)
                    //StreamWriter swjoinlikelihood = new StreamWriter(outputpath + "\\joinLikelihoodMatrix" + clusternum + ".txt");
                    //StreamWriter swabsorblikelihood1 = new StreamWriter(outputpath + "\\absorblikelihood1Matrix" + clusternum + ".txt");
                    //StreamWriter swabsorblikelihood2 = new StreamWriter(outputpath + "\\absorblikelihood2Matrix" + clusternum + ".txt");
                    //StreamWriter swcollapselikelihood = new StreamWriter(outputpath + "\\collapselikelihoodMatrix" + clusternum + ".txt");
                    //StreamWriter swcosine = new StreamWriter(outputpath + "\\cosineMatrix" + clusternum + ".txt");

                    //SparseVectorList svl = new SparseVectorList();

                    //for (int i = 0; i < nodearray.Length; i++)
                    //    if (nodearray[i] != null && nodearray[i].valid == true)
                    //    {
                    //        for (int j = 0; j < nodearray.Length; j++)
                    //            if (nodearray[j] != null && nodearray[j].valid == true)
                    //            {
                    //                double cos = svl.Cosine(nodearray[i].data, nodearray[j].data);
                    //                swcosine.Write(cos + ",");

                    //                if (j == i)
                    //                {
                    //                    swjoinlikelihood.Write("0,");
                    //                    swabsorblikelihood1.Write("0,");
                    //                    swabsorblikelihood2.Write("0,");
                    //                    swcollapselikelihood.Write("0,");
                    //                    continue;
                    //                }
                    //                double alphaValueSum, part1, part2;
                    //                double f2 = GetLogF(nodearray[i], nodearray[j], out alphaValueSum);

                    //                double write1 = nodearray[i].JoinLogLikelihood(this.cacheclass, nodearray[i], nodearray[j], f2, out part1, out part2) - nodearray[i].log_likelihood - nodearray[j].log_likelihood;
                    //                double write2 = nodearray[i].AbsorbLogLikelihood(this.cacheclass, nodearray[i], nodearray[j], f2, out part1, out part2) - nodearray[i].log_likelihood - nodearray[j].log_likelihood;
                    //                //double write3 = nodearray[i].AbsorbLogLikelihood(this.cacheclass, nodearray[j], nodearray[i], f2, out part1, out part2) - nodearray[i].log_likelihood - nodearray[j].log_likelihood;
                    //                double write4 = nodearray[i].CollapseLogLikelihood(this.cacheclass, nodearray[i], nodearray[j], f2, out part1, out part2) - nodearray[i].log_likelihood - nodearray[j].log_likelihood;

                    //                swjoinlikelihood.Write(write1 + ",");
                    //                swabsorblikelihood1.Write(write2 + ",");
                    //                //swabsorblikelihood2.Write(write3 + ",");
                    //                swcollapselikelihood.Write(write4 + ",");
                    //            }
                    //        swjoinlikelihood.WriteLine();
                    //        swabsorblikelihood1.WriteLine();
                    //        swabsorblikelihood2.WriteLine();
                    //        swcollapselikelihood.WriteLine();
                    //        swcosine.WriteLine();
                    //    }
                    //swjoinlikelihood.Flush();
                    //swjoinlikelihood.Close();
                    //swabsorblikelihood1.Flush();
                    //swabsorblikelihood1.Close();
                    //swabsorblikelihood2.Flush();
                    //swabsorblikelihood2.Close();
                    //swcollapselikelihood.Flush();
                    //swcollapselikelihood.Close();
                    //swcosine.Flush();
                    //swcosine.Close();
                    #endregion
                //}

                OutputMergeRecord(sw, m, node1, node2, log_likelihood_ratio, log_total_ratio);
                MergeSingleStep(node1, node2, m, log_likelihood_ratio + (node1.log_likelihood + node2.log_likelihood), logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);

                int rebuild_times_num_upperbound = 0;
                switch (this.k)
                {
                    case 1: rebuild_times_num_upperbound = 40; break;
                    case 5: rebuild_times_num_upperbound = 20; break;
                    case 20: rebuild_times_num_upperbound = 10; break;
                    default: break;
                }
                if (this.clusternum > 1 && this.spilltree != null && 
                    this.spilltree.force_rebuild_spilltree >= 1 && 
                    rebuild_times_num <= rebuild_times_num_upperbound)
                {
                    TransferFeatureVectors();
                    CacheNearestNeighborsForAll();
                    rebuild_times_num++;
                }              
                if (this.clusternum % interval == 0)
                {
                    //Console.WriteLine(clusternum);
                    this.cachedict.ClearInvalidItems();
                }

                //if (clusternum <= 6000)
                //{
                //    //sw.Flush();
                //    //sw.Close();
                //    //Console.ReadKey();
                //}
            }

            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }

        public void TransferFeatureVectors()
        {
            //TransferNodes();
            if (this.algorithm_index == Constant.SPILLTREE_BRT)
                InitializeSpillTree();
            CacheNearestNeighborsForAll();
        }

        public virtual RoseTreeNode MergeSingleStep(RoseTreeNode node1, RoseTreeNode node2, int m, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            switch (m)
            {
                case 0: return JoinMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2); 
                case 1: return AbsorbMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2); 
                case 2: return AbsorbMerge(node2, node1, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                case 3: return CollapseMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2); 
                default: return null;
            }
        }

        public RoseTreeNode JoinMerge(RoseTreeNode node1, RoseTreeNode node2, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            RoseTreeNode[] newchildren = new RoseTreeNode[2];
            newchildren[0] = node1;
            newchildren[1] = node2;

            RoseTreeNode newnode = GenerateNewNode(newchildren, node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, 0, Math.Max(node1.tree_depth, node2.tree_depth) + 1);
            this.clusternum--;

            int[] nearestneighborlist = SearchNearestNeighbors(newnode.indices.array_index);
            CacheNearestNeighbors(newnode, nearestneighborlist);

            return newnode;
        }

        public RoseTreeNode AbsorbMerge(RoseTreeNode node1, RoseTreeNode node2, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            RoseTreeNode[] newchildren = new RoseTreeNode[node1.children.Length + 1];
            for (int i = 0; i < node1.children.Length; i++)
                newchildren[i] = node1.children[i];
            newchildren[node1.children.Length] = node2;

            RoseTreeNode newnode = GenerateNewNode(newchildren, node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, 1, Math.Max(node1.tree_depth, node2.tree_depth + 1));
            this.clusternum--;

            int[] nearestneighborlist = SearchNearestNeighbors(newnode.indices.array_index);
            CacheNearestNeighbors(newnode, nearestneighborlist);

            node1.children = null;
            return newnode;
        }

        public RoseTreeNode CollapseMerge(RoseTreeNode node1, RoseTreeNode node2, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            RoseTreeNode[] newchildren = new RoseTreeNode[node1.children.Length + node2.children.Length];
            for (int i = 0; i < node1.children.Length; i++)
                newchildren[i] = node1.children[i];
            for (int i = 0; i < node2.children.Length; i++)
                newchildren[node1.children.Length + i] = node2.children[i];

            RoseTreeNode newnode = GenerateNewNode(newchildren, node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, 3, Math.Max(node1.tree_depth, node2.tree_depth));
            this.clusternum--;

            int[] nearestneighborlist = SearchNearestNeighbors(newnode.indices.array_index);
            CacheNearestNeighbors(newnode, nearestneighborlist);
            
            node1.children = null;
            node2.children = null;
            return newnode;
        }

        public int[] SearchNearestNeighbors(int index)
        {
            int[] nearest_neighbor_list = null;
            switch (this.algorithm_index)
            {
                case Constant.BRT: nearest_neighbor_list = SearchNearestNeighborsBRT(index); break;
                case Constant.KNN_BRT: nearest_neighbor_list = SearchNearestNeighborskNNBRT(index); break;
                case Constant.SPILLTREE_BRT: nearest_neighbor_list = SearchNearestNeighborsSpilltreeBRT(index); break;
                default: return null;
            }
            return nearest_neighbor_list;
        }

        public int[] SearchNearestNeighborsBRT(int index)
        {
            List<int> nearestneighbor_list = new List<int>();

            for (int i = 0; i < index; i++)
                if (this.nodearray[i] != null && this.nodearray[i].valid == true && i != index)
                    nearestneighbor_list.Add(i);

            return nearestneighbor_list.ToArray();
        }

        public int[] SearchNearestNeighborskNNBRT(int index)
        {
            int[] indices = kNN.Search(index, this.nodearray, this.k);
            return indices;
        }

        public static int testtime = 0;
        public static double testprecision = 0;
        public static double testprecision2 = 0;
        public static int totalsearchtime = 0;
        public static int failtime = 0;
        public static double testprojectprecision = 0;
        public static int testprojecttime = 0;
        public int[] SearchNearestNeighborsSpilltreeBRT(int index)
        {
            bool force_brute_force_search;
            int search_neighbor_num;
            int[] nearest_neighbor_list = this.spilltree.Search(this.nodearray[index], out force_brute_force_search, out search_neighbor_num);
            
            if (force_brute_force_search == true)
                nearest_neighbor_list = kNN.SearchProject(index, this.nodearray, this.k, this.projectdimension);
            
            //test precision
            if (nearest_neighbor_list!=null)
            {
                int[] nearest_neighbor_list_project = kNN.SearchProject(index, this.nodearray, this.k, this.projectdimension);
                int correct_nearest_neighor_num = 0;
                for (int i = 0; i < nearest_neighbor_list.Length; i++)
                {
                    if (nearest_neighbor_list[i] == -1)
                        break;
                    for (int j = 0; j < nearest_neighbor_list_project.Length; j++)
                    {
                        if (nearest_neighbor_list[i] == nearest_neighbor_list_project[j])
                        {
                            correct_nearest_neighor_num++;
                            break;
                        }
                    }
                }

                int[] nearest_neighbor_list_org = kNN.Search(index, this.nodearray, this.k);
                int maxNNnumber = 0;
                for (int i = 0; i < nearest_neighbor_list_org.Length; i++)
                    if (nearest_neighbor_list_org[i] != -1)
                        maxNNnumber++;
                    else
                        break;

                //Console.WriteLine("SpillTree Precision: {0}/{1}, {2}%", correct_nearest_neighor_num, nearest_neighbor_list.Length, 100 * correct_nearest_neighor_num / nearest_neighbor_list.Length);
                if (maxNNnumber != 0)
                {
                    testprecision += (double)correct_nearest_neighor_num / nearest_neighbor_list.Length;
                    testprecision2 += (double)correct_nearest_neighor_num / maxNNnumber;
                    testtime++;
                }

                //test random projection precision
                correct_nearest_neighor_num = 0;
                for (int i = 0; i < nearest_neighbor_list_project.Length; i++)
                {
                    if (nearest_neighbor_list_project[i] == -1)
                        break;
                    for (int j = 0; j < nearest_neighbor_list_org.Length; j++)
                    {
                        if (nearest_neighbor_list_project[i] == nearest_neighbor_list_org[j])
                        {
                            correct_nearest_neighor_num++;
                            break;
                        }
                    }
                }
                //Console.WriteLine("SpillTree Precision: {0}/{1}, {2}%", correct_nearest_neighor_num, nearest_neighbor_list.Length, 100 * correct_nearest_neighor_num / nearest_neighbor_list.Length);
                if (maxNNnumber != 0)
                {
                    testprojectprecision += (double)correct_nearest_neighor_num / maxNNnumber;
                    testprojecttime++;
                }
            }

            //test fail times
            if (force_brute_force_search)
                failtime++;
            totalsearchtime++;

            return nearest_neighbor_list;
        }

        public virtual void CacheNearestNeighbors(RoseTreeNode newnode, int[] nearestneighborlist)
        {
            try
            {
                for (int i = 0; i < nearestneighborlist.Length; i++)
                {
                    if (nearestneighborlist[i] < 0) continue;
                    RoseTreeNode nearestneighbor = nodearray[nearestneighborlist[i]];

                    double cache_valuearray_plus_alpha;
                    double[] log_likelihood_part1 = new double[4];
                    double[] log_likelihood_part2 = new double[4];

                    double logf = GetLogF(newnode, nearestneighbor, out cache_valuearray_plus_alpha);
                    double join_log_likelihood_ratio = newnode.JoinLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[0], out log_likelihood_part2[0]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double absorb_log_likelihood_ratio1 = newnode.AbsorbLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[1], out log_likelihood_part2[1]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double absorb_log_likelihood_ratio2 = newnode.AbsorbLogLikelihood(this.cacheclass, nearestneighbor, newnode, logf, out log_likelihood_part1[2], out log_likelihood_part2[2]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double collapse_log_likelihood_ratio = newnode.CollapseLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[3], out log_likelihood_part2[3]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    CacheKey[] ck = new CacheKey[4];
                    CacheValue[] cv = new CacheValue[4];

                    ck[0] = new CacheKey(join_log_likelihood_ratio, (this.cachestamp++), Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth));
                    ck[1] = new CacheKey(absorb_log_likelihood_ratio1, (this.cachestamp++), Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth - 1));
                    ck[2] = new CacheKey(absorb_log_likelihood_ratio2, (this.cachestamp++), Math.Abs(nearestneighbor.tree_depth - newnode.tree_depth - 1));
                    ck[3] = new CacheKey(collapse_log_likelihood_ratio, (this.cachestamp++), Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth));

                    for (int r = 0; r < 4; r++)
                        cv[r] = new CacheValue(newnode, nearestneighbor, r, cache_valuearray_plus_alpha, logf, log_likelihood_part1[r], log_likelihood_part2[r]);
                    for (int r = 0; r < 4; r++)
                        this.cachedict.Insert(ck[r], cv[r]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public virtual RoseTreeNode GenerateNewNode(RoseTreeNode[] newchildren, RoseTreeNode node1, RoseTreeNode node2, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2, int m, int tree_depth)
        {
            RoseTreeNode newnode = MergeTwoNodes(newchildren, node1, node2, cache_valuearray_plus_alpha, m);
            CacheNodeValues(newnode, log_likelihood, logf, node1.cache_nodevalues.logf_part1 + node2.cache_nodevalues.logf_part1, node1.cache_nodevalues.subtree_leaf_count + node2.cache_nodevalues.subtree_leaf_count, tree_depth, log_likelihood_part1, log_likelihood_part2,
                node1, node2, m);
            InvalidateTwoNodes(node1, node2, m);
            return newnode;
        }

        public void CacheNodeValues(RoseTreeNode newnode, double log_likelihood, double logf, double logf_part1, int subtree_leaf_count, int tree_depth, double log_likelihood_part1, double log_likelihood_part2)
        {
            newnode.log_likelihood = log_likelihood;
            newnode.cache_nodevalues.logf = logf;
            newnode.cache_nodevalues.logf_part1 = logf_part1;
            newnode.cache_nodevalues.subtree_leaf_count = subtree_leaf_count;
            newnode.tree_depth = tree_depth;
            newnode.cache_nodevalues.log_likelihood_part1 = log_likelihood_part1;
            newnode.cache_nodevalues.log_likelihood_part2 = log_likelihood_part2;
            //xiting
            newnode.log_likelihood_posterior = log_likelihood;
        }

        public void CacheNodeValues(RoseTreeNode newnode, double log_likelihood, double logf, double logf_part1, int subtree_leaf_count, int tree_depth, double log_likelihood_part1, double log_likelihood_part2,
     RoseTreeNode node1, RoseTreeNode node2, int m)
        {
            //Previous
            newnode.log_likelihood = log_likelihood;
            newnode.cache_nodevalues.logf = logf;
            newnode.cache_nodevalues.logf_part1 = logf_part1;
            newnode.cache_nodevalues.subtree_leaf_count = subtree_leaf_count;
            newnode.tree_depth = tree_depth;
            newnode.cache_nodevalues.log_likelihood_part1 = log_likelihood_part1;
            newnode.cache_nodevalues.log_likelihood_part2 = log_likelihood_part2;

            //Cache more values for numeric accuracy
            //children_log_likelihood_sum
            switch (m)
            {
                case 0: newnode.cache_nodevalues.children_log_likelihood_sum = node1.log_likelihood + node2.log_likelihood; break;
                case 1: newnode.cache_nodevalues.children_log_likelihood_sum = node1.cache_nodevalues.children_log_likelihood_sum + node2.log_likelihood; break;
                case 2: newnode.cache_nodevalues.children_log_likelihood_sum = node1.log_likelihood + node2.cache_nodevalues.children_log_likelihood_sum; break;
                default: newnode.cache_nodevalues.children_log_likelihood_sum = node1.cache_nodevalues.children_log_likelihood_sum + node2.cache_nodevalues.children_log_likelihood_sum; break;
            }
            double children_log_likelihood_sum2 = 0;
            foreach (RoseTreeNode child in newnode.children)
                children_log_likelihood_sum2 += child.log_likelihood;
            //if (Math.Abs(newnode.cache_nodevalues.children_log_likelihood_sum - children_log_likelihood_sum2) > 1e-5)
            //    Console.Write("");
        }

        public RoseTreeNode MergeTwoNodes(RoseTreeNode[] newchildren, RoseTreeNode node1, RoseTreeNode node2, double cache_valuearray_plus_alpha, int m)
        {
            SparseVectorList newdata = MergeData(node1, node2, cache_valuearray_plus_alpha);
            RoseTreeNode newnode = new RoseTreeNode(newchildren, newdata, ((this.algorithm_index == Constant.SPILLTREE_BRT) ? projection.GenerateProjectData(newdata) : null), nodecounter);
            if (this.algorithm_index == Constant.SPILLTREE_BRT)
                this.spilltree.Insert(newnode);

            this.nodearray[nodecounter] = newnode;
            nodecounter++;
            return newnode;
        }

        public void InvalidateTwoNodes(RoseTreeNode node1, RoseTreeNode node2, int m)
        {
            int index1 = node1.indices.array_index;
            int index2 = node2.indices.array_index;

            InvalidateNode(index1);
            InvalidateNode(index2);

            if (m != 0)
                NullifyNode(index1);
            if (m == 2)
                NullifyNode(index2);
        }

        public SparseVectorList MergeData(RoseTreeNode node1, RoseTreeNode node2, double cache_valuearray_plus_alpha)
        {
            List<int> overlapping_keylist;
            SparseVectorList newdata = new SparseVectorList(node1.data.model_index);
            int new_vector_length;

            newdata = newdata.Add(false, this.model_index, node1.data, node2.data, out overlapping_keylist, out new_vector_length);
            if (newdata.keyarray.Length != new_vector_length)
                newdata.Resize(new_vector_length);

            newdata.valuearray_sum = node1.data.valuearray_sum + node2.data.valuearray_sum;
            newdata.cache_valuearray_plus_alpha = cache_valuearray_plus_alpha;

            //Xiting
            //if (this.model_index == Constant.DCM || this.algorithm_index == Constant.KNN_BRT)
            //    //newdata.GetNorm(this.idf, this.model_index);
            //    newdata.GetNormDCM();

            //if (this.algorithm_index == Constant.KNN_BRT)
            //    if (this.model_index == Constant.DCM)
            //        newdata.GetNormDCM();
            //    else if (this.model_index == Constant.VMF)
            //        newdata.GetNorm(this.idf, this.model_index);

            //newdata.GetNormDCM();

            //newdata.GetNorm(idf, model_index);

            if (this.model_index == Constant.VMF)
                newdata.GetNormvMF();
            else
                newdata.GetNormDCM();

            return newdata;
        }

        public void InvalidateNode(int index)
        {
            this.nodearray[index].Invalidate();
        }

        public void NullifyNode(int index)
        {
            this.nodearray[index] = null;
        }

        public void FindRoot()
        {
#if PrintDetailedProcess
            Console.WriteLine("Finding the root");
#endif
            int indexCount = 0;
            DateTime startTime = DateTime.Now;
            for (int i = 0; i < nodearray.Length; i++)
            {
                if (nodearray[i] != null && nodearray[i].valid == true)
                {
                    this.root = nodearray[i];
                    indexCount++;
                }
                if (i % 100 == 0)
                    Util.Write("FindRoot " + i, i, nodearray.Length, DateTime.Now - startTime);
            }
        }

        public double GetLogF(RoseTreeNode node1, RoseTreeNode node2, out double cache_valuearray_plus_alpha)
        {
            SparseVectorList newdata = new SparseVectorList(model_index);
            List<int> overlapping_keylist;
            int new_vector_length;
            if (node1 == null || node2 == null)
                Console.Write("");
            newdata = newdata.TryAdd(this.model_index, node1.data, node2.data, out overlapping_keylist, out new_vector_length);

            int new_valuearray_sum = node1.data.valuearray_sum + node2.data.valuearray_sum;
            double logf = LogF(node1.cache_nodevalues.logf_part1 + node2.cache_nodevalues.logf_part1, new_valuearray_sum, node1.data, node2.data, newdata, node1.cache_nodevalues.subtree_leaf_count + node2.cache_nodevalues.subtree_leaf_count, overlapping_keylist, out cache_valuearray_plus_alpha, new_vector_length);

            if (!(logf > double.MinValue))
                throw new Exception("invalid value!");

            return logf;
        }

        public double LogF(                         // initial
            double newlogf_part1,                   //logf_part1
            int new_valuearray_sum,                 //valuearray_sum
            SparseVectorList featurevector1,        //null
            SparseVectorList featurevector2,        //null
            SparseVectorList featurevector,         //featurevectors[i]
            int newsubtree_leaf_count,              //1
            List<int> overlapping_keylist,          //null
            out double cache_valuearray_plus_alpha, //out
            int new_vector_length)                  //featurevectors[i].count
        {
            if (this.model_index == Constant.DCM)
                return LogDCM(newlogf_part1, new_valuearray_sum, featurevector1, featurevector2, featurevector, overlapping_keylist, out cache_valuearray_plus_alpha, new_vector_length);
            else if (this.model_index == Constant.VMF)
            {
                cache_valuearray_plus_alpha = double.MinValue;
                return LogvMF(newsubtree_leaf_count, featurevector, new_vector_length);
            }
            else
            {
                cache_valuearray_plus_alpha = double.MinValue;
                return LogBernoulli(newsubtree_leaf_count, featurevector);
            }
        }

        public double LogDCM(double newlogf_part1, int new_valuearray_sum, SparseVectorList featurevector1, SparseVectorList featurevector2, SparseVectorList featurevector, List<int> overlapping_keylist, out double cache_valuearray_plus_alpha, int new_vector_length)
        {
            //if (overlapping_keylist == null || (overlapping_keylist != null && 3 * overlapping_keylist.Count > featurevector.count))
                return LogDCM1(newlogf_part1, new_valuearray_sum, featurevector, out cache_valuearray_plus_alpha, new_vector_length);
            //else
            //    return LogDCM2(newlogf_part1, new_valuearray_sum, featurevector1, featurevector2, featurevector, overlapping_keylist, out cache_valuearray_plus_alpha, new_vector_length);
        }

        public double LogDCM1(double newlogf_part1, int new_valuearray_sum, SparseVectorList featurevector, out double cache_valuearray_plus_alpha, int new_vector_length)
        {
            if (new_vector_length != 0)
            {
                double logf = 0;              
                int[] valuearray = featurevector.valuearray;

                logf += newlogf_part1;
                logf -= cacheclass.GetLogAlphaSumItem(new_valuearray_sum);

                cache_valuearray_plus_alpha = 0;
                for (int i = 0; i < new_vector_length; i++)
                    cache_valuearray_plus_alpha += cacheclass.GetLogAlphaItem(valuearray[i]);

                logf += cache_valuearray_plus_alpha;              
                return logf;
            }
            else
            {
                cache_valuearray_plus_alpha = 0;
                return 0;
            }
        }

        public double LogDCM2(double newlogf_part1, int new_valuearray_sum, SparseVectorList featurevector1, SparseVectorList featurevector2, SparseVectorList featurevector, List<int> overlapping_keylist, out double cache_valuearray_plus_alpha, int new_vector_length)
        {
            if (new_vector_length != 0)
            {
                double logf = 0;
                double overlapping = 0;
                double overlapping1 = 0;
                double overlapping2 = 0;

                int[] keyarray1 = featurevector1.keyarray;
                int[] valuearray1 = featurevector1.valuearray;
                int[] keyarray2 = featurevector2.keyarray;
                int[] valuearray2 = featurevector2.valuearray;
                int[] keyarray = featurevector.keyarray;
                int[] valuearray = featurevector.valuearray;
                int pt = 0;
                int pt1 = 0;
                int pt2 = 0;

                logf += newlogf_part1;
                logf -= cacheclass.GetLogAlphaSumItem(new_valuearray_sum);

                for (int i = 0; i < overlapping_keylist.Count; i++)
                {
                    int overlapping_key = overlapping_keylist[i];
                    while (keyarray1[pt1] != overlapping_key) pt1++;
                    while (keyarray2[pt2] != overlapping_key) pt2++;
                    while (keyarray[pt] != overlapping_key) pt++;

                    overlapping1 += cacheclass.GetLogAlphaItem(valuearray1[pt1]);
                    overlapping2 += cacheclass.GetLogAlphaItem(valuearray2[pt2]);
                    overlapping += cacheclass.GetLogAlphaItem(valuearray[pt]);
                }

                cache_valuearray_plus_alpha = (featurevector1.cache_valuearray_plus_alpha - overlapping1) + (featurevector2.cache_valuearray_plus_alpha - overlapping2) + overlapping;
                logf += cache_valuearray_plus_alpha;
                return logf;
            }
            else
            {
                cache_valuearray_plus_alpha = 0;
                return 0;
            }
        }

        public double LogvMF(int newsubtree_leaf_count, SparseVectorList featurevector, int new_vector_length)
        {
            if (new_vector_length != 0)
            {
                double d = lfv.lexiconsize;
                double norm = featurevector.AddNorm(featurevector, new_vector_length, lfv.lexiconsize, this.mu_0_each_dimension, mu_0_each_dimension_sqr);

                double logf = newsubtree_leaf_count * LogCoefficient(d, this.kappa);
                logf += LogCoefficient(d, this.kappa * this.R_0);
                logf -= LogCoefficient(d, this.kappa * norm);

                if (!(logf > double.MinValue))
                    throw new Exception("Invalid Value!");

                return logf;
            }
            else
                throw new Exception("Illegal size of x");
        }

        public double LogCoefficient(double d, double local_kappa)
        {
            double c = 0;
            c += Math.Log(local_kappa) * (d / 2 - 1);
            c -= Math.Log(2 * Math.PI) * d / 2;
            c -= LogModifiedBessel(d / 2 - 1, local_kappa);
            return c;
        }

        public double LogModifiedBessel(double nu, double z)
        {
            try
            {
                double local_alpha = 1 + (z / nu) * (z / nu);
                double local_eta = Math.Sqrt(local_alpha) + Math.Log(z / nu) - Math.Log(1 + Math.Sqrt(local_alpha));
                double logmodifiedbessel = -Math.Log(2 * Math.PI * nu) + nu * local_eta - 0.25 * Math.Log(local_alpha);
                return logmodifiedbessel;
            }
            catch (OverflowException e)
            {
                Console.WriteLine(e.Message);
                return double.MinValue;
            }
        }

        public double LogBernoulli(int newsubtree_leaf_count, SparseVectorList featurevector)
        {
            return double.MinValue;
        }

        public void   LabelTreeIndices(out int depth)
        {
#if PrintDetailedProcess
            Console.WriteLine("Labeling rose tree's indices");
#endif
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(this.root);
            int tree_index = 0;
            depth = 0;

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;
                depth++;
                for (int i = 0; i < nodelistcount; i++)
                {
                    RoseTreeNode node = nodelist[0];
                    node.indices.tree_index = tree_index;
                    tree_index++;
                    if (node.children != null)
                        for (int j = 0; j < node.children.Length; j++)
                            nodelist.Add(node.children[j]);
                    nodelist.Remove(node);
                }
            }
#if PrintDetailedProcess
            Console.WriteLine("Done with labeling rose tree's indices, the depth is " + depth);
#endif
        }

        public int[] SearchTreeNeighbors(RoseTreeNode query, int neighbors_num, int least_search_neighbors_num, out int actual_search_neighbors_num)
        {
            RoseTreeNode query_parent = query.parent;
            List<RoseTreeNode> subtree_leaf = GetSubTreeLeaf(query_parent);

            while (subtree_leaf.Count < least_search_neighbors_num)
            {
                if (query_parent.parent == null) break;
                query_parent = query_parent.parent;
                subtree_leaf = GetSubTreeLeaf(query_parent);
            }

            kNearestNeighbor kNN = new kNearestNeighbor();
            int[] indices = kNN.SearchTree(query, subtree_leaf.ToArray(), neighbors_num, this.nodearray);

            actual_search_neighbors_num = subtree_leaf.Count - 1;
            return indices;
        }

        public static List<RoseTreeNode> GetSubTreeLeaf(RoseTreeNode node)
        {
            List<RoseTreeNode> subtreeleaflist = new List<RoseTreeNode>();
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(node);

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;

                for (int i = 0; i < nodelistcount; i++)
                {
                    RoseTreeNode subnode = nodelist[0];

                    if (subnode.children != null)
                        for (int j = 0; j < subnode.children.Length; j++)
                            nodelist.Add(subnode.children[j]);
                    else
                        subtreeleaflist.Add(subnode);

                    nodelist.RemoveAt(0);
                }
            }

            return subtreeleaflist;
        }

        public static string MergeRecordFileName = null;
        protected static int FileIndex = 0;
        int MergeTreeIndex;
        protected StreamWriter InitializeMergeRecordWriter()
        {
            StreamWriter sw = null;
            if (MergeRecordFileName != null)
            {
                try
                {
                    sw = new StreamWriter(outputpath + "MergeRecord_" + MergeRecordFileName + ".dat");
                }
                catch
                {
                    sw = new StreamWriter(outputpath + "MergeRecord_" + String.Format("{0:MMdd_HHmmss}", DateTime.Now)
                        + "_" + FileIndex + ".dat");
                }
                FileIndex++;
            }

            MergeTreeIndex = clusternum;
            JoinCnt = AbsorbLCnt = AbsorbRCnt = CollapseCnt = 0;
            return sw;
        }

        public int JoinCnt, AbsorbLCnt, AbsorbRCnt, CollapseCnt;
        protected void OutputMergeRecord(StreamWriter sw, int m, RoseTreeNode node0, RoseTreeNode node1,
            double loglikelihood, double cachekeylikelihood)
        {
            if (sw != null)
            {
                sw.Write("{0}\t", MergeTreeIndex);
                switch (m)
                {
                    case 0: sw.WriteLine("Join\t"); JoinCnt++; break;
                    case 1: sw.WriteLine("AbsorbL\t"); AbsorbLCnt++; break;
                    case 2: sw.WriteLine("AbsorbR\t"); AbsorbRCnt++; break;
                    case 3: sw.WriteLine("Collapse\t"); CollapseCnt++; break;
                }

                sw.Write("\t{0}({1},d{2})\t", node0.MergeTreeIndex, node0.LeafCount, node0.tree_depth);
                sw.Write("\t{0}({1},d{2})\t", node1.MergeTreeIndex, node1.LeafCount, node1.tree_depth);

                sw.WriteLine();
                sw.WriteLine("\t{0}\t{1}\t{2}", loglikelihood, (cachekeylikelihood - loglikelihood), cachekeylikelihood);
                MergeTreeIndex++;
                sw.Flush();
            }
            else
            {
                switch (m)
                {
                    case 0: JoinCnt++; break;
                    case 1: AbsorbLCnt++; break;
                    case 2: AbsorbRCnt++; break;
                    case 3: CollapseCnt++; break;
                }
            }
        }

        public IList<RoseTreeNode> GetAllTreeLeaf()
        {
            int leafnum = lfv.featurevectors.Length;
            if (leafnum == nodearray.Length / 2)
            {
                List<RoseTreeNode> leaves = new List<RoseTreeNode>();
                for (int ileaf = 0; ileaf < leafnum; ileaf++)
                {
                    RoseTreeNode leaf = nodearray[ileaf];
                    if (leaf != null && leaf.parent != null)
                        leaves.Add(leaf);
                }
                return leaves.AsReadOnly();
                //return nodearray.ToList().GetRange(0, leafnum).AsReadOnly();
            }
            else
            {
                //After disable TransferNodes() this will not be entered
                List<RoseTreeNode> leaves = GetSubTreeLeaf(this.root);
                return leaves.AsReadOnly();
            }
        }

        public IList<RoseTreeNode> GetAllValidTreeNodes()
        {
            List<RoseTreeNode> validnodes = new List<RoseTreeNode>();
            for (int i = 0; i < nodearray.Length; i++)
            {
                if (nodearray[i] != null && (nodearray[i].parent != null || nodearray[i] == root))
                {
                    validnodes.Add(nodearray[i]);
                }
                //if (nodearray[i].indices.array_index != i)
                //    throw new Exception("Error GetAllValidTreeNodes!");
            }
            return validnodes.AsReadOnly();
        }

        public IList<RoseTreeNode> GetAllValidInternalTreeNodes()
        {
            List<RoseTreeNode> validnodes = new List<RoseTreeNode>();
            for (int i = nodearray.Length / 2; i < nodearray.Length; i++)
            {
                if (nodearray[i] != null && (nodearray[i].parent != null || nodearray[i] == root))
                    validnodes.Add(nodearray[i]);
            }
            return validnodes.AsReadOnly();
        }

        public RoseTreeStructureInfo[] StructureInfo()
        {
            RoseTreeStructureInfo levelinfo1 = new RoseTreeStructureInfo();
            RoseTreeStructureInfo levelinfo2 = new RoseTreeStructureInfo();
            RoseTreeStructureInfo[] info = new RoseTreeStructureInfo[] { levelinfo1, levelinfo2 };

            RoseTreeNode[] nodes_level1 = this.root.children;
            List<RoseTreeNode> nodes_level2 = new List<RoseTreeNode>();
            for (int i = 0; i < nodes_level1.Length; i++)
                if (nodes_level1[i].children != null)
                    nodes_level2.AddRange(nodes_level1[i].children);
                else
                    nodes_level2.Add(nodes_level1[i]);

            levelinfo1.ChildrenCount = nodes_level1.Length;
            levelinfo2.ChildrenCount = nodes_level2.Count;

            int sum = 0;
            int ssum = 0;
            int notonecnt = 0;
            for (int i = 0; i < nodes_level1.Length; i++)
            {
                if (nodes_level1[i].children != null)
                {
                    int childrencnt = RoseTree.GetSubTreeLeaf(nodes_level1[i]).Count;
                    sum += childrencnt;
                    ssum += childrencnt * childrencnt;
                    notonecnt++;
                }
            }
            levelinfo1.ShrinkChildrenCount = notonecnt;
            levelinfo1.AverageChildrenLeaves = (double)sum / notonecnt ;
            levelinfo1.StdChildrenLeaves = Math.Sqrt((double)ssum / notonecnt - levelinfo1.AverageChildrenLeaves * levelinfo1.AverageChildrenLeaves);


            sum = ssum = notonecnt = 0;
            foreach(RoseTreeNode node in nodes_level2)
            {
                if (node.children != null)
                {
                    int childrencnt = RoseTree.GetSubTreeLeaf(node).Count;
                    sum += childrencnt;
                    ssum += childrencnt * childrencnt;
                    notonecnt++;
                }
            }
            levelinfo2.ShrinkChildrenCount = notonecnt;
            levelinfo2.AverageChildrenLeaves = (double)sum / notonecnt;
            levelinfo2.StdChildrenLeaves = Math.Sqrt((double)ssum / notonecnt - levelinfo2.AverageChildrenLeaves * levelinfo2.AverageChildrenLeaves);

            return info;
        }

        public string StructureToString()
        {
            string str = "";
            str += "TreeDepth:" + this.root.tree_depth + "\n";
            RoseTreeNode[] nodes_level1 = this.root.children;
            List<RoseTreeNode> nodes_level2 = new List<RoseTreeNode>();

            int shrinkcnt1 = 0;
            for (int i = 0; i < nodes_level1.Length; i++)
                if (nodes_level1[i].children != null)
                {
                    nodes_level2.AddRange(nodes_level1[i].children);
                    shrinkcnt1++;
                }
                else
                    nodes_level2.Add(nodes_level1[i]);
            int shrinkcnt2 = 0;
            foreach (RoseTreeNode node in nodes_level2)
                if (node.children != null)
                    shrinkcnt2++;

            str += "Level 1:\t" + nodes_level1.Length + "\t" + shrinkcnt1 + "\n";
            str += "Level 2:\t" + nodes_level2.Count + "\t" + shrinkcnt2 + "\n";

            str +=  "Level 1 leaves:\t";
            for (int i = 0; i < nodes_level1.Length; i++)
            {
                RoseTreeNode node = nodes_level1[i];
                if(node.children!=null)
                    str += RoseTree.GetSubTreeLeaf(node).Count + "\t";
            }
            str += "\n";

            str += "Level 2 leaves:\t";
            for (int i = 0; i < nodes_level2.Count; i++)
            {
                RoseTreeNode node = nodes_level2[i];
                if (node.children != null)
                    str += RoseTree.GetSubTreeLeaf(node).Count + "\t";
            }
            str += "\n";

            return str;
        }

        public RoseTreeNode GetNodeByArrayIndex(int array_index)
        {
            //if (array_index >= nodearray.Length || array_index < 0)
            //    Console.WriteLine("Error! nodearray.Length = {0}, array_index = {1}", 
            //        nodearray.Length, array_index);
            return nodearray[array_index];
        }

        public void SetNodeArray(RoseTreeNode node)
        {
            nodearray[node.indices.array_index] = node;
        }

        //public double GetAverageInternalNodeDegree()
        //{
        //    int internalnodecnt = 0;
        //    int degree = 0;
        //    foreach (RoseTreeNode node in nodearray)
        //    {
        //        if (node != null && node.valid && node.children != null)
        //        {
        //            internalnodecnt++;
        //            degree += node.children.Length;
        //        }
        //    }

        //    Console.WriteLine("D:{0}, I:{1}, Avg:{2}", degree, internalnodecnt, (double)degree / internalnodecnt);
        //    return (double)degree / internalnodecnt;
        //}

        public CacheClass GetCacheClass()
        {
            return cacheclass;
        }
    }

    public class RoseTreeStructureInfo
    {
        public int ChildrenCount;
        public int ShrinkChildrenCount;
        public double AverageChildrenLeaves;
        public double StdChildrenLeaves;
    }
}
