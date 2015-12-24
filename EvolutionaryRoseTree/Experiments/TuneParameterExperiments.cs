using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;

using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.Accuracy;
using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Smoothness;
using System.Diagnostics;

namespace EvolutionaryRoseTree.Experiments
{
    public enum DataSetType { NewYorkTimes, BingNewsMicrosoft, BingNewsObama, BingNewsDebt, BingNewsSyria };

    class TuneParameterExperiments
    {
        public static void Entry()
        {
            TuneParameters_Evolutionary();
            //TuneParameters_DrawRoseTree();
            
            //Tune20NewsGroupsRoseTreeParameters_Accuracy();
            //Console.ReadKey();
        }

        public static void TuneParameters_Evolutionary()
        {
            ///NEW_YORK_TIMES_TEST_SMOOTHNESS, SCALABILITY_TEST, NORMALIZED_SMOOTHNESS_COST, PROJ_WEIGHT_1, NORMALIZE_PROJ_WEIGHT///
            ///!TUNE_NTY_PARAMETERS, PRINT_INITIAL_CONSTRAINT_TREE///
            ///PROJ_WEIGHT_1, PROJ_WEIGHT_3///
            /// PRINT_VIOLATION_CURVE ////

#if !(!APPROXIMATE_LIKELIHOOD && !AVERAGE_ORDER_COST && !AVERAGE_ORDER_COST2)
#if !(NEW_YORK_TIMES_TEST_SMOOTHNESS && SCALABILITY_TEST && NORMALIZED_SMOOTHNESS_COST)
            Console.WriteLine("[Warning!!] Defines may be wrong!");
#endif
#endif
            //ExperimentParameters.Description = "Exp4_NYT_100_RForg_C0_OT2_k7_PredSD_ConfR0.8_CT5_ST1" + ExperimentParameters.Description;

            double selectRatio = 1;
            int[] sampletimes = new int[] { 3, 4, 5 }; //1, 3, 4, 5, 8
            int sampleNumber = 1000;
            int smoothnessOrders = 7;
            int constraintTreeNumber = 5;

            //ExperimentParameters.Description = "Exp4_NYT_100_RForg_C0_OT2_k7_PredSD_ConfR0.8_CT5_ST1" + ExperimentParameters.Description;
            ExperimentParameters.Description =
                string.Format("ExpD_NYT{3}_1EN67_C0_OT4_k{0}_PredSD_ConfR{1}_CT{2}_{4}",
                smoothnessOrders, selectRatio * 10,
                constraintTreeNumber, (sampleNumber == 1000 ? "" : sampleNumber.ToString()),
                (sampletimes.Length == 5 ? "" : (GetSampleTimesString(sampletimes) + "_") ),
                ExperimentParameters.Description);
            DataSetType newsdataset = DataSetType.NewYorkTimes;
            Constraint.DataProjectionType = DataProjectionType.DataPredictionSearchDown;
            LooseTreeOrderConstraint.LooseOrderDeltaRatio = -1; // -1;
            BuildRoseTree.BRestrictBinary = false;
            if (BuildRoseTree.BRestrictBinary)
            {
                RobinsonFouldsDistance.RFDataProjectionType = DataProjectionType.MaxSimilarityDocument; //DataProjectionType.MaxSimilarityNode
                DataProjection.DocumentSkipPickedCount = 1;
                //RobinsonFouldsDistance.RFDataProjectionType = DataProjectionType.DataPredictionSearchDown; //DataProjectionType.MaxSimilarityNode
                //DataProjection.DocumentSkipPickedCount = 1;
                //ConstraintTree.bExpandToBinaryConstraintTree = true;
            }

            #region gamma alpha
            //double[] gammas = new double[] { 0.01, 0.03, 0.1, 0.2, 0.3 };
            //double[] alphas = new double[] { 0.01, 0.02 };
            //EvolvingDouble[] gammas = new EvolvingDouble[]{
            //    new EvolvingDouble(new double[]{0.03, 0.03, 0.03, 0.03, 0.03, 0.03, 0.03, 0.3, 0.03, 0.03, 0.03, 0.03, 0.1, 0.03, 0.03, 0.1, 0.03, 0.03, 0.03, 0.03, 0.03})};
            //EvolvingDouble[] alphas = new EvolvingDouble[]{
            //    new EvolvingDouble(new double[]{1e-3, 1e-3, 1e-3, 1e-3, 1e-4, 1e-4, 1e-4, 1e-4, 1e-4, 1e-3, 1e-3, 1e-3, 1e-3, 1e-3, 1e-4, 1e-3, 1e-5, 1e-4, 1e-4, 1e-3, 1e-3})};
            //Org NYT
            //EvolvingDouble[] gammas = new EvolvingDouble[] { new EvolvingDouble(0.03) };
            //EvolvingDouble[] alphas = new EvolvingDouble[] { new EvolvingDouble(5e-4) };

            EvolvingDouble[] gammas = new EvolvingDouble[] { new EvolvingDouble(1e-6) };
            EvolvingDouble[] alphas = new EvolvingDouble[] { new EvolvingDouble(1e-7) };
         
            //EvolvingDouble[] gammas = new EvolvingDouble[] { new EvolvingDouble(0.1) };
            //EvolvingDouble[] alphas = new EvolvingDouble[] { new EvolvingDouble(1e-5) };
            //EvolvingDouble[] gammas = new EvolvingDouble[] { new EvolvingDouble(1e-5) };
            //EvolvingDouble[] alphas = new EvolvingDouble[] { new EvolvingDouble(1e-6) };

            //Binary Tree
            //EvolvingDouble[] gammas = new EvolvingDouble[]{
            //    new EvolvingDouble(new double[]{0.45, 0.45, 0.15, 0.45, 0.45, 0.35, 0.3, 0.45, 0.45})};
            //EvolvingDouble[] alphas = new EvolvingDouble[]{
            //    new EvolvingDouble(new double[]{0.003, 0.003, 0.003, 0.001, 0.003, 0.003, 0.003, 0.003, 0.005})};
            //EvolvingDouble[] gammas = new EvolvingDouble[] { new EvolvingDouble(0.15) };
            //EvolvingDouble[] alphas = new EvolvingDouble[] { new EvolvingDouble(0.003) };

            //double[] gammas_double = new double[] { 0.01, 0.02, 0.03, 0.05, 0.08, 0.1, 0.12, 0.15, 0.18, 0.2, 0.22, 0.25, 0.28, 0.3, 0.35, 0.4, 0.45 }; //0.03, 0.05, 0.08, 0.1, 0.2, 0.3 };
            //double[] alphas_double = new double[] { 1e-5, 3e-5, 1e-4, 3e-4, 1e-3, 3e-3, 0.005, 0.01, 0.1 };
            //double[] gammas_double = new double[25]; 
            //double[] alphas_double = new double[] { 0.001, 0.002, 0.003, 0.004, 0.005 };
            //for (int i = 0; i < 25; i++) gammas_double[i] = 0.02 + 0.02 * i;
            //double[] alphas_double = new double[] { 5e-4 };
            //EvolvingDouble[] gammas = GetEvolvingDoubles(gammas_double);
            //EvolvingDouble[] alphas = GetEvolvingDoubles(alphas_double);
            #endregion

            int[] samplenumbers = new int[] { sampleNumber };
            ExperimentParameters.ConstraintRoseTreeWeights = new double[smoothnessOrders];
            for (int i = 0; i < constraintTreeNumber; i++)
                ExperimentParameters.ConstraintRoseTreeWeights[smoothnessOrders - i - 1] = 1;
            var edgeCost = 1;
            var ratio = Math.Pow(selectRatio, 1.0 / (constraintTreeNumber - 1));
            ExperimentParameters.RemoveConflictsParameters = selectRatio == 1 ? null : (new RemoveConflictParameters(ratio, 1, 1, 1, edgeCost, edgeCost, edgeCost));
            ExperimentParameters.BNormalizeConstraintRoseTreeWeights = false;
            int constraintTreeNumberLimit = ExperimentParameters.ConstraintRoseTreeWeights.Length;

            /// Constraint ///
            #region constraint weight 
            ConstraintParameter[] constraints = new ConstraintParameter[]{
                //new NoConstraintParameter(),
                //new OrderConstraintParameter(1e-300, 0),

#region distance
                //--------------------Distance Extend----------
                //new DistanceConstraintParameter(1e-35),
                //new DistanceConstraintParameter(1e-30),
                //new DistanceConstraintParameter(1e-25),
                //new DistanceConstraintParameter(1e-20),
                //new DistanceConstraintParameter(1e-15),
                //new DistanceConstraintParameter(1e-10),
                //new DistanceConstraintParameter(1e-5),
                //new DistanceConstraintParameter(1e0),
                //new DistanceConstraintParameter(1e5),
                //new DistanceConstraintParameter(1e10),
                //new DistanceConstraintParameter(1e15),
                //--------------------Distance
                //new DistanceConstraintParameter(1e-25),
                //new DistanceConstraintParameter(1e-20),
                //new DistanceConstraintParameter(1e-15),
                //new DistanceConstraintParameter(1e-10),
                //new DistanceConstraintParameter(1e-5),
                //new DistanceConstraintParameter(1e0),
                //new DistanceConstraintParameter(1e5),
                //new DistanceConstraintParameter(1e10),
                //--------------------DistanceS0
                //new DistanceConstraintParameter(1e-25),
                //--------------------DistanceExtend2
                //new DistanceConstraintParameter(1e-25),
                //new DistanceConstraintParameter(1e-20),
                //new DistanceConstraintParameter(1e-15),
                //new DistanceConstraintParameter(1e-10),
                //new DistanceConstraintParameter(1e-5),
                //new DistanceConstraintParameter(1e0),
                //new DistanceConstraintParameter(1e5),
                //new DistanceConstraintParameter(1e10),
                //new DistanceConstraintParameter(1e15),
                //new DistanceConstraintParameter(1e20),
                //new DistanceConstraintParameter(1e25),
                //--------------------DistanceExtend3
                //new DistanceConstraintParameter(1e-30),
                //new DistanceConstraintParameter(1e-25),
                //new DistanceConstraintParameter(1e-20),
                //new DistanceConstraintParameter(1e-15),
                //new DistanceConstraintParameter(1e-10),
                //new DistanceConstraintParameter(1e-5),
                //new DistanceConstraintParameter(1e0),
                //new DistanceConstraintParameter(1e5),
                //new DistanceConstraintParameter(1e10),
                //--------------------Distance 2
                //new DistanceConstraintParameter(1e-20),
                //new DistanceConstraintParameter(1e-15),
                //new DistanceConstraintParameter(1e-10),
                //new DistanceConstraintParameter(1e-5),
                //new DistanceConstraintParameter(1e0),
                //new DistanceConstraintParameter(1e5),
                //new DistanceConstraintParameter(1e10),
                //new DistanceConstraintParameter(1e15),
                //new DistanceConstraintParameter(1e20),
                //--------------------Distance 3
                //new DistanceConstraintParameter(1e-10),
                //new DistanceConstraintParameter(3e-8),
                //new DistanceConstraintParameter(1e-5),
                //new DistanceConstraintParameter(3e-3),
                //new DistanceConstraintParameter(1e0),
                //new DistanceConstraintParameter(3e2),
                //new DistanceConstraintParameter(1e5),
                //new DistanceConstraintParameter(3e7),

#endregion
#region order
                
                //new LooseOrderConstraintParameter(0,0),
                //new LooseOrderConstraintParameter(1e-10,1e-10),
                //new LooseOrderConstraintParameter(3e-10,3e-10),
                //new LooseOrderConstraintParameter(1e-9,1e-9),
                //new LooseOrderConstraintParameter(3e-9,3e-9),
                //new LooseOrderConstraintParameter(1e-8,1e-8),
                //new LooseOrderConstraintParameter(3e-8,3e-8),
                //new LooseOrderConstraintParameter(1e-7,1e-7),
                //new LooseOrderConstraintParameter(3e-7,3e-7),
                //new LooseOrderConstraintParameter(1e-6, 1e-6),
                //---LOrder
                //new LooseOrderConstraintParameter(3e-6,3e-6),
                //new LooseOrderConstraintParameter(1e-5,1e-5),
                //new LooseOrderConstraintParameter(3e-5,3e-5),
                //new LooseOrderConstraintParameter(1e-4,1e-4),
                //new LooseOrderConstraintParameter(3e-4,3e-4),
                //new LooseOrderConstraintParameter(1e-3,1e-3),
                //new LooseOrderConstraintParameter(3e-3,3e-3),
                //new LooseOrderConstraintParameter(1e-2,1e-2),
                //---Order
                //new OrderConstraintParameter(3e-6,3e-6),
                //new OrderConstraintParameter(1e-5,1e-5),
                //new OrderConstraintParameter(3e-5,3e-5),
                //new OrderConstraintParameter(1e-4,1e-4),
                //new OrderConstraintParameter(3e-4,3e-4),
                //new OrderConstraintParameter(1e-3,1e-3),
                //new OrderConstraintParameter(3e-3,3e-3),
                //new OrderConstraintParameter(1e-2,1e-2),
                //--Order0
                //new OrderConstraintParameter(1e-4,1e-4),
                //--Order1
                //new OrderConstraintParameter(1e-3,1e-3),
                //--Order2
                //new OrderConstraintParameter(1e-2,1e-2),
                //--Order3
                //new OrderConstraintParameter(1,1),
                //--Order4
                //new OrderConstraintParameter(10,10),
                //-----------LOrderTest
                //new LooseOrderConstraintParameter(1e-6,1e-6),
                //new LooseOrderConstraintParameter(1e-4,1e-4),
                //new LooseOrderConstraintParameter(1e-2,1e-2),
                //-----------OrderTest(OT)
                //new OrderConstraintParameter(1e-6,1e-6),
                //new OrderConstraintParameter(1e-4,1e-4),
                //new OrderConstraintParameter(1e-2,1e-2),
                //-----------OrderTest2(OT2)
                //new OrderConstraintParameter(3e-6,3e-6),
                //new OrderConstraintParameter(1e-4,1e-4),
                //new OrderConstraintParameter(1e-2,1e-2),
                //-----------OrderTest(OT3)
                //new OrderConstraintParameter(1e-6,1e-6),
                //new OrderConstraintParameter(1e-4,1e-4),
                //new OrderConstraintParameter(1e-2,1e-2),
                //new OrderConstraintParameter(1,1),
                //new OrderConstraintParameter(10,10),
                //-----------OrderTest(OT4)
                new OrderConstraintParameter(1e-3,1e-3),
                new OrderConstraintParameter(1e-2,1e-2),
                new OrderConstraintParameter(1e-1,1e-1),
                new OrderConstraintParameter(1,1),
                //-----------5LOrderTest(5LOT)
                //new LooseOrderConstraintParameter(5e-6,5e-6),
                //new LooseOrderConstraintParameter(5e-4,5e-4),
                //new LooseOrderConstraintParameter(5e-2,5e-2),
                //-----------LOrderTest2
                //new LooseOrderConstraintParameter(1e-5,1e-5),
                //new LooseOrderConstraintParameter(3e-5,3e-5),
                //new LooseOrderConstraintParameter(1e-4,1e-4),
                //new LooseOrderConstraintParameter(3e-4,3e-4),
                //new LooseOrderConstraintParameter(1e-3,1e-3),
                //new LooseOrderConstraintParameter(3e-3,3e-3),
                //-----------LOrderTest3(LOT3)
                //new LooseOrderConstraintParameter(1e-6,1e-6),
                //new LooseOrderConstraintParameter(1e-4,1e-4),
                //new LooseOrderConstraintParameter(1e-2,1e-2),
                //new LooseOrderConstraintParameter(1,1),
                //new LooseOrderConstraintParameter(10,10),
                //-----------OrderSF Extend
                //new OrderConstraintParameter(3e-6,0),
                //new OrderConstraintParameter(1e-5,0),
                //new OrderConstraintParameter(3e-5,0),
                //new OrderConstraintParameter(1e-4,0),
                //new OrderConstraintParameter(3e-4,0),
                //new OrderConstraintParameter(1e-3,0),
                //new OrderConstraintParameter(3e-3,0),
                //new OrderConstraintParameter(1e-2,0),
                //new LooseOrderConstraintParameter(3e-2, 0),
                //-----------OrderSFExtend3
                //new OrderConstraintParameter(6e-6,1e-20),
                //new OrderConstraintParameter(2e-5,1e-20),
                //new OrderConstraintParameter(6e-5,1e-20),
                //new OrderConstraintParameter(2e-4,1e-20),
                //new OrderConstraintParameter(6e-4,1e-20),
                //new OrderConstraintParameter(2e-3,1e-20),
                //new OrderConstraintParameter(6e-3,1e-20),
                //new OrderConstraintParameter(2e-2,1e-20),
                //-----------OrderSFOrg
                //new OrderConstraintParameter(3e-6,1e-20),
                //new OrderConstraintParameter(1e-5,1e-20),
                //new OrderConstraintParameter(3e-5,1e-20),
                //new OrderConstraintParameter(1e-4,1e-20),
                //new OrderConstraintParameter(3e-4,1e-20),
                //new OrderConstraintParameter(1e-3,1e-20),
                //new OrderConstraintParameter(3e-3,1e-20),
                //new OrderConstraintParameter(1e-2,1e-20),
                //--------------

                //new LooseOrderConstraintParameter(1e100,1e100),
#endregion
              };
            #endregion

            /// Evolutionary Parameters ///
            string indexpath = null;
            string defaultfield = null;
            string[] querystrings = null;
            #region dataset
            switch (newsdataset)
            {
                case DataSetType.NewYorkTimes:
                    ExperimentParameters.DatasetIndex = Constant.NEW_YORK_TIMES;

                    indexpath = ExperimentParameters.NewYorkTimesPath;
                    defaultfield = "Cleaned Taxonomic Classifier";
                    string rawqueryStr = "(Top/Features/Arts OR Top/Features/Style/ OR Top/Features/Travel/ OR " +
                        "Top/News/Business/ OR Top/News/Sports)";
                    int deltamonthpertime = 2; //month
                    int startyear = 2006, startmonth = 1;
                    int[] time = new int[9]; //[21];
                    querystrings = new string[time.Length];
                    for (int itime = 0; itime < time.Length; itime++)
                        querystrings[itime] = GetQueryString(itime, rawqueryStr, startyear, startmonth, deltamonthpertime);
                    break;
                case DataSetType.BingNewsObama:
                    ExperimentParameters.DatasetIndex = Constant.INDEXED_BING_NEWS;

                    indexpath = ExperimentParameters.DataPath + @"data\BingNewsData\BingNewsIndex_Obama\BingNews_Obama_Sep_4months4_RemoveSimilar_RemoveNoise_4Words\";
                    defaultfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
                    rawqueryStr = "Obama";
                    string startdate = "2012-9-10";
                    int timespan = 1;
                    int timeslotsnum = 6;
                    int deltatime = timespan;

                    DateTime startdatetime = EvolutionaryExperiments.GetDateTime(startdate);
                    querystrings = new string[timeslotsnum];
                    for (int itime = 0; itime < timeslotsnum; itime++)
                        querystrings[itime] = rawqueryStr + " AND " +
                            EvolutionaryExperiments.GetIndexedBingNewsDateQuery(startdatetime, timespan, itime, deltatime);
                    break;
                case DataSetType.BingNewsDebt:
                    ExperimentParameters.DatasetIndex = Constant.INDEXED_BING_NEWS;

                    indexpath = ExperimentParameters.DataPath + @"data\BingNewsData\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar\";
                    //indexpath = ExperimentParameters.DataPath + @"data\BingNewsData\debt_crisis2\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar2\";
                    defaultfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
                    rawqueryStr = "debt crisis";
                    startdate = "2012-2-1";
                    timespan = 14;
                    timeslotsnum = 10;
                    deltatime = timespan;

                    startdatetime = EvolutionaryExperiments.GetDateTime(startdate);
                    querystrings = new string[timeslotsnum];
                    for (int itime = 0; itime < timeslotsnum; itime++)
                        querystrings[itime] = rawqueryStr + " AND " +
                            EvolutionaryExperiments.GetIndexedBingNewsDateQuery(startdatetime, timespan, itime, deltatime);
                    break;
                default:
                    throw new NotImplementedException();
            }
            #endregion

            #region not always used parameters
            AlgorithmParameter[] algorithms = new AlgorithmParameter[]{
                 new KNNAlgorithmParameter(50)};
            int[] models = new int[]{
                 Constant.DCM};

            double[] overlapratio = new double[] { -1 };
            
            /// Rule ///
            Rules[] rules = new Rules[] { new Rules() };

            DataProjection.AbandonCosineThreshold = -1; // 0.25;
            if (BuildRoseTree.BRestrictBinary)
                DataProjection.AbandonTreeDepthThreshold = 500;//int.MaxValue;//
            else
                DataProjection.AbandonTreeDepthThreshold = 4;//4
            if (!BuildRoseTree.BRestrictBinary) DataProjection.DocumentSkipPickedCount = 2;
            DataProjection.NewTopicAlpha = 0; //0;
            DataProjection.DocumentCutGain = 1;//1
            DataProjection.DocumentTolerateCosine = 0.2;//0.2
            //DataProjection.AbandonCosineThreshold = -1;
            //DataProjection.AbandonTreeDepthThreshold = 10;//4
            //DataProjection.DocumentSkipPickedCount = 2;
            //DataProjection.NewTopicAlpha = 0;
            //DataProjection.DocumentCutGain = 0;//1
            //DataProjection.DocumentTolerateCosine = -1;//0.2
            #endregion not always used parameters

            #region output files
            StreamWriter ofile = InitializeResultPrinter();
            RobinsonFouldsDistance.ofile = ofile;

            InitializeDrawRoseTreePath();
            StreamWriter ofileMatlab = InitializeMatlabDataFunctionPrinterEvolutionary(
                sampletimes, samplenumbers, overlapratio, models, algorithms, rules,
                gammas, alphas, constraints, querystrings, indexpath);
#if PRINT_VIOLATION_CURVE
            StreamWriter ofileVioCurve = InitializeViolationCurve();
            ExperimentParameters.ViolationCurveFile = ofileVioCurve;
#endif
            #endregion output files

            int experimentNumber = alphas.Length * gammas.Length * samplenumbers.Length * overlapratio.Length
       * algorithms.Length * models.Length * sampletimes.Length * rules.Length * constraints.Length * querystrings.Length;
            int experimentIndex = 0;


            DateTime time_begin = DateTime.Now;
            for (int isampletimes = 0; isampletimes < sampletimes.Length; isampletimes++)
            {
                ExperimentParameters.SampleTimes = sampletimes[isampletimes];
                ofile.WriteLine("////////////////////////SampleTimes:" + sampletimes[isampletimes] + "////////////////////////");
                for (int isamplenum = 0; isamplenum < samplenumbers.Length; isamplenum++)
                {
                    ExperimentParameters.SampleNumber = samplenumbers[isamplenum];
                    ofile.WriteLine("////////////////////////SampleNum:" + samplenumbers[isamplenum] + "////////////////////////");
                    for (int ioverlapratio = 0; ioverlapratio < overlapratio.Length; ioverlapratio++)
                    {
                        ExperimentParameters.SampleOverlapRatio = overlapratio[ioverlapratio];
                        ofile.WriteLine("////////////////////////SampleOverlapRatio:" + overlapratio[ioverlapratio] + "////////////////////////");   
                        ExperimentParameters.SampleTimes = sampletimes[isampletimes];
                        ExperimentParameters.SampleOverlapRatio = -1;
                        for (int imodel = 0; imodel < models.Length; imodel++)
                        {
                            ExperimentParameters.ModelIndex = models[imodel];
                            ofile.WriteLine("////////////////////////" + (models[imodel] == Constant.DCM ? "DCM" : "vMF") + "////////////////////////");
                            // use the same ldinfo for all below parameters
                            for (int ialgorithm = 0; ialgorithm < algorithms.Length; ialgorithm++)
                            {
                                algorithms[ialgorithm].Set();
                                ofile.WriteLine("////////////////////////" + algorithms[ialgorithm] + "////////////////////////");
                                for (int irule = 0; irule < rules.Length; irule++)
                                {
                                    ExperimentParameters.Rules = rules[irule];
                                    ofile.WriteLine("---------------------Rule:" + irule + "---------------------");
                                    for (int igamma = 0; igamma < gammas.Length; igamma++)
                                    {
                                        EvolvingDouble gamma = gammas[igamma];
                                        ofile.WriteLine("---------------------Gamma:" + gammas[igamma] + "---------------------");
                                        for (int ialpha = 0; ialpha < alphas.Length; ialpha++)
                                        {
                                            EvolvingDouble alpha = alphas[ialpha];
                                            ofile.WriteLine("---------------------Alpha:" + alphas[ialpha] + "---------------------");
                                            ofile.WriteLine(DateTime.Now);

                                            //List<double> loglikelihoodStdRecord = null;

                                            for (int iconstraint = 0; iconstraint < constraints.Length; iconstraint++)
                                            {
                                                ofile.WriteLine("<Constraint " + iconstraint + ">");
                                                ConstraintParameter constraint = constraints[iconstraint];
                                                /// Evolutionary ///
                                                ExperimentParameters.NewYorkTimesPath = ExperimentParameters.IndexedBingNewsPath = indexpath;
                                                ExperimentParameters.LoadDataQueryDefaultField = defaultfield;
                                                RoseTree previousRoseTree = null;
                                                ExperimentParameters.ConstraintRoseTree = null;
                                                ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;

                                                List<RoseTree> constraintrosetrees = new List<RoseTree>();
                                                for (int itime = 0; itime < querystrings.Length; itime++)
                                                {
                                                    ExperimentParameters.RoseTreeParameters.gamma = gamma.GetValue(itime);
                                                    ExperimentParameters.RoseTreeParameters.alpha = alpha.GetValue(itime);
                                                    ExperimentParameters.LoadDataQueryString = querystrings[itime];

                                                    #region intialization
                                                    double loglikelihoodRes = -1;
                                                    RoseTreeStructureInfo[] structureInfo = null;
                                                    double[] accuracyRes = null;
                                                    double[] smoothnessCost = null;
                                                    RoseTree rosetree = null;
                                                    double runningTime = -1;
                                                    double smoothnessCostUnbiased = -1;
                                                    #endregion intialization

                                                    //try
                                                    {
                                                        //ExperimentRoseTree.LoadDataInfo();

                                                        smoothnessCost = null;
                                                        runningTime = -1;

                                                        ofile.Write("Exp {0}\t", experimentIndex);
                                                        ofile.Write("Rule: " + irule + "\t");
                                                        PrintRoseTreeParametersEvolutionary(ofile);
                                                        //RoseTree.MergeRecordFileName = experimentIndex + "";
                                                        //ExperimentParameters.CacheValueRecordFileName = experimentIndex + "";
                                                        ExperimentRoseTree.ExperimentIndex = experimentIndex;

                                                        DateTime t0 = DateTime.Now;
                                                        rosetree = ExperimentRoseTree.GetRoseTree();
                                                        DateTime t1 = DateTime.Now;
                                                        runningTime = (t1.Ticks - t0.Ticks) / 1e7;

                                                        PrintRoseTreeStructure(rosetree, ofile);

                                                        loglikelihoodRes = (rosetree as ConstrainedRoseTree).LogLikelihood;
                                                        ofile.WriteLine("loglikelihood: " + loglikelihoodRes);

                                                        structureInfo = rosetree.StructureInfo();

                                                        LoadDataInfo ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo;
                                                        ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
#if !TUNE_NTY_PARAMETERS
                                                        //RoseTree gtrosetree = ExperimentRoseTree.GetRoseTree(ldinfo);
                                                        //accuracyRes = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);

                                                        smoothnessCost = (rosetree as ConstrainedRoseTree).GetNormalizedSmoothnessCost();
                                                        smoothnessCostUnbiased = 1;
                                                        try
                                                        {
                                                            if (previousRoseTree != null) smoothnessCostUnbiased = 1 - RobinsonFouldsDistance.CalculateDistance(previousRoseTree, rosetree);
                                                        }
                                                        catch
                                                        {
                                                            ofile.WriteLine("Calculate RF failed!");
                                                            smoothnessCostUnbiased = -1;
                                                        }
                                                        ofile.WriteLine("Smoothness: " + smoothnessCost[0] + "\t" + smoothnessCost[1] + "\t" + smoothnessCostUnbiased);
                                                        smoothnessCost = new double[] { smoothnessCost[0], smoothnessCost[1], smoothnessCostUnbiased };
#endif
                                                        ofileMatlab.WriteLine(ExperimentResultToString(ofile, rosetree, loglikelihoodRes, smoothnessCost, runningTime, structureInfo, accuracyRes));
#if PRINT_VIOLATION_CURVE
                                                        ofileVioCurve.WriteLine();
#endif

                                                        /// draw tree ///
                                                        try
                                                        {
                                                            ExperimentRoseTree.DrawRoseTree(rosetree, experimentIndex + "");
                                                            ExperimentRoseTree.DrawRoseTree(rosetree, experimentIndex + "_i", true);
                                                            ExperimentRoseTree.DrawConstraintTree(rosetree, experimentIndex + "c");
                                                            ExperimentRoseTree.DrawConstraintTree(rosetree, experimentIndex + "c_i", true);
                                                        }
                                                        catch (Exception edrawtree)
                                                        {
                                                            ofile.WriteLine("DrawTreeFailed: " + edrawtree.Message);
                                                        }

                                                        /// set constraint tree ///
                                                        if (constraintrosetrees.Count >= constraintTreeNumberLimit)
                                                            constraintrosetrees.RemoveAt(0);
                                                        constraintrosetrees.Add(rosetree);
                                                        constraint.Set(constraintrosetrees);
                                                        previousRoseTree = rosetree;
                                                        //(rosetree as ConstrainedRoseTree).ExperimentIndex = experimentIndex;

                                                        experimentIndex++;

                                                        PrintProgress(experimentIndex, experimentNumber, time_begin);
                                                    }
                                                    //catch (Exception e)
//                                                    {
//                                                        ofile.WriteLine("Failed: " + e.Message);
//                                                        ofile.Flush();
//                                                        ofile.WriteLine(e.StackTrace);
//                                                        ofileMatlab.WriteLine(ExperimentResultToString(ofile, rosetree, loglikelihoodRes, smoothnessCost, runningTime, structureInfo, accuracyRes));
//#if PRINT_VIOLATION_CURVE
//                                                        ofileVioCurve.WriteLine();
//#endif                                                        
//                                                        experimentIndex++;
//                                                    }
                                                    ofile.Flush();
                                                    ofileMatlab.Flush();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if PRINT_VIOLATION_CURVE
            ofileVioCurve.Close();
#endif
            EndMatlabDataFunction(ofileMatlab);
            ofile.WriteLine("\n=============================Final Result=============================");
            ofile.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                sampletimes.Length, samplenumbers.Length, overlapratio.Length,
                models.Length, algorithms.Length, rules.Length,
                gammas.Length, alphas.Length, constraints.Length);
            ofile.Write(ExperimentParametersToString("Sample Time", sampletimes.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Sample Number", samplenumbers.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Sample Overlap Ratio", overlapratio.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Model", models.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Algorithm", algorithms.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Rule", rules.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Gamma", gammas.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Alpha", alphas.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Constraint", constraints.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("QueryString", querystrings.Cast<object>().ToArray<object>()));
            //ofile.WriteLine("----------------------------------------------------------------------");
            //for (int i = 0; i < experimentNumber; i++)
            //    ofile.Write(ExperimentResultToString(structureInfo[i], accuracyRes[i], loglikelihoodRes[i]) + "\t");
            ofile.WriteLine("\n======================================================================");
            ofile.WriteLine(TimeEfficiencyToString(experimentNumber, time_begin));

            ofile.Close();
        }

        private static string GetSampleTimesString(int[] samplenumbers)
        {
            string str = "ST";
            for (int i = 0; i < samplenumbers.Length; i++)
            {
                str += samplenumbers[i];
            }
            return str;
        }


        private static EvolvingDouble[] GetEvolvingDoubles(double[] num_double)
        {
            EvolvingDouble[] num = new EvolvingDouble[num_double.Length];
            for (int inum = 0; inum < num_double.Length; inum++)
                num[inum] = new EvolvingDouble(num_double[inum]);
            return num;
        }

        public static string GetQueryString(int itime,
            string rawqueryStr, int startyear, int startmonth, int deltamonthpertime)
        {
            //if(12%deltamonthpertime!=0 || (startmonth-1)%deltamonthpertime!=0)
            //    throw new Exception("Error! Illegal input time format!");

            DateTime startdate = new DateTime(startyear, startmonth, 1);
            int addedmonths = itime * deltamonthpertime;
            startdate = startdate.AddMonths(addedmonths);

            string str = rawqueryStr + " AND Publication\\ Year:" + startdate.Year + " AND (";
            for (int month = startdate.Month; month < deltamonthpertime + startdate.Month; month++)
            {
                if (month > 12)
                    throw new Exception("Error! Illegal input time format2!");
                if (month != startdate.Month)
                    str += " OR ";
                str += "Publication\\ Month:" + month;
            }
            str += ")";
            return str;
        }

        public static void TuneParameters_DrawRoseTree()
        {
            /// SCALABILITY_TEST ////
            /// PROJ_WEIGHT_1, PROJ_WEIGHT_3 ///
            /// PRINT_VIOLATION_CURVE, KDD_EXP_STRING ////

            //DataProjTest: Default PW2
            ExperimentParameters.Description = "Exp1_DProj_W0C0_PredAllAY500_2_" + ExperimentParameters.Description;
            Constraint.DataProjectionType = DataProjectionType.DataPredictionNodeDepthWeighted;
            GroundTruthRoseTree.BBulidGroundTruthTree = true;
            LooseTreeOrderConstraint.LooseOrderDeltaRatio = 0.4;// -1;
            StreamWriter ofile = InitializeResultPrinter();
            
            InitializeDrawRoseTreePath();
            ofile.WriteLine("DataSet:\t" + ExperimentParameters.TwentyNewsGroupPath);
            ofile.WriteLine("Data Projection Type:" + Constraint.DataProjectionType);
            if (!ExperimentParameters.TwentyNewsGroupPath.Contains("17"))
                throw new Exception("Wrong GTClusterNumber for ConstrainedBayesionBinaryTree!");

            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine("DataSet:\t" + ExperimentParameters.TwentyNewsGroupPath);
            Console.WriteLine("Data Projection Type:" + Constraint.DataProjectionType);
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

            double[] gammas = new double[] { 0.1 };
            double[] alphas = new double[] { 0.4 }; //{ 0.01 };
            
            //double[] gammas = new double[] { 0.01, 0.03, 0.05, 0.08, 0.1, 0.3, 0.5 }; //0.03, 0.05, 0.08, 0.1, 0.2, 0.3 };
            //double[] alphas = new double[] { 1e-6, 1e-5, 1e-4, 1e-3, 0.005, 0.01, 0.05, 0.1 };

            int[] samplenumbers = new int[] { 2000 }; //{ 2000 };
            AlgorithmParameter[] algorithms = new AlgorithmParameter[]{
                new KNNAlgorithmParameter(500),//50
                //new BRTAlgorithmParameter(),
            };
            int[] models = new int[]{
                 Constant.DCM};
            double[] overlapratio = new double[] { 0, 0.2, 0.5, 0.8, 1 };//0, 0.2, 0.5, 0.8, 1
            int[] sampletimes = new int[] { 1, 2, 3, 4, 5 };//, 6, 7, 8, 9, 10
            int[] infosampletimes = new int[] { 1, 2, 3, 4, 5 };//, 6, 7, 8, 9, 10
            /// Rule ///
            //Rules rules0 = new Rules();
            //rules0.AddMaxRule(100, 100, Int32.MaxValue);
            Rules[] rules = new Rules[] { new Rules() };

            #region not always used parameter
            DataProjection.AbandonCosineThreshold = -1; //-1
            DataProjection.AbandonTreeDepthThreshold = 10;//10
            DataProjection.DocumentSkipPickedCount = 2;//2
            DataProjection.NewTopicAlpha = 1e-500;//1e-30
            DataProjection.DocumentCutGain = 0;//0
            DataProjection.DocumentTolerateCosine = 0;//0
            DataProjection.ClusterSizeWeight = 1;//0
            DataProjection.NewTopicAlphaCosine = 1;
            //DataProjection.NewTopicAlpha = 1e-30;// 1e-30;
            //DataProjection.ClusterSizeWeight = 1; //0;
            //DataProjection.AbandonCosineThreshold = 0.25; //-1;
            //DataProjection.AbandonTreeDepthThreshold = 10;//4
            //DataProjection.DocumentSkipPickedCount = 2;
            //DataProjection.DocumentCutGain = 0;//1
            //DataProjection.DocumentTolerateCosine = 0;//0.2
            ofile.WriteLine("New Topic Alpha:" + DataProjection.NewTopicAlpha);
            ofile.WriteLine("New Topic Alpha Cosine:" + DataProjection.NewTopicAlphaCosine);
            ofile.WriteLine("Cluster Size Weight:" + DataProjection.ClusterSizeWeight);
            ofile.WriteLine("AbandonCosineThreshold:" + DataProjection.AbandonCosineThreshold);
#if PROJ_WEIGHT_1
            ofile.WriteLine("PROJ_WEIGHT_1");
#elif PROJ_WEIGHT_3
            ofile.WriteLine("PROJ_WEIGHT_3");
#else
            ofile.WriteLine("PROJ_WEIGHT_2");
#endif
            #endregion
            /// Constraint ///
            ConstraintParameter[] constraints = new ConstraintParameter[]{
                //new NoConstraintParameter(false),
                //new DistanceConstraintParameter(1e-50,false),
                //new DistanceConstraintParameter(1e-35,false),
                //new DistanceConstraintParameter(1e-30,false),
                //new DistanceConstraintParameter(1e-25,false),
                //new DistanceConstraintParameter(1e-20,false),
                //new DistanceConstraintParameter(1e-15,false),
                //new DistanceConstraintParameter(1e-10,false),
                //new DistanceConstraintParameter(1e-5,false),
                //new DistanceConstraintParameter(1,false),
                //new DistanceConstraintParameter(1e5,false),
                //new DistanceConstraintParameter(1e10,false),
                //new DistanceConstraintParameter(1e15,false),
                //new DistanceConstraintParameter(1e15,false),
                //new DistanceConstraintParameter(1e50),
                //new LooseOrderConstraintParameter(1e-50,1e-50,false),
                //new LooseOrderConstraintParameter(1e-14,1e-14,false),
                //new LooseOrderConstraintParameter(1e-12,1e-12,false),
                //new LooseOrderConstraintParameter(1e-10,1e-10,false),
                //new LooseOrderConstraintParameter(0,0,false),
                //new LooseOrderConstraintParameter(1e-8,1e-8,false),
                //new LooseOrderConstraintParameter(1e-6,1e-6,false),
                //new LooseOrderConstraintParameter(1e-4,1e-4,false),
                //new LooseOrderConstraintParameter(1e-2,1e-2,false),
                //new LooseOrderConstraintParameter(1e0,1e0,false),
                //new LooseOrderConstraintParameter(1e2,1e2,false),
                //new LooseOrderConstraintParameter(1e50,1e50,false),
                new OrderConstraintParameter(1e100,1e100,false),
              };

            StreamWriter ofileMatlab = InitializeMatlabDataFunctionPrinter(
                sampletimes, samplenumbers, models, overlapratio, algorithms, rules,
                gammas, alphas, constraints, infosampletimes);
#if PRINT_VIOLATION_CURVE
            StreamWriter ofileVioCurve = InitializeViolationCurve();
            ExperimentParameters.ViolationCurveFile = ofileVioCurve;
#endif
            if (infosampletimes.Length != sampletimes.Length)
                throw new Exception("[ERROR]Constraint Sample Times Length Does Not Match!");

            //records of result
            int experimentNumber = alphas.Length * gammas.Length * samplenumbers.Length * overlapratio.Length
                * algorithms.Length * models.Length * sampletimes.Length * rules.Length * constraints.Length;
            RoseTreeStructureInfo[][] structureInfo = new RoseTreeStructureInfo[experimentNumber][];
            double[][] accuracyRes = new double[experimentNumber][];
            double[] loglikelihoodRes = new double[experimentNumber];
            double[] smoothnessCost = null;
            int experimentIndex = 0;

            RoseTree rosetree = null;
            double runningTime = -1;

            DateTime time_begin = DateTime.Now;
            DateTime time_stop = DateTime.Now;//, time_stop_2;
            for (int isampletimes = 0; isampletimes < sampletimes.Length; isampletimes++)
            {
                ExperimentParameters.SampleTimes = sampletimes[isampletimes];
                ofile.WriteLine("////////////////////////SampleTimes:" + sampletimes[isampletimes] + "////////////////////////");
                for (int isamplenum = 0; isamplenum < samplenumbers.Length; isamplenum++)
                {
                    ExperimentParameters.SampleNumber = samplenumbers[isamplenum];
                    ofile.WriteLine("////////////////////////SampleNum:" + samplenumbers[isamplenum] + "////////////////////////");
                    // use the same ground truth tree for all below parameters
                    ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
                    ExperimentParameters.SampleOverlapRatio = -1;
                    for (int imodel = 0; imodel < models.Length; imodel++)
                    {
                        ExperimentParameters.ModelIndex = models[imodel];
                        ofile.WriteLine("////////////////////////" + (models[imodel] == Constant.DCM ? "DCM" : "vMF") + "////////////////////////");
                        //RoseTree.MergeRecordFileName = experimentIndex + "_gt";
                        RoseTree gtrosetree = ExperimentRoseTree.GetRoseTree();
                        //ExperimentRoseTree.DrawRoseTree(gtrosetree, experimentIndex + "_gt");
                        for (int ioverlapratio = 0; ioverlapratio < overlapratio.Length; ioverlapratio++)
                        {
                            ExperimentParameters.SampleOverlapRatio = overlapratio[ioverlapratio];
                            ofile.WriteLine("////////////////////////SampleOverlapRatio:" + overlapratio[ioverlapratio] + "////////////////////////");
                            // use the same constraint tree for all below parameters
                            ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
                            ExperimentParameters.SampleTimes = infosampletimes[isampletimes];
                            //RoseTree.MergeRecordFileName = experimentIndex + "_c0";
                            RoseTree consrosetree = ExperimentRoseTree.GetRoseTree();
                            //ExperimentParameters.RoseTreeKeyWordNumber = 1;
                            //ExperimentRoseTree.DrawRoseTree(consrosetree, experimentIndex + "_ctree");
                            //ExperimentRoseTree.DrawRoseTree(consrosetree, experimentIndex + "_ctree_i", true);
                            //ofile.WriteLine("ConstraintTreeSampleFile: " + consrosetree.lfv.samplefilename);
                            // draw constraint tree after data projection
                            //ConstraintTree constrainttree = new ConstraintTree(consrosetree, gtrosetree.lfv);
                            //ExperimentRoseTree.DrawConstraintTree(constrainttree, "_c.gv");
                            //constrainttree.OutputDataProjectionResult(ExperimentParameters.OutputDebugPath + "DataProjection.dat");

                            // use the original data to build the tree
                            ExperimentParameters.SampleTimes = sampletimes[isampletimes];
                            ExperimentParameters.SampleOverlapRatio = -1;
                            // use the same ldinfo for all below parameters
                            LoadDataInfo ldinfo = ExperimentRoseTree.LoadDataInfo();
                            for (int ialgorithm = 0; ialgorithm < algorithms.Length; ialgorithm++)
                            {
                                algorithms[ialgorithm].Set();
                                ofile.WriteLine("////////////////////////" + algorithms[ialgorithm] + "////////////////////////");
                                for (int irule = 0; irule < rules.Length; irule++)
                                {
                                    ExperimentParameters.Rules = rules[irule];
                                    ofile.WriteLine("---------------------Rule:" + irule + "---------------------");
                                    for (int igamma = 0; igamma < gammas.Length; igamma++)
                                    {
                                        ExperimentParameters.RoseTreeParameters.gamma = gammas[igamma];
                                        ofile.WriteLine("---------------------Gamma:" + gammas[igamma] + "---------------------");
                                        for (int ialpha = 0; ialpha < alphas.Length; ialpha++)
                                        {
                                            ExperimentParameters.RoseTreeParameters.alpha = alphas[ialpha];
                                            ofile.WriteLine("---------------------Alpha:" + alphas[ialpha] + "---------------------");
                                            ofile.WriteLine(DateTime.Now);
                                            //List<double> loglikelihoodStdRecord = null;

                                            for (int iconstraint = 0; iconstraint < constraints.Length; iconstraint++)
                                            {
                                                constraints[iconstraint].Set(consrosetree, gtrosetree);
                                                ofile.WriteLine("<Constraint " + iconstraint + ">");
                                                double smoothnessCostUnbiased = -1;
                                                try
                                                {
                                                    rosetree = null;
                                                    smoothnessCost = null;
                                                    runningTime = -1;

                                                    //ofile.Write("Rule: " + irule + "\t");
                                                    PrintRoseTreeParameters(ofile, experimentIndex);

                                                    ofile.WriteLine(ldinfo.lfv.samplefilename);
                                                    //RoseTree.MergeRecordFileName = experimentIndex + "";
                                                    //ExperimentParameters.CacheValueRecordFileName = experimentIndex + "";

                                                    //time_stop_2 = DateTime.Now;
                                                    //Console.WriteLine("[TimeAnalysis] {0}s", (time_stop_2.Ticks - time_stop.Ticks) / 1e7);
                                                    //time_stop = DateTime.Now;
                                                    //ExperimentParameters.OriginalConstraintTreeFileName = experimentIndex + "co";

                                                    DateTime t0 = DateTime.Now;
                                                    rosetree = ExperimentRoseTree.GetRoseTree(ldinfo);
                                                    DateTime t1 = DateTime.Now;
                                                    runningTime = (t1.Ticks - t0.Ticks) / 1e7;

                                                    //time_stop_2 = DateTime.Now;
                                                    //Console.WriteLine("[TimeAnalysis] {0}s", (time_stop_2.Ticks - time_stop.Ticks) / 1e7);
                                                    //time_stop = DateTime.Now;

                                                    //ConstraintTree testconstree = new ConstraintTree(rosetree, rosetree.lfv);
                                                    //ExperimentRoseTree.DrawConstraintTree(testconstree, experimentIndex + "_testctree");
                                                    //ExperimentRoseTree.DrawConstraintTree(testconstree, experimentIndex + "_testctree_i", true);

                                                    PrintRoseTreeStructure(rosetree, ofile);
                                                    loglikelihoodRes[experimentIndex] = (rosetree as ConstrainedRoseTree).LogLikelihood;
                                                    ofile.WriteLine("loglikelihood: " + loglikelihoodRes[experimentIndex]);
                                                    structureInfo[experimentIndex] = rosetree.StructureInfo();
                                                    accuracyRes[experimentIndex] = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);
                                                    smoothnessCost = (rosetree as ConstrainedRoseTree).GetNormalizedSmoothnessCost();
                                                    smoothnessCostUnbiased = 1;
                                                    //if (consrosetree != null) smoothnessCostUnbiased = 1 - RobinsonFouldsDistance.CalculateDistance(consrosetree, rosetree);
                                                    try
                                                    {
                                                        if (consrosetree != null) smoothnessCostUnbiased = 1 - RobinsonFouldsDistance.CalculateDistance(gtrosetree, rosetree);
                                                    }
                                                    catch
                                                    {
                                                        smoothnessCostUnbiased = -1;
                                                    }
                                                    ofile.WriteLine("Smoothness: " + smoothnessCost[0] + "\t" + smoothnessCost[1] + "\t" + smoothnessCostUnbiased);
                                                    smoothnessCost = new double[] { smoothnessCost[0], smoothnessCost[1], smoothnessCostUnbiased };
                                                    ofileMatlab.WriteLine(ExperimentResultToString(ofile, rosetree, loglikelihoodRes[experimentIndex], smoothnessCost, runningTime, structureInfo[experimentIndex], accuracyRes[experimentIndex]));
#if PRINT_VIOLATION_CURVE
                                                    ofileVioCurve.WriteLine();
#endif
                                                    /// draw tree ///
                                                    //try
                                                    //{
                                                    //    ExperimentRoseTree.DrawRoseTree(rosetree, experimentIndex + "");
                                                    //    ExperimentRoseTree.DrawRoseTree(rosetree, experimentIndex + "_i", true);
                                                    //    ExperimentRoseTree.DrawConstraintTree(rosetree, experimentIndex + "c");
                                                    //    ExperimentRoseTree.DrawConstraintTree(rosetree, experimentIndex + "c_i", true);
                                                    //}
                                                    //catch (Exception edrawtree)
                                                    //{
                                                    //    ofile.WriteLine("DrawTreeFailed: " + edrawtree.Message);
                                                    //}

                                                    experimentIndex++;

                                                    PrintProgress(experimentIndex, experimentNumber, time_begin);

                                                    //time_stop_2 = DateTime.Now;
                                                    //Console.WriteLine("[TimeAnalysis] {0}s", (time_stop_2.Ticks - time_stop.Ticks) / 1e7);
                                                    //time_stop = DateTime.Now;
                                                }
                                                catch (Exception e)
                                                {
                                                    ofile.WriteLine("Failed: " + e.Message);
                                                    ofile.WriteLine(e.StackTrace);
                                                    ofileMatlab.WriteLine(ExperimentResultToString(ofile, rosetree, loglikelihoodRes[experimentIndex], smoothnessCost, runningTime, structureInfo[experimentIndex], accuracyRes[experimentIndex]));
#if PRINT_VIOLATION_CURVE
                                                    ofileVioCurve.WriteLine();
#endif      
                                                    experimentIndex++;
                                                }
                                                ofileMatlab.Flush();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if PRINT_VIOLATION_CURVE
            ofileVioCurve.Close();
#endif
            EndMatlabDataFunction(ofileMatlab);
            ofile.WriteLine("\n=============================Final Result=============================");
            ofile.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                sampletimes.Length, samplenumbers.Length, models.Length,
                overlapratio.Length, algorithms.Length, rules.Length,
                gammas.Length, alphas.Length, constraints.Length);
            ofile.Write(ExperimentParametersToString("Sample Time", sampletimes.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("[Info Sample Time]", infosampletimes.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Sample Number", samplenumbers.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Model", models.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Sample Overlap Ratio", overlapratio.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Algorithm", algorithms.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Rule", rules.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Gamma", gammas.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Alpha", alphas.Cast<object>().ToArray<object>()));
            ofile.Write(ExperimentParametersToString("Constraint", constraints.Cast<object>().ToArray<object>()));
            //ofile.WriteLine("----------------------------------------------------------------------");
            //for (int i = 0; i < experimentNumber; i++)
            //    ofile.Write(ExperimentResultToString(structureInfo[i], accuracyRes[i], loglikelihoodRes[i]) + "\t");
            ofile.WriteLine("\n======================================================================");
            ofile.WriteLine(TimeEfficiencyToString(experimentNumber, time_begin));

            ofile.Close();

            //Console.WriteLine("[Cosine Time]: {0}s", (SparseVectorList.CosineCalculateTimeSum / 1e7 / SparseVectorList.CosineCalculateTimeCnt));
            //Console.WriteLine("[Feature Vecter Count]: {0}", (SparseVectorList.FeatureVectorCount / 2.0 / SparseVectorList.CosineCalculateTimeCnt));
            //Console.WriteLine("[Feature Vecter Length]: {0}", (SparseVectorList.FeatureVectorLength / 2.0 / SparseVectorList.CosineCalculateTimeCnt));
            //Console.WriteLine("[New Feature Vecter Count]: {0}", ((double)ConstrainedRoseTree.FeatureVectorLength / ConstrainedRoseTree.FeatureVectorCnt));
            //Console.WriteLine("[New Feature Vecter Number]: {0}", ConstrainedRoseTree.FeatureVectorCnt);
            //Console.ReadKey();
        }

        public static void PrintProgress(int experimentIndex, int experimentNumber, DateTime time_begin)
        {
            DateTime now = DateTime.Now;
            double avgTime = (now.Ticks - time_begin.Ticks) / 1e7 / experimentIndex;
            double formatedAvgTime = Math.Floor(1000 * avgTime) / 1000;
            Console.WriteLine("==========Finish build tree {0} out of {1}, avg time {2}s==========", experimentIndex, experimentNumber, formatedAvgTime);
            double remainingTime = avgTime * (experimentNumber - experimentIndex);
            double hours, minutes, seconds;
            GetHourMinuteSecond(remainingTime, out hours, out minutes, out seconds);
            seconds = Math.Floor(seconds);
            if (hours > 0)
                Console.WriteLine("==========Remaining: {0} hours, {1} minutes, {2} seconds ==========", hours, minutes, seconds);
            else if (minutes > 0)
                Console.WriteLine("==========Remaining: {0} minutes, {1} seconds ==========", minutes, seconds);
            else
                Console.WriteLine("==========Remaining: {0} seconds ==========", seconds);
        }

        public static string TimeEfficiencyToString(int experimentNumber, DateTime time_begin)
        {
            DateTime now = DateTime.Now;
            double deltaTime = (now.Ticks - time_begin.Ticks) / 1e7;
            double avgTime = deltaTime / experimentNumber;
            double formatedAvgTime = Math.Floor(10000 * avgTime) / 10000;

            double hours, minutes, seconds;
            GetHourMinuteSecond(deltaTime, out hours, out minutes, out seconds);
            seconds = Math.Floor(seconds);

            string str = "";
            str += string.Format("Total experiment: {0}\n", experimentNumber);
            if (hours > 0)
                str += string.Format("Using time: {0} hours, {1} minutes, {2} seconds\n", hours, minutes, seconds);
            else if (minutes > 0)
                str += string.Format("Using time: {0} minutes, {1} seconds\n", minutes, seconds);
            else
                str += string.Format("Using time: {0} seconds\n", seconds);
            str += string.Format("Avg experiment time: {0}s\n", formatedAvgTime);

            return str;
        }

        public static void GetHourMinuteSecond(double deltatime, out double hours, out double minutes, out double seconds)
        {
            hours = minutes = 0;
            seconds = deltatime;
            if (seconds > 60)
            {
                minutes = Math.Floor(seconds / 60);
                seconds -= minutes * 60;
            }
            if (minutes > 60)
            {
                hours = Math.Floor(minutes / 60);
                minutes -= hours * 60;
            }
        }
        

        #region prev
        //public static void Tune20NewsGroupsRoseTreeParameters_Accuracy()
        //{
        //    ExperimentParameters.Description = "RoseTreeAccuracy_" + ExperimentParameters.Description;
        //    StreamWriter ofile = InitializeResultPrinter();
        //    ofile.WriteLine("DataSet:\t" + ExperimentParameters.TwentyNewsGroupPath);

        //    //double[] alphas = new double[] { 0.1 , 0.2, 0.3, 0.4, 0.5, 0.8, 1, 2, 3, 5 };
        //    //double[] gammas = new double[] { 0.005 , 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.1, 0.2, 0.3 };
        //    double[] alphas = new double[] { 0.4 };//0.01 , 0.1, 0.2, 0.4, 0.6, 0.8, 1, 3};// , 10 };
        //    double[] gammas = new double[] { 0.05 };//0.001 , 0.005, 0.01, 0.03, 0.05, 0.08, 0.1, 0.3, 0.6 };
        //    int[] samplenumbers = new int[] { 50 };
        //    int[] algorithms = new int[]{
        //         Constant.BRT};
        //    int[] models = new int[]{
        //         Constant.DCM};
        //    int[] sampletimes = new int[] { 1 };//,2,3,4,5
        //    int[] infosampletimes = new int[] { 1 };//6,7,8,9,10
        //    /// Rule ///
        //    Rules[] rules = new Rules[] { new Rules() };
        //    /// Constraint ///
        //    int constraintCnt = 30;
        //    ConstraintParameter[] constraints = new ConstraintParameter[constraintCnt];
        //    constraints[0] = new NoConstraintParameter();
        //    constraints[constraintCnt - 1] = new OrderConstraintParameter(double.MaxValue, double.MaxValue);
        //    double begin = Math.Log10(0.01 / samplenumbers[0] / samplenumbers[0]), end = 5 + begin;
        //    double delta = (end - begin) / (constraintCnt - 2);
        //    for (int i = 1; i < constraintCnt - 1; i++)
        //    {
        //        double punishweight = Math.Pow(10, begin + (i - 1) * delta);
        //        //double punishweight = begin + (i - 1) * delta;
        //        //constraints[i] = new DistanceConstraintParameter(punishweight);
        //        constraints[i] = new OrderConstraintParameter(punishweight, punishweight, 0.5);
        //    }
        //    //double p0 = 10.0 / samplenumbers[0] / samplenumbers[0];
        //    //ConstraintParameter[] constraints = new ConstraintParameter[]{
        //    //    new OrderConstraintParameter(p0, p0),
        //    //    new OrderConstraintParameter(5*p0, 5*p0),
        //    //  };


        //    StreamWriter ofileMatlab = InitializeMatlabDataFunctionPrinter(
        //        sampletimes, samplenumbers, models, algorithms, rules,
        //        gammas, alphas, constraints, infosampletimes);

        //    if (infosampletimes.Length != sampletimes.Length)
        //        throw new Exception("[ERROR]Constraint Sample Times Length Does Not Match!");

        //    //records of result
        //    int experimentNumber = alphas.Length * gammas.Length * samplenumbers.Length
        //        * algorithms.Length * models.Length * sampletimes.Length * rules.Length * constraints.Length;
        //    RoseTreeStructureInfo[][] structureInfo = new RoseTreeStructureInfo[experimentNumber][];
        //    double[][] accuracyRes = new double[experimentNumber][];
        //    double[] loglikelihoodRes = new double[experimentNumber];
        //    int experimentIndex = 0;

        //    RoseTree rosetree;
        //    for (int isampletimes = 0; isampletimes < sampletimes.Length; isampletimes++)
        //    {
        //        ExperimentParameters.SampleTimes = sampletimes[isampletimes];
        //        ofile.WriteLine("////////////////////////SampleTimes:" + sampletimes[isampletimes] + "////////////////////////");
        //        for (int isamplenum = 0; isamplenum < samplenumbers.Length; isamplenum++)
        //        {
        //            ExperimentParameters.SampleNumber = samplenumbers[isamplenum];
        //            ofile.WriteLine("////////////////////////SampleNum:" + samplenumbers[isamplenum] + "////////////////////////");
        //            // use the same ground truth tree for all below parameters
        //            ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
        //            ExperimentParameters.SampleTimes = infosampletimes[isampletimes];
        //            RoseTree consrosetree = ExperimentRoseTree.GetRoseTree();
        //            ExperimentParameters.SampleTimes = sampletimes[isampletimes];
        //            RoseTree gtrosetree = ExperimentRoseTree.GetRoseTree();
        //            ConstraintTree constrainttree = new ConstraintTree(consrosetree, gtrosetree.lfv);
        //            constrainttree.OutputDataProjectionResult(ExperimentParameters.OutputDebugPath + "DataProjection.dat");

        //            for (int imodel = 0; imodel < models.Length; imodel++)
        //            {
        //                ExperimentParameters.ModelIndex = models[imodel];
        //                ofile.WriteLine("////////////////////////" + (models[imodel] == Constant.DCM ? "DCM" : "vMF") + "////////////////////////");
        //                // use the same ldinfo for all below parameters
        //                LoadDataInfo ldinfo = ExperimentRoseTree.LoadDataInfo();
        //                for (int ialgorithm = 0; ialgorithm < algorithms.Length; ialgorithm++)
        //                {
        //                    ExperimentParameters.RoseTreeParameters.algorithm_index = algorithms[ialgorithm];
        //                    ofile.WriteLine("////////////////////////" + (algorithms[ialgorithm] == Constant.BRT ? "BRT" : "KNN_BRT") + "////////////////////////");
        //                    for (int irule = 0; irule < rules.Length; irule++)
        //                    {
        //                        ExperimentParameters.Rules = rules[irule];
        //                        ofile.WriteLine("---------------------Rule:" + irule + "---------------------");
        //                        for (int igamma = 0; igamma < gammas.Length; igamma++)
        //                        {
        //                            ExperimentParameters.RoseTreeParameters.gamma = gammas[igamma];
        //                            ofile.WriteLine("---------------------Gamma:" + gammas[igamma] + "---------------------");
        //                            for (int ialpha = 0; ialpha < alphas.Length; ialpha++)
        //                            {
        //                                ExperimentParameters.RoseTreeParameters.alpha = alphas[ialpha];
        //                                ofile.WriteLine("---------------------Alpha:" + alphas[ialpha] + "---------------------");
        //                                ofile.WriteLine(DateTime.Now);
        //                                for (int iconstraint = 0; iconstraint < constraints.Length; iconstraint++)
        //                                {
        //                                    constraints[iconstraint].Set(consrosetree);
        //                                    ofile.Write("<Constraint " + iconstraint + ">");
        //                                    try
        //                                    {
        //                                        ofile.Write("Rule: " + irule + "\t");
        //                                        PrintRoseTreeParameters(ofile);

        //                                        rosetree = ExperimentRoseTree.GetRoseTree(ldinfo);

        //                                        PrintRoseTreeStructure(rosetree, ofile);
        //                                        loglikelihoodRes[experimentIndex] = (rosetree as ConstrainedRoseTree).LogLikelihood;
        //                                        structureInfo[experimentIndex] = rosetree.StructureInfo();
        //                                        accuracyRes[experimentIndex] = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);
        //                                        ofileMatlab.Write(ExperimentResultToString(structureInfo[experimentIndex], accuracyRes[experimentIndex], loglikelihoodRes[experimentIndex]) + "\t");
        //                                        experimentIndex++;
        //                                    }
        //                                    catch (Exception e)
        //                                    {
        //                                        ofile.WriteLine("Failed:" + e.Message);
        //                                        ofileMatlab.Write(ExperimentResultToString(structureInfo[experimentIndex], accuracyRes[experimentIndex], loglikelihoodRes[experimentIndex]) + "\t");
        //                                        experimentIndex++;
        //                                    }
        //                                    ofileMatlab.Flush();
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    EndMatlabDataFunction(ofileMatlab);
        //    ofile.WriteLine("\n=============================Final Result=============================");
        //    ofile.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7}", sampletimes.Length, samplenumbers.Length,
        //        models.Length, algorithms.Length, rules.Length,
        //        gammas.Length, alphas.Length, constraints.Length);
        //    ofile.Write(ExperimentParametersToString("Sample Time", sampletimes.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("[Info Sample Time]", infosampletimes.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Sample Number", samplenumbers.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Model", models.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Algorithm", algorithms.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Rule", rules.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Gamma", gammas.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Alpha", alphas.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Constraint", constraints.Cast<object>().ToArray<object>()));
        //    ofile.WriteLine("----------------------------------------------------------------------");
        //    for (int i = 0; i < experimentNumber; i++)
        //        ofile.Write(ExperimentResultToString(structureInfo[i], accuracyRes[i], loglikelihoodRes[i]) + "\t");
        //    ofile.WriteLine("\n======================================================================");

        //    ofile.Close();
        //}

        //public static void Tune20NewsGroupsRoseTreeParameters_Accuracy_NoConstraint()
        //{
        //    ExperimentParameters.Description = "RoseTreeAccuracy_" + ExperimentParameters.Description;
        //    StreamWriter ofile = InitializeResultPrinter();
        //    ofile.WriteLine("DataSet:" + ExperimentParameters.TwentyNewsGroupPath);

        //    double[] alphas = new double[] { 0.1 };//, 0.2, 0.3, 0.4, 0.5, 0.8, 1, 2, 3, 5 };
        //    double[] gammas = new double[] { 0.005 };//, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.1, 0.2, 0.3 };
        //    int[] samplenumbers = new int[] { 1000 };
        //    int[] algorithms = new int[]{
        //         Constant.KNN_BRT};
        //    int[] models = new int[]{
        //         Constant.DCM};
        //    int[] sampletimes = new int[] { 1 };
        //    /// Rule ///
        //    Rules rules0 = new Rules();
        //    rules0.AddMaxRule(50, 30, 3);
        //    rules0.AddMaxRule(20, 100, 4);
        //    rules0.AddMaxRule(10, 80, 5);
        //    rules0.AddMinRule(300, 1, 0);
        //    rules0.AddMinRule(100, 5, 1);
        //    rules0.AddMinRule(30, 20, 2);
        //    Rules[] rules = new Rules[] { new Rules(), rules0 };

        //    //records of result
        //    int experimentNumber = alphas.Length * gammas.Length * samplenumbers.Length
        //        * algorithms.Length * models.Length * sampletimes.Length * rules.Length;
        //    RoseTreeStructureInfo[][] structureInfo = new RoseTreeStructureInfo[experimentNumber][];
        //    double[][] accuracyRes = new double[experimentNumber][];
        //    double[] loglikelihoodRes = new double[experimentNumber];
        //    int experimentIndex = 0;

        //    RoseTree rosetree;
        //    for (int isampletimes = 0; isampletimes < sampletimes.Length; isampletimes++)
        //    {
        //        ExperimentParameters.SampleTimes = sampletimes[isampletimes];
        //        ofile.WriteLine("////////////////////////SampleTimes:" + sampletimes[isampletimes] + "////////////////////////");
        //        for (int isamplenum = 0; isamplenum < samplenumbers.Length; isamplenum++)
        //        {
        //            ExperimentParameters.SampleNumber = samplenumbers[isamplenum];
        //            ofile.WriteLine("////////////////////////SampleNum:" + samplenumbers[isamplenum] + "////////////////////////");
        //            // use the same ground truth tree for all below parameters
        //            ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
        //            RoseTree gtrosetree = ExperimentRoseTree.GetRoseTree();
        //            for (int imodel = 0; imodel < models.Length; imodel++)
        //            {
        //                ExperimentParameters.ModelIndex = models[imodel];
        //                ofile.WriteLine("////////////////////////" + (models[imodel] == Constant.DCM ? "DCM" : "vMF") + "////////////////////////");
        //                // use the same ldinfo for all below parameters
        //                ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;
        //                LoadDataInfo ldinfo = ExperimentRoseTree.LoadDataInfo();
        //                for (int ialgorithm = 0; ialgorithm < algorithms.Length; ialgorithm++)
        //                {
        //                    ExperimentParameters.RoseTreeParameters.algorithm_index = algorithms[ialgorithm];
        //                    ofile.WriteLine("////////////////////////" + (algorithms[ialgorithm] == Constant.BRT ? "BRT" : "KNN_BRT") + "////////////////////////");
        //                    for (int irule = 0; irule < rules.Length; irule++)
        //                    {
        //                        ExperimentParameters.Rules = rules[irule];
        //                        ofile.WriteLine("---------------------Rule:" + irule + "---------------------");
        //                        for (int igamma = 0; igamma < gammas.Length; igamma++)
        //                        {
        //                            ExperimentParameters.RoseTreeParameters.gamma = gammas[igamma];
        //                            ofile.WriteLine("---------------------Gamma:" + gammas[igamma] + "---------------------");
        //                            for (int ialpha = 0; ialpha < alphas.Length; ialpha++)
        //                            {
        //                                ExperimentParameters.RoseTreeParameters.alpha = alphas[ialpha];
        //                                ofile.WriteLine("---------------------Alpha:" + alphas[ialpha] + "---------------------");
        //                                try
        //                                {
        //                                    ofile.Write("Rule: " + irule + "\t");
        //                                    PrintRoseTreeParameters(ofile);
        //                                    if (ldinfo == null)
        //                                    {
        //                                        rosetree = ExperimentRoseTree.GetRoseTree();
        //                                        ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo;
        //                                    }
        //                                    else
        //                                        rosetree = ExperimentRoseTree.GetRoseTree(ldinfo);
        //                                    PrintRoseTreeStructure(rosetree, ofile);
        //                                    loglikelihoodRes[experimentIndex] = (rosetree as ConstrainedRoseTree).LogLikelihood;
        //                                    accuracyRes[experimentIndex] = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);
        //                                    structureInfo[experimentIndex] = rosetree.StructureInfo();
        //                                    ofile.WriteLine(ExperimentResultToString(structureInfo[experimentIndex], accuracyRes[experimentIndex], loglikelihoodRes[experimentIndex]));
        //                                    experimentIndex++;
        //                                }
        //                                catch (Exception e)
        //                                {
        //                                    ofile.WriteLine(e.Message);
        //                                    ofile.WriteLine(ExperimentResultToString(null, null, loglikelihoodRes[experimentIndex]));
        //                                    experimentIndex++;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    ofile.WriteLine("\n=============================Final Result=============================");
        //    ofile.WriteLine("{0} {1} {2} {3} {4} {5}", sampletimes.Length, samplenumbers.Length,
        //        algorithms.Length, rules.Length, gammas.Length, alphas.Length);
        //    ofile.Write(ExperimentParametersToString("Sample Time", sampletimes.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Sample Number", samplenumbers.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Model", models.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Algorithm", algorithms.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Rule", rules.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Gamma", gammas.Cast<object>().ToArray<object>()));
        //    ofile.Write(ExperimentParametersToString("Alpha", alphas.Cast<object>().ToArray<object>()));
        //    ofile.WriteLine("----------------------------------------------------------------------");
        //    for (int i = 0; i < experimentNumber; i++)
        //        ofile.Write(ExperimentResultToString(structureInfo[i], accuracyRes[i], loglikelihoodRes[experimentIndex]) + "\t");
        //    ofile.WriteLine("\n======================================================================");

        //    ofile.Close();
        //}

        //public static void Tune20NewsGroupsRoseTreeParameters()
        //{
        //    ExperimentParameters.Description = "RoseTree_" + ExperimentParameters.Description;
        //    StreamWriter ofile = InitializeResultPrinter();

        //    double[] alphas = new double[] { 0.01, 0.1, 1, 2, 3, 5, 10, 100 };
        //    double[] gammas = new double[] { 0.001, 0.01, 0.1, 0.2, 0.3, 0.5, 0.8 };
        //    int[] samplenumbers = new int[] { 500, 1000 };
        //    int[] algorithms = new int[]{
        //         Constant.BRT};
        //    int[] sampletimes = new int[] { 1, 2, 3 };

        //    RoseTree rosetree;
        //    for (int isampletimes = 0; isampletimes < sampletimes.Length; isampletimes++)
        //    {
        //        ExperimentParameters.SampleTimes = sampletimes[isampletimes];
        //        ofile.WriteLine("////////////////////////SampleTimes:" + sampletimes[isampletimes] + "////////////////////////");
        //        for (int isamplenum = 0; isamplenum < samplenumbers.Length; isamplenum++)
        //        {
        //            ExperimentParameters.SampleNumber = samplenumbers[isamplenum];
        //            // use the same ldinfo for all below parameters
        //            LoadDataInfo ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo = null;
        //            ofile.WriteLine("////////////////////////SampleNum:" + samplenumbers[isamplenum] + "////////////////////////");
        //            for (int ialgorithm = 0; ialgorithm < algorithms.Length; ialgorithm++)
        //            {
        //                ExperimentParameters.RoseTreeParameters.algorithm_index = algorithms[ialgorithm];
        //                ofile.WriteLine("////////////////////////" + (algorithms[ialgorithm] == Constant.BRT ? "BRT" : "KNN_BRT") + "////////////////////////");
        //                for (int igamma = 0; igamma < gammas.Length; igamma++)
        //                {
        //                    ExperimentParameters.RoseTreeParameters.gamma = gammas[igamma];
        //                    ofile.WriteLine("---------------------Gamma:" + gammas[igamma] + "---------------------");
        //                    for (int ialpha = 0; ialpha < alphas.Length; ialpha++)
        //                    {
        //                        ExperimentParameters.RoseTreeParameters.alpha = alphas[ialpha];
        //                        ofile.WriteLine("---------------------Alpha:" + alphas[ialpha] + "---------------------");
        //                        {
        //                            PrintRoseTreeParameters(ofile);
        //                            if (ldinfo == null)
        //                            {
        //                                rosetree = ExperimentRoseTree.GetRoseTree();
        //                                ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo;
        //                            }
        //                            else
        //                                rosetree = ExperimentRoseTree.GetRoseTree(ldinfo);
        //                            PrintRoseTreeStructure(rosetree, ofile);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    ofile.Close();
        //}
        #endregion prev

        public static string ExperimentParametersToString(string name, object[] parameter)
        {
            string str = "";
            str += name + "\n";
            for (int i = 0; i < parameter.Length; i++)
                str += parameter[i] + "\t";
            str += "\n";
            return str;
        }

        private static string ExperimentResultToString(
            StreamWriter ofile,
            RoseTree rosetree, double loglikelihood, double[] smoothnessCost, double runningTime,
            RoseTreeStructureInfo[] info, double[] accuracy)
        {
            string str = "";
            int stage = 0;
            try
            {

                str += loglikelihood + "\t";

                if (smoothnessCost != null)
                {
                    for (int i = 0; i < smoothnessCost.Length; i++)
                        str += smoothnessCost[i] + "\t";
                    if (smoothnessCost.Length != 3)
                    {
                        for (int i = smoothnessCost.Length; i < 3; i++)
                            str += "-1\t";
                    }
                    //throw new Exception("Error! Smoothness Cost should be length of 3!");
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                        str += "-1\t";
                }

                stage++;

                str += runningTime + "\t";

                if (rosetree != null)
                    str += rosetree.JoinCnt + "\t" + rosetree.AbsorbLCnt + "\t"
                        + rosetree.AbsorbRCnt + "\t" + rosetree.CollapseCnt + "\t"
                        + rosetree.root.tree_depth + "\t";
                else
                    str += "-1\t-1\t-1\t-1\t-1\t";

                stage++;

                if (info != null)
                {
                    for (int i = 0; i < info.Length; i++)
                    {
                        RoseTreeStructureInfo levelinfo = info[i];
                        str += levelinfo.ChildrenCount + "\t";
                        str += levelinfo.ShrinkChildrenCount + "\t";
                        str += levelinfo.AverageChildrenLeaves + "\t";
                        str += levelinfo.StdChildrenLeaves + "\t";
                    }
                }
                else
                {
                    for (int i = 0; i < 8; i++)
                        str += "-1\t";
                }

                stage++;

                if (accuracy != null)
                {
                    for (int i = 0; i < accuracy.Length - 1; i++)
                        str += accuracy[i] + "\t";
                    str += accuracy[accuracy.Length - 1] + "\t";
                }
                else
                {
                    for (int i = 0; i < 11; i++)
                        str += "-1\t";
                    str += "-1\t";
                }

                stage++;

                str += GetResetSpillTreePrecision() + "\t";

                stage++;

#if !KDD_EXP_STRING
                if (rosetree != null)
                    str += rosetree.GetAllValidInternalTreeNodes().Count + "\t";
                else
                    str += "-1\t";

                stage++;

                if (rosetree != null && (rosetree as ConstrainedRoseTree).GetConstraint() is TreeOrderConstraint)
                    str += ((rosetree as ConstrainedRoseTree).GetConstraint() as TreeOrderConstraint).GetConstraintTree().NotFreeConstraintTreeLeafCount + "\t";
                else if (rosetree != null && (rosetree as ConstrainedRoseTree).GetConstraint() is TreeDistanceConstraint)
                    str += ((rosetree as ConstrainedRoseTree).GetConstraint() as TreeDistanceConstraint).NotFreeConstraintTreeLeafCount + "\t";
                else
                    str += "-1\t";

                stage++;

                if (rosetree != null)
                {
                    double avgleafdis, avgleafsquaredis;
                    (rosetree as ConstrainedRoseTree).GetAverageTreeLeafDistances(out avgleafdis, out avgleafsquaredis);
                    str += avgleafdis + "\t";
                    str += avgleafsquaredis + "\t";
                }
                else
                    str += "-1\t-1\t";

                stage++;

                if (rosetree != null)
                {
                    Constraint constraint = (rosetree as ConstrainedRoseTree).GetConstraint();
                    int leftsms = 3*(7-1);
                    int constrainttreecnt = -1;
                    
                    if (constraint.ConstraintType == ConstraintType.Multiple)
                        constrainttreecnt = (constraint as MultipleConstraints).constraintCnt;
                    else if (constraint.ConstraintType == ConstraintType.NoConstraint)
                    {
                        if (((rosetree as ConstrainedRoseTree).SmoothCostConstraint) != null)
                        {
                            foreach (Constraint addconstraint in (rosetree as ConstrainedRoseTree).SmoothCostConstraint)
                            {
                                if (addconstraint is MultipleConstraints)
                                {
                                    constrainttreecnt = (addconstraint as MultipleConstraints).constraintCnt;
                                    break;
                                }
                            }
                        }
                    }

                    //if(ExperimentParameters.ConstraintRoseTrees!=null)
                    //    foreach (RoseTree rosetreeiter in ExperimentParameters.ConstraintRoseTrees)
                    //    {
                    //        Console.WriteLine((rosetreeiter as ConstrainedRoseTree).RoseTreeId);
                    //    }

                    if (constrainttreecnt > 0)
                    {
                        for (int last = 2; last <= constrainttreecnt; last++)
                        {
                            double[] smoothness = (rosetree as ConstrainedRoseTree).GetNormalizedSmoothnessCost(last);
                            str += smoothness[0] + "\t" + smoothness[1] + "\t";

                            RoseTree constraintRoseTree = ExperimentParameters.ConstraintRoseTrees[constrainttreecnt - last];
                            if (!BuildRoseTree.BRestrictBinary)
                            {
                                double smoothnessCostUnbiased = 1 - RobinsonFouldsDistance.CalculateDistance(constraintRoseTree, rosetree);
                                str += smoothnessCostUnbiased + "\t";
                            }
                            else
                                str += "1\t";
                            leftsms -= 3;
                            //Console.WriteLine(string.Format("RoseTreeId:{0}", (constraintRoseTree as ConstrainedRoseTree).RoseTreeId));
                        }

                    }
                    for (int i = 0; i < leftsms / 3; i++)
                        str += "0\t0\t1\t";
                }

                if (rosetree != null)
                {
                    double[] depthInfos = ExperimentRoseTree.GetTreeDepthInfo(rosetree);
                    foreach (double depthInfo in depthInfos)
                        str += depthInfo + "\t";
                }
                else
                    str += "-1\t-1\t-1\t-1\t";
#endif
            }
            catch (Exception e)
            {
                Console.WriteLine("rosetree is ConstrainedRoseTree? {0}", rosetree is ConstrainedRoseTree);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                ofile.WriteLine("ExperimentResultToString Error! Stage {0}\n", stage);
                ofile.Flush();
                str += "-1\t";
            }

            str += ";";

            return str;
        }

        private static string GetResetSpillTreePrecision()
        {
            string str = "";
            //SpillTree precision
            if (RoseTree.testtime > 0)
                str += (RoseTree.testprecision / RoseTree.testtime) + "\t";
            else
                str += "0\t";
            //SpillTree precision 2
            if (RoseTree.testtime > 0)
                str += (RoseTree.testprecision2 / RoseTree.testtime) + "\t";
            else
                str += "0\t";
            //SpillTree projection precision
            if (RoseTree.testprojecttime > 0)
                str += (RoseTree.testprojectprecision / RoseTree.testprojecttime) + "\t";
            else
                str += "0\t";
            //SpillTree fail times ratio
            if (RoseTree.totalsearchtime > 0)
                str += ((double)RoseTree.failtime / RoseTree.totalsearchtime) + "\t";
            else
                str += "0\t";
            //SpillTree overlapping numbers ratio
            if (SpillTree.OverallNodesNumber > 0)
                str += ((double)SpillTree.OverlappingNodesNumber / SpillTree.OverallNodesNumber);
            else
                str += "0";

            //Reset
            RoseTree.testprecision = 0;
            RoseTree.testprecision2 = 0;
            RoseTree.testtime = 0;
            RoseTree.testprojectprecision = 0;
            RoseTree.testprojecttime = 0;
            RoseTree.failtime = 0;
            RoseTree.totalsearchtime = 0;
            SpillTree.OverlappingNodesNumber = 0;
            SpillTree.OverallNodesNumber = 0;

            return str;
        }

        private static string ExperimentResultArrayMeaning = "LogLH, SmDis, SmOrder, SmRF, RunTime; " +
            "JnCnt, ALCnt, ARCnt, CpCnt, TreeDepth; " +
            "L1CC, L1NC, L1AvgLf, L1StdLf; " +
            "L2CC, L2NC, L2AvgLf, L2StdLf; " +
            "L1NMI, L2NMI, L1Kmean, L2Kmean, L1Purity, L2Purity; " +
            "L1sNMI, L2sNMI, L1sKmean, L2sKmean, L1sPurity, L2sPurity; " +
            "SpPrec, SpPrec2, RpPrec, SpFail, OlNodes; " +
#if KDD_EXP_STRING
            "";
#else
            "InternalNodes, CTNodes, LeafDis, LeafSDis;" +
            "SmDis2, SmOrder2, SmRF2, SmDis3, SmOrder3, SmRF3, " +
            "SmDis4, SmOrder4, SmRF4, SmDis5, SmOrder5, SmRF5, " +
            "SmDis6, SmOrder6, SmRF6, SmDis7, SmOrder7, SmRF7;" +
            "minDepth, maxDepth, avgDepth, stdDepth;";
#endif
        private static void PrintRoseTreeParameters(StreamWriter ofile, int experimentIndex)
        {
            ofile.Write("Exp:" + experimentIndex + "\t");
            ofile.Write("Alpha: " + ExperimentParameters.RoseTreeParameters.alpha + "\t");
            ofile.Write("Gamma: " + ExperimentParameters.RoseTreeParameters.gamma + "\t");
            ofile.Write("Al:" + ExperimentParameters.RoseTreeParameters.algorithm_index + "\t");
            ofile.Write("SN:" + ExperimentParameters.SampleNumber + "\t");
            ofile.Write("ST:" + ExperimentParameters.SampleTimes + "\t");
            ofile.WriteLine();

            ofile.Flush();
        }

        public static void PrintRoseTreeParametersEvolutionary(StreamWriter ofile)
        {
            ofile.Write("Alpha: " + ExperimentParameters.RoseTreeParameters.alpha + "\t");
            ofile.Write("Gamma: " + ExperimentParameters.RoseTreeParameters.gamma + "\t");
            ofile.Write("Al:" + ExperimentParameters.RoseTreeParameters.algorithm_index + "\t");
            ofile.Write("SN:" + ExperimentParameters.SampleNumber + "\t");
            ofile.Write("ST:" + ExperimentParameters.SampleTimes + "\t");
            ofile.WriteLine();
            ofile.WriteLine("---" + ExperimentParameters.LoadDataQueryString + "\t");

            ofile.Flush();
        }

        public static void PrintRoseTreeStructure(RoseTree rosetree, StreamWriter ofile)
        {
            ofile.Write(rosetree.StructureToString());

            ofile.Flush();
        }

        public static StreamWriter InitializeResultPrinter()
        {
            string filename = ExperimentParameters.Description + ".dat";
            filename = ExperimentParameters.TuneParameterResultPath + filename;

            if (!Directory.Exists(ExperimentParameters.TuneParameterResultPath))
                Directory.CreateDirectory(ExperimentParameters.TuneParameterResultPath);

            StreamWriter ofile = new StreamWriter(filename);
#if NEW_CONSTRAINT_MODEL
            ofile.Write("~~~~~~~~~~~~~~NCM");
#if NEW_MODEL_2
            ofile.Write("2");
#elif NEW_MODEL_3
            ofile.Write("3");
#endif
            ofile.WriteLine("~~~~~~~~~~~~~~");
#else
            ofile.WriteLine("OCM");
#endif

            ofile.WriteLine("exe = " + AppDomain.CurrentDomain.FriendlyName);
            ofile.WriteLine("LooseOrderDeltaRatio = {0}", LooseTreeOrderConstraint.LooseOrderDeltaRatio);

            return ofile;
        }

        private static StreamWriter InitializeViolationCurve()
        {
            string filename = "GetData_" + ExperimentParameters.Description + "_VioCurve.dat";
            filename = ExperimentParameters.OutputMatlabFunctionPath + filename;

            StreamWriter ofile = new StreamWriter(filename);
            return ofile;
        }

        private static StreamWriter InitializeMatlabDataFunctionPrinter(
            int[] sampletimes, int[] samplenumbers, int[] models, double[] overlapratio, AlgorithmParameter[] algorithms,
            Rules[] rules, double[] gammas, double[] alphas, ConstraintParameter[] constraints,
            int[] infosampletimes, string datasetname = null)
        {
            string filename = "GetData_" + ExperimentParameters.Description + ".m";
            filename = ExperimentParameters.OutputMatlabFunctionPath + filename;

            if (!Directory.Exists(ExperimentParameters.OutputMatlabFunctionPath))
                Directory.CreateDirectory(ExperimentParameters.OutputMatlabFunctionPath);

            StreamWriter ofileMatlab = new StreamWriter(filename);

            //Initialize Function Header
            ofileMatlab.WriteLine("function [datadim, dimdesp, dimpara, infodimdesp, infodimpara data] = GetData()");
            //corresponding detail data
            ofileMatlab.WriteLine("Description = '{0}';", ExperimentParameters.Description);
            ofileMatlab.WriteLine("% exe = " + AppDomain.CurrentDomain.FriendlyName);
            ofileMatlab.WriteLine("disp(['[' Description ']']);");
            //dataset
            if (datasetname != null)
                ofileMatlab.WriteLine("dataset = '" + datasetname + " ';");
            else
                ofileMatlab.WriteLine("dataset = '" + ExperimentParameters.TwentyNewsGroupPath + " ';");
            ofileMatlab.WriteLine("disp(['-DATASET-\t' dataset]);");
            //datadim
            ofileMatlab.WriteLine("datadim = [{0} {1} {2} {3} {4} {5} {6} {7} {8}];",
                sampletimes.Length, samplenumbers.Length, models.Length, 
                overlapratio.Length, algorithms.Length, rules.Length,
                gammas.Length, alphas.Length, constraints.Length);
            //dimdesp
            ofileMatlab.WriteLine("dimdesp = {");
            ofileMatlab.WriteLine("\t'Sample Time',...");
            ofileMatlab.WriteLine("\t'Sample Number',...");
            ofileMatlab.WriteLine("\t'Model',...");
            ofileMatlab.WriteLine("\t'Sample Overlap Ratio',...");
            ofileMatlab.WriteLine("\t'Algorithm',...");
            ofileMatlab.WriteLine("\t'Rule',...");
            ofileMatlab.WriteLine("\t'Gamma',...");
            ofileMatlab.WriteLine("\t'Alpha',...");
            ofileMatlab.WriteLine("\t'Constraint'...");
            ofileMatlab.WriteLine("};");
            //dimpara
            ofileMatlab.WriteLine("dimpara = {");
            ofileMatlab.Write(GetMatlabParameterString(sampletimes.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(samplenumbers.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(models.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(overlapratio.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(algorithms.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(rules.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(gammas.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(alphas.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(constraints.Cast<object>().ToArray<object>()));
            ofileMatlab.WriteLine("};");
            //infodimdesp
            ofileMatlab.WriteLine("infodimdesp = {");
            ofileMatlab.WriteLine("\t'[Info Sample Time]'...");
            ofileMatlab.WriteLine("};");
            //infodimpara
            ofileMatlab.WriteLine("infodimpara = {");
            ofileMatlab.Write(GetMatlabParameterString(infosampletimes.Cast<object>().ToArray<object>()));
            ofileMatlab.WriteLine("};");

            //data
            ofileMatlab.WriteLine();
            ofileMatlab.WriteLine("data = [");

            return ofileMatlab;
        }

        public static StreamWriter InitializeMatlabDataFunctionPrinterEvolutionary(
            int[] sampletimes, int[] samplenumbers, double[] overlapratio, int[] models, AlgorithmParameter[] algorithms,
            Rules[] rules, EvolvingDouble[] gammas, EvolvingDouble[] alphas, ConstraintParameter[] constraints,
            string[] nytQueryStr, string datasetname = null)
        {
            string filename = "GetData_" + ExperimentParameters.Description + ".m";
            filename = ExperimentParameters.OutputMatlabFunctionPath + filename;

            if (!Directory.Exists(ExperimentParameters.OutputMatlabFunctionPath))
                Directory.CreateDirectory(ExperimentParameters.OutputMatlabFunctionPath);

            StreamWriter ofileMatlab = new StreamWriter(filename);

            //Initialize Function Header
            ofileMatlab.WriteLine("function [datadim, dimdesp, dimpara, infodimdesp, infodimpara data] = GetData()");
            //corresponding detail data
            ofileMatlab.WriteLine("%" + ExperimentParameters.Description);
            ofileMatlab.WriteLine("% exe = " + AppDomain.CurrentDomain.FriendlyName);
            //dataset
            if (datasetname != null)
                ofileMatlab.WriteLine("dataset = '" + datasetname + " ';");
            else
                ofileMatlab.WriteLine("dataset = '" + ExperimentParameters.NewYorkTimesPath + " ';");
            ofileMatlab.WriteLine("disp(['-DATASET-\t' dataset]);");
            //datadim
            ofileMatlab.WriteLine("datadim = [{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}];",
                sampletimes.Length, samplenumbers.Length, overlapratio.Length,
                models.Length, algorithms.Length, rules.Length,
                gammas.Length, alphas.Length, constraints.Length, nytQueryStr.Length);
            //dimdesp
            ofileMatlab.WriteLine("dimdesp = {");
            ofileMatlab.WriteLine("\t'Sample Time',...");
            ofileMatlab.WriteLine("\t'Sample Number',...");
            ofileMatlab.WriteLine("\t'Sample Overlap Ratio',...");
            ofileMatlab.WriteLine("\t'Model',...");
            ofileMatlab.WriteLine("\t'Algorithm',...");
            ofileMatlab.WriteLine("\t'Rule',...");
            ofileMatlab.WriteLine("\t'Gamma',...");
            ofileMatlab.WriteLine("\t'Alpha',...");
            ofileMatlab.WriteLine("\t'Constraint'...");
            ofileMatlab.WriteLine("\t'QueryString'...");
            ofileMatlab.WriteLine("};");
            //dimpara
            ofileMatlab.WriteLine("dimpara = {");
            ofileMatlab.Write(GetMatlabParameterString(sampletimes.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(samplenumbers.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(overlapratio.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(models.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(algorithms.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(rules.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(gammas.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(alphas.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(constraints.Cast<object>().ToArray<object>()));
            ofileMatlab.Write(GetMatlabParameterString(nytQueryStr.Cast<object>().ToArray<object>()));
            ofileMatlab.WriteLine("};");
            //infodimdesp
            ofileMatlab.WriteLine("infodimdesp = {");
            //ofileMatlab.WriteLine("\t'[Info Sample Time]'...");
            ofileMatlab.WriteLine("};");
            //infodimpara
            ofileMatlab.WriteLine("infodimpara = {");
            //ofileMatlab.Write(GetMatlabParameterString(infosampletimes.Cast<object>().ToArray<object>()));
            ofileMatlab.WriteLine("};");

            //data
            ofileMatlab.WriteLine();
            ofileMatlab.WriteLine("data = [");

            return ofileMatlab;
        }

        private static void EndMatlabDataFunction(StreamWriter ofileMatlab)
        {
            ofileMatlab.WriteLine();
            ofileMatlab.WriteLine("];");

            //result meaning
            ofileMatlab.WriteLine("%Result Meaning:{0}", ExperimentResultArrayMeaning);

            ofileMatlab.WriteLine();
            ofileMatlab.WriteLine("end");

            ofileMatlab.Close();
        }

        private static string GetMatlabParameterString(object[] parameter)
        {
            string str = "\t{";
            if (parameter[0] is int || parameter[0] is double)
            {
                for (int i = 0; i < parameter.Length; i++)
                    str += parameter[i] + "\t";
            }
            else
            {
                for (int i = 0; i < parameter.Length; i++)
                    str += "'" + parameter[i] + "'\t";
            }

            str = str.TrimEnd('\t');
            str += "},...\n";
            return str;
        }

        public static void InitializeDrawRoseTreePath()
        {
            ExperimentParameters.DrawRoseTreePath = ExperimentParameters.TuneParameterResultPath
                + ExperimentParameters.Description + "\\";
        }

    }
}
