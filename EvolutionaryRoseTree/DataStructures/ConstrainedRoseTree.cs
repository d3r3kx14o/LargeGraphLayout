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
using System.Diagnostics;
using RoseTreeTaxonomy.DrawTree;
namespace EvolutionaryRoseTree.DataStructures
{
    //Add a Run() function with constraint
    class ConstrainedRoseTree : RuleRoseTree
    {
        public static int RoseTreeCount = 0;
        public int RoseTreeId = -1;
        protected Constraint constraint;
        public int mergedtreepointer;
        public double LogLikelihood { get { return this.root.log_likelihood; } }
        public List<Constraint> SmoothCostConstraint = new List<Constraint>();

        public double sizePunishMinRatio = 0;
        public double sizePunishMaxRatio = 0;
        protected int SizePunishMin = int.MaxValue;
        protected int SizePunishMax = int.MaxValue;
        public static int SizePunishTooSmallMax = 5;
        public static double SizePunishTooSmallWeight = 0.1;
        protected static double SizePunishMaxMinDifference = 6;
        protected double SizePunishSlope = 0;
        public static double sizepunishWeight = 5 * Math.Exp(-SizePunishMaxMinDifference);

        #region Adjust Tree Structure
        public static int AdjustStructureCollapseThreshold = 10;
        public static int AdjustStructureOthersThreshold = 10;
        public static double AdjustStructureOpenClusterFactor = 0.05;
        public static int AdjustStructureOpenClusterThreshold = 50;
        public static double AdjustStructureOpenNodeClusterAlphaRatio = 0.3;
        public static double[] AdjustStructureTestGammas = new double[] { 0.1, 0.2, 0.3, 0.4 };
        #endregion Adjust Tree Structure

        public ConstrainedRoseTree(
            int dataset_index,   //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET                             
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
            base(dataset_index, algorithm_index, experiment_index, random_projection_algorithm_index, model_index, projectdimension, k, lfv, alpha, gamma, tau, kappa, R_0, outputpath)
        {
            RoseTreeId = RoseTreeCount++;

            int leafnodesnum = this.lfv.featurevectors.Length;
            if (k > leafnodesnum)
                k = leafnodesnum;
            if (projectdimension > leafnodesnum)
                projectdimension = leafnodesnum;

            //SetSizePunishMinMax(sizePunishMinRatio, sizePunishMaxRatio);
        }

        public virtual void Run(
            Constraint constraint,
            int interval,               //Constant.intervals[0]:30
            out int depth,
            out double log_likelihood)
        {
            //Do not clear invalid nodes (by scan) because invalid node will be removed immediately
            interval = int.MaxValue;
            if (constraint is TreeOrderConstraint)
                (constraint as TreeOrderConstraint).SetConstrainedRoseTree(this);
            if (constraint is MultipleConstraints)
                if ((constraint as MultipleConstraints).GetLastConstraint() is TreeOrderConstraint)
                    (constraint as MultipleConstraints).SetConstrainedRoseTree(this);

            if (constraint != null)
                this.constraint = constraint;
            else
                this.constraint = new NoConstraint();

            Console.WriteLine("caching");
            CacheCacheClass();              //Cache log values as dictionaries
            Console.WriteLine("cached");
            Initialize();                   //Initialize (random projection,) nodes, (spill tree)
            //if (this.constraint.ConstraintType == ConstraintType.TreeOrder)
            {
                for (int i = 0; i < nodearray.Length/2; i++)
                    nodearray[i].MergeTreeIndex = nodearray[i].indices.initial_index;
                mergedtreepointer = nodearray.Length / 2;
            }

            Console.WriteLine("caching neighbours");
            CacheNearestNeighborsForAll();
            Console.WriteLine("cached");
            MergeLoop(interval);
            FindRoot();
            UpdateDepthInTree();
            if (CacheValueRecord != null) CacheValueRecord.Close();

            ConstraintTree ctree = null;
            if (constraint is TreeOrderConstraint)
            {
                ctree = (constraint as TreeOrderConstraint).GetConstraintTree();
                AdjustChildrenOrder(ctree);
                ctree.UpdateLeafNumbers();
                ctree.PostProcessContainedInformation();
            }
            if (constraint is MultipleConstraints)
            {
                foreach (var constraint0 in (constraint as MultipleConstraints).GetConstraints())
                {
                    if (constraint0 is TreeOrderConstraint)
                    {
                        ctree = (constraint0 as TreeOrderConstraint).GetConstraintTree();
                        AdjustChildrenOrder(ctree);
                        ctree.UpdateLeafNumbers();
                        ctree.PostProcessContainedInformation();
                    }
                }
            }

            Console.WriteLine("labeling");
            LabelTreeIndices(out depth);
            Console.WriteLine("labeled");

            this.spilltree = null;
            //this.cacheclass = null;

            log_likelihood = this.root.log_likelihood;
        }

        static DateTime time_run = DateTime.Now;
        static void PrintRunTime(string description)
        {
            Console.WriteLine("[RunTime:{0}] {1}s", description, (DateTime.Now.Ticks - time_run.Ticks) / 1e7);
            time_run = DateTime.Now;
        }

        //When cache nearest neighbors, consider both p(D|T) and p(T)
        protected StreamWriter CacheValueRecord = null;
        protected double cacheNNindex = 0;

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
                //Dictionary<CacheKey, CacheValue> newnodecachedict = newnode.CacheMergePairs;

                for (int i = 0; i < nearestneighborlist.Length; i++)
                {
                    if (nearestneighborlist[i] < 0) continue;
                    RoseTreeNode nearestneighbor = nodearray[nearestneighborlist[i]];

                    //if (nearestneighbor.MergeTreeIndex == 2979)
                    //    Console.Write("");

                    double cache_valuearray_plus_alpha;
                    double[] log_likelihood_part1 = new double[4];
                    double[] log_likelihood_part2 = new double[4];

                    double logf = GetLogF(newnode, nearestneighbor, out cache_valuearray_plus_alpha);
#if NEW_CONSTRAINT_MODEL                   
                    double[] log_treeprobability = new double[4];
                    double join_log_treeprobability_ratio = constraint.GetLogJoinTreeProbabilityRatio(newnode, nearestneighbor);
                    double absorb_log_treeprobability_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(newnode, nearestneighbor);
                    double absorb_log_treeprobability2_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(nearestneighbor, newnode);
                    double collapse_log_treeprobability_ratio = constraint.GetLogCollapseTreeProbabilityRatio(newnode, nearestneighbor);
                    log_treeprobability[0] = join_log_treeprobability_ratio + newnode.LogTreeProbability + nearestneighbor.LogTreeProbability;
                    log_treeprobability[1] = absorb_log_treeprobability_ratio + newnode.LogTreeProbability + nearestneighbor.LogTreeProbability;
                    log_treeprobability[2] = absorb_log_treeprobability2_ratio + newnode.LogTreeProbability + nearestneighbor.LogTreeProbability;
                    log_treeprobability[3] = collapse_log_treeprobability_ratio + newnode.LogTreeProbability + nearestneighbor.LogTreeProbability;

                    double join_log_likelihood_ratio = PosterierJoinLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, join_log_treeprobability_ratio, out log_likelihood_part1[0], out log_likelihood_part2[0]) - (newnode.log_likelihood_posterior + nearestneighbor.log_likelihood_posterior);
                    double absorb_log_likelihood_ratio1 = PosterierAbsorbLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, absorb_log_treeprobability_ratio, out log_likelihood_part1[1], out log_likelihood_part2[1]) - (newnode.log_likelihood_posterior + nearestneighbor.log_likelihood_posterior);
                    double absorb_log_likelihood_ratio2 = PosterierAbsorbLogLikelihood(this.cacheclass, nearestneighbor, newnode, logf, absorb_log_treeprobability2_ratio, out log_likelihood_part1[2], out log_likelihood_part2[2]) - (newnode.log_likelihood_posterior + nearestneighbor.log_likelihood_posterior);
                    double collapse_log_likelihood_ratio = PosterierCollapseLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, collapse_log_treeprobability_ratio, out log_likelihood_part1[3], out log_likelihood_part2[3]) - (newnode.log_likelihood_posterior + nearestneighbor.log_likelihood_posterior);

                    //double join_log_likelihood_ratio = PosterierJoinLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, join_log_treeprobability_ratio, out log_likelihood_part1[0], out log_likelihood_part2[0]);
                    //double absorb_log_likelihood_ratio1 = PosterierAbsorbLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, absorb_log_treeprobability_ratio, out log_likelihood_part1[1], out log_likelihood_part2[1]);
                    //double absorb_log_likelihood_ratio2 = PosterierAbsorbLogLikelihood(this.cacheclass, nearestneighbor, newnode, logf, absorb_log_treeprobability2_ratio, out log_likelihood_part1[2], out log_likelihood_part2[2]);
                    //double collapse_log_likelihood_ratio = PosterierCollapseLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, collapse_log_treeprobability_ratio, out log_likelihood_part1[3], out log_likelihood_part2[3]);
                    //join_log_likelihood_ratio = join_log_likelihood_ratio == double.MinValue ? double.MinValue : (log_likelihood_part1[0] - join_log_likelihood_ratio);
                    //absorb_log_likelihood_ratio1 = absorb_log_likelihood_ratio1 == double.MinValue ? double.MinValue : (log_likelihood_part1[1] - absorb_log_likelihood_ratio1);
                    //absorb_log_likelihood_ratio2 = absorb_log_likelihood_ratio2 == double.MinValue ? double.MinValue : (log_likelihood_part1[2] - absorb_log_likelihood_ratio2);
                    //collapse_log_likelihood_ratio = collapse_log_likelihood_ratio == double.MinValue ? double.MinValue : (log_likelihood_part1[3] - collapse_log_likelihood_ratio);
              
#else
                    double[] log_treeprobability_ratio = new double[4];
                    double join_log_likelihood_ratio = newnode.JoinLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[0], out log_likelihood_part2[0]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double absorb_log_likelihood_ratio1 = newnode.AbsorbLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[1], out log_likelihood_part2[1]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double absorb_log_likelihood_ratio2 = newnode.AbsorbLogLikelihood(this.cacheclass, nearestneighbor, newnode, logf, out log_likelihood_part1[2], out log_likelihood_part2[2]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double collapse_log_likelihood_ratio = newnode.CollapseLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[3], out log_likelihood_part2[3]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);

                    ConstraintTree.bayesFactor = log_likelihood_part1[0] - log_likelihood_part2[0];
                    double join_log_treeprobability_ratio = constraint.GetLogJoinTreeProbabilityRatio(newnode, nearestneighbor);
                    ConstraintTree.bayesFactor = log_likelihood_part1[1] - log_likelihood_part2[1];
                    double absorb_log_treeprobability_ratio1 = constraint.GetLogAbsorbTreeProbabilityRatio(newnode, nearestneighbor);
                    ConstraintTree.bayesFactor = log_likelihood_part1[2] - log_likelihood_part2[2];
                    double absorb_log_treeprobability_ratio2 = constraint.GetLogAbsorbTreeProbabilityRatio(nearestneighbor, newnode);
                    ConstraintTree.bayesFactor = log_likelihood_part1[3] - log_likelihood_part2[3];
                    double collapse_log_treeprobability_ratio = constraint.GetLogCollapseTreeProbabilityRatio(newnode, nearestneighbor);
                    //join_log_treeprobability_ratio = absorb_log_treeprobability_ratio1 = absorb_log_treeprobability_ratio2 = collapse_log_treeprobability_ratio;
                    log_treeprobability_ratio[0] = join_log_treeprobability_ratio;
                    log_treeprobability_ratio[1] = absorb_log_treeprobability_ratio1;
                    log_treeprobability_ratio[2] = absorb_log_treeprobability_ratio2;
                    log_treeprobability_ratio[3] = collapse_log_treeprobability_ratio;
#endif

                    CacheKey[] ck = new CacheKey[4];
                    CacheValue[] cv = new CacheValue[4];

#if NEW_CONSTRAINT_MODEL    
                    for (int r = 0; r < 4; r++)
                        cv[r] = new ConstrainedCacheValue(newnode, nearestneighbor, r, cache_valuearray_plus_alpha, logf, log_likelihood_part1[r], log_likelihood_part2[r], log_treeprobability[r]);
                    int[] depth_difference;
                    double similarity;
                    double[] secondarykey = GetSecondaryKey(cv, out similarity, out depth_difference);
                    ck[0] = new ConstrainedCacheKey(join_log_likelihood_ratio, join_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex, depth_difference[0], similarity, secondarykey[0]);
                    ck[1] = new ConstrainedCacheKey(absorb_log_likelihood_ratio1, absorb_log_likelihood_ratio1, basetag + 4 * nearestneighbor.MergeTreeIndex + 1, depth_difference[1], similarity, secondarykey[1]);
                    ck[2] = new ConstrainedCacheKey(absorb_log_likelihood_ratio2, absorb_log_likelihood_ratio2, basetag + 4 * nearestneighbor.MergeTreeIndex + 2, depth_difference[2], similarity, secondarykey[2]);
                    ck[3] = new ConstrainedCacheKey(collapse_log_likelihood_ratio, collapse_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex + 3, depth_difference[3], similarity, secondarykey[3]);
#else
                    for (int r = 0; r < 4; r++)
                        cv[r] = new ConstrainedCacheValue(newnode, nearestneighbor, r, cache_valuearray_plus_alpha, logf, log_likelihood_part1[r], log_likelihood_part2[r], log_treeprobability_ratio[r]);
                    int[] depth_difference;
                    double similarity;
                    GetSimilarityAndDepthDifference(cv, out similarity, out depth_difference);
                    ck[0] = new ConstrainedCacheKey(join_log_likelihood_ratio + join_log_treeprobability_ratio, join_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex, depth_difference[0], similarity);
                    ck[1] = new ConstrainedCacheKey(absorb_log_likelihood_ratio1 + absorb_log_treeprobability_ratio1, absorb_log_likelihood_ratio1, basetag + 4 * nearestneighbor.MergeTreeIndex + 1, depth_difference[1], similarity);
                    ck[2] = new ConstrainedCacheKey(absorb_log_likelihood_ratio2 + absorb_log_treeprobability_ratio2, absorb_log_likelihood_ratio2, basetag + 4 * nearestneighbor.MergeTreeIndex + 2, depth_difference[2], similarity);
                    ck[3] = new ConstrainedCacheKey(collapse_log_likelihood_ratio + collapse_log_treeprobability_ratio, collapse_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex + 3, depth_difference[3], similarity);
 #endif
                    for (int r = 0; r < 4; r++)
                    {
                        this.cachedict.Insert(ck[r], cv[r]);
                        //newnodecachedict.Add(ck[r], cv[r]);
                        if(this.constraint.ConstraintType != ConstraintType.NoConstraint)
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
        public override void CacheNearestNeighbors(RoseTreeNode newnode, int[] nearestneighborlist)
        {
            //try
            {
                int leafnum = this.lfv.featurevectors.Length;
                int basetag = 8 * cacheNNindex++ * leafnum;
                //if (newnode.MergeTreeIndex == 122)
                //    Console.Write("");
                Dictionary<CacheKey, CacheValue> newnodecachedict = newnode.CacheMergePairs;

                for (int i = 0; i < nearestneighborlist.Length; i++)
                {
                    if (nearestneighborlist[i] < 0) continue;
                    RoseTreeNode nearestneighbor = nodearray[nearestneighborlist[i]];

                    CacheKey[] ck = new CacheKey[4];
                    CacheValue[] cv = new CacheValue[4];
                    double cache_valuearray_plus_alpha;
                    double[] log_likelihood_part1 = new double[4];
                    double[] log_likelihood_part2 = new double[4];

                    double logf = GetLogF(newnode, nearestneighbor, out cache_valuearray_plus_alpha);
                    double similarity = newnode.data.Cosine(newnode.data, nearestneighbor.data);
#if NEW_CONSTRAINT_MODEL
                    ck[0] = new ConstrainedCacheKey(double.NaN, double.NaN, basetag + 4 * nearestneighbor.MergeTreeIndex, Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth), similarity);
                    ck[1] = new ConstrainedCacheKey(double.NaN, double.NaN, basetag + 4 * nearestneighbor.MergeTreeIndex + 1, Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth - 1), similarity);
                    ck[2] = new ConstrainedCacheKey(double.NaN, double.NaN, basetag + 4 * nearestneighbor.MergeTreeIndex + 2, Math.Abs(nearestneighbor.tree_depth - newnode.tree_depth - 1), similarity);
                    ck[3] = new ConstrainedCacheKey(double.NaN, double.NaN, basetag + 4 * nearestneighbor.MergeTreeIndex + 3, Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth), similarity);
#else
                    double join_log_likelihood_ratio = newnode.JoinLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[0], out log_likelihood_part2[0]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double absorb_log_likelihood_ratio1 = newnode.AbsorbLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[1], out log_likelihood_part2[1]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double absorb_log_likelihood_ratio2 = newnode.AbsorbLogLikelihood(this.cacheclass, nearestneighbor, newnode, logf, out log_likelihood_part1[2], out log_likelihood_part2[2]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    double collapse_log_likelihood_ratio = newnode.CollapseLogLikelihood(this.cacheclass, newnode, nearestneighbor, logf, out log_likelihood_part1[3], out log_likelihood_part2[3]) - (newnode.log_likelihood + nearestneighbor.log_likelihood);
                    ck[0] = new ConstrainedCacheKey(double.NaN, join_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex, Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth), similarity);
                    ck[1] = new ConstrainedCacheKey(double.NaN, absorb_log_likelihood_ratio1, basetag + 4 * nearestneighbor.MergeTreeIndex + 1, Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth - 1), similarity);
                    ck[2] = new ConstrainedCacheKey(double.NaN, absorb_log_likelihood_ratio2, basetag + 4 * nearestneighbor.MergeTreeIndex + 2, Math.Abs(nearestneighbor.tree_depth - newnode.tree_depth - 1), similarity);
                    ck[3] = new ConstrainedCacheKey(double.NaN, collapse_log_likelihood_ratio, basetag + 4 * nearestneighbor.MergeTreeIndex + 3, Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth), similarity);

#endif
                    for (int r = 0; r < 4; r++)
                    {
                        cv[r] = new ConstrainedCacheValue(newnode, nearestneighbor, r, cache_valuearray_plus_alpha, logf, log_likelihood_part1[r], log_likelihood_part2[r], double.NaN);
                        newnode.CacheMergePairs.Add(ck[r], cv[r]);
                    }
                }

#if !APPROXIMATE_LIKELIHOOD
                    error here
#endif
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }
#endif

        #region calculate posterier likelihood
        public double PosterierJoinLogLikelihood(CacheClass cacheclass, RoseTreeNode node1, RoseTreeNode node2, double logf, double log_treeprobability_ratio, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            double log_likelihood = 0;
#if NEW_MODEL_2 || NEW_MODEL_3
            log_likelihood_part1 = cacheclass.GetLogPi(2) + logf + log_treeprobability_ratio;
#else
            log_likelihood_part1 = cacheclass.GetLogPi(2) + logf + log_treeprobability_ratio + node1.LogTreeProbability + node2.LogTreeProbability;
#endif
            log_likelihood_part2 = cacheclass.GetLogOneMinusPi(2) + (node1.log_likelihood_posterior + node2.log_likelihood_posterior);

            if (log_likelihood_part1 > log_likelihood_part2)
                log_likelihood = log_likelihood_part1 + Math.Log(1 + Math.Exp(log_likelihood_part2 - log_likelihood_part1));
            else
                log_likelihood = log_likelihood_part2 + Math.Log(1 + Math.Exp(log_likelihood_part1 - log_likelihood_part2));

            return log_likelihood;
        }

        public double PosterierAbsorbLogLikelihood(CacheClass cacheclass, RoseTreeNode node1, RoseTreeNode node2, double logf, double log_treeprobability_ratio, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            log_likelihood_part1 = 0;
            log_likelihood_part2 = 0;

            if (!(node1.children != null && node1.children.Length != 0))
                return double.MinValue;

            double log_likelihood = 0;
#if NEW_MODEL_2
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + 1) + logf + log_treeprobability_ratio;
#elif NEW_MODEL_3
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + 1) + logf + log_treeprobability_ratio 
                + node1.cache_nodevalues.children_log_incremental_cost_sum;
#else
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + 1) + logf + log_treeprobability_ratio + node1.LogTreeProbability + node2.LogTreeProbability;
#endif
            log_likelihood_part2 = cacheclass.GetLogOneMinusPi(node1.children.Length + 1) +
                node1.cache_nodevalues.children_log_likelihood_posterior_sum + node2.log_likelihood_posterior;

            if (log_likelihood_part1 > log_likelihood_part2)
                log_likelihood = log_likelihood_part1 + Math.Log(1 + Math.Exp(log_likelihood_part2 - log_likelihood_part1));
            else
                log_likelihood = log_likelihood_part2 + Math.Log(1 + Math.Exp(log_likelihood_part1 - log_likelihood_part2));

            return log_likelihood;
        }

        public double PosterierCollapseLogLikelihood(CacheClass cacheclass, RoseTreeNode node1, RoseTreeNode node2, double logf, double log_treeprobability_ratio, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            log_likelihood_part1 = 0;
            log_likelihood_part2 = 0;

            if (!(node1.children != null && node1.children.Length != 0) || !(node2.children != null && node2.children.Length != 0))
                return double.MinValue;

            double log_likelihood = 0;
#if NEW_MODEL_2
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + node2.children.Length) + logf + log_treeprobability_ratio;
#elif NEW_MODEL_3
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + node2.children.Length) + logf + log_treeprobability_ratio
                + node1.cache_nodevalues.children_log_incremental_cost_sum + node2.cache_nodevalues.children_log_incremental_cost_sum;
#else
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + node2.children.Length) + logf + log_treeprobability_ratio + node1.LogTreeProbability + node2.LogTreeProbability;
#endif
            log_likelihood_part2 = cacheclass.GetLogOneMinusPi(node1.children.Length + node2.children.Length) +
                node1.cache_nodevalues.children_log_likelihood_posterior_sum + node2.cache_nodevalues.children_log_likelihood_posterior_sum;

            if (log_likelihood_part1 > log_likelihood_part2)
                log_likelihood = log_likelihood_part1 + Math.Log(1 + Math.Exp(log_likelihood_part2 - log_likelihood_part1));
            else
                log_likelihood = log_likelihood_part2 + Math.Log(1 + Math.Exp(log_likelihood_part1 - log_likelihood_part2));

            return log_likelihood;
        }
        #endregion
        //When merge two nodes, tell constraints merge result
        //public static long FeatureVectorLength = 0;
        //public static int FeatureVectorCnt = 0;
        public double logTreeProbabilityRatio;
        //static StreamWriter ofile_rs = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\rs_plus.dat");
        public override RoseTreeNode MergeSingleStep(RoseTreeNode node1, RoseTreeNode node2, int m, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
#if ADJUST_BINGNEW_STEPBYSTEP
            Dictionary<int, Dictionary<int, int>> changeM = Experiments.EvolutionaryExperiments.changeM;
            if (changeM.ContainsKey(node1.MergeTreeIndex))
            {
                Dictionary<int, int> changeM2 = changeM[node1.MergeTreeIndex];
                if (changeM2.ContainsKey(node2.MergeTreeIndex))
                    m = changeM2[node2.MergeTreeIndex];
            }
#endif

#if !UNSORTED_CACHE
            CacheValue topCacheValue = cachedict.getTopValue();
#else
            CacheValue topCacheValue = lastbestkvp.Value;
#endif
#if !SCALABILITY_TEST
            if (CacheValueRecord != null) WriteAllValidCachePair();
#endif
            //remove invalid pairs immediately
            //Console.WriteLine("[{0}] {1},{2}", mergedtreepointer, node1.MergeTreeIndex, node2.MergeTreeIndex);
            this.cachedict.RemoveInvalidateNodePairs(node1);
            this.cachedict.RemoveInvalidateNodePairs(node2);
            node1.CacheMergePairs.Clear(); node2.CacheMergePairs.Clear();
            node1.valid = node2.valid = false;  //for updatecachevalues

            RoseTreeNode newnode = null;
#if !UNSORTED_CACHE
            logTreeProbabilityRatio = constraint.GetMergeTreeLogProbabilityRatio(node1, node2, m);
#else
#if NEW_CONSTRAINT_MODEL
            logTreeProbabilityRatio = (topCacheValue as ConstrainedCacheValue).log_treeprobability - node1.LogTreeProbability - node2.LogTreeProbability;
#else
            logTreeProbabilityRatio = (topCacheValue as ConstrainedCacheValue).log_treeprobability_ratio;
#endif
#endif
            //if (Math.Abs(logTreeProbabilityRatio) > 0)
            //    Console.Write("");
            //if (Math.Abs(logTreeProbabilityRatio -
            //    (topCacheValue as ConstrainedCacheValue).log_treeprobability_ratio) > 1e-8)
            //    Console.WriteLine("Error calculating logTreeProbabilityRatio!");
            //if (Math.Abs(logTreeProbabilityRatio + node1.LogTreeProbability + node2.LogTreeProbability -
            //    (topCacheValue as ConstrainedCacheValue).log_treeprobability) > 1e-8)
            //    Console.WriteLine("Error calculating logTreeProbabilityRatio!");
            //ofile_rs.WriteLine("[{0}]\t{1}\t{2}\t{3}", mergedtreepointer, log_likelihood - node1.log_likelihood - node2.log_likelihood,
            //    log_likelihood_part1 - log_likelihood_part2, log_likelihood_part1 - log_likelihood);
            //ofile_rs.Flush();
            switch (m)
            {
                case 0:
                    constraint.MergeTwoTrees(node1, node2, MergeType.Join);
                    foreach (Constraint addconstraint in SmoothCostConstraint)
                        addconstraint.MergeTwoTrees(node1, node2, MergeType.Join);
                    newnode = JoinMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                    break;
                case 1:
                    constraint.MergeTwoTrees(node1, node2, MergeType.AbsorbL);
                    foreach (Constraint addconstraint in SmoothCostConstraint)
                        addconstraint.MergeTwoTrees(node1, node2, MergeType.AbsorbL);
                    newnode = AbsorbMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                    break;
                case 2:
                    constraint.MergeTwoTrees(node1, node2, MergeType.AbsorbR);
                    foreach (Constraint addconstraint in SmoothCostConstraint)
                        addconstraint.MergeTwoTrees(node1, node2, MergeType.AbsorbR);
                    newnode = AbsorbMerge(node2, node1, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                    break;
                case 3:
                    constraint.MergeTwoTrees(node1, node2, MergeType.Collapse);
                    foreach (Constraint addconstraint in SmoothCostConstraint)
                        addconstraint.MergeTwoTrees(node1, node2, MergeType.Collapse);
                    newnode = CollapseMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                    break;
                default: return null;
            }

            mergedtreepointer++;
            //if (mergedtreepointer == 1988)
            //    Console.Write("");

            //CheckFeatureVectorCorrectness(node1.data);
            //CheckFeatureVectorCorrectness(node2.data);
            //CheckFeatureVectorCorrectness(newnode.data);

            //if (node1.data.count + node2.data.count < newnode.data.count)
            //    throw new Exception("Error Adding!");

            //Console.WriteLine("[{0}] {1}", mergedtreepointer, newnode.data.count);

            //FeatureVectorLength += newnode.data.count;
            //FeatureVectorCnt++;

            return newnode;
        }

        #region manually merge
        public RoseTreeNode ManuallyMergeSingleStep(RoseTreeNode node1, RoseTreeNode node2, int m, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            RoseTreeNode mFirstNode = nodearray[0];
            nodecounter = 0;

            RoseTreeNode newnode = null;
            logTreeProbabilityRatio = 0;
//#if !UNSORTED_CACHE
//            logTreeProbabilityRatio = constraint.GetMergeTreeLogProbabilityRatio(node1, node2, m);
//#else
//#if NEW_CONSTRAINT_MODEL
//            logTreeProbabilityRatio = (topCacheValue as ConstrainedCacheValue).log_treeprobability - node1.LogTreeProbability - node2.LogTreeProbability;
//#else
//            logTreeProbabilityRatio = (topCacheValue as ConstrainedCacheValue).log_treeprobability_ratio;
//#endif
//#endif
            switch (m)
            {
                case 0:
                    //constraint.MergeTwoTrees(node1, node2, MergeType.Join);
                    //foreach (Constraint addconstraint in SmoothCostConstraint)
                    //    addconstraint.MergeTwoTrees(node1, node2, MergeType.Join);
                    newnode = ManuallyJoinMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                    break;
                case 1:
                    //constraint.MergeTwoTrees(node1, node2, MergeType.AbsorbL);
                    //foreach (Constraint addconstraint in SmoothCostConstraint)
                    //    addconstraint.MergeTwoTrees(node1, node2, MergeType.AbsorbL);
                    newnode = ManuallyAbsorbMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                    break;
                default: throw new Exception("Can only deal with m = 0 and m = 1 presently");
            }

            mergedtreepointer++;

            nodearray[0] = mFirstNode;
            return newnode;
        }

        public RoseTreeNode ManuallyJoinMerge(RoseTreeNode node1, RoseTreeNode node2, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            RoseTreeNode[] newchildren = new RoseTreeNode[2];
            newchildren[0] = node1;
            newchildren[1] = node2;

            RoseTreeNode newnode = GenerateNewNode(newchildren, node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, 0, Math.Max(node1.tree_depth, node2.tree_depth) + 1);
            //this.clusternum--;

            //int[] nearestneighborlist = SearchNearestNeighbors(newnode.indices.array_index);
            //CacheNearestNeighbors(newnode, nearestneighborlist);

            return newnode;
        }

        public RoseTreeNode ManuallyAbsorbMerge(RoseTreeNode node1, RoseTreeNode node2, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            RoseTreeNode[] newchildren = new RoseTreeNode[node1.children.Length + 1];
            for (int i = 0; i < node1.children.Length; i++)
                newchildren[i] = node1.children[i];
            newchildren[node1.children.Length] = node2;

            RoseTreeNode newnode = GenerateNewNode(newchildren, node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, 1, Math.Max(node1.tree_depth, node2.tree_depth + 1));
            //this.clusternum--;

            //int[] nearestneighborlist = SearchNearestNeighbors(newnode.indices.array_index);
            //CacheNearestNeighbors(newnode, nearestneighborlist);

            node1.children = null;
            return newnode;
        }
        #endregion manually merge

        //static StreamWriter ofile_update = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\update.dat");
        public void UpdateCacheValues(int arrayindex)
        {
            //if (arrayindex == 136 || arrayindex == 120)
            //    Console.Write("");
            //if (arrayindex < 0 || arrayindex >= nodearray.Length)
            //    Console.Write("");
#if OPEN_LARGE_CLUSTER
            if (nodearray[arrayindex] == null)
                return;
#endif
            RoseTreeNode node = nodearray[arrayindex];
            //if (node == null)
            //    Console.Write("");
            if (!node.valid)
                throw new Exception("Error!");

            //Dictionary<CacheKey, CacheValue> new_nodecachedict = new Dictionary<CacheKey, CacheValue>();
            HashSet<CacheKey> removedkeyhash = new HashSet<CacheKey>();
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in node.CacheMergePairs)
            {
                if (!kvp.Value.node1.valid || !kvp.Value.node2.valid)
                {
                    removedkeyhash.Add(kvp.Key);
                    continue;
                }
                //remove
                //ofile_update.WriteLine("Remove key for {0} and {1} ({2}). Tag: {3}",
                //    kvp.Value.node1.MergeTreeIndex, kvp.Value.node2.MergeTreeIndex, kvp.Value.m, kvp.Key.tag);
                //ofile_update.Flush();
                if (!cachedict.RemoveKey(kvp.Key))
                {
                    removedkeyhash.Add(kvp.Key);
                    continue;
                }
                //update
                double logf = kvp.Value.logf;
                RoseTreeNode node0 = kvp.Value.node1, node1 = kvp.Value.node2;
                int m = kvp.Value.m;
                ConstrainedCacheKey ck = kvp.Key as ConstrainedCacheKey;
                ConstrainedCacheValue cv = kvp.Value as ConstrainedCacheValue;
#if NEW_CONSTRAINT_MODEL
                /// New Constraint Model ///
                double log_treeprobability_ratio, log_likelihood_ratio_posterior, log_likelihood_part1, log_likelihood_part2;
                switch (m)
                {
                    case 0:
                        log_treeprobability_ratio = constraint.GetLogJoinTreeProbabilityRatio(node0, node1);
                        log_likelihood_ratio_posterior = PosterierJoinLogLikelihood(this.cacheclass, node0, node1, logf, log_treeprobability_ratio, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                        break;
                    case 1:
                        log_treeprobability_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(node0, node1);
                        log_likelihood_ratio_posterior = PosterierAbsorbLogLikelihood(this.cacheclass, node0, node1, logf, log_treeprobability_ratio, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                        break;
                    case 2:
                        log_treeprobability_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(node1, node0);
                        log_likelihood_ratio_posterior = PosterierAbsorbLogLikelihood(this.cacheclass, node1, node0, logf, log_treeprobability_ratio, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                        break;
                    default://case 3:
                        log_treeprobability_ratio = constraint.GetLogCollapseTreeProbabilityRatio(node0, node1);
                        log_likelihood_ratio_posterior = PosterierCollapseLogLikelihood(this.cacheclass, node0, node1, logf, log_treeprobability_ratio, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                        break;
                }
                ck.UpdatePosteriorRatio(log_likelihood_ratio_posterior);
                cv.UpdateCacheValue(log_likelihood_part1, log_likelihood_part2, log_treeprobability_ratio + node0.LogTreeProbability + node1.LogTreeProbability);
#else
                /// Previous Constraint Model ///
                double log_treeprobability_ratio;
                switch (m)
                {
                    case 0: log_treeprobability_ratio = constraint.GetLogJoinTreeProbabilityRatio(node0, node1); break;
                    case 1: log_treeprobability_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(node0, node1); break;
                    case 2: log_treeprobability_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(node1, node0); break;
                    default: log_treeprobability_ratio = constraint.GetLogCollapseTreeProbabilityRatio(node0, node1); break;
                }
                ck.UpdatePosteriorRatio(kvp.Key.log_likelihood_ratio + log_treeprobability_ratio);
                cv.UpdateCacheValue(log_treeprobability_ratio);
#endif
                //ofile_update.WriteLine("Add key for {0} and {1} ({2}). Tag: {3}",
                //    cv.node1.MergeTreeIndex, cv.node2.MergeTreeIndex, cv.m, ck.tag);
                //ofile_update.Flush();
                //new_nodecachedict.Add(ck, cv);
            }

            foreach (CacheKey removedkey in removedkeyhash)
                node.CacheMergePairs.Remove(removedkey);
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in node.CacheMergePairs)
                cachedict.Insert(kvp.Key, kvp.Value);
            //ofile_update.Flush();
            //node.CacheMergePairs = new_nodecachedict;
        }

#if NEW_CONSTRAINT_MODEL
        public override RoseTreeNode GenerateNewNode(RoseTreeNode[] newchildren, RoseTreeNode node1, RoseTreeNode node2,
            double log_likelihood_posterior, double logf, double cache_valuearray_plus_alpha,
            double log_likelihood_part1, double log_likelihood_part2, int m, int tree_depth)
        {
            double log_likelihood = double.NaN;
            double children_log_likelihood_posterior_sum = double.NaN;
            double children_log_treeprobability_sum = double.NaN;
            double children_log_incremental_cost_sum = double.NaN;

            switch (m)
            {
                case 0:
                    log_likelihood = node1.JoinLogLikelihood(cacheclass, node1, node2, logf, out log_likelihood_part1, out log_likelihood_part2);
                    children_log_likelihood_posterior_sum = node1.log_likelihood_posterior + node2.log_likelihood_posterior; 
                    children_log_incremental_cost_sum = logTreeProbabilityRatio;
                    children_log_treeprobability_sum = node1.LogTreeProbability + node2.LogTreeProbability; break;
                case 1:
                    log_likelihood = node1.AbsorbLogLikelihood(cacheclass, node1, node2, logf, out log_likelihood_part1, out log_likelihood_part2);
                    children_log_likelihood_posterior_sum = node1.cache_nodevalues.children_log_likelihood_posterior_sum + node2.log_likelihood_posterior;
                    children_log_incremental_cost_sum = node1.cache_nodevalues.children_log_incremental_cost_sum + logTreeProbabilityRatio;
                    children_log_treeprobability_sum = node1.cache_nodevalues.children_log_treeprobability_sum + node2.LogTreeProbability; break;
                case 2:
                    log_likelihood = node1.AbsorbLogLikelihood(cacheclass, node2, node1, logf, out log_likelihood_part1, out log_likelihood_part2);
                    children_log_likelihood_posterior_sum = node1.log_likelihood_posterior + node2.cache_nodevalues.children_log_likelihood_posterior_sum; 
                    children_log_incremental_cost_sum = node2.cache_nodevalues.children_log_incremental_cost_sum + logTreeProbabilityRatio;
                    children_log_treeprobability_sum = node1.LogTreeProbability + node2.cache_nodevalues.children_log_treeprobability_sum ; break;
                case 3:
                    log_likelihood = node1.CollapseLogLikelihood(cacheclass, node1, node2, logf, out log_likelihood_part1, out log_likelihood_part2);
                    children_log_likelihood_posterior_sum = node1.cache_nodevalues.children_log_likelihood_posterior_sum + node2.cache_nodevalues.children_log_likelihood_posterior_sum;
                    children_log_incremental_cost_sum = node1.cache_nodevalues.children_log_incremental_cost_sum + node2.cache_nodevalues.children_log_incremental_cost_sum + logTreeProbabilityRatio;
                    children_log_treeprobability_sum = node1.cache_nodevalues.children_log_treeprobability_sum + node2.cache_nodevalues.children_log_treeprobability_sum; break;
            }

            RoseTreeNode newnode = base.GenerateNewNode(newchildren, node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, m, tree_depth);

            newnode.MergeTreeIndex = mergedtreepointer;
            newnode.log_likelihood_posterior = log_likelihood_posterior - (node1.log_likelihood + node2.log_likelihood) + (node1.log_likelihood_posterior + node2.log_likelihood_posterior);
            newnode.LogTreeProbability = logTreeProbabilityRatio + node1.LogTreeProbability + node2.LogTreeProbability;
            newnode.cache_nodevalues.children_log_likelihood_posterior_sum = children_log_likelihood_posterior_sum;
            newnode.cache_nodevalues.children_log_treeprobability_sum = children_log_treeprobability_sum;
            newnode.cache_nodevalues.children_log_incremental_cost_sum = children_log_incremental_cost_sum;

            return newnode;
        }
#else
        public override RoseTreeNode GenerateNewNode(RoseTreeNode[] newchildren, RoseTreeNode node1, RoseTreeNode node2,
    double log_likelihood, double logf, double cache_valuearray_plus_alpha,
    double log_likelihood_part1, double log_likelihood_part2, int m, int tree_depth)
        {
            RoseTreeNode newnode = base.GenerateNewNode(newchildren, node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, m, tree_depth);
            newnode.MergeTreeIndex = mergedtreepointer;
            return newnode;
        }
#endif

        protected virtual void GetSimilarityAndDepthDifference(CacheValue[] cv, out double similarity, out int[] depth_difference)
        {
            RoseTreeNode newnode = cv[0].node1;
            RoseTreeNode nearestneighbor = cv[0].node2;

            similarity = newnode.data.Cosine(newnode.data, nearestneighbor.data);
            depth_difference = new int[4]
            {
                Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth),
                Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth - 1),
                Math.Abs(nearestneighbor.tree_depth - newnode.tree_depth - 1),
                Math.Abs(newnode.tree_depth - nearestneighbor.tree_depth)
            };
        }



        void CheckFeatureVectorCorrectness(SparseVectorList vector)
        {
            int[] keylist = vector.keyarray;
            int prev = -1;
            for (int i = 0; i < vector.count; i++)
            {
                if(prev>keylist[i])
                    throw new Exception("Fail CheckFeatureVectorCorrectness!");
                prev = keylist[i];
            }
        }

        public void InitializeCacheValueRecord(string filename)
        {
            CacheValueRecord = new StreamWriter(filename);
            loglikelihoodStat = new DataStatistic();
            logtreeprobStat = new DataStatistic();
            logposterior = new DataStatistic();
            //LikelihoodStdRecords = new List<double>();
        }

        DataStatistic loglikelihoodStat, logtreeprobStat, logposterior;
        public void WriteNewlyCachedValue(CacheKey[] ck)
        {
            for (int i = 0; i < ck.Length; i++)
            {
                if (ck[i].keyvalue != double.MinValue && ck[i].keyvalue != double.MaxValue)
                {
                    loglikelihoodStat.AddData(ck[i].log_likelihood_ratio);
                    logtreeprobStat.AddData(ck[i].keyvalue - ck[i].log_likelihood_ratio);
                    logposterior.AddData(ck[i].keyvalue);
                }
            }
        }

        //public static List<double> LikelihoodStdRecords;
        public void WriteAllValidCachePair()
        {
            CacheValueRecord.WriteLine(loglikelihoodStat.ToString() + logtreeprobStat.ToString() + logposterior.ToString());
            CacheValueRecord.Write("Iter " + mergedtreepointer + ":\t");
            CacheValueRecord.WriteLine(this.cachedict.AllValidPairToString());
            CacheValueRecord.Flush();

            //LikelihoodStdRecords.Add(loglikelihoodStat.Max - loglikelihoodStat.Avg);
            //LikelihoodStdRecords.Add(loglikelihoodStat.Std);
            //LikelihoodStdRecords.Add(CacheSortedDictionary.RecordedStd);

            loglikelihoodStat = new DataStatistic();
            logtreeprobStat = new DataStatistic();
            logposterior = new DataStatistic();
        }

        #region Smoothness Cost
        public void AddSmoothCostConstraint(Constraint addconstraint)
        {
            SmoothCostConstraint.Add(addconstraint);
        }

        static Type[] constraintType = { typeof(TreeDistanceConstraint), typeof(TreeOrderConstraint) }; 
        public double[] GetNormalizedSmoothnessCost(int last = 1)
        {
            double[] smoothcost = new double[constraintType.Length];
            for (int i = 0; i < constraintType.Length; i++)
            {
                Constraint corconstraint = null;
                Constraint constraintTemp = this.constraint;
                if (constraintTemp.ConstraintType == ConstraintType.Multiple)
                    constraintTemp = (constraint as MultipleConstraints).GetLastConstraint(last);

                if (constraintTemp.GetType() == constraintType[i] ||
                    constraintTemp.GetType().BaseType == constraintType[i])
                    corconstraint = constraintTemp;
                else
                {
                    foreach (Constraint addconstraint in SmoothCostConstraint)
                    {
                        if (addconstraint.ConstraintType == ConstraintType.Multiple)
                            constraintTemp = (addconstraint as MultipleConstraints).GetLastConstraint(last);
                        else
                            constraintTemp = addconstraint;

                        if (constraintTemp.GetType() == constraintType[i] ||
                            constraintTemp.GetType().BaseType == constraintType[i])
                        {
                            corconstraint = constraintTemp;
                            break;
                        }
                    }
                }

                if (corconstraint != null)
                    smoothcost[i] = corconstraint.NormalizedSmoothnessCost;
                else
                    smoothcost[i] = 0;
            }
            return smoothcost;
        }
        #endregion Smoothness Cost

        #region Adjust rose tree children order
        public void AdjustChildrenOrder(ConstraintTree constriantTree)
        {
            List<RoseTreeNode> rtqueue = new List<RoseTreeNode>();
            List<ConstraintTreeNode> ctqueue = new List<ConstraintTreeNode>();
            rtqueue.Add(root);
            ctqueue.Add(constriantTree.Root);//Sometimes not the root
            while (rtqueue.Count != 0)
            {
                RoseTreeNode rtnode = rtqueue[0];
                ConstraintTreeNode ctnode = ctqueue[0];
                rtqueue.RemoveAt(0);
                ctqueue.RemoveAt(0);

                if (rtnode.children == null)
                    continue;
                
                List<int> orderedMergeTreeIndices = new List<int>();
                foreach (ConstraintTreeNode child_ctnode in ctnode.Children)
                    orderedMergeTreeIndices.Add(child_ctnode.InitialIndex);
                rtnode.AdjustChildrenOrder(orderedMergeTreeIndices);

                rtqueue.AddRange(rtnode.children);
                ctqueue.AddRange(ctnode.Children);
            }
        }

        #endregion

        #region open rose tree node
        public List<RoseTreeNode> OpenedNodeList = new List<RoseTreeNode>();
        public bool IsTreeOpened { get { return OpenedNodeList.Count != 0; } }

        public void OpenRoseTreeNode(RoseTreeNode rtnode, RoseTree constraintRoseTree,
            DataProjectionRelation projRelation,
            double alpha, double gamma, bool bRelabelTree = true,
            double loseorderpunishweight = -1, double increaseorderpunishweight = -1)
        {
            //Console.Write("Open:[{0}]\t", rtnode.MergeTreeIndex);

            #region check if input is legal
            if (nodearray[rtnode.indices.array_index] != rtnode)
                throw new Exception("Error! Node tried to open is not in this rose tree!");
            if (rtnode.tree_depth != 2)
                throw new Exception("Error! Presently can only open node with depth of 2!");
            if (this.constraint.ConstraintType != ConstraintType.NoConstraint &&
                this.constraint.ConstraintType != ConstraintType.TreeOrder &&
                this.constraint.ConstraintType != ConstraintType.LooseTreeOrder)
                throw new Exception("Error! This rose tree node cannot be opened because of unsupported constraint type!");
            if (this.constraint is TreeOrderConstraint)
            {
                if (loseorderpunishweight < 0) loseorderpunishweight = (this.constraint as TreeOrderConstraint).LoseOrderPunishWeight;
//#if OPEN_LARGE_CLUSTER_MOD_2
//                if (increaseorderpunishweight < 0) increaseorderpunishweight = 0;
//#else
                if (increaseorderpunishweight < 0) increaseorderpunishweight = (this.constraint as TreeOrderConstraint).IncreaseOrderPunishWeight;
//#endif
            }
            #endregion check if input is legal

            /// Build a constraint tree ///
            Constraint subconstraint;
            switch (this.constraint.ConstraintType)
            {
                case ConstraintType.NoConstraint:
                    subconstraint = new NoConstraint();
                    break;
                case ConstraintType.TreeOrder:
                    subconstraint = new TreeOrderConstraint(constraintRoseTree, lfv, loseorderpunishweight, increaseorderpunishweight, projRelation);
                    break;
                default: //case ConstraintType.LooseTreeOrder:
                    subconstraint = new LooseTreeOrderConstraint(constraintRoseTree, lfv, loseorderpunishweight, increaseorderpunishweight, projRelation);
                    break;
            }

            int subdepth;
            double subloglikelihood;
            SubRoseTree subrosetree = new SubRoseTree(this, rtnode, alpha, gamma, AdjustStructureOthersThreshold);
            subrosetree.Run(subconstraint, int.MaxValue, out subdepth, out subloglikelihood);
            if (subdepth == 2)
            {
                foreach (RoseTreeNode childrtnode in rtnode.children)
                    childrtnode.parent = rtnode;
                return;
            }

            OpenedNodeList.Add(rtnode);
            /// Open the corresponding rose tree node ///
            rtnode.children = subrosetree.root.children;
            rtnode.OpenedNode = true;
            foreach (RoseTreeNode rtchild in subrosetree.root.children) rtchild.parent = rtnode;

            /// Width first traversal ///             
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            int pointer = 0;
            nodelist.Add(rtnode);
            while (pointer != nodelist.Count)
            {
                RoseTreeNode listrtnode = nodelist[pointer];
                foreach (RoseTreeNode child in listrtnode.children)
                    if (child.children != null)
                        nodelist.Add(child);
                pointer++;
            }

            //Check legal structure
            //foreach(RoseTreeNode newtopicnode in nodelist)
            //    if(newtopicnode.children.Length==1)
            //        Console.WriteLine("Illegal tree structure");

            //Find merge tree index for new topic nodes
            nodelist.RemoveAt(0);
            int lastmergetreeindex = rtnode.MergeTreeIndex;
            foreach (RoseTreeNode newtopicnode in nodelist)
            {
                lastmergetreeindex = FindLastInvalidMergeTreeIndexBefore(lastmergetreeindex);
                //if (lastmergetreeindex == 206)
                //    Console.Write("");
                newtopicnode.MergeTreeIndex = lastmergetreeindex;
                newtopicnode.indices.array_index = lastmergetreeindex;
                nodearray[lastmergetreeindex] = newtopicnode;
                //Console.Write(lastmergetreeindex + "\t");
            }
            //Console.WriteLine();
            //Update tree depth
            for (int inode = nodelist.Count - 1; inode >= 0; inode--)
                UpdateTreeDepth(nodelist[inode]);
            UpdateTreeDepth(rtnode);
            //rtnode.tree_depth = subdepth;
            RoseTreeNode updatenode = rtnode;
            while (updatenode.parent != null)
            {
                //updatenode.parent.tree_depth = updatenode.tree_depth + 1;
                UpdateTreeDepth(updatenode.parent);
                updatenode = updatenode.parent;
            }

            
            /// Modify the constraint tree ///
            if (this.constraint.ConstraintType != ConstraintType.NoConstraint)
                (this.constraint as TreeOrderConstraint).
                    UpdateConstraintTreeNodeOpened(subrosetree, constraintRoseTree, nodelist, projRelation);

            if (bRelabelTree)
            {
                int depth;
                LabelTreeIndices(out depth);
            }
        }

        private void UpdateTreeDepth(RoseTreeNode rtnode)
        {
            if (rtnode.children == null)
                rtnode.tree_depth = 1;
            else{
                int maxchilddepth = int.MinValue;
                foreach (RoseTreeNode child in rtnode.children)
                    if (child.tree_depth > maxchilddepth)
                        maxchilddepth = child.tree_depth;
                rtnode.tree_depth = maxchilddepth + 1;
            }
        }

        private int FindLastInvalidMergeTreeIndexBefore(int lastmergetreeindex)
        {
            while (lastmergetreeindex > 0)
            {
                lastmergetreeindex--;
                if (nodearray[lastmergetreeindex] == null || 
                    nodearray[lastmergetreeindex].parent == null && 
                    nodearray[lastmergetreeindex] != root)
                    return lastmergetreeindex;
            }
            throw new Exception("Error! FindLastInvalidMergeTreeIndexBefore");
        }
        
        #endregion open rose tree node

        #region collapse too small clusters
        Dictionary<RoseTreeNode, List<RoseTreeNode>> collapsednodes = new Dictionary<RoseTreeNode, List<RoseTreeNode>>();
        public Dictionary<RoseTreeNode, List<RoseTreeNode>> CollapseTooSmallClusters(int collapseClusterSize, int collapseClusterNumber)
        {
            IList<RoseTreeNode> validinternalnodes = GetAllValidInternalTreeNodes();
            Dictionary<RoseTreeNode, List<RoseTreeNode>> collapseNodesDic = new Dictionary<RoseTreeNode,List<RoseTreeNode>>();
            HashSet<RoseTreeNode> singleclusterremove = new HashSet<RoseTreeNode>();
            foreach (RoseTreeNode validinternalnode in validinternalnodes)
            {
                //single cluster child, weird structure
                if(validinternalnode.tree_depth == 2 && validinternalnode.parent!=null)
                {
                    RoseTreeNode parent = validinternalnode.parent;
                    if (validinternalnode.LeafCount + parent.children.Length - 1 == parent.LeafCount)
                    {
                        collapseNodesDic.Add(parent, new List<RoseTreeNode>());
                        collapseNodesDic[parent].Add(validinternalnode);
                        singleclusterremove.Add(parent);
                        continue;
                    }
                }
                if (validinternalnode.LeafCount <= collapseClusterSize && validinternalnode.tree_depth == 2)
                {
                    RoseTreeNode parent = validinternalnode.parent;
                    if (!collapseNodesDic.ContainsKey(parent))
                        collapseNodesDic.Add(parent, new List<RoseTreeNode>());
                    collapseNodesDic[parent].Add(validinternalnode);
                }
            }

            foreach (KeyValuePair<RoseTreeNode, List<RoseTreeNode>> kvp in collapseNodesDic)
            {
                if (!singleclusterremove.Contains(kvp.Key) && 
                    kvp.Value.Count < collapseClusterNumber)
                {
                    int verysmallcount = 0;
                    foreach (RoseTreeNode collapsenode in kvp.Value)
                        if (collapsenode.children.Length< 5)
                            verysmallcount++;
                    if (verysmallcount < 2)
                        continue;
                }
                CollapseNode(kvp.Key, kvp.Value);
                collapsednodes.Add(kvp.Key, kvp.Value);
            }

            return collapsednodes;
        }

        public void CollapseNode(RoseTreeNode parent, List<RoseTreeNode> collapsechildlist)
        {
            TestConstraintTreeRoseTreeConsistent();

            //Modify rose tree
            RoseTreeNode[] children = parent.children;
            List<RoseTreeNode> orgchildren = new List<RoseTreeNode>();
            foreach (RoseTreeNode child in children)
                if (!collapsechildlist.Contains(child))
                    orgchildren.Add(child);
            int newchildrenlen = orgchildren.Count;
            foreach (RoseTreeNode collapsechild in collapsechildlist)
                newchildrenlen += collapsechild.children.Length;

            children = new RoseTreeNode[newchildrenlen];
            int ichild = 0;
            foreach (RoseTreeNode rtnode in orgchildren)
                children[ichild++] = rtnode;
            foreach (RoseTreeNode collapsechild in collapsechildlist)
            {
                foreach (RoseTreeNode rtnode in collapsechild.children)
                {
                    children[ichild++] = rtnode;
                    rtnode.parent = parent;
                }
                //collapsechild.Invalidate();
                //Console.WriteLine("Set null:{0}", collapsechild.MergeTreeIndex);
                nodearray[collapsechild.MergeTreeIndex] = null;
                collapsechild.parent = null;
            }
            parent.children = children;

            //Modify constraint tree
            ConstraintTree ctree = null;
            if (constraint is TreeOrderConstraint)
            {
                ctree = (constraint as TreeOrderConstraint).GetConstraintTree();
            }
            if (constraint is MultipleConstraints)
                if ((constraint as MultipleConstraints).GetLastConstraint() is TreeOrderConstraint)
                    ctree = ((constraint as MultipleConstraints).GetLastConstraint() as TreeOrderConstraint).GetConstraintTree();
            if (ctree != null)
            {
                //Console.Write("Collapse: ");
                foreach (RoseTreeNode collapsechild in collapsechildlist)
                {
                    ctree.CollapseTooSmallCollapseNode(collapsechild);
                    //Console.Write("{0} ", collapsechild.MergeTreeIndex);
                }
                //Console.WriteLine();
            }

            TestConstraintTreeRoseTreeConsistent();
        }

        #endregion collapse too small clusters

        public Constraint GetConstraint()
        {
            return constraint;
        }

        #region adjust tree structure no remove nodes
        internal void AdjustTreeStructureProject()
        {
            //Trace.WriteLine("In AdjustTreeStructureProject");

            List<RoseTreeNode> nodeList = new List<RoseTreeNode>();
            nodeList.Add(root);

            int nodeIndex = 0;
            while (nodeIndex < nodeList.Count)
            {
                var node = nodeList[nodeIndex];

                if (node.children != null && node.children.Length != 0)
                {
                    AdjustTreeNodeProject(node);
                    nodeList.AddRange(node.children);
                }

                nodeIndex++;
            }

            RecalculateTreeCachedData();

            int depth;
            LabelTreeIndices(out depth);
        }

        private void AdjustTreeNodeProject(RoseTreeNode node)
        {
            //Trace.WriteLine("In AdjustTreeNodeProject, node " + node.MergeTreeIndex);
            
            List<RoseTreeNode> topicNodes = new List<RoseTreeNode>();
            List<RoseTreeNode> docNodes = new List<RoseTreeNode>();

            foreach (var child in node.children)
            {
                if (child.children == null || child.children.Length == 0)
                    docNodes.Add(child);
                else
                    topicNodes.Add(child);
            }

            //Trace.WriteLine(string.Format("DocNodeCnt: {0}, TopicNodeCnt: {1}", docNodes.Count, topicNodes.Count));

            if (topicNodes.Count == 0 || docNodes.Count == 0)
                return;

            foreach (var docNode in docNodes)
            {
                double maxSimi = double.MinValue;
                RoseTreeNode maxSimiNode = null;
                foreach (var topicNode in topicNodes)
                {
                    double simi = MaxSimilarityContentVectorDataProjection.
                        ContentVectorCosine(topicNode.data, docNode.data);
                    if (simi > maxSimi)
                    {
                        maxSimi = simi;
                        maxSimiNode = topicNode;
                    }
                }
                AssignDoumentToTopicNode(docNode, maxSimiNode);
            }

            if (topicNodes.Count != 1)
                node.children = topicNodes.ToArray<RoseTreeNode>();
            else
            {
                var newchildren = topicNodes[0].children;
                foreach (var child in newchildren)
                    child.parent = node;
                node.children = newchildren;
                nodearray[topicNodes[0].indices.array_index] = null;
                AdjustTreeNodeProject(node);
            }
        }

        private void AssignDoumentToTopicNode(RoseTreeNode documentNode, RoseTreeNode topicNode)
        {
            //Trace.WriteLine("In AssignDoumentToTopicNode");

            var preChildren = topicNode.children;
            var newChildren = new RoseTreeNode[preChildren.Length + 1];

            for (int i = 0; i < preChildren.Length; i++)
            {
                newChildren[i] = preChildren[i];
            }
            newChildren[preChildren.Length] = documentNode;
            documentNode.parent = topicNode;
            topicNode.children = newChildren;
        }
        #endregion

        #region adjust tree structure remove nodes
        internal void AdjustTreeStructure()
        {
            if (model_index != RoseTreeTaxonomy.Constants.Constant.DCM)
                Console.WriteLine("Can only deal with DCM now! Other Methods Result in Incorrect LogLikelihoods!");
                //throw new Exception("Can only deal with DCM now!");

            TestConstraintTreeRoseTreeConsistent();

            AdjustTreeStructureCollapseTooSmallTopics();
            AdjustTreeStructureBalance();
            //AdjustTreeStructureOpenLargeClusters();
            RecalculateTreeCachedData();

            int depth;
            LabelTreeIndices(out depth);
            if (constraint is TreeOrderConstraint)
            {
                ConstraintTree ctree = (constraint as TreeOrderConstraint).GetConstraintTree();
                AdjustChildrenOrder(ctree);
            
            }
            if (constraint is MultipleConstraints)
                if ((constraint as MultipleConstraints).GetLastConstraint() is TreeOrderConstraint)
                    AdjustChildrenOrder(((constraint as MultipleConstraints).GetLastConstraint() as TreeOrderConstraint).GetConstraintTree());
        }

        public bool TestConstraintTreeRoseTreeConsistent()
        {
            ConstraintTree constraintTree = null;
            if (constraint is TreeOrderConstraint)
                constraintTree = (constraint as TreeOrderConstraint).GetConstraintTree();
            if (constraint is MultipleConstraints)
                constraintTree = ((constraint as MultipleConstraints).GetLastConstraint() as TreeOrderConstraint).GetConstraintTree();

            if (constraintTree == null)
            {
                Trace.WriteLine("TestConsistentError!! Constraint Tree is null!");
                return false;
            }

            //ADD this//
            foreach (var node in nodearray)
                if (node != null)
                    if (node.MergeTreeIndex != node.indices.array_index ||
                        nodearray[node.MergeTreeIndex] != node)
                    {
                        Trace.WriteLine(string.Format("TestConsistentError!! Node mergeIndex: {0}, array index: {1}, test {2}",
                            node.MergeTreeIndex, node.indices.array_index, nodearray[node.MergeTreeIndex] == node));
                    }
            //End//

            List<RoseTreeNode> rtqueue = new List<RoseTreeNode>();
            List<ConstraintTreeNode> ctqueue = new List<ConstraintTreeNode>();
            rtqueue.Add(root);
            ctqueue.Add(constraintTree.Root);//Sometimes not the root
            while (rtqueue.Count != 0)
            {
                RoseTreeNode rtnode = rtqueue[0];
                ConstraintTreeNode ctnode = ctqueue[0];

                if (!TestNodeConsistent(rtnode, ctnode))
                    return false;

                rtqueue.RemoveAt(0);
                ctqueue.RemoveAt(0);

                if (rtnode.children == null)
                    continue;

                List<int> orderedMergeTreeIndices = new List<int>();
                foreach (ConstraintTreeNode child_ctnode in ctnode.Children)
                    orderedMergeTreeIndices.Add(child_ctnode.InitialIndex);
                rtnode.AdjustChildrenOrder(orderedMergeTreeIndices);

                rtqueue.AddRange(rtnode.children);
                ctqueue.AddRange(ctnode.Children);
            }

            string drawpath = "!repalce this!";
            string rosetreefilename = "!replace this!";
            string constrainttreefilename = "!replace this!";
            DrawRoseTree drawRoseTree = new DrawRoseTree(this, drawpath, 10000);
            drawRoseTree.DrawTree(rosetreefilename);
            constraintTree.DrawConstraintTree(constrainttreefilename);


            return true;
        }

        public bool TestNodeConsistent(RoseTreeNode rtnode, ConstraintTreeNode ctnode)
        {
            int rtchildcnt = rtnode.children == null ? 0 : rtnode.children.Length;
            int ctchildcnt = ctnode.Children == null ? 0 : ctnode.Children.Count;

            bool bTestSuccess = true;
            if (!rtnode.valid)
            {
                Trace.WriteLine(string.Format("Rose Tree Node {0} not valid:", rtnode.MergeTreeIndex));
                bTestSuccess = false;
            }

            if (rtnode.MergeTreeIndex != ctnode.InitialIndex)
            {
                Trace.WriteLine(string.Format("Rose Tree Node Index: {0}, Constraint Tree Node index {1}",
                    rtnode.MergeTreeIndex, ctnode.InitialIndex));
                bTestSuccess = false;
            }

            if (rtchildcnt != ctchildcnt)
            {
                Trace.WriteLine(string.Format("TestConsistentError!! Node Children Cnt: {0},{1}", rtchildcnt, ctchildcnt));
                if (rtnode.children != null)
                {
                    Trace.Write("RoseTreeNode" + rtnode.MergeTreeIndex + "Children: ");
                    foreach (var child in rtnode.children)
                        Trace.Write(child.MergeTreeIndex + "\t");
                    Trace.WriteLine("");
                }
                if (ctnode.Children != null)
                {
                    Trace.Write("ConstraintTreeNode" + ctnode.InitialIndex + "Children: ");
                    foreach (var child in ctnode.Children)
                        Trace.Write(child.InitialIndex + "\t");
                    Trace.WriteLine("");
                }
                bTestSuccess = false;
            }

            if (!bTestSuccess)
                return false;

            if (rtchildcnt != 0)
            {
                HashSet<int> mergeTreeIndices = new HashSet<int>();
                foreach (var rtchild in rtnode.children)
                    mergeTreeIndices.Add(rtchild.MergeTreeIndex);
                foreach(var ctchild in ctnode.Children)
                    if (!mergeTreeIndices.Contains(ctchild.InitialIndex))
                    {
                        Trace.WriteLine(string.Format("TestConsistentError!! NotConsistent ContraintTreeNode: {0}", ctchild.InitialIndex));
                        Trace.Write("RoseTreeNodeChildren: ");
                        foreach (var child in rtnode.children)
                            Trace.Write(child.MergeTreeIndex + "\t");
                        Trace.WriteLine("");
                        Trace.Write("ConstraintTreeNodeChildren: ");
                        foreach (var child in ctnode.Children)
                            Trace.Write(child.InitialIndex + "\t");
                        Trace.WriteLine("");
                        return false;
                    }
            }

            return true;
        }

        public bool TestMergeTreeIndex(RoseTreeNode rtnode, ConstraintTreeNode ctnode)
        {
            if (rtnode == null || ctnode == null)
            {
                Trace.WriteLine(string.Format("TestConsistentError!! Node MergeTreeIndex:{0},{1}",
                    (rtnode == null ? "null" : rtnode.MergeTreeIndex.ToString()),
                    (ctnode == null ? "null" : ctnode.InitialIndex.ToString())));
            }
            if (rtnode.MergeTreeIndex != ctnode.InitialIndex)
            {
                Trace.WriteLine(string.Format("TestConsistentError!! MergeTreeIndex:{0},{1}",
                    rtnode.MergeTreeIndex, ctnode.InitialIndex));
                return false;
            }
            else
                return true;
        }

        private void AdjustTreeStructureOpenLargeClusters()
        {
            IList<RoseTreeNode> rtnodesbfs = TopicBreadthFirstTraversal();

            int openThreshold = (int)(AdjustStructureOpenClusterFactor * lfv.featurevectors.Length);
            openThreshold = Math.Max(openThreshold, AdjustStructureOpenClusterThreshold);
            foreach (RoseTreeNode rtnode in rtnodesbfs)
            {
                if (rtnode.tree_depth == 2 && rtnode.LeafCount > openThreshold)
                {
                    AdjustStructureOpenNode(rtnode);
                    //update tree depth
                    RoseTreeNode updatenode = rtnode;
                    while (updatenode != null)
                    {
                        UpdateTreeDepth(updatenode);
                        updatenode = updatenode.parent;
                    }
                }
            }
        }

        private void AdjustStructureOpenNode(RoseTreeNode rtnode)
        {
            double alpharatio = AdjustStructureOpenNodeClusterAlphaRatio;
            if (rtnode.BOthers)
            {
                Console.WriteLine("Others node open! {0}", rtnode.MergeTreeIndex);
                alpharatio = 1;
            }
            int subdepth;
            double subloglikelihood;
            SubRoseTree bestSubrosetree = null, subrosetree;
            double bestSubrosetreeScore = double.MinValue;
            List<double> testGammas = new List<double>(AdjustStructureTestGammas);
            testGammas.Add(this.gamma);
            foreach (double testGamma in testGammas)
            {
                subrosetree = new SubRoseTree(this, rtnode, alpha * alpharatio, testGamma, AdjustStructureOthersThreshold);
                subrosetree.Run(new NoConstraint(), int.MaxValue, out subdepth, out subloglikelihood);
                double score = subrosetree.GetClusteringScore();
                //Console.WriteLine(score);
                if (score > bestSubrosetreeScore)
                {
                    bestSubrosetreeScore = score;
                    bestSubrosetree = subrosetree;
                }
            }
            //Console.WriteLine("-----------------------------------");
            subrosetree = bestSubrosetree;
            if (subrosetree == null || subrosetree.root.tree_depth == 2 || (subrosetree.root.children.Length == 2 && subrosetree.root.children[1].LeafCount < AdjustStructureOthersThreshold))
            {
                Console.WriteLine("AdjustStructure Warning! Large cluster not opened! Node {0} [{1} documents]", rtnode.MergeTreeIndex, rtnode.LeafCount);
                foreach (RoseTreeNode childrtnode in rtnode.children)
                    childrtnode.parent = rtnode;
            }
            else
            {
                if (subrosetree.root.tree_depth != 3)
                    throw new Exception("Can only deal with subtree with depth 3!");
                rtnode.children = subrosetree.root.children;
                int lastmergetreeindex = rtnode.MergeTreeIndex;
                Console.WriteLine("Opened node:" + rtnode.MergeTreeIndex);
                foreach (RoseTreeNode child in rtnode.children)
                {
                    //Deal with the last topic (may be less than threshold)
                    if (child.LeafCount < AdjustStructureOthersThreshold)
                    {
                        if (child != rtnode.children[rtnode.children.Length - 1])
                            throw new Exception("Error! only last node can be less than threshold");
                        if (child.LeafCount == 1)
                        {
                            List<RoseTreeNode> documents = new List<RoseTreeNode>();
                            documents.Add(child);
                            RemoveDocuments(rtnode, documents);
                        }
                        else
                        {
                            //collapse
                            RoseTreeNode[] newchildren = new RoseTreeNode[rtnode.children.Length + child.children.Length - 1];
                            for (int ichild = 0; ichild < rtnode.children.Length - 1; ichild++)
                                newchildren[ichild] = rtnode.children[ichild];
                            for (int ichild = 0; ichild < child.children.Length; ichild++)
                            {
                                newchildren[ichild + rtnode.children.Length - 1] = child.children[ichild];
                                newchildren[ichild + rtnode.children.Length - 1].parent = rtnode;
                            } 
                            rtnode.children = newchildren;

                            //remove
                            RemoveDocuments(rtnode, child.children.ToList<RoseTreeNode>());
                        }
                        break;
                    }

                    //Split 
                    child.parent = rtnode;
                    //put child in the tree
                    lastmergetreeindex = FindLastInvalidMergeTreeIndexBefore(lastmergetreeindex);
                    child.MergeTreeIndex = lastmergetreeindex;
                    child.indices.array_index = lastmergetreeindex;
                    nodearray[lastmergetreeindex] = child;
                    child.tree_depth = 2;
                    //update constraint tree
                    if (constraint is TreeOrderConstraint)
                        (constraint as TreeOrderConstraint).GetConstraintTree().UpdateConstraintTreeSplit(child);
                    else if (constraint is MultipleConstraints)
                    {
                        if ((constraint as MultipleConstraints).GetLastConstraint() is TreeOrderConstraint)
                            ((constraint as MultipleConstraints).GetLastConstraint() as TreeOrderConstraint).GetConstraintTree().UpdateConstraintTreeSplit(child);
                    }
                }
                rtnode.OpenedNode = true;
            }
        }

        private void AdjustTreeStructureBalance()
        {
            IList<RoseTreeNode> rtnodesbfs = TopicBreadthFirstTraversal();

            foreach (RoseTreeNode rtnode in rtnodesbfs.Reverse<RoseTreeNode>())
            {
                if (nodearray[rtnode.MergeTreeIndex] == null ||
                    (rtnode != root && rtnode.parent == null))
                    continue;

                bool bAdjust = false;
                if (rtnode.tree_depth > 2)
                {
                    foreach(RoseTreeNode child in rtnode.children)
                        if (child.children == null)
                        {
                            bAdjust = true;
                            break;
                        }
                }
                if (bAdjust)
                {
                    AdjustNodeBalance(rtnode);
                    //update tree depth
                    RoseTreeNode updatenode = rtnode;
                    while (updatenode != null)
                    {
                        UpdateTreeDepth(updatenode);
                        updatenode = updatenode.parent;
                    }
                }
            }
        }

        private void AdjustNodeBalance(RoseTreeNode rtnode)
        {
            List<RoseTreeNode> documents = new List<RoseTreeNode>();
            foreach (RoseTreeNode child in rtnode.children)
                if (child.children == null)
                    documents.Add(child);
            if (documents.Count >= AdjustStructureOthersThreshold)
            {
#if ADJ_REMOVE_ROOT_OTHERS
                if (rtnode.parent == null && rtnode.children.Length > documents.Count + 1)
                {
                    RemoveDocuments(rtnode, documents);
                }
                else
                    AdjustStructureSetDocumentsOthers(rtnode, documents);
#else
                AdjustStructureSetDocumentsOthers(rtnode, documents);
#endif
            }
            else
            {
                if (documents.Count + 1 == rtnode.children.Length)
                {
                    /// only one topic node ///
                    RoseTreeNode topicNode = null;
                    foreach(RoseTreeNode child in rtnode.children)
                        if (child.children != null)
                        {
                            topicNode = child;
                            break;
                        }

                    //collapse the topic node
                    List<RoseTreeNode> collapsenodelist = new List<RoseTreeNode>();
                    collapsenodelist.Add(topicNode);
                    CollapseNode(rtnode, collapsenodelist);

                    //may need more adjustment
                    if (topicNode.LeafCount != topicNode.children.Length)
                        AdjustNodeBalance(rtnode);
                }
                else
                {
                    /// multiple topic nodes ///
                    RoseTreeNode parentnode = rtnode.parent;
                    if (OnlyOneTopicNode(parentnode))
                        AdjustNodeBalance(parentnode);
                    else
                    {
                        RemoveDocuments(rtnode, documents);
                    }
                }
            }
        }

        private bool OnlyOneTopicNode(RoseTreeNode rtnode)
        {
            int topicNodeNumber = 0;

            try
            {
                if (rtnode == null || rtnode.children == null)
                    return false;

                foreach (RoseTreeNode child in rtnode.children)
                    if (child != null && child.children != null)
                        topicNodeNumber++;
            }
            catch
            {
                Console.WriteLine("OnlyOneTopicNode Exception!");
                Console.WriteLine("rtnode == null:" + rtnode == null);
                bool bChildNull = false;
                foreach (RoseTreeNode child in rtnode.children)
                    if (child == null)
                        bChildNull = true;
                Console.WriteLine("rtnode child null:" + bChildNull);
                return false;
            }

            return topicNodeNumber == 1;
        }

        private void AdjustStructureSetDocumentsOthers(RoseTreeNode rtnode, List<RoseTreeNode> documents)
        {
            TestConstraintTreeRoseTreeConsistent();

            //search an index for the newly generated topic
            //int minMergeTreeIndex = int.MaxValue;
            //foreach (RoseTreeNode document in documents)
            //    if (document.MergeTreeIndex < minMergeTreeIndex)
            //        minMergeTreeIndex = document.MergeTreeIndex;
            int topicIndex = FindLastInvalidMergeTreeIndexBefore(rtnode.MergeTreeIndex);
#if PRINT_MODIFIED_NODES
            Console.WriteLine("Added Topic Node:" + topicIndex);
#endif
            //generate new node, put in the tree
            RoseTreeNode othersnode = new RoseTreeNode(documents.ToArray(), null, null, topicIndex);
            othersnode.MergeTreeIndex = topicIndex;
            othersnode.indices.array_index = topicIndex;
            nodearray[topicIndex] = othersnode;
            othersnode.BOthers = true;

            //adjust children for rtnode 
            RoseTreeNode[] newchildren = new RoseTreeNode[rtnode.children.Length - documents.Count + 1];
            int ichild = 0;
            foreach (RoseTreeNode child in rtnode.children)
            {
                if (child.children != null)
                    newchildren[ichild++] = child;
            }
            newchildren[ichild] = othersnode;
            othersnode.parent = rtnode;
            rtnode.children = newchildren;

            //adjust constraint tree
            if (constraint is TreeOrderConstraint)
                (constraint as TreeOrderConstraint).GetConstraintTree().UpdateConstraintTreeSplit(othersnode, true);
            else if (constraint is MultipleConstraints)
            {
                if ((constraint as MultipleConstraints).GetLastConstraint() is TreeOrderConstraint)
                    ((constraint as MultipleConstraints).GetLastConstraint() as TreeOrderConstraint).GetConstraintTree().UpdateConstraintTreeSplit(othersnode, true);
            }

            othersnode.tree_depth = 2;
            othersnode.LeafCount = othersnode.children.Length;

            TestConstraintTreeRoseTreeConsistent();
        }

        private void RemoveDocuments(RoseTreeNode rtnode, List<RoseTreeNode> documents)
        {

#if PRINT_MODIFIED_NODES
            Console.Write("Removed Docs:");
            foreach (RoseTreeNode rdoc in documents)
                Console.Write(rdoc.MergeTreeIndex + "\t");
            Console.WriteLine();
#endif

            TestConstraintTreeRoseTreeConsistent();

            //adjust children of rtnode
            RoseTreeNode[] newchildren = new RoseTreeNode[rtnode.children.Length - documents.Count];
            int index = 0;
            foreach (RoseTreeNode child in rtnode.children)
                if (child.children != null)
                {
                    newchildren[index++] = child;
                }
                else
                {
                    child.parent = null;
                }
            rtnode.children = newchildren;

            //adjust constraint tree
            if (constraint is TreeOrderConstraint)
                (constraint as TreeOrderConstraint).GetConstraintTree().RemoveDocuments(rtnode, documents);
            else if (constraint is MultipleConstraints)
            {
                if ((constraint as MultipleConstraints).GetLastConstraint() is TreeOrderConstraint)
                    ((constraint as MultipleConstraints).GetLastConstraint() as TreeOrderConstraint).GetConstraintTree().RemoveDocuments(rtnode, documents);
            }

            TestConstraintTreeRoseTreeConsistent();
        }

        private void AdjustTreeStructureCollapseTooSmallTopics()
        {
            IList<RoseTreeNode> rtnodesbfs = TopicBreadthFirstTraversal();

            while (true)
            {
                Dictionary<RoseTreeNode, List<RoseTreeNode>> collapseNodeList = new Dictionary<RoseTreeNode, List<RoseTreeNode>>();
                foreach (RoseTreeNode rtnode in rtnodesbfs.Reverse<RoseTreeNode>())
                {
                    if (rtnode.tree_depth == 2 &&
                        rtnode.LeafCount < AdjustStructureCollapseThreshold)
                    {
                        RoseTreeNode parent = rtnode.parent;
                        if (parent == null || nodearray[rtnode.MergeTreeIndex] == null)
                            continue;
                        if (!collapseNodeList.ContainsKey(parent))
                            collapseNodeList.Add(parent, new List<RoseTreeNode>());
                        collapseNodeList[parent].Add(rtnode);
                    }
                }

                if (collapseNodeList.Count == 0)
                    break;

                foreach (KeyValuePair<RoseTreeNode, List<RoseTreeNode>> kvp in collapseNodeList)
                {
                    CollapseNode(kvp.Key, kvp.Value);
                    UpdateTreeDepth(kvp.Key);
                    //dirtyNodes.Add(kvp.Key);
                }
            }
        }

        protected void RecalculateTreeCachedData()
        {
            IList<RoseTreeNode> rtnodesbfs = TopicBreadthFirstTraversal();
            foreach (RoseTreeNode rtnode in rtnodesbfs.Reverse<RoseTreeNode>())
            {
                //if (rtnode.MergeTreeIndex == 949)
                //    Console.Write("");
                //Console.WriteLine(rtnode.MergeTreeIndex);
                RecalculateNodeCachedData(rtnode);
            }
        }

        private void RecalculateNodeCachedData(RoseTreeNode rtnode)
        {
            RoseTreeNode node0 = rtnode.children[0];
            int m = 0;
            for (int inode = 1; inode < rtnode.children.Length; inode++)
            {
                RoseTreeNode node1 = rtnode.children[inode];
                node0 = ManuallyMergeTwoNodes(node0, node1, m);
                m = 1;
            }

            nodearray[rtnode.MergeTreeIndex] = node0;
            if (rtnode.parent != null)
            {
                node0.parent = rtnode.parent;
                rtnode.parent = null;
                for (int ichild = 0; ichild < node0.parent.children.Length; ichild++)
                {
                    if (node0.parent.children[ichild] == rtnode)
                    {
                        node0.parent.children[ichild] = node0;
                        break;
                    }
                }
            }
            else
                this.root = node0;
            node0.MergeTreeIndex = rtnode.MergeTreeIndex;
            node0.indices.array_index = rtnode.indices.array_index;
            node0.indices.initial_index = rtnode.indices.initial_index;
            node0.indices.tree_index = rtnode.indices.tree_index;
            node0.BOthers = rtnode.BOthers;
            node0.OpenedNode = rtnode.OpenedNode;
            UpdateLeafCount(node0);

            UpdateDepthInTree();
        }

        RoseTreeNode ManuallyMergeTwoNodes(RoseTreeNode mergednode, RoseTreeNode mergednode2, int m)
        {
            double cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, log_likelihood;

            double logf = GetLogF(mergednode, mergednode2, out cache_valuearray_plus_alpha);
            switch (m)
            {
                case 0: log_likelihood = mergednode.JoinLogLikelihood(cacheclass, mergednode, mergednode2, logf, out log_likelihood_part1, out log_likelihood_part2); break;
                case 1: log_likelihood = mergednode.AbsorbLogLikelihood(cacheclass, mergednode, mergednode2, logf, out log_likelihood_part1, out log_likelihood_part2); break;
                case 2: log_likelihood = mergednode.AbsorbLogLikelihood(cacheclass, mergednode2, mergednode, logf, out log_likelihood_part1, out log_likelihood_part2); break;
                default: log_likelihood = mergednode.CollapseLogLikelihood(cacheclass, mergednode, mergednode2, logf, out log_likelihood_part1, out log_likelihood_part2); break;
            }

            mergednode = ManuallyMergeSingleStep(mergednode, mergednode2, m, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);

            return mergednode;
        }

        private void RecalculateNodeCachedData2(RoseTreeNode rtnode)
        {
            //Console.WriteLine(rtnode.MergeTreeIndex);
            //double mlogf, mlogf1, mloglikelihood, mloglikelihood_part1, mloglikelihood_part2;
            //int msubtree_leafcount, mtree_depth;

            //calculate data and logf
            RoseTreeNode node0 = rtnode.children[0];
            SparseVectorList vector = node0.data;
            List<int> overlapping_keylist;
            int new_vector_length = 0;
            double cache_valuearray_plus_alpha;
            double logf_part1 = node0.cache_nodevalues.logf_part1;
            double loglikelihoodsum = node0.log_likelihood;
            int valuearray_sum = node0.data.valuearray_sum;
            int subtree_leaf_count = node0.cache_nodevalues.subtree_leaf_count;
            for (int inode = 1; inode < rtnode.children.Length; inode++)
            {
                RoseTreeNode node1 = rtnode.children[inode];
                vector = vector.AddValue(vector, node1.data, out overlapping_keylist, out new_vector_length);
                valuearray_sum += node1.data.valuearray_sum;
                logf_part1 += node1.cache_nodevalues.logf_part1;
                loglikelihoodsum += node1.log_likelihood;
                subtree_leaf_count += node1.cache_nodevalues.subtree_leaf_count;
            }

            vector.valuearray_sum = valuearray_sum;
            double logf = LogDCM1(logf_part1, valuearray_sum, vector, 
                out cache_valuearray_plus_alpha, new_vector_length);

            //calculate likelihoods
            double log_likelihood_part1 = 0;
            double log_likelihood_part2 = 0;

            double log_likelihood = 0;
            log_likelihood_part1 = cacheclass.GetLogPi(rtnode.children.Length) + logf;
            log_likelihood_part2 = cacheclass.GetLogOneMinusPi(rtnode.children.Length) + loglikelihoodsum;

            if (log_likelihood_part1 > log_likelihood_part2)
                log_likelihood = log_likelihood_part1 + Math.Log(1 + Math.Exp(log_likelihood_part2 - log_likelihood_part1));
            else
                log_likelihood = log_likelihood_part2 + Math.Log(1 + Math.Exp(log_likelihood_part1 - log_likelihood_part2));

            rtnode.data = vector;
            CacheNodeValues(rtnode, log_likelihood, logf, logf_part1, subtree_leaf_count, -1, log_likelihood_part1, log_likelihood_part2);
            rtnode.LeafCount = rtnode.cache_nodevalues.subtree_leaf_count;
            UpdateTreeDepth(rtnode);
            //RoseTreeNode node1 = rtnode.children[0];
            //for (int inode = 1; inode < rtnode.children.Length; inode++)
            //{
            //    RoseTreeNode node2 = rtnode.children[inode];
            //    //prepare new node data


            //    //generate new node
            //    RoseTreeNode newnode = base.MergeSingleStep(node1,node2,3);

            //    node1 = newnode;
            //}

        }

        private IList<RoseTreeNode> TopicBreadthFirstTraversal()
        {
            List<RoseTreeNode> queue = new List<RoseTreeNode>();
            List<RoseTreeNode> aidqueue = new List<RoseTreeNode>();

            if (this.root.tree_depth < 2)
                return queue.AsReadOnly();

            queue.Add(this.root);
            aidqueue.Add(this.root);

            while (aidqueue.Count > 0)
            {
                RoseTreeNode node = aidqueue[0];
                aidqueue.RemoveAt(0);

                //if (node.children != null)
                {
                    foreach (RoseTreeNode child in node.children)
                    {
                        if (child.tree_depth >= 2)
                        {
                            if (nodearray[child.MergeTreeIndex] == null ||
                                (child.parent == null &&
                                child != root))
                                Trace.WriteLine("");
                            queue.Add(child);
                            aidqueue.Add(child);
                        }
                    }
                }
            }

            return queue.AsReadOnly();
        }

        private IList<RoseTreeNode> BreadthFirstTraversal()
        {
            List<RoseTreeNode> queue = new List<RoseTreeNode>();
            List<RoseTreeNode> aidqueue = new List<RoseTreeNode>();
            queue.Add(this.root);
            aidqueue.Add(this.root);

            while (aidqueue.Count > 0)
            {
                RoseTreeNode node = aidqueue[0];
                aidqueue.RemoveAt(0);

                if (node.children != null)
                {
                    queue.AddRange(node.children);
                    aidqueue.AddRange(node.children);
                }
            }

            return queue.AsReadOnly();
        }
        #endregion

        public void GetAverageTreeLeafDistances(out double avgleafdis, out double avgleafsquaredis)
        {
            double dissum = 0;
            double dissquaresum = 0;
            IList<RoseTreeNode> leaves = GetAllTreeLeaf();
            foreach(RoseTreeNode leaf0 in leaves)
                foreach (RoseTreeNode leaf1 in leaves)
                {
                    RoseTreeNode commonancestor = GetCommonAncestor(leaf0, leaf1);
                    int distance = leaf0.DepthInTree + leaf1.DepthInTree - commonancestor.DepthInTree;
                    dissum += distance;
                    dissquaresum += distance * distance;
                }
            avgleafdis = dissum / leaves.Count / leaves.Count;
            avgleafsquaredis = dissquaresum / leaves.Count / leaves.Count;
        }

        static RoseTreeNode prevNode0 = null;
        static HashSet<RoseTreeNode> ancestorlist = null;
        static RoseTreeNode GetCommonAncestor(RoseTreeNode node0, RoseTreeNode node1)
        {
            RoseTreeNode ancestor;
            if (prevNode0 != node0)
            {
                ancestorlist = new HashSet<RoseTreeNode>();
                ancestor = node0;
                ancestorlist.Add(node0);
                while (ancestor.parent != null)
                {
                    ancestorlist.Add(ancestor.parent);
                    ancestor = ancestor.parent;
                }
            }

            ancestor = node1;
            while (true)
            {
                if (ancestor == null)
                    throw new Exception("no common ancestor!");
                if (ancestorlist.Contains(ancestor))
                    return ancestor;
                ancestor = ancestor.parent;
            }
        }
    }

    class DataStatistic
    {
        public double Max = double.MinValue;
        public double Min = double .MaxValue;
        public double Avg { get { return Sum/cnt; } }
        public double Std { get { return Math.Sqrt(SumSquare / cnt - Avg * Avg); } }
        private double Sum = 0;
        private double SumSquare = 0;
        private int cnt = 0;

        public void AddData(double data)
        {
            Sum += data;
            SumSquare += data * data;
            if (data > Max)
                Max = data;
            if (data < Min)
                Min = data;
            cnt++;
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3};", Max, Min, Avg, Std);
        }

        public void Clear()
        {
            Max = double.MinValue;
            Min = double.MaxValue;
            Sum = SumSquare = 0;
            cnt = 0;
        }
    }
}
