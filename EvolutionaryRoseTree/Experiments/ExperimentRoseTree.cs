using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using RoseTreeTaxonomy.Algorithms;

using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.DataStructures;
namespace EvolutionaryRoseTree.Experiments
{
    class ExperimentRoseTree
    {
        //public static bool BDrawRoseTree = false;
        public static bool BDrawRoseTreeNode = true;
        public static bool BDrawRoseTreeAttribute = false;

        public static LoadDataInfo LastRoseTreeLoadDataInfo = null;

        public static LoadDataInfo LoadDataInfo()
        {
            Console.WriteLine("Loading feature vectors...");

            BuildRoseTree.SampleNumber = ExperimentParameters.SampleNumber;

            //load data
            LoadDataInfo ldinfo;
            switch (ExperimentParameters.DatasetIndex)
            {
                case RoseTreeTaxonomy.Constants.Constant.BING_NEWS:
                    ldinfo = BuildRoseTree.LoadBingNewsData(
                        ExperimentParameters.BingNewsPath,
                        ExperimentParameters.SamplePath,
                        ExperimentParameters.Time,
                        ExperimentParameters.DatasetIndex,
                        ExperimentParameters.ModelIndex,
                        ExperimentParameters.SampleTimes);
                    break;
                case RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP:
                    ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(
                        ExperimentParameters.TwentyNewsGroupPath,
                        ExperimentParameters.SamplePath,
                        ExperimentParameters.DatasetIndex,
                        ExperimentParameters.ModelIndex,
                        ExperimentParameters.SampleTimes,
                        ExperimentParameters.SampleOverlapRatio);
                    break;
                case RoseTreeTaxonomy.Constants.Constant.NEW_YORK_TIMES:
                    ldinfo = BuildRoseTree.LoadNewYorkTimesGroupData(
                        ExperimentParameters.NewYorkTimesPath,
                        ExperimentParameters.SamplePath,
                        ExperimentParameters.DatasetIndex,
                        ExperimentParameters.ModelIndex,
                        ExperimentParameters.SampleTimes,
                        ExperimentParameters.LoadDataQueryDefaultField,
                        ExperimentParameters.LoadDataQueryString,
                        ExperimentParameters.SampleOverlapRatio);
                    break;
                case RoseTreeTaxonomy.Constants.Constant.INDEXED_BING_NEWS:
                     ldinfo = BuildRoseTree.LoadLuceneIndexedBingNewsData(
                        ExperimentParameters.IndexedBingNewsPath,
                        ExperimentParameters.SamplePath,
                        ExperimentParameters.DatasetIndex,
                        ExperimentParameters.ModelIndex,
                        ExperimentParameters.SampleTimes,
                        ExperimentParameters.LoadDataQueryDefaultField,
                        ExperimentParameters.LoadDataQueryString,
                        ExperimentParameters.SampleOverlapRatio);
                    break;
                default:
                    throw new Exception("Only BingNewsData & 20NewsGroup & new york times & indexed bing news data Dataset are Supported Now!");
            }

            //Console.WriteLine("Finish: Loading feature vectors");

            return ldinfo;
        }

        public static int ExperimentIndex = -1;


        //return a rose tree specified by ExperimentParameters
        public static RoseTree GetRoseTree()
        {
            //load data
            LoadDataInfo ldinfo = LoadDataInfo();

            return GetRoseTree(ldinfo);
        }

        public static double BuildRoseTreeRunningTime = double.NaN;
        //return a rose tree specified by ExperimentParameters
        public static RoseTree GetRoseTree(LoadDataInfo loaddatainfo)
        {
            BuildRoseTree.SampleNumber = ExperimentParameters.SampleNumber;
            BuildRoseTree.CacheValueRecordFileName = ExperimentParameters.CacheValueRecordFileName;
            BuildRoseTree.ViolationCurveFile = ExperimentParameters.ViolationCurveFile;

            RoseTreeParameters para = ExperimentParameters.RoseTreeParameters;

            //load data
            LoadDataInfo ldinfo = loaddatainfo;
            LastRoseTreeLoadDataInfo = ldinfo;

            //Initialize Constraint
            Constraint constraint = GetParameterConstraint(ExperimentParameters.ConstraintType, loaddatainfo, ExperimentParameters.ConstraintRoseTrees);
            if (ExperimentParameters.ConstraintType != ConstraintType.GroundTruth
                && ExperimentParameters.SmoothCostConstraintTypes != null 
                && ExperimentParameters.ConstraintRoseTrees != null)
            {
                BuildRoseTree.SmoothCostConstraints = new List<Constraint>();
                foreach (ConstraintType smconstraintType in ExperimentParameters.SmoothCostConstraintTypes)
                {
                    Constraint smoothconstraint = GetParameterConstraint(smconstraintType, loaddatainfo, ExperimentParameters.ConstraintRoseTrees2, true);
                    BuildRoseTree.SmoothCostConstraints.Add(smoothconstraint);
                }
            }
            else
                BuildRoseTree.SmoothCostConstraints = null;

            #region remove conflicts
            if (constraint != null &&
                constraint.ConstraintType == ConstraintType.Multiple)
            {
                var multiConstraint = (constraint as MultipleConstraints);
                var removeConflictParameters = multiConstraint.RemoveConflictParameters;
                if (removeConflictParameters != null &&
                    (multiConstraint.BaseConstraintType == ConstraintType.LooseTreeOrder ||
                    multiConstraint.BaseConstraintType == Constraints.ConstraintType.TreeOrder))
                {
                    var constraintweightsall = multiConstraint.ConstraintWeightsAll;
                    var constraints = multiConstraint.Constraints;
                    List<ConstraintTree> constraintTrees = new List<ConstraintTree>();
                    int weightIndex = constraintweightsall.Length - constraints.Count;
                    foreach (var constraint2 in constraints)
                    {
                        if (constraintweightsall[weightIndex] != 0)
                        {
                            constraintTrees.Add((constraint2 as TreeOrderConstraint).GetConstraintTree());
                        }
                        weightIndex++;
                    }

                    var removeConflicts = new RemoveContraintTreeConflicts(constraintTrees,
                        removeConflictParameters);
                    removeConflicts.Start();

                    var smoothnessConstraints = BuildRoseTree.SmoothCostConstraints;
                    if (smoothnessConstraints != null)
                    {
                        foreach (var smoothnessConstraint in smoothnessConstraints)
                        {
                            if(smoothnessConstraint.ConstraintType == ConstraintType.Multiple &&
                                (smoothnessConstraint as MultipleConstraints).BaseConstraintType ==
                                ConstraintType.TreeDistance)
                            {
                                var multiDisConstraint = smoothnessConstraint as MultipleConstraints;
                                var distanceConstraints = new List<TreeDistanceConstraint>();
                                weightIndex = constraintweightsall.Length - constraints.Count;
                                foreach (var constraint2 in multiDisConstraint.Constraints)
                                {
                                    if (constraintweightsall[weightIndex] != 0)
                                    {
                                        distanceConstraints.Add(constraint2 as TreeDistanceConstraint);
                                    }
                                    weightIndex++;
                                }
                                removeConflicts.RemoveDistanceConstraintConflict(distanceConstraints);
                            }
                        }
                    }
                }
            }
            #endregion

            //Print Constraint
#if PRINT_INITIAL_CONSTRAINT_TREE
            DrawAllConstraintTrees(constraint, ExperimentIndex + "c0_i", true);
            DrawAllConstraintTrees(constraint, ExperimentIndex + "c0");
#endif

            //Print Constraint
            if (ExperimentParameters.OriginalConstraintTreeFileName != null &&
                constraint != null && constraint is TreeOrderConstraint)
            {
                DrawConstraintTree((constraint as TreeOrderConstraint).GetConstraintTree(),
                    ExperimentParameters.OriginalConstraintTreeFileName, false, true);
                DrawConstraintTree((constraint as TreeOrderConstraint).GetConstraintTree(),
                    ExperimentParameters.OriginalConstraintTreeFileName + "i", true, true);
            }

            Console.WriteLine("Building Rose Tree...");

            DateTime t0 = DateTime.Now;
            //build tree
            RoseTree rosetree = null;
            if (ExperimentParameters.ConstraintType != ConstraintType.GroundTruth)
                rosetree = BuildRoseTree.BuildTree(ldinfo,
                                 constraint,
                                 ExperimentParameters.Rules,
                                 ExperimentParameters.RoseTreeParameters,
                                 ExperimentParameters.LikelihoodPath,
                                 ExperimentParameters.OutputDebugPath);
            else
                rosetree = BuildRoseTree.BuildGroundTruthRoseTree(ldinfo,
                        ExperimentParameters.RoseTreeParameters,
                        ExperimentParameters.LikelihoodPath,
                        ExperimentParameters.OutputDebugPath);
            DateTime t1 = DateTime.Now;
            BuildRoseTreeRunningTime = (t1.Ticks - t0.Ticks) / 1e7;

            //if (BDrawRoseTree)
            //    DrawRoseTree(rosetree);

            //Console.WriteLine("Finish: building Rose Tree");
            
            return rosetree;
        }

        //public static int DrawRoseTreeIndex = 0;
        //public static void DrawRoseTree(RoseTree rosetree)
        //{
        //    string drawpath = ExperimentParameters.DrawRoseTreePath + ExperimentParameters.Description + "\\";
        //    if (!Directory.Exists(drawpath))
        //        Directory.CreateDirectory(drawpath);
        //    RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new
        //        RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawpath, 100);
        //    drawrosetree.Run(String.Format(ExperimentParameters.TimeFormat, DateTime.Now) +
        //        "_" + DrawRoseTreeIndex + ".gv");
        //    DrawRoseTreeIndex++;
        //}

        public static void DrawRoseTree(RoseTree rosetree, string filename, bool bDrawInternalNodesOnly = false)
        {
            string drawpath = ExperimentParameters.DrawRoseTreePath;
            if (!Directory.Exists(drawpath))
                Directory.CreateDirectory(drawpath);
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new
                RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawpath, ExperimentParameters.RoseTreeKeyWordNumber, BDrawRoseTreeNode, BDrawRoseTreeAttribute);
            drawrosetree.Run(filename + ".gv", bDrawInternalNodesOnly);
        }

        public static void DrawConstraintTree(ConstraintTree ctree, string filename, bool bDrawInternalNodesOnly = false,
            bool bDrawLeafNumber = false)
        {
            string drawpath = ExperimentParameters.DrawRoseTreePath;
            if (!Directory.Exists(drawpath))
                Directory.CreateDirectory(drawpath);

            ctree.DrawConstraintTree(drawpath + filename + ".gv", bDrawInternalNodesOnly, bDrawLeafNumber);
        }

        public static void DrawAllConstraintTrees(Constraint rosetreeconstraint, string filename, bool bDrawInternalNodesOnly = false)
        {
            string drawpath = ExperimentParameters.DrawRoseTreePath;
            if (!Directory.Exists(drawpath))
                Directory.CreateDirectory(drawpath);

            //if (crosetree != null)
            {
                if (rosetreeconstraint is MultipleConstraints)
                {
                    for (int i = 1; i <= (rosetreeconstraint as MultipleConstraints).constraintCnt; i++)
                    {
                        Constraint singleconstraint = (rosetreeconstraint as MultipleConstraints).GetLastConstraint(i);
                        if (singleconstraint is TreeOrderConstraint)
                        {
                            (singleconstraint as TreeOrderConstraint).DrawConstraintTree(drawpath + filename + "_last" + i + ".gv", bDrawInternalNodesOnly);
                        }
                    }
                }
                else if (rosetreeconstraint is TreeOrderConstraint)
                {
                    (rosetreeconstraint as TreeOrderConstraint).DrawConstraintTree(drawpath + filename + "_last1.gv", bDrawInternalNodesOnly);
                }
            }
        }
        
        public static void DrawConstraintTree(RoseTree rosetree, string filename, bool bDrawInternalNodesOnly = false)
        {
            string drawpath = ExperimentParameters.DrawRoseTreePath;
            if (!Directory.Exists(drawpath))
                Directory.CreateDirectory(drawpath);

            ConstrainedRoseTree crosetree = rosetree as ConstrainedRoseTree;
            if (crosetree!=null)
            {
                Constraint rosetreeconstraint = crosetree.GetConstraint();
                if (rosetreeconstraint is MultipleConstraints)
                {
                    rosetreeconstraint = (rosetreeconstraint as MultipleConstraints).GetLastConstraint();
                }
                if(rosetreeconstraint is TreeOrderConstraint)
                {
                    (rosetreeconstraint as TreeOrderConstraint).DrawConstraintTree(drawpath + filename + ".gv", bDrawInternalNodesOnly);
                }
            }
        }

        private static Constraint GetParameterConstraint(ConstraintType constraintType, 
            LoadDataInfo ldinfo, RoseTree constraintRoseTree, bool bDisableUpdate = false)
        {
            Constraint constraint = null;
            switch (constraintType)
            {
                case ConstraintType.TreeDistance:
                    constraint = new TreeDistanceConstraint(
                        constraintRoseTree,
                        ldinfo.lfv,
                        ExperimentParameters.TreeDistanceType,
                        ExperimentParameters.TreeDistancePunishweight);
                    break;
                case ConstraintType.TreeOrder:
                    constraint = new TreeOrderConstraint(
                        constraintRoseTree,
                        ldinfo.lfv,
                        ExperimentParameters.LoseOrderPunishweight,
                        ExperimentParameters.IncreaseOrderPunishweight,
                        ExperimentParameters.AffectLeaveCntPunishWeight);
                    (constraint as TreeOrderConstraint).SetLogLikelihoodStdRecord(ExperimentParameters.LoglikelihoodStdRecord);
                    if (bDisableUpdate) (constraint as TreeOrderConstraint).DisableUpdate();
                    break;
                case ConstraintType.LooseTreeOrder:
                    constraint = new LooseTreeOrderConstraint(
                       constraintRoseTree,
                       ldinfo.lfv,
                       ExperimentParameters.LoseOrderPunishweight,
                       ExperimentParameters.IncreaseOrderPunishweight,
                       ExperimentParameters.AffectLeaveCntPunishWeight);
                    (constraint as TreeOrderConstraint).SetLogLikelihoodStdRecord(ExperimentParameters.LoglikelihoodStdRecord);
                    if (bDisableUpdate) (constraint as TreeOrderConstraint).DisableUpdate();
                    break;
                //case ConstraintType.GroundTruth:
                //    constraint = new GroundTruthConstraint(
                //        ldinfo.lfv);
                //    break;
            }

            return constraint;
        }

        private static Constraint GetParameterConstraint(ConstraintType constraintType,
    LoadDataInfo ldinfo, List<RoseTree> constraintRoseTrees, bool bDisableUpdate = false)
        {
            if (constraintType == ConstraintType.NoConstraint)
                return null;
            List<Constraint> constraints = new List<Constraint>();
            foreach (RoseTree constraintRoseTree in constraintRoseTrees)
            {
                Constraint constraint = GetParameterConstraint(constraintType, ldinfo, constraintRoseTree, bDisableUpdate);
                constraints.Add(constraint);
            }
            Constraint multiconstraint = new MultipleConstraints(constraints, constraintType,
                ExperimentParameters.ConstraintRoseTreeWeights, ExperimentParameters.BNormalizeConstraintRoseTreeWeights,
                ExperimentParameters.RemoveConflictsParameters );
            return multiconstraint;
        }

        public static double[] GetTreeDepthInfo(RoseTree rosetree)
        {
            double minDepth = Double.MaxValue;
            double maxDepth = Double.MinValue;
            double depthSum = 0;
            double depthSquareSum = 0;
            double depthCnt = 0;

            var leaves = rosetree.GetAllTreeLeaf();
            foreach (var leaf in leaves)
            {
                int depth = leaf.DepthInTree;
                if (depth < minDepth)
                    minDepth = depth;
                if (depth > maxDepth)
                    maxDepth = depth;
                depthSum += depth;
                depthSquareSum += depth * depth;

                depthCnt++;
            }

            return new double[] { minDepth, maxDepth, depthSum / depthCnt, Math.Sqrt(depthSquareSum / depthCnt - depthSum * depthSum / depthCnt / depthCnt) };
        }
    }
}
