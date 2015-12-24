using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;

using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Constants;
namespace EvolutionaryRoseTree.Constraints
{

    //The tree is a constraint, this is a rose tree
    class ConstraintTree
    {
        public static bool WeightedLeafNode = true;
        public static double NewTopicRelaxRatio = 1;
        public static bool bExpandToBinaryConstraintTree = false;
        public int NotFreeConstraintTreeLeafCount = 0;
        public double MultipleProjectionFactor = 0.5;

        public ConstraintTreeNode Root;

        //each node corresponds to one leaf in rosetree
        //seek constraintnode by previous rosetree's array index
        protected ConstraintTreeNode[] OriginalConstraintTreeNodes;
        protected RoseTree rosetree;
        protected IList<RoseTreeNode> rosetreeleaves;
        protected LoadFeatureVectors lfv;
        public int mergedtreepointer;  //point to the end of valid merged trees, increase only
        bool bDrawLeafNumber;

        protected ConstraintTreeNode[] MergedTrees;   //arranged by intialindex
        protected DataProjection dataprojection;

        protected InheritParentInfo inheritParentInfo;
        List<ConstraintTreeNode> freedocuments = new List<ConstraintTreeNode>();
        public int FreeDocumentsNumber { get { return freedocuments.Count; } }

        protected ConstrainedRoseTree constrainedRoseTree;
        protected DataProjectionRelation projectRelation;

        public ConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv, DataProjectionRelation projectionRelation = null)
        {
            this.rosetree = rosetree;
            this.lfv = lfv;
            this.IsInitiallyOpened = rosetree is ConstrainedRoseTree ? (rosetree as ConstrainedRoseTree).IsTreeOpened : false;
            this.projectRelation = projectionRelation;

            Initialize();

        }

        #region remove conflicts
        public void SetOthersAsFreeNodes(int[] nodeIDs)
        {
            var hashIDs = new HashSet<int>();
            foreach (var nodeID in nodeIDs)
                hashIDs.Add(nodeID);
            for (int i = 0; i < lfv.featurevectors.Length; i++)
            {
                if (!hashIDs.Contains(i))
                {
                    var node = MergedTrees[i];
                    if (node.IsFreeNode)
                        continue;
                    //remove as constraint tree
                    RemoveNodeFromConstraintTree(node);
                    //Set the node as free node
                    node.SetAsFreeNode();
                    freedocuments.Add(node);
                }
            }

            UpdateLeafNumbers();
            InitializeInheritInfo();
        }

        public int[] GetCommonStructures()
        {
            var commonList = new List<int>();
            for (int i = 0; i < MergedTrees.Length / 2; i++)
            {
                if (!MergedTrees[i].IsFreeNode)
                    commonList.Add(i);
            }
            return commonList.ToArray<int>();
        }

        Dictionary<ConstraintTreeNode, SparseVectorList> featureVectorDict = null;
        public Dictionary<ConstraintTreeNode, SparseVectorList> GetFeatureVectorDict()
        {
            if (featureVectorDict == null)
            {
                featureVectorDict = new Dictionary<ConstraintTreeNode, SparseVectorList>();

                var constraintTreeNodes = GetAllValidTreeNodes();
                foreach (var constraintTreeNode in constraintTreeNodes.Reverse<ConstraintTreeNode>())
                {
                    SparseVectorList vector = null;
                    if (constraintTreeNode.Children == null || constraintTreeNode.Children.Count == 0)
                    {
                        vector = lfv.featurevectors[constraintTreeNode.InitialIndex];
                    }
                    else
                    {
                        var children = constraintTreeNode.Children;
                        vector = Add(featureVectorDict[children[0]], featureVectorDict[children[1]]);
                        for (int i = 2; i < children.Count; i++)
                        {
                            vector = Add(vector, featureVectorDict[children[i]]);      
                        }
                    }
                    featureVectorDict.Add(constraintTreeNode, vector);
                }
            }
            return featureVectorDict;
        }

        SparseVectorList Add(SparseVectorList vector1, SparseVectorList vector2)
        {
            List<int> overlapping_keylist;
            SparseVectorList vector = new SparseVectorList(vector1.model_index);
            int new_vector_length;

            vector = vector.Add(false, vector1.model_index, vector1, vector2, out overlapping_keylist, out new_vector_length);
            if (vector.keyarray.Length != new_vector_length)
                vector.Resize(new_vector_length);

            vector.valuearray_sum = vector1.valuearray_sum + vector2.valuearray_sum;

            if (vector.model_index == RoseTreeTaxonomy.Constants.Constant.VMF)
                vector.GetNormvMF();
            else
                vector.GetNormDCM();

            //Console.WriteLine("---------Vector1---------");
            //Console.WriteLine(vector1);
            //Console.WriteLine("---------Vector2---------");
            //Console.WriteLine(vector2);
            //Console.WriteLine("---------Added---------");
            //Console.WriteLine(vector);

            return vector;
        }

        internal List<ConstraintTreeNode> GetAllValidTreeNodes()
        {
            if (Root == null)
                return new List<ConstraintTreeNode>();

            var constraintTreeNodes = new List<ConstraintTreeNode>();

            var queue = new List<ConstraintTreeNode>();
            queue.Add(Root);
            int index=0;
            while(index<queue.Count)
            {
                var node = queue[index];
                if(node.Children!=null && node.Children.Count>0)
                    queue.AddRange(node.Children);
                index++;
            }

            queue.AddRange(this.freedocuments);

            return queue;
        }
        #endregion

        bool bDisableUpdate = false;
        public void DisableUpdate()
        {
            bDisableUpdate = true;
        }

        public void SetConstrainedRoseTree(ConstrainedRoseTree constrainedRoseTree)
        {
            this.constrainedRoseTree = constrainedRoseTree;
        }

        MultipleConstraints multiconstraint = null;
        public int iConstraint { get; protected set; }
        public void SetParentMultipleConstraint(MultipleConstraints multiconstraint, int iConstraint)
        {
            this.multiconstraint = multiconstraint;
            this.iConstraint = iConstraint;
        }


        #region return probability: broken order numbers
        public static double AffectedLeafNumber;
        public static double bayesFactor = double.MaxValue;
        public static double bayesFactor_th = Math.Log(0.1);
        public void GetMergeBrokenOrderNumbers(RoseTreeNode rtnode0, RoseTreeNode rtnode1,
            bool addbranch0, bool addbranch1,
            out double order2unorder, out double unorder2order)
        {
            //try
            {
                ConstraintTreeNode node0 = MergedTrees[rtnode0.MergeTreeIndex];
                ConstraintTreeNode node1 = MergedTrees[rtnode1.MergeTreeIndex];
                // node not in the constraint tree
                if (node0.IsFreeNode || node1.IsFreeNode)
                {
                    order2unorder = unorder2order = 0;
#if !NEW_YORK_TIMES_TEST_SMOOTHNESS
                    if (!node0.IsFreeNode)
                    {
                        TrySplitCost(rtnode0.MergeTreeIndex, node0, node0, addbranch0, out order2unorder, out unorder2order);
                        if (rtnode1.tree_depth > 3)
                        {
                            order2unorder = 1e10;
                            if (addbranch0 || addbranch1)
                                order2unorder = 1e12;
                        }
                        else if ((rtnode0.LeafCount < 10 || bayesFactor < bayesFactor_th))// ||&& rtnode1.LeafCount > 5 rtnode0.data.Cosine(rtnode0.data, rtnode1.data) < 0.2)
                            //else if ((bayesFactor < bayesFactor_th))// ||&& rtnode1.LeafCount > 5 rtnode0.data.Cosine(rtnode0.data, rtnode1.data) < 0.2)
                            order2unorder = 1e6;
                    }
                    if (!node1.IsFreeNode)
                    {
                        TrySplitCost(rtnode1.MergeTreeIndex, node1, node1, addbranch1, out order2unorder, out unorder2order);
                        if (rtnode0.tree_depth > 3)
                        {
                            order2unorder = 1e10;
                            if (addbranch0 || addbranch1)
                                order2unorder = 1e12;
                        }
                        else if ((rtnode1.LeafCount < 10 || bayesFactor < bayesFactor_th)) // ||&& rtnode0.LeafCount > 5 rtnode0.data.Cosine(rtnode0.data, rtnode1.data) < 0.2)
                            //else if ((bayesFactor < bayesFactor_th)) // ||&& rtnode0.LeafCount > 5 rtnode0.data.Cosine(rtnode0.data, rtnode1.data) < 0.2)
                            order2unorder = 1e6;
                    }
#endif
                    return;
                }

                double order2unorder0, unorder2order0, order2unorder1, unorder2order1;
                ConstraintTreeNode commonancestor = GetCommonAncestor(node0, node1);
                double affleafcnt1 = TryMergeToAncestor(rtnode0.MergeTreeIndex, node0, commonancestor, addbranch0, out order2unorder0, out unorder2order0);
                double affleafcnt2 = TryMergeToAncestor(rtnode1.MergeTreeIndex, node1, commonancestor, addbranch1, out order2unorder1, out unorder2order1);
                AffectedLeafNumber = affleafcnt1 + affleafcnt2;// commonancestor.LeafNumber;

                order2unorder = order2unorder0 + order2unorder1;
                unorder2order = unorder2order0 + unorder2order1;
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    throw new Exception("Error");
            //}
        }

        protected double TryMergeToAncestor(int mergetreeindex,
            ConstraintTreeNode node, ConstraintTreeNode ancestor,
            bool addbranch, out double order2unorder, out double unorder2order)
        {
            node.SetActiveMergedTree(mergetreeindex);

            order2unorder = unorder2order = 0;
            double leaf = node.MergedLeafNumber, leafssum;//A, sum(a^2)

            if (addbranch)
            {
                //if (node.MergedLeafNumber < node.LeafNumber)  //they are equal with or without this judge
                if (node.Children != null &&
                    node.Children.Count != node.ActiveMergedTree.MergedChildren.Count)
                {
                    double A = node.MergedLeafNumber;
                    double B = node.LeafNumber - node.MergedLeafNumber;
                    unorder2order += (A * A - node.MergedChildLeafSquareSum) / 2 * B;
                    leafssum = A * A + node.ChildLeafSquareSum - node.MergedChildLeafSquareSum;

                    //Console.WriteLine("Test split free: {0}", mergetreeindex);
                    //if (unorder2order == 0) unorder2order = 1;
                    if (SplitFree(node, mergetreeindex))
                    {
                        fu2oViolation += unorder2order;
                        unorder2order = 0;
                    }
                    else
                        u2oViolation += unorder2order;
                    //if (node.LeafNumber < 10)
                    //    unorder2order = 1e100;
                }
                else
                {
                    node = node.Parent;
                    leafssum = node.ChildLeafSquareSum;
                }
            }
            else
                leafssum = node.ChildLeafSquareSum;

            while (!node.Equals(ancestor))
            {
                leaf = node.LeafNumber;
                //calculate collapse cost with parent
                ConstraintTreeNode parent = node.Parent;
                order2unorder += (leaf * leaf - leafssum) / 2 * (parent.LeafNumber - leaf);
                //update leaf&leafssum
                leafssum = parent.ChildLeafSquareSum - leaf * leaf + leafssum;
                node = parent;
            }
            o2uViolation += order2unorder;

            return leaf;
        }


        protected void TrySplitCost(int mergetreeindex,
            ConstraintTreeNode node, ConstraintTreeNode ancestor,
            bool addbranch, out double order2unorder, out double unorder2order)
        {
            node.SetActiveMergedTree(mergetreeindex);

            order2unorder = unorder2order = 0;
            double leaf = node.MergedLeafNumber, leafssum;//A, sum(a^2)

            if (addbranch)
            {
                //if (node.MergedLeafNumber < node.LeafNumber)  //they are equal with or without this judge
                if (node.Children != null &&
                    node.Children.Count != node.ActiveMergedTree.MergedChildren.Count)
                {
                    double A = node.MergedLeafNumber;
                    double B = node.LeafNumber - node.MergedLeafNumber;
                    unorder2order += (A * A - node.MergedChildLeafSquareSum) / 2 * B;
                    leafssum = A * A + node.ChildLeafSquareSum - node.MergedChildLeafSquareSum;
                    //if (unorder2order == 0) unorder2order = 1;

                    //Console.WriteLine("Test split free: {0}", mergetreeindex);
                    if (SplitFree(node, mergetreeindex))
                    {
                        fu2oViolation += unorder2order;
                        unorder2order = 0;
                    }
                    else
                        u2oViolation += unorder2order;
                    //if (node.LeafNumber < 10)
                    //    unorder2order = 1e100;
                }
            }
        }

#if NORMALIZE_PROJ_WEIGHT
        public void GetMergeBrokenOrderNumbersNoWeight(RoseTreeNode rtnode0, RoseTreeNode rtnode1,
     bool addbranch0, bool addbranch1,
     out double order2unorder, out double unorder2order)
        {
            //try
            {
                ConstraintTreeNode node0 = MergedTrees[rtnode0.MergeTreeIndex];
                ConstraintTreeNode node1 = MergedTrees[rtnode1.MergeTreeIndex];
                // node not in the constraint tree
                if (node0.IsFreeNode || node1.IsFreeNode)
                {
                    order2unorder = unorder2order = 0;
#if !NEW_YORK_TIMES_TEST_SMOOTHNESS
                    if (!node0.IsFreeNode)
                    {
                        TrySplitCostNoWeight(rtnode0.MergeTreeIndex, node0, node0, addbranch0, out order2unorder, out unorder2order);
                    }
                    if (!node1.IsFreeNode)
                    {
                        TrySplitCostNoWeight(rtnode1.MergeTreeIndex, node1, node1, addbranch1, out order2unorder, out unorder2order);
                    }
#endif
                    return;
                }

                double order2unorder0, unorder2order0, order2unorder1, unorder2order1;
                ConstraintTreeNode commonancestor = GetCommonAncestor(node0, node1);
                double affleafcnt1 = TryMergeToAncestorNoWeight(rtnode0.MergeTreeIndex, node0, commonancestor, addbranch0, out order2unorder0, out unorder2order0);
                double affleafcnt2 = TryMergeToAncestorNoWeight(rtnode1.MergeTreeIndex, node1, commonancestor, addbranch1, out order2unorder1, out unorder2order1);
                AffectedLeafNumber = affleafcnt1 + affleafcnt2;// commonancestor.LeafNumber;

                order2unorder = order2unorder0 + order2unorder1;
                unorder2order = unorder2order0 + unorder2order1;
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    throw new Exception("Error");
            //}
        }

        protected double TryMergeToAncestorNoWeight(int mergetreeindex,
            ConstraintTreeNode node, ConstraintTreeNode ancestor,
            bool addbranch, out double order2unorder, out double unorder2order)
        {
            node.SetActiveMergedTree(mergetreeindex);

            order2unorder = unorder2order = 0;
            double leaf = node.NWMergedLeafNumber, leafssum;//A, sum(a^2)

            if (addbranch)
            {
                //if (node.MergedLeafNumber < node.LeafNumber)  //they are equal with or without this judge
                if (node.Children != null &&
                    node.Children.Count != node.ActiveMergedTree.MergedChildren.Count)
                {
                    double A = node.NWMergedLeafNumber;
                    double B = node.NWLeafNumber - node.NWMergedLeafNumber;
                    unorder2order += (A * A - node.NWMergedChildLeafSquareSum) / 2 * B;
                    leafssum = A * A + node.NWChildLeafSquareSum - node.NWMergedChildLeafSquareSum;

                    //Console.WriteLine("Test split free: {0}", mergetreeindex);
                    //if (unorder2order == 0) unorder2order = 1;
                    if (SplitFree(node, mergetreeindex))
                    {
                        fu2oViolation += unorder2order;
                        unorder2order = 0;
                    }
                    else
                        u2oViolation += unorder2order;
                    //if (node.LeafNumber < 10)
                    //    unorder2order = 1e100;
                }
                else
                {
                    node = node.Parent;
                    leafssum = node.NWChildLeafSquareSum;
                }
            }
            else
                leafssum = node.NWChildLeafSquareSum;

            while (!node.Equals(ancestor))
            {
                leaf = node.NWLeafNumber;
                //calculate collapse cost with parent
                ConstraintTreeNode parent = node.Parent;
                order2unorder += (leaf * leaf - leafssum) / 2 * (parent.NWLeafNumber - leaf);
                //update leaf&leafssum
                leafssum = parent.NWChildLeafSquareSum - leaf * leaf + leafssum;
                node = parent;
            }
            o2uViolation += order2unorder;

            return leaf;
        }


        protected void TrySplitCostNoWeight(int mergetreeindex,
            ConstraintTreeNode node, ConstraintTreeNode ancestor,
            bool addbranch, out double order2unorder, out double unorder2order)
        {
            node.SetActiveMergedTree(mergetreeindex);

            order2unorder = unorder2order = 0;
            double leaf = node.NWMergedLeafNumber, leafssum;//A, sum(a^2)

            if (addbranch)
            {
                //if (node.MergedLeafNumber < node.LeafNumber)  //they are equal with or without this judge
                if (node.Children != null &&
                    node.Children.Count != node.ActiveMergedTree.MergedChildren.Count)
                {
                    double A = node.NWMergedLeafNumber;
                    double B = node.NWLeafNumber - node.NWMergedLeafNumber;
                    unorder2order += (A * A - node.NWMergedChildLeafSquareSum) / 2 * B;
                    leafssum = A * A + node.NWChildLeafSquareSum - node.NWMergedChildLeafSquareSum;
                    //if (unorder2order == 0) unorder2order = 1;

                    //Console.WriteLine("Test split free: {0}", mergetreeindex);
                    if (SplitFree(node, mergetreeindex))
                    {
                        fu2oViolation += unorder2order;
                        unorder2order = 0;
                    }
                    else
                        u2oViolation += unorder2order;
                    //if (node.LeafNumber < 10)
                    //    unorder2order = 1e100;
                }
            }
        }
#endif
        #endregion return probability: broken order numbers

        #region on merge tree
        public void GetMergeCost(RoseTreeNode node0, RoseTreeNode node1, MergeType mergetype,
            out double order2unorder, out double unorder2order)
        {
            switch (mergetype)
            {
                case MergeType.Join:
                    GetMergeCost(node0, node1, true, true, out order2unorder, out unorder2order);
                    break;
                case MergeType.AbsorbL:
                    GetMergeCost(node0, node1, false, true, out order2unorder, out unorder2order);
                    break;
                case MergeType.AbsorbR:
                    GetMergeCost(node0, node1, true, false, out order2unorder, out unorder2order);
                    break;
                default: //collapse
                    GetMergeCost(node0, node1, false, false, out order2unorder, out unorder2order);
                    break;
            }
        }

        protected double o2uViolation, u2oViolation, fu2oViolation;
        protected virtual void GetMergeCost(RoseTreeNode rtnode0, RoseTreeNode rtnode1,
            bool addbranch0, bool addbranch1,
            out double order2unorder, out double unorder2order)
        {
            o2uViolation = 0; u2oViolation = 0; fu2oViolation = 0;
            
#if NORMALIZE_PROJ_WEIGHT
            this.GetMergeBrokenOrderNumbersNoWeight(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);
            //TestAlgorithmCorrectness(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);
#else
            this.GetMergeBrokenOrderNumbers(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);
#endif       
            order2unorder = o2uViolation;
            unorder2order = u2oViolation;             
            if (BuildRoseTree.ViolationCurveFile != null)
                BuildRoseTree.ViolationCurveFile.Write("{0}\t{1}\t{2}\t", o2uViolation, u2oViolation, fu2oViolation);
        }

        protected void TestAlgorithmCorrectness(RoseTreeNode rtnode0, RoseTreeNode rtnode1,
    bool addbranch0, bool addbranch1,
    out double order2unorder, out double unorder2order)
        {
#if PROJ_WEIGHT_1
            double mo2uViolation = o2uViolation;
            double mu2oViolation = u2oViolation;
            double mfu2oViolation = fu2oViolation;

            o2uViolation = 0; u2oViolation = 0; fu2oViolation = 0;
            this.GetMergeBrokenOrderNumbers(rtnode0, rtnode1, addbranch0, addbranch1, out order2unorder, out unorder2order);

            if (mo2uViolation != o2uViolation ||
                mu2oViolation != u2oViolation ||
                mfu2oViolation != fu2oViolation)
            {
                Console.WriteLine("Error! TestAlgorithmCorrectness fails!\n");
                throw new Exception("Error! TestAlgorithmCorrectness fails!\n");
            }

            o2uViolation = mo2uViolation; u2oViolation = mu2oViolation; fu2oViolation = mfu2oViolation;
#else
            order2unorder = unorder2order = 0;
            Console.WriteLine("Warning! TestAlgorithmCorrectness not working!\n");
#endif
        }

        public void MergeTree(RoseTreeNode rtnode0, RoseTreeNode rtnode1,
            bool addbranch0, bool addbranch1)
        {
            //if (mergedtreepointer == 3871 || mergedtreepointer == 3884)
            //    Console.Write("");
            //Console.WriteLine("m" + mergedtreepointer);
            ConstraintTreeNode node0 = MergedTrees[rtnode0.MergeTreeIndex];
            ConstraintTreeNode node1 = MergedTrees[rtnode1.MergeTreeIndex];
            // node not in the constraint tree
            if (node0.IsFreeNode && node1.IsFreeNode)
            {
                ConstraintTreeNode newnode = CreateFreeConstraintNode();
                newnode.AddFreeChildren(node0, node1, addbranch0, addbranch1);
                newnode.InitialIndex = mergedtreepointer;
                MergedTrees[mergedtreepointer] = newnode;
                if (Root == null || Root.IsFreeNode)
                    Root = newnode;
            }
            else
            {
                if (node0.IsFreeNode)
                    node0 = AttachFreeNode(node1, node0, addbranch1, addbranch0, rtnode1.MergeTreeIndex);
                else if (node1.IsFreeNode)
                    node1 = AttachFreeNode(node0, node1, addbranch0, addbranch1, rtnode0.MergeTreeIndex);

                List<int> affectedRoseTreeNodeIndices = new List<int>();
                //adjust tree structure
                ConstraintTreeNode commonancestor = GetCommonAncestor(node0, node1);

                ConstraintTreeNode container0 =
                    MergeToAncestor(rtnode0.MergeTreeIndex, node0, commonancestor, addbranch0, affectedRoseTreeNodeIndices);
                ConstraintTreeNode container1 =
                    MergeToAncestor(rtnode1.MergeTreeIndex, node1, commonancestor, addbranch1, affectedRoseTreeNodeIndices);

#if !UNSORTED_CACHE && CONSTRAINT_CHANGE_UPDATE_ALL
                UpdateAffectedRoseTreeNodeIndices(commonancestor, affectedRoseTreeNodeIndices);
#endif
                //UpdateMergedTreePosition(commonancestor);
                commonancestor.MergeTree(mergedtreepointer,
                    rtnode0.MergeTreeIndex, node0, container0,
                    rtnode1.MergeTreeIndex, node1, container1);
                commonancestor.InitialIndex = mergedtreepointer;
                MergedTrees[mergedtreepointer] = commonancestor;

#if !UNSORTED_CACHE
                if (!bDisableUpdate)
                {
                    //update cached values
                    affectedRoseTreeNodeIndices.Remove(rtnode0.MergeTreeIndex);
                    affectedRoseTreeNodeIndices.Remove(rtnode1.MergeTreeIndex);
                    UpdateCacheValues(affectedRoseTreeNodeIndices);
                }
#endif
            }

            mergedtreepointer++;
        }

        private void UpdateCacheValues(List<int> affectedRoseTreeNodeIndices)
        {
            if (multiconstraint != null)
                multiconstraint.RecordAffectedArrayIndices(iConstraint, affectedRoseTreeNodeIndices);
            else
            {
                foreach (int affectnodeindex in affectedRoseTreeNodeIndices)
                    constrainedRoseTree.UpdateCacheValues(affectnodeindex);
            }
        }

        protected ConstraintTreeNode AttachFreeNode(ConstraintTreeNode cnode, ConstraintTreeNode freenode, 
            bool addbranch0, bool addbranch1, int cnode_mergetreeindex)
        {
            ConstraintTreeNode attachnode = cnode; // = cnode.Children == null ? cnode.Parent : cnode;
            if (addbranch0){
                cnode.SetActiveMergedTree(cnode_mergetreeindex);
                if (cnode.Children == null || cnode.Children.Count == cnode.ActiveMergedTree.MergedChildren.Count)
                    if (cnode.Parent != null)
                        attachnode = cnode.Parent;
            }

            var node = attachnode.AttachFreeNode(freenode, addbranch1);
            attachnode.UpdateLeafNumbers();
            node.UpdateLeafNumbers();

            return node;
            //attachnode.AttachFreeNode(freenode, Root, lfv.featurevectors.Length);
            //return freenode.Children == null ? freenode : attachnode;
        }

        protected ConstraintTreeNode MergeToAncestor(int mergetreeindex, ConstraintTreeNode node, ConstraintTreeNode ancestor,
            bool addbranch, List<int> affectedRoseTreeNodeIndices)
        {
            //if (mergetreeindex == 253)
            //    Console.Write("");
            node.SetActiveMergedTree(mergetreeindex);
            ConstraintTreeNode container = ancestor;

            if (addbranch)
            {
                if (node.Children != null &&
                    node.Children.Count != node.ActiveMergedTree.MergedChildren.Count)
                {
                    container = node.Split(inheritParentInfo);
                    MergedTrees[container.InitialIndex] = container;
                }
                else
                {
                    container = node;
                    //node.Parent.SetChildMerged(node);
                    node = node.Parent;
                }
            }
            ConstraintTreeNode collapsednode = null;
            while (node != ancestor)
            {
                collapsednode = node;
                node = node.Parent.CollapseLinkWithChild(node);    //node = node.Parent;
            }
            if (collapsednode != null && collapsednode.MergedChildren != null)
                foreach (int mergeindex in collapsednode.MergedChildren.Keys)
                    MergedTrees[mergeindex] = ancestor;

#if! UNSORTED_CACHE && !CONSTRAINT_CHANGE_UPDATE_ALL
            UpdateAffectedRoseTreeNodeIndices(collapsednode, affectedRoseTreeNodeIndices);
#endif
            return container;
        }

        protected void UpdateAffectedRoseTreeNodeIndices(ConstraintTreeNode collapsednode, List<int> affectedRoseTreeNodeIndices)
        {
            if (collapsednode == null || collapsednode.Children == null)// || collapsednode.InitialIndex == mergetreeindex)
                return;

            List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            queue.Add(collapsednode);
            while (queue.Count != 0)
            {
                ConstraintTreeNode ctnode = queue[0];
                queue.RemoveAt(0);

                if (ctnode.IsUnfinished())
                {
                    List<ConstraintTreeNode> children_ctnode = new List<ConstraintTreeNode>();
                    children_ctnode.AddRange(ctnode.Children);
                    if (ctnode.MergedChildren != null)
                    {
                        foreach (MergedTree mergedchild in ctnode.MergedChildren.Values)
                        {
                            affectedRoseTreeNodeIndices.Add(mergedchild.MergeTreeIndex);
                            foreach (ConstraintTreeNode child in mergedchild.MergedChildren)
                                children_ctnode.Remove(child);
                        }
                    }
                    queue.AddRange(children_ctnode);
                }
                else
                {
                    affectedRoseTreeNodeIndices.Add(ctnode.InitialIndex);
                    continue;
                }
            }
        }

        private void UpdateMergedTreePosition(ConstraintTreeNode commonancestor)
        {
            if (commonancestor.MergedChildren != null)
            {
                foreach (int mergetreeindex in commonancestor.MergedChildren.Keys)
                {
                    MergedTrees[mergetreeindex] = commonancestor;
                }
            }
        }
        #endregion on merge tree

        #region build the constraint tree
        protected void Initialize()
        {
            this.dataprojection = InitializeDataProjection(rosetree, this.lfv);
            //(dataprojection as RoseTreePredictionSearchDown).PrintAllCosinePredValue(lfv.featurevectors);

            //prepare leaves
            rosetreeleaves = rosetree.GetAllTreeLeaf();
            //OriginalConstraintTreeNodes = new ConstraintTreeNode[rosetreeleaves.Count];
            OriginalConstraintTreeNodes = new ConstraintTreeNode[2 * rosetree.lfv.featurevectors.Length];
            MergedTrees = new ConstraintTreeNode[2 * lfv.featurevectors.Length];

            BuildUpConstraintTree();
            if (WeightedLeafNode)
                AssignNearestNeighbour();
            else
                AssignNearestNeighbourUnweighted();
            AddAllNewTopicNodes();


            RemoveOriginalLeaves();
            //if (bExpandToBinaryConstraintTree)
            //    ExpandToBinaryConstraintTree();
            UpdateLeafNumbers();

            //for split/merge info
            InitializeInheritInfo();

            mergedtreepointer = lfv.featurevectors.Length;

            dataprojection = null;
        }

        private void CountConstraintTreeMaxChildNumber()
        {
            List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            queue.Add(this.Root);
            int maxchildnumber = int.MinValue;
            while (queue.Count != 0)
            {
                ConstraintTreeNode node = queue[0];
                queue.RemoveAt(0);

                if (node.Children != null)
                {
                    queue.AddRange(node.Children);
                    maxchildnumber = Math.Max(maxchildnumber, node.Children.Count);
                }
            }
            Console.WriteLine("maxchildnumber: {0}", maxchildnumber);
        }

        private void ExpandToBinaryConstraintTree()
        {
            List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            queue.Add(this.Root);
            while (queue.Count != 0)
            {
                ConstraintTreeNode node = queue[0];
                queue.RemoveAt(0);

                if (node.Children != null)
                {
                    queue.AddRange(node.Children);
                    if (node.Children.Count != 2)
                        Console.WriteLine("!!Not binary constraint tree!");
                }
            }
            Console.WriteLine("Pass Test!");
        }

        //may be better if consider constraint tree's leaf count
        protected void InitializeInheritInfo()
        {
            if (Root == null)
                return;

            inheritParentInfo = new InheritParentInfo();

            // Topics
            Dictionary<int, KeyValuePair<int, double>> topicInheritParentInfos = new Dictionary<int, KeyValuePair<int, double>>();
            Dictionary<int, double> topicSize = new Dictionary<int, double>();
            IList<RoseTreeNode> validInternalNode = rosetree.GetAllValidInternalTreeNodes();
            foreach (RoseTreeNode rtnode in validInternalNode)
            {
                ConstraintTreeNode ctnode = OriginalConstraintTreeNodes[rtnode.MergeTreeIndex];
                ConstraintTreeNode parent_ctnode = ctnode.Parent;
                ctnode.InitializeCorrespondingInformation(ctnode.NearestNeighbourArrayIndex);

                if (parent_ctnode == null) continue;
                topicSize.Add(ctnode.NearestNeighbourArrayIndex, ctnode.DocumentNumber);
                KeyValuePair<int, double> inheritinfo = new KeyValuePair<int, double>(
                    parent_ctnode.NearestNeighbourArrayIndex,
                    (double)ctnode.DocumentNumber / parent_ctnode.DocumentNumber);
                //ctnode.LeafNumber / parent_ctnode.LeafNumber);
                topicInheritParentInfos.Add(ctnode.NearestNeighbourArrayIndex, inheritinfo);
            }
            topicSize.Add(Root.NearestNeighbourArrayIndex, lfv.featurevectors.Length);
            topicInheritParentInfos.Add(Root.NearestNeighbourArrayIndex, new KeyValuePair<int, double>(Root.NearestNeighbourArrayIndex, 1));

            inheritParentInfo.TopicInheritParentInfos = topicInheritParentInfos;
            inheritParentInfo.TopicSize = topicSize;

            // Documents
            int docNum = lfv.featurevectors.Length;
            int[] documentInheritInfo_index = new int[docNum];
            double[] documentInheritInfo_weight = new double[docNum];
            for (int i = 0; i < docNum; i++)
            {
                ConstraintTreeNode docNode = MergedTrees[i];
                if (docNode.IsFreeNode) continue;
                documentInheritInfo_index[i] = docNode.Parent.NearestNeighbourArrayIndex;
                documentInheritInfo_weight[i] = (double)docNode.DocumentNumber / docNode.Parent.DocumentNumber;
                //documentInheritInfo_weight[i] = docNode.LeafNumber / docNode.Parent.LeafNumber;
            }

            inheritParentInfo.DocumentInheritParentInfo_Index = documentInheritInfo_index;
            inheritParentInfo.DocumentInheritParentInfo_Weight = documentInheritInfo_weight;
        }

        protected void InitializeInheritInfo_Prev()
        {
            inheritParentInfo = new InheritParentInfo();

            // Topics
            Dictionary<int, KeyValuePair<int, double>> topicInheritParentInfos = new Dictionary<int, KeyValuePair<int, double>>();
            Dictionary<int, double> topicSize = new Dictionary<int, double>();
            IList<RoseTreeNode> validInternalNode = rosetree.GetAllValidInternalTreeNodes();
            foreach (RoseTreeNode rtnode in validInternalNode)
            {
                ConstraintTreeNode ctnode = OriginalConstraintTreeNodes[rtnode.MergeTreeIndex];
                ConstraintTreeNode parent_ctnode = ctnode.Parent;
                ctnode.InitializeCorrespondingInformation(ctnode.NearestNeighbourArrayIndex);
                topicSize.Add(ctnode.NearestNeighbourArrayIndex, ctnode.DocumentNumber);

                if (parent_ctnode == null) continue;
                KeyValuePair<int, double> inheritinfo = new KeyValuePair<int, double>(
                    parent_ctnode.NearestNeighbourArrayIndex,
                    (double)ctnode.DocumentNumber / parent_ctnode.DocumentNumber);
                //ctnode.LeafNumber / parent_ctnode.LeafNumber);
                topicInheritParentInfos.Add(ctnode.NearestNeighbourArrayIndex, inheritinfo);
            }
            topicSize[Root.NearestNeighbourArrayIndex] = lfv.featurevectors.Length;
#if OPEN_LARGE_CLUSTER
#endif
            inheritParentInfo.TopicInheritParentInfos = topicInheritParentInfos;
            inheritParentInfo.TopicSize = topicSize;

            // Documents
            int docNum = lfv.featurevectors.Length;
            int[] documentInheritInfo_index = new int[docNum];
            double[] documentInheritInfo_weight = new double[docNum];
            for (int i = 0; i < docNum; i++)
            {
                ConstraintTreeNode docNode = MergedTrees[i];
                if (docNode.IsFreeNode) continue;
                documentInheritInfo_index[i] = docNode.Parent.NearestNeighbourArrayIndex;
                documentInheritInfo_weight[i] = (double)docNode.DocumentNumber / docNode.Parent.DocumentNumber;
                //documentInheritInfo_weight[i] = docNode.LeafNumber / docNode.Parent.LeafNumber;
            }

            inheritParentInfo.DocumentInheritParentInfo_Index = documentInheritInfo_index;
            inheritParentInfo.DocumentInheritParentInfo_Weight = documentInheritInfo_weight;
        }

        //copy original rosetree, build a new one
        protected void BuildUpConstraintTree()
        {
            //build structure
            if (rosetree.root.children == null)
            {
                Root = OriginalConstraintTreeNodes[0] = NewConstraintNode();
                this.Root.OriginalLinkedNodeIndex = 0;
                return;
            }

            //width-first traversal
            List<RoseTreeNode> rtnodelist = new List<RoseTreeNode>();
            rtnodelist.Add(rosetree.root);
            List<ConstraintTreeNode> cnodelist = new List<ConstraintTreeNode>();
            this.Root = NewConstraintNode();
            cnodelist.Add(this.Root);
            OriginalConstraintTreeNodes[rosetree.root.indices.array_index] = this.Root;
            this.Root.OriginalLinkedNodeIndex = rosetree.root.indices.array_index;
            while (rtnodelist.Count != 0)
            {
                RoseTreeNode rosetreenode = rtnodelist[0];
                ConstraintTreeNode ctreenode = cnodelist[0];
                if (rosetreenode.tree_depth > 2)
                {
                    RoseTreeNode[] children = rosetreenode.children;
                    for (int i = 0; i < children.Length; i++)
                    {
                        RoseTreeNode rtchild = children[i];
                        if (rtchild.tree_depth == 1)
                        {
                            ConstraintTreeNode childctreenode = ctreenode.CreateChild();
                            OriginalConstraintTreeNodes[rtchild.indices.array_index] = childctreenode;
                            childctreenode.OriginalLinkedNodeIndex = rtchild.indices.array_index;
                        }
                        else
                        {
                            rtnodelist.Add(rtchild);
                            ConstraintTreeNode childctreenode = ctreenode.CreateChild();
                            cnodelist.Add(childctreenode);
                            //Added for different data projection types
                            OriginalConstraintTreeNodes[rtchild.indices.array_index] = childctreenode;
                            childctreenode.OriginalLinkedNodeIndex = rtchild.indices.array_index;
                        }
                    }
                }
                else if (rosetreenode.tree_depth == 2)
                {
                    RoseTreeNode[] children = rosetreenode.children;
                    for (int i = 0; i < children.Length; i++)
                    {
                        ConstraintTreeNode childctreenode = ctreenode.CreateChild();
                        OriginalConstraintTreeNodes[children[i].indices.array_index] = childctreenode;
                        childctreenode.OriginalLinkedNodeIndex = children[i].indices.array_index;
                    }
                }
                else
                    Console.WriteLine("Error");

                ctreenode.NearestNeighbourArrayIndex = rosetreenode.MergeTreeIndex;
                //if (rosetreenode.tree_depth != 1)
                //    ctreenode.InitializeCorrespondingInformation(rosetreenode.MergeTreeIndex);
                rtnodelist.RemoveAt(0);
                cnodelist.RemoveAt(0);
            }
        }

        //StreamWriter ofile_projection = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\projection_content.dat");
        protected void AssignNearestNeighbour()
        {
            SparseVectorList[] featurevectors = lfv.featurevectors;
            //StreamWriter ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\LeafWeights.dat");

            //CompareOverlapRatio();

            int projectInternalNodeCnt = 0, projectDocumentCnt = 0, projectNewDocumentCnt = 0;
            for (int i = 0; i < featurevectors.Length; i++)
            {
                SparseVectorList vector = featurevectors[i];
#if WRITE_PROJECTION_CONTENT
                (dataprojection as RoseTreePredictionDataProjection).ofile.WriteLine("[Project] {0}", i);
#endif
                NodeProjectionType projType;
                int nnarrayindex = GetProjectedArrayIndex(vector, i, out projType);
                if (nnarrayindex != i)
                    Console.Write("");
                ConstraintTreeNode newnode = null;
                switch (projType)
                {
                    case NodeProjectionType.Cousin:
                        newnode = OriginalConstraintTreeNodes[nnarrayindex].AddCousin();
                        projectDocumentCnt++;
                        break;
                    case NodeProjectionType.InCluster:
                        //if (nnarrayindex < 0 || nnarrayindex >= OriginalConstraintTreeNodes.Length)
                        //    Console.Write("");
                        newnode = OriginalConstraintTreeNodes[nnarrayindex].CreateChild();
                        projectInternalNodeCnt++;
                        //newnode = OriginalConstraintTreeNodes[nnarrayindex].AddNewTopic();
                        break;
                    case NodeProjectionType.Abandon:
                        newnode = CreateFreeConstraintNode();
                        newnode.InitialIndex = i;
                        newnode.NearestNeighbourArrayIndex = -1;
                        newnode.NewDocumentNearestNeighbourArrayIndex = nnarrayindex;
                        MergedTrees[i] = newnode;
                        //newnode.SetLeafMergedTree(i);
                        freedocuments.Add(newnode);
                        projectNewDocumentCnt++;
                        break;
                    default:
                        throw new NotImplementedException();
                }
#if WRITE_PROJECTION_CONTENT
                (dataprojection as RoseTreePredictionDataProjection).ofile.WriteLine("[NN] {0}", nnarrayindex);
#endif
                if (projType != NodeProjectionType.Abandon)
                {
                    newnode.InitialIndex = i;
                    newnode.NearestNeighbourArrayIndex = nnarrayindex;
                    //update leaf nodes'weight
#if PROJ_WEIGHT_1
                    newnode.UpdateLeafNodeLeafNumbers(1);
#elif PROJ_WEIGHT_3
                    double cosinesimi = vector.Cosine(vector, rosetree.GetNodeByArrayIndex(nnarrayindex).data);
                    newnode.UpdateLeafNodeLeafNumbers(Math.Pow(cosinesimi,1.0/3));
#else
                    double cosinesimi = vector.Cosine(vector, rosetree.GetNodeByArrayIndex(nnarrayindex).data);
                    newnode.UpdateLeafNodeLeafNumbers(cosinesimi);
#endif
                    MergedTrees[i] = newnode;
                    newnode.SetLeafMergedTree(i);
                    NotFreeConstraintTreeLeafCount++;
                }
            }
            Console.WriteLine("Project to Internal Node : {0} ({1}%)", projectInternalNodeCnt, 100 * projectInternalNodeCnt / featurevectors.Length);
            Console.WriteLine("Project to Document Node: {0} ({1}%)", projectDocumentCnt, 100 * projectDocumentCnt / featurevectors.Length);
            Console.WriteLine("Abandon Node: {0} ({1}%)", projectNewDocumentCnt, 100 * projectNewDocumentCnt / featurevectors.Length);
        }

        protected virtual int GetProjectedArrayIndex(SparseVectorList vector, int vectorid, out NodeProjectionType projType)
        {
            return dataprojection.GetProjectedArrayIndex(vector, out projType);
        }

        protected void CompareOverlapRatio()
        {
            StreamWriter ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\CompareOverlapRatio.dat");

            int docNum0 = rosetree.lfv.featurevectors.Length;
            int docNum1 = this.lfv.featurevectors.Length;

            //Calculate by sampleitems
            int sampleitemoverlap = 0;
            int samplelineoverlap = 0;
            int vectoroverlap = 0;
            ofile.WriteLine("=====================Calculate by sampleitems=====================");
            IList<int> sampleitems0 = rosetree.lfv.GetSampleItems();
            IList<int> sampleitems1 = lfv.GetSampleItems();
            List<int> remainindexes0 = rosetree.lfv.remainedindexes;
            List<int> remainindexes1 = lfv.remainedindexes;
            int isampleitem0 = 0;
            foreach (int sampleitem0 in sampleitems0)
            {
                int isampleitem1 = 0;
                foreach (int sampleitem1 in sampleitems1)
                {
                    if (sampleitem0 == sampleitem1)
                    {
                        sampleitemoverlap++;
                        SparseVectorList vector0 = rosetree.lfv.featurevectors[isampleitem0];
                        SparseVectorList vector1 = lfv.featurevectors[isampleitem1];
                        string sampleline0 = rosetree.lfv.GetSampleLine(isampleitem0);
                        string sampleline1 = lfv.GetSampleLine(isampleitem1);
                        ofile.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                        ofile.WriteLine("Cosine: " + vector0.Cosine(vector0, vector1));
                        int vectorsame = CompareSparseVector(vector0, vector1);
                        ofile.WriteLine("Same vector:" + vectorsame);
                        if (vectorsame > 0)
                            vectoroverlap++;
                        else
                        {
                            ofile.WriteLine("<{0},{1}>", remainindexes0[isampleitem0], remainindexes1[isampleitem1]);
                            ofile.WriteLine("--------------------------------------------");
                            ofile.WriteLine(SparseVectorToString(vector0));
                            ofile.WriteLine("--------------------------------------------");
                            ofile.WriteLine(SparseVectorToString(vector1));
                            ofile.WriteLine("--------------------------------------------");
                            if (sampleline0.Equals(sampleline1))
                                ofile.WriteLine(sampleline0);
                        }
                        ofile.WriteLine("Same sample line:" + sampleline0.Equals(sampleline1));
                        if (sampleline0.Equals(sampleline1))
                            samplelineoverlap++;
                        //else
                        //{
                        //    ofile.WriteLine("--------------------------------------------");
                        //    ofile.WriteLine(sampleline0);
                        //    ofile.WriteLine("--------------------------------------------");
                        //    ofile.WriteLine(sampleline1);
                        //}
                        break;
                    }
                    isampleitem1++;
                }
                isampleitem0++;
            }

            ofile.WriteLine("OverlapRatio: {0}%", 100.0 * sampleitemoverlap / docNum0);
            ofile.WriteLine("OverlapRatio_SampleLine: {0}%", 100.0 * samplelineoverlap / docNum0);
            ofile.WriteLine("==========================================");

            //Calculate by samplelines
            ofile.WriteLine("=====================Calculate by samplelines=====================");
            samplelineoverlap = 0;
            for (isampleitem0 = 0; isampleitem0 < docNum0; isampleitem0++)
            {
                for (int isampleitem1 = 0; isampleitem1 < docNum1; isampleitem1++)
                {
                    string sampleline0 = rosetree.lfv.GetSampleLine(isampleitem0);
                    string sampleline1 = lfv.GetSampleLine(isampleitem1);
                    if (sampleline0.Equals(sampleline1))
                    {
                        samplelineoverlap++;
                        break;
                    }
                }
            }
            ofile.WriteLine("OverlapRatio_SampleLine: {0}%", 100.0 * samplelineoverlap / docNum0);
            ofile.WriteLine("==========================================");

            //Calculate by vectors
            ofile.WriteLine("=====================Calculate by vectors=====================");
            vectoroverlap = 0;
            for (isampleitem0 = 0; isampleitem0 < docNum0; isampleitem0++)
            {
                for (int isampleitem1 = 0; isampleitem1 < docNum1; isampleitem1++)
                {
                    SparseVectorList vector0 = rosetree.lfv.featurevectors[isampleitem0];
                    SparseVectorList vector1 = lfv.featurevectors[isampleitem1];
                    if (CompareSparseVector(vector0, vector1) > 0)
                    {
                        vectoroverlap++;
                        break;
                    }
                }
            }
            ofile.WriteLine("OverlapRatio_Vectors: {0}%", 100.0 * vectoroverlap / docNum0);
            ofile.WriteLine("==========================================");

            ofile.Flush();
            ofile.Close();
        }

        protected int CompareSparseVector(SparseVectorList vector0, SparseVectorList vector1)
        {
            if (vector0.keyarray.Length == vector1.keyarray.Length)
            {
                int[] keyarray0 = vector0.keyarray;
                int[] keyarray1 = vector1.keyarray;
                int[] valuearray0 = vector0.valuearray;
                int[] valuearray1 = vector1.valuearray;
                for (int i = 0; i < keyarray0.Length; i++)
                {
                    if (keyarray0[i] != keyarray1[i])
                        return -2;
                    if (valuearray0[i] != valuearray1[i])
                        return -2;
                }
            }
            else
                return -1;

            return 1;
        }

        public static string SparseVectorToString(SparseVectorList vector)
        {
            string str = "";
            int[] keyarray = vector.keyarray;
            int[] valuearray = vector.valuearray;
            str += string.Format("[{0}]\t", keyarray.Length);
            for (int i = 0; i < keyarray.Length; i++)
            {
                str += string.Format("<{0},{1}>", keyarray[i], valuearray[i]);
            }
            return str;
        }

        protected void AssignNearestNeighbourUnweighted()
        {
            throw new NotImplementedException();
            //SparseVectorList[] featurevectors = lfv.featurevectors;
            //for (int i = 0; i < featurevectors.Length; i++)
            //{
            //    SparseVectorList vector = featurevectors[i];
            //    //int nnarrayindex = Constraint.FindNearestNeighbour(vector, rosetree, rosetreeleaves);
            //    bool bCousin;
            //    int nnarrayindex = dataprojection.GetProjectedArrayIndex(vector, out bCousin);
            //    ConstraintTreeNode newnode;
            //    if (bCousin)
            //        newnode = OriginalConstraintTreeNodes[nnarrayindex].AddCousin();
            //    else
            //        newnode = OriginalConstraintTreeNodes[nnarrayindex].CreateChild();
            //    newnode.InitialIndex = i;
            //    newnode.UpdateLeafNodeLeafNumbers(1);
            //    MergedTrees[i] = newnode;
            //    newnode.SetLeafMergedTree(i);
            //}
        }

        protected void AddAllNewTopicNodes()
        {
            List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            queue.Add(this.Root);
            while (queue.Count != 0)
            {
                ConstraintTreeNode cnode = queue[0];
                queue.RemoveAt(0);

                cnode.AddNewTopicNodesToChildren();
                if (cnode.Children != null)
                    queue.AddRange(cnode.Children);
            }
        }

        //remove original tree leaves correspond to rosetree
        protected virtual void RemoveOriginalLeaves()
        {
            for (int i = 0; i < rosetree.lfv.featurevectors.Length; i++)
            {
                ConstraintTreeNode orgtreenode = OriginalConstraintTreeNodes[i];
                if (orgtreenode != null && orgtreenode.Children == null)
                {
                    RemoveNodeFromConstraintTree(orgtreenode);
                }
            }
        }

        private void RemoveNodeFromConstraintTree(ConstraintTreeNode orgtreenode)
        {
            //remove this node from constrainttree
            ConstraintTreeNode parent = orgtreenode.Parent;
            if (parent != null)
                if (parent.Children.Count > 2)
                    parent.Children.Remove(orgtreenode);
                else
                {
                    parent.Children.Remove(orgtreenode);
                    if (parent.Parent != null)
                    {
                        parent.CollapseLinkWithParentNoUpdate();
                    }
                    else
                        if (parent.Children.Count == 0)
                        {
                            this.Root = null;
                        }
                        else
                        {
                            if (parent.Children[0].Children != null)
                            {
                                this.Root = parent.Children[0];
                                this.Root.Parent = null;
                            }
                        }
                }
            else
                this.Root = null;
        }

        public void UpdateLeafNumbers()
        {
            if (Root == null)
                return;

            //record tree nodes by their depth in tree
            List<List<ConstraintTreeNode>> ctreenodes_alldepth = new List<List<ConstraintTreeNode>>();
            List<ConstraintTreeNode> ctreenodes_depth = new List<ConstraintTreeNode>();
            ctreenodes_alldepth.Add(ctreenodes_depth);
            //width-first traversal
            List<ConstraintTreeNode> cnodelist = new List<ConstraintTreeNode>();
            cnodelist.Add(this.Root);
            cnodelist.Add(null);
            while (cnodelist.Count != 1)    //always a "null"
            {
                ConstraintTreeNode ctreenode = cnodelist[0];
                if (ctreenode != null)
                {
                    //ctreenodes_depth.Add(ctreenode);      //add leaves too
                    if (ctreenode.Children != null)
                    {
                        cnodelist.AddRange(ctreenode.Children);
                        ctreenodes_depth.Add(ctreenode);    //add only intermediate nodes
                    }
                }
                else
                {
                    ctreenodes_depth = new List<ConstraintTreeNode>();
                    ctreenodes_alldepth.Add(ctreenodes_depth);
                    cnodelist.Add(null);
                }
                cnodelist.RemoveAt(0);
            }

            //update from nodes with largest depth
            for (int depth = ctreenodes_alldepth.Count - 1; depth >= 0; depth--)
            {
                ctreenodes_depth = ctreenodes_alldepth[depth];
                foreach (ConstraintTreeNode node in ctreenodes_depth)
                {
                    //if (node.Children == null)
                    //{
                    //  node.UpdateLeafNodeLeafNumbers(1);
                    //  MergedTrees[node.InitialIndex] = node;
                    //  node.SetLeafMergedTree(node.InitialIndex);
                    //}
                    //else
                    node.UpdateLeafNumbers();
                }
            }
        }
        #endregion build the constraint tree

        #region postprocess
        bool bPostProcessFinished = false;
        public void PostProcessContainedInformation()
        {
            if (bPostProcessFinished)
                throw new Exception("Error! Post process only once!");
            bPostProcessFinished = true;

            ////traversal
            //List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            //queue.Add(Root);

            //while (queue.Count != 0)
            //{
            //    ConstraintTreeNode ctnode = queue[0];
            //    queue.RemoveAt(0);

            //    ctnode.PostProcessContainedInformation(inheritParentInfo);

            //    foreach (ConstraintTreeNode child_ctnode in ctnode.Children)
            //        if (child_ctnode.Children != null)
            //            queue.Add(child_ctnode);
            //}

            Dictionary<int, double> freedoc_containedinfo = new Dictionary<int, double>();
            freedoc_containedinfo.Add(Root.NearestNeighbourArrayIndex, 1.0 / lfv.featurevectors.Length);
            foreach (ConstraintTreeNode freedoc in freedocuments)
            {
                //add to all ancestors except root
                ConstraintTreeNode addinfonode = freedoc.Parent;
                while (addinfonode != Root)
                {
                    //if (addinfonode.InitialIndex == 188)
                    //    Console.Write("");
                    addinfonode.AddContainedInformation(freedoc_containedinfo);
                    addinfonode = addinfonode.Parent;
                }
            }
        }
        #endregion postprocess

        #region draw tree
        public void DrawConstraintTree(string filename, bool bDrawInternalNodesOnly = false,
            bool bDrawLeafNumber = false)
        {
            //try
            {
                this.bDrawLeafNumber = bDrawLeafNumber;

                if (bDrawInternalNodesOnly)
                    DrawInternalConstraintTree(filename);
                else
                {
                    StreamWriter drawtree = new StreamWriter(filename);
#if PROJECTION_FILE_BINARY
                    BinaryWriter drawtreeprojectionSingle = new BinaryWriter(new FileStream(filename.Substring(0, filename.Length - 3) + "_proj_single.bin", FileMode.Create));
                    BinaryWriter drawtreeprojectionMany = new BinaryWriter(new FileStream(filename.Substring(0, filename.Length - 3) + "_proj_many.bin", FileMode.Create));
#else
                    StreamWriter drawtreeprojection = new StreamWriter(filename.Substring(0, filename.Length - 3) + "_proj.dat");
#endif
                    drawtree.WriteLine("digraph G \n {graph[ \n rankdir = \"TD\"];");

                    int LastTreeDocumentCount = rosetree.lfv.featurevectors.Length;
#if PROJECTION_FILE_BINARY
                    //drawtreeprojection.Write(LastTreeDocumentCount);
#else
                    drawtreeprojection.WriteLine("DocCnt = {0}", LastTreeDocumentCount);
#endif
                    //width-first traversal
                    List<ConstraintTreeNode> cnodelist = new List<ConstraintTreeNode>();
                    cnodelist.Add(this.Root);
                    int treeindex = 0;
                    //int newDocCnt = 0, docPairCnt = 0, docNewTopicCnt = 0, leafCnt = 0;
                    IList<RoseTreeNode> rosetreenodes = rosetree.GetAllValidTreeNodes();
                    while (cnodelist.Count != 0)
                    {
                        ConstraintTreeNode ctreenode = cnodelist[0];
                        if (ctreenode.Children != null)
                            cnodelist.AddRange(ctreenode.Children);
                        ctreenode.DrawTreeIndex = treeindex;

                        if (ctreenode.Parent != null)
                            drawtree.WriteLine(ctreenode.Parent.DrawTreeIndex + "->" + ctreenode.DrawTreeIndex);
                        DrawConstraintNode(drawtree, ctreenode);

                        if (ctreenode.Children == null)
                        {
#if PROJECTION_FILE_BINARY
                            Dictionary<int, KeyValuePair<Dictionary<int, double>, double>> weightedKeyWordsDictionary = GetNodeWeightedWordsDictionary(ctreenode, weightedSimilarity);
                            DrawNodeProjectionBinary(drawtreeprojectionSingle, ctreenode, LastTreeDocumentCount, rosetreenodes, true);
                            DrawNodeProjectionBinary(drawtreeprojectionMany, ctreenode, LastTreeDocumentCount, rosetreenodes, false, weightedKeyWordsDictionary);
#else
                            DrawNodeProjection(drawtreeprojection, ctreenode, LastTreeDocumentCount, rosetreenodes);
#endif
                        }
                        cnodelist.RemoveAt(0);
                        treeindex++;
                    }

                    //if (newDocCnt + docPairCnt + docNewTopicCnt == LastTreeDocumentCount)
                    //{
                    //    Console.WriteLine("New document count: {0}, {1}%", newDocCnt, 100 * newDocCnt / lfv.featurevectors.Length);
                    //    Console.WriteLine("Document pair count: {0}, {1}%", docPairCnt, 100 * docPairCnt / lfv.featurevectors.Length);
                    //    Console.WriteLine("New topic document count: {0}, {1}%", docNewTopicCnt, 100 * docNewTopicCnt / lfv.featurevectors.Length);
                    //}
                    //drawtreeprojection.WriteLine("New document count: {0}, {1}%", newDocCnt, 100.0 * newDocCnt / lfv.featurevectors.Length);
                    //drawtreeprojection.WriteLine("Document pair count: {0}, {1}%", docPairCnt, 100.0 * docPairCnt / lfv.featurevectors.Length);
                    //drawtreeprojection.WriteLine("New topic document count: {0}, {1}%", docNewTopicCnt, 100.0 * docNewTopicCnt / lfv.featurevectors.Length);

#if PROJECTION_FILE_BINARY
                    drawtreeprojectionSingle.Flush();
                    drawtreeprojectionSingle.Close();
                    drawtreeprojectionMany.Flush();
                    drawtreeprojectionMany.Close();
#else
                    drawtreeprojection.Flush();
                    drawtreeprojection.Close();
#endif
                    drawtree.WriteLine("}");
                    drawtree.Flush();
                    drawtree.Close();
                }
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine(e.StackTrace);
            //    Console.WriteLine("bDrawInternalNodesOnly={0}", bDrawInternalNodesOnly);
            //    Console.WriteLine("bDrawLeafNumber={0}", bDrawLeafNumber);
            //    Console.ReadKey();
            //}
        }

        private void DrawNodeProjectionBinary(BinaryWriter drawtreeprojection, ConstraintTreeNode ctreenode,
            int LastTreeDocumentCount, IList<RoseTreeNode> rosetreenodes, bool bSingleProjection,
            Dictionary<int, KeyValuePair<Dictionary<int, double>, double>> weightedKeyWordsDictionary = null)
        {
            double projCosine = double.NaN;

            SortedDictionary<CosineKey, RoseTreeNode> projectionCandidates = new SortedDictionary<CosineKey, RoseTreeNode>(new ReverseDoubleComparer());
            if (!bSingleProjection)
            {
                if (ctreenode.NearestNeighbourArrayIndex >= 0)
                {
                    projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NearestNeighbourArrayIndex).data);
                }
                else
                    if (ctreenode.NewDocumentNearestNeighbourArrayIndex < LastTreeDocumentCount)
                    {
                        projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).data);
                    }
                    else
                    {
                        projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).data);
                    }
                //leafCnt++;

                /// Multiple projection ///
                double projCosineCut = projCosine * MultipleProjectionFactor;

                //#region org weighted many to many projection
                //Dictionary<string, double> weightedTerms = new Dictionary<string, double>();
                ////weightedTerms.Add("lumia", 10);
                ////weightedTerms.Add("nokia", 8);
                //double enhanceWeight = 10;
                ////string[] enhancekeywords = new string[]{"analytics","vendor","quarter","2011","million",
                ////"top","share","largest","percent","q4"};
                ////string[] enhancekeywords = new string[]{"vendor","analytics","world","quarter","2011",
                ////"million","top","shipments","share","2.7","units","lumia","fourth","devices","research",
                ////"mobile","maker","36"};
                //string[] enhancekeywords = new string[]{"vendor","analytics","world","quarter","2011",
                //"million","top","shipments","share","2.7","units"};

                //foreach (string enhancekeyword in enhancekeywords)
                //    weightedTerms.Add(enhancekeyword, enhanceWeight);

                //Dictionary<int, double> weightedIndices = GetWeightedTermIndices(weightedTerms);

                //foreach (RoseTreeNode rosetreenode in rosetreenodes)
                //{
                //    if (rosetreenode.MergeTreeIndex == ctreenode.NearestNeighbourArrayIndex ||
                //        rosetreenode.MergeTreeIndex == ctreenode.NewDocumentNearestNeighbourArrayIndex)
                //        continue;
                //    double cosine = GetWeightedCosineSimilarity(rosetreenode.data, lfv.featurevectors[ctreenode.InitialIndex], weightedIndices);
                //    if (cosine > projCosineCut)
                //        projectionCandidates.Add(new CosineKey(cosine, rosetreenode.MergeTreeIndex), rosetreenode);
                //}
                //#endregion
                //#region org many to many projection
                //foreach (RoseTreeNode rosetreenode in rosetreenodes)
                //{
                //    if (rosetreenode.MergeTreeIndex == ctreenode.NearestNeighbourArrayIndex ||
                //        rosetreenode.MergeTreeIndex == ctreenode.NewDocumentNearestNeighbourArrayIndex)
                //        continue;
                //    double cosine = GetCosineSimilarity(rosetreenode.data, lfv.featurevectors[ctreenode.InitialIndex]);
                //    if (cosine > projCosineCut)
                //        projectionCandidates.Add(new CosineKey(cosine, rosetreenode.MergeTreeIndex), rosetreenode);
                //}
                //#endregion

                foreach (RoseTreeNode rosetreenode in rosetreenodes)
                {
                    if (rosetreenode.MergeTreeIndex == ctreenode.NearestNeighbourArrayIndex ||
                        rosetreenode.MergeTreeIndex == ctreenode.NewDocumentNearestNeighbourArrayIndex)
                        continue;

                    KeyValuePair<Dictionary<int, double>, double> kvp = GetNodeWeightedIndices(rosetreenode, weightedKeyWordsDictionary);
                    Dictionary<int, double> weightedIndices = kvp.Key;
                    double similarityThreshold;
                    if (weightedIndices != null)
                        similarityThreshold = kvp.Value;
                    else
                    {
                        double projectSimilarityThreshold = GetNodeSimilarityThreshold(rosetreenode, ctreenode, similarityThresholds);
                        if (projectSimilarityThreshold >= 0)
                            similarityThreshold = projectSimilarityThreshold;

                        else
                            similarityThreshold = projCosineCut;
                    }

                    double cosine = GetCosineSimilarity(rosetreenode.data, lfv.featurevectors[ctreenode.InitialIndex], weightedIndices);
                    if (cosine > similarityThreshold)
                        projectionCandidates.Add(new CosineKey(cosine, rosetreenode.MergeTreeIndex), rosetreenode);
                }

            }

            //row cnt
            if(ctreenode.NearestNeighbourArrayIndex >= 0)
                drawtreeprojection.Write(projectionCandidates.Count + 1);
            else
                drawtreeprojection.Write(projectionCandidates.Count + 2);

            drawtreeprojection.Write(ctreenode.InitialIndex);
            drawtreeprojection.Write(ctreenode.Parent.InitialIndex);

            if (ctreenode.NearestNeighbourArrayIndex < LastTreeDocumentCount)
                if (ctreenode.NearestNeighbourArrayIndex < 0)
                {
                    drawtreeprojection.Write(((int)-2));
                    drawtreeprojection.Write(rosetree.root.MergeTreeIndex);
                    drawtreeprojection.Write(((float)0));
                }
                else
                {
                    drawtreeprojection.Write(ctreenode.NearestNeighbourArrayIndex);
                    drawtreeprojection.Write(rosetree.GetNodeByArrayIndex(ctreenode.NearestNeighbourArrayIndex).parent.MergeTreeIndex);
                }
            else
            {
                drawtreeprojection.Write(((int)-1));
                drawtreeprojection.Write(ctreenode.NearestNeighbourArrayIndex);
            }
            if (ctreenode.NearestNeighbourArrayIndex >= 0)
            {
                projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NearestNeighbourArrayIndex).data);
                drawtreeprojection.Write(((float)projCosine));
            }
            else
                if (ctreenode.NewDocumentNearestNeighbourArrayIndex < LastTreeDocumentCount)
                {
                    projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).data);
                    drawtreeprojection.Write(ctreenode.NewDocumentNearestNeighbourArrayIndex);
                    drawtreeprojection.Write(rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).parent.MergeTreeIndex);
                    drawtreeprojection.Write(((float)projCosine));
                }
                else
                {
                    projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).data);
                    drawtreeprojection.Write(((int)-1));
                    drawtreeprojection.Write(ctreenode.NewDocumentNearestNeighbourArrayIndex);
                    drawtreeprojection.Write(((float)projCosine));
                }
            //leafCnt++;

            foreach (KeyValuePair<CosineKey, RoseTreeNode> kvp in projectionCandidates)
            {
                RoseTreeNode rosetreenode = kvp.Value;
                if (rosetreenode.children == null)
                {
                    drawtreeprojection.Write(rosetreenode.MergeTreeIndex);
                    drawtreeprojection.Write(rosetreenode.parent.MergeTreeIndex);
                }
                else
                {
                    //project to internal node
                    drawtreeprojection.Write(((int)-1));
                    drawtreeprojection.Write(rosetreenode.MergeTreeIndex);
                }
                drawtreeprojection.Write(((float)kvp.Key.Cosine));
            }

        }

        private void DrawNodeProjection(StreamWriter drawtreeprojection, ConstraintTreeNode ctreenode,
            int LastTreeDocumentCount, IList<RoseTreeNode> rosetreenodes)
        {
            double projCosine = double.NaN;
            //if (ctreenode.InitialIndex == 148)
            //    Console.Write("");
            drawtreeprojection.Write("{0}({1})\t", ctreenode.InitialIndex, ctreenode.Parent.InitialIndex);
            if (ctreenode.NearestNeighbourArrayIndex < LastTreeDocumentCount)
                if (ctreenode.NearestNeighbourArrayIndex < 0)
                {
                    drawtreeprojection.Write("{0}({1})", ctreenode.NearestNeighbourArrayIndex, rosetree.root.MergeTreeIndex);
                    //newDocCnt++;
                }
                else
                {
                    drawtreeprojection.Write("{0}({1})", ctreenode.NearestNeighbourArrayIndex,
                        rosetree.GetNodeByArrayIndex(ctreenode.NearestNeighbourArrayIndex).parent.MergeTreeIndex);
                    //docPairCnt++;
                }
            else
            {
                drawtreeprojection.Write("{0}", ctreenode.NearestNeighbourArrayIndex);
                //docNewTopicCnt++;
            }
            if (ctreenode.NearestNeighbourArrayIndex >= 0)
            {
                projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NearestNeighbourArrayIndex).data);
                drawtreeprojection.Write("\t{0}\t", projCosine);
            }
            else
                if (ctreenode.NewDocumentNearestNeighbourArrayIndex < LastTreeDocumentCount)
                {
                    projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).data);
                    drawtreeprojection.Write("\t{0}({1})\t{2}\t", ctreenode.NewDocumentNearestNeighbourArrayIndex, rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).parent.MergeTreeIndex,
                       projCosine);
                }
                else
                {
                    projCosine = GetCosineSimilarity(lfv.featurevectors[ctreenode.InitialIndex], rosetree.GetNodeByArrayIndex(ctreenode.NewDocumentNearestNeighbourArrayIndex).data);
                    drawtreeprojection.Write("\t{0}\t{1}\t", ctreenode.NewDocumentNearestNeighbourArrayIndex,
                        projCosine);
                }
            //leafCnt++;

#if !SINGLE_PROJECTION
            /// Multiple projection ///
            double projCosineCut = projCosine * MultipleProjectionFactor;
            SortedDictionary<CosineKey, RoseTreeNode> projectionCandidates = new SortedDictionary<CosineKey, RoseTreeNode>(new ReverseDoubleComparer()); 
            foreach (RoseTreeNode rosetreenode in rosetreenodes)
            {
                if (rosetreenode.MergeTreeIndex == ctreenode.NearestNeighbourArrayIndex ||
                    rosetreenode.MergeTreeIndex == ctreenode.NewDocumentNearestNeighbourArrayIndex)
                    continue;
                double cosine = GetCosineSimilarity(rosetreenode.data, lfv.featurevectors[ctreenode.InitialIndex]);
                if (cosine > projCosineCut)
                    projectionCandidates.Add(new CosineKey(cosine, rosetreenode.MergeTreeIndex), rosetreenode);
            }

            foreach (KeyValuePair<CosineKey, RoseTreeNode> kvp in projectionCandidates)
            {
                RoseTreeNode rosetreenode = kvp.Value;
                drawtreeprojection.Write(rosetreenode.MergeTreeIndex);
                if (rosetreenode.children == null)
                    drawtreeprojection.Write("({0})", rosetreenode.parent.MergeTreeIndex);
                drawtreeprojection.Write("\t" + kvp.Key.Cosine + "\t");
            }
#endif

            drawtreeprojection.WriteLine();
        }

        class ReverseDoubleComparer : IComparer<CosineKey>
        {

            public int Compare(CosineKey x, CosineKey y)
            {
                int res = y.Cosine.CompareTo(x.Cosine);
                if (res == 0)
                    res = x.Index.CompareTo(y.Index);
                return res;
            }
        }

        class CosineKey 
        {
            public readonly double Cosine;
            public readonly int Index;
            public CosineKey(double cosine, int index)
            {
                Cosine = cosine;
                Index = index;
            }
        }

        //double GetCosineSimilarity(SparseVectorList v0, SparseVectorList v1)
        //{
        //    //if (v0 == null)
        //    //    Console.WriteLine("v0 is null");
        //    //if (v1 == null)
        //    //    Console.WriteLine("v1 is null");
        //    return v0.Cosine(v0, v1);
        //}

        private Dictionary<int, KeyValuePair<Dictionary<int, double>, double>> GetNodeWeightedWordsDictionary(
            ConstraintTreeNode ctreenode, 
            Dictionary<int, Dictionary<int, KeyValuePair<Dictionary<int, double>, double>>> weightedSimilarity)
        {
            if (weightedSimilarity == null)
                return null;

            ConstraintTreeNode node = ctreenode;
            while (node.Parent != null)
            {
                if (weightedSimilarity.ContainsKey(node.Parent.InitialIndex))
                {
                    //similarityThresholds = new Dictionary<int, double>();
                    //foreach (KeyValuePair<int, KeyValuePair<Dictionary<int, double>, double>> kvp in weightedSimilarity[node.Parent.InitialIndex])
                    //{
                    //    similarityThresholds.Add(kvp.Key, kvp.Value.Value);
                    //    //Console.WriteLine("similarityThresholds: {0} {1}", kvp.Key, kvp.Value.Value);
                    //}
                    return weightedSimilarity[node.Parent.InitialIndex];
                }
                node = node.Parent;
            }
            return null;
        }

        static KeyValuePair<Dictionary<int, double>, double> GetNodeWeightedIndices(
            RoseTreeNode rtnode,
            Dictionary<int, KeyValuePair<Dictionary<int, double>, double>> weightedKeyWordsDictionary)
        {
            if (weightedKeyWordsDictionary == null)
                return new KeyValuePair<Dictionary<int, double>, double>(null, -1);

            RoseTreeNode node = rtnode;
            while (node.parent != null)
            {
                if (weightedKeyWordsDictionary.ContainsKey(node.parent.MergeTreeIndex))
                    return weightedKeyWordsDictionary[node.parent.MergeTreeIndex];
                node = node.parent;
            }

            return new KeyValuePair<Dictionary<int, double>, double>(null, -1);
        }

        private double GetNodeSimilarityThreshold(RoseTreeNode rtnode, ConstraintTreeNode ctreenode, Dictionary<int, double> similarityThresholds)
        {
            if (similarityThresholds == null)
                return -1;

            RoseTreeNode node = rtnode;
            while (node.parent != null)
            {
                if (similarityThresholds.ContainsKey(node.parent.MergeTreeIndex))
                {
                    double simiTh = similarityThresholds[node.parent.MergeTreeIndex];
                    List<int> currentRestrict = similarityThEffCurrentNodes[node.parent.MergeTreeIndex];
                    if (currentRestrict == null)
                        return simiTh;
                    else
                    {
                        ConstraintTreeNode cnode = ctreenode;
                        while(cnode.Parent!=null)
                        {
                            if (currentRestrict.Contains(cnode.Parent.InitialIndex))
                                return simiTh;
                            cnode = cnode.Parent;
                        }
                    }
                }
                node = node.parent;
            }

            return -1;
        }

        Dictionary<int, double> GetWeightedTermIndices(Dictionary<string, double> termWeights)
        {
            Dictionary<int, double> termIndexWeights = new Dictionary<int, double>();
            foreach (KeyValuePair<string, double> kvp in termWeights)
                termIndexWeights.Add(rosetree.lfv.lexicon[kvp.Key], kvp.Value);
            return termIndexWeights;
        }

        public double GetCosineSimilarity(SparseVectorList v0, SparseVectorList v1,
            Dictionary<int, double> termWeights = null)
        {
            if (termWeights == null)
                return v0.Cosine(v0, v1);

            Dictionary<int, int> prevTFs0 = new Dictionary<int, int>();
            Dictionary<int, int> prevTFs1 = new Dictionary<int, int>();

            double prevNorm0  = SetWeightedVector(v0, prevTFs0, termWeights);
            double prevNorm1 = SetWeightedVector(v1, prevTFs1, termWeights);
            //if (prevNorm0 != v0.normvalue && prevNorm1 != v1.normvalue)
            //    Console.Write("");

            double cosine = v0.Cosine(v0, v1);

            ReSetWeightedVector(v0, prevTFs0, prevNorm0);
            ReSetWeightedVector(v1, prevTFs1, prevNorm1);

            return cosine;

        }

        private double SetWeightedVector(SparseVectorList v, Dictionary<int, int> prevTFs, Dictionary<int, double> termWeights)
        {
            double prevnorm = v.normvalue;
            for (int i = 0; i < v.count; i++)
            {
                if (termWeights.ContainsKey(v.keyarray[i]))
                {
                    prevTFs.Add(i, v.valuearray[i]);
                    v.valuearray[i] = (int)(Math.Round(v.valuearray[i] * termWeights[v.keyarray[i]]));
                }
            }
            v.GetNormDCM();

            return prevnorm;
        }

        private void ReSetWeightedVector(SparseVectorList v, Dictionary<int, int> prevTFs, double prevNorm)
        {
            foreach (KeyValuePair<int, int> prevTF in prevTFs)
                v.valuearray[prevTF.Key] = prevTF.Value;

            v.normvalue = prevNorm;
        }

        void DrawInternalConstraintTree(string filename)
        {
            StreamWriter drawtree = new StreamWriter(filename);
            drawtree.WriteLine("digraph G \n {graph[ \n rankdir = \"TD\"];");

            //width-first traversal
            List<ConstraintTreeNode> cnodelist = new List<ConstraintTreeNode>();
            cnodelist.Add(this.Root);
            int treeindex = 0;
            while (cnodelist.Count != 0)
            {
                ConstraintTreeNode ctreenode = cnodelist[0];
                if (ctreenode.Children != null)
                    cnodelist.AddRange(ctreenode.Children);
                ctreenode.DrawTreeIndex = treeindex;
                if (ctreenode.Children != null)    //draw internal nodes only
                {
                    if (ctreenode.Parent != null)
                        drawtree.WriteLine(ctreenode.Parent.DrawTreeIndex + "->" + ctreenode.DrawTreeIndex);
                    DrawConstraintNode(drawtree, ctreenode);
                }
                cnodelist.RemoveAt(0);
                treeindex++;
            }

            drawtree.WriteLine("}");
            drawtree.Flush();
            drawtree.Close();
        }

        private void DrawConstraintNode(StreamWriter drawtree, ConstraintTreeNode node)
        {
            drawtree.Write(node.DrawTreeIndex + "[color = grey, label = \"");
            //drawtree.Write("-----data----- \\n");
            //drawtree.Write("TreeIndex=" + node.TreeIndex + "\\n");
            //drawtree.Write("[ " + node.TreeIndex + " ]\\n");
            //drawtree.Write(node.LeafNumber + "," + node.ChildLeafSquareSum + "\\n");

            // -- output merge order -- //
            if (node.InitialIndex >= 0)
            {
                drawtree.Write("-{0}-\\n", node.InitialIndex);
                if (bDrawLeafNumber)
                    drawtree.Write("({0})\\n", Math.Round(node.LeafNumber, 4));
            }
            //if (node.Children == null && node.InitialIndex != -1)
            //{
            //    SparseVectorList vector = this.lfv.featurevectors[node.InitialIndex];
            //    int maxindex = -1;
            //    int maxvalue = int.MinValue;
            //    int[] valuearray = vector.valuearray;
            //    for (int i = 0; i < valuearray.Length; i++)
            //    {
            //        if (maxvalue < valuearray[i])
            //        {
            //            maxindex = i;
            //            maxvalue = valuearray[i];
            //        }
            //    }
            //    int maxkey = vector.keyarray[maxindex];
            //    drawtree.Write(this.lfv.invertlexicon[maxkey] + "(" + maxvalue + ")\\n");
            //}

            //if (node.Children != null && node.ContainedInformation != null)
            if (node.ContainedInformation != null)
            {
                Dictionary<int, double> info = node.ContainedInformation;
                foreach (int treeindex in info.Keys)
                {
                    //if (!inheritParentInfo.TopicSize.ContainsKey(treeindex))
                    //    Console.Write("");
                    //if (info[treeindex] > 1.01)
                    //{
                    //    Console.WriteLine("Incorrect contained info!");
                    //    Console.WriteLine("Index = " + node.InitialIndex);
                    //    Console.WriteLine("Key = {0}, Value = {1}", treeindex, info[treeindex]);
                    //    Console.WriteLine("IsFreeNode = " + node.IsFreeNode);
                    //    Console.WriteLine("OriginalLinkedNodeIndex = " + node.OriginalLinkedNodeIndex);
                    //    Console.ReadKey();
                    //}
                    drawtree.Write("~{0} ({1},{2})~\\n", treeindex, info[treeindex],
                        info[treeindex] * inheritParentInfo.TopicSize[treeindex] / node.DocumentNumber);
                }
                //drawtree.Write("~{0} ({1})~\\n", treeindex, info[treeindex]);

            }
            else if (node.Children == null)
            {
                drawtree.Write("[ {0} ]", node.NearestNeighbourArrayIndex);
            }

            drawtree.WriteLine("\", shape=\"record\"];");
        }


        #region set draw tree weigthed similarity
        Dictionary<int, Dictionary<int, KeyValuePair<Dictionary<int, double>, double>>> weightedSimilarity = null;
        Dictionary<int, double> similarityThresholds = null;
        Dictionary<int, List<int>> similarityThEffCurrentNodes = new Dictionary<int, List<int>>();
        public void SetDrawTreeWeightedSimilarity(List<AdjustedFlowKeyWordsWeight> adjustedFlows)
        {
            weightedSimilarity = new Dictionary<int, Dictionary<int, KeyValuePair<Dictionary<int, double>, double>>>();
            similarityThresholds = new Dictionary<int, double>();

            //calculate key words
            foreach (AdjustedFlowKeyWordsWeight adjustedFlow in adjustedFlows)
            {
                if (!adjustedFlow.bonlysimilarity)
                {
                    List<int> enkeywords = GetEnhanceKeyWords(adjustedFlow);
                    Dictionary<int, double> weighedterms = new Dictionary<int, double>();
                    foreach (int enkeyword in enkeywords)
                        weighedterms.Add(enkeyword, adjustedFlow.enhanceweight);

                    KeyValuePair<Dictionary<int, double>, double> kvp = new KeyValuePair<Dictionary<int, double>, double>(weighedterms, adjustedFlow.similarityThreshold);
                    Dictionary<int, KeyValuePair<Dictionary<int, double>, double>> dict = new Dictionary<int, KeyValuePair<Dictionary<int, double>, double>>();
                    dict.Add(adjustedFlow.prevNodeIndex, kvp);

                    foreach (int presentindex in adjustedFlow.nodeindices)
                        weightedSimilarity.Add(presentindex, dict);

                    similarityThresholds.Add(adjustedFlow.prevNodeIndex, adjustedFlow.similarityThreshold);
                    similarityThEffCurrentNodes.Add(adjustedFlow.prevNodeIndex, null);
                }
                else
                {
                    similarityThresholds.Add(adjustedFlow.prevNodeIndex, adjustedFlow.similarityThreshold);
                    similarityThEffCurrentNodes.Add(adjustedFlow.prevNodeIndex, adjustedFlow.nodeindices.ToList<int>());
                }
            }
        }

        private List<int> GetEnhanceKeyWords(AdjustedFlowKeyWordsWeight adjustedFlow)
        {
            List<int> keywordsA = GetTopKeyWordIndices(rosetree, adjustedFlow.prevNodeIndex, adjustedFlow.filterKeyWordsK);

            List<int> keywordsB = null;
            foreach (int flowindex in adjustedFlow.nodeindices)
            {
                List<int> keywordsC = GetTopKeyWordIndices(constrainedRoseTree, flowindex, adjustedFlow.filterKeyWordsK);
                keywordsB = FindCombineKeyWordIndices(keywordsB, keywordsC);
            }

            List<int> keywords = SubstractKeyWordIndices(keywordsA, keywordsB);
            if (keywords.Count > adjustedFlow.effectiveKeyWordsK)
                keywords.RemoveRange(adjustedFlow.effectiveKeyWordsK, 
                    keywords.Count - adjustedFlow.effectiveKeyWordsK);

            //Console.WriteLine("Selected key words:");
            //Dictionary<int, string> invertedlexicon = (lfv as LoadGlobalFeatureVectors).invertlexicon;
            //foreach (int wordindex in keywords)
            //{
            //    Console.Write("{0}\t", invertedlexicon[wordindex]);
            //}
            //Console.WriteLine();

            return keywords;
        }

        private List<int> GetTopKeyWordIndices(RoseTree rosetree, int nodeIndex, int k)
        {
            SparseVectorList vector = rosetree.GetNodeByArrayIndex(nodeIndex).data;
            MinHeapInt mhd = new MinHeapInt(k);
            for (int i = 0; i < k; i++)
                mhd.insert(-1, int.MinValue);
            for (int iword = 0; iword < vector.count; iword++)
            {
                if (vector.valuearray[iword] > mhd.min())
                    mhd.changeMin(iword, vector.valuearray[iword]);
            }
            MinHeapInt.heapSort(mhd);
            int[] sortedIndices = mhd.getIndices();

            List<int> wordindices = new List<int>();
            foreach (int iword in sortedIndices)
                wordindices.Add(vector.keyarray[iword]);

            //Console.WriteLine();
            //Dictionary<int,string> invertedlexicon = (lfv as LoadGlobalFeatureVectors).invertlexicon;
            //foreach (int wordindex in wordindices)
            //{
            //    Console.Write("{0}\t", invertedlexicon[wordindex]);
            //}
            //Console.WriteLine();

            return wordindices;
        }

        private List<int> FindCombineKeyWordIndices(List<int> list0, List<int> list1)
        {
            if (list0 == null)
                return list1;
            if (list1 == null)
                return list0;
            List<int> list = new List<int>();
            foreach (int item0 in list0)
            {
                if (list1.Contains(item0))
                    list.Add(item0);
            }
            return list;
        }

        private List<int> SubstractKeyWordIndices(List<int> list0, List<int> list1)
        {
            List<int> list = new List<int>();

            foreach (int item0 in list0)
            {
                if (!list1.Contains(item0))
                    list.Add(item0);
            }

            return list;
        }
        #endregion

        #endregion draw tree

        #region open rose tree node
        bool IsUpdated = false;
        bool IsInitiallyOpened;
        public void UpdateConstraintTreeNodeOpened(SubRoseTree subrosetree, RoseTree ConstraintRoseTree,
            List<RoseTreeNode> widthTraversalNodeList, DataProjectionRelation projRelation)
        {
            if (ConstraintRoseTree != rosetree)
                throw new Exception("[UpdateConstraintTreeNodeOpened] Error! Incorrect input ConstraintRoseTree");
            //Update inheritinfo
            if (!IsInitiallyOpened && !IsUpdated)
                UpdateInheritInfo(projRelation);
            
            //Split node from constraint tree
            //if (widthTraversalNodeList.Count == 0)
            //    Console.Write("");
            ConstraintTreeNode subtreeconstraintnode = MergedTrees[widthTraversalNodeList[widthTraversalNodeList.Count-1]
                .children[0].MergeTreeIndex].Parent;
            //subtreeconstraintnode.IsOpenedNode = true;
            for (int pointer = widthTraversalNodeList.Count - 1; pointer >= 0; pointer--)
            {
                RoseTreeNode newtopicnode = widthTraversalNodeList[pointer];
                MergedTree mergedchildren = new MergedTree(newtopicnode.MergeTreeIndex);
                foreach (RoseTreeNode child in newtopicnode.children)
                    mergedchildren.AddNode(MergedTrees[child.MergeTreeIndex]);
                subtreeconstraintnode.SetActiveMergedTree(mergedchildren);
                ConstraintTreeNode newsplitnode = subtreeconstraintnode.Split(inheritParentInfo);
                newsplitnode.UpdateLeafNumbers();
                MergedTrees[newtopicnode.MergeTreeIndex] = newsplitnode;
                newsplitnode.InitialIndex = newtopicnode.MergeTreeIndex;
            }
        }

        void UpdateInheritInfo(DataProjectionRelation projRelation)
        {
            IList<RoseTreeNode> validnodes = rosetree.GetAllValidInternalTreeNodes();
            ConstraintTree openedConstraintTree = GetSucceedRelationConstraintTree(rosetree, this.lfv, projRelation);
            #region print differences 
            //List<int> newtopicindices = new List<int>();
            //foreach (int topicIndex in inheritParentInfo.TopicInheritParentInfos.Keys)
            //{
            //    KeyValuePair<int, double> info = inheritParentInfo.TopicInheritParentInfos[topicIndex];
            //    double topicsize = inheritParentInfo.TopicSize[topicIndex];
            //    if (openedConstraintTree.inheritParentInfo.TopicInheritParentInfos.ContainsKey(topicIndex))
            //    {
            //        KeyValuePair<int, double> info2 = openedConstraintTree.inheritParentInfo.TopicInheritParentInfos[topicIndex];
            //        double topicsize2 = openedConstraintTree.inheritParentInfo.TopicSize[topicIndex];
            //        if (info2.Key != info.Key || info2.Value != info.Value || topicsize2 != topicsize)
            //            Console.WriteLine("Topic-{0}-\t,<{1},{2}> {3} vs <{4},{5}> {6}", topicIndex,
            //                info.Key, info.Value, topicsize, info2.Key, info2.Value, topicsize2);
            //    }
            //    else
            //        Console.WriteLine("Topic index {0} not in new inheritParentInfo", topicIndex);
            //}
            //foreach (int topicIndex in openedConstraintTree.inheritParentInfo.TopicInheritParentInfos.Keys)
            //{
            //    if (!inheritParentInfo.TopicInheritParentInfos.ContainsKey(topicIndex))
            //    {
            //        KeyValuePair<int, double> info2 = openedConstraintTree.inheritParentInfo.TopicInheritParentInfos[topicIndex];
            //        double topicsize2 = openedConstraintTree.inheritParentInfo.TopicSize[topicIndex];
            //        Console.WriteLine("New Topic-{0}-\t <{1},{2}> {3}", topicIndex,
            //            info2.Key, info2.Value, topicsize2);
            //        newtopicindices.Add(topicIndex);
            //    }
            //}
            //for (int idoc = 0; idoc < lfv.featurevectors.Length; idoc++)
            //{
            //    if (this.inheritParentInfo.DocumentInheritParentInfo_Index[idoc] !=
            //        openedConstraintTree.inheritParentInfo.DocumentInheritParentInfo_Index[idoc] ||
            //        this.inheritParentInfo.DocumentInheritParentInfo_Weight[idoc] !=
            //        openedConstraintTree.inheritParentInfo.DocumentInheritParentInfo_Weight[idoc])
            //    {
            //        if (newtopicindices.Contains(openedConstraintTree.inheritParentInfo.DocumentInheritParentInfo_Index[idoc]))
            //            continue;
            //        Console.WriteLine("Doc-{0}-\t{1},{2} vs {3},{4}", idoc, this.inheritParentInfo.DocumentInheritParentInfo_Index[idoc],
            //            this.inheritParentInfo.DocumentInheritParentInfo_Weight[idoc],
            //            openedConstraintTree.inheritParentInfo.DocumentInheritParentInfo_Index[idoc],
            //            openedConstraintTree.inheritParentInfo.DocumentInheritParentInfo_Weight[idoc]);
            //    }
            //}
            #endregion  print differences 

            this.inheritParentInfo = openedConstraintTree.inheritParentInfo;
            IsUpdated = true;
            //foreach (RoseTreeNode validnode in validnodes)
            //{
            //    if (validnode.OpenedNode)
            //    {
            //        List<RoseTreeNode> queue = new List<RoseTreeNode>();
            //        queue.AddRange(validnode.children);
            //        while (queue.Count != 0)
            //        {
            //            RoseTreeNode updatenode = queue[0];
            //            queue.RemoveAt(0);

            //            if (updatenode.children != null)
            //            {

            //            }
            //            else
            //            {
            //            }
                        
            //            if (updatenode.children != null)
            //                queue.AddRange(updatenode.children);
            //        }
            //    }
            //}
        }

        public DataProjectionRelation GetDataProjectionRelation()
        {
            DataProjectionRelation dp = new DataProjectionRelation();
            int docCnt = this.lfv.featurevectors.Length;
            dp.IsFreenode = new bool[docCnt];
            dp.NearestNeighbourIndex = new int[docCnt];

            for (int idoc = 0; idoc < docCnt; idoc++)
            {
                ConstraintTreeNode cnode = MergedTrees[idoc];
                dp.IsFreenode[idoc] = cnode.IsFreeNode;
                if(cnode.IsFreeNode)
                    dp.NearestNeighbourIndex[idoc] = cnode.NewDocumentNearestNeighbourArrayIndex;
                else
                    dp.NearestNeighbourIndex[idoc] = cnode.NearestNeighbourArrayIndex;
            }

            dp.PrevDocCnt = rosetree.lfv.featurevectors.Length;
            return dp;
        }
        #endregion open rose tree node

        #region collapse too small clusters
        public void CollapseTooSmallCollapseNode(RoseTreeNode collapseNode)
        {
            ConstraintTreeNode cnode = MergedTrees[collapseNode.MergeTreeIndex];
            //if (cnode.Parent.InitialIndex == 389)
            //    Console.Write("");
            cnode.Parent.CollapseLinkWithChild(cnode);
            //foreach(int key in cnode.Parent.ContainedInformation)
            //    cnode.Parent.ContainedInformation[key]>1
        }

        public void UpdateCollapsedTooSmallClusters(Dictionary<RoseTreeNode, List<RoseTreeNode>> collapsednodes)
        {
            Dictionary<int, KeyValuePair<int, double>> collapsednewinfo = new Dictionary<int, KeyValuePair<int, double>>();
            foreach (KeyValuePair<RoseTreeNode, List<RoseTreeNode>> kvp in collapsednodes)
            {
                RoseTreeNode rosetreeparent = kvp.Key;
                foreach (RoseTreeNode collapsednode in kvp.Value)
                {
                    if (inheritParentInfo.TopicInheritParentInfos.ContainsKey(collapsednode.MergeTreeIndex))
                    {
                        int collapseindex = collapsednode.MergeTreeIndex;
                        KeyValuePair<int, double> parentinfo = inheritParentInfo.TopicInheritParentInfos[collapseindex];
                        if (parentinfo.Key == rosetreeparent.MergeTreeIndex)
                            collapsednewinfo.Add(collapseindex, parentinfo);
                        else
                        {
                            int parentindex = rosetreeparent.MergeTreeIndex;
                            collapsednewinfo.Add(collapseindex, new KeyValuePair<int, double>(parentindex, 1));
                            inheritParentInfo.TopicInheritParentInfos.Add(parentindex, parentinfo);
                            inheritParentInfo.TopicSize.Add(parentindex, inheritParentInfo.TopicSize[collapseindex]);
                        }
                        inheritParentInfo.TopicInheritParentInfos.Remove(collapseindex);
                        inheritParentInfo.TopicSize.Remove(collapseindex);
                    }
                    //int topicsize = inheritParentInfo.TopicSize[collapsednode.MergeTreeIndex];
                    //KeyValuePair<int, double> parentinfo0 = inheritParentInfo.TopicInheritParentInfos[collapsednode.MergeTreeIndex];
                    //KeyValuePair<int, double> parentinfo1 = inheritParentInfo.TopicInheritParentInfos[parentinfo0.Key];

                    //KeyValuePair<int, double> newparentinfo = new KeyValuePair<int, double>(parentinfo1.Key, parentinfo1.Value * parentinfo0.Value);
                    //inheritParentInfo.TopicInheritParentInfos[newparentinfo]
                }
            }

            /// Update ///
            List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            queue.Add(Root);
            while (queue.Count != 0)
            {
                ConstraintTreeNode cnode = queue[0];
                queue.RemoveAt(0);

                if (cnode.Children == null)
                {
                    if (cnode.NearestNeighbourArrayIndex >= 0)
                    {
                        if (collapsednewinfo.ContainsKey(cnode.NearestNeighbourArrayIndex))
                            cnode.NearestNeighbourArrayIndex = collapsednewinfo[cnode.NearestNeighbourArrayIndex].Key;
                    }
                    else
                    {
                        if (collapsednewinfo.ContainsKey(cnode.NewDocumentNearestNeighbourArrayIndex))
                            cnode.NewDocumentNearestNeighbourArrayIndex = collapsednewinfo[cnode.NewDocumentNearestNeighbourArrayIndex].Key;
                    }
                    continue;
                }

                foreach (KeyValuePair<int, KeyValuePair<int, double>> kvp in collapsednewinfo)
                {
                    if (cnode.OriginalLinkedNodeIndex == kvp.Key)
                        cnode.OriginalLinkedNodeIndex = -1;
                    if (cnode.ContainedInformation.ContainsKey(kvp.Key))
                    {
                        double info = cnode.ContainedInformation[kvp.Key] * kvp.Value.Value;
                        cnode.ContainedInformation.Remove(kvp.Key);
                        if (cnode.ContainedInformation.ContainsKey(kvp.Value.Key))
                        {
                            info += cnode.ContainedInformation[kvp.Value.Key];
                            if (info > 1) info = 1;
                            cnode.ContainedInformation[kvp.Value.Key] = info;
                        }
                        else
                        {
                            //if (info > 1)
                            //    Console.Write("");
                            cnode.ContainedInformation.Add(kvp.Value.Key, info);
                        }
                    }

                    //KeyValuePair<int,double> kvp in cnode.ContainedInformation
                }

                if (cnode.Children != null)
                    queue.AddRange(cnode.Children);
            }
        }
        #endregion collapse too small clusters

        #region update constraint tree adjusting structure
        public void UpdateConstraintTreeSplit(RoseTreeNode splitNode, bool bPutInEnd = false)
        {
            ConstraintTreeNode cParentNode = MergedTrees[splitNode.parent.MergeTreeIndex];
            MergedTree mergedchildren = new MergedTree(splitNode.MergeTreeIndex);
            foreach (RoseTreeNode child in splitNode.children)
                mergedchildren.AddNode(MergedTrees[child.MergeTreeIndex]);
            cParentNode.SetActiveMergedTree(mergedchildren);
            ConstraintTreeNode cSplitnode = cParentNode.Split(inheritParentInfo, bPutInEnd);
            cSplitnode.UpdateLeafNumbers();
            MergedTrees[splitNode.MergeTreeIndex] = cSplitnode;
            cSplitnode.InitialIndex = splitNode.MergeTreeIndex;
        }

        public void RemoveDocuments(RoseTreeNode removeNode, IList<RoseTreeNode> documents)
        {
            ConstraintTreeNode cRemoveNode = MergedTrees[removeNode.MergeTreeIndex];

            //remove documents from the constraint tree
            int cnt = 0;
            for (int index = 0; index < cRemoveNode.Children.Count; index++)
            {
                if (cRemoveNode.Children[index].Children == null)
                {
                    cRemoveNode.Children.RemoveAt(index);
                    index--;
                    cnt++;
                }
            }
            if (cnt != documents.Count)
                throw new Exception("Error when RemoveDocuments in the tree!");

            //remove contained information
            Dictionary<int, double> info = null;

            while (cRemoveNode != null && cRemoveNode.OriginalLinkedNodeIndex > 0)
            {
                if (info == null)
                {
                    info = new Dictionary<int, double>();
                    foreach (RoseTreeNode document in documents)
                    {
                        int docIndex = document.MergeTreeIndex;
                        int addIndex = inheritParentInfo.DocumentInheritParentInfo_Index[docIndex];
                        if (addIndex == 0)
                            continue;
                        if (info.ContainsKey(addIndex))
                            info[addIndex] += inheritParentInfo.DocumentInheritParentInfo_Weight[docIndex];
                        else
                            info.Add(addIndex, inheritParentInfo.DocumentInheritParentInfo_Weight[docIndex]);
                    }
                }
                else
                {
                    Dictionary<int, double> parentInfo = new Dictionary<int, double>();
                    foreach (KeyValuePair<int, double> kvp in info)
                    {
                        KeyValuePair<int, double> parentTopicInfo = inheritParentInfo.TopicInheritParentInfos[kvp.Key];
                        if (parentInfo.ContainsKey(parentTopicInfo.Key))
                            parentInfo[parentTopicInfo.Key] += parentTopicInfo.Value * kvp.Value;
                        else
                            parentInfo.Add(parentTopicInfo.Key, parentTopicInfo.Value * kvp.Value);
                    }
                    info = parentInfo;
                }

                cRemoveNode.RemoveContainedInformation(info);
                cRemoveNode = cRemoveNode.Parent;
            }

            foreach (RoseTreeNode document in documents)
                MergedTrees[document.MergeTreeIndex] = null;
        }

        #endregion update constraint tree adjusting structure

        //#region test data projection
        //public void OutputDataProjectionResult(string filename)
        //{
        //    StreamWriter ofile = new StreamWriter(filename);

        //    GroundTruthMergeOrder gtlabel0 = new GroundTruthMergeOrder(rosetree.lfv);
        //    GroundTruthMergeOrder gtlabel1 = new GroundTruthMergeOrder(lfv);

        //    int label0_1, label0_2, label1_1, label1_2;
        //    int correctcntlevel1 = 0, correctcntlevel2 = 0;

        //    SparseVectorList[] featurevectors = lfv.featurevectors;
        //    for (int i = 0; i < featurevectors.Length; i++)
        //    {
        //        SparseVectorList vector = featurevectors[i];
        //        //int nnarrayindex = Constraint.FindNearestNeighbour(vector, rosetree, rosetreeleaves);
        //        int nnarrayindex = dataprojection.GetProjectedArrayIndex(vector);
        //        ofile.WriteLine("{0}->{1}", i, nnarrayindex);

        //        gtlabel0.GetLabel(nnarrayindex, out label0_1, out label0_2);
        //        gtlabel1.GetLabel(i, out label1_1, out label1_2);
        //        if (label0_1 == label1_1)
        //            correctcntlevel1++;
        //        if (label0_2 == label1_2)
        //            correctcntlevel2++;
        //    }
        //    ofile.WriteLine("Level 1 correct ratio: {0}", (double)correctcntlevel1 / featurevectors.Length);
        //    ofile.WriteLine("Level 2 correct ratio: {0}", (double)correctcntlevel2 / featurevectors.Length);

        //    ofile.Close();
        //}
        //#endregion test data projection

        protected ConstraintTreeNode GetCommonAncestor(ConstraintTreeNode node0, ConstraintTreeNode node1)
        {
            List<ConstraintTreeNode> ancestorlist = new List<ConstraintTreeNode>();
            ConstraintTreeNode ancestor = node0;
            ancestorlist.Add(node0);
            while (ancestor.Parent != null)
            {
                ancestorlist.Add(ancestor.Parent);
                ancestor = ancestor.Parent;
            }

            ancestor = node1;
            while (true)
            {
                if (ancestor == null)
                    throw new Exception("no common ancestor!");
                if (ancestorlist.Contains(ancestor))
                    return ancestor;
                ancestor = ancestor.Parent;
            }
        }

        public IList<ConstraintTreeNode> GetAllTreeLeaves()
        {
            return this.MergedTrees.ToList<ConstraintTreeNode>().
                GetRange(0, this.MergedTrees.Length / 2).AsReadOnly();
        }

        public int GetLeafCount()
        {
            if (this.MergedTrees.Length != 2 * lfv.featurevectors.Length)
                throw new Exception("Unexpected!");
            return this.MergedTrees.Length / 2;
        }

        public ConstraintTreeNode GetLeafByInitialIndex(int initialindex)
        {
            ConstraintTreeNode node = MergedTrees[initialindex];
            //if (node.InitialIndex != initialindex)
            //    throw new Exception("");
            return node;
        }

        public ConstraintTreeNode GetNodeByMergeIndex(int mergetreeindex)
        {
            return MergedTrees[mergetreeindex];
        }

        public List<ConstraintTreeNode> GetSubTreeLeaves(ConstraintTreeNode node)
        {
            List<ConstraintTreeNode> leafnodes = new List<ConstraintTreeNode>();

            List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            queue.Add(node);

            while (queue.Count > 0)
            {
                ConstraintTreeNode cnode = queue[0];
                queue.RemoveAt(0);

                if (cnode.Children == null || cnode.Children.Count == 0)
                    leafnodes.Add(cnode);
                else
                    queue.AddRange(cnode.Children);
            }

            return leafnodes;
        }

        #region for loose order constraint inherit
        protected virtual ConstraintTreeNode NewConstraintNode()
        {
            return new ConstraintTreeNode();
        }

        protected virtual ConstraintTreeNode CreateFreeConstraintNode()
        {
            return new ConstraintTreeNode(true);
        }

        protected virtual bool SplitFree(ConstraintTreeNode node, int mergetreeindex)
        {
            return false;
        }

        protected virtual ConstraintTree GetSucceedRelationConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv, DataProjectionRelation projRelation)
        {
            return new SucceedRelationConstraintTree(rosetree, this.lfv, projRelation);
        }

        protected virtual DataProjection InitializeDataProjection(RoseTree rosetree, LoadFeatureVectors lfv)
        {
            return Constraint.InitializeDataProjection(rosetree, this.lfv);
        }
        #endregion


        

    }

    class DataProjectionRelation
    {
        public int[] NearestNeighbourIndex; //original mergetreeindex in the rose tree
        public bool[] IsFreenode;           //if abandoned
        public int PrevDocCnt;
    }

    class AdjustedFlowKeyWordsWeight
    {
        public int prevNodeIndex = 0;
        public int[] nodeindices = null;
        public double similarityThreshold = 0;
        public double enhanceweight = 5;
        public int filterKeyWordsK = 100;
        public int effectiveKeyWordsK = 20;
        public bool bonlysimilarity = false;
    }
}
