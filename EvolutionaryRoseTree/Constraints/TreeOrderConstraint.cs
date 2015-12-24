using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EvolutionaryRoseTree.DataStructures;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;
using System.IO;

namespace EvolutionaryRoseTree.Constraints
{
    class TreeOrderConstraint : Constraint
    {
        public double LoseOrderPunishWeight = 0;
        public double IncreaseOrderPunishWeight = 0;
        public double AffLeavePunishWeight = 0;

        protected ConstraintTree ConstraintTree;
#if NORMALIZED_SMOOTHNESS_COST
        public override double NormalizedSmoothnessCost { get { return SmoothnessCost / Math.Pow(ConstraintTree.NotFreeConstraintTreeLeafCount, 3); } }
#else
        public override double NormalizedSmoothnessCost { get { return SmoothnessCost; } }
#endif
#if AVERAGE_ORDER_COST2
        public static double LargeClusterRelaxExp = 10;
#endif
        public TreeOrderConstraint(RoseTree rosetree, LoadFeatureVectors lfv,
            double loseorderpunishweight, double increaseorderpunishweight, DataProjectionRelation projRelation = null,
            ConstraintType constraintType = ConstraintType.TreeOrder) :
            base(constraintType) 
        {
            BuildConstraintTree(rosetree, lfv, projRelation);
            this.LoseOrderPunishWeight = loseorderpunishweight;
            this.IncreaseOrderPunishWeight = increaseorderpunishweight;
        }

        public TreeOrderConstraint(RoseTree rosetree, LoadFeatureVectors lfv,
            double loseorderpunishweight, double increaseorderpunishweight, double affleavePunishWeight, DataProjectionRelation projRelation = null,
            ConstraintType constraintType = ConstraintType.TreeOrder) :
            base(constraintType)
        {
            BuildConstraintTree(rosetree, lfv, projRelation);
            this.LoseOrderPunishWeight = loseorderpunishweight;
            this.IncreaseOrderPunishWeight = increaseorderpunishweight;
            this.AffLeavePunishWeight = affleavePunishWeight;
        }

        public void DisableUpdate()
        {
            ConstraintTree.DisableUpdate();
        }

        protected virtual void BuildConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv, DataProjectionRelation projRelation)
        {
            if (projRelation == null)
                this.ConstraintTree = new ConstraintTree(rosetree, lfv);
            else
                this.ConstraintTree = new SucceedRelationConstraintTree(rosetree, lfv, projRelation);
        }

        public void SetConstrainedRoseTree(ConstrainedRoseTree constrainedRoseTree)
        {
            ConstraintTree.SetConstrainedRoseTree(constrainedRoseTree);
        }

        public void SetParentMultipleConstraint(MultipleConstraints multiconstraint, int iConstraint)
        {
            ConstraintTree.SetParentMultipleConstraint(multiconstraint, iConstraint);
        }

        #region get proper probability
        public override double GetLogJoinTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            double order2unorder, unorder2order;
            ConstraintTree.GetMergeBrokenOrderNumbers(node0, node1, true, true, out order2unorder, out unorder2order);
#if AVERAGE_ORDER_COST
            return (-LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order) / ((node0.LeafCount + node1.LeafCount) * (node0.LeafCount * node1.LeafCount));
#else
#if AVERAGE_ORDER_COST2
            return (-LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order) / Math.Pow(node0.LeafCount + node1.LeafCount, LargeClusterRelaxExp);
#else
            return -LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order;
#endif
#endif
        }

        public override double GetLogAbsorbTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            double order2unorder, unorder2order;
            ConstraintTree.GetMergeBrokenOrderNumbers(node0, node1, false, true, out order2unorder, out unorder2order);
#if AVERAGE_ORDER_COST
            return (-LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order) / ((node0.LeafCount + node1.LeafCount) * (node0.LeafCount * node1.LeafCount));
#else
#if AVERAGE_ORDER_COST2
            return (-LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order) / Math.Pow(node0.LeafCount + node1.LeafCount, LargeClusterRelaxExp);
#else
            return -LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order;
#endif    
#endif
        }

        public override double GetLogCollapseTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            double order2unorder, unorder2order;
            ConstraintTree.GetMergeBrokenOrderNumbers(node0, node1, false, false, out order2unorder, out unorder2order);
#if AVERAGE_ORDER_COST
            return (-LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order) / ((node0.LeafCount + node1.LeafCount) * (node0.LeafCount * node1.LeafCount));
#else
#if AVERAGE_ORDER_COST2
            return (-LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order) / Math.Pow(node0.LeafCount + node1.LeafCount, LargeClusterRelaxExp);
#else
            return -LoseOrderPunishWeight * order2unorder - IncreaseOrderPunishWeight * unorder2order;
#endif
#endif      
        }

        public void SetLogLikelihoodStdRecord(List<double> record)
        {
            if (record == null)
                return;
            loglikelihoodStdRecord = new double[record.Count];
            int index = 0;
            foreach (double std in record)
                loglikelihoodStdRecord[index++] = std;
        }

        double[] loglikelihoodStdRecord = null;
        ////int mergeiter = 0;
        ////double coeff = 1;
        //private double GetTransformedTreeProbabilityRatio(double deltapunish, RoseTreeNode node0, RoseTreeNode node1)
        //{
        //    //int n0 = node0.LeafCount, n1 = node1.LeafCount;
        //    //int n = n0 + n1;
        //    //n0 = n0 * n0 * n0;
        //    //n1 = n1 * n1 * n1;
        //    //n = n * n * n;
        //    //n0 = n0 * (n0 - 1) * (n0 - 2) / 6;
        //    //n1 = n1 * (n1 - 1) * (n1 - 2) / 6;
        //    //n = n * (n - 1) * (n - 2) / 6;
        //    //n0 = 1;
        //    //n1 = 1;
        //    //n = 2;
        //    double raw = deltapunish - AffLeavePunishWeight * ConstraintTree.AffectedLeafNumber; //(deltapunish + node0.LogTreeProbability + node1.LogTreeProbability) / n - node0.LogTreeProbability / n0 - node1.log_likelihood / n1;
        //    //return raw * coeff;
        //    return raw;
        //}

        public override double GetLogTreeProbabilityFromRatio(double log_treeprobability_ratio, RoseTreeNode node0, RoseTreeNode node1)
        {
            //int n0 = node0.LeafCount, n1 = node1.LeafCount;
            //int n = n0 + n1;
            //n = n * n * n;
            //return 0;// log_treeprobability_ratio* n;
            return log_treeprobability_ratio + node0.LogTreeProbability + node1.LogTreeProbability;
        }
        #endregion get proper probability

        #region on merge two trees
        //return logMergeProbability
        //int iter = 0;
        public override void MergeTwoTrees(RoseTreeNode node0, RoseTreeNode node1, MergeType mergetype)
        {
            /// Calculate Smoothness Cost ///
            double order2unorder, unorder2order;
            ConstraintTree.GetMergeCost(node0, node1, mergetype, out order2unorder, out unorder2order);
            var deltaSmoothnessCost = - order2unorder - IncreaseOrderPunishWeight / LoseOrderPunishWeight * unorder2order;
            SmoothnessCost += deltaSmoothnessCost;
            //if (deltaSmoothnessCost > 0)
            //    Console.WriteLine("Error!");

            /// Start Merge Tree ///
            //Update depth in tree
            switch (mergetype)
            {
                case MergeType.Join:
                    ConstraintTree.MergeTree(node0, node1, true, true);
                    break;
                case MergeType.AbsorbL:
                    ConstraintTree.MergeTree(node0, node1, false, true);
                    break;
                case MergeType.AbsorbR:
                    ConstraintTree.MergeTree(node0, node1, true, false);
                    break;
                case MergeType.Collapse:
                    ConstraintTree.MergeTree(node0, node1, false, false);
                    break;
            }

            //DrawConstraintTree(@"D:\Project\EvolutionaryRoseTreeData\rosetree\test1853_1854_20\ct" + iter++ + ".gv");
            //iter++;
            /// Update coeff ///
            //if (loglikelihoodStdRecord != null)
            //    {
            //        mergeiter++;
            //        if (mergeiter != loglikelihoodStdRecord.Length)
            //            coeff = loglikelihoodStdRecord[mergeiter] / loglikelihoodStdRecord[0];
            //    }

            ConstraintTree.UpdateLeafNumbers();
        }
        #endregion on merge two trees

        #region open rose tree node
        public void UpdateConstraintTreeNodeOpened(SubRoseTree subrosetree, RoseTree ConstraintRoseTree,
            List<RoseTreeNode> widthTraversalNodeList, DataProjectionRelation projRelation)
        {
            ConstraintTree.UpdateConstraintTreeNodeOpened(subrosetree, ConstraintRoseTree, widthTraversalNodeList, projRelation);
        }
        #endregion open rose tree node

        public void DrawConstraintTree(string filename, bool bDrawInternalNodesOnly = false)
        {
            ConstraintTree.DrawConstraintTree(filename, bDrawInternalNodesOnly);
        }

        public ConstraintTree GetConstraintTree()
        {
            return ConstraintTree;
        }
    }

}
