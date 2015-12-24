using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;
using System.IO;

namespace EvolutionaryRoseTree.Constraints
{
    class LooseTreeOrderConstraint : TreeOrderConstraint
    {
        public static double LooseOrderDeltaRatio = 0.4;

        public LooseTreeOrderConstraint(RoseTree rosetree, LoadFeatureVectors lfv,
            double loseorderpunishweight, double increaseorderpunishweight, DataProjectionRelation projRelation = null) :
            base(rosetree, lfv, loseorderpunishweight, increaseorderpunishweight, projRelation, ConstraintType.LooseTreeOrder)
        {
        }

        public LooseTreeOrderConstraint(RoseTree rosetree, LoadFeatureVectors lfv,
            double loseorderpunishweight, double increaseorderpunishweight, double affleavePunishWeight, DataProjectionRelation projRelation = null) :
            base(rosetree, lfv, loseorderpunishweight, increaseorderpunishweight, affleavePunishWeight, projRelation, ConstraintType.LooseTreeOrder)
        {
        }

        protected override void BuildConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv, DataProjectionRelation projRelation)
        {
            if (projRelation == null)
                this.ConstraintTree = new LooseConstraintTree(rosetree, lfv);
            else
                this.ConstraintTree = new SucceedRelationLooseConstraintTree(rosetree, lfv, projRelation);
        }
    }

    class LooseConstraintTree : ConstraintTree
    {
        public LooseConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv,
            DataProjectionRelation projectionRelation = null) :
            base(rosetree, lfv, projectionRelation)
        {
        }

        static bool bSplitFreeEnabled = true;
        protected override bool SplitFree(ConstraintTreeNode node, int mergetreeindex)
        {
            return bSplitFreeEnabled && 
                (node as LooseConstraintTreeNode).SplitFreeMergedChildren.Contains(mergetreeindex);
        }

        protected override void GetMergeCost(RoseTreeNode rtnode0, RoseTreeNode rtnode1,
            bool addbranch0, bool addbranch1,
            out double order2unorder, out double unorder2order)
        {
            //bSplitFreeEnabled = false;
            //GetMergeBrokenOrderNumbers(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);
            //bSplitFreeEnabled = true;

            o2uViolation = 0; u2oViolation = 0; fu2oViolation = 0;

#if NORMALIZE_PROJ_WEIGHT
            this.GetMergeBrokenOrderNumbersNoWeight(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);
            //TestAlgorithmCorrectness(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);
#else
            this.GetMergeBrokenOrderNumbers(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);
#endif
            order2unorder = o2uViolation;
            unorder2order = u2oViolation + fu2oViolation;
            if (BuildRoseTree.ViolationCurveFile != null)
                BuildRoseTree.ViolationCurveFile.Write("{0}\t{1}\t{2}\t", o2uViolation, u2oViolation, fu2oViolation);

        }

        #region inherit
        protected override ConstraintTreeNode NewConstraintNode()
        {
            return new LooseConstraintTreeNode();
        }

        protected override ConstraintTreeNode CreateFreeConstraintNode()
        {
            return new LooseConstraintTreeNode(true);
        }

        protected override ConstraintTree GetSucceedRelationConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv, DataProjectionRelation projRelation)
        {
            return new SucceedRelationLooseConstraintTree(rosetree, this.lfv, projRelation);
        }
        #endregion

        #region previous code
        //protected override ConstraintTreeNode MergeToAncestor(int mergetreeindex,
        //    ConstraintTreeNode node, ConstraintTreeNode ancestor,
        //    bool addbranch, List<int> affectedRoseTreeNodeIndices)
        //{
        //    node.SetActiveMergedTree(mergetreeindex);
        //    ConstraintTreeNode container = ancestor;

        //    if (addbranch)
        //    {
        //        if (node.Children != null &&
        //            node.Children.Count != node.ActiveMergedTree.MergedChildren.Count)
        //            container = node.Split(inheritParentInfo);
        //        else
        //        {
        //            container = node;
        //            node = node.Parent;
        //        }
        //    }

        //    ConstraintTreeNode collapsednode = null;
        //    while (node != ancestor)
        //    {
        //        collapsednode = node;
        //        node.Parent.CollapseLinkWithChild(node);
        //        node = node.Parent;
        //    }

        //    if (collapsednode != null && collapsednode.MergedChildren != null)
        //        foreach (int mergeindex in collapsednode.MergedChildren.Keys)
        //            MergedTrees[mergeindex] = ancestor;

        //    UpdateAffectedRoseTreeNodeIndices(collapsednode, affectedRoseTreeNodeIndices);

        //    return container;
        //}

        //protected override void BuildUpConstraintTree()
        //{
        //    //build structure
        //    if (rosetree.root.children == null)
        //    {
        //        Root = OriginalConstraintTreeNodes[0];
        //        return;
        //    }

        //    //width-first traversal
        //    List<RoseTreeNode> rtnodelist = new List<RoseTreeNode>();
        //    rtnodelist.Add(rosetree.root);
        //    List<ConstraintTreeNode> cnodelist = new List<ConstraintTreeNode>();
        //    this.Root = new LooseConstraintTreeNode();
        //    cnodelist.Add(this.Root);
        //    OriginalConstraintTreeNodes[rosetree.root.indices.array_index] = this.Root;
        //    while (rtnodelist.Count != 0)
        //    {
        //        RoseTreeNode rosetreenode = rtnodelist[0];
        //        ConstraintTreeNode ctreenode = cnodelist[0];
        //        if (rosetreenode.tree_depth > 2)
        //        {
        //            RoseTreeNode[] children = rosetreenode.children;
        //            for (int i = 0; i < children.Length; i++)
        //            {
        //                RoseTreeNode rtchild = children[i];
        //                if (rtchild.tree_depth == 1)
        //                {
        //                    ConstraintTreeNode childctreenode = ctreenode.CreateChild();
        //                    OriginalConstraintTreeNodes[rtchild.indices.array_index] = childctreenode;
        //                }
        //                else
        //                {
        //                    rtnodelist.Add(rtchild);
        //                    ConstraintTreeNode childctreenode = ctreenode.CreateChild();
        //                    cnodelist.Add(childctreenode);
        //                    //Added for different data projection types
        //                    OriginalConstraintTreeNodes[rtchild.indices.array_index] = childctreenode;
        //                }
        //            }
        //        }
        //        else if (rosetreenode.tree_depth == 2)
        //        {
        //            RoseTreeNode[] children = rosetreenode.children;
        //            for (int i = 0; i < children.Length; i++)
        //            {
        //                ConstraintTreeNode childctreenode = ctreenode.CreateChild();
        //                OriginalConstraintTreeNodes[children[i].indices.array_index] = childctreenode;
        //            }
        //        }
        //        else
        //            Console.WriteLine("Error");

        //        ctreenode.NearestNeighbourArrayIndex = rosetreenode.MergeTreeIndex;
        //        //if (rosetreenode.tree_depth != 1)
        //        //    ctreenode.InitializeCorrespondingInformation(rosetreenode.MergeTreeIndex);
        //        rtnodelist.RemoveAt(0);
        //        cnodelist.RemoveAt(0);
        //    }
        //}


        //public override void MergeTree(RoseTreeNode rtnode0, RoseTreeNode rtnode1, bool addbranch0, bool addbranch1)
        //{
        //    LooseConstraintTreeNode.mergetreepointer = this.mergedtreepointer;

        //    ConstraintTreeNode node0 = MergedTrees[rtnode0.MergeTreeIndex];
        //    ConstraintTreeNode node1 = MergedTrees[rtnode1.MergeTreeIndex];

        //    // node not in the constraint tree
        //    if (node0.IsFreeNode && node1.IsFreeNode)
        //    {
        //        ConstraintTreeNode newnode = CreateFreeConstraintNode();
        //        newnode.AddFreeChildren(node0, node1, addbranch0, addbranch1);
        //        newnode.InitialIndex = mergedtreepointer;
        //        MergedTrees[mergedtreepointer] = newnode;
        //    }
        //    else
        //    {
        //        if (node0.IsFreeNode)
        //            node0 = AttachFreeNode(node0, node1);
        //        else if (node1.IsFreeNode)
        //            node1 = AttachFreeNode(node1, node0);

        //        List<int> affectedRoseTreeNodeIndices = new List<int>();
        //        //adjust tree structure
        //        ConstraintTreeNode commonancestor = GetCommonAncestor(node0, node1);

        //        ConstraintTreeNode container0 =
        //            MergeToAncestor(rtnode0.MergeTreeIndex, node0, commonancestor, addbranch0, affectedRoseTreeNodeIndices);
        //        ConstraintTreeNode container1 =
        //            MergeToAncestor(rtnode1.MergeTreeIndex, node1, commonancestor, addbranch1, affectedRoseTreeNodeIndices);

        //        //UpdateMergedTreePosition(commonancestor);
        //        (commonancestor as LooseConstraintTreeNode).MergeTree(mergedtreepointer,
        //            rtnode0.MergeTreeIndex, node0, container0,
        //            rtnode1.MergeTreeIndex, node1, container1);
        //        commonancestor.InitialIndex = mergedtreepointer;
        //        MergedTrees[mergedtreepointer] = commonancestor;

        //        //update cached values
        //        affectedRoseTreeNodeIndices.Remove(rtnode0.MergeTreeIndex);
        //        affectedRoseTreeNodeIndices.Remove(rtnode1.MergeTreeIndex);
        //        foreach (int affectnodeindex in affectedRoseTreeNodeIndices)
        //            constrainedRoseTree.UpdateCacheValues(affectnodeindex);
        //    }

        //    mergedtreepointer++;
        //}
        #endregion
    }

    class LooseConstraintTreeNode : ConstraintTreeNode
    {
        public HashSet<int> SplitFreeMergedChildren = new HashSet<int>();
        public List<ConstraintTreeNode> SplitFreeCandidates; //node and its group label (merge index)

        //public static int mergetreepointer;

        public LooseConstraintTreeNode(bool bFreeNode = false)
            : base(bFreeNode)
        {
        }

        public LooseConstraintTreeNode(ConstraintTreeNode parent)
            : base(parent)
        {
        }


        public LooseConstraintTreeNode(ConstraintTreeNode parent, MergedTree splitMergedTree)
            : base(parent, splitMergedTree)
        {
        }

        public override ConstraintTreeNode CollapseLinkWithChild(ConstraintTreeNode child)
        {
            //Console.WriteLine("Collapse Link: Parent {0}, Child {1}", this.OriginalLinkedNodeIndex, child.OriginalLinkedNodeIndex);
            if (this.SplitFreeCandidates == null)
                this.SplitFreeCandidates = new List<ConstraintTreeNode>();
            this.SplitFreeCandidates.AddRange(child.Children);
            if (child.MergedChildren != null)
                foreach (int mergetreeindex in child.MergedChildren.Keys)
                    this.SplitFreeMergedChildren.Add(mergetreeindex);
            //foreach (int mergetreeindex in (child as LooseConstraintTreeNode).SplitFreeMergedChildren)
            //    this.SplitFreeMergedChildren.Add(mergetreeindex);

            return base.CollapseLinkWithChild(child);
        }

        public override ConstraintTreeNode Split(InheritParentInfo inheritParentInfos, bool bPutInEnd = false)
        {
            //Console.WriteLine("Split: Parent {0}, Child Merge Tree index: [{1}]", this.OriginalLinkedNodeIndex, ActiveMergedTree.MergeTreeIndex);
            ConstraintTreeNode splitNewNode = base.Split(inheritParentInfos, bPutInEnd);

            if (SplitFreeMergedChildren != null &&
                SplitFreeMergedChildren.Contains(ActiveMergedTree.MergeTreeIndex))
            {
                SplitFreeMergedChildren.Remove(ActiveMergedTree.MergeTreeIndex);
                this.SplitFreeCandidates.Add(splitNewNode);
            }

            return splitNewNode;
        }

        public override void MergeTree(int mergetreeindex,
            int mergetreeindex0, ConstraintTreeNode node0, ConstraintTreeNode container0,
            int mergetreeindex1, ConstraintTreeNode node1, ConstraintTreeNode container1)
        {
            MergedTree mergetree0 = (MergedChildren != null && MergedChildren.ContainsKey(mergetreeindex0)) ? MergedChildren[mergetreeindex0] : null;
            MergedTree mergetree1 = (MergedChildren != null && MergedChildren.ContainsKey(mergetreeindex1)) ? MergedChildren[mergetreeindex1] : null;
  
            base.MergeTree(mergetreeindex, mergetreeindex0, null, container0, mergetreeindex1, null, container1);
            
            SplitFreeUpdateOnTreeMerge(mergetreeindex,
                    mergetreeindex0, node0, container0, mergetree0,
                    mergetreeindex1, node1, container1, mergetree1);
        }

        private void SplitFreeUpdateOnTreeMerge(int mergetreeindex,
    int mergetreeindex0, ConstraintTreeNode node0, ConstraintTreeNode container0, MergedTree mergetree0,
    int mergetreeindex1, ConstraintTreeNode node1, ConstraintTreeNode container1, MergedTree mergetree1)
        {
            bool bLoose0 = true, bLoose1 = true;
            double looseDocument0 = 0, looseDocument1 = 0;

            //remove children of mergedtree from splitfreecandidates
            foreach (int mergeindex in SplitFreeMergedChildren)
            {
                if (mergeindex == mergetreeindex0 || mergeindex == mergetreeindex1)
                    continue;
                MergedTree mergetree = MergedChildren[mergeindex];
                if (SplitFreeCandidates.Contains(mergetree.MergedChildren[0]))
                {
                    foreach (ConstraintTreeNode node in mergetree.MergedChildren)
                        SplitFreeCandidates.Remove(node);
                }
            }

            if (this.SplitFreeCandidates == null)
                return;

            if (this.SplitFreeMergedChildren.Contains(mergetreeindex0))
            {
                SplitFreeMergedChildren.Remove(mergetreeindex0);
            }
            else if (this.SplitFreeCandidates.Contains(container0))
            {
                SplitFreeCandidates.Remove(container0);
            }
            else
                bLoose0 = false;
            looseDocument0 = mergetree0 == null ? container0.LeafNumber : mergetree0.LeafNumber;
            if (!bLoose0) looseDocument0 *= -1;

            if (this.SplitFreeMergedChildren.Contains(mergetreeindex1))
            {
                SplitFreeMergedChildren.Remove(mergetreeindex1);
            }
            else if (this.SplitFreeCandidates.Contains(container1))
            {
                SplitFreeCandidates.Remove(container1);
            }
            else
                bLoose1 = false;
            looseDocument1 = mergetree1 == null? container1.LeafNumber : mergetree1.LeafNumber;
            if (!bLoose1) looseDocument1 *= -1;

            //if (Math.Abs(looseDocument0) + Math.Abs(looseDocument1) != MergedChildren[mergetreeindex].LeafNumber)
            //    Console.Write("");

            if ((looseDocument0 + looseDocument1)
                / (Math.Abs(looseDocument0) + Math.Abs(looseDocument1)) > LooseTreeOrderConstraint.LooseOrderDeltaRatio)
            {
                this.SplitFreeMergedChildren.Add(mergetreeindex);
                //Console.WriteLine("{0} set merged", mergetreeindex);
            }
        }


        private void SplitFreeUpdateOnTreeMerge_previous(int mergetreeindex,
            int mergetreeindex0, ConstraintTreeNode node0, ConstraintTreeNode container0, MergedTree mergetree0,
            int mergetreeindex1, ConstraintTreeNode node1, ConstraintTreeNode container1, MergedTree mergetree1)
        {
            bool bLoose0 = true, bLoose1 = true;

            //remove children of mergedtree from splitfreecandidates
            foreach (int mergeindex in SplitFreeMergedChildren)
            {
                if (mergeindex == mergetreeindex0 || mergeindex == mergetreeindex1)
                    continue;
                MergedTree mergetree = MergedChildren[mergeindex];
                if (SplitFreeCandidates.Contains(mergetree.MergedChildren[0]))
                {
                    foreach (ConstraintTreeNode node in mergetree.MergedChildren)
                        SplitFreeCandidates.Remove(node);
                }
            }

            if (this.SplitFreeCandidates == null)
                return;

            if (this.SplitFreeMergedChildren.Contains(mergetreeindex0))
            {
                SplitFreeMergedChildren.Remove(mergetreeindex0);
            }
            else if (this.SplitFreeCandidates.Contains(container0))
            {
                SplitFreeCandidates.Remove(container0);
            }
            else
                bLoose0 = false;

            if (this.SplitFreeMergedChildren.Contains(mergetreeindex1))
            {
                SplitFreeMergedChildren.Remove(mergetreeindex1);
            }
            else if (this.SplitFreeCandidates.Contains(container1))
            {
                SplitFreeCandidates.Remove(container1);
            }
            else
                bLoose1 = false;

            if (bLoose0 && bLoose1)
            {
                this.SplitFreeMergedChildren.Add(mergetreeindex);
                //Console.WriteLine("{0} set merged", mergetreeindex);
            }
        }



        #region inherit
        protected override ConstraintTreeNode NewConstraintNode()
        {
            return new LooseConstraintTreeNode();
        }

        protected override ConstraintTreeNode NewConstraintNode(ConstraintTreeNode parent)
        {
            return new LooseConstraintTreeNode(parent);
        }

        protected override ConstraintTreeNode NewConstraintNode(ConstraintTreeNode parent, MergedTree splitMergedTree)
        {
            return new LooseConstraintTreeNode(parent, splitMergedTree);
        }
        #endregion
    }
}
