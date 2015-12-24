//#define APPROXIMATE_LIKELIHOOD
//#define WRITE_PROJECTION_CONTENT
//#define NEW_CONSTRAINT_MODEL
//#define UNSORTED_CACHE
//#define CONSTRAINT_CHANGE_UPDATE_ALL
//#define NEW_MODEL_2
//#define NEW_MODEL_3
//#define SMOOTHNESS_ANALYSE
//#define SCALABILITY_TEST
//#define DISTANCE_CONSTRAINT_2
//#define OPEN_LARGE_CLUSTER
//#define OPEN_LARGE_CLUSTER_MOD_2
//#define OPEN_LARGE_CLUSTER_NO_CONSTRAINT
//#define NEW_YORK_TIMES_TEST_SMOOTHNESS
//#define NORMALIZED_SMOOTHNESS_COST
//#define COLLAPSE_SMALL_CLUSTER
//#define AVERAGE_ORDER_COST
//#define AVERAGE_ORDER_COST2
//#define NYT_LEADING_PARAGRAPH
//#define ADJUST_BINGNEW_STEPBYSTEPl
//#define ADJUST_TREE_STRUCTURE
//#define NORMALIZE_PROJ_WEIGHT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EvolutionaryRoseTree.Data;
using EvolutionaryRoseTree.Tests;
using EvolutionaryRoseTree.Experiments;
using EvolutionaryRoseTree.Util;
using System.Diagnostics;
using System.Threading;
using System.IO;
using ConstrainedRoseTreeLibrary.Data;

namespace EvolutionaryRoseTree
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestRoseTreeLibary();

            //Experiment.PrintDefines();

            //===========================================//

            //TestReadingData.Entry();

            //DataPreprocessing.Entry();

            //DataAnalysis.Entry();

            //Test.TestEntry();

            //AccuracyExperiment.Entry();

            //TuneParameterExperiments.Entry();

            //ScalabilityExperiment.Entry();

            //EvolutionaryExperiments.Entry();

            //RunTKDEExperimentsOnMultipleComputers();

            //===========================================//

            //Console.ReadKey();
        }

        //private static void TestRoseTreeLibary()
        //{

        //    //raw docs
        //    Dictionary<string, int> v1 = new Dictionary<string, int>() { { "a", 10 }, { "b", 1 } };
        //    Dictionary<string, int> v2 = new Dictionary<string, int>() { { "a", 10 }, { "c", 1 } };
        //    Dictionary<string, int> v3 = new Dictionary<string, int>() { { "a", 10 }, { "d", 1 } };
        //    Dictionary<string, int> v4 = new Dictionary<string, int>() { { "a", 8 }, { "b", 4 }, { "c", 2 }, { "d", 2 }, { "e", 1 } };
        //    Dictionary<string, int> v5 = new Dictionary<string, int>() { { "a", 8 }, { "b", 4 }, { "c", 2 }, { "d", 2 }, { "f", 1 } };
        //    Dictionary<string, int> v6 = new Dictionary<string, int>() { { "a", 4 }, { "b", 8 }, { "c", 2 }, { "d", 2 }, { "e", 1 } };
        //    Dictionary<string, int> v7 = new Dictionary<string, int>() { { "a", 4 }, { "b", 8 }, { "c", 2 }, { "d", 2 }, { "f", 1 } };

        //    List<Dictionary<string, int>> vectors = new Dictionary<string, int>[] { v1, v2, v3, v4, v5, v6, v7 }.ToList();
        //    List<RawDocument> rawDocs = new List<RawDocument>(vectors.ConvertAll(vec => new RawDocument(vec)));

        //    //raw paras
        //    ConstrainedRoseTreeLibrary.BuildTree.RoseTreeParameters rtPara = new ConstrainedRoseTreeLibrary.BuildTree.RoseTreeParameters()
        //    {
        //        alpha = 0.05,
        //        gamma = 0.1,
        //        k = 3,
        //    };

        //    //build tree
        //    var rosetree = ConstrainedRoseTreeLibrary.BuildTree.BuildRoseTree.GetRoseTree(rawDocs, rtPara);

        //    //draw tree
        //    var drawTree = new ConstrainedRoseTreeLibrary.DrawTree.DrawRoseTree(rosetree);
        //    drawTree.DrawTree("rt.gv");

        //    //analyze tree
        //    foreach (var node in rosetree.GetAllTreeLeaf())
        //    {
        //        Console.Write(ConstrainedRoseTreeLibrary.AnalyzeTree.AnalyzeTreeData.GetNodeID(rosetree, node).ToString() + "\t");
        //    }
        //    Console.WriteLine();

        //}

        //static int threadIndex = 0;
        //private static void RunTKDEExperimentsOnMultipleComputers()
        //{
        //    var config = FileOperations.LoadConfigFile("configTKDE.txt");
            
        //    var threadNum = int.Parse(config["ThreadNumber"][0]);
        //    var indexPath = config["IndexPath"][0];
        //    var inputPath = config["InputPath"][0];
        //    var outputPath = config["OutputPath"][0];

        //    var machineName = System.Environment.MachineName;

        //    if (!inputPath.EndsWith("\\")) inputPath += "\\";
        //    if (!outputPath.EndsWith("\\")) outputPath += "\\";

        //    for (int i = 0; i < threadNum; i++)
        //    {
        //        var thread = new Thread(() =>
        //            {
        //                var threadName = machineName + "_" + threadIndex;

        //                while (true)
        //                {
        //                    var expConfig = GetTKDEExperimentConfig(inputPath, threadName);

        //                    if (expConfig == null)
        //                    {
        //                        //wait until the expConfig file is updated
        //                        Thread.Sleep(5000);
        //                    }
        //                    else
        //                    {
        //                        //start running experiments
        //                        var exp = new HandlingConflictExperiments(expConfig, inputPath, outputPath, threadName);
        //                        exp.Start();
        //                    }
        //                }
        //            });
        //        thread.Start();
        //        threadIndex++;

        //        Thread.Sleep(2000);
        //    }
        //}

        //private static TKDEExperimentConfig GetTKDEExperimentConfig(string inputPath,
        //    string threadName)
        //{
        //    var configPath = inputPath + "Config\\";
        //    var configFileName = configPath + "expConfig.txt";
        //    var lockFileName = configPath + "lock_" + threadName + ".lock";

        //    #region lock
        //    while (true)
        //    {
        //        File.Create(lockFileName);

        //        bool isGetLock = true;
        //        foreach (var filename in Directory.GetFiles(configPath))
        //        {
        //            if (filename.StartsWith("lock_") && filename != lockFileName)
        //            {
        //                isGetLock = false;
        //                break;
        //            }
        //        }

        //        if (isGetLock)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            File.Delete(lockFileName);
        //            Thread.Sleep((int)(1000 * (new Random((int)DateTime.Now.Ticks).NextDouble())));
        //        }
        //    }
        //    #endregion


        //    #region read & write
        //    var configTmpFileName = configFileName + ".tmp";
        //    var sr = new StreamReader(configFileName);
        //    var sw = new StreamWriter(configTmpFileName);

        //    TKDEExperimentConfig config = null;
        //    string line = sr.ReadLine();
        //    if (line != null && line.Length > 0)
        //    {
        //        config = new TKDEExperimentConfig(line);
        //    }

        //    while ((line = sr.ReadLine()) != null)
        //    {
        //        sw.WriteLine(line);
        //    }

        //    sw.Flush();
        //    sw.Close();

        //    sr.Close();

        //    File.Copy(configFileName, configTmpFileName);
        //    File.Delete(configTmpFileName);
        //    #endregion


        //    #region unlock
        //    File.Delete(lockFileName);
        //    #endregion

        //    return config;
        //}

      
    }
}
