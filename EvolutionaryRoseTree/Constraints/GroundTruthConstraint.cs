using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;

namespace EvolutionaryRoseTree.Constraints
{
    //Use to build a ground truth rose tree (20NewsGroup)
    class GroundTruthConstraint : Constraint
    {
        LoadFeatureVectors lfv;

        int[] labels0;  //first level labels
        int[] labels1;  //second level labels
        Dictionary<string, int> labelHash0;
        Dictionary<string, int> labelHash1;
        Dictionary<int, int> label1to0Hash;
        Dictionary<int, int> label0cntHash;
        Dictionary<int, int> label1cntHash;

        NodeMergeCondition[] nodeMergeConditions;

        public GroundTruthConstraint(LoadFeatureVectors lfv)
            : base(ConstraintType.GroundTruth)
        {
            this.lfv = lfv;

            InitializeLabels();
            nodeMergeConditions = new NodeMergeCondition[2 * labels1.Length];
        }

        #region intialize
        private void InitializeLabels()
        {
            lfv.GetSampleLabels(out labels1, out labelHash1);

            int sampledNumber = labels1.Length;
            if (lfv.featurevectors.Length != sampledNumber)
                throw new Exception("Sample number not match!");


            labels0 = new int[sampledNumber];
            labelHash0 = new Dictionary<string, int>();
            label1to0Hash = new Dictionary<int, int>();
            int label0cnt = 0;
            foreach (string fulllabel in labelHash1.Keys)
            {
                string prefixlabel = fulllabel.Split('.')[0];
                if (!labelHash0.ContainsKey(prefixlabel))
                {
                    labelHash0.Add(prefixlabel, label0cnt);
                    label0cnt++;
                }
                label1to0Hash.Add(labelHash1[fulllabel], labelHash0[prefixlabel]);
            }
            //remove label1s that does not contain in sampled data
            HashSet<int> labellist = new HashSet<int>();
            for (int i = 0; i < sampledNumber; i++)
                labellist.Add(labels1[i]);
            Dictionary<int, int> label1to0Hashbuffer = new Dictionary<int, int>();
            foreach (int label1 in labellist)
                label1to0Hashbuffer.Add(label1, label1to0Hash[label1]);
            label1to0Hash = label1to0Hashbuffer;

            //initialize label0
            for (int i = 0; i < sampledNumber; i++)
                labels0[i] = label1to0Hash[labels1[i]];

            //cnt correspontding labels
            label0cntHash = new Dictionary<int, int>();
            foreach (int label1 in label1to0Hash.Keys)
            {
                int label0 = label1to0Hash[label1];
                if (label0cntHash.ContainsKey(label0))
                    label0cntHash[label0]++;
                else
                    label0cntHash.Add(label0, 1);
            }

            label1cntHash = new Dictionary<int, int>();
            for (int i = 0; i < sampledNumber; i++)
            {
                int label1 = labels1[i];

                if (label1cntHash.ContainsKey(label1))
                    label1cntHash[label1]++;
                else
                    label1cntHash.Add(label1, 1);
            }
        }
        #endregion intialize

        #region get proper probability
        public override double GetLogJoinTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            int level0, level1, label0, label1;
            if (CanIncreaseLevel(node0, out level0, out label0) &&
                CanIncreaseLevel(node1, out level1, out label1))
            {
                if (level0 == level1)
                {
                    if (level0 == 1 && label0 == label1 ||
                        level0 == 2 && label1to0Hash[label0] == label1to0Hash[label1] ||
                        level0 == 3)
                        return 0;
                }
            }

            return Double.MinValue;
        }

        public override double GetLogAbsorbTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            int level0, level1, label0, label1;
            if (!CanIncreaseLevel(node0, out level0, out label0) &&
                CanIncreaseLevel(node1, out level1, out label1))
            {
                if (level0 == level1 + 1)
                {
                    if (level0 == 2 && label0 == label1 ||
                        level0 == 3 && label0 == label1to0Hash[label1] ||
                        level0 == 4)
                        return 0;
                }
            }

            return Double.MinValue;
        }

        public override double GetLogCollapseTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            int level0, level1, label0, label1;
            if (!CanIncreaseLevel(node0, out level0, out label0) &&
                !CanIncreaseLevel(node1, out level1, out label1))
            {
                if (level0 == level1 && label0 == label1)
                    return 0;
            }

            return Double.MinValue;
        }

        private bool CanIncreaseLevel(RoseTreeNode node, out int level, out int label)
        {
            NodeMergeCondition mergecondition = nodeMergeConditions[node.MergeTreeIndex];

            if (mergecondition == null)
            {
                level = node.tree_depth;

                NodeMergeCondition childmergecondi;
                switch (node.tree_depth)
                {
                    case 1:
                        label = labels1[node.indices.initial_index];
                        if (label1cntHash[label] == 1)
                            if (label0cntHash[label1to0Hash[label]] > 1)
                                level = 2;
                            else
                            {
                                level = 3;
                                label = label1to0Hash[label];
                            }
                        break;
                    case 2:
                        childmergecondi = nodeMergeConditions[node.children[0].MergeTreeIndex];
                        level = childmergecondi.Level + 1;
                        label = childmergecondi.Label;
                        if (level == 3)
                            label = label1to0Hash[label];
                        else if (level == 4)
                            label = 0;
                        break;
                    case 3:
                        childmergecondi = nodeMergeConditions[node.children[0].MergeTreeIndex];
                        level = childmergecondi.Level + 1;
                        if (level == 3)
                            label = label1to0Hash[childmergecondi.Label];
                        else
                            label = 0;
                        break;
                    default:    //case 4
                        label = 0;
                        break;
                }

                bool bincrease;
                switch (level)
                {
                    case 1:
                        bincrease = true;
                        break;
                    case 2:
                        if (label1cntHash[label] == 1)
                            bincrease = true;
                        else if (label1cntHash[label] == node.children.Length)
                        {
                            if (label0cntHash[label1to0Hash[label]] == 1)
                            {
                                level = 3;
                                label = label1to0Hash[label];
                            }
                            bincrease=true;
                        }
                        else
                            bincrease=false;
                        break;
                    case 3:
                        bincrease = label0cntHash[label] == 1 ||
                            label0cntHash[label] == node.children.Length;
                        break;
                    default:    //case 4
                        bincrease = false;
                        break;
                }

                mergecondition = new NodeMergeCondition(bincrease, level, label);
                nodeMergeConditions[node.MergeTreeIndex] = mergecondition;
            }

            level = mergecondition.Level;
            label = mergecondition.Label;
            return mergecondition.BCanIncrease;
        }

        //private bool CanIncreaseLevel(RoseTreeNode node, out int level, out int label)
        //{
        //    bool bincrease = false;
        //    level = node.tree_depth;

        //    switch (node.tree_depth)
        //    {
        //        case 1:
        //            bincrease = true;
        //            label = labels1[node.indices.initial_index];
        //            if (label1cntHash[label] == 1)
        //                if (label0cntHash[label1to0Hash[label]] > 1)
        //                    level = 2;
        //                else
        //                {
        //                    level = 3;
        //                    label = label1to0Hash[label];
        //                }
        //            break;
        //        case 2:
        //            label = labels1[node.children[0].indices.initial_index];
        //            if (node.children.Length == label1cntHash[label])
        //            {
        //                bincrease = true;
        //                if (label0cntHash[label1to0Hash[label]] == 1)
        //                {
        //                    level = 3;
        //                    label = label1to0Hash[label];
        //                }
        //            }
        //            break;
        //        case 3:
        //            label = labels1[node.children[0].children[0].indices.initial_index];
        //            label = label1to0Hash[label];
        //            if (label0cntHash[label] == 1)
        //                bincrease = true;
        //            break;
        //        default:    //case 4
        //            label = 0;
        //            break;
        //    }

        //    return bincrease;
        //}

        //private int GetLabel(RoseTreeNode node, int level)
        //{
        //    //if level 4, it can always collapse with each other
        //    if (level == 4)
        //        return 0;

        //    int label;
        //    while (node.children != null)
        //        node = node.children[0];
        //    label = labels1[node.indices.initial_index];
        //    if(level==3)
        //        label = label1to0Hash[label];

        //    return label;
        //}


        #endregion get proper probability

        #region on merge two trees
        //return logMergeProbability
        public override void MergeTwoTrees(RoseTreeNode node0, RoseTreeNode node1, MergeType mergetype)
        {
            //double prob = 0;
            ////test
            //switch (mergetype)
            //{
            //    case MergeType.Join:
            //        prob = GetLogJoinTreeProbability(node0, node1);
            //        break;
            //    case MergeType.AbsorbL:
            //        prob = GetLogAbsorbTreeProbability(node0, node1);
            //        break;
            //    case MergeType.AbsorbR:
            //        prob = GetLogAbsorbTreeProbability(node1, node0);
            //        break;
            //    case MergeType.Collapse:
            //        prob = GetLogCollapseTreeProbability(node0, node1);
            //        break;
            //}

            //if (prob == Double.MinValue)
            //    throw new Exception("Error building ground truth tree!");
        }
        #endregion on merge two trees
    }

    class NodeMergeCondition
    {
        public NodeMergeCondition(bool bincrease, int level, int label)
        {
            this.BCanIncrease = bincrease;
            this.Level = level;
            this.Label = label;
        }

        public bool BCanIncrease { get; protected set; }
        public int Level { get; protected set; }
        public int Label { get; protected set; }
    }
}
