//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using EvolutionaryRoseTree.Constraints;
//using EvolutionaryRoseTree.DataStructures;
//using EvolutionaryRoseTree.Smoothness;
//using RoseTreeTaxonomy.Constants;

//namespace EvolutionaryRoseTree.Experiments
//{
//    #region config
//    class TKDEExperimentConfig
//    {
//        public int sampleNumber = 1000;
//        public double selectRatio = 1;
//        public int smoothnessOrders = 7;
//        public int constraintTreeNumber = 5;
//        public int[] sampleTimes;
//        public double[] constraintWeights;
//        public double gamma;
//        public double alpha;

//        public string configStr;

//        public TKDEExperimentConfig(string str)
//        {
//            configStr = str;
//            throw new NotImplementedException();
//        }
//    }
//    #endregion

//    class HandlingConflictExperiments
//    {
//        TKDEExperimentConfig expConfig;
//        string inputPath;
//        string outputPath;
//        string threadName;

//        public HandlingConflictExperiments(TKDEExperimentConfig expConfig,
//            string inputPath, string outputPath, string threadName)
//        {
//            this.expConfig = expConfig;
//            this.inputPath = inputPath;
//            this.outputPath = outputPath;
//            this.threadName = threadName;
//        }

//        public void Start()
//        {
//            #region --------------initialization--------------

//            string logFileDir = outputPath + "Log\\";
//            if (!Directory.Exists(logFileDir))
//                Directory.CreateDirectory(logFileDir);
//            var logsw = new StreamWriter(logFileDir + "log_" + threadName + ".log");
//            WriteLog(logsw, "===================Start===================\n" +
//                expConfig.configStr);

//            #region macros
//            ///NEW_YORK_TIMES_TEST_SMOOTHNESS, SCALABILITY_TEST, NORMALIZED_SMOOTHNESS_COST, PROJ_WEIGHT_1, NORMALIZE_PROJ_WEIGHT///
//#if !(!APPROXIMATE_LIKELIHOOD && !AVERAGE_ORDER_COST && !AVERAGE_ORDER_COST2)
//#if !(NEW_YORK_TIMES_TEST_SMOOTHNESS && SCALABILITY_TEST && NORMALIZED_SMOOTHNESS_COST)
//                        Console.WriteLine("[Warning!!] Defines may be wrong!");
//#endif
//#endif
//            #endregion

//            #region general parameters

//            int sampleNumber = expConfig.sampleNumber;
//            double selectRatio = expConfig.selectRatio;
//            int smoothnessOrders = expConfig.smoothnessOrders;
//            int constraintTreeNumber = expConfig.constraintTreeNumber;
//            int[] sampletimes = expConfig.sampleTimes;

//            ExperimentParameters.Description =
//                string.Format("D_SN{3}_k{0}_ConfR{1}_CT{2}_Ga{5}Al{6}_{7}{4}_",
//                smoothnessOrders, (selectRatio * 10), constraintTreeNumber, (sampleNumber == 1000 ? "" : sampleNumber.ToString()),
//                String.Format(ExperimentParameters.TimeFormat, DateTime.Now),
//                expConfig.gamma, expConfig.alpha,
//                (samplenumbers.Length == 5 ? "" : GetSampleTimesString(samplenumbers))
//                );
//            DataSetType newsdataset = DataSetType.NewYorkTimes;
//            Constraint.DataProjectionType = DataProjectionType.DataPredictionSearchDown;
//            LooseTreeOrderConstraint.LooseOrderDeltaRatio = -1; // -1;

//            //Gamma, Alpha
//            EvolvingDouble[] gammas = new EvolvingDouble[] { new EvolvingDouble(expConfig.gamma) };
//            EvolvingDouble[] alphas = new EvolvingDouble[] { new EvolvingDouble(expConfig.alpha) };

//            ExperimentParameters.ConstraintRoseTreeWeights = new double[smoothnessOrders];
//            for (int i = 0; i < constraintTreeNumber; i++)
//                ExperimentParameters.ConstraintRoseTreeWeights[smoothnessOrders - i - 1] = 1;
//            var edgeCost = 1;
//            var ratio = Math.Pow(selectRatio, 1.0 / (constraintTreeNumber - 1));
//            ExperimentParameters.RemoveConflictsParameters = selectRatio == 1 ? null : (new RemoveConflictParameters(ratio, 1, 1, 1, edgeCost, edgeCost, edgeCost));
//            ExperimentParameters.BNormalizeConstraintRoseTreeWeights = false;
//            int constraintTreeNumberLimit = ExperimentParameters.ConstraintRoseTreeWeights.Length;

//            //Constraint weight
//            ConstraintParameter[] constraints = new ConstraintParameter[expConfig.constraintWeights.Length];
//            for (int i = 0; i < expConfig.constraintWeights.Length; i++)
//            {
//                constraints[i] = new OrderConstraintParameter(expConfig.constraintWeights[i],
//                    expConfig.constraintWeights[i]);
//            }

//            #endregion

//            string indexpath = null;
//            string defaultfield = null;
//            string[] querystrings = null;
//            #region dataset
//            switch (newsdataset)
//            {
//                case DataSetType.NewYorkTimes:
//                    ExperimentParameters.DatasetIndex = Constant.NEW_YORK_TIMES;

//                    indexpath = ExperimentParameters.NewYorkTimesPath;
//                    defaultfield = "Cleaned Taxonomic Classifier";
//                    string rawqueryStr = "(Top/Features/Arts OR Top/Features/Style/ OR Top/Features/Travel/ OR " +
//                        "Top/News/Business/ OR Top/News/Sports)";
//                    int deltamonthpertime = 2; //month
//                    int startyear = 2006, startmonth = 1;
//                    int[] time = new int[9]; //[21];
//                    querystrings = new string[time.Length];
//                    for (int itime = 0; itime < time.Length; itime++)
//                        querystrings[itime] = TuneParameterExperiments.GetQueryString(itime, rawqueryStr, startyear, startmonth, deltamonthpertime);
//                    break;
//                case DataSetType.BingNewsObama:
//                    ExperimentParameters.DatasetIndex = Constant.INDEXED_BING_NEWS;

//                    indexpath = ExperimentParameters.DataPath + @"data\BingNewsData\BingNewsIndex_Obama\BingNews_Obama_Sep_4months4_RemoveSimilar_RemoveNoise_4Words\";
//                    defaultfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
//                    rawqueryStr = "Obama";
//                    string startdate = "2012-9-10";
//                    int timespan = 1;
//                    int timeslotsnum = 6;
//                    int deltatime = timespan;

//                    DateTime startdatetime = EvolutionaryExperiments.GetDateTime(startdate);
//                    querystrings = new string[timeslotsnum];
//                    for (int itime = 0; itime < timeslotsnum; itime++)
//                        querystrings[itime] = rawqueryStr + " AND " +
//                            EvolutionaryExperiments.GetIndexedBingNewsDateQuery(startdatetime, timespan, itime, deltatime);
//                    break;
//                case DataSetType.BingNewsDebt:
//                    ExperimentParameters.DatasetIndex = Constant.INDEXED_BING_NEWS;

//                    indexpath = ExperimentParameters.DataPath + @"data\BingNewsData\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar\";
//                    //indexpath = ExperimentParameters.DataPath + @"data\BingNewsData\debt_crisis2\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar2\";
//                    defaultfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
//                    rawqueryStr = "debt crisis";
//                    startdate = "2012-2-1";
//                    timespan = 14;
//                    timeslotsnum = 10;
//                    deltatime = timespan;

//                    startdatetime = EvolutionaryExperiments.GetDateTime(startdate);
//                    querystrings = new string[timeslotsnum];
//                    for (int itime = 0; itime < timeslotsnum; itime++)
//                        querystrings[itime] = rawqueryStr + " AND " +
//                            EvolutionaryExperiments.GetIndexedBingNewsDateQuery(startdatetime, timespan, itime, deltatime);
//                    break;
//                default:
//                    throw new NotImplementedException();
//            }
//            #endregion

//            #region not always used parameters
//            AlgorithmParameter[] algorithms = new AlgorithmParameter[]{
//                 new KNNAlgorithmParameter(50)};
//            int[] models = new int[]{
//                 Constant.DCM};

//            double[] overlapratio = new double[] { -1 };

//            /// Rule ///
//            Rules[] rules = new Rules[] { new Rules() };

//            DataProjection.AbandonCosineThreshold = -1; // 0.25;
//            if (BuildRoseTree.BRestrictBinary)
//                DataProjection.AbandonTreeDepthThreshold = 500;//int.MaxValue;//
//            else
//                DataProjection.AbandonTreeDepthThreshold = 4;//4
//            if (!BuildRoseTree.BRestrictBinary) DataProjection.DocumentSkipPickedCount = 2;
//            DataProjection.NewTopicAlpha = 0; //0;
//            DataProjection.DocumentCutGain = 1;//1
//            DataProjection.DocumentTolerateCosine = 0.2;//0.2
//            //DataProjection.AbandonCosineThreshold = -1;
//            //DataProjection.AbandonTreeDepthThreshold = 10;//4
//            //DataProjection.DocumentSkipPickedCount = 2;
//            //DataProjection.NewTopicAlpha = 0;
//            //DataProjection.DocumentCutGain = 0;//1
//            //DataProjection.DocumentTolerateCosine = -1;//0.2
//            #endregion not always used parameters

//            #region output files
//            StreamWriter ofile = TuneParameterExperiments.InitializeResultPrinter();
//            RobinsonFouldsDistance.ofile = ofile;

//            TuneParameterExperiments.InitializeDrawRoseTreePath();
//            StreamWriter ofileMatlab = TuneParameterExperiments.InitializeMatlabDataFunctionPrinterEvolutionary(
//                sampletimes, samplenumbers, overlapratio, models, algorithms, rules,
//                gammas, alphas, constraints, querystrings, indexpath);
//#if PRINT_VIOLATION_CURVE
//            StreamWriter ofileVioCurve = InitializeViolationCurve();
//            ExperimentParameters.ViolationCurveFile = ofileVioCurve;
//#endif
//            #endregion output files

//            #endregion

//            #region --------------running experiments--------------


//            #endregion

//            #region end
//            logsw.Flush();
//            logsw.Close();
//            #endregion


//            /// Evolutionary Parameters ///
 

//            int experimentNumber = alphas.Length * gammas.Length * samplenumbers.Length * overlapratio.Length
//       * algorithms.Length * models.Length * sampletimes.Length * rules.Length * constraints.Length * querystrings.Length;
//            int experimentIndex = 0;


//            DateTime time_begin = DateTime.Now;
//            for (int isampletimes = 0; isampletimes < sampletimes.Length; isampletimes++)
//            {
//                ExperimentParameters.SampleTimes = sampletimes[isampletimes];
//                ofile.WriteLine("////////////////////////SampleTimes:" + sampletimes[isampletimes] + "////////////////////////");
//                for (int isamplenum = 0; isamplenum < samplenumbers.Length; isamplenum++)
//                {
//                    ExperimentParameters.SampleNumber = samplenumbers[isamplenum];
//                    ofile.WriteLine("////////////////////////SampleNum:" + samplenumbers[isamplenum] + "////////////////////////");
//                    for (int ioverlapratio = 0; ioverlapratio < overlapratio.Length; ioverlapratio++)
//                    {
//                        ExperimentParameters.SampleOverlapRatio = overlapratio[ioverlapratio];
//                        ofile.WriteLine("////////////////////////SampleOverlapRatio:" + overlapratio[ioverlapratio] + "////////////////////////");
//                        ExperimentParameters.SampleTimes = sampletimes[isampletimes];
//                        ExperimentParameters.SampleOverlapRatio = -1;
//                        for (int imodel = 0; imodel < models.Length; imodel++)
//                        {
//                            ExperimentParameters.ModelIndex = models[imodel];
//                            ofile.WriteLine("////////////////////////" + (models[imodel] == Constant.DCM ? "DCM" : "vMF") + "////////////////////////");
//                            // use the same ldinfo for all below parameters
//                            for (int ialgorithm = 0; ialgorithm < algorithms.Length; ialgorithm++)
//                            {
//                                algorithms[ialgorithm].Set();
//                                ofile.WriteLine("////////////////////////" + algorithms[ialgorithm] + "////////////////////////");
//                                for (int irule = 0; irule < rules.Length; irule++)
//                                {
//                                    ExperimentParameters.Rules = rules[irule];
//                                    ofile.WriteLine("---------------------Rule:" + irule + "---------------------");
//                                    for (int igamma = 0; igamma < gammas.Length; igamma++)
//                                    {
//                                        EvolvingDouble gamma = gammas[igamma];
//                                        ofile.WriteLine("---------------------Gamma:" + gammas[igamma] + "---------------------");
//                                        for (int ialpha = 0; ialpha < alphas.Length; ialpha++)
//                                        {
//                                            EvolvingDouble alpha = alphas[ialpha];
//                                            ofile.WriteLine("---------------------Alpha:" + alphas[ialpha] + "---------------------");
//                                            ofile.WriteLine(DateTime.Now);

//                                            //List<double> loglikelihoodStdRecord = null;

//                                            for (int iconstraint = 0; iconstraint < constraints.Length; iconstraint++)
//                                            {
//                                                ofile.WriteLine("<Constraint " + iconstraint + ">");
//                                                ConstraintParameter constraint = constraints[iconstraint];
//                                                /// Evolutionary ///
//                                                ExperimentParameters.NewYorkTimesPath = ExperimentParameters.IndexedBingNewsPath = indexpath;
//                                                ExperimentParameters.LoadDataQueryDefaultField = defaultfield;
//                                                RoseTree previousRoseTree = null;
//                                                ExperimentParameters.ConstraintRoseTree = null;
//                                                ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;

//                                                List<RoseTree> constraintrosetrees = new List<RoseTree>();
//                                                for (int itime = 0; itime < querystrings.Length; itime++)
//                                                {
//                                                    ExperimentParameters.RoseTreeParameters.gamma = gamma.GetValue(itime);
//                                                    ExperimentParameters.RoseTreeParameters.alpha = alpha.GetValue(itime);
//                                                    ExperimentParameters.LoadDataQueryString = querystrings[itime];

//                                                    #region intialization
//                                                    double loglikelihoodRes = -1;
//                                                    RoseTreeStructureInfo[] structureInfo = null;
//                                                    double[] accuracyRes = null;
//                                                    double[] smoothnessCost = null;
//                                                    RoseTree rosetree = null;
//                                                    double runningTime = -1;
//                                                    double smoothnessCostUnbiased = -1;
//                                                    #endregion intialization

//                                                    //try
//                                                    {
//                                                        //ExperimentRoseTree.LoadDataInfo();

//                                                        smoothnessCost = null;
//                                                        runningTime = -1;

//                                                        ofile.Write("Exp {0}\t", experimentIndex);
//                                                        ofile.Write("Rule: " + irule + "\t");
//                                                        PrintRoseTreeParametersEvolutionary(ofile);
//                                                        //RoseTree.MergeRecordFileName = experimentIndex + "";
//                                                        //ExperimentParameters.CacheValueRecordFileName = experimentIndex + "";
//                                                        ExperimentRoseTree.ExperimentIndex = experimentIndex;

//                                                        DateTime t0 = DateTime.Now;
//                                                        rosetree = ExperimentRoseTree.GetRoseTree();
//                                                        DateTime t1 = DateTime.Now;
//                                                        runningTime = (t1.Ticks - t0.Ticks) / 1e7;

//                                                        PrintRoseTreeStructure(rosetree, ofile);

//                                                        loglikelihoodRes = (rosetree as ConstrainedRoseTree).LogLikelihood;
//                                                        ofile.WriteLine("loglikelihood: " + loglikelihoodRes);

//                                                        structureInfo = rosetree.StructureInfo();

//                                                        LoadDataInfo ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo;
//                                                        ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
//#if !TUNE_NTY_PARAMETERS
//                                                        //RoseTree gtrosetree = ExperimentRoseTree.GetRoseTree(ldinfo);
//                                                        //accuracyRes = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);

//                                                        smoothnessCost = (rosetree as ConstrainedRoseTree).GetNormalizedSmoothnessCost();
//                                                        smoothnessCostUnbiased = 1;
//                                                        try
//                                                        {
//                                                            if (previousRoseTree != null) smoothnessCostUnbiased = 1 - RobinsonFouldsDistance.CalculateDistance(previousRoseTree, rosetree);
//                                                        }
//                                                        catch
//                                                        {
//                                                            ofile.WriteLine("Calculate RF failed!");
//                                                            smoothnessCostUnbiased = -1;
//                                                        }
//                                                        ofile.WriteLine("Smoothness: " + smoothnessCost[0] + "\t" + smoothnessCost[1] + "\t" + smoothnessCostUnbiased);
//                                                        smoothnessCost = new double[] { smoothnessCost[0], smoothnessCost[1], smoothnessCostUnbiased };
//#endif
//                                                        ofileMatlab.WriteLine(ExperimentResultToString(ofile, rosetree, loglikelihoodRes, smoothnessCost, runningTime, structureInfo, accuracyRes));
//#if PRINT_VIOLATION_CURVE
//                                                        ofileVioCurve.WriteLine();
//#endif

//                                                        /// draw tree ///
//                                                        try
//                                                        {
//                                                            ExperimentRoseTree.DrawRoseTree(rosetree, experimentIndex + "");
//                                                            ExperimentRoseTree.DrawRoseTree(rosetree, experimentIndex + "_i", true);
//                                                            ExperimentRoseTree.DrawConstraintTree(rosetree, experimentIndex + "c");
//                                                            ExperimentRoseTree.DrawConstraintTree(rosetree, experimentIndex + "c_i", true);
//                                                        }
//                                                        catch (Exception edrawtree)
//                                                        {
//                                                            ofile.WriteLine("DrawTreeFailed: " + edrawtree.Message);
//                                                        }

//                                                        /// set constraint tree ///
//                                                        if (constraintrosetrees.Count >= constraintTreeNumberLimit)
//                                                            constraintrosetrees.RemoveAt(0);
//                                                        constraintrosetrees.Add(rosetree);
//                                                        constraint.Set(constraintrosetrees);
//                                                        previousRoseTree = rosetree;
//                                                        //(rosetree as ConstrainedRoseTree).ExperimentIndex = experimentIndex;

//                                                        experimentIndex++;

//                                                        PrintProgress(experimentIndex, experimentNumber, time_begin);
//                                                    }
//                                                    //catch (Exception e)
//                                                    //                                                    {
//                                                    //                                                        ofile.WriteLine("Failed: " + e.Message);
//                                                    //                                                        ofile.Flush();
//                                                    //                                                        ofile.WriteLine(e.StackTrace);
//                                                    //                                                        ofileMatlab.WriteLine(ExperimentResultToString(ofile, rosetree, loglikelihoodRes, smoothnessCost, runningTime, structureInfo, accuracyRes));
//                                                    //#if PRINT_VIOLATION_CURVE
//                                                    //                                                        ofileVioCurve.WriteLine();
//                                                    //#endif                                                        
//                                                    //                                                        experimentIndex++;
//                                                    //                                                    }
//                                                    ofile.Flush();
//                                                    ofileMatlab.Flush();
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//#if PRINT_VIOLATION_CURVE
//            ofileVioCurve.Close();
//#endif
//            EndMatlabDataFunction(ofileMatlab);
//            ofile.WriteLine("\n=============================Final Result=============================");
//            ofile.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
//                sampletimes.Length, samplenumbers.Length, overlapratio.Length,
//                models.Length, algorithms.Length, rules.Length,
//                gammas.Length, alphas.Length, constraints.Length);
//            ofile.Write(ExperimentParametersToString("Sample Time", sampletimes.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Sample Number", samplenumbers.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Sample Overlap Ratio", overlapratio.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Model", models.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Algorithm", algorithms.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Rule", rules.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Gamma", gammas.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Alpha", alphas.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("Constraint", constraints.Cast<object>().ToArray<object>()));
//            ofile.Write(ExperimentParametersToString("QueryString", querystrings.Cast<object>().ToArray<object>()));
//            //ofile.WriteLine("----------------------------------------------------------------------");
//            //for (int i = 0; i < experimentNumber; i++)
//            //    ofile.Write(ExperimentResultToString(structureInfo[i], accuracyRes[i], loglikelihoodRes[i]) + "\t");
//            ofile.WriteLine("\n======================================================================");
//            ofile.WriteLine(TimeEfficiencyToString(experimentNumber, time_begin));

//            ofile.Close();
//        }

//        private string GetSampleTimesString(int[] samplenumbers)
//        {
//            string str = "ST";
//            for (int i = 0; i < samplenumbers.Length; i++)
//            {
//                str += samplenumbers[i];
//            }
//            return str;
//        }


//        public void WriteLog(StreamWriter logsw, string str)
//        {
//            logsw.Write(string.Format("[{0}]\t"), DateTime.Now.ToShortTimeString());
//            logsw.WriteLine(str);
//            logsw.Flush();
//        }
//    }
//}
