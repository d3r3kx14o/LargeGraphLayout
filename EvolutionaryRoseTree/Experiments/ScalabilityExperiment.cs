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
namespace EvolutionaryRoseTree.Experiments
{
    class ScalabilityExperiment
    {
        public static void Entry()
        {
            Scalability_NewYorkTimes();
        }

        public static void Scalability_NewYorkTimes()
        {
            ///  APPROXIMATE_LIKELIHOOD, SCALABILITY_TEST, NYT_LEADING_PARAGRAPH ///

            ExperimentParameters.Description = "Exp5_KNN_Binary_10000_100_Distance1EN50_CosAllAN_ST5" + ExperimentParameters.Description;
            ExperimentParameters.DatasetIndex = Constant.NEW_YORK_TIMES;
            Constraint.DataProjectionType = DataProjectionType.MaxSimilarityNode;
            LooseTreeOrderConstraint.LooseOrderDeltaRatio = -0.2;
            //Binary
            BuildRoseTree.BRestrictBinary = true;

            EvolvingDouble[] gammas = new EvolvingDouble[]{
                new EvolvingDouble(0.03)};
            EvolvingDouble[] alphas = new EvolvingDouble[]{
                new EvolvingDouble(-1)};//-1:automatic
                //new EvolvingDouble(0.00001)};

            //int[] samplenumbers = new int[] { 100, 200, 400, 800, 1600, 3200, 6400, 12800, 25600, 51200, 102400 };
            //int[] samplenumbers = new int[] {  100000, 50000, 20000, 10000, 5000, 2000, 1000, 500, 200, 100 };
            //int[] samplenumbers = new int[] { 50000, 20000, 10000, 5000, 2000, 1000, 500, 200, 100 };
            int[] samplenumbers = new int[] { 10000, 5000, 2000, 1000, 500, 200, 100 };
            //int[] samplenumbers = new int[] { 5000 };

            /// Constraint ///
            ConstraintParameter[] constraints = new ConstraintParameter[]{
                //new LooseOrderConstraintParameter(1e0, 1e0, false)
                //new LooseOrderConstraintParameter(1e-5, 1e-5, false)
                //new NoConstraintParameter(false)
                //new DistanceConstraintParameter(1e-10 ,false)
                new DistanceConstraintParameter(1e-50 ,false)
              };

            /// Evolutionary Parameters ///
            AlgorithmParameter[] algorithms = new AlgorithmParameter[]{
                 new KNNAlgorithmParameter(50)
                 //new SpillTreeAlgorithmParameter(10, 50)
                 //new BRTAlgorithmParameter()
            };

            int[] sampletimes = new int[] { 5 }; //, 2, 3, 4, 5 , 6, 7, 8, 9, 10

            #region not always used parameters
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\data\NYTimes\NYTIndex";
            string defaultfield = "Cleaned Taxonomic Classifier"; //"Publication Year";
            string rawqueryStr = "(Top/Features/Arts OR Top/Features/Style/ OR Top/Features/Travel/ OR " +
            "Top/News/Business/ OR Top/News/Sports)";
            string[] nytQueryString = new string[] { 
                (rawqueryStr + "AND (" + GetContinuousYearString(1987, 1992, "Publication\\ Year:") + ")"), 
                (rawqueryStr + "AND (" + GetContinuousYearString(1993, 2007, "Publication\\ Year:") + ")"),  
            };

            int[] models = new int[]{
                 Constant.DCM};
            double[] overlapratio = new double[] { -1 };

            /// Rule ///
            Rules[] rules = new Rules[] { new Rules() };

            #endregion not always used parameters

            #region output files
            StreamWriter ofile = InitializeResultPrinter();
            ofile.WriteLine("================================================================");
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Sample Time", sampletimes.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Sample Number", samplenumbers.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Sample Overlap Ratio", overlapratio.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Model", models.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Algorithm", algorithms.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Rule", rules.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Gamma", gammas.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Alpha", alphas.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("Constraint", constraints.Cast<object>().ToArray<object>()));
            ofile.Write(TuneParameterExperiments.ExperimentParametersToString("QueryString", nytQueryString.Cast<object>().ToArray<object>()));
            ofile.WriteLine("{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                sampletimes.Length, samplenumbers.Length, overlapratio.Length,
                models.Length, algorithms.Length, rules.Length,
                gammas.Length, alphas.Length, constraints.Length);
            ofile.WriteLine("================================================================");
            ofile.Flush();

            InitializeDrawRoseTreePath();
            StreamWriter ofileMatlab = TuneParameterExperiments.InitializeMatlabDataFunctionPrinterEvolutionary(
                sampletimes, samplenumbers, overlapratio, models, algorithms, rules,
                gammas, alphas, constraints, nytQueryString, indexpath);
            #endregion output files

            int experimentNumber = alphas.Length * gammas.Length * samplenumbers.Length * overlapratio.Length
       * algorithms.Length * models.Length * sampletimes.Length * rules.Length * constraints.Length * nytQueryString.Length;
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
                                            if (alpha.GetValue(0) < 0)
                                                alpha = new EvolvingDouble(Math.Pow(0.1, (int)Math.Ceiling(Math.Log(ExperimentParameters.SampleNumber))));

                                            ofile.WriteLine("---------------------Alpha:" + alphas[ialpha] + "---------------------");
                                            ofile.WriteLine(DateTime.Now);

                                            for (int iconstraint = 0; iconstraint < constraints.Length; iconstraint++)
                                            {
                                                ofile.WriteLine("<Constraint " + iconstraint + ">");
                                                ConstraintParameter constraint = constraints[iconstraint];
                                                /// Evolutionary ///
                                                ExperimentParameters.NewYorkTimesPath = indexpath;
                                                ExperimentParameters.LoadDataQueryDefaultField = defaultfield;
                                                RoseTree previousRoseTree = null;
                                                ExperimentParameters.ConstraintRoseTree = null;
                                                ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;

                                                for (int itime = 0; itime < nytQueryString.Length; itime++)
                                                {
                                                    ExperimentParameters.RoseTreeParameters.gamma = gamma.GetValue(itime);
                                                    ExperimentParameters.RoseTreeParameters.alpha = alpha.GetValue(itime);
                                                    ExperimentParameters.LoadDataQueryString = nytQueryString[itime];

                                                    #region intialization
                                                    double loglikelihoodRes = -1;
                                                    RoseTreeStructureInfo[] structureInfo = null;
                                                    RoseTree rosetree = null;
                                                    double runningTime = -1;
                                                    #endregion intialization

                                                    try
                                                    {
                                                        runningTime = -1;

                                                        ofile.Write("Rule: " + irule + "\t");
                                                        TuneParameterExperiments.PrintRoseTreeParametersEvolutionary(ofile);

                                                        //DateTime t0 = DateTime.Now;
                                                        rosetree = ExperimentRoseTree.GetRoseTree();
                                                        //DateTime t1 = DateTime.Now;
                                                        //runningTime = (t1.Ticks - t0.Ticks) / 1e7;
                                                        runningTime = ExperimentRoseTree.BuildRoseTreeRunningTime;

                                                        TuneParameterExperiments.PrintRoseTreeStructure(rosetree, ofile);

                                                        loglikelihoodRes = (rosetree as ConstrainedRoseTree).LogLikelihood;
                                                        ofile.WriteLine("loglikelihood: " + loglikelihoodRes);
                                                        ofile.WriteLine("[run time] {0}s", runningTime);

                                                        structureInfo = rosetree.StructureInfo();

                                                        ofileMatlab.WriteLine("{0}, {1};", runningTime, loglikelihoodRes);

                                                        ///// draw tree ///
                                                        //try
                                                        //{
                                                        //    ExperimentRoseTree.DrawRoseTree(rosetree, experimentIndex + "_i", true);
                                                        //    ExperimentRoseTree.DrawConstraintTree(rosetree, experimentIndex + "c_i", true);
                                                        //}
                                                        //catch (Exception edrawtree)
                                                        //{
                                                        //    ofile.WriteLine("DrawTreeFailed: " + edrawtree.Message);
                                                        //}

                                                        /// set constraint tree ///
                                                        constraint.Set(rosetree);
                                                        previousRoseTree = rosetree;

                                                        experimentIndex++;

                                                        TuneParameterExperiments.PrintProgress(experimentIndex, experimentNumber, time_begin);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Console.WriteLine("Failed: " + e.Message);
                                                        Console.WriteLine(e.StackTrace);
                                                        ofile.WriteLine("Failed: " + e.Message);
                                                        ofile.WriteLine(e.StackTrace);
                                                        ofileMatlab.WriteLine("-1, -1;");
                                                        experimentIndex++;
                                                    }
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

            EndMatlabDataFunction(ofileMatlab);
            //ofile.WriteLine("\n=============================Final Result=============================");
            //ofile.WriteLine("----------------------------------------------------------------------");
            //for (int i = 0; i < experimentNumber; i++)
            //    ofile.Write(ExperimentResultToString(structureInfo[i], accuracyRes[i], loglikelihoodRes[i]) + "\t");
            ofile.WriteLine("\n======================================================================");
            ofile.WriteLine(TuneParameterExperiments.TimeEfficiencyToString(experimentNumber, time_begin));

            ofile.Close();
        }

        static string GetContinuousYearString(int beginYear, int endYear, string fieldhead)
        {
            string str = "";
            for (int year = beginYear; year < endYear; year++)
            {
                str += fieldhead + year + " OR ";
            }
            str += endYear;
            return str;
        }

        private static StreamWriter InitializeResultPrinter()
        {
            string filename = ExperimentParameters.Description + ".dat";
            filename = ExperimentParameters.ScalabilityResultPath + filename;

            if (!Directory.Exists(ExperimentParameters.ScalabilityResultPath))
                Directory.CreateDirectory(ExperimentParameters.ScalabilityResultPath);

            StreamWriter ofile = new StreamWriter(filename);
            ofile.WriteLine(AppDomain.CurrentDomain.FriendlyName);

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

            ofile.WriteLine("LooseOrderDeltaRatio = {0}", LooseTreeOrderConstraint.LooseOrderDeltaRatio);
          
            return ofile;
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

        public static string ExperimentResultArrayMeaning = "runtime, likelihood";

        private static void InitializeDrawRoseTreePath()
        {
            ExperimentParameters.DrawRoseTreePath = ExperimentParameters.ScalabilityResultPath
                + ExperimentParameters.Description + "\\";
        }
    }
}
