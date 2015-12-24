using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Directory = System.IO.Directory;

using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Constants;

using EvolutionaryRoseTree.Constants;
using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.Accuracy;
using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Smoothness;


using Lucene.Net.Search;
using Lucene.Net.Store;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;
using ConstrainedRoseTreeLibrary.Data;

namespace EvolutionaryRoseTree.Tests
{
    class Test
    {
        public static void TestEntry()
        {
            //TestRoseTreeLibary();

            //TestBuildRuleRoseTree();
            //TestSingle20NGRoseTree_PrevExperiment();
            //TestBuildConstrainedTree();
            //TestBuildRuleRoseTree();
            //TestSampleFileOverlapRatio();
            //TestLoadNewYorkTimesData();
            //TestCalculateRobinsonFouldDistance();
            //TestAverageDegree();
            //TestGetMediumValue();
            //TestProgramMemory();
            //TestExpandedCacheClass();
            //TestSparseVectorAdd();
            //TestMinHeapDouble();            //in fact: max heap
            //TestMaxHeapDouble();            //messed up
            //TestMinHeapInt();            //in fact: max heap

            //TestBuildSingleRoseTree();
            //TestMSRCluster();

            //ChangeConfigEvolutionary();
            //GetAllFoldersInRoot();
            //ChangeConfigHTF();

            //RemoveResultToTheRoot();
            //AdjustResultNodeOrder();
            //TestSimpleAccuracy();

            //GetTopicNumber();

            //CountLeafTopicNumber();
            //DataStatistics();
            //TreeDepthDataStatistics();
            //FolderTreeDepthStatistics();

            //binary tree
            //TestBuildSingleBayesianBinaryTree();

            //Parse Result to TextFlow
            //GenerateInputForTextFlow();
            //PostProcessPropFile();
            //TestConvertBinaryProjectionFileToText();
            //ConvertRoseRiverToTextFlow();

            //AdjustResultNodeOrder();

            //PrintZeroTreeTimeRatio();

            //PrintDocumentProjections();
            //PrintRelatedDocuments();
            //PrintRelatedDocumentsBody();
            //TestTemp();

            //CalculateTripleAndFanViolationSumExperiment();
            //CalculateTripleAndFanViolationSum(
            //    @"C:\Users\v-xitwan\Desktop\temp\",
            //    "16c.gv", "16c0_last7.gv");
                //@"C:\Users\v-xitwan\Desktop\temp\",
                //"16c.gv", "16c0_last7.gv");

            //string[] names = new string[] { "ST12_CT1", "ST12_CT2", "ST12_CT3", "ST12_CT4", "ST12_CT5", "ST345_CT1", "ST345_CT2", "ST345_CT3", "ST345_CT4", "ST345_CT5"};
            //foreach(string name in names)
            //    ParseTuneParameterFileRF(name);

            //string[] names = new string[] { "CT1_", "CT2_", "CT3_", "CT4_", "CT5_" };
            //foreach (var name in names)
            //    ParseSmTripleFan(name);

            //string[] names = new string[] { "ConfCount_CT1_", "ConfCount_CT2_", "ConfCount_CT3_", "ConfCount_CT4_", "ConfCount_CT5_" };
            //foreach (var name in names)
            //    ParseConflictConstraints(name);

            //TestNMI();
            //TestMeanStd();
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
        //        alpha = 0.01,
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

        private static void TestMeanStd()
        {
            Statistics stat = new Statistics();
            for (int i = 0; i < 1000; i++)
            {
                stat.ObserveNumber(50);
            }
            stat.ObserveNumber(1000);
            Console.WriteLine(stat.GetAverage());
            Console.WriteLine(stat.GetStd());
        }

        private static void TestNMI()
        {
            int[] labels0 = new int[1000];
            int[] labels1 = new int[1000];
            for (int i = 0; i < labels0.Length; i++)
            {
                labels0[i] = i % 10;
            }
            for (int i = 0; i < labels1.Length; i++)
            {
                labels1[i] = i % 2;
            }

            Console.WriteLine(NMI.GetNormalizedMutualInfo(labels0, labels1));

            //Random random = new Random();
            //int[] labels0 = new int[1000];
            //int[] labels1 = new int[1000];
            //for (int i = 0; i < labels0.Length; i++)
            //{
            //    labels0[i] = random.Next(60); 
            //}
            //for (int i = 0; i < labels1.Length; i++)
            //{
            //    labels1[i] = labels0[i] / 4;
            //    //labels1[i] = random.Next(20);
            //}
            //Console.WriteLine(NMI.GetNormalizedMutualInfo(labels0, labels1));
        }

        private static void ParseSmTripleFan(string name)
        {
            string directory = @"D:\Project\EvolutionaryRoseTreeData\TuneParameters\MultiConstraintTree\SmTripleFan\";
            //string name = "CT1_";
            string[] inputfilenames = new string[] { name + "ST1.dat", name + "ST2.dat", name + "ST3.dat", name + "ST4.dat", name + "ST5.dat" };
            int[] startExperimentId = new int[] { 0, 72, 0, 72, 144 };
            string outputfilename = "GetData_SmTripleFan_" + name.Substring(0,3) + ".m";

            StreamWriter ofile = new StreamWriter(directory + outputfilename);
            ofile.WriteLine("function data = GetData_SmTripleFan()");
            ofile.WriteLine("data = [...");

            int experimentid;
            int printedSmTripleFan = 0;
            int totalSmTripleFan = 14;
            //Regex r = new Regex("^(?<experimentIndex>\\d+)\t(?<k>\\d+):\t(?<violationNumber>\\d+)\t(?<violationRatio>\\d+)\t");
            int ifile = 0;
            foreach (string inputfilename in inputfilenames)
            {
                string[] lines = File.ReadAllLines(directory+inputfilename);
                experimentid = startExperimentId[ifile];
                foreach (string line in lines)
                {
                    //Match m = r.Match(line);
                    string[] tokens = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] tokens2 = tokens[0].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] tokens3 = tokens[1].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    int experimentid_now = int.Parse(tokens2[0]);
                    double violationNumber = double.Parse(tokens3[0]);
                    double violationRatio = double.Parse(tokens3[1]);

                    if (experimentid_now != experimentid)
                    {
                        for (int iexp = 0; iexp < experimentid_now - experimentid; iexp++)
                        {
                            for (int i = printedSmTripleFan; i < totalSmTripleFan; i++)
                                ofile.Write("0\t");
                            ofile.WriteLine(";");
                            printedSmTripleFan = 0;
                        }
                        experimentid = experimentid_now;
                    }

                    ofile.Write("{0}\t{1}\t", (-violationNumber), (-violationRatio));
                    printedSmTripleFan += 2;
                    ofile.Flush();

                }

                ofile.WriteLine(";");
                printedSmTripleFan = 0;
                ifile++;
            }

            ofile.WriteLine("];");
            ofile.WriteLine("end");
            ofile.Close();
        }

        private static void ParseConflictConstraints(string name)
        {
            string directory = @"D:\Project\EvolutionaryRoseTreeData\TuneParameters\MultiConstraintTree\ConflictConstraints\";
            //string name = "ConfCount_CT1_";
            string[] inputfilenames = new string[] { name + "ST1.dat", name + "ST2.dat", name + "ST3.dat", name + "ST4.dat", name + "ST5.dat" };
            int[] startExperimentId = new int[] { 0, 72, 0, 72, 144 };
            string outputfilename = "GetData_" + name.Substring(0, 13) + ".m";

            StreamWriter ofile = new StreamWriter(directory + outputfilename);
            ofile.WriteLine("function data = GetData_ConflictConstraints()");
            ofile.WriteLine("data = [...");

            int experimentid;
            int printedConflictConstraints = 0;
            int totalConflictConstraints = 4;
            //Regex r = new Regex("^(?<experimentIndex>\\d+)\t(?<k>\\d+):\t(?<violationNumber>\\d+)\t(?<violationRatio>\\d+)\t");
            int ifile = 0;
            foreach (string inputfilename in inputfilenames)
            {
                string[] lines = File.ReadAllLines(directory + inputfilename);
                experimentid = startExperimentId[ifile];
                foreach (string line in lines)
                {
                    //Match m = r.Match(line);
                    string[] tokens = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    int experimentid_now = int.Parse(tokens[0]);
                    //double violationNumber = double.Parse(tokens[0]);
                    //double violationRatio = double.Parse(tokens3[1]);

                    if (experimentid_now != experimentid)
                    {
                        for (int iexp = 0; iexp < experimentid_now - experimentid; iexp++)
                        {
                            for (int i = printedConflictConstraints; i < totalConflictConstraints; i++)
                                ofile.Write("0\t");
                            ofile.WriteLine(";");
                            printedConflictConstraints = 0;
                        }
                        experimentid = experimentid_now;
                    }

                    ofile.Write("{0}\t{1}\t{2}\t{3}\t", tokens[1], tokens[2], tokens[3], tokens[4]);
                    printedConflictConstraints += 4;
                    ofile.Flush();

                }

                ofile.WriteLine(";");
                printedConflictConstraints = 0;
                ifile++;
            }

            ofile.WriteLine("];");
            ofile.WriteLine("end");
            ofile.Close();
        }

        private static void ParseTuneParameterFileRF(string name)
        {
            string directory = @"D:\Project\EvolutionaryRoseTreeData\TuneParameters\MultiConstraintTree\";
            //string name = "ST345_CT5";
            string filename = "Exp4_NYT_RF_C0_Order_k7c0_" + name + ".dat";
            string outputfilename = "GetData_" + name + ".m";

            StreamWriter ofile = new StreamWriter(directory + outputfilename);
            string[] lines = File.ReadAllLines(directory + filename);
            int printedRF = 0;
            foreach (string line in lines)
            {
                if (line.StartsWith("Exp"))
                {
                    for(int i=printedRF;i<14;i++)
                        ofile.Write("0\t");
                    ofile.WriteLine(";...");
                    printedRF = 0;
                }
                else if(line.StartsWith("[RF]"))
                {
                    string[] tokens = line.Substring(4).Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    ofile.Write("{0}\t{1}\t", double.Parse(tokens[0]), double.Parse(tokens[1]));
                    printedRF += 2;
                    ofile.Flush();
                }
            }
            ofile.Close();
        }

        #region calculate triple and fan violation sum experiment
        static void CalculateTripleAndFanViolationSumExperiment()
        {
            string directory = @"D:\Projects\EvoTree\Project\EvolutionaryRoseTreeData\TuneParameters\MultipleConstraintTreeData\Exp4_NYT_RF_C0_Order_k7c0_ST345_CT5\";
            int startFileId = 144;
            int checkFileNumber = 72;
            string outputfilename = @"D:\Projects\EvoTree\Project\EvolutionaryRoseTreeData\TuneParameters\MultipleConstraintTreeData\CT5_ST5.dat";

            int predExperimentNumber = 280;


            int experimentId = 0;
            StreamWriter ofile = new StreamWriter(outputfilename);
            DateTime beginTime = DateTime.Now;
            for (int i = startFileId; i < startFileId + checkFileNumber; i++)
            {
                string filename0 = i + "c.gv";
                for (int j = 1; j <= 7; j++)
                {
                    string filename1 = i + "c0_last" + j + ".gv";
                    if (File.Exists(directory + filename1))
                    {
                        double[] res = CalculateTripleAndFanViolationSum(directory, filename0, filename1);
                        ofile.WriteLine("{0}\t{1}:\t{2}\t{3}\t{4}", i, j, res[0], res[1], res[2]);
                        ofile.Flush();
                        experimentId++;
                        EvolutionaryRoseTree.Experiments.TuneParameterExperiments.PrintProgress(experimentId, predExperimentNumber, beginTime);
                    }
                    else
                        break;
                }
            }

            ofile.Close();

        }


        static double[] CalculateTripleAndFanViolationSum(string directory, string filename0, string filename1)
        {
            //string directory = @"D:\Projects\EvoTree\Project\EvolutionaryRoseTreeData\TuneParameters\MultipleConstraintTreeData\Exp4_NYT_RF_C0_Order_k7c0_ST12_CT1\";
            //string filename0 = "1c.gv";
            //string filename1 = "1c0_last1.gv";

            //string directory = @"D:\Projects\EvoTree\Project\EvolutionaryRoseTreeData\TuneParameters\MultipleConstraintTreeData\";
            //string filename0 = "fake0_0.gv";
            //string filename1 = "fake0_1.gv";

            long t0 = DateTime.Now.Ticks;

            ConstraintTreeFromFile ctree0 = new ConstraintTreeFromFile(directory + filename0);
            ConstraintTreeFromFile ctree1 = new ConstraintTreeFromFile(directory + filename1);

            int leafNodeIndex0, leafNodeIndex1, leafNodeIndex2;
            int depth0_1_ctree0, depth0_2_ctree0, depth1_2_ctree0;
            int depth0_1_ctree1, depth0_2_ctree1, depth1_2_ctree1;
            List<int> leafNodeIndices = ctree0.LeafNodeIndices;

            int violationNumber = 0;
            int leafNumber = leafNodeIndices.Count;
            for (int i = 0; i < leafNodeIndices.Count; i++)
            {
                if (i % 10 == 0)
                    Console.WriteLine(i);
                for (int j = i + 1; j < leafNodeIndices.Count; j++)
                    for (int k = j + 1; k < leafNodeIndices.Count; k++)
                    {
                        leafNodeIndex0 = leafNodeIndices[i];
                        leafNodeIndex1 = leafNodeIndices[j];
                        leafNodeIndex2 = leafNodeIndices[k];

                        depth0_1_ctree0 = ctree0.GetCommonAncestor(leafNodeIndex0, leafNodeIndex1).depthInTree;
                        depth0_1_ctree1 = ctree1.GetCommonAncestor(leafNodeIndex0, leafNodeIndex1).depthInTree;
                        depth0_2_ctree0 = ctree0.GetCommonAncestor(leafNodeIndex0, leafNodeIndex2).depthInTree;
                        depth0_2_ctree1 = ctree1.GetCommonAncestor(leafNodeIndex0, leafNodeIndex2).depthInTree;

                        if (!CheckTripleRelationConsistent(depth0_1_ctree0, depth0_1_ctree1, depth0_2_ctree0, depth0_2_ctree1))
                        {
                            violationNumber++;
                            continue;
                        }

                        depth1_2_ctree0 = ctree0.GetCommonAncestor(leafNodeIndex1, leafNodeIndex2).depthInTree;
                        depth1_2_ctree1 = ctree1.GetCommonAncestor(leafNodeIndex1, leafNodeIndex2).depthInTree;

                        if (!CheckTripleRelationConsistent(depth1_2_ctree0, depth1_2_ctree1, depth0_2_ctree0, depth0_2_ctree1))
                        {
                            violationNumber++;
                            continue;
                        }

                        if (!CheckTripleRelationConsistent(depth0_1_ctree0, depth0_1_ctree1, depth1_2_ctree0, depth1_2_ctree1))
                        {
                            violationNumber++;
                            continue;
                        }

                        //Console.WriteLine("{0},{1},{2}\n", leafNodeIndex0, leafNodeIndex1, leafNodeIndex2);
                    }
            }

            long t1 = DateTime.Now.Ticks;

            double[] res = new double[] { violationNumber, (100.0 * violationNumber / leafNumber / (leafNumber - 1) / (leafNumber - 2) * 6), (t1 - t0) / 1e8 };

            Console.WriteLine("Violation Number: {0}", violationNumber);
            Console.WriteLine("Violation Ratio: {0}%", (100.0 * violationNumber / leafNumber / (leafNumber - 1) / (leafNumber - 2) * 6));
            Console.WriteLine("{0} s", (t1 - t0) / 1e7);

            Console.ReadLine();
            return res;
        }

        static bool CheckTripleRelationConsistent(int depth0_tree0, int depth0_tree1,
            int depth1_tree0, int depth1_tree1)
        {
            if ((depth0_tree1 - depth1_tree1) * (depth0_tree0 - depth1_tree0) > 0)
                return true;
            else if (depth0_tree1 == depth1_tree1 && depth0_tree0 == depth1_tree0)
                return true;
            else return false;
        }

        class ConstraintTreeNodeFromFile
        {
            public ConstraintTreeNodeFromFile(int treeindex, int nodeindex, ConstraintTreeNodeFromFile parent)
            {
                this.treeindex = treeindex;
                this.nodeindex = nodeindex;

                if (parent != null)
                {
                    this.depthInTree = parent.depthInTree + 1;
                    this.parent = parent;
                }
                else
                {
                    this.depthInTree = 0;
                    this.parent = null;
                }
            }

            public int treeindex;
            public int nodeindex;
            public int depthInTree;

            public ConstraintTreeNodeFromFile parent;
        }

        class ConstraintTreeFromFile
        {
            List<ConstraintTreeNodeFromFile> nodelist = new List<ConstraintTreeNodeFromFile>();
            Dictionary<int, Dictionary<int, ConstraintTreeNodeFromFile>> CommonAncestorCache = new Dictionary<int, Dictionary<int, ConstraintTreeNodeFromFile>>();
            public int LeafNumber = 0;
            public List<int> LeafNodeIndices = new List<int>();
            Dictionary<int, int> LeafNodeToTreeIndexDic = new Dictionary<int, int>();
            Dictionary<int, int> LeaftTreeToNodeIndexDic = new Dictionary<int, int>();

            public ConstraintTreeFromFile(string filename)
            {
                string[] lines = File.ReadAllLines(filename);
                int lineid = 4;

                string[] splittoken = new string[] { "->" };
                Regex r = new Regex("-(?<nodeid>\\d+)-");
                Match m;

                ConstraintTreeNodeFromFile root = new ConstraintTreeNodeFromFile(0, -1, null);
                nodelist.Add(root);

                while (lines.Length > lineid + 1)
                {
                    string line0 = lines[lineid++];
                    string line1 = lines[lineid++];

                    int parentTreeIndex = int.Parse(line0.Split(splittoken, StringSplitOptions.RemoveEmptyEntries)[0]);
                    int childTreeIndex = int.Parse(line0.Split(splittoken, StringSplitOptions.RemoveEmptyEntries)[1]);

                    int childNodeIndex = -1;
                    m = r.Match(line1);
                    if (m.Success && line1.Contains(" ]\""))
                    {
                        childNodeIndex = int.Parse(m.Result("${nodeid}"));
                        LeafNumber++;
                        LeafNodeIndices.Add(childNodeIndex);
                        LeafNodeToTreeIndexDic.Add(childNodeIndex, childTreeIndex);
                        LeaftTreeToNodeIndexDic.Add(childTreeIndex, childNodeIndex);
                    }

                    ConstraintTreeNodeFromFile node = new ConstraintTreeNodeFromFile(childTreeIndex, childNodeIndex, nodelist[parentTreeIndex]);
                    nodelist.Add(node);
                }
            }

            public ConstraintTreeNodeFromFile GetCommonAncestor(int leafNodeIndex0, int leafNodeIndex1)
            {
                if (leafNodeIndex0 > leafNodeIndex1)
                {
                    leafNodeIndex0 += leafNodeIndex1;
                    leafNodeIndex1 = leafNodeIndex0 - leafNodeIndex1;
                    leafNodeIndex0 = leafNodeIndex0 - leafNodeIndex1;
                }

                if (CommonAncestorCache.ContainsKey(leafNodeIndex0)
                    && CommonAncestorCache[leafNodeIndex0].ContainsKey(leafNodeIndex1))
                {
                    return CommonAncestorCache[leafNodeIndex0][leafNodeIndex1];
                }
                else
                {
                    List<ConstraintTreeNodeFromFile> ancestors0 = new List<ConstraintTreeNodeFromFile>();
                    ConstraintTreeNodeFromFile leaf0 = nodelist[LeafNodeToTreeIndexDic[leafNodeIndex0]];
                    ConstraintTreeNodeFromFile leaf1 = nodelist[LeafNodeToTreeIndexDic[leafNodeIndex1]];
                    ConstraintTreeNodeFromFile commonancestor = null;
                    while (leaf0.parent != null)
                    {
                        ancestors0.Add(leaf0.parent);
                        leaf0 = leaf0.parent;
                    }

                    while (leaf1.parent != null)
                    {
                        if (ancestors0.Contains(leaf1.parent))
                        {
                            commonancestor = leaf1.parent;
                            break;
                        }
                        leaf1 = leaf1.parent;
                    }

                    if (!CommonAncestorCache.ContainsKey(leafNodeIndex0))
                        CommonAncestorCache.Add(leafNodeIndex0, new Dictionary<int, ConstraintTreeNodeFromFile>());
                    //if (CommonAncestorCache[leafNodeIndex0].ContainsKey(leafNodeIndex1))
                    CommonAncestorCache[leafNodeIndex0].Add(leafNodeIndex1, commonancestor);

                    return commonancestor;
                }

            }
        }
        #endregion 

        private static void TestTemp()
        {
            //Console.WriteLine(@"\[(?<docluceneindex>\d+)\]");
            string str0 = "103[color = grey, label =\"-4543-\\nsony(152)\\ne3(119)\n";
            Regex r = new Regex("(?<treenodetreeid>\\d+)\\[color = grey, label =\\\"-" + 4543 + "-(?<tail>.+)\n");// new Regex(@"\[(?<docluceneindex>\d+)\]");
            Match m = r.Match(str0);
            Console.WriteLine(m.Success);
            if (m.Success)
            {
                Console.WriteLine(m.Result("${treenodetreeid}\n${tail}"));
            }
        }

        private static void PrintRelatedDocuments()
        {
            string folder = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected_0202\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D_Modified\";
            //string folder = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0522_095405_gamma0.42alpha0.003KNN100merge0.01split0.01cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            string rosetreefilename;

            string luceneindexpath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Jan-July_newMS\";
            //string luceneindexpath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\debt_crisis2\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar2\";
            IndexSearcher indexsearcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(luceneindexpath)), true);
            string titlefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;

            Regex r = new Regex("\\[(?<docluceneid>\\d+)\\]");
            while (true)
            {
                int nodeid;
                string keyword;
                Console.WriteLine("Input rose tree file name:");
                rosetreefilename = Console.ReadLine();
                Console.WriteLine("Input tree node index:");
                nodeid = int.Parse(Console.ReadLine());
                Console.WriteLine("Input keyword:");
                keyword = Console.ReadLine();
                Regex r_keyword = new Regex(keyword, RegexOptions.IgnoreCase);

                Dictionary<int, string> relatedlines = FindRelatedLines(File.ReadAllText(folder + rosetreefilename), nodeid);

                Console.WriteLine("-----------------Related Documents----------------");
                foreach (var kvp in relatedlines)
                {
                    int docluceneid = int.Parse(r.Match(kvp.Value).Result("${docluceneid}"));
                    string title = indexsearcher.Doc(docluceneid).Get(titlefield);
                    if (r_keyword.Match(title).Success)
                        Console.WriteLine("-{0}-\t{1}", kvp.Key, title);
                }
                Console.WriteLine("-------------------------------------------------");
            }
        }

        private static void PrintRelatedDocumentsBody()
        {
            string folder = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected_0202\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D_Modified\";
            //string folder = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0522_095405_gamma0.42alpha0.003KNN100merge0.01split0.01cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            string rosetreefilename;

            string luceneindexpath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Jan-July_newMS\";
            //string luceneindexpath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\debt_crisis2\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar2\";
            IndexSearcher indexsearcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(luceneindexpath)), true);
            string titlefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            string bodyfield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleDescription;

            Regex r = new Regex("\\[(?<docluceneid>\\d+)\\]");
            while (true)
            {
                int nodeid;
                string[] keywords;
                Console.WriteLine("Input rose tree file name:");
                rosetreefilename = Console.ReadLine();
                Console.WriteLine("Input tree node index:");
                nodeid = int.Parse(Console.ReadLine());
                Console.WriteLine("Input pattern:");
                keywords = Console.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                //string pattern = "(";
                //foreach (var keyword in keywords)
                //    pattern += keyword + "|";
                //pattern = pattern.Substring(0, pattern.Length - 1);
                //pattern += ")";
                //string pattern = Console.ReadLine();
                List<Regex> r_keywords = new List<Regex>(); // new Regex(pattern, RegexOptions.IgnoreCase);
                foreach (var keyword in keywords)
                    r_keywords.Add(new Regex(keyword, RegexOptions.IgnoreCase));

                Dictionary<int, string> relatedlines = FindRelatedLines(File.ReadAllText(folder + rosetreefilename), nodeid);

                Console.WriteLine("-----------------Related Documents----------------");
                foreach (var kvp in relatedlines)
                {
                    int docluceneid = int.Parse(r.Match(kvp.Value).Result("${docluceneid}"));
                    string title = indexsearcher.Doc(docluceneid).Get(titlefield);
                    string body = indexsearcher.Doc(docluceneid).Get(bodyfield);
                    string all = title + "." + body;
                    string[] lines = all.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                    //if (r_keyword.Match(title).Success)
                    //    Console.WriteLine("-{0}-\t{1}", kvp.Key, title);

                    List<string> matchlines = new List<string>();
                    foreach (string line in lines)
                    {
                        bool bflag = true;
                        foreach (var r_keyword in r_keywords)
                            if (!r_keyword.Match(line).Success)
                            {
                                bflag = false;
                                break;
                            }
                        if (bflag)
                            matchlines.Add(line);
                    }

                    if (matchlines.Count != 0)
                    {
                        Console.WriteLine("-{0}-\t", kvp.Key);
                        foreach (var line in matchlines)
                            Console.WriteLine(line);
                    }
                }
                Console.WriteLine("-------------------------------------------------");
            }
        }

        private static void PrintDocumentProjections()
        {
            //Leaf topic nodes only!!!

            string folder = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected_0202\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D_Modified\";
            string rosetreefilename0 = "0.gv";
            string rosetreefilename1 = "1.gv";
            string constraintfile = "1c.gv";

            string luceneindexpath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Jan-July_newMS\";
            IndexSearcher indexsearcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(luceneindexpath)), true);

            while (true)
            {
                int treenodeid0 = -1, treenodeid1 = -1;
                Console.WriteLine("Input node id for the first tree:");
                treenodeid0 = int.Parse(Console.ReadLine());
                Console.WriteLine("Input node id for the second tree:");
                treenodeid1 = int.Parse(Console.ReadLine());

                List<DocumentProjection> docprojs = GetDocumentProjections(treenodeid0, treenodeid1, folder, rosetreefilename0, rosetreefilename1, constraintfile);

                Console.WriteLine("---------------Document Projections--------------");

                foreach (var proj in docprojs)
                    Console.Write(proj.ToString(indexsearcher));

                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine();
            }

        }

        private static List<DocumentProjection> GetDocumentProjections(int treenodeid0, int treenodeid1, string folder, string rosetreefilename0, string rosetreefilename1, string constraintfile)
        {
            string lines0 = File.ReadAllText(folder + rosetreefilename0);
            string lines1 = File.ReadAllText(folder + rosetreefilename1);
            string ctlines = File.ReadAllText(folder + constraintfile);

            Dictionary<int, string> relatedlines0 = FindRelatedLines(lines0, treenodeid0);
            Dictionary<int, string> relatedlines1 = FindRelatedLines(lines1, treenodeid1);
            Dictionary<int, string> ctrelatedlines = FindRelatedLines(ctlines, treenodeid1);

            Regex r_projprevid = new Regex("\\[\\s(?<projid>\\d+)\\s\\]");
            List<DocumentProjection> projs = new List<DocumentProjection>();

            foreach (var kvp in ctrelatedlines)
            {
                int projid1 = kvp.Key;
                Match m = r_projprevid.Match(kvp.Value);
                if(m.Success)
                {
                    int projid0 = int.Parse(m.Result("${projid}"));

                    if (relatedlines0.ContainsKey(projid0))
                    {
                        DocumentProjection proj = new DocumentProjection();
                        proj.docLabel0 = relatedlines0[projid0];
                        proj.docLabel1 = relatedlines1[projid1];

                        projs.Add(proj);
                    }
                }
            }
            return projs;
        }


        private static Dictionary<int, string> FindRelatedLines(string lines, int treenodeid)
        {
            Dictionary<int, string> relatedlines = new Dictionary<int, string>();
            Regex r_nodeid = new Regex("-(?<nodeid>\\d+)-");

            Regex r_treeid = new Regex("(?<treenodetreeid>\\d+)\\[color.+-" + treenodeid + "-");//(?<tail>.+)\n");
            int treeid = int.Parse(r_treeid.Match(lines).Result("${treenodetreeid}"));

            Regex r_children = new Regex("\n"+ treeid + "->(?<childtreeid>\\d+)");
            int childtreeid_min, childtreeid_max;
            Match m_children = r_children.Match(lines);
            childtreeid_min = int.Parse(m_children.Result("${childtreeid}"));
            while(m_children.NextMatch().Success)
                m_children = m_children.NextMatch();
            childtreeid_max = int.Parse(m_children.Result("${childtreeid}"));

            for (int childtreeid = childtreeid_min; childtreeid <= childtreeid_max; childtreeid++)
            {
                Regex r_relatedline = new Regex("\n" + childtreeid + "\\[color(?<body>.+)\n");
                Match m_relatedline = r_relatedline.Match(lines);
                string relatedline = m_relatedline.Value;
                int nodeid = int.Parse(r_nodeid.Match(relatedline).Result("${nodeid}"));
                relatedlines.Add(nodeid, relatedline);
            }

            return relatedlines;
        }

        private class DocumentProjection
        {
            public string docLabel0;
            public string docLabel1;

            static Regex r = new Regex(@"\[(?<docindex>\d+)\]");
            static string titlefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;

            public string ToString(IndexSearcher indexsearcher)
            {
                int docIndex0, docIndex1;
                string str;

                docIndex0 = int.Parse(r.Match(docLabel0).Result("${docindex}"));
                docIndex1 = int.Parse(r.Match(docLabel1).Result("${docindex}"));
                str = docIndex0 + "-" + docIndex1 + "\n";
                str += indexsearcher.Doc(docIndex0).Get(titlefield) + "\n";
                str += indexsearcher.Doc(docIndex1).Get(titlefield) + "\n";
                
                return str;
            }
        }

        private static int Height2DocumentNumberFactor = 1000;

        private static void ConvertRoseRiverToTextFlow()
        {
            string inputfolder = @"D:\Project\HTF_CaseStudy\Release\NodeProperitiesForTextFlow_Candidate_Ver4\";
            string outputfolder = @"D:\Project\ERT\TextFlow\Visualization.Toolkit.DAGLayout\Visualization.Toolkit.DAGLayout.Web\";

            List<RoseRiverNodes> nodeprops;
            List<RoseRiverEdges> edgeprops;

            GetRoseRiverProperties(inputfolder, out nodeprops, out edgeprops);

            TextFlowInfo textflowinfo = new TextFlowInfo(nodeprops, edgeprops);

            textflowinfo.Print(outputfolder);
        }

        private static void GetRoseRiverProperties(string inputfolder, out List<RoseRiverNodes> nodeprops, out List<RoseRiverEdges> edgeprops)
        {
            nodeprops = new List<RoseRiverNodes>();
            edgeprops = new List<RoseRiverEdges>();

            int time = 0;
            while (true)
            {
                if (File.Exists(inputfolder + time + ".prop"))
                    nodeprops.Add(new RoseRiverNodes(time, inputfolder + time + ".prop"));
                else
                    break;
                time++;
            }

            time = 1;
            while (true)
            {
                if (File.Exists(inputfolder + time + "edge.prop"))
                    edgeprops.Add(new RoseRiverEdges(time, inputfolder + time + "edge.prop"));
                else
                    break;
                time++;
            }
        }

        class TextFlowInfo
        {
            public int totalClusterNumber;

            public List<TextFlowInfoSingleTime> textflowinfolist;

            public TextFlowInfo(List<RoseRiverNodes> nodeprops, List<RoseRiverEdges> edgeprops)
            {
                textflowinfolist = new List<TextFlowInfoSingleTime>();

                foreach (RoseRiverNodes roserivernodes in nodeprops)
                    textflowinfolist.Add(new TextFlowInfoSingleTime(roserivernodes));

                int time = 0;
                foreach (RoseRiverEdges roseriveredges in edgeprops)
                {
                    textflowinfolist[time].SetOutputFlow(roseriveredges, textflowinfolist[time+1]);
                    textflowinfolist[time + 1].SetInputFlow(roseriveredges, textflowinfolist[time]);
                    time++;
                }

                foreach (TextFlowInfoSingleTime textflowsingletime in textflowinfolist)
                    textflowsingletime.FilterNodes();

                foreach (TextFlowInfoSingleTime textflowsingletime in textflowinfolist)
                    textflowsingletime.PostProcessNodesHeight();

                totalClusterNumber = 0;
                foreach (var textflowinfo in textflowinfolist)
                    totalClusterNumber = Math.Max(totalClusterNumber, textflowinfo.nodes.Count);
            }

            internal void Print(string outputfolder)
            {
                outputfolder += string.Format("ClientBin_{0:MMdd_HHmmss}\\", DateTime.Now);

                PrintClusterDocExtractor(outputfolder + "ClusterDocExtractor_bingnews\\");
                PrintClusterKeywordExtractor(outputfolder + "ClusterKeywordExtractor_bingnews\\");
                PrintClusterProperty(outputfolder + "ClusterProperty_bingnews\\");
                PrintGlobalSplitMerge(outputfolder + "GlobalSplitMerge_bingnews\\");
            }

            internal void PrintClusterDocExtractor(string ofolder)
            {
                Directory.CreateDirectory(ofolder);
                for (int i = 0; i < totalClusterNumber; i++)
                {
                    File.WriteAllLines(ofolder + "cluster_" + i + ".txt", new string[] { i.ToString() });
                }
            }

            internal void PrintClusterKeywordExtractor(string ofolder)
            {
                for (int i = 0; i < totalClusterNumber; i++)
                {
                    string oclusterfolder = ofolder + "cluster_" + i + "\\";
                    Directory.CreateDirectory(oclusterfolder);
                    File.WriteAllLines(oclusterfolder + "cluster_" + i + ".txt", new string[] { i.ToString() });
                }
            }

            internal void PrintClusterProperty(string ofolder)
            {
                Directory.CreateDirectory(ofolder);
                for (int i = 2000; i < 2000 + textflowinfolist.Count; i++)
                {
                    StreamWriter ofile = new StreamWriter(ofolder + i + "_cluster.txt");
                    ofile.WriteLine(i);
                    ofile.WriteLine();

                    foreach (TextFlowNode node in textflowinfolist[i - 2000].nodes)
                    {
                        ofile.WriteLine("{0}\t{1}\t{2}\t{3}",
                            node.color[0], node.color[1], node.color[2], node.color[3]);
                    }

                    ofile.Flush();
                    ofile.Close();
                }
            }

            internal void PrintGlobalSplitMerge(string ofolder)
            {
                Directory.CreateDirectory(ofolder);
                for (int i = 2000; i < 2000 + textflowinfolist.Count; i++)
                {
                    TextFlowInfoSingleTime textflowinfo = textflowinfolist[i - 2000];

                    //_cluster
                    StreamWriter ofile = new StreamWriter(ofolder + i + "_cluster.txt");
                    ofile.WriteLine(i);
                    ofile.WriteLine();
                    ofile.WriteLine("ID	|Size	|	keyword:frequency:tfidf,");
                    foreach (TextFlowNode node in textflowinfo.nodes)
                        ofile.WriteLine("{0}\t|{1}\t|",
                            node.clusterid,
                            (int)Math.Round(node.height * Height2DocumentNumberFactor));
                    ofile.Flush();
                    ofile.Close();

                    //_output
                    if (i != 2000 + textflowinfolist.Count - 1)
                    {
                        ofile = new StreamWriter(ofolder + i + "_output.txt");
                        ofile.WriteLine("Output from " + i);
                        ofile.WriteLine();
                        foreach (TextFlowNode node in textflowinfo.nodes)
                        {
                            var weights = node.outputFlow.nodeWeights.GetEnumerator();
                            weights.MoveNext();
                            foreach (TextFlowNode dstnode in node.outputFlow.nodes)
                            {
                                ofile.WriteLine("From Cluster: {0}, {1}", node.clusterid, weights.Current);
                                ofile.WriteLine("To Cluster: {0}", dstnode.clusterid);
                                ofile.WriteLine("ID	|Size	|	keyword:frequency:tfidf,");
                                ofile.WriteLine("{0}	|0	|", node.clusterid);
                                ofile.WriteLine();

                                weights.MoveNext();
                            }
                        }
                        ofile.Flush();
                        ofile.Close();
                    }

                    //_input
                    if (i != 2000)
                    {
                        ofile = new StreamWriter(ofolder + i + "_input.txt");
                        ofile.WriteLine("Input to " + i);
                        ofile.WriteLine();
                        foreach (TextFlowNode node in textflowinfo.nodes)
                        {
                            var weights = node.inputFlow.nodeWeights.GetEnumerator();
                            weights.MoveNext();
                            foreach (TextFlowNode srcnode in node.inputFlow.nodes)
                            {
                                ofile.WriteLine("To Cluster: {0}, {1}", node.clusterid, weights.Current);
                                ofile.WriteLine("From Cluster: {0}", srcnode.clusterid);
                                ofile.WriteLine("ID	|Size	|	keyword:frequency:tfidf,");
                                ofile.WriteLine("{0}	|0	|", node.clusterid);
                                ofile.WriteLine();

                                weights.MoveNext();
                            }
                        }
                        ofile.Flush();
                        ofile.Close();
                    }
                }
            }
        }

        class TextFlowInfoSingleTime
        {
            public List<TextFlowNode> nodes;

            public Dictionary<int, TextFlowNode> rridToNode;

            public TextFlowInfoSingleTime(RoseRiverNodes roserivernodes)
            {
                nodes = new List<TextFlowNode>();
                rridToNode = new Dictionary<int, TextFlowNode>();
                foreach (RoseRiverNode roserivernode in roserivernodes.nodes)
                {
                    TextFlowNode node = new TextFlowNode(roserivernode);
                    rridToNode.Add(roserivernode.ClusterID, node);
                    nodes.Add(node);
                }
            }

            public void SetOutputFlow(RoseRiverEdges roseriveredges,
                TextFlowInfoSingleTime nextTextFlowInfo)
            {
                foreach (RoseRiverEdge roseriveredge in roseriveredges.edges)
                {
                    rridToNode[roseriveredge.PreClusterID].SetOutputFlow(
                         nextTextFlowInfo.rridToNode[roseriveredge.CurClusterID],
                         roseriveredge.PreHeight
                        );
                }
            }

            public void SetInputFlow(RoseRiverEdges roseriveredges,
                TextFlowInfoSingleTime preTextFlowInfo)
            {
                foreach (RoseRiverEdge roseriveredge in roseriveredges.edges)
                {
                    rridToNode[roseriveredge.CurClusterID].SetInputFlow(
                         preTextFlowInfo.rridToNode[roseriveredge.PreClusterID],
                         roseriveredge.CurHeight
                        );
                }
            }


            internal void PostProcessNodesHeight()
            {
                foreach (TextFlowNode node in nodes)
                    node.PostProcessNodesHeight();
            }

            internal void FilterNodes()
            {
                List<TextFlowNode> nodesnew = new List<TextFlowNode>();
                foreach (TextFlowNode node in nodes)
                {
                    if (!NodeFiltered(node))
                        nodesnew.Add(node);
                    else
                    {
                        node.RemoveFromFlow();
                    }
                }

                nodes.Clear();
                nodes = nodesnew;

                int newid = 0;
                foreach (TextFlowNode node in nodes)
                {
                    node.clusterid = newid;
                    newid++;
                }
            }

            internal bool NodeFiltered(TextFlowNode node)
            {
                return
                    (node.outputFlow.nodes.Count == 0 &&
                    node.inputFlow.nodes.Count == 0) ||
                    (node.outputFlow.GetTotalHeight() + node.inputFlow.GetTotalHeight()) * Height2DocumentNumberFactor < 6000;
            }
        }

        class TextFlowNode
        {
            public int clusterid;
            public double height;
            public int[] color;

            public TextFlowFlow inputFlow = new TextFlowFlow();
            public TextFlowFlow outputFlow = new TextFlowFlow();

            public TextFlowNode(RoseRiverNode roserivernode)
            {
                clusterid = roserivernode.ClusterOrder; //correct!
                color = roserivernode._Visual_FillColor;
            }

            internal void SetOutputFlow(TextFlowNode outputNode, double outputWeight)
            {
                outputFlow.nodes.Add(outputNode);
                outputFlow.nodeWeights.Add(outputWeight);
            }
            internal void SetInputFlow(TextFlowNode inputNode, double inputWeight)
            {
                inputFlow.nodes.Add(inputNode);
                inputFlow.nodeWeights.Add(inputWeight);
            }

            public void PostProcessNodesHeight()
            {
                //calculate height
                double totalheightsum = 0;
                int heightcnt = 0;

                if (inputFlow.nodes.Count != 0)
                {
                    totalheightsum += inputFlow.GetTotalHeight();
                    heightcnt++;
                }

                if (outputFlow.nodes.Count != 0)
                {
                    totalheightsum += outputFlow.GetTotalHeight();
                    heightcnt++;
                }

                if (heightcnt == 0)
                    height = 0;
                else
                    height = totalheightsum / heightcnt;

                //normalize
                inputFlow.NormalizeHeight();
                outputFlow.NormalizeHeight();
            }

            internal void RemoveFromFlow()
            {
                foreach (TextFlowNode srcnode in inputFlow.nodes)
                {
                    int index = srcnode.outputFlow.nodes.IndexOf(this);
                    srcnode.outputFlow.nodes.RemoveAt(index);
                    srcnode.outputFlow.nodeWeights.RemoveAt(index);
                }

                foreach (TextFlowNode dstnode in outputFlow.nodes)
                {
                    int index = dstnode.inputFlow.nodes.IndexOf(this);
                    dstnode.inputFlow.nodes.RemoveAt(index);
                    dstnode.inputFlow.nodeWeights.RemoveAt(index);
                }
            }
        }

        class TextFlowFlow
        {
            public List<TextFlowNode> nodes = new List<TextFlowNode>();
            public List<double> nodeWeights = new List<double>();

            public double GetTotalHeight()
            {
                double height = 0;
                foreach (double nodeWeight in nodeWeights)
                    height += nodeWeight;
                return height;
            }

            public void NormalizeHeight()
            {
                double totalHeight = GetTotalHeight();
                if (totalHeight != 0) 
                    for (int i = 0; i < nodeWeights.Count; i++)
                        nodeWeights[i] /= totalHeight;
            }
        }

        class RoseRiverNodes
        {
            public int time;
            public List<RoseRiverNode> nodes;

            public RoseRiverNodes(int time, string filename)
            {
                this.time = time;
                string[] lines = File.ReadAllLines(filename);
                nodes = new List<RoseRiverNode>();

                foreach (string line in lines)
                    nodes.Add(new RoseRiverNode(line));
            }
        }

        class RoseRiverEdges
        {
            public int time;
            public List<RoseRiverEdge> edges;

            public RoseRiverEdges(int time, string filename)
            {
                this.time = time;
                string[] lines = File.ReadAllLines(filename);
                edges = new List<RoseRiverEdge>();

                foreach (string line in lines)
                    edges.Add(new RoseRiverEdge(line));
            }
        }

        class RoseRiverNode
        {
            public int ClusterID;
            public int ClusterOrder;
            public int[] _Visual_FillColor;

            public RoseRiverNode(string line)
            {
                string[] tokens = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    string propName = token.Split(new char[]{':'}, StringSplitOptions.RemoveEmptyEntries)[0];
                    string propVal = token.Split(new char[]{':'}, StringSplitOptions.RemoveEmptyEntries)[1];
                    switch (propName)
                    {
                        case "ClusterID":
                            ClusterID = int.Parse(propVal);
                            break;
                        case "ClusterOrder":
                            ClusterOrder = int.Parse(propVal);
                            break;
                        case "_Visual_FillColor":
                            _Visual_FillColor = new int[4];
                            for (int i = 0; i < 4; i++)
                                _Visual_FillColor[i] = int.Parse(propVal.Substring(2 * i + 1, 2), NumberStyles.AllowHexSpecifier);
                            break;
                        default:
                            break;
                    }
                }
            } 
        }

        class RoseRiverEdge
        {
            public int PreClusterID;
            public int CurClusterID;
            public double PreHeight;
            public double CurHeight;

            public RoseRiverEdge(string line)
            {
                string[] tokens = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    string propName = token.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string propVal = token.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    switch (propName)
                    {
                        case "PreClusterID":
                            PreClusterID = int.Parse(propVal);
                            break;
                        case "CurClusterID":
                            CurClusterID = int.Parse(propVal);
                            break;
                        case "PreHeight":
                            PreHeight = double.Parse(propVal);
                            break;
                        case "CurHeight":
                            CurHeight = double.Parse(propVal);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static void PrintZeroTreeTimeRatio()
        {
            string folder = @"D:\Study Data\实践\yuvs\mobile_cif\";
            string[] filenames_org = new string[] { "time_img_q3.txt", "time_img_q2.txt", "time_img_q1.txt", "time_img_q0.txt" };
            string[] filenames_eva = new string[] { "time_inter_q3.txt", "time_inter_q2.txt", "time_inter_q1.txt", "time_inter_q0.txt" };

            for (int i = 0; i < 4; i++)
            {
                string[] lines_org = File.ReadAllLines(folder + filenames_org[i]);
                string[] lines_eva = File.ReadAllLines(folder + filenames_eva[i]);

                int time_org_0 = int.Parse(lines_org[0].Split(new char[] { ':' }, StringSplitOptions.None)[1]);
                int time_org_1 = int.Parse(lines_org[1].Split(new char[] { ':' }, StringSplitOptions.None)[1]);
                int time_eva_0 = int.Parse(lines_eva[0].Split(new char[] { ':' }, StringSplitOptions.None)[1]);
                int time_eva_1 = int.Parse(lines_eva[1].Split(new char[] { ':' }, StringSplitOptions.None)[1]);

                Console.WriteLine(((double)time_eva_0 + time_eva_1) / (time_org_0 + time_org_1));
            }
        }

        #region generate input for text flow
        public static int FilterDocumentSize = 0;
        public static bool BSplitToLowestLevel = false;
        public static bool BOutputDebugFile = false;
        public static double ProjectionSimilarityFilterThreshold = -1;
        public static bool RRProjection = true;
        public static bool BSubstitudeOrder = true;

        //public static bool BReorder = true;
        enum OrderType { Natural, RROrder, Shuffle };
        static OrderType ordertype = OrderType.RROrder;

        #region test
        private static void PostProcessPropFile()
        {
            string inputfolder = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\HTF_Final_Data\0323_203650_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            for (int time = 0; time < 28; time++)
            {
                string filename = inputfolder + time + ".prop";
                //string filename = inputfolder + "ClusterNodeData" + time + ".dat";
                string[] lines = File.ReadAllLines(filename);
                StreamWriter ofile = new StreamWriter(filename);
                foreach (string line in lines)
                {
                    string[] tokens = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    tokens[4] = "ClusterCutNodeRows:" + tokens[4];
                    tokens[5] = "FocusedNodes:" + tokens[5];
                    foreach (string token in tokens)
                        ofile.Write(token + "\t");
                    ofile.WriteLine();
                }
                ofile.Flush();
                ofile.Close();
            }
        }

        private static void TestConvertBinaryProjectionFileToText()
        {
            for (int i = 1; i < 25; i++)
            {
                //string filename = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0526_155047_gamma0.42alpha0.003KNN100merge6E-06split6E-06cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\" + i;
                //string filename = @"D:\Project\TextFlowComparison\HTF_Final_Data\0323_203650_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\" + i;
                string filename = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0522_095405_gamma0.42alpha0.003KNN100merge0.01split0.01cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\" + i;
                ConvertBinaryProjectionFileToText(filename + "c_proj_many.bin", filename + "c_proj.dat");
                //ConvertBinaryProjectionFileToText(filename + "_proj_many.bin", filename + "_proj_many.dat");
            }
        }
        #endregion test

        static void GenerateInputForTextFlow()
        {
            string inputfolder = @"D:\Project\TextFlowComparison\HTF_Final_Data\0323_203650_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            string outputfolder = string.Format(@"D:\Project\ERT\TextFlow\Visualization.Toolkit.DAGLayout\Visualization.Toolkit.DAGLayout.Web\ClientBin_{0:MMdd_HHmmss}\", DateTime.Now);
            string substitudeorderfolder = inputfolder + "NodeProperitiesForTextFlow_TO_NH_Filter0_Iter20\\";

            if (BSubstitudeOrder && ordertype != OrderType.RROrder)
                Console.WriteLine("[Warning] The substituded order is not used!");

            int time = 0;
            Clusters prevClusters = null;
            Clusters nowClusters = null;
            Flows nowFlows = null;
            int MaxClusterNumber = int.MinValue;
            while (File.Exists(string.Format("{0}{1}.gv", inputfolder, time)))
            {
                //Cluster
                nowClusters = new Clusters(string.Format("{0}{1}", inputfolder, time), time);
                if (BSubstitudeOrder)
                    nowClusters.SubstitudeOrder(substitudeorderfolder + time);

                //Flow
                if (prevClusters != null)
                {
                    if (RRProjection)
                        nowFlows = new Flows(string.Format("{0}{1}edge.prop", inputfolder, time), true);
                    else
                    {
                        string flowfilename = string.Format("{0}{1}c_proj", inputfolder, time);
                        if (!File.Exists(flowfilename + ".dat"))
                            ConvertBinaryProjectionFileToText(flowfilename + "_single.bin", flowfilename + ".dat");
                        nowFlows = new Flows(flowfilename + ".dat", false);
                    }
                    nowFlows.InitializeClusterFlows(prevClusters, nowClusters);
                    prevClusters.PrintOutputFile(outputfolder);
                    nowClusters.PrintInputFile(outputfolder);
                    if (BOutputDebugFile)
                    {
                        prevClusters.PrintOutputDebugFile(outputfolder);
                        nowClusters.PrintInputDebugFile(outputfolder);
                    }
                }

                nowClusters.PrintClusterData(outputfolder);
                MaxClusterNumber = Math.Max(MaxClusterNumber, nowClusters.ClusterCount);
                
                prevClusters = nowClusters;
                time++;
            }

            PrintGenTextFlowClusterFiles(outputfolder, MaxClusterNumber);
        }

        class Clusters
        {
            int timestamp;
            Cluster[] ClusterArray;  //ClusterArray[Cluster.assignedid]=Cluster;            
            public int ClusterCount { get { return ClusterArray.Length; } }
            string[] seperator = new string[] { "->" };

            public Dictionary<int, int> OrgIdToAssignedId { get; protected set; }
            Dictionary<int, int> treeid2orgidDic;   //treeid==cutrow
            Dictionary<int, List<int>> treerelation; //tree structure recorded by treeid
            Dictionary<int, int> row2treeid;
            Dictionary<int, Cluster> rrclusterID2cluster = new Dictionary<int, Cluster>();

            bool bPropertyReset = false;

            public Clusters(string filename, int timestamp)
            {
                this.timestamp = 2000 + timestamp;

                Initialize(filename);

                if (File.Exists(filename + ".prop"))
                    ResetClusterPropertyFromFile(filename);

                Console.WriteLine(ClusterArray.Length);
            }

            public void Initialize(string filename)
            {
                StreamReader ifile = new StreamReader(filename + ".gv");

                string line;
                treerelation = new Dictionary<int, List<int>>();
                treeid2orgidDic = new Dictionary<int, int>();
                while ((line = ifile.ReadLine()) != null)
                {
                    if (line.Contains("->"))
                    {
                        string[] tokens = line.Split(seperator, 2, StringSplitOptions.RemoveEmptyEntries);
                        int parent = int.Parse(tokens[0]);
                        int child = int.Parse(tokens[1]);
                        if (!treerelation.ContainsKey(parent))
                            treerelation.Add(parent, new List<int>());
                        treerelation[parent].Add(child);
                    }
                    else if (line.Contains("[color"))
                    {
                        int treeid, orgid;
                        GetTreeIdOrgIdRelation(line, out treeid, out orgid);
                        treeid2orgidDic.Add(treeid, orgid);
                    }
                }

                int assignedId = 0;
                List<Cluster> ClusterList = new List<Cluster>();
                OrgIdToAssignedId = new Dictionary<int, int>();
                int totaldoccnt = 0;
                foreach (KeyValuePair<int, List<int>> kvp in treerelation)
                {
                    //bool bsat = true;
                    int directdocumentcnt = 0;
                    foreach (int child in kvp.Value)
                        if (!treerelation.ContainsKey(child))
                        {
                            //bsat = false;
                            //break;
                            directdocumentcnt++;
                        }
                    if (directdocumentcnt != 0 && directdocumentcnt != kvp.Value.Count)
                        throw new Exception("A topic is has two kinds of children!");
                    if (directdocumentcnt != 0)
                    {
                        ClusterList.Add(new Cluster(treeid2orgidDic[kvp.Key], assignedId, directdocumentcnt));
                        OrgIdToAssignedId.Add(treeid2orgidDic[kvp.Key], assignedId);
                        assignedId++;
                        totaldoccnt += directdocumentcnt;
                    }
                }

                ClusterArray = ClusterList.ToArray<Cluster>();
                //Console.WriteLine("totaldoccnt = {0}", totaldoccnt);
                //foreach (Cluster cluster in ClusterArray)
                //    Console.Write(cluster.OrgId + "\t");
                //Console.WriteLine();

                ifile.Close();

            }

            public void ResetClusterPropertyFromFile(string filename)
            {
                SetRow2TreeId(filename);

                string[] lines = File.ReadAllLines(filename + ".prop");
                //int truetimestamp = timestamp - 2000;
                //string[] lines = null;
                //if (truetimestamp >= 10)
                //    lines = File.ReadAllLines(filename.Substring(0, filename.Length - 2) + "ClusterNodeData" + truetimestamp + ".dat");
                //else
                //    lines = File.ReadAllLines(filename.Substring(0, filename.Length - 1) + "ClusterNodeData" + truetimestamp + ".dat");

                int cntsum = 0;
                foreach (string line in lines)
                {
                    CutRow cutrow = new CutRow(line);
                    List<int> childtreeids = GetChildTreeIds(cutrow.Rows);
                    cntsum += childtreeids.Count;
                    foreach (int treeid in childtreeids)
                        ResetClusterProperty(treeid, cutrow);
                }

                //if (cntsum != ClusterArray.Length)
                //    throw new Exception("Error! cntsum != ClusterArray.Length");

                switch(ordertype)
                {
                    case OrderType.RROrder:
                        SortByOrder();
                        break;
                    case OrderType.Shuffle:
                        Shuffle();
                        break;
                }
                bPropertyReset = true;
            }

            public void SubstitudeOrder(string filename)
            {
                string[] lines = File.ReadAllLines(filename + ".prop");
                foreach (var line in lines)
                {
                    CutRow cutrow = new CutRow(line);
                    List<int> childtreeids = GetChildTreeIds(cutrow.Rows);
                    foreach (int treeid in childtreeids)
                        ResetClusterOrder(treeid, cutrow.Order);
                }

                switch (ordertype)
                {
                    case OrderType.RROrder:
                        SortByOrder();
                        break;
                    case OrderType.Shuffle:
                        Shuffle();
                        break;
                }
            }

            private void SetRow2TreeId(string filename)
            {
                StreamReader ifile = new StreamReader(filename + "_i.gv");

                string line;
                int row = 0;
                row2treeid = new Dictionary<int, int>();
                while ((line = ifile.ReadLine()) != null)
                {
                    if (line.Contains("[color"))
                    {
                        int treeid, orgid;
                        GetTreeIdOrgIdRelation(line, out treeid, out orgid);
                        row2treeid.Add(row++, treeid);
                    }
                }

                ifile.Close();
            }

            private void SortByOrder()
            {
                List<Cluster> clusterlist = new List<Cluster>(ClusterArray);
                clusterlist.Sort();
                ClusterArray = clusterlist.ToArray<Cluster>();
                int newassignedid = 0;
                foreach(Cluster cluster in ClusterArray)
                {
                    cluster.AssignedId = newassignedid;
                    OrgIdToAssignedId[cluster.OrgId] = newassignedid;
                    newassignedid++;
                    //Console.Write("{0}({1})\t", cluster.Order, cluster.OrgId);
                }
                //Console.WriteLine();
            }

            private void Shuffle()
            {
                Random random = new Random(DateTime.Now.Millisecond);
                foreach (Cluster cluster in ClusterArray)
                    cluster.Order = random.Next();
                SortByOrder();
            }

            private void ResetClusterOrder(int treeid, int order)
            {
                int orgid = treeid2orgidDic[treeid];
                Cluster cluster = ClusterArray[OrgIdToAssignedId[orgid]];
                cluster.Order = order;
            }

            private void ResetClusterProperty(int treeid, CutRow cutrow)
            {
                int orgid = treeid2orgidDic[treeid];
                Cluster cluster = ClusterArray[OrgIdToAssignedId[orgid]];
                cluster.Color = cutrow.Color;
                cluster.Order = cutrow.Order;
                cluster.DocumentNumber = cluster.DocumentNumber * cutrow.Scale;
                cluster.Scale = cutrow.Scale;
                cluster.RRClusterID = cutrow.RRClusterID;
                cluster.LeftHeight = cutrow.LeftHeight;
                cluster.RightHeight = cutrow.RightHeight;
                rrclusterID2cluster.Add(cluster.RRClusterID, cluster);

                if (cluster.DocumentNumber <= FilterDocumentSize)
                {
                    cluster.bVisualize = false;
                    cluster.Order = int.MaxValue;
                }
                //cluster.AssignedId = -1;
                //OrgIdToAssignedId[orgid] = -1;
            }

            private List<int> GetChildTreeIds(List<int> parentcutrows)
            {
                List<int> childtreeids = new List<int>();
                foreach (int parenttcutrow in parentcutrows)
                    childtreeids.AddRange(GetChildTreeIds(parenttcutrow));
                return childtreeids;
            }

            private List<int> GetChildTreeIds(int parenttcutrow)
            {
                List<int> leaves = new List<int>();
                List<int> queue = new List<int>();
                queue.Add(row2treeid[parenttcutrow]);
                while (queue.Count != 0)
                {
                    int node = queue[0];
                    queue.RemoveAt(0);

                    List<int> nodechildren = treerelation[node];
                    // first child is not a leaf
                    if(treerelation.ContainsKey(nodechildren[0]))
                        queue.AddRange(nodechildren);
                    else
                        leaves.Add(node);
                }

                return leaves;
            }

            class CutRow
            {
                public CutRow(string str)
                {
                    string[] tokens = str.Split('\t');
                    foreach (string token in tokens)
                    {
                        if (token.StartsWith("ClusterOrder:"))
                        {
                            Order = int.Parse(token.Substring(token.IndexOf(':') + 1));
                        }
                        else if (token.StartsWith("ClusterCutNodeRows:"))
                        {
                            int index0 = token.IndexOf('[');
                            int index1 = token.IndexOf(']');
                            string[] rowstrs = token.Substring(index0 + 1, index1 - index0 - 1)
                                .Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries);
                            Rows = new List<int>();
                            foreach (string rowstr in rowstrs)
                                Rows.Add(int.Parse(rowstr));
                        }
                        else if (token.StartsWith("_Visual_FillColor:"))
                        {
                            int index0 = token.IndexOf('#');
                            string colorstr = token.Substring(index0 + 1, 8);
                            Color = new int[4];
                            for (int i = 0; i < 4; i++)
                                Color[i] = int.Parse(colorstr.Substring(2 * i, 2), NumberStyles.AllowHexSpecifier);
                        }
                        else if (token.StartsWith("Scale:"))
                        {
                            Scale = double.Parse(token.Substring(token.IndexOf(':') + 1));
                        }
                        else if (token.StartsWith("ClusterID:"))
                        {
                            RRClusterID = int.Parse(token.Substring(token.IndexOf(':') + 1));
                        }
                        else if (token.StartsWith("LeftHeight:"))
                        {
                            LeftHeight = double.Parse(token.Substring(token.IndexOf(':') + 1));
                        }
                        else if (token.StartsWith("RightHeight:"))
                        {
                            RightHeight = double.Parse(token.Substring(token.IndexOf(':') + 1));
                        }
                    }
                }
                public List<int> Rows { get; protected set; }
                public int Order { get; protected set; }
                public int[] Color { get; protected set; }
                public double Scale { get; protected set; }
                public int RRClusterID { get; protected set; }
                public double LeftHeight { get; protected set; }
                public double RightHeight { get; protected set; }
            }


            public void PrintInputFile(string folder)
            {
                string dir = string.Format(folder + "GlobalSplitMerge_bingnews\\");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string ofilename = string.Format("{0}{1}_input.txt", dir, timestamp);
                StreamWriter ofile = new StreamWriter(ofilename);

                ofile.WriteLine("Input to {0}\n", timestamp);

                foreach (Cluster cluster in ClusterArray)
                {
                    int endid = cluster.AssignedId;
                    if (!cluster.bVisualize)
                        continue;

                    foreach (var kvp in cluster.input)
                    {
                        int startid = kvp.Key.AssignedId;
                        double ratio = kvp.Value / cluster.inputdocumentcntsum;

                        PrintInputFileUnit(ofile, startid, endid, ratio);
                    }
                }

                ofile.Flush();
                ofile.Close();
            }

            public void PrintOutputFile(string folder)
            {
                string dir = string.Format(folder + "GlobalSplitMerge_bingnews\\");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string ofilename = string.Format("{0}{1}_output.txt", dir, timestamp);
                StreamWriter ofile = new StreamWriter(ofilename);

                ofile.WriteLine("Output from {0}\n", timestamp);

                foreach (Cluster cluster in ClusterArray)
                {
                    int startid = cluster.AssignedId;
                    if (!cluster.bVisualize)
                        continue;

                    foreach (var kvp in cluster.output)
                    {
                        int endid = kvp.Key.AssignedId;
                        double ratio = kvp.Value / cluster.outputdocumentcntsum;

                        PrintOutputFileUnit(ofile, startid, endid, ratio);
                    }

                }

                ofile.Flush();
                ofile.Close();
            }

            public void PrintInputDebugFile(string folder)
            {
                string dir = string.Format(folder + "GlobalSplitMerge_bingnews\\");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string ofilename = string.Format("{0}{1}_input_debug.txt", dir, timestamp);
                StreamWriter ofile = new StreamWriter(ofilename);

                foreach (Cluster cluster in ClusterArray)
                {
                    int endid = cluster.AssignedId;
                    if (!cluster.bVisualize)
                        continue;

                    ofile.Write("{0}\t{1}\t{2}\t{3}\t", cluster.Color[0], cluster.Color[1], cluster.Color[2], cluster.Color[3]); //argb
                    ofile.Write("{0}\t{1}\t{2}\t{3}\n", 
                        cluster.inputdocumentcntsumweighted / cluster.inputdocumentcntsum,
                        cluster.inputdocumentcntsum, 
                        cluster.DocumentNumber / cluster.Scale, 
                        cluster.inputdocumentcntsum / cluster.DocumentNumber * cluster.Scale);
                }

                ofile.Flush();
                ofile.Close();
            }

            public void PrintOutputDebugFile(string folder)
            {
                string dir = string.Format(folder + "GlobalSplitMerge_bingnews\\");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string ofilename = string.Format("{0}{1}_output_debug.txt", dir, timestamp);
                StreamWriter ofile = new StreamWriter(ofilename);

                foreach (Cluster cluster in ClusterArray)
                {
                    int startid = cluster.AssignedId;
                    if (!cluster.bVisualize)
                        continue;

                    ofile.Write("{0}\t{1}\t{2}\t{3}\t", cluster.Color[0], cluster.Color[1], cluster.Color[2], cluster.Color[3]); //argb
                    ofile.Write("{0}\t{1}\t{2}\t{3}\n",
                        cluster.outputdocumentcntsumweighted / cluster.outputdocumentcntsum,
                        cluster.outputdocumentcntsum, 
                        cluster.DocumentNumber / cluster.Scale,
                        cluster.outputdocumentcntsum / cluster.DocumentNumber * cluster.Scale);

                }

                ofile.Flush();
                ofile.Close();
            }

            public void PrintClusterData(string folder)
            {
                string dir = string.Format(folder + "GlobalSplitMerge_bingnews\\");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string ofilename = string.Format("{0}{1}_cluster.txt", dir, timestamp);
                StreamWriter ofile = new StreamWriter(ofilename);

                ofile.WriteLine("{0}\n", timestamp);
                ofile.WriteLine("ID	|Size	|	keyword:frequency:tfidf,");

                foreach (Cluster cluster in ClusterArray)
                {
                    if (RRProjection)
                    {
                        double heightsum = 0;
                        int heightcnt = 0;
                        if (cluster.LeftHeight != 0)
                        {
                            heightsum += cluster.LeftHeight;
                            heightcnt++;
                        }
                        if (cluster.RightHeight != 0)
                        {
                            heightsum += cluster.RightHeight;
                            heightcnt++;
                        }
                        if (heightcnt != 0)
                            cluster.DocumentNumber = heightsum / heightcnt;
                        else
                            cluster.DocumentNumber = 5;
                    }
                    if (cluster.bVisualize)
                        ofile.WriteLine("{0}	|{1}	|", cluster.AssignedId, (int)Math.Round(100 * cluster.DocumentNumber));
                }

                ofile.Flush();
                ofile.Close();

                if (bPropertyReset)
                    PrintClusterColor(folder);
            }

            public void PrintClusterColor(string folder)
            {
                string dir = string.Format(folder + "ClusterProperty_bingnews\\");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                string ofilename = string.Format("{0}{1}_cluster.txt", dir, timestamp);
                StreamWriter ofile = new StreamWriter(ofilename);

                ofile.WriteLine("{0}\n", timestamp);

                foreach (Cluster cluster in ClusterArray)
                    ofile.WriteLine("{0}\t{1}\t{2}\t{3}", cluster.Color[0], cluster.Color[1], cluster.Color[2], cluster.Color[3]); //argb

                ofile.Flush();
                ofile.Close();

            }

            void PrintInputFileUnit(StreamWriter ofile, int startid, int endid, double ratio)
            {
                ofile.WriteLine("To Cluster: {0}, {1}", endid, ratio);
                ofile.WriteLine("From Cluster: {0}", startid);
                ofile.WriteLine("ID	|Size	|	keyword:frequency:tfidf,");
                ofile.WriteLine("{0}	|0	|", startid);
                ofile.WriteLine();
            }

            void PrintOutputFileUnit(StreamWriter ofile, int startid, int endid, double ratio)
            {
                ofile.WriteLine("From Cluster: {0}, {1}", startid, ratio);
                ofile.WriteLine("To Cluster: {0}", endid);
                ofile.WriteLine("ID	|Size	|	keyword:frequency:tfidf,");
                ofile.WriteLine("{0}	|0	|", startid);
                ofile.WriteLine();
            }

            private void GetTreeIdOrgIdRelation(string line, out int treeid, out int orgid)
            {
                int index0 = line.IndexOf('[');
                int index1 = line.IndexOf('-', index0 + 1);
                int index2 = line.IndexOf('-', index1 + 1);
                treeid = int.Parse(line.Substring(0, index0));
                orgid = int.Parse(line.Substring(index1 + 1, index2 - index1 - 1));
            }

            public Cluster GetClusterByOrgId(int orgid)
            {
                if(OrgIdToAssignedId.ContainsKey(orgid))
                    return ClusterArray[OrgIdToAssignedId[orgid]];
                return null;
            }

            internal Cluster GetClusterByRRClusterID(int rrclusterid)
            {
                return rrclusterID2cluster[rrclusterid];
            }

            public IList<Cluster> GetClusters()
            {
                return ClusterArray.ToList<Cluster>().AsReadOnly();
            }
        }

        class Cluster : IComparable
        {
            public Cluster(int OrgId, int AssignedId, int DocumentNumber)
            {
                this.OrgId = OrgId;
                this.AssignedId = AssignedId;
                this.DocumentNumber = DocumentNumber;
            }

            public int OrgId;
            public int AssignedId;
            public double DocumentNumber;
            public double Scale;
            public int RRClusterID;
            public double LeftHeight;
            public double RightHeight;
            public Dictionary<Cluster, double> input = new Dictionary<Cluster,double>();
            public Dictionary<Cluster, double> output = new Dictionary<Cluster,double>();
            public double inputdocumentcntsum = 0;
            public double outputdocumentcntsum = 0;
            public double inputdocumentcntsumweighted = 0;
            public double outputdocumentcntsumweighted = 0;
            public int[] Color = null;
            public int Order;
            public bool bVisualize = true;

            int IComparable.CompareTo(object obj)
            {
                Cluster cluster = (Cluster)obj;
                int r = this.Order.CompareTo(cluster.Order);
                if (r != 0)
                    return r;
                return this.OrgId.CompareTo(cluster.OrgId);
            }
        }

        class Flows
        {
            public Dictionary<int, Dictionary<int, double>> flows;
            public Dictionary<int, Dictionary<int, double>> flowsweighted;
            public Dictionary<int, Dictionary<int, KeyValuePair<double, double>>> flowsDoubleSide;

            public bool bRRProj;

            public Flows(string filename, bool bRRProj)
            {
                this.bRRProj = bRRProj;

                if (bRRProj)
                    Initialize_RoseRiver(filename);
                else
                    Initialize_TextFlow(filename);
            }

            void Initialize_TextFlow(string filename)
            {
                StreamReader ifile = new StreamReader(filename);
                flows = new Dictionary<int, Dictionary<int, double>>();
                flowsweighted = new Dictionary<int, Dictionary<int, double>>();

                string line;
                ifile.ReadLine();//doccnt
                char[] separator = new char[] { '\t' };
                while ((line = ifile.ReadLine()) != null)
                {
                    string[] tokens = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length == 0)
                        break;
                    int endid, startid;
                    double cosine;
                    endid = GetClusterId(tokens[0]);
                    if (tokens.Length == 3)
                    {
                        startid = GetClusterId(tokens[1]);
                        cosine = double.Parse(tokens[2]);
                    }
                    else if (tokens.Length == 4)
                    {
                        startid = GetClusterId(tokens[2]);
                        cosine = double.Parse(tokens[3]);
                    }
                    else
                        throw new Exception("error file format!");

                    if (cosine <= ProjectionSimilarityFilterThreshold)
                        continue;

                    if (!flows.ContainsKey(startid))
                        flows.Add(startid, new Dictionary<int, double>());
                    if (!flows[startid].ContainsKey(endid))
                        flows[startid].Add(endid, 0);
                    flows[startid][endid]++;

                    if (!flowsweighted.ContainsKey(startid))
                        flowsweighted.Add(startid, new Dictionary<int, double>());
                    if (!flowsweighted[startid].ContainsKey(endid))
                        flowsweighted[startid].Add(endid, 0);
                    flowsweighted[startid][endid] += cosine;
                }

                //PrintFlows();
                ifile.Close();
            }

            void Initialize_RoseRiver(string filename)
            {
                StreamReader ifile = new StreamReader(filename);
                flowsDoubleSide = new Dictionary<int,Dictionary<int,KeyValuePair<double,double>>>();
                
                string line;
                char[] separator = new char[] { '\t' };
                int startid, endid;
                double preHeight, curHeight;
                while ((line = ifile.ReadLine()) != null)
                {
                    string[] tokens = line.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                    startid = int.Parse(tokens[0].Substring(tokens[0].IndexOf(':') + 1));
                    endid = int.Parse(tokens[1].Substring(tokens[1].IndexOf(':') + 1));
                    preHeight = double.Parse(tokens[2].Substring(tokens[2].IndexOf(':') + 1));
                    curHeight = double.Parse(tokens[3].Substring(tokens[3].IndexOf(':') + 1));

                    if (!flowsDoubleSide.ContainsKey(startid))
                        flowsDoubleSide.Add(startid, new Dictionary<int, KeyValuePair<double,double>>());
                    if (!flowsDoubleSide[startid].ContainsKey(endid))
                        flowsDoubleSide[startid].Add(endid, new KeyValuePair<double,double>(preHeight,curHeight));
                }

                //PrintFlows();
                ifile.Close();
            }

            int GetClusterId(string token)
            {
                int index = token.IndexOf('(');
                if (index > 0)
                    return int.Parse(token.Substring(index + 1, token.Length - index - 2));
                else
                    return int.Parse(token);
            }

            public void InitializeClusterFlows(Clusters clusters0, Clusters clusters1)
            {
                if(bRRProj)
                    InitializeClusterFlows_RoseRiver(clusters0, clusters1);
                else
                    InitializeClusterFlows_TextFlow(clusters0, clusters1);
            }

            void InitializeClusterFlows_TextFlow(Clusters clusters0, Clusters clusters1)
            {
                double totalflowdocs = 0;
                double loseflowdocs = 0;
                foreach (var kvp0 in flows)
                {
                    int startorgid = kvp0.Key;
                    foreach (var kvp1 in kvp0.Value)
                    {
                        int endorgid = kvp1.Key;
                        double documentcounts = kvp1.Value;
                        totalflowdocs += documentcounts;

                        Cluster startcluster = clusters0.GetClusterByOrgId(startorgid);
                        Cluster endcluster = clusters1.GetClusterByOrgId(endorgid);
                        if (startcluster == null || endcluster == null)
                        {
                            loseflowdocs += documentcounts;
                            continue;
                        }

                        startcluster.output.Add(endcluster, documentcounts);
                        endcluster.input.Add(startcluster, documentcounts);
                        startcluster.outputdocumentcntsum += documentcounts;
                        endcluster.inputdocumentcntsum += documentcounts;
                    }
                }

                //weighted
                foreach (var kvp0 in flowsweighted)
                {
                    int startorgid = kvp0.Key;
                    foreach (var kvp1 in kvp0.Value)
                    {
                        int endorgid = kvp1.Key;
                        double documentcounts = kvp1.Value;
                        totalflowdocs += documentcounts;

                        Cluster startcluster = clusters0.GetClusterByOrgId(startorgid);
                        Cluster endcluster = clusters1.GetClusterByOrgId(endorgid);
                        if (startcluster == null || endcluster == null)
                        {
                            loseflowdocs += documentcounts;
                            continue;
                        }

                        //startcluster.output.Add(endcluster, documentcounts);
                        //endcluster.input.Add(startcluster, documentcounts);
                        startcluster.outputdocumentcntsumweighted += documentcounts;
                        endcluster.inputdocumentcntsumweighted += documentcounts;
                    }
                }

                //Console.WriteLine("Lose flow docs: {0}% ({1} out of {2})", (int)(100 * loseflowdocs / totalflowdocs), loseflowdocs, totalflowdocs);

            }

            void InitializeClusterFlows_RoseRiver(Clusters clusters0, Clusters clusters1)
            {
                foreach (var kvp0 in flowsDoubleSide)
                {
                    int startRRClusterID = kvp0.Key;
                    Cluster startCluster = clusters0.GetClusterByRRClusterID(startRRClusterID);

                    foreach (var kvp1 in kvp0.Value)
                    {
                        int endRRClusterID = kvp1.Key;
                        Cluster endCluster = clusters1.GetClusterByRRClusterID(endRRClusterID);

                        double prevHeight = kvp1.Value.Key;
                        double curHeight = kvp1.Value.Value;

                        startCluster.output.Add(endCluster, prevHeight);
                        endCluster.input.Add(startCluster, curHeight);
                    }
                }

                foreach (var cluster in clusters0.GetClusters())
                    cluster.outputdocumentcntsum = cluster.RightHeight;

                foreach (var cluster in clusters1.GetClusters())
                    cluster.inputdocumentcntsum = cluster.LeftHeight;
            }

            private void PrintFlows()
            {
                Console.WriteLine("------Flows----");
                foreach (var kvp0 in flows)
                {
                    int startId = kvp0.Key;
                    Console.Write("[{0}]\t", startId);
                    foreach (var kvp1 in flows[startId])
                    {
                        int endId = kvp1.Key;
                        double simi = kvp1.Value;
                        Console.Write("{0},{1}\t", endId, simi);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine("--------------");
            }

        }

        static void PrintGenTextFlowClusterFiles(string folder, int maxClusterNumber)
        {
            string dir = folder + "ClusterDocExtractor_bingnews\\";
            Directory.CreateDirectory(dir);
            for (int i = 0; i < maxClusterNumber; i++)
                File.AppendAllText(dir + "cluster_" + i + ".txt", i + "");

            dir = folder + "ClusterKeywordExtractor_bingnews\\";
            Directory.CreateDirectory(dir);
            for (int i = 0; i < maxClusterNumber; i++)
            {
                string subdir = dir + "cluster_" + i + "\\";
                Directory.CreateDirectory(subdir);
                File.AppendAllText(subdir + "cluster_" + i + ".txt", i + "");
            }
        }


        static void ConvertBinaryProjectionFileToText(string binaryfilename, string outputfilename)
        {
            BinaryReader br = new BinaryReader(File.Open(binaryfilename, FileMode.Open));
            StreamWriter ofile = new StreamWriter(outputfilename);

            long pos = 0;
            long length = br.BaseStream.Length;
            while (pos < length)
            {
                int triplenumber = br.ReadInt32();
                int nodeId = br.ReadInt32();
                int nodeparentid = br.ReadInt32();
                pos += 3 * sizeof(int);
                ofile.Write("{0}({1})\t", nodeId, nodeparentid);

                int projNodeId, projParentNodeId;
                float similarity;
                bool bwrite = true;
                for (int i = 0; i < triplenumber; i++)
                {
                    projNodeId = br.ReadInt32();
                    projParentNodeId = br.ReadInt32();
                    similarity = br.ReadSingle();

                    if (bwrite)
                    {
                        if (projNodeId >= 0)
                        {
                            ofile.Write("{0}({1})\t{2}\t", projNodeId, projParentNodeId, similarity);
                            bwrite = false;
                        }
                        else if (projNodeId == -1)
                        {
                            ofile.Write("{0}\t{1}\t", projParentNodeId, similarity);
                            bwrite = false;
                        }
                        else if (projNodeId == -2)
                        {
                            ofile.Write("{0}({1})\t", projNodeId, projParentNodeId);
                        }
                    }

                    pos += 2 * sizeof(int) + sizeof(float);
                }

                ofile.WriteLine();
            }

            br.Close();
            ofile.Flush();
            ofile.Close();
        }


        #endregion generate input for text flow



       
        private static void DataStatistics()
        {
            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\start[2012-1-8]span7slot28sample10000\0323_203650_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\FineResults\0322_101318_gamma0.36alpha0.004KNN100merge1E-06split1E-06cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_Cl15_520\";
            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0526_155047_gamma0.42alpha0.003KNN100merge6E-06split6E-06cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0522_095405_gamma0.42alpha0.003KNN100merge0.01split0.01cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            
            int time = 0;

            double depthsum = 0, depthsumsquare = 0;
            double l1cntsum = 0, l1cntsumsquare = 0;
            double cntsum = 0, cntsumsquare = 0;

            int mindepth = int.MaxValue, maxdepth = int.MinValue;
            int minl1cnt = int.MaxValue, maxl1cnt = int.MinValue;
            int mincnt = int.MaxValue, maxcnt = int.MinValue;

            while (true)
            {
                string filename = inputpath + time + "_i.gv";
                if (!File.Exists(filename))
                    break;

                StreamReader sr = new StreamReader(filename);
                string line;
                Dictionary<int, List<int>> tree = new Dictionary<int, List<int>>();
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("->"))
                    {
                        string[] tokens = line.Split('-');
                        int head = int.Parse(tokens[0]);
                        int tail = int.Parse(tokens[1].Substring(1));

                        if (!tree.ContainsKey(head))
                            tree.Add(head, new List<int>());
                        tree[head].Add(tail);
                    }
                }

                //BFS
                int treedepth = 0;
                int level1NodeNumber = 0;
                int nodeNumber = 0;

                List<int> queue = new List<int>();
                queue.Add(0);
                queue.Add(-1);

                while (queue.Count != 0)
                {
                    int node = queue[0];
                    queue.RemoveAt(0);

                    if (node == -1)
                    {
                        treedepth++;
                        if (queue.Count != 0)
                            queue.Add(-1);
                        continue;
                    }

                    if (tree.ContainsKey(node))
                    {
                        queue.AddRange(tree[node]);
                    }
                    nodeNumber++;
                }

                level1NodeNumber = tree[0].Count;

                Console.WriteLine("time {0}: depth {1} L1Cnt {2} TotalCnt {3}", time, treedepth, level1NodeNumber, nodeNumber);

                depthsum += treedepth;
                depthsumsquare += treedepth * treedepth;
                l1cntsum += level1NodeNumber;
                l1cntsumsquare += level1NodeNumber * level1NodeNumber;
                cntsum += nodeNumber;
                cntsumsquare += nodeNumber * nodeNumber;

                if (mindepth > treedepth) mindepth = treedepth;
                if (minl1cnt > level1NodeNumber) minl1cnt = level1NodeNumber;
                if (mincnt > nodeNumber) mincnt = nodeNumber;
                if (maxdepth < treedepth) maxdepth = treedepth;
                if (maxl1cnt < level1NodeNumber) maxl1cnt = level1NodeNumber;
                if (maxcnt < nodeNumber) maxcnt = nodeNumber;

                time++;

                sr.Close();
            }

            Console.WriteLine("Tree Depth: {0}+-{1} [{2}, {3}]", depthsum / time, Math.Sqrt(depthsumsquare / time - depthsum * depthsum / time / time), mindepth, maxdepth);
            Console.WriteLine("Level1 Topics: {0}+-{1} [{2}, {3}]", l1cntsum / time, Math.Sqrt(l1cntsumsquare / time - l1cntsum * l1cntsum / time / time), minl1cnt, maxl1cnt);
            Console.WriteLine("Total Topics: {0}+-{1} [{2}, {3}]", cntsum / time, Math.Sqrt(cntsumsquare / time - cntsum * cntsum / time / time), mincnt, maxcnt);

        }

        public class Statistics
        {
            double min = double.MaxValue;
            double max = double.MinValue;
            double sum = 0;
            double sumsquare = 0;
            int cnt = 0;

            public void ObserveNumber(double num)
            {
                if (num > max)
                    max = num;
                if (num < min)
                    min = num;
                sum += num;
                sumsquare += num * num;
                cnt++;
            }

            public double GetMin()
            {
                return min;
            }

            public double GetMax()
            {
                return max;
            }

            public double GetAverage()
            {
                return sum / cnt;
            }

            public double GetStd()
            {
                return Math.Sqrt(sumsquare / cnt - sum * sum / cnt / cnt);
            }

            public double GetSum()
            {
                return sum;
            }

            public string PrintResult(string name = null)
            {
                string str = "";
                str += String.Format("------------" + name + " Statistics" + "------------\n");
                str += String.Format("Min:{0}\n", GetMin());
                str += String.Format("Max:{0}\n", GetMax());
                str += String.Format("Avg:{0}\n", GetAverage());
                str += String.Format("Std:{0}\n", GetStd());
                //str += String.Format("Cnt:{0}\n", cnt);

                Console.WriteLine(str);
                return str;
            }
        }

        private static void CountLeafTopicNumber()
        {

            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\FineResults\0322_101318_gamma0.36alpha0.004KNN100merge1E-06split1E-06cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_Cl15_520\";
            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\start[2012-1-8]span7slot2sample100\0311_182538_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\start[2012-1-8]span7slot28sample10000\0323_203650_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\start[2012-1-8]span7slot28sample10000\0323_203650_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";

            int time = 0;
            int leaftopicsum = 0;
            while (true)
            {
                string filename = inputpath + time + "_i.gv";
                if (!File.Exists(filename))
                    break;

                StreamReader sr = new StreamReader(filename);
                string line;
                HashSet<int> head = new HashSet<int>();
                HashSet<int> tail = new HashSet<int>();
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("->"))
                    {
                        string[] tokens = line.Split('-');
                        head.Add(int.Parse(tokens[0]));
                        tail.Add(int.Parse(tokens[1].Substring(1)));
                    }
                }


                foreach (int headitem in head)
                    tail.Remove(headitem);

                int leaftopiccnt = tail.Count;
                Console.WriteLine("time {0}: {1}", time, leaftopiccnt);
                leaftopicsum += leaftopiccnt;

                time++;

                sr.Close();
            }

            Console.WriteLine("Total leaf topic cnt:" + leaftopicsum);
        }

        private static void GetTopicNumber()
        {
            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected_0202\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D_Modified\";
            string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\start[2012-1-8]span7slot28sample10000\0323_203650_gamma0.12alpha0.01KNN100merge0.0001split0.0001cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            int start = 0;
            int end = 9;

            int[] topiccnts = new int[end - start + 1];
            for (int i = start; i <= end; i++)
            {
                StreamReader sr = new StreamReader(inputpath + i + "_i.gv");
                int linecnt = 0;
                while (sr.ReadLine() != null)
                    linecnt++;
                topiccnts[i] = linecnt / 2 - 2;
            }

            //int topicsum = 0, topicsquaresum = 0;
            foreach (int topiccnt in topiccnts)
            {
                //topicsum += topiccnt;
                //topicsquaresum += topiccnt * topiccnt;
                Console.Write(topiccnt + "\t");
            }


        }

        private static void RemoveResultToTheRoot()
        {
            string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D\";
            string outpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D_RemoveRootEdge\";
            int start = 1;
            int end = 9;
            string nodeEndStr = "\", shape=\"record\"];";

            Directory.CreateDirectory(outpath);
            DirectoryInfo info = new DirectoryInfo(inputpath);
            FileInfo[] files = info.GetFiles();
            foreach (FileInfo file in files)
                File.Copy(file.FullName, outpath + file.Name, true);

            for (int i = start; i <= end; i++)
            {
                StreamReader sr = new StreamReader(inputpath + i + "c_i.gv");
                StreamWriter sw = new StreamWriter(outpath + i + "c_i.gv");

                int prevRootId = -1;
                int iline = 0;
                string line;
                string rootIdString = null;
                while ((line = sr.ReadLine()) != null)
                {
                    iline++;
                    if (iline == 4)
                    {
                        Regex r = new Regex("~([0-9]+)");
                        Match match = r.Match(line);
                        if (match.Success)
                        {
                            prevRootId = int.Parse(match.Value.Substring(1, match.Length - 1));
                            rootIdString = "~" + prevRootId + " (";
                        }
                        else
                            throw new Exception("Error finding root!");

                        line = line.Substring(0, match.NextMatch().Index) + nodeEndStr;
                    }
                    else if (iline > 4)
                    {
                        int index = line.IndexOf(rootIdString);
                        if (index > 0)
                        {
                            //line = line.Substring(0, index) + nodeEndStr;
                            int index2 = line.IndexOf("~\\n", index) + 3;
                            line = line.Substring(0, index) + line.Substring(index2);
                        }
                    }

                    sw.WriteLine(line);
                }

                sw.Flush();
                sw.Close();

            }
        }

        private static void AdjustResultNodeOrder()
        {

            //string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected_0202\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D_RemoveRootEdge\";
            //string outpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\selected_0202\0202_190937_gamma0.27alpha0.01KNN100merge64split64cos0.25newalpha1E-20_LooseOrder0.4_OCM_AvgO2(5)_CSC_CSW0.65_D_Modified\";
            string inputpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0526_155047_gamma0.42alpha0.003KNN100merge6E-06split6E-06cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";
            string outpath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\DebtCrisis\0526_155047_gamma0.42alpha0.003KNN100merge6E-06split6E-06cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520_D_Modified\";

            Directory.CreateDirectory(outpath);
            DirectoryInfo info = new DirectoryInfo(inputpath);
            FileInfo[] files = info.GetFiles();
            foreach (FileInfo file in files)
                try
                {
                    if (file.Name.Contains("0"))
                        File.Copy(file.FullName, outpath + file.Name);
                    else
                        File.Copy(file.FullName, outpath + file.Name, true);
                }
                catch
                {
                    //Console.WriteLine("Please give a clean output path!");
                }

            Dictionary<int, List<MoveNode>> movecommands = GetCommands();

            foreach (int i in movecommands.Keys)
            {
                List<MoveNode> commands = movecommands[i];

                StreamReader sr = new StreamReader(inputpath + i + "c.gv");
                StreamWriter sw = new StreamWriter(outpath + i + "c.gv");

                string allcontent = sr.ReadToEnd();
                foreach (MoveNode command in commands)
                    allcontent = command.Move(allcontent);

                //int prevRootId = -1;
                //int iline = 0;
                //string line;
                //string rootIdString = null;
                //while ((line = sr.ReadLine()) != null)
                //{
                //    iline++;
                //    if (iline == 4)
                //    {
                //        Regex r = new Regex("~([0-9]+)");
                //        Match match = r.Match(line);
                //        if (match.Success)
                //        {
                //            prevRootId = int.Parse(match.Value.Substring(1, match.Length - 1));
                //            rootIdString = "~" + prevRootId + " (";
                //        }
                //        else
                //            throw new Exception("Error finding root!");

                //        line = line.Substring(0, match.NextMatch().Index) + nodeEndStr;
                //    }
                //    else if (iline > 4)
                //    {
                //        int index = line.IndexOf(rootIdString);
                //        if (index > 0)
                //        {
                //            line = line.Substring(0, index) + nodeEndStr;
                //        }
                //    }

                //    sw.WriteLine(line);
                //}

                sw.WriteLine(allcontent);
                sw.Flush();
                sw.Close();

            }
        }

        private static Dictionary<int, List<MoveNode>> GetCommands()
        {
            int time;
            List<MoveNode> commands;
            Dictionary<int, List<MoveNode>> movecommands = new Dictionary<int, List<MoveNode>>();
            int pivot;

            time = 1;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(12479, 12589));
            movecommands.Add(time, commands);

            time = 2;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(12586, 12737));
            commands.Add(new SwitchNode(12737, 12614));
            commands.Add(new SwitchNode(12737, 12656));
            pivot = 12687;
            commands.Add(new MoveNodeBefore(12709, pivot));
            //commands.Add(new MoveNodeBefore(12646, pivot));
            //commands.Add(new MoveNodeBefore(12675, pivot));
            movecommands.Add(time, commands);

            time = 3;
            commands = new List<MoveNode>();
            pivot = 11472;
            commands.Add(new MoveNodeAfter(11396, pivot));
            commands.Add(new MoveNodeAfter(11344, pivot));
            //commands.Add(new SwitchNode(11436, 11396));
            movecommands.Add(time, commands);


            time = 4;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(9816, 9881));
            commands.Add(new MoveNodeAfter(9790, 9776));
            movecommands.Add(time, commands);

            time = 5;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(9079, 9155));
            commands.Add(new SwitchNode(9155, 9153));
            movecommands.Add(time, commands);

            time = 6;
            commands = new List<MoveNode>();
            pivot = 7937;
            commands.Add(new MoveNodeAfter(7851, pivot));
            commands.Add(new MoveNodeAfter(7858, pivot));
            movecommands.Add(time, commands);

            time = 7;
            commands = new List<MoveNode>();
            pivot = 7962;
            commands.Add(new MoveNodeAfter(7902, pivot));
            commands.Add(new MoveNodeAfter(7879, pivot));
            movecommands.Add(time, commands);

            time = 8;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(8883, 8862));
            commands.Add(new SwitchNode(8862, 8928));
            commands.Add(new MoveNodeAfter(8849, 8991));
            movecommands.Add(time, commands);

            time = 9;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(7329, 7343));
            commands.Add(new SwitchNode(7370, 7329));
            commands.Add(new MoveNodeBefore(7354, 7403));
            movecommands.Add(time, commands);

            return movecommands;
        }

        private static Dictionary<int, List<MoveNode>> GetCommandsPrev()
        {
            int time;
            List<MoveNode> commands;
            Dictionary<int, List<MoveNode>> movecommands = new Dictionary<int, List<MoveNode>>();
            int pivot;

            time = 1;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3743, 3851));
            commands.Add(new MoveNodeAfter(3891, 3883));
            commands.Add(new MoveNodeAfter(3902, 3895));

            commands.Add(new MoveNodeAfter(3895, 3938));
            commands.Add(new MoveNodeAfter(3682, 3938));
            commands.Add(new MoveNodeAfter(3899, 3938));
            commands.Add(new MoveNodeAfter(3889, 3938));
            movecommands.Add(time, commands);

            time = 2;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3026, 3108));
            commands.Add(new SwitchNode(3028, 3085));

            commands.Add(new MoveNodeAfter(3026, 3123));
            commands.Add(new MoveNodeAfter(3094, 3123));
            commands.Add(new MoveNodeAfter(1746, 3123));
            commands.Add(new MoveNodeAfter(3088, 3123));
            commands.Add(new MoveNodeAfter(2887, 3123));
            commands.Add(new MoveNodeAfter(3130, 3123));
            commands.Add(new MoveNodeAfter(3097, 3123));

            commands.Add(new MoveNodeAfter(3143, 2929));
            movecommands.Add(time, commands);

            time = 3;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3803, 3783));

            pivot = 3742;
            commands.Add(new MoveNodeAfter(3803, pivot));
            commands.Add(new MoveNodeAfter(3781, pivot));
            commands.Add(new MoveNodeAfter(3780, pivot));
            commands.Add(new MoveNodeAfter(3783, pivot));
            commands.Add(new MoveNodeAfter(3796, pivot));
            commands.Add(new MoveNodeAfter(3822, pivot));
            movecommands.Add(time, commands);


            time = 4;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3634, 3591));

            pivot = 3657;
            commands.Add(new MoveNodeAfter(3622, pivot));
            commands.Add(new MoveNodeAfter(3634, pivot));
            commands.Add(new MoveNodeAfter(3601, pivot));
            commands.Add(new MoveNodeAfter(3652, pivot));
            commands.Add(new MoveNodeAfter(3591, pivot));
            commands.Add(new MoveNodeAfter(3617, pivot));
            movecommands.Add(time, commands);

            time = 5;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3356, 3331));

            pivot = 3361;
            commands.Add(new MoveNodeAfter(3356, pivot));
            commands.Add(new MoveNodeAfter(3349, pivot));
            commands.Add(new MoveNodeAfter(3331, pivot));
            commands.Add(new MoveNodeAfter(3334, pivot));

            commands.Add(new MoveNodeAfter(3361, 3124));
            commands.Add(new MoveNodeAfter(3000, 3124));
            movecommands.Add(time, commands);

            time = 6;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3897, 3884));

            pivot = 3734;
            commands.Add(new MoveNodeAfter(3897, pivot));
            commands.Add(new MoveNodeAfter(3884, pivot));
            commands.Add(new MoveNodeAfter(3681, pivot));
            commands.Add(new MoveNodeAfter(3863, pivot));
            commands.Add(new MoveNodeAfter(3896, pivot));
            movecommands.Add(time, commands);

            time = 7;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(4560, 4537));

            pivot = 4583;
            commands.Add(new MoveNodeAfter(4560, pivot));
            commands.Add(new MoveNodeAfter(4549, pivot));
            commands.Add(new MoveNodeAfter(4537, pivot));
            commands.Add(new MoveNodeAfter(4558, pivot));

            commands.Add(new MoveNodeAfter(4471, 4521));
            movecommands.Add(time, commands);

            time = 8;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3502, 3556));
            commands.Add(new SwitchNode(3531, 3506));

            pivot = 3527;
            commands.Add(new MoveNodeAfter(3531, pivot));
            commands.Add(new MoveNodeAfter(3501, pivot));
            commands.Add(new MoveNodeAfter(3477, pivot));
            commands.Add(new MoveNodeAfter(3506, pivot));
            commands.Add(new MoveNodeAfter(3556, pivot));
            commands.Add(new MoveNodeAfter(3502, pivot));
            movecommands.Add(time, commands);

            time = 9;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3381, 3418));
            commands.Add(new MoveNodeBefore(3311, 3075));
            commands.Add(new MoveNodeAfter(3398, 3367));

            pivot = 3404;
            commands.Add(new MoveNodeAfter(3311, pivot));
            commands.Add(new MoveNodeAfter(3398, pivot));
            commands.Add(new MoveNodeAfter(3367, pivot));
            commands.Add(new MoveNodeAfter(3253, pivot));
            commands.Add(new MoveNodeAfter(2879, pivot));
            commands.Add(new MoveNodeAfter(3375, pivot));
            commands.Add(new MoveNodeAfter(3372, pivot));
            commands.Add(new MoveNodeAfter(3418, pivot));
            commands.Add(new MoveNodeAfter(3381, pivot));
            movecommands.Add(time, commands);

            return movecommands;
        }


        private static Dictionary<int, List<MoveNode>> GetCommands0()
        {
            int time;
            List<MoveNode> commands;
            Dictionary<int, List<MoveNode>> movecommands = new Dictionary<int, List<MoveNode>>();

            time = 1;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3743, 3851));
            commands.Add(new MoveNodeAfter(3891, 3883));
            commands.Add(new MoveNodeAfter(3902, 3895));
            movecommands.Add(time, commands);

            time = 2;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3026, 3108));
            commands.Add(new SwitchNode(3028, 3085));
            movecommands.Add(time, commands);

            time = 3;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3803, 3783));
            movecommands.Add(time, commands);

            time = 4;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3634, 3591));
            movecommands.Add(time, commands);

            time = 5;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3356, 3331));
            movecommands.Add(time, commands);

            time = 6;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(3897, 3884));
            movecommands.Add(time, commands);

            time = 7;
            commands = new List<MoveNode>();
            commands.Add(new SwitchNode(4560, 4537));
            movecommands.Add(time, commands);

            time = 8;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3502, 3556));
            commands.Add(new SwitchNode(3531, 3506));
            movecommands.Add(time, commands);

            time = 9;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeBefore(3381, 3418));
            commands.Add(new MoveNodeBefore(3311, 3075));
            commands.Add(new MoveNodeAfter(3398, 3367));
            movecommands.Add(time, commands);

            return movecommands;
        }

        private static Dictionary<int, List<MoveNode>> GetCommands1()
        {
            int time;
            List<MoveNode> commands;
            Dictionary<int, List<MoveNode>> movecommands = new Dictionary<int, List<MoveNode>>();

            time = 1;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3895, 3872));
            commands.Add(new MoveNodeAfter(3889, 3872));
            //commands.Add(new MoveNodeBefore(3873, 3899));
            commands.Add(new MoveNodeBefore(3877, 3899));
            movecommands.Add(time, commands);

            time = 2;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3026, 3128));
            commands.Add(new MoveNodeAfter(3088, 3128));
            commands.Add(new MoveNodeAfter(3097, 3128));
            commands.Add(new SwitchNode(3128, 3104));
            commands.Add(new MoveNodeBefore(3081, 3130));
            commands.Add(new MoveNodeBefore(2929, 3130));
            movecommands.Add(time, commands);

            time = 3;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3803, 2891));
            commands.Add(new MoveNodeAfter(3783, 2891));
            commands.Add(new MoveNodeAfter(3796, 2891));
            commands.Add(new MoveNodeBefore(2891, 3822));
            commands.Add(new MoveNodeBefore(3768, 3822));
            commands.Add(new MoveNodeBefore(3761, 3822));
            movecommands.Add(time, commands);

            time = 4;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3622, 3649));
            commands.Add(new MoveNodeAfter(3634, 3649));
            commands.Add(new MoveNodeAfter(3591, 3649));
            commands.Add(new MoveNodeAfter(3617, 3649));
            movecommands.Add(time, commands);

            time = 5;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3356, 3361));
            commands.Add(new MoveNodeAfter(3331, 3361));
            commands.Add(new MoveNodeAfter(3334, 3361));
            commands.Add(new MoveNodeAfter(2300, 3124));
            commands.Add(new MoveNodeAfter(3235, 3124));
            movecommands.Add(time, commands);

            time = 6;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3897, 3830));
            commands.Add(new MoveNodeAfter(3884, 3830));
            commands.Add(new MoveNodeAfter(3896, 3830));
            commands.Add(new MoveNodeAfter(3830, 3870));
            movecommands.Add(time, commands);

            time = 7;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(4560, 4555));
            commands.Add(new MoveNodeAfter(4537, 4555));
            commands.Add(new MoveNodeAfter(4558, 4555));
            commands.Add(new MoveNodeBefore(4583, 4573));
            commands.Add(new MoveNodeAfter(4087, 4521));
            movecommands.Add(time, commands);

            time = 8;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3531, 3527));
            commands.Add(new MoveNodeAfter(3506, 3527));
            commands.Add(new MoveNodeAfter(3556, 3527));
            commands.Add(new MoveNodeAfter(3502, 3527));
            movecommands.Add(time, commands);

            time = 9;
            commands = new List<MoveNode>();
            commands.Add(new MoveNodeAfter(3398, 3404));
            commands.Add(new MoveNodeAfter(3311, 3404));
            commands.Add(new MoveNodeAfter(3375, 3404));
            commands.Add(new MoveNodeAfter(3418, 3404));
            commands.Add(new MoveNodeAfter(3381, 3404));
            commands.Add(new MoveNodeBefore(3350, 3372));
            movecommands.Add(time, commands);

            return movecommands;
        }

        abstract class MoveNode
        {
            public virtual string Move(string allcontent)
            {
                return null;
            }
        }

        class MoveNodeBefore : MoveNode
        {
            int nodeindex;
            int beforenodeindex;
            public MoveNodeBefore(int nodeindex, int beforenodeindex)
            {
                this.nodeindex = nodeindex;
                this.beforenodeindex = beforenodeindex;
            }

            public override string Move(string allcontent)
            {
                int index0 = allcontent.IndexOf("-" + nodeindex + "-");
                int index1 = allcontent.IndexOf("-" + beforenodeindex + "-");

                if (index0 < index1)
                    return allcontent;

                int indexstart0 = allcontent.Substring(0, index0).LastIndexOf('\n');
                indexstart0 = allcontent.Substring(0, indexstart0 - 1).LastIndexOf('\n');
                int indexend0 = allcontent.IndexOf('\n', index0);

                int indexstart1 = allcontent.Substring(0, index1).LastIndexOf('\n');
                indexstart1 = allcontent.Substring(0, indexstart1 - 1).LastIndexOf('\n');
                int indexend1 = allcontent.IndexOf('\n', index1);

                indexstart0++;
                indexstart1++;
                indexend0++;
                indexend1++;
                //Console.WriteLine(allcontent.Substring(indexstart1, indexend1 - indexstart1));
                //return null;

                string head = allcontent.Substring(0, indexstart1);
                string beforenodecontent = allcontent.Substring(indexstart1, indexend1 - indexstart1);
                string middle = allcontent.Substring(indexend1, indexstart0 - indexend1);
                string nodecontent = allcontent.Substring(indexstart0, indexend0 - indexstart0);
                string end = allcontent.Substring(indexend0);

                return head + nodecontent + beforenodecontent + middle + end;
            }
        }

        class MoveNodeAfter : MoveNode
        {
            int nodeindex;
            int afternodeindex;
            public MoveNodeAfter(int nodeindex, int afternodeindex)
            {
                this.nodeindex = nodeindex;
                this.afternodeindex = afternodeindex;
            }

            public override string Move(string allcontent)
            {
                int index1 = allcontent.IndexOf("-" + nodeindex + "-");
                int index0 = allcontent.IndexOf("-" + afternodeindex + "-");

                if (index0 < index1)
                    return allcontent;

                int indexstart0 = allcontent.Substring(0, index0).LastIndexOf('\n');
                indexstart0 = allcontent.Substring(0, indexstart0 - 1).LastIndexOf('\n');
                int indexend0 = allcontent.IndexOf('\n', index0);

                int indexstart1 = allcontent.Substring(0, index1).LastIndexOf('\n');
                indexstart1 = allcontent.Substring(0, indexstart1 - 1).LastIndexOf('\n');
                int indexend1 = allcontent.IndexOf('\n', index1);

                indexstart0++;
                indexstart1++;
                indexend0++;
                indexend1++;
                //Console.WriteLine(allcontent.Substring(indexstart1, indexend1 - indexstart1));
                //return null;

                string head = allcontent.Substring(0, indexstart1);
                string beforenodecontent = allcontent.Substring(indexstart1, indexend1 - indexstart1);
                string middle = allcontent.Substring(indexend1, indexstart0 - indexend1);
                string nodecontent = allcontent.Substring(indexstart0, indexend0 - indexstart0);
                string end = allcontent.Substring(indexend0);

                return head + middle + nodecontent + beforenodecontent + end;
            }
        }

        class SwitchNode : MoveNode
        {
            int nodeindex;
            int afternodeindex;
            public SwitchNode(int nodeindex, int afternodeindex)
            {
                if (nodeindex < afternodeindex)
                {
                    this.nodeindex = afternodeindex;
                    this.afternodeindex = nodeindex;

                }
                else
                {
                    this.nodeindex = nodeindex;
                    this.afternodeindex = afternodeindex;
                }
            }

            public override string Move(string allcontent)
            {
                int index1 = allcontent.IndexOf("-" + nodeindex + "-");
                int index0 = allcontent.IndexOf("-" + afternodeindex + "-");

                if (index0 < index1)
                    return allcontent;

                int indexstart0 = allcontent.Substring(0, index0).LastIndexOf('\n');
                indexstart0 = allcontent.Substring(0, indexstart0 - 1).LastIndexOf('\n');
                int indexend0 = allcontent.IndexOf('\n', index0);

                int indexstart1 = allcontent.Substring(0, index1).LastIndexOf('\n');
                indexstart1 = allcontent.Substring(0, indexstart1 - 1).LastIndexOf('\n');
                int indexend1 = allcontent.IndexOf('\n', index1);

                indexstart0++;
                indexstart1++;
                indexend0++;
                indexend1++;
                //Console.WriteLine(allcontent.Substring(indexstart1, indexend1 - indexstart1));
                //return null;

                string head = allcontent.Substring(0, indexstart1);
                string beforenodecontent = allcontent.Substring(indexstart1, indexend1 - indexstart1);
                string middle = allcontent.Substring(indexend1, indexstart0 - indexend1);
                string nodecontent = allcontent.Substring(indexstart0, indexend0 - indexstart0);
                string end = allcontent.Substring(indexend0);

                return head + nodecontent + middle + beforenodecontent + end;
            }
        }


        private static void ChangeConfigEvolutionary()
        {
            string filename = "ChangeConfig.txt";
            string configEvoFileName = "ConfigEvolutionary.txt";
            string[] lines = File.ReadAllLines(filename);
            double clusterSizeWeight = double.Parse(lines[0]);
            //List<double> constriantWeight = new List<double>();
            string[] tokens = lines[1].Split('\t');
            //foreach (string token in tokens)
            //    constriantWeight.Add(double.Parse(token));

            double presentConstraint = double.Parse(tokens[0]);
            //Change config file
            string[] evolines = File.ReadAllLines(configEvoFileName);
            int iline = 0;
            Dictionary<int, string> changedlines = new Dictionary<int, string>();
            foreach (string line in evolines)
            {
                string[] evotokens = line.Split('\t');
                if (evotokens[0] == "ClusterSizeWeight")
                {
                    changedlines.Add(iline, evotokens[0] + "\t" + clusterSizeWeight);
                }
                else if (evotokens[0] == "MergeParameter" || evotokens[0] == "SplitParameter")
                {
                    changedlines.Add(iline, evotokens[0] + "\t" + presentConstraint);
                }
                iline++;
            }
            foreach (KeyValuePair<int, string> kvp in changedlines)
                evolines[kvp.Key] = kvp.Value;
            File.WriteAllLines(configEvoFileName, evolines);

            //save new changeconfig
            StreamWriter sw = new StreamWriter(filename);
            sw.WriteLine(lines[0]);
            for (int i = 1; i < tokens.Length; i++)
                sw.Write(tokens[i] + "\t");
            sw.WriteLine();
            sw.Flush();
            sw.Close();
        }

        #region for archiving similarity
        private static void GetAllFoldersInRoot()
        {
            string ifilename = "root.txt";
            string ofilename = "foldersInRoot.txt";
            string root = File.ReadAllLines(ifilename)[0];
            string[] dirs = Directory.GetDirectories(root);

            File.WriteAllLines(ofilename, dirs);
        }


        private static void ChangeConfigHTF()
        {
            string folderfilename = "foldersInRoot.txt";
            string configFileName = "config.txt";

            string[] folders = File.ReadAllLines(folderfilename);
            string[] configlines = File.ReadAllLines(configFileName);

            int ifolder = 0;
            foreach (string folder in folders)
                if (folder.StartsWith("//"))
                    ifolder++;
            if (ifolder >= folders.Length)
                return;

            configlines[0] = folders[ifolder];
            File.WriteAllLines(configFileName, configlines);

            folders[ifolder] = "//" + folders[ifolder];
            File.WriteAllLines(folderfilename, folders);
        }
        #endregion for archiving similarity

        static string datapath = @"D:\Project\EvolutionaryRoseTreeData\";
        static int sample_times = 1;
        static int sample_number = 100;

        static string news_path = datapath + @"data\nmidata\textindex_s7groups\";
        static string bingnews_path = datapath + @"data\BingNews\";
        static string newyorktimes_path = datapath + @"data\NYTimes\NYTIndex_Year\1987";
        static string sample_path = datapath + @"sampledata\";
        static string likelihood_path = datapath + @"likelyhood\";
        static string outputdebugpath = datapath + @"outputpath\";
        static string drawtreepath = datapath + @"rosetree\";
        static int dataset_index = RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP;
        static int model_index = RoseTreeTaxonomy.Constants.Constant.VMF;

        static bool bDrawNode = false;
        static bool bDrawAttribute = true;

        public static double punishweight = 1e-2;
        public static double loseorderpunishweight = 0.1 / sample_number / sample_number;
        public static double increaseorderpunishweight = loseorderpunishweight;


        private static void TestMinHeapInt()
        {
            int[] values = new int[] { 10, 9, 2, 3, -1, 0, 2, 8, 7, 6 };
            int k = 3;
            MinHeapInt mh = new MinHeapInt(k);

            for (int i = 0; i < k; i++)
                mh.insert(-1, int.MinValue);

            for (int j = 0; j < values.Length; j++)
            {
                if (values[j] > mh.min())
                    mh.changeMin(j, values[j]);
            }
            MinHeapInt.heapSort(mh);

            int[] indices = mh.getIndices();

            foreach (int index in indices)
            {
                Console.WriteLine("{0},{1}", index, values[index]);
            }
        }

        private static void TestMinHeapInt2()
        {
            int[] values = new int[] { 10, 9, 2, 3, -1, 0, 2, 8, 7, 6 };
            int k = 3;
            MinHeapInt mh = new MinHeapInt(k);

            for (int i = 0; i < k; i++)
                mh.insert(values[i], i);

            //for (int j = 0; j < values.Length; j++)
            //{
            //    if (values[j] > mh.min())
            //        mh.changeMin(j, values[j]);
            //}
            MinHeapInt.heapSort(mh);

            int[] indices = mh.getIndices();

            foreach (int index in indices)
            {
                Console.WriteLine("{0},{1}", index, values[index]);
            }
        }

        private static void TestMinHeapDouble()
        {
            double[] cosines = new double[] { 0, -1, 0.1, 0.2, -0.1, 5, 6, 7, 9, 1.1 };
            int k = 6;
            MinHeapDouble mhd = new MinHeapDouble(k);

            for (int i = 0; i < k; i++)
                mhd.insert(-1, double.MinValue);

            for (int i = 0; i < cosines.Length; i++)
            {
                double cosine = cosines[i];

                if (cosine > mhd.min() || (cosine == mhd.min() && i < mhd.getIndices()[0]))
                    mhd.changeMin(i, cosine);
            }

            MinHeapDouble.heapSort(mhd);

            int[] indices = mhd.getIndices();

            foreach (int index in indices)
            {
                Console.WriteLine("{0},{1}", index, cosines[index]);
            }

            Console.WriteLine();
            double[] values = mhd.getValues();
            foreach (double value in values)
                Console.WriteLine(value);
        }

        private static void TestMaxHeapDouble()
        {
            double[] cosines = new double[] { 0, -1, 0.1, 0.2, -0.1, 5, 6, 7, 9, 1.1 };
            int k = 3;
            MaxHeapDouble mhd = new MaxHeapDouble(k);

            for (int i = 0; i < k; i++)
                mhd.insert(-1, double.MinValue);

            for (int i = 0; i < cosines.Length; i++)
            {
                double cosine = cosines[i];

                if (cosine < mhd.max() || (cosine == mhd.max() && i < mhd.getIndices()[0]))
                    mhd.changeMax(i, cosine);
            }

            MaxHeapDouble.heapSort(mhd);

            int[] indices = mhd.getIndices();

            foreach (int index in indices)
            {
                Console.WriteLine("{0},{1}", index, cosines[index]);
            }
        }

        private static void TestSparseVectorAdd()
        {
            int[] keyarray0 = { 6, 3, 2, 4, 5, 1 };
            int[] valuearray0 = { 6, 3, 2, 4, 5, 1 };
            int[] keyarray1 = { 2, 3, 1, 4, 5, 6 };
            int[] valuearray1 = { 2, 3, 1, 4, 5, 6 };

            SparseVectorList vector0 = new SparseVectorList(0);
            vector0.keyarray = keyarray0;
            vector0.valuearray = valuearray0;
            vector0.count = keyarray0.Length;

            SparseVectorList vector1 = new SparseVectorList(0);
            vector1.keyarray = keyarray1;
            vector1.valuearray = valuearray1;
            vector1.count = keyarray1.Length;

            List<int> overlapping_keylist;
            int length;
            SparseVectorList vector = vector0.AddValue(vector0, vector1, out overlapping_keylist, out length);

            for (int i = 0; i < length; i++)
            {
                Console.WriteLine("<{0},{1}>", vector.keyarray[i], vector.valuearray[i]);
            }

            foreach (int overlapfeature in overlapping_keylist)
                Console.WriteLine(overlapfeature);
        }

        private static void TestExpandedCacheClass()
        {
            double alpha = 0.6;
            double gamma = 0.1;
            int maxdimensionvalue = 100;    //max(wordfrequency.value)  148550
            int wordnum = 10000;            //sum(wordoccurrences)      7812282
            int datasize = 100;             //featurevector.length      1000
            int datadimension = 100;        //lexiconsize               47989   (wordfrequency.count:47748)

            CacheClass cacheclass_short = new CacheClass(alpha, gamma, maxdimensionvalue, wordnum, datasize, datadimension);
            CacheClass cacheclass_long = new CacheClass(alpha, gamma, 2 * maxdimensionvalue, 2 * wordnum, datasize, datadimension);
            cacheclass_short.Cache();
            cacheclass_long.Cache();
            ExpandedCacheClass cacheclass_expanded = new ExpandedCacheClass(cacheclass_short);

            for (int i = 1; i <= 2 * wordnum; i++)
            {
                double value1 = cacheclass_long.GetLogFactorials(i);
                double value2 = cacheclass_expanded.GetLogFactorials(i);
                if (Math.Abs(value1 - value2) > 1e-10)
                    Console.WriteLine();
            }

            for (int i = 1; i <= 2 * wordnum; i++)
            {
                double value1 = cacheclass_long.GetLogAlphaSumItem(i);
                double value2 = cacheclass_expanded.GetLogAlphaSumItem(i);
                if (Math.Abs(value1 - value2) > 1e-10)
                    Console.WriteLine();
            }

            for (int i = 1; i <= 2 * maxdimensionvalue; i++)
            {
                double value1 = cacheclass_long.GetLogAlphaItem(i);
                double value2 = cacheclass_expanded.GetLogAlphaItem(i);
                if (Math.Abs(value1 - value2) > 1e-10)
                    Console.WriteLine();
            }

            for (int i = 2; i < datasize + 1; i++)
            {
                double value1 = cacheclass_long.GetLogPi(i);
                double value2 = cacheclass_expanded.GetLogPi(i);
                if (Math.Abs(value1 - value2) > 1e-10)
                    Console.WriteLine();
            }

            for (int i = 2; i < datasize + 1; i++)
            {
                double value1 = cacheclass_long.GetLogOneMinusPi(i);
                double value2 = cacheclass_expanded.GetLogOneMinusPi(i);
                if (Math.Abs(value1 - value2) > 1e-10)
                    Console.WriteLine();
            }

        }

        #region Test Memory
        private static void TestProgramMemory()
        {
            var hHeap = Heap.HeapCreate(Heap.HeapFlags.HEAP_GENERATE_EXCEPTIONS, 0, 0);
            // if the FriendlyName is "heap.vshost.exe" then it's using the VS Hosting Process and not "Heap.Exe"
            Trace.WriteLine(AppDomain.CurrentDomain.FriendlyName + " heap created");
            Console.WriteLine(AppDomain.CurrentDomain.FriendlyName + " heap created");
            uint nSize = 100 * 1024 * 1024;
            ulong nTot = 0;
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    var ptr = Heap.HeapAlloc(hHeap, 0, nSize);
                    nTot += nSize;
                    Trace.WriteLine(String.Format("Iter #{0} {1:n0} ", i, nTot));
                    Console.WriteLine(String.Format("Iter #{0} {1:n0} ", i, nTot));
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception " + ex.Message);
                Console.WriteLine("Exception " + ex.Message);
            }


            Heap.HeapDestroy(hHeap);
            Trace.WriteLine("destroyed");
            Console.WriteLine("destroyed");

            Console.ReadKey();
        }

        public class Heap
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr HeapCreate(HeapFlags flOptions, uint dwInitialsize, uint dwMaximumSize);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr HeapAlloc(IntPtr hHeap, HeapFlags dwFlags, uint dwSize);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool HeapFree(IntPtr hHeap, HeapFlags dwFlags, IntPtr lpMem);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool HeapDestroy(IntPtr hHeap);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetProcessHeap();

            [Flags()]
            public enum HeapFlags
            {
                HEAP_NO_SERIALIZE = 0x1,
                HEAP_GENERATE_EXCEPTIONS = 0x4,
                HEAP_ZERO_MEMORY = 0x8
            }

        }
        #endregion Test Memory


        private static void TestGetMediumValue()
        {
            for (int i = 1; i <= 10; i++)
            {
                double[] values = new double[] { 9, 3, 10, 1, 2, 4, 6, 8, 7, 5 };
                Console.WriteLine("The value {0} smallest is: {1}", i, RoseTreeMath.GetDLargestValue(values, i));
            }
            Console.ReadKey();
        }

        private static void TestAverageDegree()
        {
            BuildRoseTree.SampleNumber = sample_number;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);

            RoseTreeParameters para = new RoseTreeParameters();
            para.gamma = 0.1;
            para.alpha = 100;
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree0 = BuildRoseTree.BuildTree(ldinfo,
                null, para, likelihood_path, outputdebugpath);
            DrawRoseTree(rosetree0, datapath + "rosetree\\", "0.gv");


            para.gamma = 0.1;
            para.alpha = 1;
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree1 = BuildRoseTree.BuildTree(ldinfo,
                null, para, likelihood_path, outputdebugpath);
            DrawRoseTree(rosetree1, datapath + "rosetree\\", "1.gv");

            Console.WriteLine("\n\n");

            Console.WriteLine("{0},{1},{2},{3}",
                rosetree0.JoinCnt, rosetree0.AbsorbLCnt, rosetree0.AbsorbRCnt, rosetree0.CollapseCnt);
            Console.WriteLine(rosetree0.JoinCnt - rosetree0.CollapseCnt);
            Console.WriteLine(2 * rosetree0.JoinCnt + rosetree0.AbsorbLCnt + rosetree0.AbsorbRCnt);
            //double avg0 = rosetree0.GetAverageInternalNodeDegree();
            //Console.Write(avg0);

            Console.WriteLine("{0},{1},{2},{3}",
                rosetree1.JoinCnt, rosetree1.AbsorbLCnt, rosetree1.AbsorbRCnt, rosetree1.CollapseCnt);
            Console.WriteLine(rosetree1.JoinCnt - rosetree1.CollapseCnt);
            Console.WriteLine(2 * rosetree1.JoinCnt + rosetree1.AbsorbLCnt + rosetree1.AbsorbRCnt);
            //double avg1 = rosetree1.GetAverageInternalNodeDegree();
            //Console.Write(avg1);
        }


        public static void TestLoadNewYorkTimesData()
        {
            BuildRoseTree.SampleNumber = sample_number;
            dataset_index = RoseTreeTaxonomy.Constants.Constant.NEW_YORK_TIMES;
            string defaultfield = RoseTreeTaxonomy.Constants.Constant.
                NewYorkTimesDataFields.TaxonomicClassifiers;
            string querystr = "Top/Features/Travel/Guides/Destinations/";

            LoadDataInfo ldinfo = BuildRoseTree.LoadNewYorkTimesGroupData(newyorktimes_path,
                sample_path, dataset_index, model_index, sample_times,
                defaultfield, querystr);
        }

        public static void TestCalculateRobinsonFouldDistance()
        {
            MetricTree tree0 = new MetricTree(0);
            MetricTree tree1 = new MetricTree(0);

            Console.WriteLine(RobinsonFouldsDistance.CalculateDistance(tree0, tree1));

            //double[,] A = new double[,] { 
            //    { double.MaxValue, 0.8810, 0.3808, 0.8027 }, 
            //    { double.MaxValue, 0.3484, 0.8328, 0.1764 } };

            //double[,] A = GetMatrixFromFile(@"D:\Project\ERT\MatlabCode\bghungar\A2.dat");

            //Console.WriteLine(HungarianMatching.GetMinimumWeightMatchingCost(A));

            Console.ReadKey();
        }

        public static void TestBuildRuleRoseTree()
        {
            BuildRoseTree.SampleNumber = sample_number;
            RoseTreeParameters para = new RoseTreeParameters();
            para.alpha = 0.3;   //3
            para.gamma = 0.07;   //0.1
            //para.algorithm_index = RoseTreeTaxonomy.Constants.Constant.BRT;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            //build ground truth tree
            RoseTreeTaxonomy.Algorithms.RoseTree groundtruthtree = BuildRoseTree.BuildGroundTruthRoseTree(ldinfo,
                likelihood_path, outputdebugpath);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo, null, para,
                likelihood_path, outputdebugpath);
            DrawRoseTree(rosetree, drawtreepath, "_0_.gv");

            //build rule tree
            Rules rules = new Rules();
            //rules.AddMaxBalanceRule(0, 0, Int32.MaxValue);

            RoseTreeTaxonomy.Algorithms.RoseTree rosetree2 = BuildRoseTree.BuildTree(ldinfo, null, rules,
                para, likelihood_path, outputdebugpath);
            DrawRoseTree(rosetree2, drawtreepath, "_1_.gv");

            Console.WriteLine();
            Console.WriteLine(rosetree.StructureToString());
            LabelAccuracy.OutputAllAccuracy(rosetree, groundtruthtree);
            Console.WriteLine(rosetree2.StructureToString());
            LabelAccuracy.OutputAllAccuracy(rosetree2, groundtruthtree);

            Console.ReadKey();
        }

        #region Sample overlap data
        public static void TestSampleFileOverlapRatio()
        {
            string filename1 = @"_sampleitems_200_7000_3.txt";
            string filename2 = @"_sampleitems_200_7000_3_overlap_80.txt";

            List<int> sampleitems1 = GetSampleItems(sample_path + filename1);
            List<int> sampleitems2 = GetSampleItems(sample_path + filename2);
            int overlapnum = ConfusionMatrix.GetOverlapNumber(sampleitems1, sampleitems2);

            Console.WriteLine("File 1: " + filename1);
            Console.WriteLine("File 2: " + filename2);

            Console.WriteLine("Overlap sample number: " + overlapnum);

            Console.ReadKey();
        }

        private static List<int> GetSampleItems(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            List<int> sampleitems = new List<int>();

            string line = sr.ReadLine();
            while (line != null)
            {
                sampleitems.Add(int.Parse(line));
                line = sr.ReadLine();
            }

            return sampleitems;
        }

        public static void TestSampleOverlapData()
        {
            BuildRoseTree.SampleNumber = sample_number;
            double overlap = 2;

            //load data
            LoadDataInfo ldinfo0 = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            LoadDataInfo ldinfo1 = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times, overlap);

            IList<int> sampleitems0 = ldinfo0.lfv.GetSampleItems();
            IList<int> sampleitems1 = ldinfo1.lfv.GetSampleItems();

            List<int> samplelist0 = new List<int>();
            List<int> samplelist1 = new List<int>();

            foreach (int item in sampleitems0)
                samplelist0.Add(item);
            foreach (int item in sampleitems1)
                samplelist1.Add(item);

            int overlapnum = ConfusionMatrix.GetOverlapNumber(samplelist0, samplelist1);

            Console.WriteLine("Overlap Ratio: " + (100 * overlapnum / sample_number));

            Console.ReadKey();

        }

        public static void TestSampleOverlapDataToy()
        {
            int[] sample_array0;
            int[] sample_array1;
            int num = 30;
            int samplenum = 10;

            RandomGenerator.SetSeedFromSystemTime();
            Sample sample = new Sample();
            sample.Run(num, samplenum, out sample_array0);
            for (int i = 0; i < samplenum; i++)
                Console.Write(sample_array0[i] + "\t");
            Console.WriteLine();

            sample.Run(num, sample_array0, 0.2, out sample_array1);
            for (int i = 0; i < samplenum; i++)
                Console.Write(sample_array1[i] + "\t");
            Console.WriteLine();

            sample.Run(num, sample_array0, 0.6, out sample_array1);
            for (int i = 0; i < samplenum; i++)
                Console.Write(sample_array1[i] + "\t");
            Console.WriteLine();

            Console.ReadKey();
        }
        #endregion Sample overlap data

        #region ground truth tree
        public static void TestIs2GroundTruthTreeIdentical()
        {
            BuildRoseTree.SampleNumber = sample_number;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            //build tree 0
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree0 = BuildRoseTree.BuildGroundTruthRoseTree(ldinfo,
                likelihood_path, outputdebugpath);
            //build tree 1
            Constraint groundtruthconstraint = new GroundTruthConstraint(ldinfo.lfv);
            RoseTreeParameters para = new RoseTreeParameters();
            para.algorithm_index = RoseTreeTaxonomy.Constants.Constant.BRT;
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree1 = BuildRoseTree.BuildTree(ldinfo,
                groundtruthconstraint, para,
                likelihood_path, outputdebugpath);


            LabelAccuracy.OutputAllAccuracy(rosetree0, rosetree1);
            Console.ReadKey();

        }

        public static void TestBuildGroundTruthTree()
        {
            BuildRoseTree.SampleNumber = sample_number;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildGroundTruthRoseTree(ldinfo,
                likelihood_path, outputdebugpath);

            string drawrosetree_path = datapath + @"rosetree\";
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100);
            drawrosetree.Run();

            Console.ReadKey();

        }
        #endregion ground truth tree

        #region measure accuracy
        public static void TestMeasureAccuracy_IndirectInfo()
        {
            BuildRoseTree.SampleNumber = sample_number;

            RoseTreeParameters para = new RoseTreeParameters();
            para.algorithm_index = RoseTreeTaxonomy.Constants.Constant.BRT;

            //load data 0
            LoadDataInfo ldinfo0 = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            /// Build Ground Truth Rose Tree ///
            //Constraint
            GroundTruthConstraint groundtruthconstraint0 = new GroundTruthConstraint(ldinfo0.lfv);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree groundtruth_rosetree0 = BuildRoseTree.BuildTree(ldinfo0,
                groundtruthconstraint0,
                para,
                likelihood_path, outputdebugpath);

            //load data 1
            sample_times = 2;
            LoadDataInfo ldinfo1 = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            /// Build Constrained Rose Tree ///
            double punishweight = 1e6;
            double loseorderpunishweight = 100;
            double increaseorderpunishweight = 100;
            TreeDistanceConstraint treedistanceconstraint = new TreeDistanceConstraint(groundtruth_rosetree0,
                ldinfo1.lfv, TreeDistanceType.Sum, punishweight);
            TreeOrderConstraint treeorderconstraint = new TreeOrderConstraint(groundtruth_rosetree0,
                ldinfo1.lfv, loseorderpunishweight, increaseorderpunishweight);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo1,
                treedistanceconstraint,
                para,
                likelihood_path, outputdebugpath);
            /// Build Ground Truth Rose Tree ///
            GroundTruthConstraint groundtruthconstraint1 = new GroundTruthConstraint(ldinfo1.lfv);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree groundtruth_rosetree1 = BuildRoseTree.BuildTree(ldinfo1,
                groundtruthconstraint1,
                para,
                likelihood_path, outputdebugpath);

            //string drawrosetree_path = datapath + @"rosetree\";
            //RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100);
            //drawrosetree.Run();

            Console.WriteLine("NMI(L1):" + LabelAccuracy.GetLabelAccuracy(rosetree, groundtruth_rosetree1, 1, AccuracyMeasure.NMI));
            Console.WriteLine("NMI(L2):" + LabelAccuracy.GetLabelAccuracy(rosetree, groundtruth_rosetree1, 2, AccuracyMeasure.NMI));
            Console.WriteLine("NMI(L1):" + LabelAccuracy.GetLabelAccuracy(groundtruth_rosetree0, groundtruth_rosetree1, 1, AccuracyMeasure.NMI));
            Console.WriteLine("NMI(L2):" + LabelAccuracy.GetLabelAccuracy(groundtruth_rosetree0, groundtruth_rosetree1, 2, AccuracyMeasure.NMI));

            Console.ReadKey();
        }

        public static void TestMeasureAccuracy_DirectInfo()
        {
            BuildRoseTree.SampleNumber = sample_number;

            RoseTreeParameters para = new RoseTreeParameters();
            para.algorithm_index = RoseTreeTaxonomy.Constants.Constant.BRT;
            //para.algorithm_index = RoseTreeTaxonomy.Constants.Constant.KNN_BRT;
            //para.k = sample_number / 2;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);

            /// Build Ground Grouth Rose Tree ///
            //Constraint
            GroundTruthConstraint groundtruthconstraint = new GroundTruthConstraint(ldinfo.lfv);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree groundtruth_rosetree = BuildRoseTree.BuildTree(ldinfo,
                groundtruthconstraint,
                para,
                likelihood_path, outputdebugpath);

            /// Build Constrained Rose Tree ///
            //double punishweight = Double.MaxValue;
            double loseorderpunishweight = 10000;
            double increaseorderpunishweight = 10000;
            //TreeDistanceConstraint treedistanceconstraint = new TreeDistanceConstraint(groundtruth_rosetree,
            //    ldinfo.lfv, TreeDistanceType.Sum, punishweight);
            TreeOrderConstraint treeorderconstraint = new TreeOrderConstraint(groundtruth_rosetree,
                ldinfo.lfv, loseorderpunishweight, increaseorderpunishweight);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo,
                treeorderconstraint,
                para,
                likelihood_path, outputdebugpath);

            //string drawrosetree_path = datapath + @"rosetree\";
            //RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100);
            //drawrosetree.Run();

            Console.WriteLine("NMI(L1):" + LabelAccuracy.GetLabelAccuracy(rosetree, groundtruth_rosetree, 1, AccuracyMeasure.NMI));
            Console.WriteLine("NMI(L2):" + LabelAccuracy.GetLabelAccuracy(rosetree, groundtruth_rosetree, 2, AccuracyMeasure.NMI));

            Console.ReadKey();
        }
        #endregion measure accuracy

        #region constrained (ground truth) tree
        public static void TestBuildConstrainedGroundTruthTree()
        {
            BuildRoseTree.SampleNumber = sample_number;

            RoseTreeParameters para = new RoseTreeParameters();
            para.algorithm_index = RoseTreeTaxonomy.Constants.Constant.BRT;
            //para.algorithm_index = RoseTreeTaxonomy.Constants.Constant.KNN_BRT;
            //para.k = sample_number / 2;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            //Constraint
            GroundTruthConstraint groundtruthconstraint = new GroundTruthConstraint(ldinfo.lfv);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo,
                groundtruthconstraint,
                para,
                likelihood_path, outputdebugpath);

            //string drawrosetree_path = datapath + @"rosetree\";
            //RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100);
            //drawrosetree.Run();

            Console.ReadKey();
        }

        public static void TestBuildConstrainedTree()
        {
            BuildRoseTree.SampleNumber = sample_number;
            int dataset_index = RoseTreeTaxonomy.Constants.Constant.BING_NEWS;
            double alpha = 0.4;
            double gamma = 0.05;
            RoseTreeParameters para = new RoseTreeParameters();
            para.alpha = alpha; para.gamma = gamma;

            DateTime t00 = DateTime.Now;
            Console.WriteLine("t00: " + t00);

            int time = 1853;
            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadBingNewsData(bingnews_path, sample_path, time,
                dataset_index, model_index, sample_times);

            DateTime t10 = DateTime.Now;
            Console.WriteLine("t10: " + t10);

            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo, null,
                para, likelihood_path, outputdebugpath);
            DrawRoseTree(rosetree, drawtreepath, "_0_.gv");

            DateTime t20 = DateTime.Now;
            Console.WriteLine("t20: " + t20);

            Console.WriteLine("Preparation time: " + (t10.Ticks - t00.Ticks) / 1e7 + "s");
            Console.WriteLine("Run time: " + (t20.Ticks - t10.Ticks) / 1e7 + "s");
            Console.WriteLine("-------------------------------------------------");


            //DrawRoseTree(datapath + @"rosetree\", rosetree);
            DrawRoseTree(rosetree, datapath + @"rosetree\", time + ".gv");

            DateTime t0 = DateTime.Now;
            Console.WriteLine("t0: " + t0);

            time = 1854;
            //load new data
            //double punishweight = 1e-2;
            //double loseorderpunishweight = 0.1;
            //double increaseorderpunishweight = 0.1;
            LoadDataInfo ldinfo2 = BuildRoseTree.LoadBingNewsData(bingnews_path, sample_path, time,
                dataset_index, model_index, sample_times);
            //TreeDistanceConstraint distanceconstraint = new TreeDistanceConstraint(rosetree,
            //    ldinfo2.lfv, TreeDistanceType.Sum, punishweight);



            TreeOrderConstraint orderconstraint =
                new LooseTreeOrderConstraint(rosetree, ldinfo2.lfv,
                    loseorderpunishweight, increaseorderpunishweight);


            //orderconstraint.DrawConstraintTree(datapath + @"rosetree\constrainttree.gv");

            DateTime t1 = DateTime.Now;
            Console.WriteLine("t1: " + t1);

            //build constrained tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree2 = BuildRoseTree.BuildTree(ldinfo2,
                orderconstraint,
                para,
                likelihood_path, outputdebugpath);
            DrawRoseTree(rosetree2, drawtreepath, "_1_.gv");

            DateTime t2 = DateTime.Now;
            Console.WriteLine("t2: " + t2);

            Console.WriteLine("Preparation time: " + (t1.Ticks - t0.Ticks) / 1e7 + "s");
            Console.WriteLine("Run time: " + (t2.Ticks - t1.Ticks) / 1e7 + "s");

            DrawRoseTree(rosetree2, datapath + @"rosetree\", time + ".gv");

            //Console.ReadKey();
        }
        #endregion constrained (ground truth) tree

        #region build single tree
        public static void TestBuildSingleRoseTree()
        {
            int time = 1;

            BuildRoseTree.SampleNumber = sample_number;
            dataset_index = RoseTreeTaxonomy.Constants.Constant.BING_NEWS;
            news_path = bingnews_path;

            RoseTreeParameters para = new RoseTreeParameters();
            para.kappa = 1400;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadBingNewsData(news_path, sample_path, time,
                dataset_index, model_index, sample_times);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo, null, para,
                likelihood_path, outputdebugpath);

            string drawrosetree_path = datapath + @"rosetree\";
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100, true, true);
            drawrosetree.Run();
        }

        public static void TestBuildSingleBayesianBinaryTree()
        {
            int time = 1;

            BuildRoseTree.SampleNumber = sample_number;
            dataset_index = RoseTreeTaxonomy.Constants.Constant.BING_NEWS;
            news_path = bingnews_path;

            RoseTreeParameters para = new RoseTreeParameters();
            para.kappa = 1400;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadBingNewsData(news_path, sample_path, time,
                dataset_index, model_index, sample_times);
            //build binary tree
            BuildRoseTree.BRestrictBinary = true;
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo, null, para,
                likelihood_path, outputdebugpath);

            string drawrosetree_path = datapath + @"rosetree\";
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100, true, true);
            drawrosetree.Run();
        }


        public static void TestBuild20NGRoseTree()
        {
            BuildRoseTree.SampleNumber = sample_number;

            //load data
            LoadDataInfo ldinfo = BuildRoseTree.LoadTwentyNewsGroupData(news_path, sample_path,
                dataset_index, model_index, sample_times);
            //build tree
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo,
                likelihood_path, outputdebugpath);

            string drawrosetree_path = datapath + @"rosetree\";
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100);
            drawrosetree.Run();
        }

        public static void TestSingleBingNewsRoseTree_PrevExperiment()
        {
            RoseTreeTaxonomy.Experiments.Experiment experiment = new RoseTreeTaxonomy.Experiments.Experiment();

            int time = 1;//1841
            //int dataset_index = RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP;
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree = experiment.BingNewsRoseTree(datapath + @"BingNewsData\", datapath + @"likelyhood\",
                datapath + @"sampledata\", datapath + @"outputpath\", time,
                RoseTreeTaxonomy.Constants.Constant.BING_NEWS,
                RoseTreeTaxonomy.Constants.Constant.DCM, RoseTreeTaxonomy.Constants.Constant.KNN_BRT, 1);

            string drawrosetree_path = datapath + @"rosetree\";
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100);
            drawrosetree.Run();

        }

        public static void TestSingle20NGRoseTree_PrevExperiment()
        {
            RoseTreeTaxonomy.Experiments.Experiment experiment = new RoseTreeTaxonomy.Experiments.Experiment();

            //int dataset_index = RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP;
            RoseTreeTaxonomy.Algorithms.RoseTree rosetree =
                experiment.TwentyNewsGroupRoseTree(datapath + @"nmidata\textindex_17groups", datapath + @"likelyhood\",
                datapath + @"sampledata\", datapath + @"outputpath\",
                RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP,
                RoseTreeTaxonomy.Constants.Constant.DCM,
                RoseTreeTaxonomy.Constants.Constant.BRT,
                sample_number);

            string drawrosetree_path = datapath + @"rosetree\";
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100);
            drawrosetree.Run();

        }
        #endregion build single rose tree

        //test time stamps
        public static void TestDocNumberPerTimeStamp()
        {
            String bingnewsfile = EvolutionaryRoseTree.Constants.Constant.DATA_PATH + EvolutionaryRoseTree.Constants.Constant.inputfilenames[1];
            StreamReader sr = new StreamReader(bingnewsfile);
            HashSet<int> hashtime = new HashSet<int>();
            String str;
            int line = 0;
            while ((str = sr.ReadLine()) != null)
            {
                //Console.WriteLine(str);
                String[] tokens = str.Split('\t');
                int time = int.Parse(tokens[tokens.Length - 1]);
                hashtime.Add(time);
                if (line % 10000 == 0)
                    Console.WriteLine(line);
                line++;
            }

            sr.Close();

            Console.WriteLine("time counts:" + hashtime.Count);
            Console.WriteLine("min:" + hashtime.Min());
            Console.WriteLine("max:" + hashtime.Max());
            foreach (int time in hashtime)
            {
                Console.Write(time + "\t");
            }

            List<int> timelist = hashtime.ToList();
            timelist.Sort();
            foreach (int time in timelist)
                Console.WriteLine(time + "\t");
        }

        private static void TestSimpleAccuracy()
        {
            //int[] label1 = new int[]{1,2,1,1,1,3,1};
            //int[] label2 = new int[]{2,2,1,1,1,2,2};
            //Console.WriteLine(NMI.GetNormalizedMutualInfo(label1, label2));
            //Console.WriteLine(Purity.GetPurity(label1, label2));
            //Console.WriteLine(ARI.GetAdjustedRandIndex(label1, label2));
            int samplenumber = 1000;
            int samplecluster = 50;

            Random random = new Random(0);

            int[] label1 = new int[samplenumber];
            int[] label2 = new int[samplenumber];

            for (int i = 0; i < samplenumber; i++)
                label2[i] = (int)(random.NextDouble() * samplecluster);
            //label2[i] = (int)(i * samplecluster / samplenumber);

            for (int i = 0; i < samplenumber; i++)
                label1[i] = i;// (int)(random.NextDouble() * 100);

            Console.WriteLine(NMI.GetNormalizedMutualInfo(label1, label2));

            //label1[0] = 2;

            Console.WriteLine(NMI.GetNormalizedMutualInfo(label1, label2));
            Console.WriteLine(NMI.GetNormalizedMutualInfo(label2, label1));

            Console.WriteLine(Purity.GetPurity(label1, label2));

            Console.WriteLine(Purity.GetPurity(label2, label1));


            Console.ReadKey();
        }

        private static void DrawRoseTree(RoseTreeTaxonomy.Algorithms.RoseTree rosetree, string drawrosetree_path)
        {
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree =
                new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100, bDrawNode, bDrawAttribute);
            drawrosetree.Run();
        }

        private static void DrawRoseTree(RoseTreeTaxonomy.Algorithms.RoseTree rosetree,
            string drawrosetree_path,
            string filename)
        {
            RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree =
                new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 100, bDrawNode, bDrawAttribute);
            drawrosetree.Run(filename);
        }

        private static double[,] GetMatrixFromFile(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            string line = sr.ReadLine();
            string[] tokens = line.Split('\t');
            int m = int.Parse(tokens[0]);
            int n = int.Parse(tokens[1]);

            double[,] data = new double[m, n];
            for (int i = 0; i < m; i++)
            {
                line = sr.ReadLine();
                tokens = line.Split('\t');
                for (int j = 0; j < n; j++)
                    data[i, j] = double.Parse(tokens[j]);
            }

            return data;
        }
    }
}
