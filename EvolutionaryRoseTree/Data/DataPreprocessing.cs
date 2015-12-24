using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using Directory = System.IO.Directory;

//using EvolutionaryRoseTree.Constants;
using EvolutionaryRoseTree.Experiments;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Constants;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Index;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

using Ionic.Zip;
using System.Collections;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.Tools;
using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Util;

namespace EvolutionaryRoseTree.Data
{
    class DataPreprocessing
    {
        public static void Entry()
        {
            //ParseBingNewsXMLData_Parallel(8, 7, 1);
            //ParseBingNewsXMLData();
            //MergeParallelProcessedBingNewsIndex();
            //ParseBingNewsXMLData_Parallel_Unique();
            //ParseBingNewsXMLData_Parallel(0, 1, int.MaxValue);
            //RemoveDuplicateDocuments();
            //AddDocumentFeatureVectorToIndex();
            //AddFeatureVectorToIndexConfig();
            //PrintPossibleStopWords();
            //RemoveLeadingParagraphNoKeyWords();
            //FilteringIndex();
            //ChangeLuceneIndexFieldValue();
            //PrintLunceneIndexFieldStatistics();

            //MergeParallelProcessedBingNewsIndex_DocumentByDocument_NoRepeatTimes();
            //RemoveSimilarDocuments();

            //SampleLuceneIndex();

            //string[] corpuses = new string[] { "google", "microsoft", "yahoo" };
            //foreach (var corpus in corpuses)
            //    BuildTopicGraphRoseTree(corpus);
            //PrintPossibleStopWords();
            //ParseBingNewsXMLData_Parallel_NotUnique_KeywordList();

            //TestBingNewsXMLData_Parallel_NotUnique_KeywordList();
            //GenerateSampledIndex();
            //FilteringIndexByDate();
            //dataCleanByQueryIter();

            //ProcessMatlabFile();
            //ProcessMatlabFile2();
            for (int i = 0; i <= 10; i++)
            {
                MergeMatlabFile(i);
            }
        }

        private static void MergeMatlabFile(int conf)
        {
            string folder = @"D:\Project\ERT\MatlabCode\data\new";
            string fileHeader = "GetData_ExpD_NYT_1EN67_C0_OT4_k7_PredSD_ConfR" + conf + "_";

            if (!folder.EndsWith("\\")) folder += "\\";

            List<string> filenames = new List<string>();
            foreach (var filename in Directory.GetFiles(folder))
            {
                if (filename.StartsWith(folder + fileHeader) && !filename.Contains("_p"))
                {
                    filenames.Add(filename);
                }
            }

            Console.WriteLine("Found -->{0}<-- files, Continue?", filenames.Count);
            //Console.ReadKey();

            int lineBase = 37;
            //int singleSampleTimesNumber = 36;

            var repFilename = filenames[0];
            var sw = new StreamWriter(repFilename.Substring(0, repFilename.Length - 2) + "merge2.m");
            {
                //Write header
                var sr = new StreamReader(repFilename);

                string line;
                int lineIndex = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (++lineIndex >= lineBase)
                        break;
                    if (line.StartsWith("datadim"))
                    {
                        var index = line.IndexOf("[");
                        var newline = line.Substring(0, index + 1);
                        newline += "5";
                        newline += line.Substring(index + 2);
                        sw.WriteLine(newline);
                    }
                    else if (lineIndex == 20)
                    {
                        sw.WriteLine("	{1	2	3	4	5},...");
                    }
                    else
                        sw.WriteLine(line);
                }

                sw.Flush();
                sr.Close();
            }

            //Merge data
            foreach (var filename in filenames)
            {
                var sr = new StreamReader(filename);

                string line;
                int lineIndex = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    lineIndex++;
                    //if (lineIndex >= lineBase && lineIndex < lineBase + singleSampleTimesNumber)
                    //    sw.WriteLine(line);
                    if (lineIndex < lineBase)
                        continue;
                    else if (line.Length > 0)
                        sw.WriteLine(line);
                    else
                        break;
                }
            }
            sw.WriteLine("\n];\n%Result Meaning:LogLH, SmDis, SmOrder, SmRF, RunTime; JnCnt, ALCnt, ARCnt, CpCnt, TreeDepth; L1CC, L1NC, L1AvgLf, L1StdLf; L2CC, L2NC, L2AvgLf, L2StdLf; L1NMI, L2NMI, L1Kmean, L2Kmean, L1Purity, L2Purity; L1sNMI, L2sNMI, L1sKmean, L2sKmean, L1sPurity, L2sPurity; SpPrec, SpPrec2, RpPrec, SpFail, OlNodes; InternalNodes, CTNodes, LeafDis, LeafSDis;SmDis2, SmOrder2, SmRF2, SmDis3, SmOrder3, SmRF3, SmDis4, SmOrder4, SmRF4, SmDis5, SmOrder5, SmRF5, SmDis6, SmOrder6, SmRF6, SmDis7, SmOrder7, SmRF7;minDepth, maxDepth, avgDepth, stdDepth;\n\nend");
            sw.Flush();
            sw.Close();
        }

        private static void ProcessMatlabFile2()
        {
            string folder = @"D:\Project\ERT\MatlabCode\data\";
            foreach (var filename in Directory.GetFiles(folder))
            {
                if (filename.StartsWith(folder + "GetData_Exp4_NYT_1EN56_C0_OT3_k7_PredSD_ConfR") &&
                    filename.Contains("."))
                {
                    string newfilename = filename.Replace('.','P');
                    File.Copy(filename, newfilename);
                }
            }
        }

        private static void ProcessMatlabFile()
        {
            string folder = @"D:\Project\ERT\MatlabCode\data";
            string fileHeader = "GetData_Exp4_NYT_1EN67_C0_OT3_k7_PredSD_ConfR";

            if (!folder.EndsWith("\\")) folder += "\\";

            List<string> filenames = new List<string>();
            foreach (var filename in Directory.GetFiles(folder))
            {
                if (filename.StartsWith(folder + fileHeader) && !filename.Contains("_p"))
                {
                    filenames.Add(filename);
                }
            }

            Console.WriteLine("Fould -->{0}<-- files, Continue?", filenames.Count);
            Console.ReadKey();

            int singleSampleTimesNumber = 45;
            //FilterBySampleTimes(filenames, new int[] { 1 }, singleSampleTimesNumber);
            //FilterBySampleTimes(filenames, new int[] { 2 }, singleSampleTimesNumber);
            //FilterBySampleTimes(filenames, new int[] { 3 }, singleSampleTimesNumber);
            FilterBySampleTimes(filenames, new int[] { 1, 2 }, singleSampleTimesNumber);
            //FilterBySampleTimes(filenames, new int[] { 2, 3 }, singleSampleTimesNumber);
            //FilterBySampleTimes(filenames, new int[] { 1, 3 }, singleSampleTimesNumber);
            //FilterBySampleTimes(filenames, new int[] { 1, 2, 3 }, singleSampleTimesNumber);
        }

        private static void FilterBySampleTimes(
            List<string> filenames,
            int[] sampleTimes, int singleSampleTimesNumber)
        {
            int lineBase = 37;

            foreach (var filename in filenames)
            {
                var sr = new StreamReader(filename);
                var sw = new StreamWriter(filename.Substring(0, filename.Length - 2)
                    + string.Format("p{0}.m", GetSampleTimesString(sampleTimes)));
                string line;
                int lineIndex = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    bool isSkip = true;
                    if (lineIndex < lineBase)
                        isSkip = false;
                    if (isSkip)
                    {
                        foreach (var sampleTime in sampleTimes)
                        {
                            if (lineBase + singleSampleTimesNumber * (sampleTime - 1) <= lineIndex &&
                                lineBase + singleSampleTimesNumber * (sampleTime) > lineIndex)
                            {
                                isSkip = false;
                                break;
                            }
                        }
                    }
                    if (isSkip)
                    {
                        lineIndex++;
                        continue;
                    }
                    if (line.StartsWith("datadim"))
                    {
                        var index = line.IndexOf("[");
                        var newline = line.Substring(0, index + 1);
                        newline += sampleTimes.Length.ToString();
                        newline += line.Substring(index + 2);
                        sw.WriteLine(newline);
                    }
                    else if (line == "	{1	2	3	4	5},...")
                    {
                        sw.Write("	{");
                        for (int i = 0; i < sampleTimes.Length; i++)
                        {
                            sw.Write(sampleTimes[i] + " ");
                        }
                        sw.WriteLine("},...");
                    }
                    else
                        sw.WriteLine(line);

                    lineIndex++;
                }
                sw.WriteLine("\n];\n%Result Meaning:LogLH, SmDis, SmOrder, SmRF, RunTime; JnCnt, ALCnt, ARCnt, CpCnt, TreeDepth; L1CC, L1NC, L1AvgLf, L1StdLf; L2CC, L2NC, L2AvgLf, L2StdLf; L1NMI, L2NMI, L1Kmean, L2Kmean, L1Purity, L2Purity; L1sNMI, L2sNMI, L1sKmean, L2sKmean, L1sPurity, L2sPurity; SpPrec, SpPrec2, RpPrec, SpFail, OlNodes; InternalNodes, CTNodes, LeafDis, LeafSDis;SmDis2, SmOrder2, SmRF2, SmDis3, SmOrder3, SmRF3, SmDis4, SmOrder4, SmRF4, SmDis5, SmOrder5, SmRF5, SmDis6, SmOrder6, SmRF6, SmDis7, SmOrder7, SmRF7;minDepth, maxDepth, avgDepth, stdDepth;\n\nend");
                sw.Flush();
                sw.Close();
            }
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

        #region remove noise by document ranking
        public static void dataCleanByQueryIter()
        {

            //#region parameters
            //string lucenePath = "";
            //string keywordPath = "";
            //string newLucenePath = "";
            //int max_doc_num = -1;
            //int keyword_num = -1;
            //double threshold = -1;
            //double save_ratio = -1;
            //#endregion


            //List<string> keyword_list = GeneralFileOperations.LoadWordList(keywordPath);
            //IndexReader reader = LuceneIndexOperationFunctions.GetIndexReader(lucenePath);
            //IndexSearcher searcher = LuceneIndexOperationFunctions.GetIndexSearcher(reader);
            //List<Document> doc_list = LuceneIndexOperationFunctions.Search(searcher, "NewsArticleDescription", keyword_list, max_doc_num);
            //List<string> text_list = new List<string>();
            //Dictionary<string, string> IDset = new Dictionary<string, string>();
            //for (int i = 0; i < doc_list.Count; i++)
            //{
            //    Document doc = doc_list[i];
            //    text_list.Add(doc.GetField("NewsArticleDescription").StringValue());
            //    IDset.Add(doc.GetField("DocID").StringValue(), "");
            //}
            ////		List<string> keywords = getKeywords(text_list, keyword_num);
            //int iter = 0;
            //while (true && iter < 5)
            //{
            //    iter++;
            //    Console.WriteLine("iteration------------------" + iter);
            //    List<string> newKeywords = getKeywords(text_list, keyword_num);
            //    doc_list = LuceneIndexOperationFunctions.Search(searcher, "NewsArticleDescription", newKeywords, max_doc_num);
            //    text_list = new List<string>();
            //    Dictionary<string, string> newIDset = new Dictionary<string, string>();
            //    int repeat_num = 0;
            //    for (int i = 0; i < doc_list.Count; i++)
            //    {
            //        Document doc = doc_list[i];
            //        text_list.Add(doc.GetField("NewsArticleDescription").StringValue());
            //        newIDset.Add(doc.GetField("DocID").StringValue(), "");
            //        if (IDset.ContainsKey(doc.GetField("DocID").StringValue()))
            //            repeat_num++;
            //    }
            //    Console.WriteLine(repeat_num + "  " + doc_list.Count);
            //    if ((double)repeat_num / doc_list.Count > threshold)
            //        break;
            //    IDset = newIDset;
            //    keyword_list = newKeywords;
            //}

            //doc_list = LuceneIndexOperationFunctions.Search(searcher, "NewsArticleDescription", keyword_list, 0);
            //IndexWriter writer = LuceneIndexOperationFunctions.GetIndexWriter(newLucenePath);
            //for (int i = 0; i < doc_list.Count * save_ratio; i++)
            //{
            //    Document doc = doc_list[i];
            //    writer.AddDocument(doc);
            //}
            //writer.Optimize();
            //writer.Close();

        }

        //public static List<string> getKeywords(List<string> texts, int keyword_num)
        //{

        //    //List<string> keywords = new List<string>();
        //    Dictionary<string, int> word2TF = new Dictionary<string, int>();
        //    Dictionary<string, int> word2DF = new Dictionary<string, int>();
        //    for (int i = 0; i < texts.Count; i++)
        //    {
        //        //librarytips: if it is twitter data, we need to clean it
        //        string text = texts[i];
        //        List<string> words = LuceneIndexOperationFunctions.Tokenize(text);
        //        Dictionary<string, int> word2num = new Dictionary<string, int>();
        //        for (int j = 0; j < words.Count; j++)
        //        {
        //            string word = words[j];
        //            if (word2num.ContainsKey(word) == false)
        //                word2num.Add(word, 1);
        //            else
        //                word2num.Add(word, word2num[word] + 1);
        //        }
        //        List<string> unique_words = new List<string>(word2num.Keys);
        //        for (int j = 0; j < unique_words.Count; j++)
        //        {
        //            string word = unique_words[j];
        //            if (!word2TF.ContainsKey(word))
        //            {
        //                word2TF.Add(word, word2num[word]);
        //                word2DF.Add(word, 1);
        //            }
        //            else
        //            {
        //                word2TF.Add(word, word2TF[word] + word2num[word]);
        //                word2DF.Add(word, word2DF[word] + 1);
        //            }
        //        }
        //    }

        //    {
        //        Dictionary<string, Double> word2weight = new Dictionary<string, Double>();
        //        List<string> words = new List<string>(word2TF.Keys);
        //        for (int i = 0; i < words.Count; i++)
        //        {
        //            string word = words[i];
        //            int TF = word2TF[word];
        //            int DF = word2DF[word];
        //            double weight = (double)TF / Math.Log(DF + 0.1);
        //            word2weight.Add(word, weight);
        //        }
        //        Console.WriteLine("words.Count " + words.Count);
        //        if (keyword_num > word2weight.Count)
        //            keyword_num = word2weight.Count;
        //        Console.WriteLine("Test here!!");
        //        return Utils.GetTopWords(word2weight, keyword_num);

        //        //        var word2weight_list = new List<Tuple<string,Double>>(word2weight);
        //        //        Collections.sort(word2weight_list, new Comparator<Map.Entry<string,Double>>() {   
        //        //            public int compare(Map.Entry<string, Double> entry1, Map.Entry<string,Double> entry2) {      
        //        //                return (entry2.getValue().compareTo(entry1.getValue()));
        //        //            }
        //        //        }); 

        //        ////		for(int i=0; i<100; i++){
        //        ////			System.out.println(word2weight_list.Get(i).getKey()+":\t"+word2weight_list.Get(i).getValue());
        //        ////		}
        //        //        if(keyword_num > word2weight_list.Count)
        //        //            keyword_num =  word2weight_list.Count;
        //        //        for(int i=0; i<keyword_num; i++){
        //        //            System.out.println(word2weight_list.Get(i).getKey()+":\t"+word2weight_list.Get(i).getValue());
        //        //            keywords.Add(word2weight_list.Get(i).getKey());
        //        //        }
        //    }
        //    //return keywords;
        //}
	
        #endregion

        private static void GenerateSampledIndex()
        {
            var sampleNum = 20000;
            string inputindexpath = @"D:\Project\TopicPanorama\data\Index\BingNews\2012_Jan_Dec\BingNewsIndex_Microsoft_Year2012_RemoveSimilar_RemoveNoise";
            string outputindexpath = @"D:\Project\TopicPanorama\data\Index\BingNews\2012_Jan_Dec\BingNewsIndex_Microsoft_Year2012_RemoveSimilar_RemoveNoise_Sample" + sampleNum;

            LoadSampleIndexConfigFileInfo(out inputindexpath, out outputindexpath, out sampleNum);

            LuceneDirectory inputdir = FSDirectory.Open(new DirectoryInfo(inputindexpath));
            IndexReader indexreader = IndexReader.Open(inputdir, true);

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputindexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            var totalDocNum = indexreader.NumDocs();
            Sample sample = new Sample();
            int[] samplearray;
            sample.Run(totalDocNum, sampleNum, out samplearray);

            for (int i = 0; i < sampleNum; i++)
            {
                var sampleDocID = samplearray[i];
                var document = indexreader.Document(sampleDocID);

                indexwriter.AddDocument(document);
            }

            indexreader.Close();
            indexwriter.Optimize();
            indexwriter.Close();
        }

        private static void LoadSampleIndexConfigFileInfo(out string indexpath, out string outputindex, out int samplenumber)
        {
            StreamReader sr = new StreamReader("configSampleIndex.txt");
            string line;
            indexpath = null;
            outputindex = null;
            samplenumber = -1;
            while ((line = sr.ReadLine()) != null)
            {
                switch (line)
                {
                    case "IndexPath":
                        indexpath = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "OutputIndexPath":
                        outputindex = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "SampleNumber":
                        samplenumber = int.Parse(sr.ReadLine());
                        sr.ReadLine();
                        break;
                }
            }
        }

        #region build topic graph rose tree
        static string datapath = @"D:\Project\EvolutionaryRoseTreeData\";
        static string sample_path = datapath + @"sampledata\";
        static string likelihood_path = datapath + @"likelyhood\";
        static string outputdebugpath = datapath + @"outputpath\";
        static string drawtreepath = datapath + @"rosetree\";
        static int model_index = RoseTreeTaxonomy.Constants.Constant.DCM;
        //static int sample_times = 1;

        private static void BuildTopicGraphRoseTree(string corpus)
        {
            //string corpus = "yahoo";
            string topicGraphPath = @"D:\Project\TopicPanorama\data\TopicGraphs\YahooGoogleMicrosoft_2012_JanFeb_gCTMInput_t20b0.01p1000i350si8pg1pg1_run0\" + corpus + "\\";
            
            int contentFactor = 1;
            double topologyFactor = 1;
            double[] FilterWeights = new double[] { 0.3 };        //double.MinValue
            int[] TransformCorrlationTypes = new int[] { -1 };   //0: -min, 1: normalize (0,1), 2:set to 1 if >0
            int[] CalculateShortedPathTypes = new int[] { 0 };  //0: do not calculate, 1: 1/d, set diagional, 2: 1/d-1/max, 1/(shortestpath+1/max)

            for (int iFilterWeight = 0; iFilterWeight < FilterWeights.Length; iFilterWeight++)
            {
                for (int iTransformCorrelation = 0; iTransformCorrelation < TransformCorrlationTypes.Length; iTransformCorrelation++)
                {
                    for (int iCalculateShortPath = 0; iCalculateShortPath < CalculateShortedPathTypes.Length; iCalculateShortPath++)
                    {
                        double FilterWeight = FilterWeights[iFilterWeight];
                        int TransformCorrlationType = TransformCorrlationTypes[iTransformCorrelation];
                        int CalculateShortedPathType = CalculateShortedPathTypes[iCalculateShortPath];

                        string filename = string.Format(corpus + @"_rosetreeinput_c{0}t{1}_F{2}T{3}SP{4}.txt", contentFactor, topologyFactor, (FilterWeight == double.MinValue ? "Min" : FilterWeight.ToString()), TransformCorrlationType, CalculateShortedPathType);

                        int time = (new Random((int)DateTime.Now.Ticks).Next());
                        int topicNumber = 20;
                        //APP
                        double[] alphas = new double[] { 3 };
                        double[] gammas = new double[] { 0.8 };
                        BuildRoseTree.SampleNumber = int.MaxValue;
                        ConstrainedRoseTree.AdjustStructureCollapseThreshold = 0;

                        File.Copy(topicGraphPath + filename, topicGraphPath + time + ".txt");

                        BuildRoseTree.SampleNumber = topicNumber;
                        int dataset_index = RoseTreeTaxonomy.Constants.Constant.BING_NEWS;

                        //load data
                        LoadDataInfo ldinfo = BuildRoseTree.LoadBingNewsData(topicGraphPath, sample_path, time,
                            dataset_index, model_index, 1, topicNumber);

                        for (int ialpha = 0; ialpha < alphas.Length; ialpha++)
                            for (int igamma = 0; igamma < gammas.Length; igamma++)
                            {
                                double alpha = alphas[ialpha];
                                double gamma = gammas[igamma];

                                RoseTreeParameters para = new RoseTreeParameters()
                                {
                                    alpha = alpha,
                                    gamma = gamma,
                                };

                                //build tree
                                RoseTreeTaxonomy.Algorithms.RoseTree rosetree = BuildRoseTree.BuildTree(ldinfo, null, para, likelihood_path, outputdebugpath);

#if ADJUST_TREE_STRUCTURE
                                (rosetree as ConstrainedRoseTree).AdjustTreeStructure();
#endif
                                string drawrosetree_path = topicGraphPath;
                                RoseTreeTaxonomy.DrawTree.DrawRoseTree drawrosetree = new RoseTreeTaxonomy.DrawTree.DrawRoseTree(rosetree, drawrosetree_path, 1000, true, true);
                                drawrosetree.Run(filename.Substring(0, filename.Length - 4) + "_" + (model_index == RoseTreeTaxonomy.Constants.Constant.DCM ? "DCM" : "vMF") + "_alpha" + alpha + "gamma" + gamma + ".gv");

                            }

                        File.Delete(topicGraphPath + time + ".txt");
                    }
                }
            }

        }
        #endregion 

        private static void SampleLuceneIndex()
        {
            string indexpath = @"D:\Project\TopicPanorama\data\Index\BingNews\2012_Jan_Dec\BingNewsIndex_Yahoo_Year2012_RemoveSimilar";
            string corpus = "Yahoo";
            string outputpath = @"D:\Project\TopicPanorama\data\Index\BingNewsSample\BingNewsIndex_Yahoo_JanFeb";
            string sample_path = @"D:\Project\TopicPanorama\data\samplepath\BingNews_" + corpus + "\\";
            int sample_num = 100000;

            string defaultfield = Constant.IndexedBingNewsDataFields.DiscoveryStringTime;
            string startdatestr = "2012-01-01";
            int delatime = 31 + 29;
            string querystr = EvolutionaryExperiments.GetIndexedBingNewsDateQuery(
                EvolutionaryExperiments.GetDateTime(startdatestr), delatime, 0, -1);

            IndexWriter indexwriter = new IndexWriter(FSDirectory.Open(new DirectoryInfo(outputpath)), new StandardAnalyzer(Version.LUCENE_24), true, IndexWriter.MaxFieldLength.UNLIMITED);

            try
            {
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                IndexSearcher searcher = new IndexSearcher(directory, true);

                Version version = Version.LUCENE_24;
                QueryParser queryparser = new QueryParser(version, defaultfield, new StandardAnalyzer(version));

                Query query = queryparser.Parse(querystr);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;

                Console.WriteLine("Start sampling data...");

                if (sample_num > docs.Length)
                    sample_num = docs.Length;
                string sample_filename = BuildRoseTree.InitializeSamplingData(sample_path, sample_num, docs.Length, 0, -1);

                Console.WriteLine("Finish sampling data");
                Console.WriteLine("Start adding documents");

                string[] lines = File.ReadAllLines(sample_filename);
                int lineIndex = 0;
                foreach (var line in lines)
                {
                    if (lineIndex % 5000 == 0)
                        Console.WriteLine(lineIndex);

                    int sampleindex = int.Parse(line);
                    Document document = searcher.Doc(docs[sampleindex].doc);
                    indexwriter.AddDocument(document);

                    lineIndex++;
                }

                Console.WriteLine(docs.Length);

                Console.WriteLine("Finish adding documents");
                Console.WriteLine("Start writing index...");

                indexwriter.Optimize();
                indexwriter.Close();

                Console.WriteLine("All done.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void PrintLunceneIndexFieldStatistics()
        {
            string indexpath = @"D:\Project\TopicPanorama\data\BoardReader\text_index\";

            IndexReader indexreader = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(indexpath)), true).GetIndexReader();

            string previousFieldName = "cdate";
            int docNum = indexreader.NumDocs();
            Console.WriteLine("In total {0} documents", docNum);
            double minval = double.MaxValue, maxval = double.MinValue;

            for (int idoc = 0; idoc < docNum; idoc++)
            {
                if (idoc % 1000 == 0)
                    Console.WriteLine(idoc);

                Document doc = indexreader.Document(idoc);
                double prevval = double.Parse(doc.GetField(previousFieldName).StringValue());
                minval = Math.Min(minval, prevval);
                maxval = Math.Max(maxval, prevval);
            }

            FieldTransformFunc transformfunc = TransformCdataToDateTime;
            Console.WriteLine("MinVal:{0}", transformfunc(minval.ToString()));
            Console.WriteLine("MaxVal:{0}", transformfunc(maxval.ToString()));

            Console.ReadLine();

            indexreader.Close();
        }

        public delegate string FieldTransformFunc(string fieldvalue);
        private static void ChangeLuceneIndexFieldValue()
        {
            string indexpath = @"D:\Project\TopicPanorama\data\BoardReader\text_index\";
            string outputpath = @"D:\Project\TopicPanorama\data\BoardReader\text_index_cleantime\";

            IndexReader indexreader = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(indexpath)), true).GetIndexReader();

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            string previousFieldName = "cdate";
            string newFieldName = "time";
            FieldTransformFunc transformfunc = TransformCdataToDateTime;

            int docNum = indexreader.NumDocs();
            Console.WriteLine("In total {0} documents", docNum);
            for (int idoc = 0; idoc < docNum; idoc++)
            {
                if (idoc % 1000 == 0)
                    Console.WriteLine("{0} documents added", idoc);
                Document doc = indexreader.Document(idoc);
                string prevval = doc.GetField(previousFieldName).StringValue();
                string nowval = transformfunc(prevval);
                doc.RemoveField(previousFieldName);
                doc.Add(new Field(newFieldName, nowval, Field.Store.YES, Field.Index.ANALYZED));

                indexwriter.AddDocument(doc);
            }

            indexreader.Close();

            indexwriter.Optimize();
            indexwriter.Close();
        }

        static string TransformCdataToDateTime(string cdate)
        {
            long milliseconds = Convert.ToInt64(cdate);
            DateTime date = new DateTime(milliseconds * TimeSpan.TicksPerMillisecond);
            date = date.AddYears(1969);
            String time = string.Format("{0:yyyy-MM-dd HH:mm:ss}", date);
            return time;
        }


        private static void FilteringIndexByDate()
        {
            //string indexpath = @"D:\Project\TopicPanorama\data\Index\BingNews\2012_Jan_Dec\BingNewsIndex_Microsoft_Year2012_RemoveSimilar_RemoveNoise";
            //string outputpath = @"D:\Project\TopicPanorama\data\Index\BingNews\2012_Jan_Dec\BingNewsIndex_Microsoft_Year2012_RemoveSimilar_RemoveNoiseNew_JanToApril";
            //string querystr = "European OR Eurozone OR Europe OR Euro OR Eu OR sovereign OR Spain OR Greek OR Greece OR German OR Germany OR Italy OR French OR France OR Russia OR Belgium OR Cyprus OR Slovenia OR Portugal";

            string indexpath, outputpath, startdate, enddate;

            LoadFilteringIndexByDateConfigFileInfo(out indexpath, out outputpath, out startdate, out enddate);

            string queryfield = Constant.IndexedBingNewsDataFields.DiscoveryStringTime;
            string querystr = EvolutionaryExperiments.GetIndexedBingNewsDateQuery(startdate, GetDeltaDays(startdate, enddate), 0);// "European OR Eurozone OR Europe OR Euro OR Eu OR sovereign";
            string outputfile = outputpath + "\\RemovedDocs.txt";

            Directory.CreateDirectory(outputpath);
            StreamWriter ofile = new StreamWriter(outputfile);

            IndexSearcher searcher = new IndexSearcher(FSDirectory.Open(new DirectoryInfo(indexpath)), true);

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            string[] stopwords = RoseTreeTaxonomy.Constants.StopWords.stopwords;
            Hashtable stophash = StopFilter.MakeStopSet(stopwords);

            //string titlefield = Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            //string bodyfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            //string docidfield = Constant.IndexedBingNewsDataFields.DocumentId;

            QueryParser queryparser = new QueryParser(version, queryfield, new StandardAnalyzer(version));
            Query query = queryparser.Parse(querystr);
            TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
            ScoreDoc[] docs = hits.scoreDocs;

            int docNum = searcher.GetIndexReader().NumDocs();
            bool[] selected = new bool[docNum];

            //Console.WriteLine("Filtering finshed.");

            Console.WriteLine("{0} out of {1} documents selected", docs.Length, docNum);
            int idoc = 0;
            foreach (ScoreDoc doc in docs)
            {
                if (idoc % 1000 == 0)
                    Console.WriteLine("{0} documents added", idoc);
                Document document = searcher.Doc(doc.doc);
                indexwriter.AddDocument(document);
                selected[doc.doc] = true;
                idoc++;
            }

            idoc = 0;
            foreach (bool bselected in selected)
            {
                if (!bselected)
                {
                    ofile.WriteLine(idoc);
                }
                idoc++;
            }

            ofile.Flush();
            ofile.Close();

            indexwriter.Optimize();
            indexwriter.Close();     
        }

        private static int GetDeltaDays(string startdate, string enddate)
        {
            TimeSpan timeSpan = EvolutionaryExperiments.GetDateTime(enddate) - EvolutionaryExperiments.GetDateTime(startdate);
            return timeSpan.Days;
        }


        private static void LoadFilteringIndexByDateConfigFileInfo(out string indexpath, out string outputindex, out string startDate, out string endDate)
        {
            StreamReader sr = new StreamReader("configFilterIndexByDate.txt");
            string line;
            indexpath = null;
            outputindex = null;
            startDate = null;
            endDate = null;
            while ((line = sr.ReadLine()) != null)
            {
                switch (line)
                {
                    case "IndexPath":
                        indexpath = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "OutputIndexPath":
                        outputindex = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "StartDate":
                        startDate = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "EndDate":
                        endDate = sr.ReadLine();
                        sr.ReadLine();
                        break;
                }
            }
        }


        private static void RemoveLeadingParagraphNoKeyWords()
        {
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsIndex_Obama\BingNews_Obama_Sep_4months4_RemoveSimilar";
            string outputindexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsIndex_Obama\BingNews_Obama_Sep_4months4_RemoveSimilar_RemoveNoise_4Words";
            string[] keywords = new string[] { "obama", "romney", "republican", "democrat" };
            //string[] keywords = new string[] { "obama", "romney", "republican", "democrat", "gop",
            //"u.s.", "government", "president", "united"};
            string outputfile = outputindexpath + "\\RemovedDocs.txt";

            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(indexpath)), true)).GetIndexReader();

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputindexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
    
            StreamWriter sw = new StreamWriter(outputfile);

            string[] stopwords = RoseTreeTaxonomy.Constants.StopWords.stopwords_BingNews_Obama;
            Hashtable stophash = StopFilter.MakeStopSet(stopwords);

            int docNum = indexreader.NumDocs();
            string titlefield = Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            string bodyfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            string docidfield = Constant.IndexedBingNewsDataFields.DocumentId;
            int removedDocNum = 0;
            Console.WriteLine("Total documents: {0}", docNum);
            for (int idoc = 0; idoc < docNum; idoc++)
            {
                if (idoc % 10000 == 0)
                {
                    if (idoc == 0)
                        continue;
                    Console.WriteLine("Process " + idoc + "th document!");
                    Console.WriteLine("Remove {0} out of {1}: {2}%", removedDocNum, idoc, 100 * removedDocNum / idoc);
                    sw.Flush();
                }

                Document document = indexreader.Document(idoc);
                //PreProcess
                string title = document.Get(titlefield).ToLower();
                string plain = document.Get(bodyfield).ToLower();
                string leadingPara = TestReadingData.GetLeadingParagraph(plain).ToString();

                bool bRemove = true;
                foreach(string keyword in keywords)
                {
                    if (title.Contains(keyword) || leadingPara.Contains(keyword))
                    {
                        bRemove = false;
                        break;
                    }
                }

                if (bRemove)
                {
                    sw.WriteLine("{0}\n{1}", document.Get(docidfield), title);
                    removedDocNum++;
                }
                else
                {
                    indexwriter.AddDocument(document);
                }
            }

            Console.WriteLine("Remove {0} out of {1}: {2}%", removedDocNum, docNum, 100 * removedDocNum / docNum);
            sw.WriteLine("Remove {0} out of {1}: {2}%", removedDocNum, docNum, 100 * removedDocNum / docNum);

            sw.Flush();
            sw.Close();

            indexwriter.Optimize();
            indexwriter.Close();            
        }

        private static void PrintPossibleStopWords()
        {
            //string indexpath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\debt_crisis2\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar";
            //string outputfilename = indexpath + "\\PossibleStopWords.txt";
            string indexpath, outputfilename;
            LoadStopWordConfigFileInfo(out indexpath, out outputfilename);
            string[] stopwords = RoseTreeTaxonomy.Constants.StopWords.stopwords;
            //string[] stopwords = RoseTreeTaxonomy.Constants.StopWords.stopwords_BingNews_DebtCrisis;
            int k = 1000;
            //int titleweight = 5;
            //int leadingweight = 2;
            //int bodyweight = 0;
            int titleweight = 5;
            int leadingweight = 2;
            int bodyweight = 1;

            Dictionary<string, int> termcounts = new Dictionary<string, int>();
            MinHeapInt mhd = new MinHeapInt(k);

            ///Get TermCounts
            Hashtable stophash = StopFilter.MakeStopSet(stopwords);
            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(indexpath)), true)).GetIndexReader();

            int docNum = indexreader.NumDocs();
            string titlefield = Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            string bodyfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            Console.WriteLine("Total documents: {0}", docNum);
            for (int idoc = 0; idoc < docNum; idoc++)
            {
                if (idoc % 10000 == 0)
                    Console.WriteLine("Process " + idoc + "th document!");
                Document document = indexreader.Document(idoc);
                //PreProcess
                string title = document.Get(titlefield);
                string plain = document.Get(bodyfield);

                StringBuilder titles = new StringBuilder(title + " ");
                if (titleweight > 1)
                    for (int i = 1; i < Constant.BingNewsTitleWeight; i++)
                        titles.Append(title + " ");
                StringBuilder sb = new StringBuilder(" \a");
                sb.Append(titles.ToString());
                sb.Append("\a");
                if (leadingweight > bodyweight)
                {
                    var leadingPara = TestReadingData.GetLeadingParagraph(plain);
                    for (int i = bodyweight; i < leadingweight; i++)
                        sb.Append(leadingPara + " ");
                    sb.Append("\a");
                }
                if (bodyweight > 0)
                    for (int i = 0; i < bodyweight; i++)
                        sb.Append(plain + " ");

                //Process
                string[] querylinetokens = sb.ToString().Split('\a');
                if (querylinetokens.Length < 3) continue;
                StringReader reader = new StringReader(querylinetokens[1] + " " + querylinetokens[2]);
                TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_24, reader);

                List<int> featureSeq = new List<int>();
                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new StopFilter(true, result, stophash, true);

                //Lucene.Net.Analysis.Token token = result.Next();
                result.Reset();
                TermAttribute termattr = (TermAttribute)result.GetAttribute(typeof(TermAttribute));
                while (result.IncrementToken())
                {
                    string termtext = termattr.Term();
                    if (!termcounts.ContainsKey(termtext))
                        termcounts.Add(termtext, 1);
                    else
                        termcounts[termtext]++;
                }
            }

            ///Remove stopwords
            List<String> stopwordslist = stopwords.ToList<string>();
            for (int i = 0; i < k; i++)
                mhd.insert(-1, int.MinValue);
            Dictionary<int, string> termIndex = new Dictionary<int, string>();
            int iword = 0;
            foreach(KeyValuePair<string,int> kvp in termcounts)
            {
                if(!stopwordslist.Contains(kvp.Key))
                {
                    if (kvp.Value> mhd.min())
                    mhd.changeMin(iword, kvp.Value);
                    termIndex.Add(iword, kvp.Key);
                    iword++;
                }
            }
            MinHeapInt.heapSort(mhd);
            int[] sortedIndices = mhd.getIndices();

            StreamWriter sw = new StreamWriter(outputfilename);
            foreach (int termindex in sortedIndices)
            {
                string term =termIndex[termindex];
                sw.WriteLine(term + "\t" + termcounts[term]);
            }
            sw.Flush();
            sw.Close();
        }

        private static void LoadStopWordConfigFileInfo(out string indexpath, out string outputfilename)
        {
            StreamReader sr = new StreamReader("configStopWords.txt");
            string line;
            indexpath = null;
            outputfilename = null;
            while ((line = sr.ReadLine()) != null)
            {
                switch (line)
                {
                    case "IndexPath":
                        indexpath = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "OutputStopWordFileName":
                        outputfilename = sr.ReadLine();
                        sr.ReadLine();
                        break;
                }
            }
        }

        static Version version = Version.LUCENE_24;
        static Analyzer standardanalyzer = new StandardAnalyzer(version);

        /// <summary>
        ///  Generate a folder BingNewsData contains split BingNewsData.txt by timestamp
        /// </summary>
        protected static void GenerateTemporalBingNewsData()
        {
            String BingNewsFileName = EvolutionaryRoseTree.Constants.Constant.BING_NEWS;

            String bingnewsfile = Constant.DATA_PATH + BingNewsFileName;
            String outputfilepath = Constant.DATA_PATH + BingNewsFileName.Split('.')[0] + "\\";

            StreamReader sr = new StreamReader(bingnewsfile);
            Directory.CreateDirectory(outputfilepath);

            Dictionary<int, StreamWriter> ofiles = new Dictionary<int, StreamWriter>();
            Dictionary<int, int> temporal_linecnts = new Dictionary<int, int>();
            StreamWriter ofile;
            String linetext;
            int linecnt = 0, t_linecnt;
            while ((linetext = sr.ReadLine()) != null)
            {
                //Console.WriteLine(str);
                String[] tokens = linetext.Split('\t');
                int time = int.Parse(tokens[tokens.Length - 1]);

                if (ofiles.TryGetValue(time, out ofile))
                {
                    //write to file
                    ofile.WriteLine(linetext);
                    //update line cnts
                    temporal_linecnts.TryGetValue(time, out t_linecnt);
                    temporal_linecnts[time] = t_linecnt + 1;
                }
                else
                {
                    ofile = new StreamWriter(outputfilepath + time + ".txt");
                    ofile.WriteLine(linetext);
                    ofiles.Add(time, ofile);
                    temporal_linecnts.Add(time, 1);
                }

                if (++linecnt % 10000 == 0)
                    Console.WriteLine("line " + linecnt);
                linecnt++;
            }

            Console.WriteLine("Total lines:" + linecnt);
            sr.Close();
            List<int> timelist = ofiles.Keys.ToList();
            timelist.Sort();
            foreach (int time in timelist)
            {
                temporal_linecnts.TryGetValue(time, out t_linecnt);
                ofiles.TryGetValue(time, out ofile);
                Console.WriteLine(time + ":" + t_linecnt);
                ofile.Close();
            }
        }

        //Delete alt, misc, soc from 20NewsGroup data because they only constain one sub group
        protected static void Generate17NewsGroupData()
        {
            string datapath = Constant.DATA_PATH + @"nmidata\";
            string inputindexpath = datapath + @"textindex\";
            string outputindexpath = datapath + @"textindex_17groups\";


            LuceneDirectory inputdir = FSDirectory.Open(new DirectoryInfo(inputindexpath));
            IndexReader indexreader = IndexReader.Open(inputdir, true);

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputindexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            int docNum = indexreader.NumDocs();
            //ICollection<string> fields = indexreader.GetFieldNames(IndexReader.FieldOption.ALL);
            string newsgroupfield = "newsgroup";
            List<string> deletedgroup = (new string[] { "alt", "misc", "soc" }).ToList<string>();

            for (int i = 0; i < docNum; i++)
            {
                if (i % 1000 == 0)
                    Console.WriteLine("Process " + i + "th document!");
                Document document = indexreader.Document(i);
                string group = document.Get(newsgroupfield).Split('.')[0];

                //add to indexwriter
                if (!deletedgroup.Contains(group))
                    indexwriter.AddDocument(document);
            }

            indexwriter.Optimize();
            indexwriter.Close();
        }

        //Delete alt, misc, soc from 20NewsGroup data because they only constain one sub group
        protected static void GenerateSubGroupData()
        {
            string datapath = Constant.DATA_PATH + @"nmidata\";
            string inputindexpath = datapath + @"textindex_17groups\";
            string outputindexpath = datapath + @"textindex_s7groups\";


            LuceneDirectory inputdir = FSDirectory.Open(new DirectoryInfo(inputindexpath));
            IndexReader indexreader = IndexReader.Open(inputdir, true);

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputindexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            int docNum = indexreader.NumDocs();
            //ICollection<string> fields = indexreader.GetFieldNames(IndexReader.FieldOption.ALL);
            string newsgroupfield = "newsgroup";
            List<string> deletedgroup = (new string[] { "alt", "misc", "soc", "sci" }).ToList<string>();
            List<string> deletedsubgroup = (new string[] { "comp.sys.ibm.pc.hardware", 
                "comp.windows.x", "rec.autos", "rec.sport.baseball", "talk.politics.misc", "talk.religion.misc"}).ToList<string>();

            for (int i = 0; i < docNum; i++)
            {
                if (i % 1000 == 0)
                    Console.WriteLine("Process " + i + "th document!");
                Document document = indexreader.Document(i);
                string subgroup = document.Get(newsgroupfield);
                string group = subgroup.Split('.')[0];

                //add to indexwriter
                if (!deletedgroup.Contains(group) &&
                    !deletedsubgroup.Contains(subgroup))
                    indexwriter.AddDocument(document);
            }

            indexwriter.Optimize();
            indexwriter.Close();
        }

        //Extract bing news data from Weiwei's computer
        protected static void ParseBingNewsXMLData()
        {
            string keyword = "Microsoft";
            string[] selectedFields = new string[] { "DocumentURL", "Country", "NewsArticleCategoryData",
                "NewsArticleHeadline", "NewsArticleHeadNEMap", "NewsArticleDescription", "NewsArticleBodyNEMap", 
                "DiscoveryStringTime", "PublishedDateTime"};

            //input path
            string bingnewspath = @"\\weeny\News\";  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            //string extractpath = @"D:\Temp\BingNewsTempData\all\";

            //output path
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\BingNewsIndex\";
            string infofilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\info.dat";
            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(indexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            StreamWriter infofile = new StreamWriter(infofilename);

            string[] dates = Directory.GetDirectories(bingnewspath);
            dates = new string[] { dates[0] };
            int idate = 0;
            DateTime time_begin = DateTime.Now;
            foreach (string datedirectory in dates)
            {
                Console.WriteLine("//////////Processing {0} out of {1} dates, {2}%//////////", idate, dates.Length, 100.0 * idate / dates.Length);
                Console.WriteLine("=======================================================");

                int newsfoundcnt = 0;
                string[] filenames = Directory.GetFiles(datedirectory);

                int ifile = 0;
                int fileprocessratio = 1;
                DateTime time_begin_1 = DateTime.Now;
                foreach (string filename in filenames)
                {
                    if (100 * ifile / filenames.Length >= fileprocessratio)
                    {
                        Console.WriteLine(fileprocessratio + "%");
                        fileprocessratio++;
                        //Directory.Delete(extractpath, true);
                        //PrintProgress(ifile, filenames.Length, time_begin_1);
                    }

                    try
                    {
                        ZipFile zipfile = new ZipFile(filename);
                        foreach (ZipEntry entry in zipfile.Entries)
                        {
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            ms.Position = 0;
                            XmlDocument xmldoc = new XmlDocument();
                            xmldoc.Load(ms);

                            //entry.Extract(extractpath, ExtractExistingFileAction.OverwriteSilently);

                            ///// Parse XML file ///
                            //XmlDocument xmldoc = new XmlDocument();
                            //xmldoc.Load(extractpath + entry.FileName);

                            //XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleHeadNEMap");
                            XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleBodyNEMap");
                            foreach (XmlNode bodynemapnode in list)
                            {
                                string str = bodynemapnode.InnerText;
                                if (str.Contains(keyword))
                                {
                                    XmlNode newsnode = bodynemapnode.ParentNode;
                                    //Test whether it is written in english
                                    XmlNode languagenode = newsnode.SelectSingleNode("Language");
                                    if (languagenode.InnerText != "en")
                                        continue;
                                    //Extract all useful fields
                                    string docid = newsnode.Attributes[0].Value;
                                    Document document = new Document();
                                    document.Add(new Field("DocId", docid, Field.Store.YES, Field.Index.ANALYZED));
                                    foreach (string fieldname in selectedFields)
                                    {
                                        XmlNode node = newsnode.SelectSingleNode(fieldname);
                                        if (node != null)
                                            document.Add(new Field(fieldname, node.InnerText, Field.Store.YES, Field.Index.ANALYZED));
                                    }
                                    indexwriter.AddDocument(document);
                                    newsfoundcnt++;
                                }
                            }

                            ms.Dispose();
                            /// Delete temp file 
                            //File.Delete(extractpath + entry.FileName);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    ifile++;
                }

                infofile.WriteLine(datedirectory + ": " + newsfoundcnt);
                infofile.Flush();
                idate++;

                PrintProgress(idate, dates.Length, time_begin);
            }

            infofile.Flush();
            indexwriter.Optimize();
            indexwriter.Close();
        }


        //Extract bing news data from Weiwei's computer
        protected static void ParseBingNewsXMLData_Parallel(int iProcessor, int processorNum, int days)
        {
            string keyword = "Obama";
            string[] selectedFields = new string[] { "DocumentURL", "Country", "NewsArticleCategoryData",
                "NewsArticleHeadline", "NewsArticleHeadNEMap", "NewsArticleDescription", "NewsArticleBodyNEMap", 
                "DiscoveryStringTime", "PublishedDateTime"};

            //input path
            //string[] bingnewspaths = new string[] { @"\\weeny\News1\", @"\\weeny\News2\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            //string[] bingnewspaths = new string[] { @"E:\Data\News\", @"F:\Data\News\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            string[] bingnewspaths = new string[] { @"E:\Data\News\", @"F:\Data\News\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            //string[] bingnewspaths = new string[] { @" D:\Project\EvolutionaryRoseTreeData\Toy\"};  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";

            //string extractpath = @"D:\Temp\BingNewsTempData\" + iProcessor + "\\";

            //output path
            //string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\BingNewsIndex_" + iProcessor + "\\";
            //string infofilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\info_" + iProcessor + ".dat";
            string indexpath = @"D:\Temp\BingNewsIndex_" + iProcessor + "\\";
            string infofilename = @"D:\Temp\info_" + iProcessor + ".dat";
            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(indexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            StreamWriter infofile = new StreamWriter(infofilename);

            string[] dates_all = GetDirectories(bingnewspaths);
            dates_all = FilterDates(dates_all, "2012-01-26", "2012-07-26");
            days = Math.Min(dates_all.Length, days);
            string[] dates = new string[days];
            for (int i = 0; i < days; i++)
                dates[i] = dates_all[i];
            DateTime time_begin = DateTime.Now;

            for (int idate = 0; idate < dates.Length; idate++)
            {
                string datedirectory = dates[idate];
                Console.WriteLine("//////////Processing {0} out of {1} dates, {2}%//////////", idate, dates.Length, 100.0 * idate / dates.Length);
                Console.WriteLine("=======================================================");

                int newsfoundcnt = 0;
                string[] filenames = Directory.GetFiles(datedirectory);

                //int fileprocessratio = 1;
                DateTime time_begin_1 = DateTime.Now;
                for (int ifilename = iProcessor; ifilename < filenames.Length; ifilename += processorNum)
                {
                    string filename = filenames[ifilename];
                    //if (100 * ifilename / filenames.Length >= fileprocessratio)
                    //{
                    //    Console.WriteLine(fileprocessratio + "%");
                    //    fileprocessratio += 1;
                    //    //Directory.Delete(extractpath, true);
                    //    PrintProgress(ifilename, filenames.Length, time_begin_1);
                    //}

                    try
                    {
                        ZipFile zipfile = new ZipFile(filename);
                        foreach (ZipEntry entry in zipfile.Entries)
                        {
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            ms.Position = 0;
                            XmlDocument xmldoc = new XmlDocument();
                            xmldoc.Load(ms);

                            //entry.Extract(extractpath, ExtractExistingFileAction.OverwriteSilently);

                            ///// Parse XML file ///
                            //XmlDocument xmldoc = new XmlDocument();
                            //xmldoc.Load(extractpath + entry.FileName);

                            //XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleHeadNEMap");
                            XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleBodyNEMap");
                            foreach (XmlNode bodynemapnode in list)
                            {
                                string str = bodynemapnode.InnerText;
                                if (str.Contains(keyword))
                                {
                                    XmlNode newsnode = bodynemapnode.ParentNode;
                                    //Test whether it is written in english
                                    XmlNode languagenode = newsnode.SelectSingleNode("Language");
                                    if (languagenode.InnerText != "en")
                                        continue;
                                    //Extract all useful fields
                                    string docid = newsnode.Attributes[0].Value;
                                    Document document = new Document();
                                    document.Add(new Field("DocId", docid, Field.Store.YES, Field.Index.ANALYZED));
                                    foreach (string fieldname in selectedFields)
                                    {
                                        XmlNode node = newsnode.SelectSingleNode(fieldname);
                                        if (node != null)
                                            document.Add(new Field(fieldname, node.InnerText, Field.Store.YES, Field.Index.ANALYZED));
                                    }
                                    indexwriter.AddDocument(document);
                                    newsfoundcnt++;
                                }
                            }

                            ms.Dispose();
                            /// Delete temp file 
                            //File.Delete(extractpath + entry.FileName);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                infofile.WriteLine(datedirectory + ": " + newsfoundcnt);
                infofile.Flush();

                PrintProgress(idate + 1, dates.Length, time_begin);
            }

            infofile.Flush();
            indexwriter.Optimize();
            indexwriter.Close();
        }

        protected static void ParseBingNewsXMLData_Parallel_NotUnique_KeywordList()
        {
            //string keyword = "Obama";
            string[] selectedFields = new string[] { "DocumentURL", "DocumentUrl", "Country", "NewsArticleCategoryData",
                "NewsArticleHeadline", "NewsArticleDescription", 
                "DiscoveryStringTime", "PublishedDateTime",
                "DownloadStringTime", "PublishedDateTime", "NewsSource"};
                //"NewsArticleBodyNEMap", "NewsArticleHeadNEMap"};
            //string[] luceneFields = new string[] { "DocumentURL", "DocumentURL", "Country", "NewsArticleCategoryData",
            //    "NewsArticleHeadline", "NewsArticleHeadNEMap", "NewsArticleDescription", "NewsArticleBodyNEMap", 
            //    "DiscoveryStringTime", "PublishedDateTime"};

            //string RepeatTimesField = "RepeatTimes";

            ////input path
            ////string[] bingnewspaths = new string[] { @"\\weeny\News1\", @"\\weeny\News2\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            ////string[] bingnewspaths = new string[] { @"E:\Data\News\", @"F:\Data\News\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            //string[] bingnewspaths = new string[] { @"D:\Project\EvolutionaryRoseTreeData\Toy\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            ////string extractpath = @"D:\Temp\BingNewsTempData\" + iProcessor + "\\";

            ////output path
            ////string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\BingNewsIndex_" + iProcessor + "\\";
            ////string infofilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\info_" + iProcessor + ".dat";
            //string indexpath = @"D:\Temp\BingNewsIndex_" + iProcessor + "\\";

            //string startdate = "2012-01-26";
            //string enddate = "2012-07-26";

            string[] bingnewspaths;
            int iProcessor, processorNum;
            string startdate, enddate;
            List<string[]> keywordLists;
            List<string> indexpaths;
            List<string> languages;

            LoadExtractBingNewsDataConfig_KeyWordList(out bingnewspaths, 
                out iProcessor, out processorNum, out startdate, out enddate,
                out keywordLists, out languages, out indexpaths);

            List<string> outputdirs = new List<string>();
            List<string> infofilenames = new List<string>();
            int ikeyword = 0;
            foreach (string indexpath in indexpaths)
            {
                Directory.CreateDirectory(indexpath);
                infofilenames.Add(indexpath + "BingNews_" + keywordLists[ikeyword][0] + "_" + iProcessor + "_" + processorNum + ".dat");
                outputdirs.Add(indexpath + "BingNews_" + keywordLists[ikeyword][0] + "_" + iProcessor + "_" + processorNum);
                ikeyword++;
            }

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            List<IndexWriter> indexwriters = new List<IndexWriter>();
            List<StreamWriter> infofiles = new List<StreamWriter>();
            for (ikeyword = 0; ikeyword < keywordLists.Count; ikeyword++)
            {
                LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(indexpaths[ikeyword]));
                IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
                StreamWriter infofile = new StreamWriter(infofilenames[ikeyword]);
                indexwriters.Add(indexwriter);
                infofiles.Add(infofile);
            }

            string[] dates_all = GetDirectories(bingnewspaths);
            dates_all = FilterDates(dates_all, startdate, enddate);
            //days = Math.Min(dates_all.Length, days);
            int days = dates_all.Length;
            string[] dates = new string[days];
            for (int i = 0; i < days; i++)
                dates[i] = dates_all[i];
            DateTime time_begin = DateTime.Now;

            //Dictionary<string, int> UniqueDocumentNumberHash = new Dictionary<string, int>();
            //Dictionary<string, Document> UniqueDocumentHash = new Dictionary<string, Document>();

            //HashSet<string> uniqueHashValues = new HashSet<string>();
            for (int idate = 0; idate < dates.Length; idate++)
            {
                string datedirectory = dates[idate];
                Console.WriteLine("//////////Processing {0} out of {1} dates, {2}%//////////", idate, dates.Length, 100.0 * idate / dates.Length);
                Console.WriteLine("=======================================================");

                int[] newsfoundcnts = new int[keywordLists.Count];
                string[] filenames = Directory.GetFiles(datedirectory);

                //int fileprocessratio = 1;
                DateTime time_begin_1 = DateTime.Now;
                for (int ifilename = iProcessor; ifilename < filenames.Length; ifilename += processorNum)
                {
                    string filename = filenames[ifilename];
                    //if (100 * ifilename / filenames.Length >= fileprocessratio)
                    //{
                    //    Console.WriteLine(fileprocessratio + "%");
                    //    fileprocessratio += 1;
                    //    //Directory.Delete(extractpath, true);
                    //    PrintProgress(ifilename, filenames.Length, time_begin_1);
                    //}

                    try
                    {
                        ZipFile zipfile = null;
                        List<XmlDocument> xmldocs = new List<XmlDocument>();
                        if (filename.EndsWith(".zip"))
                        {
                            zipfile = new ZipFile(filename);
                            MemoryStream ms = new MemoryStream();
                            foreach (ZipEntry entry in zipfile.Entries)
                            {
                                entry.Extract(ms);
                                ms.Position = 0;
                                XmlDocument xmldoc = new XmlDocument();
                                xmldoc.Load(ms);
                                xmldocs.Add(xmldoc);
                                ms.Dispose();
                            }
                        }
                        else
                        {
                            try
                            {
                                XmlDocument xmldoc = new XmlDocument();
                                xmldoc.Load(filename);
                                xmldocs.Add(xmldoc);
                            }
                            catch
                            {
                                var xmldoclist = GetXMLDocList(filename);
                                xmldocs.AddRange(xmldoclist);
                            }
                        }
                        foreach (XmlDocument xmldoc in xmldocs)
                        {
                            //entry.Extract(extractpath, ExtractExistingFileAction.OverwriteSilently);

                            ///// Parse XML file ///
                            //XmlDocument xmldoc = new XmlDocument();
                            //xmldoc.Load(extractpath + entry.FileName);

                            //XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleHeadNEMap");
                            XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleDescription");
                            foreach (XmlNode bodynemapnode in list)
                            {
                                for (ikeyword = 0; ikeyword < keywordLists.Count; ikeyword++)
                                {
                                    var keywords = keywordLists[ikeyword];
                                    IndexWriter indexwriter = indexwriters[ikeyword];

                                    string str = bodynemapnode.InnerText;
                                    bool bStore = false;
                                    foreach (var keyword in keywords)
                                        if (str.Contains(keyword))
                                        {
                                            bStore = true;
                                            break;
                                        }
                                    
                                    if(bStore)
                                    {
                                        XmlNode newsnode = bodynemapnode.ParentNode;
                                        XmlNode languagenode = newsnode.SelectSingleNode("Language");
                                        //Test whether it is written in english
                                        if (!languages.Contains(languagenode.InnerText))
                                            continue;

                                        //string discovertime = newsnode.SelectSingleNode("DiscoveryStringTime").InnerText;
                                        //XmlNode title = newsnode.SelectSingleNode("NewsArticleHeadline");
                                        //string hashvalue = discovertime.Substring(0, 10) + "_" + title.InnerText;
                                        //if (uniqueHashValues.Contains(hashvalue))
                                        //    continue;

                                        /// Unique Document ///
                                        //Extract all useful fields
                                        string docid = newsnode.Attributes[0].Value;
                                        Document document = new Document();
                                        document.Add(new Field("DocId", docid, Field.Store.YES, Field.Index.ANALYZED));
                                        foreach (string fieldname in selectedFields)
                                        {
                                            XmlNode node = newsnode.SelectSingleNode(fieldname);
                                            if (node != null)
                                            {
                                                string luceneFieldName = fieldname;
                                                if (luceneFieldName == "DocumentUrl")
                                                    luceneFieldName = "DocumentURL";
                                                document.Add(new Field(luceneFieldName, node.InnerText, Field.Store.YES, Field.Index.ANALYZED));
                                            }
                                        }
                                        indexwriter.AddDocument(document);
                                        newsfoundcnts[ikeyword]++;

                                    }
                                }
                            }

                            /// Delete temp file 
                            //File.Delete(extractpath + entry.FileName);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                for (ikeyword = 0; ikeyword < keywordLists.Count; ikeyword++)
                {
                    infofiles[ikeyword].WriteLine(datedirectory + ": " + newsfoundcnts[ikeyword]);
                    infofiles[ikeyword].Flush();
                }

                PrintProgress(idate + 1, dates.Length, time_begin);
            }

            Console.WriteLine("Start writing to lucene index...");

            //IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            //foreach (string hashvalue in UniqueDocumentHash.Keys)
            //{
            //    Document document = UniqueDocumentHash[hashvalue];
            //    int repeatTimes = UniqueDocumentNumberHash[hashvalue];
            //    document.Add(new Field(RepeatTimesField, repeatTimes+"", Field.Store.YES, Field.Index.ANALYZED));
            //    indexwriter.AddDocument(document);
            //}

            for (ikeyword = 0; ikeyword < keywordLists.Count; ikeyword++)
            {
                infofiles[ikeyword].Flush();
                indexwriters[ikeyword].Optimize();
                indexwriters[ikeyword].Close();
            }
        }


        protected static void TestBingNewsXMLData_Parallel_NotUnique_KeywordList()
        {
            //string keyword = "Obama";
            string[] selectedFields = new string[] { "DocumentURL", "DocumentUrl", "Country", "NewsArticleCategoryData",
                "NewsArticleHeadline", "NewsArticleDescription", 
                "DiscoveryStringTime", "PublishedDateTime",
                "DownloadStringTime", "PublishedDateTime", "NewsSource"};

            string[] bingnewspaths;
            int iProcessor, processorNum;
            string startdate, enddate;
            List<string[]> keywords;
            List<string> indexpaths;
            List<string> languages;

            LoadExtractBingNewsDataConfig_KeyWordList(out bingnewspaths,
                out iProcessor, out processorNum, out startdate, out enddate,
                out keywords, out languages, out indexpaths);

            List<string> outputdirs = new List<string>();
            List<string> infofilenames = new List<string>();
            int ikeyword = 0;
            foreach (string indexpath in indexpaths)
            {
                Directory.CreateDirectory(indexpath);
                infofilenames.Add(indexpath + "BingNews_" + keywords[ikeyword] + "_" + iProcessor + "_" + processorNum + ".dat");
                outputdirs.Add(indexpath + "BingNews_" + keywords[ikeyword] + "_" + iProcessor + "_" + processorNum);
                ikeyword++;
            }

            StreamWriter sw = new StreamWriter("infofile.dat");

            //Version version = Version.LUCENE_24;
            //Analyzer standardanalyzer = new StandardAnalyzer(version);
            //List<IndexWriter> indexwriters = new List<IndexWriter>();
            //List<StreamWriter> infofiles = new List<StreamWriter>();
            //for (ikeyword = 0; ikeyword < keywords.Count; ikeyword++)
            //{
            //    LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(indexpaths[ikeyword]));
            //    IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            //    StreamWriter infofile = new StreamWriter(infofilenames[ikeyword]);
            //    indexwriters.Add(indexwriter);
            //    infofiles.Add(infofile);
            //}

            string[] dates_all = GetDirectories(bingnewspaths);
            dates_all = FilterDates(dates_all, startdate, enddate);
            //days = Math.Min(dates_all.Length, days);
            int days = dates_all.Length;
            string[] dates = new string[days];
            for (int i = 0; i < days; i++)
                dates[i] = dates_all[i];
            DateTime time_begin = DateTime.Now;

            //Dictionary<string, int> UniqueDocumentNumberHash = new Dictionary<string, int>();
            //Dictionary<string, Document> UniqueDocumentHash = new Dictionary<string, Document>();

            //HashSet<string> uniqueHashValues = new HashSet<string>();
            HashSet<string> chineseNewsLanguages = new HashSet<string>();
            int sum_cnt_overall_docs = 0;
            int sum_cnt_chinese_docs = 0;
                
            for (int idate = 0; idate < dates.Length; idate++)
            {
                int cnt_overall_docs = 0;
                int cnt_chinese_docs = 0;
                //int cnt_Baidu_docs = 0;
                //int cnt_Amazon_docs = 0;
                //int cnt_Tencent_docs = 0;
                //int cnt_yahoo_docs = 0;

                string datedirectory = dates[idate];
                Console.WriteLine("//////////Processing {0} out of {1} dates, {2}%//////////", idate, dates.Length, 100.0 * idate / dates.Length);
                Console.WriteLine("=======================================================");

                int[] newsfoundcnts = new int[keywords.Count];
                string[] filenames = Directory.GetFiles(datedirectory);

                //int fileprocessratio = 1;
                DateTime time_begin_1 = DateTime.Now;
                for (int ifilename = iProcessor; ifilename < filenames.Length; ifilename += processorNum)
                {
                    string filename = filenames[ifilename];

                    try
                    {
                        ZipFile zipfile = null;
                        List<XmlDocument> xmldocs = new List<XmlDocument>();
                        if (filename.EndsWith(".zip"))
                        {
                            zipfile = new ZipFile(filename);
                            MemoryStream ms = new MemoryStream();
                            foreach (ZipEntry entry in zipfile.Entries)
                            {
                                entry.Extract(ms);
                                ms.Position = 0;
                                XmlDocument xmldoc = new XmlDocument();
                                xmldoc.Load(ms);
                                xmldocs.Add(xmldoc);
                                ms.Dispose();
                            }
                        }
                        else
                        {
                            try
                            {
                                XmlDocument xmldoc = new XmlDocument();
                                xmldoc.Load(filename);
                                xmldocs.Add(xmldoc);
                            }
                            catch
                            {
                                var xmldoclist = GetXMLDocList(filename);
                                xmldocs.AddRange(xmldoclist);
                            }
                        }
                        foreach (XmlDocument xmldoc in xmldocs)
                        {
                            //entry.Extract(extractpath, ExtractExistingFileAction.OverwriteSilently);

                            ///// Parse XML file ///
                            XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleDescription");
                            foreach (XmlNode bodynemapnode in list)
                            {
                                cnt_overall_docs++;

                                string str = bodynemapnode.InnerText;
                                if (str.Contains("中") || str.Contains("的") || str.Contains("地"))
                                {
                                    cnt_chinese_docs++;
                                    
                                    XmlNode newsnode = bodynemapnode.ParentNode;
                                    XmlNode languagenode = newsnode.SelectSingleNode("Language");
                                    chineseNewsLanguages.Add(languagenode.InnerText);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                sw.WriteLine("{0}\t{1}\t{2}", datedirectory, cnt_overall_docs, cnt_chinese_docs);
                sw.Flush();

                sum_cnt_overall_docs += cnt_overall_docs;
                sum_cnt_chinese_docs += cnt_chinese_docs;

                PrintProgress(idate + 1, dates.Length, time_begin);
            }

            sw.WriteLine("Overall\t{0}\t{1}", sum_cnt_overall_docs, sum_cnt_chinese_docs);
            foreach (var language in chineseNewsLanguages)
                sw.Write(language + "\t");
            sw.Flush();
            sw.Close();
        }

        private static List<XmlDocument> GetXMLDocList(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            string line;
            List<XmlDocument> xmldoclist = new List<XmlDocument>();
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Contains("<News ID="))
                {
                    string newsBody = line;
                    while (!(line = sr.ReadLine()).Contains("</News>"))
                        newsBody += "\n" + line;
                    newsBody += "\n\t</News>\n";
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(newsBody);
                    xmldoclist.Add(xmldoc);
                }
            }
            return xmldoclist;
        }

        //protected static void ParseBingNewsXMLData_Parallel_Unique(int iProcessor, int processorNum, int days)
        protected static void ParseBingNewsXMLData_Parallel_Unique()
        {
            //string keyword = "Obama";
            string[] selectedFields = new string[] { "DocumentURL", "Country", "NewsArticleCategoryData",
                "NewsArticleHeadline", "NewsArticleHeadNEMap", "NewsArticleDescription", "NewsArticleBodyNEMap", 
                "DiscoveryStringTime", "PublishedDateTime"};

            //string RepeatTimesField = "RepeatTimes";

            ////input path
            ////string[] bingnewspaths = new string[] { @"\\weeny\News1\", @"\\weeny\News2\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            ////string[] bingnewspaths = new string[] { @"E:\Data\News\", @"F:\Data\News\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            //string[] bingnewspaths = new string[] { @"D:\Project\EvolutionaryRoseTreeData\Toy\" };  //@"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\";
            ////string extractpath = @"D:\Temp\BingNewsTempData\" + iProcessor + "\\";

            ////output path
            ////string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\BingNewsIndex_" + iProcessor + "\\";
            ////string infofilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\info_" + iProcessor + ".dat";
            //string indexpath = @"D:\Temp\BingNewsIndex_" + iProcessor + "\\";

            //string startdate = "2012-01-26";
            //string enddate = "2012-07-26";

            string[] bingnewspaths;
            string indexpath;
            int iProcessor, processorNum;
            string startdate, enddate;
            string keyword;

            LoadExtractBingNewsDataConfig(out bingnewspaths, out indexpath,
                out iProcessor, out processorNum, out startdate, out enddate,
                out keyword);

            Directory.CreateDirectory(indexpath);
            string infofilename = indexpath + "BingNews_" + keyword + "_" + iProcessor + "_" + processorNum + ".dat";
            indexpath = indexpath + "BingNews_" + keyword + "_" + iProcessor + "_" + processorNum;

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(indexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            StreamWriter infofile = new StreamWriter(infofilename);

            string[] dates_all = GetDirectories(bingnewspaths);
            dates_all = FilterDates(dates_all, startdate, enddate);
            //days = Math.Min(dates_all.Length, days);
            int days = dates_all.Length;
            string[] dates = new string[days];
            for (int i = 0; i < days; i++)
                dates[i] = dates_all[i];
            DateTime time_begin = DateTime.Now;

            //Dictionary<string, int> UniqueDocumentNumberHash = new Dictionary<string, int>();
            //Dictionary<string, Document> UniqueDocumentHash = new Dictionary<string, Document>();

            HashSet<string> uniqueHashValues = new HashSet<string>();
            for (int idate = 0; idate < dates.Length; idate++)
            {
                string datedirectory = dates[idate];
                Console.WriteLine("//////////Processing {0} out of {1} dates, {2}%//////////", idate, dates.Length, 100.0 * idate / dates.Length);
                Console.WriteLine("=======================================================");

                int newsfoundcnt = 0;
                string[] filenames = Directory.GetFiles(datedirectory);

                //int fileprocessratio = 1;
                DateTime time_begin_1 = DateTime.Now;
                for (int ifilename = iProcessor; ifilename < filenames.Length; ifilename += processorNum)
                {
                    string filename = filenames[ifilename];
                    //if (100 * ifilename / filenames.Length >= fileprocessratio)
                    //{
                    //    Console.WriteLine(fileprocessratio + "%");
                    //    fileprocessratio += 1;
                    //    //Directory.Delete(extractpath, true);
                    //    PrintProgress(ifilename, filenames.Length, time_begin_1);
                    //}

                    try
                    {
                        ZipFile zipfile = new ZipFile(filename);
                        foreach (ZipEntry entry in zipfile.Entries)
                        {
                            MemoryStream ms = new MemoryStream();
                            entry.Extract(ms);
                            ms.Position = 0;
                            XmlDocument xmldoc = new XmlDocument();
                            xmldoc.Load(ms);

                            //entry.Extract(extractpath, ExtractExistingFileAction.OverwriteSilently);

                            ///// Parse XML file ///
                            //XmlDocument xmldoc = new XmlDocument();
                            //xmldoc.Load(extractpath + entry.FileName);

                            //XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleHeadNEMap");
                            XmlNodeList list = xmldoc.GetElementsByTagName("NewsArticleBodyNEMap");
                            foreach (XmlNode bodynemapnode in list)
                            {
                                string str = bodynemapnode.InnerText;
                                if (str.Contains(keyword))
                                {
                                    XmlNode newsnode = bodynemapnode.ParentNode;
                                    XmlNode languagenode = newsnode.SelectSingleNode("Language");
                                    //Test whether it is written in english
                                    if (languagenode.InnerText != "en")
                                        continue;

                                    string discovertime = newsnode.SelectSingleNode("DiscoveryStringTime").InnerText;
                                    XmlNode title = newsnode.SelectSingleNode("NewsArticleHeadline");
                                    string hashvalue = discovertime.Substring(0, 10) + "_" + title.InnerText;
                                    if (uniqueHashValues.Contains(hashvalue))
                                        continue;

                                    /// Unique Document ///
                                    //Extract all useful fields
                                    string docid = newsnode.Attributes[0].Value;
                                    Document document = new Document();
                                    document.Add(new Field("DocId", docid, Field.Store.YES, Field.Index.ANALYZED));
                                    foreach (string fieldname in selectedFields)
                                    {
                                        XmlNode node = newsnode.SelectSingleNode(fieldname);
                                        if (node != null)
                                            document.Add(new Field(fieldname, node.InnerText, Field.Store.YES, Field.Index.ANALYZED));
                                    }
                                    indexwriter.AddDocument(document);
                                    newsfoundcnt++;

                                    uniqueHashValues.Add(hashvalue);
                                }
                            }

                            ms.Dispose();
                            /// Delete temp file 
                            //File.Delete(extractpath + entry.FileName);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                infofile.WriteLine(datedirectory + ": " + newsfoundcnt);
                infofile.Flush();

                PrintProgress(idate + 1, dates.Length, time_begin);
            }

            Console.WriteLine("Start writing to lucene index...");

            //IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            //foreach (string hashvalue in UniqueDocumentHash.Keys)
            //{
            //    Document document = UniqueDocumentHash[hashvalue];
            //    int repeatTimes = UniqueDocumentNumberHash[hashvalue];
            //    document.Add(new Field(RepeatTimesField, repeatTimes+"", Field.Store.YES, Field.Index.ANALYZED));
            //    indexwriter.AddDocument(document);
            //}

            infofile.Flush();
            indexwriter.Optimize();
            indexwriter.Close();
        }

        private static void LoadExtractBingNewsDataConfig_KeyWordList(out string[] bingnewspaths, 
            out int iProcessor, out int processorNum, out string startdate, out string enddate, 
            out List<string[]> keywordLists, out List<string> languages, out List<string> indexpaths)
        {
            StreamReader sr = null;
            bingnewspaths = null;
            string indexpath = null;
            iProcessor = 0;
            processorNum = 1;
            startdate = "2012-01-06";
            enddate = "2012-07-06";
            string keyword = "Obama";
            string line;
            keywordLists = new List<string[]>();
            languages = new List<string>();
            indexpaths = new List<string>();

            try
            {
                sr = new StreamReader("config.txt");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                Console.WriteLine("Press key to continue");
                Console.ReadKey();
                return;
            }
            string str;

            int readinfo = 0;

            while ((str = sr.ReadLine()) != null)
            {
                switch (str)
                {
                    case "BingNewsPaths":
                        List<string> bingnewspathlist = new List<string>();
                        while ((str = sr.ReadLine()).Length != 0)
                            bingnewspathlist.Add(str);
                        bingnewspaths = bingnewspathlist.ToArray();
                        readinfo++;
                        break;
                    case "OutPutIndexPath":
                        indexpaths.Add(sr.ReadLine());
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "iProcessor":
                        iProcessor = int.Parse(sr.ReadLine());
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "ProcessorNumber":
                        processorNum = int.Parse(sr.ReadLine());
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "StartDate":
                        startdate = sr.ReadLine();
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "EndDate":
                        enddate = sr.ReadLine();
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "KeyWord":
                        keywordLists.Add(new string[] { sr.ReadLine() });
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "KeyWords":
                        while (true)
                        {
                            keyword = sr.ReadLine();
                            if (keyword.Length == 0)
                                break;
                            keywordLists.Add(keyword.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries));
                        }
                        readinfo++;
                        break;
                    case "AcceptedLanguages":
                        while (true)
                        {
                            line = sr.ReadLine();
                            if (line.Length == 0)
                                break;
                            languages.Add(line);
                        }
                        readinfo++;
                        break;
                    case "OutPutIndexPaths":
                        while (true)
                        {
                            indexpath = sr.ReadLine();
                            if (indexpath.Length == 0)
                                break;
                            indexpaths.Add(indexpath);
                        }
                        readinfo++;
                        break;
                    default:
                        throw new Exception("Unkown variable in config file!");

                }
            }

            if (readinfo != 8 || keywordLists.Count != indexpaths.Count)
            {
                Console.WriteLine("[Warning] Some information is absent from configure file, using defaults");
            }

            sr.Close();
        }

        private static void LoadExtractBingNewsDataConfig(out string[] bingnewspaths, out string indexpath, out int iProcessor, out int processorNum, out string startdate, out string enddate, out string keyword)
        {
            StreamReader sr = null;
            bingnewspaths = null;
            indexpath = null;
            iProcessor = 0;
            processorNum = 1;
            startdate = "2012-01-06";
            enddate = "2012-07-06";
            keyword = "Obama";

            try
            {
                sr = new StreamReader("config.txt");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Write(e.StackTrace);
                Console.WriteLine("Press key to continue");
                Console.ReadKey();
                return;
            }
            string str;

            int readinfo = 0;

            while ((str = sr.ReadLine()) != null)
            {
                switch (str)
                {
                    case "BingNewsPaths":
                        List<string> bingnewspathlist = new List<string>();
                        while ((str = sr.ReadLine()).Length != 0)
                            bingnewspathlist.Add(str);
                        bingnewspaths = bingnewspathlist.ToArray();
                        readinfo++;
                        break;
                    case "OutPutIndexPath":
                        indexpath = sr.ReadLine();
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "iProcessor":
                        iProcessor = int.Parse(sr.ReadLine());
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "ProcessorNumber":
                        processorNum = int.Parse(sr.ReadLine());
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "StartDate":
                        startdate = sr.ReadLine();
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "EndDate":
                        enddate = sr.ReadLine();
                        sr.ReadLine();
                        readinfo++;
                        break;
                    case "KeyWord":
                        keyword = sr.ReadLine();
                        sr.ReadLine();
                        readinfo++;
                        break;
                    default:
                        throw new Exception("Unkown variable in config file!");

                }
            }

            if (readinfo != 7)
            {
                Console.WriteLine("[Warning] Some information is absent from configure file, using defaults");
            }

            sr.Close();
        }

        static void PrintProgress(int experimentIndex, int experimentNumber, DateTime time_begin)
        {
            DateTime now = DateTime.Now;
            double avgTime = (now.Ticks - time_begin.Ticks) / 1e7 / experimentIndex;
            double formatedAvgTime = Math.Floor(1000 * avgTime) / 1000;
            Console.WriteLine("==========Finish experiments {0} out of {1}, avg time {2}s==========", experimentIndex, experimentNumber, formatedAvgTime);
            double remainingTime = avgTime * (experimentNumber - experimentIndex);
            double hours, minutes, seconds;
            TuneParameterExperiments.GetHourMinuteSecond(remainingTime, out hours, out minutes, out seconds);
            seconds = Math.Floor(seconds);
            if (hours > 0)
                Console.WriteLine("==========Remaining: {0} hours, {1} minutes, {2} seconds ==========", hours, minutes, seconds);
            else if (minutes > 0)
                Console.WriteLine("==========Remaining: {0} minutes, {1} seconds ==========", minutes, seconds);
            else
                Console.WriteLine("==========Remaining: {0} seconds ==========", seconds);
        }


        private static string[] GetDirectories(string[] paths)
        {
            List<string> folders = new List<string>();
            foreach (string path in paths)
                folders.AddRange(Directory.GetDirectories(path));
            return folders.ToArray();
        }

        private static string[] FilterDates(string[] dates, string startDate, string endDate)
        {
            DateTime startDateTime = EvolutionaryExperiments.GetDateTime(startDate);
            DateTime endDateTime = EvolutionaryExperiments.GetDateTime(endDate);

            List<string> selectedDates = new List<string>();
            List<string> pureDates = new List<string>();
            Dictionary<string, string> puredateToDate = new Dictionary<string, string>();
            foreach (string date in dates)
            {
                try
                {
                    string date_pure = date.Substring(date.Length - 10, 10);
                    DateTime dateTime = EvolutionaryExperiments.GetDateTime(date_pure);
                    if (dateTime.Ticks >= startDateTime.Ticks &&
                        dateTime.Ticks <= endDateTime.Ticks)
                    {
                        //selectedDates.Add(date);
                        if (puredateToDate.ContainsKey(date_pure))
                            throw new Exception("Duplicate Dates!");
                        puredateToDate.Add(date_pure, date);
                        pureDates.Add(date_pure);
                    }
                }
                catch
                {
                }
            }

            pureDates.Sort();
            foreach (string puredate in pureDates)
                selectedDates.Add(puredateToDate[puredate]);
            return selectedDates.ToArray();
        }

        protected static void MergeParallelProcessedBingNewsIndex()
        {
            string datapath = Constant.DATA_PATH + @"BingNewsData_2012\";
            string outputindexpath = datapath + @"BingNewsIndexMerged\";
            //input paths
            string inputpathhead = datapath + @"BingNewsIndex_";
            int num = 7;
            string[] inputpaths = new string[num];
            for (int i = 0; i < num; i++)
                inputpaths[i] = inputpathhead + i;

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputindexpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            //indexwriter.SetMergeFactor(1000);
            //indexwriter.SetMaxFieldLength(int.MaxValue);
            //indexwriter.SetMaxBufferedDocs(int.MaxValue);
            //indexwriter.SetMaxMergeDocs(int.MaxValue);

            FSDirectory[] fs = new FSDirectory[num];//FSDirectory.Open(new DirectoryInfo(inputindexpath));
            for (int i = 0; i < num; i++)
                fs[i] = FSDirectory.Open(new DirectoryInfo(inputpaths[i]));

            Console.WriteLine("Start merging...");

            indexwriter.AddIndexesNoOptimize(fs);

            indexwriter.Optimize();
            indexwriter.Close();
        }

        protected static void MergeParallelProcessedBingNewsIndex_DocumentByDocument()
        {
            //string inputindexpath = @"\\weeny\Temp\BingNews_Microsoft\";
            string inputindexpath = @"D:\HierarchicalTextflow\data\lucene\BingNewsIndex_Obama_sixmonths_body";
            string outputindexpath = inputindexpath + @"\BingNews_Obama_Merged\";
            //input paths
            string inputpathhead = inputindexpath + @"\BingNews_Obama_";
            int num = 6;
            string[] inputpaths = new string[num];
            for (int i = 0; i < num; i++)
                inputpaths[i] = inputpathhead + i + "_" + num;

            string RepeatTimesField = "RepeatTimes";

            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputindexpath));

            Dictionary<string, int> UniqueDocumentNumberHash = new Dictionary<string, int>();
            Dictionary<string, Document> UniqueDocumentHash = new Dictionary<string, Document>();
            string dateField = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.DiscoveryStringTime;
            string titleField = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;

            DateTime dateTime = DateTime.Now;
            for (int iPath = 0; iPath < inputpaths.Length; iPath++)
            {
                string inputpath = inputpaths[iPath];
                IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputpath)), true)).GetIndexReader();
                int docprocessratio = 1;
                int docNums = indexreader.NumDocs();
                Console.WriteLine("InputIndex:" + inputpath);
                Console.WriteLine("DocNumber: " + docNums);
                DateTime time_begin_1 = DateTime.Now;
                for (int iDoc = 0; iDoc < docNums; iDoc++)
                {
                    if (100 * iDoc / docNums >= docprocessratio)
                    {
                        Console.WriteLine(docprocessratio + "%");
                        docprocessratio += 1;
                        PrintProgress(iDoc, docNums, time_begin_1);
                    }

                    Document document = indexreader.Document(iDoc);
                    string date = document.Get(dateField).Substring(0, 10);
                    string title = document.Get(titleField);
                    string hashvalue = date + "_" + title;
                    if (UniqueDocumentNumberHash.ContainsKey(hashvalue))
                    {
                        UniqueDocumentNumberHash[hashvalue]++;
                    }
                    else
                    {
                        UniqueDocumentNumberHash.Add(hashvalue, 1);
                        UniqueDocumentHash.Add(hashvalue, document);
                    }
                }
                PrintProgress(iPath + 1, inputpaths.Length, dateTime);
            }

            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            foreach (string hashvalue in UniqueDocumentHash.Keys)
            {
                Document document = UniqueDocumentHash[hashvalue];
                int repeatTimes = UniqueDocumentNumberHash[hashvalue];
                document.Add(new Field(RepeatTimesField, repeatTimes + "", Field.Store.YES, Field.Index.ANALYZED));
                indexwriter.AddDocument(document);
                //Console.WriteLine(hashvalue);
            }

            indexwriter.Optimize();
            indexwriter.Close();
        }

        protected static void LoadMergeIndexConfigFile(out string[] inputpaths, out string outputindexpath)
        {
            List<string> inputpathlist = new List<string>();
            string line;
            outputindexpath = null; 
            StreamReader sr = new StreamReader("configMergeIndex.txt");
            while ((line = sr.ReadLine()) != null)
            {
                switch (line)
                {
                    case "InputPaths":
                        while ((line = sr.ReadLine()).Length != 0)
                            inputpathlist.Add(line);
                        break;
                    case "OutputPath":
                        outputindexpath = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    default:
                        throw new Exception("Unkown variable in config file!");
                }
            }

            inputpaths = inputpathlist.ToArray<string>();
        }

        protected static void MergeParallelProcessedBingNewsIndex_DocumentByDocument_NoRepeatTimes()
        {
            //string inputindexpath = @"\\weeny\Temp\BingNews_Microsoft\";
            //string inputindexpath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\debt_crisis2";
            //string inputpathhead = inputindexpath + @"\BingNews_debt_crisis_Filtered_";

            //string outputindexpath = inputindexpath + @"\BingNews_debt_crisis_Filtered_Merged\";
            //int num = 2;
            //string[] inputpaths = new string[num];
            //for (int i = 0; i < num; i++)
            //    inputpaths[i] = inputpathhead + i + "_" + num;

            string[] inputpaths;
            string outputindexpath;
            LoadMergeIndexConfigFile(out inputpaths, out outputindexpath);


            Version version = Version.LUCENE_24;
            Analyzer standardanalyzer = new StandardAnalyzer(version);
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputindexpath));

            HashSet<string> UniqueDocumentHash = new HashSet<string>();
            string dateField = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.DiscoveryStringTime;
            string titleField = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;

            DateTime dateTime = DateTime.Now;
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            for (int iPath = 0; iPath < inputpaths.Length; iPath++)
            {
                string inputpath = inputpaths[iPath];
                IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputpath)), true)).GetIndexReader();
                int docprocessratio = 1;
                int docNums = indexreader.NumDocs();
                Console.WriteLine("InputIndex:" + inputpath);
                Console.WriteLine("DocNumber: " + docNums);
                DateTime time_begin_1 = DateTime.Now;
                for (int iDoc = 0; iDoc < docNums; iDoc++)
                {
                    if (100 * iDoc / docNums >= docprocessratio)
                    {
                        Console.WriteLine(docprocessratio + "%");
                        docprocessratio += 1;
                        PrintProgress(iDoc, docNums, time_begin_1);
                    }

                    Document document = indexreader.Document(iDoc);
                    string date = document.Get(dateField).Substring(0, 10);
                    string title = document.Get(titleField);
                    string hashvalue = date + "_" + title;
                    if (!UniqueDocumentHash.Contains(hashvalue))
                    {
                        indexwriter.AddDocument(document);
                        UniqueDocumentHash.Add(hashvalue);
                    }
                }
                PrintProgress(iPath + 1, inputpaths.Length, dateTime);
            }

            indexwriter.Optimize();
            indexwriter.Close();
        }

        public static void RemoveDuplicateDocuments()
        {
            string inputlucenepath, outputlucene;
            try
            {
                StreamReader sw = new StreamReader("configRemoveDuplicate.txt");
                inputlucenepath = sw.ReadLine();
                outputlucene = sw.ReadLine();
            }
            catch
            {
                inputlucenepath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_Obama\BingNews_Obama_Sep_4months";
                outputlucene = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_Obama\BingNews_Obama_Sep_4months2";
            }

            string titlefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;

            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputlucenepath)), true)).GetIndexReader();
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputlucene));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            HashSet<string> titleHash = new HashSet<string>();
            int docNums = indexreader.NumDocs();
            Console.WriteLine("Total {0} docs", docNums);

            int removeDocNum = 0;
            for (int iDoc = 0; iDoc < docNums; iDoc++)
            {
                if (iDoc % 10000 == 0)
                    Console.WriteLine("Processing {0} doc", iDoc);

                Document document = indexreader.Document(iDoc);
                string title = document.Get(titlefield).ToLower();
                if (!titleHash.Contains(title))
                {
                    indexwriter.AddDocument(document);
                    titleHash.Add(title);
                }
                else
                    removeDocNum++;
            }

            indexwriter.Optimize();
            indexwriter.Close();

            Console.WriteLine("Finished. Removed {0} out of {1}", removeDocNum, docNums);
            Console.ReadKey();
        }

        #region remove similar documents
        public static void LoadConfigFileRemoveSimilarDocuments(out string inputlucenepath, out string outputlucene,
            out string tempfolder, out double removeSimilarity, out List<string> additionalstopwords, out bool bRemoveSameURL)
        {
            StreamReader sr = new StreamReader("configRemoveSimilar.txt");
            string line;
            inputlucenepath = null;
            outputlucene = null;
            tempfolder = null;
            removeSimilarity = 0.9;
            additionalstopwords = new List<string>();
            bRemoveSameURL = true;

            while ((line = sr.ReadLine()) != null)
            {
                switch (line)
                {
                    case "InputPath":
                        inputlucenepath = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "OutputPath":
                        outputlucene = sr.ReadLine();
                        sr.ReadLine();
                        break;
                    case "TempFolder":
                        tempfolder = sr.ReadLine() + "\\";
                        sr.ReadLine();
                        break;
                    case "RemoveThreshold":
                        removeSimilarity = double.Parse(sr.ReadLine());
                        sr.ReadLine();
                        break;
                    case "AdditionalStopWordList":
                        while ((line = sr.ReadLine()) != null && line.Length != 0)
                            additionalstopwords.Add(line);
                        sr.ReadLine();
                        break;
                    case "RemoveSameURL":
                        bRemoveSameURL = bool.Parse(sr.ReadLine());
                        sr.ReadLine();
                        break;
                    default:
                        throw new Exception("Unkown variable in config file!");
                }
            }
        }
        
        public static void RemoveSimilarDocuments()
        {
            //string inputlucenepath = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\debt_crisis2\BingNews_debt_crisis_Filtered_Merged";
            //string outputlucene = @"D:\Project\EvolutionaryRoseTreeData\data\BingNewsData\debt_crisis2\BingNews_debt_crisis_Filtered_Merged_RemoveSimilar";
            //string tempfolder = @"D:\Temp\";
            //double removeSimilarity = 0.9;

            string inputlucenepath, outputlucene, tempfolderpath;
            double removeSimilarity;
            List<string> addtionalStopWords;
            bool bRemoveSameURL;
            LoadConfigFileRemoveSimilarDocuments(out inputlucenepath, out outputlucene, out tempfolderpath, out removeSimilarity, out addtionalStopWords, out bRemoveSameURL);
            
            int titleweight = 3;
            int[] removeDateGranularity = new int[] { 1, 7, 30 };
            int[] removeWordGranularity = new int[] { 30, 20, 10 };
            List<string> stopwords = StopWords.stopwords.ToList<string>();
            stopwords.AddRange(addtionalStopWords);
            Hashtable stophash = StopFilter.MakeStopSet(stopwords);//stopwords_BingNews_Microsoft);
            int removeDocLength = 100;
            string inputpath;

            if (bRemoveSameURL)
            {
                Console.WriteLine("=====================RemoveSameURL=====================");
                RemoveSameURLShortDocument(inputlucenepath, tempfolderpath + "LuceneTemp_RemovedURL", removeDocLength);
                inputpath = tempfolderpath + "LuceneTemp_RemovedURL";
            }
            else
                inputpath = inputlucenepath;
    
            for (int iGranu = 0; iGranu < removeDateGranularity.Length; iGranu++)
            {
                int timeGranu = removeDateGranularity[iGranu];
                int wordGranu = removeWordGranularity[iGranu];

                Console.WriteLine("========Remove Similar Document: {0} out of {1}, Granu: {2} {3}========", iGranu, removeDateGranularity.Length,
                    timeGranu, wordGranu);

                string outputpath = tempfolderpath + "LuceneTemp_Granu" + iGranu;
                if (iGranu == removeDateGranularity.Length - 1)
                    outputpath = outputlucene;
                RemoveSimilarDocumentsGranu(inputpath, outputpath, removeSimilarity, timeGranu, wordGranu, titleweight, stophash);

                inputpath = outputpath;
            }


            Console.WriteLine("Deleting temp folders");
            Directory.Delete(tempfolderpath, true);
            //Console.WriteLine("Deleting LuceneTemp_RemovedURL...");
            //Directory.Delete(tempfolderpath + "LuceneTemp_RemovedURL", true);
            //for (int iGranu = 0; iGranu < removeDateGranularity.Length - 1; iGranu++)
            //{
            //    Console.WriteLine("Deleting LuceneTemp_Granu" + iGranu);
            //    Directory.Delete(tempfolderpath + "LuceneTemp_Granu" + iGranu, true);
            //}

            Console.WriteLine("All done");
            Console.ReadKey();
        }

        private static void RemoveSameURLShortDocument(string inputpath, string outputpath, int removeDocLength)
        {
            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputpath)), true)).GetIndexReader();
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            HashSet<string> urlHash = new HashSet<string>();
            int docNums = indexreader.NumDocs();
            Console.WriteLine("Total {0} docs", docNums);

            int removeDocNum = 0;
            string urlfield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.DocumentURL;
            string plainfield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            for (int iDoc = 0; iDoc < docNums; iDoc++)
            {
                if (iDoc % 10000 == 0)
                    Console.WriteLine("Processing {0} doc", iDoc);

                Document document = indexreader.Document(iDoc);
                string plain = document.Get(plainfield);
                if (plain == null || plain.Length < removeDocLength)
                {
                    removeDocNum++;
                    continue;
                }
                string url = document.Get(urlfield);
                //if (url != null)
                {
                    url = url.ToLower();
                    if (!urlHash.Contains(url))
                    {
                        indexwriter.AddDocument(document);
                        urlHash.Add(url);
                    }
                    else
                        removeDocNum++;
                }
                //else
                //    indexwriter.AddDocument(document);
            }
            Console.WriteLine("Finished remove same URL. Removed {0} out of {1}", removeDocNum, docNums);

            indexwriter.Optimize();
            indexwriter.Close();
        }

        private static void RemoveSimilarDocumentsGranu(string inputpath, string outputpath, double removeSimilarity,
            int timeWindowSize, int wordWindowSize, int titleweight, Hashtable stophash)
        {
            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputpath)), true)).GetIndexReader();
            LuceneDirectory outputdir = FSDirectory.Open(new DirectoryInfo(outputpath));
            IndexWriter indexwriter = new IndexWriter(outputdir, standardanalyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);

            Dictionary<int, Dictionary<int, List<SparseVectorList>>> uniqueDocHash = new Dictionary<int, Dictionary<int, List<SparseVectorList>>>();
            int docNums = indexreader.NumDocs();
            Console.WriteLine("Total {0} docs", docNums);

            int removeDocNum = 0;
            Dictionary<string, int> lexicon = new Dictionary<string, int>();
            string titlefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            string plainfield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            string datefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.DiscoveryStringTime;

            int timeslicesize = 1;
            if (timeWindowSize >= 15)
            {
                int[] dividePieceNumbers = new int[] { 3, 4, 5, 7 };
                foreach (int dividePieceNumber in dividePieceNumbers)
                    if (timeWindowSize % dividePieceNumber == 0)
                    {
                        timeslicesize = timeWindowSize / dividePieceNumber;
                        break;
                    }
                if (timeslicesize == 1)
                {
                    timeslicesize = (timeWindowSize + 2) / 3;
                    timeWindowSize = 3;
                }
                else
                    timeWindowSize /= timeslicesize;
                Console.WriteLine("Reset window size! TimeSliceSize: {0}, WindowSize: {1}", timeslicesize, timeWindowSize);
            }
            int begintimedelta = -(timeWindowSize - 1) / 2;
            int endtimedelta = timeWindowSize / 2;
            for (int iDoc = 0; iDoc < docNums; iDoc++)
            {
                if (iDoc % 10000 == 0)
                    Console.WriteLine("Processing {0} doc", iDoc);

                Document document = indexreader.Document(iDoc);
                SparseVectorList vector = GetFeatureVector(document, titlefield, plainfield, titleweight, lexicon, stophash);
                if (vector == null)
                {
                    removeDocNum++; 
                    continue;
                }

                int time = getDateTimeBingNews(document, datefield) / timeslicesize;
                int[] words = getMostFreqWordIndex(vector, wordWindowSize);
                bool bunqiue = true;
                for (int stime = time + begintimedelta; stime <= time + endtimedelta; stime++)
                {
                    if (uniqueDocHash.ContainsKey(stime))
                    {
                        Dictionary<int, List<SparseVectorList>> wordHash = uniqueDocHash[stime];
                        foreach (int sword in words)
                        {
                            if (wordHash.ContainsKey(sword))
                            {
                                List<SparseVectorList> vectorList = wordHash[sword];
                                foreach (SparseVectorList svector in vectorList)
                                    if (svector.Cosine(svector, vector) >= removeSimilarity)
                                    {
                                        bunqiue = false;
                                        break;
                                    }
                            }
                            if (!bunqiue)
                                break;
                        }
                    }
                    if (!bunqiue)
                        break;
                }

                if (bunqiue)
                {
                    int keytime = time;
                    int keyword = words[0];
                    if (!uniqueDocHash.ContainsKey(keytime))
                        uniqueDocHash.Add(keytime, new Dictionary<int, List<SparseVectorList>>());
                    Dictionary<int, List<SparseVectorList>> wordHash = uniqueDocHash[keytime];
                    if (!wordHash.ContainsKey(keyword))
                        wordHash.Add(keyword, new List<SparseVectorList>());
                    List<SparseVectorList> list = wordHash[keyword];
                    list.Add(vector);

                    indexwriter.AddDocument(document);
                }
                else
                    removeDocNum++;
            }

            Console.WriteLine("Finished remove similar documents. Removed {0} out of {1}", removeDocNum, docNums);

            int listLengthSum = 0, listCnt = 0;
            foreach (Dictionary<int, List<SparseVectorList>> hash0 in uniqueDocHash.Values)
                foreach (List<SparseVectorList> list in hash0.Values)
                {
                    listLengthSum += list.Count;
                    listCnt++;
                }
            Console.WriteLine("AvgListLength: {0}, ListCnt: {1}", listLengthSum / listCnt, listCnt);

            indexreader.Close();
            indexwriter.Optimize();
            indexwriter.Close();
        }

        private static int[] getMostFreqWordIndex(SparseVectorList featurevector, int k)
        {
            if (k == int.MaxValue)
                return new int[] { -1 };

            MinHeapInt mhd = new MinHeapInt(k);
            for (int i = 0; i < k; i++)
                mhd.insert(-1, int.MinValue);

            for (int iword = 0; iword < featurevector.keyarray.Length; iword++)
            {
                if (featurevector.valuearray[iword] > mhd.min())
                    mhd.changeMin(featurevector.keyarray[iword], featurevector.valuearray[iword]);
            }

            MinHeapInt.heapSort(mhd);

            return mhd.getIndices();
        }

        private static int getDateTimeBingNews(Document document, string datefield)
        {
            string discovertime = document.Get(datefield);
            int year = int.Parse(discovertime.Substring(0, 4));
            DateTime datetime = new DateTime(
                year,
                int.Parse(discovertime.Substring(5, 2)),
                int.Parse(discovertime.Substring(8, 2))
            );

            return 366 * year + datetime.DayOfYear;
        }

        private static SparseVectorList GetFeatureVector(Document document, string titlefield, string plainfield,
            int titleweight, Dictionary<string, int> lexicon, Hashtable stophash)
        {

            SparseVectorList featurevector = new SparseVectorList(Constant.DCM);

            string[] contents = new string[] { document.Get(titlefield), document.Get(plainfield) };
            int[] weights = new int[] { titleweight, 1 };
            int lexiconindexcount = lexicon.Count;

            for (int icontent = 0; icontent < contents.Length; icontent++)
            {
                string content = contents[icontent];
                int weight = weights[icontent];

                if (content == null)
                    return null;
                StringReader reader = new StringReader(content);
                TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_24, reader);

                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new StopFilter(true, result, stophash, true);

                result.Reset();
                TermAttribute termattr = (TermAttribute)result.GetAttribute(typeof(TermAttribute));
                while (result.IncrementToken())
                {
                    string termtext = termattr.Term();
                    int value = 0;
                    if (lexicon == null || lexicon.TryGetValue(termtext, out value) == false)
                    {
                        lexicon.Add(termtext, lexiconindexcount);
                        value = lexiconindexcount;
                        lexiconindexcount++;
                    }
                    if (!featurevector.Increase(value, weight))
                    {
                        featurevector.Insert(value, weight);
                    }
                }
            }

            featurevector.ListToArray();
            featurevector.count = featurevector.keyarray.Length;
            //featurevector.SumUpValueArray();
            if (featurevector.count < 5)
                return null;
            featurevector.InvalidateList();
            featurevector.GetNormDCM();
            return featurevector;
        }
        #endregion

        private static void AddDocumentFeatureVectorToIndex()
        {
            int titleweight = 5;
            int leadingweight = 2;
            int bodyweight = 0;

            string inputpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsIndex_Obama\BingNews_Obama_Sep_4months4_RemoveSimilar_RemoveNoise_9Words";
            string outputpath = string.Format(@"D:\Project\EvolutionaryRoseTreeData\BingNewsIndex_Obama\BingNews_Obama_Sep_4months4_RemoveSimilar_RemoveNoise_9Words_DocVector_{0}_{1}_{2}", titleweight, leadingweight, bodyweight);
            Hashtable stophash;
            if (inputpath.Contains("Microsoft"))
                stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_Microsoft);
            else if (inputpath.Contains("Obama"))
                stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_Obama);
            else if (inputpath.Contains("Syria"))
                stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_Syria);
            else if (inputpath.Contains("debt"))
                stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_DebtCrisis);
            else
                throw new Exception("Cannot tell whether it is Microsoft or Obama data!");

            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputpath)), true)).GetIndexReader();
            IndexWriter indexwriter = new IndexWriter(FSDirectory.Open(new DirectoryInfo(outputpath)), new StandardAnalyzer(Version.LUCENE_24), true, IndexWriter.MaxFieldLength.UNLIMITED);

            int docNum = indexreader.NumDocs();
            string featurevectorfield = "FeatureVector";
            string titlefield = Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            string bodyfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            Console.WriteLine("Total documents: {0}", docNum);
            for (int idoc = 0; idoc < docNum; idoc++)
            {
                if (idoc % 10000 == 0)
                    Console.WriteLine("Process " + idoc + "th document!");
                Document document = indexreader.Document(idoc);

                //PreProcess
                string title = document.Get(titlefield);
                string plain = document.Get(bodyfield);

                StringBuilder titles = new StringBuilder(title + " ");
                if (titleweight > 1)
                    for (int i = 1; i < Constant.BingNewsTitleWeight; i++)
                        titles.Append(title + " ");
                StringBuilder sb = new StringBuilder(" \a");
                sb.Append(titles.ToString());
                sb.Append("\a");
                if (leadingweight > bodyweight)
                {
                    var leadingPara = TestReadingData.GetLeadingParagraph(plain);
                    for (int i = bodyweight; i < leadingweight; i++)
                        sb.Append(leadingPara + " ");
                    sb.Append("\a");
                }
                if (bodyweight > 0)
                    for (int i = 0; i < bodyweight; i++)
                        sb.Append(plain + " ");

                //Process
                string[] querylinetokens = sb.ToString().Split('\a');
                if (querylinetokens.Length < 3) continue;
                StringReader reader = new StringReader(querylinetokens[1] + " " + querylinetokens[2]);
                TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_24, reader);

                List<int> featureSeq = new List<int>();
                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new StopFilter(true, result, stophash, true);

                //Lucene.Net.Analysis.Token token = result.Next();
                result.Reset();
                TermAttribute termattr = (TermAttribute)result.GetAttribute(typeof(TermAttribute));
                Dictionary<string, int> termcounts = new Dictionary<string, int>();
                while (result.IncrementToken())
                {
                    string termtext = termattr.Term();
                    if (!termcounts.ContainsKey(termtext))
                        termcounts.Add(termtext, 1);
                    else
                        termcounts[termtext]++;
                }

                //Sort termcounts 
                SortedDictionary<int, List<string>> invertedFeatureVector = new SortedDictionary<int, List<string>>();
                foreach (KeyValuePair<string, int> kvp in termcounts)
                {
                    if (!invertedFeatureVector.ContainsKey(kvp.Value))
                        invertedFeatureVector.Add(kvp.Value, new List<string>());
                    invertedFeatureVector[kvp.Value].Add(kvp.Key);
                }

                //output feature vector
                string featurevector = "";
                foreach (KeyValuePair<int, List<string>> kvp in invertedFeatureVector.Reverse<KeyValuePair<int, List<string>>>())
                {
                    int cnt = kvp.Key;
                    foreach (string term in kvp.Value)
                        featurevector += string.Format("{0}({1})\\n", term, cnt);
                }
                document.Add(new Field(featurevectorfield, featurevector, Field.Store.YES, Field.Index.ANALYZED));

                indexwriter.AddDocument(document);
            }

            indexreader.Close();
            indexwriter.Optimize();
            indexwriter.Close();
        }

        private static void AddFeatureVectorToIndexConfig()
        {
            string[] lines = File.ReadAllLines("configAddFeatureVector.txt");
            
            char[] seperator = new char[]{'\t'};
            int titleweight = int.Parse(lines[3].Split(seperator, StringSplitOptions.RemoveEmptyEntries)[1]);
            int leadingweight = int.Parse(lines[4].Split(seperator, StringSplitOptions.RemoveEmptyEntries)[1]);
            int bodyweight = int.Parse(lines[5].Split(seperator, StringSplitOptions.RemoveEmptyEntries)[1]);

            string inputpath = lines[0].Split(seperator, StringSplitOptions.RemoveEmptyEntries)[1];
            string outputpath = lines[1].Split(seperator, StringSplitOptions.RemoveEmptyEntries)[1];
            string stopwordfile = lines[2].Split(seperator, StringSplitOptions.RemoveEmptyEntries)[1];
            ConfigEvolutionary.SetUserSpecifiedStopWords(stopwordfile);
            Hashtable stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_UserDefined);

            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputpath)), true)).GetIndexReader();
            IndexWriter indexwriter = new IndexWriter(FSDirectory.Open(new DirectoryInfo(outputpath)), new StandardAnalyzer(Version.LUCENE_24), true, IndexWriter.MaxFieldLength.UNLIMITED);

            int docNum = indexreader.NumDocs();
            string featurevectorfield = "FeatureVector";
            string titlefield = Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            string bodyfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            Console.WriteLine("Total documents: {0}", docNum);
            for (int idoc = 0; idoc < docNum; idoc++)
            {
                if (idoc % 10000 == 0)
                    Console.WriteLine("Process " + idoc + "th document!");
                Document document = indexreader.Document(idoc);

                //PreProcess
                string title = document.Get(titlefield);
                string plain = document.Get(bodyfield);

                StringBuilder titles = new StringBuilder(title + " ");
                if (titleweight > 1)
                    for (int i = 1; i < Constant.BingNewsTitleWeight; i++)
                        titles.Append(title + " ");
                StringBuilder sb = new StringBuilder(" \a");
                sb.Append(titles.ToString());
                sb.Append("\a");
                if (leadingweight > bodyweight)
                {
                    var leadingPara = TestReadingData.GetLeadingParagraph(plain);
                    for (int i = bodyweight; i < leadingweight; i++)
                        sb.Append(leadingPara + " ");
                    sb.Append("\a");
                }
                if (bodyweight > 0)
                    for (int i = 0; i < bodyweight; i++)
                        sb.Append(plain + " ");

                //Process
                string[] querylinetokens = sb.ToString().Split('\a');
                if (querylinetokens.Length < 3) continue;
                StringReader reader = new StringReader(querylinetokens[1] + " " + querylinetokens[2]);
                TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_24, reader);

                List<int> featureSeq = new List<int>();
                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new StopFilter(true, result, stophash, true);

                //Lucene.Net.Analysis.Token token = result.Next();
                result.Reset();
                TermAttribute termattr = (TermAttribute)result.GetAttribute(typeof(TermAttribute));
                Dictionary<string, int> termcounts = new Dictionary<string, int>();
                while (result.IncrementToken())
                {
                    string termtext = termattr.Term();
                    if (!termcounts.ContainsKey(termtext))
                        termcounts.Add(termtext, 1);
                    else
                        termcounts[termtext]++;
                }

                //Sort termcounts 
                SortedDictionary<int, List<string>> invertedFeatureVector = new SortedDictionary<int, List<string>>();
                foreach (KeyValuePair<string, int> kvp in termcounts)
                {
                    if (!invertedFeatureVector.ContainsKey(kvp.Value))
                        invertedFeatureVector.Add(kvp.Value, new List<string>());
                    invertedFeatureVector[kvp.Value].Add(kvp.Key);
                }

                //output feature vector
                string featurevector = "";
                foreach (KeyValuePair<int, List<string>> kvp in invertedFeatureVector.Reverse<KeyValuePair<int, List<string>>>())
                {
                    int cnt = kvp.Key;
                    foreach (string term in kvp.Value)
                        featurevector += string.Format("{0}({1})\\n", term, cnt);
                }
                document.Add(new Field(featurevectorfield, featurevector, Field.Store.YES, Field.Index.ANALYZED));

                indexwriter.AddDocument(document);
            }

            indexreader.Close();
            indexwriter.Optimize();
            indexwriter.Close();
        }
    }
}
