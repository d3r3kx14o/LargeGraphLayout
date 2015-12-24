using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Experiments;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.ReadData;

namespace EvolutionaryRoseTree.Constraints
{
    enum ConstraintType { TreeDistance, TreeOrder, GroundTruth, NoConstraint, LooseTreeOrder, Multiple };
    enum TreeDistanceType { Sum, Max };
    enum MergeType { Join, AbsorbL, AbsorbR, Collapse };

    abstract class Constraint
    {
        public static DataProjectionType DataProjectionType 
            = DataProjectionType.DataPredictionSearchDown;

        public Constraint(ConstraintType constrainttype)
        {
            this.ConstraintType = constrainttype;
            SmoothnessCost = 0;
        }

        public readonly ConstraintType ConstraintType;
        public double SmoothnessCost { get; protected set; }
        public virtual double NormalizedSmoothnessCost { get { return SmoothnessCost; } }

        public virtual double GetLogJoinTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            return Double.NaN;
        }

        public virtual double GetLogAbsorbTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            return Double.NaN;
        }

        public virtual double GetLogCollapseTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            return Double.NaN;
        }

        public virtual double GetLogTreeProbabilityFromRatio(double log_treeprobability_ratio, RoseTreeNode node0, RoseTreeNode node1)
        {
            return Double.NaN;
        }

        //return logMergeProbability
        public virtual void MergeTwoTrees(RoseTreeNode node0, RoseTreeNode node1, MergeType mergetype) 
        {
        }

        public static DataProjection InitializeDataProjection(RoseTree rosetree, LoadFeatureVectors lfv)
        {
            //Trace.WriteLine(string.Format("Projection Type: {0}", DataProjectionType));
            switch (DataProjectionType)
            {
                case DataProjectionType.MaxSimilaritySearchDown:
                    return new MaxSimilaritySearchDown(rosetree);
                case DataProjectionType.MaxSimilarityDocument:
                    return new MaxSimilarityDataProjection(rosetree, false);
                case DataProjectionType.MaxSimilarityInternalNode:
                    return new MaxSimilarityDataProjection(rosetree, true, true);
                case DataProjectionType.MaxSimilarityNode:
                    return new MaxSimilarityDataProjection(rosetree, true);
                case DataProjectionType.MaxSimilarityNodeDepthWeighted:
                    return new MaxSimilarityDataProjectionDepthWeighted(rosetree, true);
                case DataProjectionType.MaxSimilarityDocumentContentVector:
                    return new MaxSimilarityContentVectorDataProjection(rosetree);
                //case DataProjectionType.MaxSimilarityNewTopic:
                    //return new MaxSimilarityNewTopic(rosetree, 0.1);
                case DataProjectionType.RoseTreePredictionDocument:
                    return new RoseTreePredictionDataProjection(rosetree, lfv, false);
                case DataProjectionType.RoseTreePredictionNode:
                    return new RoseTreePredictionDataProjection(rosetree, lfv, true);
                case DataProjectionType.DataPredictionSearchDown:
                    return new RoseTreePredictionSearchDown(rosetree, lfv);
                case DataProjectionType.DataPredictionDocument:
                    return new RoseTreePredictionDataProjection(rosetree, lfv, false, true);
                case DataProjectionType.DataPredictionInternalNode:
                    return new RoseTreePredictionDataProjection(rosetree, lfv, true, true, true);
                case DataProjectionType.DataPredictionNode:
                    return new RoseTreePredictionDataProjection(rosetree, lfv, true, true);
                case DataProjectionType.DataPredictionNodeDepthWeighted:
                    return new RoseTreePredictionDataProjectionDepthWeighted(rosetree, lfv, true, true);

                default:
                    throw new Exception("Unknown data projection type!");
            }
        }

        public double GetMergeTreeLogProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1, int m)
        {
            double log_treeprobabilityratio = -1;
            switch (m)
            {
                case 0: log_treeprobabilityratio = GetLogJoinTreeProbabilityRatio(node0, node1); break;
                case 1: log_treeprobabilityratio = GetLogAbsorbTreeProbabilityRatio(node0, node1); break;
                case 2: log_treeprobabilityratio = GetLogAbsorbTreeProbabilityRatio(node1, node0); break;
                case 3: log_treeprobabilityratio = GetLogCollapseTreeProbabilityRatio(node0, node1); break;
            }
            return log_treeprobabilityratio;
        }

        //#region Data Projection
        ////Data Projection 1: Cosine Similarity
        ////Return: array_index
        //public static int FindNearestNeighbour(SparseVectorList vector, RoseTree rosetree,
        //    IList<RoseTreeNode> rosetreeleaves)
        //{
        //    int maxIndex = -1;
        //    double maxCosine = Double.MinValue;
        //    SparseVectorList[] prevectors = rosetree.lfv.featurevectors;

        //    int index = 0;
        //    foreach (RoseTreeNode leaf in rosetreeleaves)
        //    {
        //        if (leaf != null && (leaf.children == null || leaf.children.Length == 0))
        //        {
        //            //if (leaf.indices.array_index != index)
        //            //    throw new Exception("[Error] Array Index Does Not Match!");
        //            double cosine = vector.Cosine(vector, prevectors[leaf.indices.initial_index]);
        //            if (cosine > maxCosine)
        //            {
        //                maxCosine = cosine;
        //                maxIndex = leaf.indices.array_index;
        //                //if (leaf.indices.array_index != leaf.indices.initial_index)
        //                //    throw new Exception("");
        //            }
        //        }
        //        index++;
        //    }

        //    return maxIndex;
        //}

        ////Data Projection 2: 
        //#endregion Data Projection
    }

    class NoConstraint : Constraint
    {
        public NoConstraint()
            : base(ConstraintType.NoConstraint)
        {
        }

        public override double GetLogJoinTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            return 0;
        }

        public override double GetLogAbsorbTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            return 0;
        }

        public override double GetLogCollapseTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            return 0;
        }

        public override double GetLogTreeProbabilityFromRatio(double log_treeprobability_ratio, RoseTreeNode node0, RoseTreeNode node1)
        {
            return 0;
        }

        public override void MergeTwoTrees(RoseTreeNode node0, RoseTreeNode node1, MergeType mergetype)
        {
        }
    }

    class MultipleConstraints : Constraint
    {
        List<Constraint> constraints;
        public int constraintCnt { get; protected set; }
        public ConstraintType BaseConstraintType { get; protected set; }
        public Type BaseType { get { return constraints[0].GetType(); } }
        public double[] ConstraintWeights;
        public double[] ConstraintWeightsAll;
        public IList<Constraint> Constraints { get { return constraints.AsReadOnly(); } }
        double constraintweightsum;
        public RemoveConflictParameters RemoveConflictParameters;

        public MultipleConstraints(List<Constraint> constraints, ConstraintType baseconstraintType,
            double[] constraintweightsall, bool isNormalize = false, RemoveConflictParameters removeConflictParameters = null)
            : base(ConstraintType.Multiple)
        {
            this.constraints = constraints;
            this.constraintCnt = constraints.Count;
            this.BaseConstraintType = baseconstraintType;
            this.RemoveConflictParameters = removeConflictParameters;
            this.ConstraintWeightsAll = constraintweightsall;

            //this.constraintweights = constraintweights;
            this.ConstraintWeights = new double[constraints.Count];
            int localpt = ConstraintWeights.Length - 1;
            int allpt = constraintweightsall.Length - 1;
            while (localpt >= 0)
            {
                ConstraintWeights[localpt] = constraintweightsall[allpt];
                localpt--;  allpt--;
            }

            if (isNormalize)
            {
                constraintweightsum = 0;
                foreach (double constraintweight in ConstraintWeights)
                    constraintweightsum += constraintweight;
            }
            else
                constraintweightsum = 1;

            if (baseconstraintType == ConstraintType.TreeDistance)
                for (int iconstraint = 0; iconstraint < constraintCnt - 1; iconstraint++)
                    (constraints[iconstraint] as TreeDistanceConstraint).bUpdateTreeLeafDepth = false;
        }

        public override double GetLogJoinTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            double logprobsum = 0;
            for(int iconstraint = 0; iconstraint<constraintCnt;iconstraint++)
            {
                logprobsum += ConstraintWeights[iconstraint] *
                    constraints[iconstraint].GetLogJoinTreeProbabilityRatio(node0, node1);
            }
            return logprobsum / constraintweightsum;
        }

        public override double GetLogAbsorbTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            double logprobsum = 0;
            for (int iconstraint = 0; iconstraint < constraintCnt; iconstraint++)
            {
                logprobsum += ConstraintWeights[iconstraint] *
                    constraints[iconstraint].GetLogAbsorbTreeProbabilityRatio(node0, node1);
            }
            return logprobsum / constraintweightsum;
        }

        public override double GetLogCollapseTreeProbabilityRatio(RoseTreeNode node0, RoseTreeNode node1)
        {
            double logprobsum = 0;
            for (int iconstraint = 0; iconstraint < constraintCnt; iconstraint++)
            {
                logprobsum += ConstraintWeights[iconstraint] *
                    constraints[iconstraint].GetLogCollapseTreeProbabilityRatio(node0, node1);
            }
            return logprobsum / constraintweightsum;
        }

        public override double GetLogTreeProbabilityFromRatio(double log_treeprobability_ratio, RoseTreeNode node0, RoseTreeNode node1)
        {
            return log_treeprobability_ratio + node0.LogTreeProbability + node1.LogTreeProbability;
        }

        HashSet<int> affectedarrayindices = new HashSet<int>();
        public override void MergeTwoTrees(RoseTreeNode node0, RoseTreeNode node1, MergeType mergetype)
        {
            if (constrainedRoseTree != null)
                affectedarrayindices.Clear();

            foreach (Constraint constraint in constraints)
                constraint.MergeTwoTrees(node0, node1, mergetype);

            if(constrainedRoseTree!=null)
                foreach (int affectnodeindex in affectedarrayindices)
                    constrainedRoseTree.UpdateCacheValues(affectnodeindex);
        }

        public Constraint GetLastConstraint(int last = 1)
        {
            return constraints[constraintCnt - last];
        }

        public IList<Constraint> GetConstraints()
        {
            return constraints.AsReadOnly();
        }

        #region update cache values
        ConstrainedRoseTree constrainedRoseTree = null;
        internal void SetConstrainedRoseTree(ConstrainedRoseTree constrainedRoseTree)
        {
            this.constrainedRoseTree = constrainedRoseTree;
            int iconstraint = 0;
            foreach (Constraint constraint in constraints)
            {
                if (constraint is TreeOrderConstraint)
                {
                    //(constraint as TreeOrderConstraint).SetConstrainedRoseTree(constrainedRoseTree);
                    (constraint as TreeOrderConstraint).SetParentMultipleConstraint(this, iconstraint);
                    iconstraint++;
                }
            }
        }

        internal void RecordAffectedArrayIndices(int iconstraint, List<int> affectedRoseTreeNodeIndices)
        {
            if (ConstraintWeights[iconstraint] != 0)
                foreach (int affectedRoseTreeNodeIndex in affectedRoseTreeNodeIndices)
                    affectedarrayindices.Add(affectedRoseTreeNodeIndex);
        }
        #endregion
    }
}
