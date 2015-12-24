using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;

namespace EvolutionaryRoseTree.Constraints
{
    class TreeDistanceConstraint : Constraint
    {
        public readonly double PunishWeight = 1;

        public int[][] ConsTreeDistance;
        public readonly TreeDistanceType DistanceType;
        RoseTree rosetree;
        LoadFeatureVectors lfv;
        public int NotFreeConstraintTreeLeafCount { get; protected set; }

        public bool bUpdateTreeLeafDepth = true;
        //public int AbandonNodeCount { get { return lfv.featurevectors.Length - NotFreeConstraintTreeLeafCount; } }

//#if NORMALIZED_SMOOTHNESS_COST
        public override double NormalizedSmoothnessCost { get { return SmoothnessCost / Math.Pow(NotFreeConstraintTreeLeafCount, 2); } }
//#else
//        public override double NormalizedSmoothnessCost { get { return SmoothnessCost; } }
//#endif
        public TreeDistanceConstraint(RoseTree rosetree,
            LoadFeatureVectors lfv, TreeDistanceType distanceType, double punishweight) :
            base(ConstraintType.TreeDistance)
        {
            this.rosetree = rosetree;
            this.lfv = lfv;
            this.DistanceType = distanceType;
            this.PunishWeight = punishweight;

            SetConstrainedTreeDistance();
        }

        bool isRemoveConstraint = false;
        //bool isTestSmoothnessZero = false;
        #region remove conflicts
        public void RemoveConflicts(ConstraintTree constraintTree)
        {
            //isTestSmoothnessZero = true;
            isRemoveConstraint = true;
            SetConstrainedTreeDistance(constraintTree);
            isRemoveConstraint = false;

            foreach (var array in ConsTreeDistance)
                foreach (var number in array)
                    if (number > 0)
                        Console.WriteLine("");

            Console.WriteLine(isRemoveConstraint);
        }
        #endregion

        #region get proper probability
        public override double GetLogJoinTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            switch (DistanceType)
            {
                case TreeDistanceType.Sum:
                    //distance between two nodes +2
                    return GetSumDistanceLogMergeProbability(node0, node1, 2);
                case TreeDistanceType.Max:
                    return GetMaxDistanceLogMergeProbability(node0, node1, 1, 1);
                default:
                    return 0;
            }
        }

        //node0 is collapsed
        public override double GetLogAbsorbTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            switch (DistanceType)
            {
                case TreeDistanceType.Sum:
                    //distance between two nodes +1
                    return GetSumDistanceLogMergeProbability(node0, node1, 1);
                case TreeDistanceType.Max:
                    return GetMaxDistanceLogMergeProbability(node0, node1, 0, 1);
                default:
                    return 0;
            }
        }

        public override double GetLogCollapseTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            switch (DistanceType)
            {
                case TreeDistanceType.Sum:
                    //distance between two nodes +0
                    return GetSumDistanceLogMergeProbability(node0, node1, 0);
                case TreeDistanceType.Max:
                    return GetMaxDistanceLogMergeProbability(node0, node1, 0, 0);
                default:
                    return 0;
            }
        }

        private double GetSumDistanceLogMergeProbability(RoseTreeNode node0, RoseTreeNode node1, int AddedDistance)
        {
            double logprob = 0;
            int deltadistance;
            List<RoseTreeNode> leaves0 = RoseTree.GetSubTreeLeaf(node0);
            List<RoseTreeNode> leaves1 = RoseTree.GetSubTreeLeaf(node1);

            foreach (RoseTreeNode leaf0 in leaves0)
                foreach (RoseTreeNode leaf1 in leaves1)
                {
                    int index0 = leaf0.indices.initial_index;
                    int index1 = leaf1.indices.initial_index;
                    if (index0 > index1)
                    {
                        int temp = index0;
                        index0 = index1;
                        index1 = temp;
                    }

                    int constreedis = ConsTreeDistance[index0][index1 - index0];
                    if (constreedis >= 0)
                        deltadistance = constreedis - 
                            (leaf0.DepthInTree + leaf1.DepthInTree + AddedDistance);
                    else
                        deltadistance = 0;
#if !DISTANCE_CONSTRAINT_2
                    logprob -= PunishWeight * deltadistance * deltadistance;
#else
                    logprob -= PunishWeight * deltadistance;
#endif
                }

            //return logprob;
            return logprob / node0.LeafCount / node1.LeafCount;
        }

        private double GetMaxDistanceLogMergeProbability(RoseTreeNode node0, RoseTreeNode node1,
            int AddedDistance0, int AddedDistance1)
        {
            double logprob = 0;
            double deltadistance;
            List<RoseTreeNode> leaves0 = RoseTree.GetSubTreeLeaf(node0);
            List<RoseTreeNode> leaves1 = RoseTree.GetSubTreeLeaf(node1);

            foreach (RoseTreeNode leaf0 in leaves0)
                foreach (RoseTreeNode leaf1 in leaves1)
                {
                    int index0 = leaf0.indices.initial_index;
                    int index1 = leaf1.indices.initial_index;
                    if (index0 > index1)
                    {
                        int temp = index0;
                        index0 = index1;
                        index1 = temp;
                    }
                    
                    int constreedis = ConsTreeDistance[index0][index1 - index0];
                    if (constreedis >= 0)
                        deltadistance = constreedis -
                        Math.Max(leaf0.DepthInTree + AddedDistance0, leaf1.DepthInTree + AddedDistance1);
                    else
                        deltadistance = 0;
#if !DISTANCE_CONSTRAINT_2
                    logprob -= PunishWeight * deltadistance * deltadistance;
#else
                    logprob -= PunishWeight * deltadistance;
#endif
                }

            //return logprob;
            return logprob / node0.LeafCount / node1.LeafCount;
        }

        #region smoothness cost
        private double GetJoinDistanceTotalCost(RoseTreeNode node0, RoseTreeNode node1)
        {
            switch (DistanceType)
            {
                case TreeDistanceType.Sum:
                    //distance between two nodes +0
                    return GetSumDistanceTotalCost(node0, node1, 2);
                case TreeDistanceType.Max:
                    return GetMaxDistanceTotalCost(node0, node1, 1, 1);
                default:
                    return 0;
            }
        }

        private double GetAbsorbDistanceTotalCost(RoseTreeNode node0, RoseTreeNode node1)
        {
            switch (DistanceType)
            {
                case TreeDistanceType.Sum:
                    //distance between two nodes +0
                    return GetSumDistanceTotalCost(node0, node1, 1);
                case TreeDistanceType.Max:
                    return GetMaxDistanceTotalCost(node0, node1, 0, 1);
                default:
                    return 0;
            }
        }

        private double GetCollapseDistanceTotalCost(RoseTreeNode node0, RoseTreeNode node1)
        {
            switch (DistanceType)
            {
                case TreeDistanceType.Sum:
                    //distance between two nodes +0
                    return GetSumDistanceTotalCost(node0, node1, 0);
                case TreeDistanceType.Max:
                    return GetMaxDistanceTotalCost(node0, node1, 0, 0);
                default:
                    return 0;
            }
        }

        private double GetSumDistanceTotalCost(RoseTreeNode node0, RoseTreeNode node1, int AddedDistance)
        {
            double cost = 0;
            int deltadistance;
            List<RoseTreeNode> leaves0 = RoseTree.GetSubTreeLeaf(node0);
            List<RoseTreeNode> leaves1 = RoseTree.GetSubTreeLeaf(node1);

            foreach (RoseTreeNode leaf0 in leaves0)
                foreach (RoseTreeNode leaf1 in leaves1)
                {
                    int index0 = leaf0.indices.initial_index;
                    int index1 = leaf1.indices.initial_index;
                    if (index0 > index1)
                    {
                        int temp = index0;
                        index0 = index1;
                        index1 = temp;
                    }

                    int constreedis = ConsTreeDistance[index0][index1 - index0];
                    if (constreedis >= 0)
                        deltadistance = constreedis
                        - (leaf0.DepthInTree + leaf1.DepthInTree + AddedDistance);
                    else
                        deltadistance = 0;
                    cost += deltadistance * deltadistance;
                }

            return cost;
        }

        private double GetMaxDistanceTotalCost(RoseTreeNode node0, RoseTreeNode node1,
            int AddedDistance0, int AddedDistance1)
        {
            double cost = 0;
            double deltadistance;
            List<RoseTreeNode> leaves0 = RoseTree.GetSubTreeLeaf(node0);
            List<RoseTreeNode> leaves1 = RoseTree.GetSubTreeLeaf(node1);

            foreach (RoseTreeNode leaf0 in leaves0)
                foreach (RoseTreeNode leaf1 in leaves1)
                {
                    int index0 = leaf0.indices.initial_index;
                    int index1 = leaf1.indices.initial_index;
                    if (index0 > index1)
                    {
                        int temp = index0;
                        index0 = index1;
                        index1 = temp;
                    }

                    int constreedis = ConsTreeDistance[index0][index1 - index0];
                    if (constreedis >= 0)
                        deltadistance = constreedis -
                        Math.Max(leaf0.DepthInTree + AddedDistance0, leaf1.DepthInTree + AddedDistance1);
                    else
                        deltadistance = 0;
                    cost += deltadistance * deltadistance;
                }

            return cost;
        }
        #endregion smoothness cost

        public override double GetLogTreeProbabilityFromRatio(double log_treeprobability_ratio, RoseTreeNode node0, RoseTreeNode node1)
        {
            return log_treeprobability_ratio + node0.LogTreeProbability + node1.LogTreeProbability;
        }
        #endregion get proper probability

        #region on merge two trees
        public override void MergeTwoTrees(RoseTreeNode node0, RoseTreeNode node1, MergeType mergetype)
        {
            //Calculate Smoothness Cost
            double cost;
            switch (mergetype)
            {
                case MergeType.Join:
                    cost = GetJoinDistanceTotalCost(node0, node1);
                    break;
                case MergeType.AbsorbL:
                    cost = GetAbsorbDistanceTotalCost(node0, node1);
                    break;
                case MergeType.AbsorbR:
                    cost = GetAbsorbDistanceTotalCost(node1, node0);
                    break;
                default:    //collapse
                    cost = GetCollapseDistanceTotalCost(node0, node1);
                    break;
            }
            SmoothnessCost += -cost;
            //if (isTestSmoothnessZero && cost != 0)
            //{
            //    Console.WriteLine("Distance smoothness error!");
            //    Console.ReadKey();
            //}
            //Update depth in tree
            if (bUpdateTreeLeafDepth)
            {
                switch (mergetype)
                {
                    case MergeType.Join:
                        IncreaseLeafDepth(node0);
                        IncreaseLeafDepth(node1);
                        break;
                    case MergeType.AbsorbL:
                        IncreaseLeafDepth(node1);
                        break;
                    case MergeType.AbsorbR:
                        IncreaseLeafDepth(node0);
                        break;
                    case MergeType.Collapse:
                        break;
                }
            }
        }

        public void IncreaseLeafDepth(RoseTreeNode treenode)
        {
            List<RoseTreeNode> leaves = RoseTree.GetSubTreeLeaf(treenode);
            foreach (RoseTreeNode leaf in leaves)
                leaf.DepthInTree++;
        }

        #endregion on merge two trees

        #region intialize constraints
        ///  project data to previous built rose tree, calculate present constrained tree distance
        /// <param name="rosetree">previous built rose tree</param>
        /// <param name="lfv">newly arrived data</param>
        /// <returns>constrained tree distance</returns>
        private void SetConstrainedTreeDistance(ConstraintTree constraintTree = null)
        {
            ConsTreeDistance = new int[lfv.featurevectors.Length][];

            if(constraintTree == null)
                constraintTree = new ConstraintTree(rosetree, lfv);
            SparseVectorList[] vectors = lfv.featurevectors;
            //cache calculated tree distance
            Dictionary<int, Dictionary<int, int>> prevTreeDistance = new Dictionary<int, Dictionary<int, int>>();
 
            //build constraint
            for (int i = 0; i < vectors.Length; i++)
            {
                ConsTreeDistance[i] = new int[vectors.Length - i];
                ConsTreeDistance[i][0] = 0;
                for (int j = i + 1; j < vectors.Length; j++)
                {
                    ConstraintTreeNode cnode0 = constraintTree.GetLeafByInitialIndex(i);
                    ConstraintTreeNode cnode1 = constraintTree.GetLeafByInitialIndex(j);

                    ConsTreeDistance[i][j - i] = CalculateTreeDistance(cnode0, cnode1);
                }
            }

            NotFreeConstraintTreeLeafCount = constraintTree.NotFreeConstraintTreeLeafCount;
            //PrintConsTreeDistance();
        }

        // This one cannot ensure 0 for order constraint with maximum punish weight: 
        // perhaps the tree will contain nodes with only one internal node as child
        private void SetConstrainedTreeDistance_Org()
        {
            ConsTreeDistance = new int[lfv.featurevectors.Length][];

            #region initialization
            //cache calculated tree distance
            Dictionary<int, Dictionary<int, int>> prevTreeDistance = new Dictionary<int, Dictionary<int, int>>();
            //map current data to the nearest neighbour of previous data
            Dictionary<int, int> nearestNeighborMap = new Dictionary<int, int>();
            //tree leaves
            IList<RoseTreeNode> rosetreeleaves = rosetree.GetAllTreeLeaf();
            #endregion initialization

            //find nearest neighbour
            DataProjection dataprojection = InitializeDataProjection(rosetree, this.lfv);
            SparseVectorList[] vectors = lfv.featurevectors;
            for (int i = 0; i < vectors.Length; i++)
            {
                //int nearestneighbourindex = FindNearestNeighbour(vectors[i], rosetree, rosetreeleaves);
                NodeProjectionType projType;
                int nearestneighbourindex = dataprojection.GetProjectedArrayIndex(vectors[i], out projType);
                switch (projType)
                {
                    case NodeProjectionType.Cousin:
                        nearestNeighborMap.Add(i, nearestneighbourindex);
                        break;
                    case NodeProjectionType.InCluster:
                        nearestNeighborMap.Add(i, rosetree.GetNodeByArrayIndex(nearestneighbourindex).children[0].indices.array_index);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            //build constraint
            for (int i = 0; i < vectors.Length; i++)
            {
                ConsTreeDistance[i] = new int[vectors.Length - i];
                ConsTreeDistance[i][0] = 0;
                for (int j = i + 1; j < vectors.Length; j++)
                {
                    int nnidx0 = nearestNeighborMap[i];
                    int nnidx1 = nearestNeighborMap[j];
                    int distance;
                    if (nnidx0 > nnidx1)
                    {
                        int temp = nnidx0;
                        nnidx0 = nnidx1;
                        nnidx1 = temp;
                    }

                    if (prevTreeDistance.ContainsKey(nnidx0) && prevTreeDistance[nnidx0].ContainsKey(nnidx1))
                        distance = prevTreeDistance[nnidx0][nnidx1];
                    else
                    {
                        distance = CalculateTreeDistance(rosetreeleaves[nnidx0], rosetreeleaves[nnidx1]);
                        if (!prevTreeDistance.ContainsKey(nnidx0))
                            prevTreeDistance.Add(nnidx0, new Dictionary<int, int>());
                        prevTreeDistance[nnidx0].Add(nnidx1, distance);
                    }

                    ConsTreeDistance[i][j - i] = distance;
                }

            }

            //PrintConsTreeDistance();
        }

        private void PrintConsTreeDistance()
        {
            for (int i = 0; i < ConsTreeDistance.Length; i++)
            {
                for (int j = 0; j < ConsTreeDistance[i].Length; j++)
                    Console.Write(ConsTreeDistance[i][j] + "\t");
                Console.WriteLine();
            }
        }

        private int CalculateTreeDistance(RoseTreeNode treenode0, RoseTreeNode treenode1)
        {
            int cadistance0 = 1, cadistance1 = 1;   //distance to common ancestor

            List<RoseTreeNode> ancestorlist = new List<RoseTreeNode>();
            RoseTreeNode ancestor = treenode0;
            while (ancestor.parent != null)
            {
                ancestorlist.Add(ancestor.parent);
                ancestor = ancestor.parent;
            }

            ancestor = treenode1.parent;
            while (true)
            {
                if (ancestor == null)
                    throw new Exception("no common ancestor!");
                if (ancestorlist.Contains(ancestor))
                {
                    cadistance0 = 1 + ancestorlist.IndexOf(ancestor);
                    break;
                }
                cadistance1++;
                ancestor = ancestor.parent;
            }

            return DistanceType == TreeDistanceType.Sum ?
                (cadistance0 + cadistance1) : Math.Max(cadistance0, cadistance1);
        }

        private int CalculateTreeDistance(ConstraintTreeNode treenode0, ConstraintTreeNode treenode1)
        {
            if (treenode0.IsFreeNode || treenode1.IsFreeNode)
                return -1;

            int cadistance0 = 1, cadistance1 = 1;   //distance to common ancestor

            List<ConstraintTreeNode> ancestorlist = new List<ConstraintTreeNode>();
            ConstraintTreeNode ancestor = treenode0;
            while (ancestor.Parent != null)
            {
                ancestorlist.Add(ancestor.Parent);
                ancestor = ancestor.Parent;
            }

            ancestor = treenode1.Parent;
            while (true)
            {
                if (ancestor == null)
                {
                    //throw new Exception("no common ancestor!");
                    return -1;   //free documents
                }
                if (ancestorlist.Contains(ancestor))
                {
                    cadistance0 = 1 + ancestorlist.IndexOf(ancestor);
                    break;
                }
                cadistance1++;
                ancestor = ancestor.Parent;
            }

            return DistanceType == TreeDistanceType.Sum ?
                (cadistance0 + cadistance1) : Math.Max(cadistance0, cadistance1);
        }

        #endregion intialize constraints

    }
}
