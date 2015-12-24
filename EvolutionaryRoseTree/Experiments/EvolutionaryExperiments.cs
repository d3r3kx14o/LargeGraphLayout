using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;

using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.Accuracy;
using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Smoothness;

namespace EvolutionaryRoseTree.Experiments
{
    class EvolutionaryExperiments
    {
        public static int CollapseSmallClusterSize = 10;
        public static int CollapseSmallClusterNumber = 4;

        public static void Entry()
        {
            ConfigEvolutionary.Load(File.ReadAllLines("configEvolutionary.txt"));

            if(ConfigEvolutionary.ConstraintTypeStr == "No")
                BuildNoConstraintEntry();
            else
                BuildEvolutionaryEntry();
        }

#if ADJUST_BINGNEW_STEPBYSTEP
        public static Dictionary<int, Dictionary<int, int>> changeM = new Dictionary<int, Dictionary<int, int>>();
#endif
        public static void BuildEvolutionaryBingNewsTree(BingNewsParameters bingnewsParas)
        {

            /// ~~~ APPROXIMATE_LIKELIHOOD, SCALABILITY_TEST, SINGLE_PROJECTION, PROJ_SEEK_BEST_MATCH_DOC, ADJUST_TREE_STRUCTURE, AVERAGE_ORDER_COST ~~///
            /// SMOOTHNESS_ANALYSE, PROJ_SEEK_BEST_MATCH_DOC ///
            /// APPROXIMATE_LIKELIHOOD, SCALABILITY_TEST, ADJUST_TREE_STRUCTURE, ADJ_REMOVE_ROOT_OTHERS, SINGLE_PROJECTION, AVERAGE_ORDER_COST, OPEN_LARGE_CLUSTER, OPEN_LARGE_CLUSTER_MOD_2, COLLAPSE_SMALL_CLUSTER ///
            /// PRINT_MODIFIED_NODES, PROJECTION_FILE_BINARY
            /// WEIGHTED_MANY_PROJECTIONS,  SUPPRESS_WORDS
            
            
#if !(APPROXIMATE_LIKELIHOOD)
            Console.WriteLine("[Warning!!] Defines may be wrong!");
#endif
            Constraint.DataProjectionType = bingnewsParas.InputDataProjectionType;

            #region parameters
            ExperimentParameters.DataPath = ConfigEvolutionary.DataPath;
            if (ConfigEvolutionary.DataPath != null)
                ExperimentParameters.UpdateBasePathRelatedPaths(ConfigEvolutionary.DataPath);
            ExperimentParameters.CodePath = ConfigEvolutionary.CodePath;
            if (ConfigEvolutionary.SamplePath != null)
                ExperimentParameters.SamplePath = ConfigEvolutionary.SamplePath;
            BuildRoseTree.BingNewsTitleWeight = ConfigEvolutionary.TitleWeight;
            BuildRoseTree.BingNewsLeadingParagraphWeight = ConfigEvolutionary.LeadingParagraphWeight;
            BuildRoseTree.BingNewsBodyWeight = ConfigEvolutionary.BodyWeight;
            BuildRoseTree.BingNewsWeightLengthNormalization = false;
            BuildRoseTree.BRestrictBinary = bingnewsParas.IsBinaryStructure;
            if (BuildRoseTree.BRestrictBinary)
            {
                RobinsonFouldsDistance.RFDataProjectionType = DataProjectionType.MaxSimilarityDocument; //DataProjectionType.MaxSimilarityNode
                DataProjection.DocumentSkipPickedCount = 1;
                //RobinsonFouldsDistance.RFDataProjectionType = DataProjectionType.DataPredictionSearchDown; //DataProjectionType.MaxSimilarityNode
                //DataProjection.DocumentSkipPickedCount = 1;
                //ConstraintTree.bExpandToBinaryConstraintTree = true;
            }

            ExperimentParameters.ConstraintRoseTree = null;
            ExperimentParameters.EvolutionaryRoseTreePath += string.Format("start[{0}]span{1}slot{2}sample{3}\\",
                                                ConfigEvolutionary.StartDate, ConfigEvolutionary.Timespan, ConfigEvolutionary.TimesplotsNum, ConfigEvolutionary.SampleNums[0]);
            if (!double.IsNaN(ConfigEvolutionary.LooseOrderDeltaRatio))
                LooseTreeOrderConstraint.LooseOrderDeltaRatio = ConfigEvolutionary.LooseOrderDeltaRatio;
            if (!double.IsNaN(ConfigEvolutionary.DepthDifferenceWeight))
                ConstrainedCacheKey.depthdifferenceWeight = ConfigEvolutionary.DepthDifferenceWeight;
            if (!double.IsNaN(ConfigEvolutionary.SuppressWordRatio))
                LoadGlobalFeatureVectors.suppressRatio = ConfigEvolutionary.SuppressWordRatio;
            if (!double.IsNaN(ConfigEvolutionary.ClusterSizeWeight))
                DataProjection.ClusterSizeWeight = ConfigEvolutionary.ClusterSizeWeight;
            if(!double.IsNaN(ConfigEvolutionary.OpenNodeClusterAlphaRatio))
                ConstrainedRoseTree.AdjustStructureOpenNodeClusterAlphaRatio = ConfigEvolutionary.OpenNodeClusterAlphaRatio;
            if (!double.IsNaN(ConfigEvolutionary.SampleNumberRatio))
                BuildRoseTree.SampleNumberRatio = ConfigEvolutionary.SampleNumberRatio;
            if (ConfigEvolutionary.ClusterCollapseDocumentNumber > 0)
                ConstrainedRoseTree.AdjustStructureCollapseThreshold = 
                    ConstrainedRoseTree.AdjustStructureOthersThreshold = ConfigEvolutionary.ClusterCollapseDocumentNumber;
#if AVERAGE_ORDER_COST2
            if (!double.IsNaN(ConfigEvolutionary.LargeClusterRelaxExp))
                TreeOrderConstraint.LargeClusterRelaxExp = ConfigEvolutionary.LargeClusterRelaxExp;
#endif
            DataProjection.DocumentTolerateCosine = ConfigEvolutionary.AbandonCosineThreshold;

            ExperimentParameters.ResetDescription();
            //ExperimentParameters.Description = "EvolutionaryBN_" + ExperimentParameters.Description;
            ExperimentParameters.Description = string.Format("{0}_gamma{1}alpha{2}KNN{3}merge{4}split{5}cos{6}newalpha{7}_{8}{9}",
                ExperimentParameters.Description, ConfigEvolutionary.Gammas[0], ConfigEvolutionary.Alphas[0], ConfigEvolutionary.KNNParameters[ConfigEvolutionary.KNNParameters.Count - 1], ConfigEvolutionary.MergeParameters[0], ConfigEvolutionary.SplitParameters[0], ConfigEvolutionary.AbandonCosineThreshold,
                ConfigEvolutionary.NewTopicAlpha, ConfigEvolutionary.ConstraintTypeStr, ConfigEvolutionary.ConstraintTypeStr == "LooseOrder" ? LooseTreeOrderConstraint.LooseOrderDeltaRatio.ToString() : null);
#if NEW_CONSTRAINT_MODEL
            ExperimentParameters.Description += "_NCM";
#if NEW_MODEL_2
            ExperimentParameters.Description += "2";
#else 
#if NEW_MODEL_3
            ExperimentParameters.Description += "3";
#endif
#endif
#else
            ExperimentParameters.Description += "_OCM";
#endif
//#if UNSORTED_CACHE
//            ExperimentParameters.Description += "_Update3";
//#else
//#if CONSTRAINT_CHANGE_UPDATE_ALL
//            ExperimentParameters.Description += "_Update2";
//#else
//            ExperimentParameters.Description += "_Update";
//#endif
            //#endif
#if AVERAGE_ORDER_COST
            ExperimentParameters.Description += "_AvgO";
#else
#if AVERAGE_ORDER_COST2
            ExperimentParameters.Description += string.Format("_AvgO2({0})", TreeOrderConstraint.LargeClusterRelaxExp);
#endif
#endif
#if OPEN_LARGE_CLUSTER
#if OPEN_LARGE_CLUSTER_MOD_2
            ExperimentParameters.Description += "_OLC2";
#else
            ExperimentParameters.Description += "_OLC";
#endif
#endif
#if COLLAPSE_SMALL_CLUSTER
            ExperimentParameters.Description += "_CSC";
#endif
            if (LoadGlobalFeatureVectors.suppressRatio != 1)
                ExperimentParameters.Description += "_SUPP" + LoadGlobalFeatureVectors.suppressRatio;
            if (DataProjection.ClusterSizeWeight != 1)
                ExperimentParameters.Description += "_CSW" + DataProjection.ClusterSizeWeight;
            if (ConstrainedCacheKey.depthdifferenceWeight != 1)
                ExperimentParameters.Description += "_DepthW" + ConstrainedCacheKey.depthdifferenceWeight;

            if (ConfigEvolutionary.IndexPath.EndsWith("\\"))
                ConfigEvolutionary.IndexPath = ConfigEvolutionary.IndexPath.Substring(0, ConfigEvolutionary.IndexPath.Length - 1);
            //if (ConfigEvolutionary.IndexPath.EndsWith("3"))
            //    ExperimentParameters.Description += "_D3";
            //else if (ConfigEvolutionary.IndexPath.EndsWith("2"))
            //    ExperimentParameters.Description += "_D2";
            //else 
            //    ExperimentParameters.Description += "_D";
            //ExperimentParameters.Description += string.Format("_AR{0}", ConstrainedRoseTree.AdjustStructureOpenNodeClusterAlphaRatio);
            if (ConfigEvolutionary.ClusterCollapseDocumentNumber > 0)
                ExperimentParameters.Description += string.Format("_Cl{0}", ConfigEvolutionary.ClusterCollapseDocumentNumber);
            ExperimentParameters.Description += string.Format("_{0}{1}{2}", BuildRoseTree.BingNewsTitleWeight,
                BuildRoseTree.BingNewsLeadingParagraphWeight, BuildRoseTree.BingNewsBodyWeight);
            if (!double.IsNaN(ConfigEvolutionary.SampleNumberRatio))
                ExperimentParameters.Description += string.Format("_SR{0}", BuildRoseTree.SampleNumberRatio);
            ExperimentParameters.DatasetIndex = Constant.INDEXED_BING_NEWS;
            InitializeDrawRoseTreePath();


            //string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\BingNewsIndex_Microsoft";
            //string indexpath = @"D:\Project\ERT_Vis\data\lucene\BingNewsIndex_Microsoft3_6_1";
            string indexpath = ConfigEvolutionary.IndexPath;
            string defaultfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            string rawqueryStr = ConfigEvolutionary.RawQueryStr;

            string startdate = ConfigEvolutionary.StartDate;
            int timespan = ConfigEvolutionary.Timespan;
            int timeslotsnum = ConfigEvolutionary.TimesplotsNum;
            int deltatime = ConfigEvolutionary.DeltaTime;

            //double gamma = 0.15;
            //double alpha = 0.2;

            //int sampleNum = ConfigEvolutionary.SampleNum;
            //AlgorithmParameter algorithm_para = new BRTAlgorithmParameter();
            //AlgorithmParameter algorithm_para = new KNNAlgorithmParameter(ConfigEvolutionary.KNNParameter);
            //AlgorithmParameter algorithm_para = new SpillTreeAlgorithmParameter(50, 500);
            //int model_index = Constant.VMF;
            int model_index = bingnewsParas.model_index;

            int overlapratio = -1;
            int sampleTimes = 1;

            /// Set parameters ///
            ExperimentParameters.IndexedBingNewsPath = indexpath;
            ExperimentParameters.LoadDataQueryDefaultField = defaultfield;

            //ExperimentParameters.SampleNumber = sampleNum;
            //ExperimentParameters.RoseTreeParameters.algorithm_index = algorithm_index;
            //algorithm_para.Set();
            ExperimentParameters.ModelIndex = model_index;

            ExperimentParameters.SampleOverlapRatio = overlapratio;
            ExperimentParameters.SampleTimes = sampleTimes;
            DataProjection.AbandonCosineThreshold = ConfigEvolutionary.AbandonCosineThreshold;

            DataProjection.NewTopicAlpha = ConfigEvolutionary.NewTopicAlpha;

            /// output file ///
            StreamWriter ofile = InitializeBingNewsResultPrinter(bingnewsParas);
#if SMOOTHNESS_ANALYSE
            StreamWriter ofileMatlab = InitializeMatlabDataFunctionPrinter(bingnewsParas.GetConstraintParameter(0));
#endif
            //The first tree's constraint
            ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;
            //RoseTree constraintRoseTree = null;
            #endregion parameters

            double loglikelihoodRes;
            RoseTreeStructureInfo[] structureInfo;
            double[] smoothnessCost = new double[2];
            DateTime startdatetime = GetDateTime(startdate);
            DateTime begin_time = DateTime.Now;
            RoseTree consrosetree = null;
#if OPEN_LARGE_CLUSTER && !OPEN_LARGE_CLUSTER_MOD_2
            RoseTree consrosetree2 = null;
#endif
#if ADJUST_BINGNEW_STEPBYSTEP
            double mNewTopicAlpha = DataProjection.NewTopicAlpha;
            double mClusterSizeWeight = DataProjection.ClusterSizeWeight;
#endif
#if WEIGHTED_MANY_PROJECTIONS
            Dictionary<int, List<AdjustedFlowKeyWordsWeight>> adjustKeyWordsWeight = new Dictionary<int, List<AdjustedFlowKeyWordsWeight>>();
            AdjustedFlowKeyWordsWeight adjustflowgroup = new AdjustedFlowKeyWordsWeight();
            adjustflowgroup.prevNodeIndex = 2092;
            adjustflowgroup.nodeindices = new int[] { 2536, 2618 };
            adjustflowgroup.similarityThreshold = 0.6;
            adjustKeyWordsWeight.Add(7, (new AdjustedFlowKeyWordsWeight[] { adjustflowgroup }).ToList<AdjustedFlowKeyWordsWeight>());

            adjustflowgroup = new AdjustedFlowKeyWordsWeight();
            adjustflowgroup.bonlysimilarity = true;
            adjustflowgroup.prevNodeIndex = 1908;
            adjustflowgroup.nodeindices = new int[] { 2431 };
            adjustflowgroup.similarityThreshold = 0.5;
            adjustKeyWordsWeight.Add(3, (new AdjustedFlowKeyWordsWeight[] { adjustflowgroup }).ToList<AdjustedFlowKeyWordsWeight>());
            //adjustflowgroup.prevNodeIndex = 3295;
            //adjustflowgroup.nodeindices = new int[] { 2498, 2603 };
            //adjustflowgroup.similarityThreshold = 0.6;
            //adjustKeyWordsWeight.Add(1, (new AdjustedFlowKeyWordsWeight[] { adjustflowgroup }).ToList<AdjustedFlowKeyWordsWeight>());
#endif
            for (int itime = 0; itime < timeslotsnum; itime++)
            {
#if ADJUST_BINGNEW_STEPBYSTEP
                //if (itime == 2)
                //{
                //    DataProjection.NewTopicAlpha = 1e-50;
                //    DataProjection.ClusterSizeWeight = 0;
                //}
                //else
                //{
                //    DataProjection.NewTopicAlpha = mNewTopicAlpha;
                //    DataProjection.ClusterSizeWeight = mClusterSizeWeight;
                //}
                //if (itime == 1)
                //{
                //    changeM = new Dictionary<int, Dictionary<int, int>>();
                //    //changeM.Add(3795, new Dictionary<int, int>());
                //    //changeM[3795].Add(3713, 3);
                //    changeM.Add(3898, new Dictionary<int, int>());
                //    changeM[3898].Add(3890, 3);
                //}
                //else
                //    changeM.Clear();
#endif

                bingnewsParas.Set(itime);
                ExperimentParameters.LoadDataQueryString = rawqueryStr + " AND " + GetIndexedBingNewsDateQuery(startdatetime, timespan, itime, deltatime);
                ofile.WriteLine("------------------{0}--------------------", itime);
                ofile.WriteLine("Gamma: {0}, Alpha: {1}", ExperimentParameters.RoseTreeParameters.gamma, ExperimentParameters.RoseTreeParameters.alpha);
                ofile.WriteLine("Split: {0}, Merge: {1}", ExperimentParameters.IncreaseOrderPunishweight, ExperimentParameters.LoseOrderPunishweight);

                RoseTree rosetree = null;
                //try
                {

#if PRINT_VIOLATION_CURVE
                    StreamWriter ofileVioCurve = InitializeViolationCurve(itime);
                    ExperimentParameters.ViolationCurveFile = ofileVioCurve;
#endif
                    DateTime t0 = DateTime.Now;
                    //RoseTree.MergeRecordFileName = itime + "";
                    //ExperimentParameters.CacheValueRecordFileName = experimentIndex + "";
                    //ExperimentParameters.OriginalConstraintTreeFileName = itime + "co";
                    rosetree = ExperimentRoseTree.GetRoseTree();
                    DateTime t1 = DateTime.Now;
                    double runningTime = (t1.Ticks - t0.Ticks) / 1e7;
#if ADJUST_TREE_STRUCTURE
                    DrawTree(rosetree, itime, ofile, null);
                    (rosetree as ConstrainedRoseTree).AdjustTreeStructure();
#endif
#if OPEN_LARGE_CLUSTER
#if !OPEN_LARGE_CLUSTER_MOD_2
                    OpenLargeClusters(consrosetree2, consrosetree, rosetree);
#if COLLAPSE_SMALL_CLUSTER
                    CollapseTooSmallClusters(consrosetree as ConstrainedRoseTree, rosetree as ConstrainedRoseTree, CollapseSmallClusterSize, CollapseSmallClusterNumber);
#endif
                    if (consrosetree != null)
                        DrawTree(consrosetree, itime - 1, ofile);
                    if (itime == timeslotsnum - 1)
                    {
                        OpenLargeClusters(consrosetree, rosetree, null);
#if COLLAPSE_SMALL_CLUSTER
                        CollapseTooSmallClusters(rosetree as ConstrainedRoseTree, null, CollapseSmallClusterSize, CollapseSmallClusterNumber);
#endif
                    }
#else
                    OpenLargeClusters(consrosetree, rosetree, null);
                    CollapseTooSmallClusters(rosetree as ConstrainedRoseTree, null, CollapseSmallClusterSize, CollapseSmallClusterNumber);
#endif
#endif
#if COLLAPSE_SMALL_CLUSTER && !OPEN_LARGE_CLUSTER
                    CollapseTooSmallClusters(consrosetree as ConstrainedRoseTree, rosetree as ConstrainedRoseTree, CollapseSmallClusterSize, CollapseSmallClusterNumber);
                    if (consrosetree != null)
                        DrawTree(consrosetree, itime - 1, ofile);
                    if (itime == timeslotsnum - 1)
                        CollapseTooSmallClusters(rosetree as ConstrainedRoseTree, null, CollapseSmallClusterSize, CollapseSmallClusterNumber);
                    //(rosetree as ConstrainedRoseTree).CollapseTooSmallClusters(CollapseSmallClusterSize, CollapseSmallClusterNumber);
#endif
                    #region post process
                    TuneParameterExperiments.PrintRoseTreeStructure(rosetree, ofile);
                    loglikelihoodRes = (rosetree as ConstrainedRoseTree).LogLikelihood;
                    ofile.WriteLine("loglikelihood: " + loglikelihoodRes);
                    
                    structureInfo = rosetree.StructureInfo();
                    double smoothnessCostUnbiased = 0;

#if SMOOTHNESS_ANALYSE
                    //accuracyRes = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);
                    smoothnessCost = (rosetree as ConstrainedRoseTree).GetNormalizedSmoothnessCost();
                    if (consrosetree != null)
                        smoothnessCostUnbiased = 1 - RobinsonFouldsDistance.CalculateDistance(consrosetree, rosetree);
                    ofileMatlab.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6};",
                        loglikelihoodRes, smoothnessCost[0], smoothnessCost[1], smoothnessCostUnbiased,
                        0, 0,  rosetree.root.tree_depth);
#endif

                    ofile.WriteLine("Smoothness: " + smoothnessCost[0] + "\t" + smoothnessCost[1] + "\t" + smoothnessCostUnbiased);
                    ofile.WriteLine("RunTime: " + runningTime + "s");
                    #endregion

#if WEIGHTED_MANY_PROJECTIONS
                    DrawTree(rosetree, itime, ofile, adjustKeyWordsWeight);
#else
                    DrawTree(rosetree, itime, ofile, null);
#endif
                    ConstraintParameter constraintpara = bingnewsParas.GetConstraintParameter(itime);
                    if (constraintpara != null)
                        constraintpara.Set(rosetree);
#if OPEN_LARGE_CLUSTER && !OPEN_LARGE_CLUSTER_MOD_2
                    consrosetree2 = consrosetree;
#endif
                    consrosetree = rosetree;

#if PRINT_VIOLATION_CURVE
                    ofileVioCurve.Flush();
                    ofileVioCurve.Close();
#endif
                }
                //catch (Exception e)
                //{
                //    Console.WriteLine(e.Message);
                //    Console.WriteLine(e.StackTrace);
                //    ofile.WriteLine(e.Message);
                //    ofile.WriteLine(e.StackTrace);
                //}

                TuneParameterExperiments.PrintProgress(itime + 1, timeslotsnum, begin_time);
                //ConstrainedCacheKey.keycutofftaillength = 6;
                //ConstrainedCacheKey.similarityBalanceWeight = 1e-4;
            }

            ofile.WriteLine(TuneParameterExperiments.TimeEfficiencyToString(timeslotsnum, begin_time));

            ofile.Flush();
            ofile.Close();

#if SMOOTHNESS_ANALYSE
            EndMatlabDataFunction(ofileMatlab);
#endif
        }

        private static StreamWriter InitializeViolationCurve(int itime)
        {
            if (!Directory.Exists(ExperimentParameters.DrawRoseTreePath))
                Directory.CreateDirectory(ExperimentParameters.DrawRoseTreePath);
            StreamWriter ofile = new StreamWriter(ExperimentParameters.DrawRoseTreePath + itime + "_VioCurve.dat");
            return ofile;
        }

        public static void BuildNoConstraintEntry()
        {
            List<double> TraverseGammas = ConfigEvolutionary.Gammas;
            List<double> TraverseAlphas = ConfigEvolutionary.Alphas;
            List<int> SampleNumbers = ConfigEvolutionary.SampleNums;
            if (SampleNumbers.Count == 1)
                for (int itime = 0; itime < ConfigEvolutionary.TimesplotsNum; itime++)
                    SampleNumbers.Add(SampleNumbers[0]);

            String StartDate = ConfigEvolutionary.StartDate;
            int Timespan = ConfigEvolutionary.Timespan;
            int TimesplotsNum = ConfigEvolutionary.TimesplotsNum;

            DateTime startDateTime = GetDateTime(StartDate);
            ConfigEvolutionary.TimesplotsNum = 1;
            ConfigEvolutionary.Gammas = new List<double>();
            ConfigEvolutionary.Alphas = new List<double>();
            ConfigEvolutionary.Gammas.Add(double.NaN);
            ConfigEvolutionary.Alphas.Add(double.NaN);
            for (int itime = 0; itime < TimesplotsNum; itime++)
            {
                string startDateShifted = GetShiftedStartDate(startDateTime, Timespan, itime);
                Console.WriteLine(startDateShifted);
                ConfigEvolutionary.StartDate = startDateShifted;
                ConfigEvolutionary.SampleNums = new List<int>();
                ConfigEvolutionary.SampleNums.Add(SampleNumbers[itime]);

                foreach (double gamma in TraverseGammas)
                {
                    ConfigEvolutionary.Gammas[0] = gamma;
                    foreach (double alpha in TraverseAlphas)
                    {
                        ConfigEvolutionary.Alphas[0] = alpha;

                        BuildEvolutionaryEntry();
                    }
                }
            }
        }

        public static void BuildEvolutionaryEntry()
        {
            //for (double gamma = 0.01; gamma <= 0.5; gamma += 0.01)
            {
                //for (double alpha = 0.01; alpha <= 0.05; alpha += 0.0025)
                {
                    //Config.Gamma = gamma;
                    //Config.Alpha = alpha;
                    //Console.WriteLine("Gamma{0} Alpha{1}", gamma, alpha);
                    //BuildEvolutionaryBingNewsTree(Constant.DCM, ConfigEvolutionary.Gammas[0], ConfigEvolutionary.Alphas[0], 1);
#if SMOOTHNESS_ANALYSE
                    BingNewsParameters bingnewsParas = new BingNewsParameters(true);
#else
                    BingNewsParameters bingnewsParas = new BingNewsParameters();
#endif
                    BuildEvolutionaryBingNewsTree(bingnewsParas);
                }
            }
        }

        public static void OpenLargeClusters(RoseTree consrosetree, RoseTree rosetree, RoseTree constrainedrosetree)
        {
            if (rosetree == null)
                return;
            int OpenDocNumberTh = (int)(ExperimentParameters.OpenNodeClusterSizeRatio * rosetree.lfv.featurevectors.Length);
            ConstrainedRoseTree crosetree = rosetree as ConstrainedRoseTree;
            IList<RoseTreeNode> validinternalnodes = rosetree.GetAllValidInternalTreeNodes();
            DataProjectionRelation projRelation = null;
            foreach (RoseTreeNode validinternalnode in validinternalnodes)
            {
                if (validinternalnode.LeafCount >= OpenDocNumberTh && validinternalnode.tree_depth == 2)
                {
                    if (projRelation == null && crosetree.GetConstraint() is TreeOrderConstraint)
                        projRelation = (crosetree.GetConstraint() as TreeOrderConstraint).GetConstraintTree().GetDataProjectionRelation();
                    crosetree.OpenRoseTreeNode(validinternalnode, consrosetree, projRelation,
                    rosetree.alpha * ExperimentParameters.OpenNodeClusterAlphaRatio, 
                    rosetree.gamma, //ExperimentParameters.OpenNodeClusterGamma,
                    false);
                }
            }
            int treedepth;
            rosetree.LabelTreeIndices(out treedepth);

            //if (constrainedrosetree != null)
            //    throw new Exception("Not finished! Contained Info is not correct");
        }

        public static void CollapseTooSmallClusters(ConstrainedRoseTree rosetree, 
            ConstrainedRoseTree constrainedrosetree,
            int collapseClusterSize, int collapseClusterNumber)
        {
            if (rosetree == null)
                return;

            Dictionary<RoseTreeNode, List<RoseTreeNode>> collapsednodes = rosetree.CollapseTooSmallClusters(collapseClusterSize, collapseClusterNumber);
            if (constrainedrosetree != null)
            {
                if (constrainedrosetree.GetConstraint() is TreeOrderConstraint)
                {
                    (constrainedrosetree.GetConstraint() as TreeOrderConstraint).GetConstraintTree().
                        UpdateCollapsedTooSmallClusters(collapsednodes);
                }
                else if (constrainedrosetree.GetConstraint() is MultipleConstraints)
                {
                    Constraint constraint = (constrainedrosetree.GetConstraint() as MultipleConstraints).GetLastConstraint();
                    if (constraint is TreeOrderConstraint)
                        (constraint as TreeOrderConstraint).GetConstraintTree().
                        UpdateCollapsedTooSmallClusters(collapsednodes);
                }
            }
        }

        private static void DrawTree(RoseTree rosetree, int itime, StreamWriter ofile,
            Dictionary<int, List<AdjustedFlowKeyWordsWeight>> adjustKeyWordsWeight = null)
        {
            /// draw tree ///
            try
            {
                //if (itime == 9)
                //    Console.Write("");
                ExperimentRoseTree.DrawRoseTree(rosetree, itime + "");
                ExperimentRoseTree.DrawRoseTree(rosetree, itime + "_i", true);
                if (adjustKeyWordsWeight != null && adjustKeyWordsWeight.ContainsKey(itime))
                    SetAdjustedKeyWordsWeight(rosetree, adjustKeyWordsWeight[itime]);
                ExperimentRoseTree.DrawConstraintTree(rosetree, itime + "c");
                ExperimentRoseTree.DrawConstraintTree(rosetree, itime + "c_i", true);
                Console.WriteLine("TreeDepth:" + rosetree.root.tree_depth);
            }
            catch (Exception edrawtree)
            {
                Console.WriteLine("DrawTreeFailed: " + edrawtree.Message);
                Console.WriteLine(edrawtree.StackTrace);
                ofile.WriteLine("DrawTreeFailed: " + edrawtree.Message);
                ofile.WriteLine(edrawtree.StackTrace);
            }
        }

        private static void SetAdjustedKeyWordsWeight(RoseTree rosetree, List<AdjustedFlowKeyWordsWeight> adjustedFlows)
        {
            ConstrainedRoseTree consrosetree = rosetree as ConstrainedRoseTree;
            Constraint constraint = consrosetree.GetConstraint();
            if (constraint is TreeOrderConstraint)
            {
                ConstraintTree ctree = (constraint as TreeOrderConstraint).GetConstraintTree();
                ctree.SetDrawTreeWeightedSimilarity(adjustedFlows);
            }
            else
                Console.WriteLine("Warning! Could not SetAdjustedKeyWordsWeight. constraint is not TreeOrderConstraint!");
        }

        public static DateTime GetDateTime(string startdate)
        {
            string[] tokens = startdate.Split('-');
            return new DateTime(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]));
        }

        public static string GetIndexedBingNewsDateQuery(DateTime startdatetime, int timespan, int itime, int deltatime = -1)
        {
            if (deltatime < 0)
                deltatime = timespan;
            string str = "(";
            string timefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.DiscoveryStringTime;
            string timeformat = "{0:yyyy-MM-dd}";
            DateTime date = startdatetime.AddDays(deltatime * itime);
            for (int iday = 0; iday < timespan; iday++)
            {
                str += timefield + ":";
                str += string.Format(timeformat, date.AddDays(iday));
                if (iday != timespan - 1)
                    str += " OR ";
            }

            str += ")";
            return str;
        }

        public static string GetIndexedBingNewsDateQuery(string startdate, int timespan, int itime, int deltatime = -1)
        {
            return GetIndexedBingNewsDateQuery(GetDateTime(startdate), timespan, itime, deltatime);
        }
        
        static string GetShiftedStartDate(DateTime startdatetime, int timespan, int itime)
        {
            string timeformat = "{0:yyyy-MM-dd}";
            DateTime date = startdatetime.AddDays(timespan * itime);
            return string.Format(timeformat, date);
        }

        public static void BuildEvolutionaryNewYorkTimesTree()
        {
            ExperimentParameters.Description = "EvolutionaryNYT_" + ExperimentParameters.Description;
            ExperimentParameters.DatasetIndex = Constant.NEW_YORK_TIMES;
            InitializeDrawRoseTreePath();


            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\NYTimes\NYTIndex_Year\2006";
            string defaultfield = "Cleaned Taxonomic Classifier";
            string[] selectedClassifiers = new string[]{
                //"Top/Classifieds/Automobiles",
                "Top/Classifieds/Job Market",
                "Top/Classifieds/Real Estate",
                "Top/Features/Arts",
                "Top/Features/Style/",
                "Top/Features/Travel/",
                "Top/News/Business/",
                "Top/News/Health/",
                "Top/News/Science/",
                "Top/News/Sports/"
            };
            string rawqueryStr = "(Top/Features/Arts OR Top/Features/Style/ OR Top/Features/Travel/ OR " +
            "Top/News/Business/ OR Top/News/Sports)";
            //string rawqueryStr = "(5 OR 6 OR 7 OR 8 OR 9) AND Cleaned\\ Taxonomic\\ Classifier:Top/News/Sports";


            int[] month = new int[12];
            for (int i = 0; i < month.Length; i++) month[i] = i + 1;
            //int[] month = new int[] { 1, 2 };

            double gamma = 0.05;
            double alpha = 0.8;

            int sampleNum = 100;
            int algorithm_index = Constant.BRT;
            int model_index = Constant.DCM;

            int overlapratio = -1;
            int sampleTimes = 1;

            int experimentType = 1;

            //Rules rules;
            //ConstraintParameter constraintparameter = new NoConstraintParameter();
            //ConstraintParameter constraintparameter = new DistanceConstraintParameter(15);
            ConstraintParameter constraintparameter = new LooseOrderConstraintParameter(5, 5);
            //ConstraintParameter constraintparameter = new OrderConstraintParameter(1e50, 1e50);
            //ConstraintParameter constraintparameter = new LooseOrderConstraintParameter(double.MaxValue, double.MaxValue);

            /// Set parameters ///
            ExperimentParameters.NewYorkTimesPath = indexpath;
            ExperimentParameters.LoadDataQueryDefaultField = defaultfield;

            ExperimentParameters.RoseTreeParameters.gamma = gamma;
            ExperimentParameters.RoseTreeParameters.alpha = alpha;
            ExperimentParameters.SampleNumber = sampleNum;
            ExperimentParameters.RoseTreeParameters.algorithm_index = algorithm_index;
            ExperimentParameters.ModelIndex = model_index;

            ExperimentParameters.SampleOverlapRatio = overlapratio;
            ExperimentParameters.SampleTimes = sampleTimes;

            /// Matlab file ///
            StreamWriter ofile = InitializeResultPrinter(constraintparameter);
            StreamWriter ofileMatlab = InitializeMatlabDataFunctionPrinter(constraintparameter);

            //The first tree's constraint
            ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;
            //RoseTree constraintRoseTree = null;

            double loglikelihoodRes;
            RoseTreeStructureInfo[] structureInfo;
            double[] accuracyRes;
            double[] smoothnessCost;
            RoseTree previousRoseTree = null;
            for (int imonth = 0; imonth < month.Length; imonth++)
            {
                ExperimentParameters.LoadDataQueryString = rawqueryStr + " AND Publication\\ Month:" + month[imonth].ToString();
                ofile.WriteLine("------------------Month {0}--------------------", month[imonth]);

                //DateTime t0 = DateTime.Now;
                RoseTree rosetree = ExperimentRoseTree.GetRoseTree();
                //MetricTree metrictree = new MetricTree(rosetree, rosetree.GetNodeByArrayIndex(0));
                if (ExperimentParameters.ConstraintRoseTrees != null)
                {
                    ConstraintTree constrainttree = new ConstraintTree(ExperimentParameters.ConstraintRoseTrees[0], rosetree.lfv);
                    ExperimentRoseTree.DrawConstraintTree(constrainttree, imonth + "_oc.gv");
                }

                //DateTime t1 = DateTime.Now;
                //runningTime = (t1.Ticks - t0.Ticks) / 1e7;
                LoadDataInfo ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo;
                ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
                RoseTree gtrosetree = ExperimentRoseTree.GetRoseTree(ldinfo);

                TuneParameterExperiments.PrintRoseTreeStructure(rosetree, ofile);
                loglikelihoodRes = (rosetree as ConstrainedRoseTree).LogLikelihood;
                ofile.WriteLine("loglikelihood: " + loglikelihoodRes);
                structureInfo = rosetree.StructureInfo();
                accuracyRes = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);
                smoothnessCost = (rosetree as ConstrainedRoseTree).GetNormalizedSmoothnessCost();
                //ofileMatlab.WriteLine(ExperimentResultToString(loglikelihoodRes, smoothnessCost, runningTime, structureInfo[experimentIndex], accuracyRes[experimentIndex]));
                double smoothnessCostUnbiased = 0;
                if (previousRoseTree != null)
                    smoothnessCostUnbiased = 1 - RobinsonFouldsDistance.CalculateDistance(previousRoseTree, rosetree);
                ofile.WriteLine("Smoothness: " + smoothnessCost[0] + "\t" + smoothnessCost[1] + "\t" + smoothnessCostUnbiased);
                ofileMatlab.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5};", 
                    loglikelihoodRes, smoothnessCost[0], smoothnessCost[1], smoothnessCostUnbiased,
                    accuracyRes[6], accuracyRes[7]);

                /// draw tree ///
                try
                {
                    ExperimentRoseTree.DrawRoseTree(rosetree, imonth + "");
                    ExperimentRoseTree.DrawConstraintTree(rosetree, imonth + "_c");
                }
                catch (Exception edrawtree)
                {
                    ofile.WriteLine("DrawTreeFailed: " + edrawtree.Message);
                }

                if (experimentType == 1)
                    constraintparameter.Set(rosetree);
                else
                    constraintparameter.Set(gtrosetree);
                previousRoseTree = rosetree;
            }

            ofile.Flush();
            ofile.Close();

            EndMatlabDataFunction(ofileMatlab);
        }

        private static StreamWriter InitializeBingNewsResultPrinter(BingNewsParameters bingnewspara)
        {
            string filename = ExperimentParameters.Description + ".dat";
            filename = ExperimentParameters.EvolutionaryRoseTreePath + filename;

            if (!Directory.Exists(ExperimentParameters.EvolutionaryRoseTreePath))
                Directory.CreateDirectory(ExperimentParameters.EvolutionaryRoseTreePath);

            StreamWriter ofile = new StreamWriter(filename);

            ofile.WriteLine("exe = " + AppDomain.CurrentDomain.FriendlyName);
            ofile.WriteLine("Constraint = " + bingnewspara.GetConstraintParameter(0));
            ofile.WriteLine("gamma = " + ExperimentParameters.RoseTreeParameters.gamma);
            ofile.WriteLine("alpha = " + ExperimentParameters.RoseTreeParameters.alpha);
            ofile.WriteLine("samplenumber = " + ExperimentParameters.SampleNumber);
            ofile.WriteLine("similarityWeight = " + ConstrainedCacheKey.similarityBalanceWeight);
            ofile.WriteLine("data projection type = " + Constraint.DataProjectionType);
            ofile.WriteLine("abandon cosine th = " + DataProjection.AbandonCosineThreshold);
            ofile.WriteLine("loose order delta ratio = " + LooseTreeOrderConstraint.LooseOrderDeltaRatio);
            ofile.WriteLine("depth difference weight = " + ConstrainedCacheKey.depthdifferenceWeight);
            ofile.Write("knn parameters = ");
            foreach (int knn in bingnewspara.knnparameters)
                ofile.Write(knn + "\t");
            ofile.WriteLine();
            ofile.WriteLine("new topic alpha = " + DataProjection.NewTopicAlpha);
#if NEW_CONSTRAINT_MODEL
            ofile.WriteLine("NEW_CONSTRAINT_MODEL true");
#else
            ofile.WriteLine("NEW_CONSTRAINT_MODEL false");
#endif
            return ofile;
        }

        private static StreamWriter InitializeResultPrinter(ConstraintParameter constraintparameter)
        {
            string filename = ExperimentParameters.Description + ".dat";
            filename = ExperimentParameters.EvolutionaryRoseTreePath + filename;

            if (!Directory.Exists(ExperimentParameters.EvolutionaryRoseTreePath))
                Directory.CreateDirectory(ExperimentParameters.EvolutionaryRoseTreePath);

            StreamWriter ofile = new StreamWriter(filename);

            ofile.WriteLine("Constraint = " + constraintparameter);
            ofile.WriteLine("gamma = " + ExperimentParameters.RoseTreeParameters.gamma);
            ofile.WriteLine("alpha = " +  ExperimentParameters.RoseTreeParameters.alpha);
            ofile.WriteLine("samplenumber = " + ExperimentParameters.SampleNumber);
            ofile.WriteLine("similarityWeight = " + ConstrainedCacheKey.similarityBalanceWeight);
            ofile.WriteLine("data projection type = " + Constraint.DataProjectionType);
            ofile.WriteLine("abandon cosine th = " + DataProjection.AbandonCosineThreshold);
#if NEW_CONSTRAINT_MODEL
            ofile.WriteLine("NEW_CONSTRAINT_MODEL true");
#else
            ofile.WriteLine("NEW_CONSTRAINT_MODEL false");
#endif
            ofile.Flush();
            return ofile;
        }

        private static void InitializeDrawRoseTreePath()
        {
            ExperimentParameters.DrawRoseTreePath = ExperimentParameters.EvolutionaryRoseTreePath
                + ExperimentParameters.Description + "\\";
        }

        private static StreamWriter InitializeMatlabDataFunctionPrinter(ConstraintParameter constraintparameter)
        {
            string filename = "GetData_Exp4_" + ExperimentParameters.Description + ".m";
            filename = ExperimentParameters.OutputMatlabFunctionPath + filename;

            if (!Directory.Exists(ExperimentParameters.OutputMatlabFunctionPath))
                Directory.CreateDirectory(ExperimentParameters.OutputMatlabFunctionPath);

            StreamWriter ofileMatlab = new StreamWriter(filename);

            //Initialize Function Header
            ofileMatlab.WriteLine("function [likelihood, smoothcost, accuracy, depth] = GetEvolutionaryResult()");
            //corresponding detail data
            ofileMatlab.WriteLine("%" + ExperimentParameters.Description);
            ofileMatlab.WriteLine("% exe = " + AppDomain.CurrentDomain.FriendlyName);

            ofileMatlab.WriteLine("disp('Constraint = {0}');", constraintparameter);
            ofileMatlab.WriteLine("disp('gamma = {0}');", ExperimentParameters.RoseTreeParameters.gamma);
            ofileMatlab.WriteLine("disp('alpha = {0}');", ExperimentParameters.RoseTreeParameters.alpha);
            ofileMatlab.WriteLine("disp('samplenumber = {0}');", ExperimentParameters.SampleNumber);

            //data
            ofileMatlab.WriteLine();
            ofileMatlab.WriteLine("data = [");

            return ofileMatlab;
        }

        private static void EndMatlabDataFunction(StreamWriter ofileMatlab)
        {
            ofileMatlab.WriteLine();
            ofileMatlab.WriteLine("];");

            ofileMatlab.WriteLine("likelihood=data(:,1);");
            ofileMatlab.WriteLine("smoothcost=data(:,2:4);");
            ofileMatlab.WriteLine("accuracy=data(:,5:6);");
            ofileMatlab.WriteLine("depth=data(:,7);");

            ofileMatlab.WriteLine();
            ofileMatlab.WriteLine("end");

            ofileMatlab.Close();
        }
    }
}
