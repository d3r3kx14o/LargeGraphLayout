using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.Constants;

using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.Accuracy;
using EvolutionaryRoseTree.DataStructures;

namespace EvolutionaryRoseTree.Experiments
{
    class AccuracyExperiment
    {
        public static void Entry()
        {
            TraversalConstraintStrengthExperiment();
        }

        //public static void TestDataProjectionAccuracy()
        //{
        //    ExperimentParameters.Description = "DataProjectionAccuracy_" + ExperimentParameters.Description;
        //    StreamWriter ofile = InitializeResultPrinter();
        //    ofile.WriteLine("DataSet:\t" + ExperimentParameters.TwentyNewsGroupPath);

        //    //double[] alphas = new double[] { 0.1 , 0.2, 0.3, 0.4, 0.5, 0.8, 1, 2, 3, 5 };
        //    //double[] gammas = new double[] { 0.005 , 0.01, 0.02, 0.03, 0.04, 0.05, 0.06, 0.07, 0.08, 0.1, 0.2, 0.3 };
        //    double[] alphas = new double[] { 0.01, 0.1, 0.2, 0.4, 0.6, 0.8, 1, 3 };// , 10 };
        //    double[] gammas = new double[] { 0.001, 0.005, 0.01, 0.03, 0.05, 0.08, 0.1, 0.3, 0.6 };
        //    int[] samplenumbers = new int[] { 100 };
        //    int[] models = new int[]{
        //         Constant.DCM};
        //    int[] sampletimes = new int[] { 1, 2, 3, 4, 5 };
        //    int[] infosampletimes = new int[] { 6, 7, 8, 9, 10 };

        //    if (infosampletimes.Length != sampletimes.Length)
        //        throw new Exception("[ERROR]Constraint Sample Times Length Does Not Match!");

        //    //records of result
        //    int experimentNumber = alphas.Length * gammas.Length * samplenumbers.Length
        //        * algorithms.Length * models.Length * sampletimes.Length * rules.Length * constraints.Length;
        //    RoseTreeStructureInfo[][] structureInfo = new RoseTreeStructureInfo[experimentNumber][];
        //    double[][] accuracyRes = new double[experimentNumber][];
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
        //                                    ofile.WriteLine("<Constraint " + iconstraint + ">");
        //                                    try
        //                                    {
        //                                        ofile.Write("Rule: " + irule + "\t");
        //                                        PrintRoseTreeParameters(ofile);

        //                                        rosetree = ExperimentRoseTree.GetRoseTree(ldinfo);

        //                                        PrintRoseTreeStructure(rosetree, ofile);
        //                                        structureInfo[experimentIndex] = rosetree.StructureInfo();
        //                                        accuracyRes[experimentIndex] = LabelAccuracy.OutputAllAccuracy(rosetree, gtrosetree, ofile);
        //                                        ofile.WriteLine(ExperimentResultToString(structureInfo[experimentIndex], accuracyRes[experimentIndex]));
        //                                        experimentIndex++;
        //                                    }
        //                                    catch (Exception e)
        //                                    {
        //                                        ofile.WriteLine("Failed:" + e.Message);
        //                                        ofile.WriteLine(ExperimentResultToString(structureInfo[experimentIndex], accuracyRes[experimentIndex]));
        //                                        experimentIndex++;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

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
        //        ofile.Write(ExperimentResultToString(structureInfo[i], accuracyRes[i]) + "\t");
        //    ofile.WriteLine("\n======================================================================");

        //    ofile.Close();
        //}

        public static void ConstraintTreeAccuracy()
        {
            ExperimentParameters.Description = "CTreeAccuracy" + "_" +
                String.Format(ExperimentParameters.TimeFormat, DateTime.Now);
            StreamWriter ofile = InitializeResultPrinter();

            ExperimentParameters.SampleNumber = 10;
            ExperimentParameters.DatasetIndex = RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP;
            ofile.WriteLine("SampleNumber: " + ExperimentParameters.SampleNumber);
            ofile.Flush();

            int repeatTimes = 10;

            for (int iTestTime = 0; iTestTime < repeatTimes; iTestTime++)
            {
                ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
                ExperimentParameters.SampleTimes = 2 * iTestTime;
                RoseTree groundtruth_constraint = ExperimentRoseTree.GetRoseTree();

                ExperimentParameters.SampleTimes = 2 * iTestTime + 1;
                RoseTree groundtruth = ExperimentRoseTree.GetRoseTree();
                //ExperimentParameters.ConstraintRoseTree = groundtruth;

                LoadDataInfo ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo;

                ofile.WriteLine("------------------------" + iTestTime + "------------------------");
                ofile.WriteLine(DateTime.Now);
                ConstraintTree constree = new ConstraintTree(groundtruth_constraint, ldinfo.lfv);
                LabelAccuracy.OutputAllAccuracy(constree, groundtruth, ofile);
                ofile.WriteLine();

                ofile.WriteLine();
            }

            ofile.Close();
        }

        public static void TraversalConstraintStrengthExperiment()
        {
            //ExperimentParameters.RoseTreeParameters.algorithm_index = RoseTreeTaxonomy.Constants.Constant.BRT;
            ExperimentParameters.Description = "ConstraintStrength" + "_" +
                String.Format(ExperimentParameters.TimeFormat, DateTime.Now);
            StreamWriter ofile = InitializeResultPrinter();

            double[] punishweightset_distance = new double[] {1e-4, 1e-2, 1e1, 1e5, 1e50 };
            double[] punishweightset_order = new double[] { 1e-5, 1e-3, 1e-1, 100, 1e20, Double.MaxValue };
            int repeatTimes = 10;
            double[,][] accuracyresult = new double[repeatTimes, 
                2 + punishweightset_distance.Length + punishweightset_order.Length][];

            ExperimentParameters.SampleNumber = 1000;
            ExperimentParameters.DatasetIndex = RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP;
            ofile.WriteLine("SampleNumber: " + ExperimentParameters.SampleNumber);
            //ofile.WriteLine("SampleNumber: " + ExperimentParameters.SampleNumber + "\tBRT");


            ofile.Flush();

            for (int iTestTime = 0; iTestTime < repeatTimes; iTestTime++)
            {
                ofile.WriteLine("------------------------" + iTestTime + "------------------------");
                ofile.WriteLine(DateTime.Now);
                int algorithmcnt = 0;

                ExperimentParameters.ConstraintType = ConstraintType.GroundTruth;
                ExperimentParameters.SampleTimes = 2 * iTestTime;
                RoseTree groundtruth_constraint = ExperimentRoseTree.GetRoseTree();
                ExperimentParameters.ConstraintRoseTree = groundtruth_constraint;
                ofile.WriteLine((groundtruth_constraint as GroundTruthRoseTree).LabelsCountToString());

                ExperimentParameters.SampleTimes = 2 * iTestTime + 1;
                RoseTree groundtruth = ExperimentRoseTree.GetRoseTree();
                //ExperimentParameters.ConstraintRoseTree = groundtruth;
                ofile.WriteLine((groundtruth as GroundTruthRoseTree).LabelsCountToString());
                ofile.WriteLine();

                LoadDataInfo ldinfo = ExperimentRoseTree.LastRoseTreeLoadDataInfo;

                //-----------------------------------------------------------------------//
                //Test directly projection Accuracy
                ofile.WriteLine("<|Direct Project|>");
                ConstraintTree constree = new ConstraintTree(groundtruth_constraint, ldinfo.lfv);
                accuracyresult[iTestTime, algorithmcnt++] = LabelAccuracy.OutputAllAccuracy(constree, groundtruth, ofile);
                ofile.WriteLine();

                //Test No Constraint Accuracy
                ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;
                RoseTree rosetree0 = ExperimentRoseTree.GetRoseTree(ldinfo);
                ofile.WriteLine("<|No Constraint|>");
                accuracyresult[iTestTime, algorithmcnt++] = LabelAccuracy.OutputAllAccuracy(rosetree0, groundtruth, ofile);
                ofile.WriteLine();
                TuneParameterExperiments.PrintRoseTreeStructure(rosetree0, ofile);

                //Test Distance Constraint Accuracy
                ExperimentParameters.ConstraintType = ConstraintType.TreeDistance;
                ofile.WriteLine("<|Distance Constraint|>");
                for (int i = 0; i < punishweightset_distance.Length; i++)
                {
                    double punishweight = punishweightset_distance[i];
                    ExperimentParameters.TreeDistancePunishweight = punishweight;
                    RoseTree rosetree = ExperimentRoseTree.GetRoseTree(ldinfo);
                    ofile.WriteLine(punishweight+":");
                    accuracyresult[iTestTime, algorithmcnt++] = LabelAccuracy.OutputAllAccuracy(rosetree, groundtruth, ofile);
                    TuneParameterExperiments.PrintRoseTreeStructure(rosetree, ofile);
                }
                ofile.WriteLine();

                //Test Order Constraint Accuracy
                ExperimentParameters.ConstraintType = ConstraintType.TreeOrder;
                ofile.WriteLine("<|Order Constraint|>");
                for (int i = 0; i < punishweightset_order.Length; i++)
                {
                    double punishweight = punishweightset_order[i];
                    ExperimentParameters.IncreaseOrderPunishweight =
                        ExperimentParameters.LoseOrderPunishweight = punishweight;
                    RoseTree rosetree = ExperimentRoseTree.GetRoseTree(ldinfo);
                    ofile.WriteLine(punishweight + ":");
                    accuracyresult[iTestTime, algorithmcnt++] = LabelAccuracy.OutputAllAccuracy(rosetree, groundtruth, ofile);
                    TuneParameterExperiments.PrintRoseTreeStructure(rosetree, ofile);

                    if (i == punishweightset_order.Length - 1)
                        LabelAccuracy.OutputAllAccuracy(constree, rosetree, ofile);
                }
                ofile.WriteLine();

                ofile.WriteLine(AccuracyResultToString(accuracyresult, iTestTime));
                ofile.WriteLine();
            }

            ofile.WriteLine("------------------------Final Result------------------------");
            for (int iTestTime = 0; iTestTime < repeatTimes; iTestTime++)
                ofile.WriteLine(AccuracyResultToString(accuracyresult, iTestTime));
            ofile.Close();
        }

        private static string AccuracyResultToString(double[,][] accuracyresult, int testtime)
        {
            string str = "";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < accuracyresult.GetLength(1); j++)
                    str += accuracyresult[testtime, j][i] + "\t";
                str += ";\n";
            }
            return str;
        }

        private static StreamWriter InitializeResultPrinter()
        {
            DateTime datetime = DateTime.Now;
            string filename = ExperimentParameters.Description + ".dat";
            filename = ExperimentParameters.AccuracyResultPath + filename;

            if (!Directory.Exists(ExperimentParameters.AccuracyResultPath))
                Directory.CreateDirectory(ExperimentParameters.AccuracyResultPath);

            StreamWriter ofile = new StreamWriter(filename);

            return ofile;
        }

    }
}
