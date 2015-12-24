using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.IO;

using EvolutionaryRoseTree.Constraints;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Tools;

namespace EvolutionaryRoseTree.DataStructures
{
    class RuleRoseTree : RoseTree
    {
        public RuleRoseTree(
            int dataset_index,          //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET                             
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

        #region Rules

        Rules rules;
        Dictionary<int, MaxRule> maxrulemap;
        List<int> maxruleclusterlist;
        Dictionary<int, MinRule> minrulemap;
        List<int> minruleclusterlist;
        protected MinRule ActivatedMinRule = null;
        int maxclusterpointer = 0;
        int minclusterpointer = 0;
        int maxActivatedClusterNumber;
        int minActivatedClusterNumber;
        int nextMinActivatedClusterNumber;
#if !SCALABILITY_TEST
        protected bool bNoRules = true;
#endif
        public void SetUpRules(Rules rules)
        {
            this.rules = rules;
            
            // max rules
            maxrulemap = new Dictionary<int, MaxRule>();
            maxruleclusterlist = new List<int>();
            foreach (MaxRule rule in rules.MaxRuleList)
            {
                if (maxruleclusterlist.Contains(rule.ClusterNumber))
                    throw new Exception("Duplicate Cluster! Please merge!");
                maxruleclusterlist.Add(rule.ClusterNumber);
                maxrulemap.Add(rule.ClusterNumber, rule);
            }

            maxruleclusterlist.Sort();
            maxruleclusterlist.Reverse();

            // min rules
            minrulemap = new Dictionary<int, MinRule>();
            minruleclusterlist = new List<int>();
            foreach (MinRule rule in rules.MinRuleList)
            {
                if (minruleclusterlist.Contains(rule.ClusterNumber))
                    throw new Exception("Duplicate Cluster! Please merge!");
                minruleclusterlist.Add(rule.ClusterNumber);
                minrulemap.Add(rule.ClusterNumber, rule);
            }

            minruleclusterlist.Sort();
            minruleclusterlist.Reverse();

            //activate max rule
            int clusterNumber = lfv.featurevectors.Length;
            while (maxclusterpointer < maxruleclusterlist.Count
                && maxruleclusterlist[maxclusterpointer] >= clusterNumber)
                maxclusterpointer++;

            if (maxclusterpointer < maxruleclusterlist.Count)
            {
                this.cachedict = new RuleCacheSortedDictionary();
                maxActivatedClusterNumber = maxruleclusterlist[maxclusterpointer];
                (this.cachedict as RuleCacheSortedDictionary).SetMaxRule(maxrulemap[maxActivatedClusterNumber]);
            }

            //find next activated min rule
            while (minclusterpointer < minruleclusterlist.Count
                && minruleclusterlist[minclusterpointer] > clusterNumber)
                minclusterpointer++;

            if (minclusterpointer < minruleclusterlist.Count)
            {
                if (!(this.cachedict is RuleCacheSortedDictionary))
                    this.cachedict = new RuleCacheSortedDictionary();
                nextMinActivatedClusterNumber = minruleclusterlist[minclusterpointer];
            }
            else
                nextMinActivatedClusterNumber = -1;

#if !SCALABILITY_TEST
            bNoRules = false;
#endif
        }

        protected void CheckActivatedRule()
        {
            if (maxActivatedClusterNumber == this.clusternum)
            {
                maxclusterpointer++;
                if (maxclusterpointer < maxruleclusterlist.Count)
                {
                    maxActivatedClusterNumber = maxruleclusterlist[maxclusterpointer];
                    (this.cachedict as RuleCacheSortedDictionary).SetMaxRule(maxrulemap[maxActivatedClusterNumber]);
                }
                else
                    (this.cachedict as RuleCacheSortedDictionary).SetMaxRule(new EmptyRule());
            }

            if (nextMinActivatedClusterNumber == this.clusternum)
            {
                minActivatedClusterNumber = nextMinActivatedClusterNumber;
                (this.cachedict as RuleCacheSortedDictionary).SetMinRule(minrulemap[minActivatedClusterNumber]);
                ActivatedMinRule = minrulemap[minActivatedClusterNumber];
                minrulemap[minActivatedClusterNumber].Activate(nodearray);
                minclusterpointer++;
                if (minclusterpointer < minruleclusterlist.Count)
                    nextMinActivatedClusterNumber = minruleclusterlist[minclusterpointer];
                else
                    nextMinActivatedClusterNumber = -1;
            }

        }
        #endregion Rules

        //protected static double logTreeProbabilityRatio;
        public override void MergeLoop(int interval)
        {
            StreamWriter sw = InitializeMergeRecordWriter();

            while (clusternum > 1)
            {
                if (clusternum % 100 == 0)
                    Console.WriteLine("In the " + clusternum + "th cluster");
                //Console.Write(clusternum + " ");

                RoseTreeNode node1, node2;
                int m;
                double log_likelihood_ratio, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2;
                double log_total_ratio;

                //if ((this as ConstrainedRoseTree).mergedtreepointer == 254)
                //    Console.Write("");
                log_total_ratio = getTopOne(out node1, out node2, out m, out log_likelihood_ratio, out logf, out cache_valuearray_plus_alpha, out log_likelihood_part1, out log_likelihood_part2);
                if (m == -1)
                {
                    //if (this.cachedict is RuleCacheSortedDictionary)
                    //    (this.cachedict as RuleCacheSortedDictionary).ClearAll();
                    //else
                    //   this.cachedict = new CacheSortedDictionary();
                    CacheNearestNeighborsForAll();
                    log_total_ratio = getTopOne(out node1, out node2, out m, out log_likelihood_ratio, out logf, out cache_valuearray_plus_alpha, out log_likelihood_part1, out log_likelihood_part2);
                    Console.WriteLine("ReCache All!");
                    //if (m == -1)
                    //{
                    //    for (int i = 0; i < nodearray.Length; i++)
                    //        if (nodearray[i] != null && nodearray[i].valid)
                    //            Console.Write(nodearray[i].LeafCount + "[" + nodearray[i].tree_depth + "]\t");
                    //    Console.WriteLine();
                    //    int mk = this.k;
                    //    this.k = clusternum;
                    //    CacheNearestNeighborsForAll();
                    //    log_total_ratio = this.cachedict.getTopOne(out node1, out node2, out m, out log_likelihood_ratio, out logf, out cache_valuearray_plus_alpha, out log_likelihood_part1, out log_likelihood_part2);
                    //    k = mk;
                    //    if (m == -1)
                    //        throw new Exception("Contradict rules!");
                    //}
                }
                //sw.WriteLine(log_likelihood_ratio);

#if !SCALABILITY_TEST
                OutputMergeRecord(sw, m, node1, node2, log_likelihood_ratio, log_total_ratio);
#endif

                RoseTreeNode newnode = MergeSingleStep(node1, node2, m, log_likelihood_ratio + (node1.log_likelihood + node2.log_likelihood), logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                //logTreeProbabilityRatio = log_total_ratio - log_likelihood_ratio;

                UpdateLeafCount(newnode);

#if! SCALABILITY_TEST
                if (ActivatedMinRule != null)
                    ActivatedMinRule.OnMerge(node1, node2, newnode);
#endif

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

#if !UNSORTED_CACHE
                if (this.clusternum % interval == 0)
                {
                    this.cachedict.ClearInvalidItems();
                }
#endif
#if! SCALABILITY_TEST
                if(!bNoRules)
                    CheckActivatedRule();
#endif
            }

            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }

#if UNSORTED_CACHE
        protected KeyValuePair<CacheKey, CacheValue> lastbestkvp;
#endif
        public double getTopOne(out RoseTreeNode node0, out RoseTreeNode node1, out int m, 
            out double log_likelihood_ratio, out double logf, out double cache_valuearray_plus_alpha, 
                out double log_likelihood_part1, out double log_likelihood_part2)
        {
#if UNSORTED_CACHE
            /// UNSORTED CACHE ///
            ConstrainedRoseTree constrainedRoseTree = this as ConstrainedRoseTree;
            int mergetreepointer = constrainedRoseTree.mergedtreepointer;
            Constraint constraint = constrainedRoseTree.GetConstraint();
            ConstrainedCacheKey bestkey = new ConstrainedCacheKey(double.MinValue, double.MinValue, int.MaxValue, int.MaxValue, double.MinValue);
            KeyValuePair<CacheKey, CacheValue> bestkvp = new KeyValuePair<CacheKey, CacheValue>(bestkey, null);
            for (int iNode = 0; iNode < mergetreepointer; iNode++)
            {
                RoseTreeNode node = nodearray[iNode];
                if (node == null || !node.valid)
                    continue;
                foreach (KeyValuePair<CacheKey, CacheValue> mergepair in node.CacheMergePairs)
                {
                    if (mergepair.Value.node2.valid)
                    {
                        node0 = mergepair.Value.node1;
                        node1 = mergepair.Value.node2;
                        //if (node0.MergeTreeIndex == 191 && node1.MergeTreeIndex == 180)
                        //    Console.Write("");
#if NEW_CONSTRAINT_MODEL
                        double log_treeprobability, log_likelihood_ratio_posterior;
                        logf = mergepair.Value.logf;
                        switch (mergepair.Value.m)
                        {
                            case 0:
                                log_treeprobability = constraint.GetLogJoinTreeProbabilityRatio(node0, node1) + node0.LogTreeProbability + node1.LogTreeProbability;
                                log_likelihood_ratio_posterior = constrainedRoseTree.PosterierJoinLogLikelihood(this.cacheclass, node0, node1, logf, log_treeprobability, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                                break;
                            case 1:
                                log_treeprobability = constraint.GetLogAbsorbTreeProbabilityRatio(node0, node1) + node0.LogTreeProbability + node1.LogTreeProbability;
                                log_likelihood_ratio_posterior = constrainedRoseTree.PosterierAbsorbLogLikelihood(this.cacheclass, node0, node1, logf, log_treeprobability, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                                break;
                            case 2:
                                log_treeprobability = constraint.GetLogAbsorbTreeProbabilityRatio(node1, node0) + node0.LogTreeProbability + node1.LogTreeProbability;
                                log_likelihood_ratio_posterior = constrainedRoseTree.PosterierAbsorbLogLikelihood(this.cacheclass, node1, node0, logf, log_treeprobability, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                                break;
                            default://case 3:
                                log_treeprobability = constraint.GetLogCollapseTreeProbabilityRatio(node0, node1) + node0.LogTreeProbability + node1.LogTreeProbability;
                                log_likelihood_ratio_posterior = constrainedRoseTree.PosterierCollapseLogLikelihood(this.cacheclass, node0, node1, logf, log_treeprobability, out log_likelihood_part1, out log_likelihood_part2) - (node0.log_likelihood_posterior + node1.log_likelihood_posterior);
                                break;
                        }
                        (mergepair.Key as ConstrainedCacheKey).UpdatePosteriorRatio(log_likelihood_ratio_posterior);
                        (mergepair.Value as ConstrainedCacheValue).UpdateCacheValue(log_likelihood_part1, log_likelihood_part2, log_treeprobability);
#else
                        double log_treeprobability_ratio;
                        switch (mergepair.Value.m)
                        {
                            case 0: log_treeprobability_ratio = constraint.GetLogJoinTreeProbabilityRatio(node0, node1); break;
                            case 1: log_treeprobability_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(node0, node1); break;
                            case 2: log_treeprobability_ratio = constraint.GetLogAbsorbTreeProbabilityRatio(node1, node0); break;
                            default: log_treeprobability_ratio = constraint.GetLogCollapseTreeProbabilityRatio(node0, node1); break;
                        }
                        (mergepair.Key as ConstrainedCacheKey).UpdatePosteriorRatio(mergepair.Key.log_likelihood_ratio + log_treeprobability_ratio);
                        (mergepair.Value as ConstrainedCacheValue).UpdateCacheValue(log_treeprobability_ratio);
#endif
                        if (mergepair.Key.CompareTo(bestkvp.Key) < 0)
                            bestkvp = mergepair;
                    }
                }
            }
            if (bestkvp.Value != null)
            {
                ConstrainedCacheKey ck = (bestkvp.Key as ConstrainedCacheKey);
                ConstrainedCacheValue cv = (bestkvp.Value as ConstrainedCacheValue);
                node0 = cv.node1;
                node1 = cv.node2;
                m = cv.m;
                log_likelihood_ratio = ck.log_likelihood_ratio;
                logf = cv.logf;
                cache_valuearray_plus_alpha = double.NaN;
                log_likelihood_part1 = cv.log_likelihood_part1;
                log_likelihood_part2 = cv.log_likelihood_part2;
                lastbestkvp = bestkvp;
                return ck.keyvalue;
            }
            else
            {
                node0 = null;
                node1 = null;
                m = -1;
                log_likelihood_ratio = double.MinValue;
                logf = double.MinValue;
                cache_valuearray_plus_alpha = double.MinValue;
                log_likelihood_part1 = double.MinValue;
                log_likelihood_part2 = double.MinValue;
                return double.MinValue;
            }
#else
            /// SORTED CACHE ///
            return this.cachedict.getTopOne(out node0, out node1, out m, out log_likelihood_ratio, out logf, out cache_valuearray_plus_alpha, out log_likelihood_part1, out log_likelihood_part2);
#endif
        }

        protected void UpdateLeafCount(RoseTreeNode node)
        {
            if (node.children == null)
                node.LeafCount = 1;
            else
            {
                int leafcnt = 0;
                for (int i = 0; i < node.children.Length; i++)
                    leafcnt += node.children[i].LeafCount;
                node.LeafCount = leafcnt;
            }
        }

        public void UpdateDepthInTree()
        {
            if (root == null) return;
            root.DepthInTree = 0;
            DateTime startTime = DateTime.Now;
            for (int inode = nodearray.Length - 1; inode >= 0; inode--)
            {
                RoseTreeNode rtnode = nodearray[inode];
                if (rtnode != null && rtnode.parent != null)
                    rtnode.DepthInTree = rtnode.parent.DepthInTree + 1;
                var progress = nodearray.Length - 1 - inode;
                if (inode % 100 == 0)
                    Write("Update" + progress, progress, nodearray.Length, DateTime.Now - startTime);
            }
        }
        public static void Write(string message, int progress, int total, TimeSpan timeElaspsed)
        {
            Console.Write("\r" + message + " ({0}/{1}), ETA {2}", progress, total, new TimeSpan(timeElaspsed.Ticks * (total - progress) / (Math.Max(progress, 1))).TotalMinutes);
        }
    }

    class Rules
    {
        public List<MaxRule> MaxRuleList = new List<MaxRule>();
        public List<MinRule> MinRuleList = new List<MinRule>();

        //public bool bMaxBalanceRuleAdded = false;
        public void AddMaxRule(int clusterNumber, int maxcollapsesize, int maxjoindepth)
        {
            //if (bMaxBalanceRuleAdded)
            //    Console.WriteLine("[Warning] Could not add rules because balance rules added");
            //else
                MaxRuleList.Add(new MaxRule(clusterNumber, maxcollapsesize, maxjoindepth));
        }

        public void AddMinRule(int clusterNumber, int minjoinsize, int mincollapsedepth)
        {
            MinRuleList.Add(new MinRule(clusterNumber, minjoinsize, mincollapsedepth));
        }

        //public void AddMaxBalanceRule(int clusterNumber, int maxDeltaDepth, int maxDeltaLeaves)
        //{
        //    if (!bMaxBalanceRuleAdded && MaxRuleList.Count != 0)
        //    {
        //        Console.WriteLine("[Warning] {0} maxrules cleared because max balance rules added", MaxRuleList.Count);
        //        MaxRuleList.Clear();
        //    }
        //    bMaxBalanceRuleAdded = true;
        //    MaxRuleList.Add(new MaxBalanceRule(clusterNumber, maxDeltaDepth, maxDeltaLeaves));
        //}

        public override string ToString()
        {
            string str = "<";

            foreach (MaxRule maxrule in MaxRuleList)
                str += "Max(" + maxrule.ClusterNumber + "," + maxrule.MaxCollapseSize + "," + maxrule.MaxJoinDepth + ")\t";
            foreach(MinRule minrule in MinRuleList)
                str += "Min(" + minrule.ClusterNumber + "," + minrule.MinJoinSize + "," + minrule.MinCollapseDepth + ")\t";

            str = str.TrimEnd('\t');
            str += ">";
            return str;
        }
    }

    abstract class Rule
    {
        public virtual bool PassRule(RoseTreeNode node0, RoseTreeNode node1, int m)
        {
            return true;
        }
    }

    class MaxRule : Rule
    {
        public MaxRule(int clusterNumber, int maxcollapsesize, int maxjoindepth)
        {
            this.ClusterNumber = clusterNumber;
            this.MaxCollapseSize = maxcollapsesize;
            this.MaxJoinDepth = maxjoindepth; ;
        }
        public int ClusterNumber;
        public int MaxCollapseSize;
        public int MaxJoinDepth;

        /// Join, AbsorbL, AbsorbR, Collapse
        public override bool PassRule(RoseTreeNode node0, RoseTreeNode node1, int m)
        {
            switch (m)
            {
                case 0:
                    return node0.tree_depth <= this.MaxJoinDepth &&
                        node1.tree_depth <= this.MaxJoinDepth;
                case 1:
                    return node0.LeafCount <= this.MaxCollapseSize &&
                        node1.tree_depth <= this.MaxJoinDepth;
                case 2:
                    return node0.tree_depth <= this.MaxJoinDepth &&
                        node1.LeafCount <= this.MaxCollapseSize;
                case 3:
                    return node0.LeafCount <= this.MaxCollapseSize &&
                        node1.LeafCount <= this.MaxCollapseSize;
            }

            return false;
        }

        public bool PassRule_Prev(RoseTreeNode node0, RoseTreeNode node1, int m)
        {
            switch (m)
            {
                case 0:
                    return node0.tree_depth <= this.MaxJoinDepth &&
                        node1.tree_depth <= this.MaxJoinDepth;
                case 1:
                    return node1.tree_depth <= this.MaxJoinDepth;
                case 2:
                    return node0.tree_depth <= this.MaxJoinDepth;
                case 3:
                    return node0.LeafCount + node1.LeafCount <= this.MaxCollapseSize;
            }

            return false;
        }
    }

    class MinRule : Rule
    {
        public MinRule(int clusterNumber, int minjoinsize, int mincollapsedepth)
        {
            this.ClusterNumber = clusterNumber;
            this.MinJoinSize = minjoinsize;
            this.MinCollapseDepth = mincollapsedepth; ;
        }
        public int ClusterNumber;
        public int MinJoinSize;
        public int MinCollapseDepth;

        HashSet<RoseTreeNode> SmallNodes;
        HashSet<RoseTreeNode> ShallowNodes;
        public virtual void Activate(RoseTreeNode[] nodearray)
        {
            SmallNodes = new HashSet<RoseTreeNode>();
            ShallowNodes = new HashSet<RoseTreeNode>();

            RoseTreeNode node;
            for (int i = 0; i < nodearray.Length; i++)
            {
                node=nodearray[i];
                if (node != null && node.valid)
                {
                    if (node.LeafCount <= MinJoinSize)
                        SmallNodes.Add(node);
                    if (node.tree_depth <= MinCollapseDepth)
                        ShallowNodes.Add(node);
                }
            }

            Console.WriteLine("MINRULE activated: {0} small nodes, {1} shallow nodes", SmallNodes.Count, ShallowNodes.Count);
        }

        public virtual void OnMerge(RoseTreeNode node0, RoseTreeNode node1, RoseTreeNode newnode)
        {
            SmallNodes.Remove(node0);
            ShallowNodes.Remove(node0);

            SmallNodes.Remove(node1);
            ShallowNodes.Remove(node1);

            if (newnode.LeafCount <= MinJoinSize)
                SmallNodes.Add(newnode);
            if (newnode.tree_depth <= MinCollapseDepth)
                ShallowNodes.Add(newnode);
        }

        /// Join, AbsorbL, AbsorbR, Collapse
        public override bool PassRule(RoseTreeNode node0, RoseTreeNode node1, int m)
        {
            if (SmallNodes.Count == 0 && ShallowNodes.Count == 0)
                return true;

            switch (m)
            {
                case 0:
                    return //(node0.LeafCount <= MinJoinSize && node1.LeafCount <= MinJoinSize) ||
                        PassRule(node0, true, true) || PassRule(node1, true, true);
                case 1:
                    return PassRule(node0, false, true) || PassRule(node1, true, false);
                case 2:
                    return PassRule(node0, true, false) || PassRule(node1, false, true);
                case 3:
                    return PassRule(node0, false, false) || PassRule(node1, false, false);
            }

            return false;
        }

        private bool PassRule(RoseTreeNode node, bool bBranch, bool bOtherBranch)
        {
            if (node.LeafCount <= MinJoinSize && !bOtherBranch ||
                node.tree_depth <= MinCollapseDepth && bBranch)
                return true;
            else
                return false;
        }

    }


    class MaxBalanceRule : MaxRule
    {
        public int MaxDeltaDepth, MaxDeltaLeaves;
        public MaxBalanceRule(int clusterNumber, int maxDeltaDepth, int maxDeltaLeaves)
            : base(clusterNumber, 0, 0)
        {
            this.ClusterNumber = clusterNumber;
            this.MaxDeltaDepth = maxDeltaDepth;
            this.MaxDeltaLeaves = maxDeltaLeaves;
        }

        public override bool PassRule(RoseTreeNode node0, RoseTreeNode node1, int m)
        {
            int deltabranch;
            switch (m)
            {
                case 1:  deltabranch = -1;break;
                case 2:  deltabranch = 1;break;
                default: deltabranch = 0; break;
            }
            if (Math.Abs(node0.tree_depth - node1.tree_depth + deltabranch) > MaxDeltaDepth)
                return false;
            //if (Math.Abs(node0.LeafCount - node1.LeafCount) > MaxDeltaLeaves)
            //    return false;
            return true;
        }
    }


    class EmptyRule : Rule
    {
        public EmptyRule(): base()
        {
        }

        public override bool PassRule(RoseTreeNode node0, RoseTreeNode node1, int m)
        {
            return true;
        }
    }
}
