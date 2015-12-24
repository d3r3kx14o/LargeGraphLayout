using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.Constants;

using EvolutionaryRoseTree.Constraints;
namespace EvolutionaryRoseTree.DataStructures
{
    class SubRoseTree : ConstrainedRoseTree
    {
        RoseTree MainRoseTree;
        public RoseTreeNode SubRoseTreeRoot { get; protected set; }   //node in MainRoseTree
        public static double cutLogBayesainFactor = Math.Log(1);
        //public static int mergeTogetherCutFactor = 10;
        int MergeCutThreshold;

        public SubRoseTree(ConstrainedRoseTree mainrosetree, RoseTreeNode subrosetreeroot, double alpha, double gamma, int mergeCutThreshold) :
            base(mainrosetree.dataset_index, mainrosetree.algorithm_index, mainrosetree.experiment_index,
            mainrosetree.random_projection_algorithm_index, mainrosetree.model_index, mainrosetree.projectdimension,
            mainrosetree.k, mainrosetree.lfv, mainrosetree.alpha, mainrosetree.gamma, mainrosetree.tau,
            mainrosetree.kappa, mainrosetree.R_0, mainrosetree.outputpath, mainrosetree.sizePunishMinRatio, mainrosetree.sizePunishMaxRatio)
        {
            if (subrosetreeroot.tree_depth != 2)
                throw new Exception("[SubRoseTree] Error building sub rose tree!");
            if (model_index != Constant.DCM)
                throw new Exception("Current SubRoseTree support only DCM!");

            //substitute: lfv, alpha, gamma, outputpath
            MainRoseTree = mainrosetree;
            SubRoseTreeRoot = subrosetreeroot;
            MergeCutThreshold = mergeCutThreshold;

            if (algorithm_index == Constant.SPILLTREE_BRT)
                this.projection = mainrosetree.projection;

            this.alpha = alpha;
            this.gamma = gamma;
        }

        public override void Run(
            Constraint constraint,
            int interval,               //Constant.intervals[0]:30
            out int depth,
            out double log_likelihood)
        {
            //Do not clear invalid nodes (by scan) because invalid node will be removed immediately
            interval = int.MaxValue;

            if (constraint is TreeOrderConstraint)
                (constraint as TreeOrderConstraint).SetConstrainedRoseTree(this);

            if (constraint != null)
                this.constraint = constraint;
            else
                this.constraint = new NoConstraint();

            //if (constraint is TreeOrderConstraint)
            //{
            //    (constraint as TreeOrderConstraint).LoseOrderPunishWeight = 0;
            //    (constraint as TreeOrderConstraint).IncreaseOrderPunishWeight = 0;
            //    Console.WriteLine("Sub RoseTree constraint always 0!");
            //}
            //this.constraint = new NoConstraint();
            //Console.WriteLine("Sub RoseTree always No Constraint!");

            CacheSubTreeCacheClass();              //Cache log values as dictionaries

            InitializeSubTree();                   //Initialize (random projection,) nodes, (spill tree)

            mergedtreepointer = nodearray.Length / 2;

            CacheNearestNeighborsForAll();
            MergeLoop(interval);

            FindRoot();
            
            //LabelTreeIndices(out depth);

            if (CacheValueRecord != null) CacheValueRecord.Close();

            //if (constraint is TreeOrderConstraint)
            //{
            //    ConstraintTree ctree = (constraint as TreeOrderConstraint).GetConstraintTree();
            //    AdjustSubTreeChildrenOrder(ctree);
            //    //ctree.UpdateLeafNumbers();
            //    //ctree.PostProcessContainedInformation();
            //}
            this.spilltree = null;

            RecalculateTreeCachedData();
            depth = this.root.tree_depth;
            log_likelihood = this.root.log_likelihood;

            //Console.WriteLine(this.StructureToString());
        }

        private void CacheSubTreeCacheClass()
        {
            this.cacheclass = new CacheClass(this.alpha, this.gamma, this.lfv.maxdimensionvalue, 
                this.lfv.wordnum, SubRoseTreeRoot.children.Length, this.lfv.lexiconsize);
            this.cacheclass.Cache();
        }

        private void InitializeSubTree()
        {
            InitializeNodesSubTree();

            if (this.algorithm_index == Constant.SPILLTREE_BRT)
                InitializeSpillTree();
        }

        private void InitializeNodesSubTree()
        {
            this.clusternum = this.lfv.featurevectors.Length;
            this.initial_clusternum = clusternum;

            this.nodearray = new RoseTreeNode[2 * this.clusternum];

            foreach (RoseTreeNode leafnode in SubRoseTreeRoot.children)
            {
                nodearray[leafnode.indices.array_index] = leafnode;
                leafnode.valid = true;
                leafnode.CacheMergePairs = new Dictionary<CacheKey,CacheValue>();
                leafnode.spilltree_positions.Clear();
            }

            this.nodecounter = clusternum;
            this.clusternum = SubRoseTreeRoot.children.Length;

            if (this.k > clusternum) k = clusternum;
        }

        #region mergeloop
        public override void MergeLoop(int interval)
        {
            StreamWriter sw = InitializeMergeRecordWriter();

            while (clusternum > 1)
            {
                #region previous
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
                #endregion previous

                double logBayesainFactor = newnode.cache_nodevalues.log_likelihood_part1 
                    - newnode.cache_nodevalues.log_likelihood_part2;
                if (logBayesainFactor < cutLogBayesainFactor)
                {
                    if (BuildCutRoseTree_Prev())
                        break;
                }
            }

            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }

        private bool BuildCutRoseTree_Prev()
        {
            //int mergecut = SubRoseTreeRoot.children.Length / mergeTogetherCutFactor;
            //if (mergecut < 2) mergecut = 2;
            int mergecut = MergeCutThreshold;

            //Find valid clusters
            List<RoseTreeNode> validnodes = new List<RoseTreeNode>();
            List<RoseTreeNode> mergelist = new List<RoseTreeNode>();
            List<RoseTreeNode> uniqueclusters = new List<RoseTreeNode>();
            foreach (RoseTreeNode rtnode in nodearray)
            {
                if (rtnode != null && rtnode.valid)
                {
                    validnodes.Add(rtnode);
                    if (rtnode.LeafCount < mergecut)
                        mergelist.Add(rtnode);
                    else
                    {
                        uniqueclusters.Add(rtnode);
                        if (rtnode.tree_depth > 2) CollapseSubtreeStructure(rtnode);
                    }
                }
            }

            //if (uniqueclusters.Count == 0)
            //{
            //    cutLogBayesainFactor /= 10;
            //    return false;
            //}

            RoseTreeNode mergedcluster = GetMergedCluster_Prev(mergelist);
            if (mergedcluster != null) uniqueclusters.Add(mergedcluster);
            if (uniqueclusters.Count > 1)
            {
                GenerateRoot(uniqueclusters);
                foreach (RoseTreeNode child in root.children)
                    if (child.children != null)
                        child.LeafCount = child.children.Length;
            }
            //if (uniqueclusters.Count > 1)
            //    GetMergedCluster(uniqueclusters);

            return true;
        }

        private void BuildCutRoseTree()
        {
            //int mergecut = SubRoseTreeRoot.children.Length / mergeTogetherCutFactor;
            //if (mergecut < 2) mergecut = 2;
            int mergecut = MergeCutThreshold;

            //Find valid clusters
            List<RoseTreeNode> validnodes = new List<RoseTreeNode>();
            List<RoseTreeNode> mergelist = new List<RoseTreeNode>();
            List<RoseTreeNode> uniqueclusters = new List<RoseTreeNode>();
            foreach (RoseTreeNode rtnode in nodearray)
            {
                if (rtnode != null && rtnode.valid)
                {
                    validnodes.Add(rtnode);
                    if (rtnode.LeafCount < mergecut)
                        mergelist.Add(rtnode);
                    else
                    {
                        uniqueclusters.Add(rtnode);
                        //if (rtnode.tree_depth > 2) CollapseSubtreeStructure(rtnode);
                    }
                }
            }

            RoseTreeNode mergedcluster = GetMergedCluster(mergelist);
            if (mergedcluster != null) uniqueclusters.Add(mergedcluster);
            //if (uniqueclusters.Count > 1)
            //    GenerateRoot(uniqueclusters);
            //else
            //    GenerateRoot(uniqueclusters[0].children.ToList<RoseTreeNode>());
            if (uniqueclusters.Count > 1)
                GetMergedCluster(uniqueclusters);
        }

        private void GenerateRoot(List<RoseTreeNode> uniqueclusters)
        {
            int rootindex = nodearray.Length - 2;
            //try
            {
                root = new RoseTreeNode(uniqueclusters.ToArray<RoseTreeNode>(),
                    null, null, rootindex);
            }
            //catch(Exception e) 
            //{
            //    Console.WriteLine(e.Message);
            //}
            root.MergeTreeIndex = rootindex;
            nodearray[rootindex] = root;
            root.tree_depth = 3;

            for (int i = 0; i < rootindex; i++)
            {
                if (nodearray[i] != null)
                    nodearray[i].valid = false;
            }
        }

        private RoseTreeNode GetMergedCluster(List<RoseTreeNode> mergelist)
        {
            if (mergelist.Count == 0)
                return null;

            RoseTreeNode mergednode = mergelist[0];
            for (int inode = 1; inode < mergelist.Count; inode++)
            {
                RoseTreeNode mergednode2 = mergelist[inode];
                mergednode = ManuallyMergeTwoNodes(mergednode, mergednode2);
            }

            return mergednode;
        }

        RoseTreeNode ManuallyMergeTwoNodes(RoseTreeNode mergednode, RoseTreeNode mergednode2)
        {
            double cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2, log_likelihood;

            int m = (mergednode.tree_depth > 1 ? 1 : 0) + (mergednode2.tree_depth > 1 ? 2 : 0);

            double logf = GetLogF(mergednode, mergednode2, out cache_valuearray_plus_alpha);
            switch (m)
            {
                case 0: log_likelihood = mergednode.JoinLogLikelihood(cacheclass, mergednode, mergednode2, logf, out log_likelihood_part1, out log_likelihood_part2); break;
                case 1: log_likelihood = mergednode.AbsorbLogLikelihood(cacheclass, mergednode, mergednode2, logf, out log_likelihood_part1, out log_likelihood_part2); break;
                case 2: log_likelihood = mergednode.AbsorbLogLikelihood(cacheclass, mergednode2, mergednode, logf, out log_likelihood_part1, out log_likelihood_part2); break;
                default: log_likelihood = mergednode.CollapseLogLikelihood(cacheclass, mergednode, mergednode2, logf, out log_likelihood_part1, out log_likelihood_part2); break;
            }

            mergednode = MergeSingleStep(mergednode, mergednode2, m, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);

            return mergednode;
        }

        private RoseTreeNode GetMergedCluster_Prev(List<RoseTreeNode> mergelist)
        {
            if (mergelist.Count == 0)
                return null;
            if (mergelist.Count == 1)
            {
                if (mergelist[0].tree_depth > 2)
                    CollapseSubtreeStructure(mergelist[0]);
                return mergelist[0];
            }
            int mergeclusterindex = nodearray.Length - 3;
            List<int> overlapping_keylist;
            int length;
            //calculate data
            SparseVectorList data = mergelist[0].data;
            bool bCollapseStructure = false;
            for (int index = 1; index < mergelist.Count; index++)
            {
                RoseTreeNode mergenode = mergelist[index];
                data = data.AddValue(data, mergenode.data, out overlapping_keylist, out length);
                if (mergenode.tree_depth > 1) bCollapseStructure = true;
            }

            RoseTreeNode mergednode = new RoseTreeNode(mergelist.ToArray<RoseTreeNode>(),
                data, null, mergeclusterindex);
            mergednode.MergeTreeIndex = mergeclusterindex;
            nodearray[mergeclusterindex] = mergednode;

            if (bCollapseStructure) CollapseSubtreeStructure(mergednode);
            return mergednode;
        }

        void CollapseSubtreeStructure(RoseTreeNode rtnode)
        {
            List<RoseTreeNode> leaves = GetSubTreeLeaf(rtnode);
            rtnode.children = leaves.ToArray<RoseTreeNode>();
            foreach (RoseTreeNode leaf in leaves)
                leaf.parent = rtnode;
            rtnode.tree_depth = 2;
        }
        #endregion merge loop

        #region Adjust rose tree children order
        public void AdjustSubTreeChildrenOrder(ConstraintTree constriantTree)
        {
            List<RoseTreeNode> rtqueue = new List<RoseTreeNode>();
            List<ConstraintTreeNode> ctqueue = new List<ConstraintTreeNode>();
            rtqueue.Add(root);
            ctqueue.Add(constriantTree.GetNodeByMergeIndex(root.MergeTreeIndex));//Sometimes not the root
            while (rtqueue.Count != 0)
            {
                RoseTreeNode rtnode = rtqueue[0];
                ConstraintTreeNode ctnode = ctqueue[0];
                rtqueue.RemoveAt(0);
                ctqueue.RemoveAt(0);

                if (rtnode.children == null)
                    continue;
                List<int> rtchildindices = new List<int>();
                foreach (RoseTreeNode child_rtnode in rtnode.children)
                    rtchildindices.Add(child_rtnode.MergeTreeIndex);

                List<int> orderedMergeTreeIndices = new List<int>();
                List<ConstraintTreeNode> corr_ctnodelist = new List<ConstraintTreeNode>();
                foreach (ConstraintTreeNode child_ctnode in ctnode.Children)
                {
                    if (!rtchildindices.Contains(child_ctnode.InitialIndex))
                        continue;
                    corr_ctnodelist.Add(child_ctnode);
                    orderedMergeTreeIndices.Add(child_ctnode.InitialIndex);
                }
                rtnode.AdjustChildrenOrder(orderedMergeTreeIndices);

                rtqueue.AddRange(rtnode.children);
                ctqueue.AddRange(corr_ctnodelist);
            }
        }

        #endregion

        internal double GetClusteringScore()
        {
            double sum = 0, squaresum = 0, cnt = 0;
            foreach(RoseTreeNode child in root.children)
                if (child.LeafCount >= MergeCutThreshold)
                {
                    sum += child.LeafCount;
                    squaresum += child.LeafCount * child.LeafCount;
                    cnt++;
                }
            //return root.log_likelihood;
            return -(squaresum / cnt / cnt - sum * sum / cnt / cnt);
            //return root.log_likelihood - (squaresum / cnt / cnt - sum * sum / cnt / cnt);
        }
    }
}
