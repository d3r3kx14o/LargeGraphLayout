using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Constants;
using System.Collections;
using EvolutionaryRoseTree.Experiments;
using RoseTreeTaxonomy.ReadData;

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

using System.Text.RegularExpressions;
namespace EvolutionaryRoseTree.Data
{
    class TestReadingData
    {
        public static void Entry()
        {
            //AnalysisLuceneIndexData();
            //SeekLuceneIndexClassifierData();
            //TestReadIndexedBingNewsData();
            //SeekLuceneIndexClassifierData();
            TestReadIndexedBingNewsData();
            //TestDuplicateIds();
            //TestDocumentLength();

            //TestQueryingLuceneData();
        }

        public static void TestQueryingLuceneData()
        {
            string startdatestr = "2012-01-01";
            int deltatime = 366;

            string[] indexpaths = new string[]{
                @"D:\Project\TopicPanorama\data\BingNews\BingNewsIndex_Apple_Year2012\",
                @"D:\Project\TopicPanorama\data\BingNews\BingNewsIndex_Google_Year2012_RemoveSimilar\",
                @"D:\Project\TopicPanorama\data\BingNews\BingNewsIndex_Microsoft_Year2012_RemoveSimilar\"
            };

            foreach(string indexpath in indexpaths)
                TestQueryingLuceneData(indexpath, startdatestr, deltatime);

        }

        public static void TestQueryingLuceneData(string indexpath, string startdatestr, int deltatime)
        {
            //string indexpath = @"D:\Project\TopicPanorama\data\BingNews\BingNewsIndex_Apple_Year2012\";
            //string indexpath = @"D:\Project\TopicPanorama\data\BingNews\BingNewsIndex_Google_Year2012_RemoveSimilar\";
            //string indexpath = @"D:\Project\TopicPanorama\data\BingNews\BingNewsIndex_Microsoft_Year2012_RemoveSimilar\";
            string defaultfield = Constant.IndexedBingNewsDataFields.DiscoveryStringTime;
            //string startdatestr = "2012-02-01";
            //int deltatime = 29;
            string querystr = EvolutionaryExperiments.GetIndexedBingNewsDateQuery(
                EvolutionaryExperiments.GetDateTime(startdatestr), deltatime, 0, -1);

            try
            {
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                IndexSearcher searcher = new IndexSearcher(directory, true);

                Version version = Version.LUCENE_24;
                QueryParser queryparser = new QueryParser(version, defaultfield, new StandardAnalyzer(version));

                Query query = queryparser.Parse(querystr);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;

                Console.WriteLine(docs.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void TestDocumentLength()
        {
            string inputpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Jan-July_newMS_RemoveSimilar";

            IndexReader indexreader = (new IndexSearcher(FSDirectory.Open(new DirectoryInfo(inputpath)), true)).GetIndexReader();

            double titlelengthsum = 0;
            double leadinglengthsum = 0;
            double bodylengthsum = 0;

            int docNums = indexreader.NumDocs();
            Console.WriteLine("Total {0} docs", docNums);

            string titlefield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
            string plainfield = RoseTreeTaxonomy.Constants.Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            for (int iDoc = 0; iDoc < docNums; iDoc++)
            {
                if (iDoc % 10000 == 0)
                    Console.WriteLine("Processing {0} doc", iDoc);

                Document document = indexreader.Document(iDoc);

                titlelengthsum += document.Get(titlefield).Length;
                string plain = document.Get(plainfield);
                string leading = GetLeadingParagraph(plain);
                leadinglengthsum += leading.Length;
                bodylengthsum += plain.Length - leading.Length;
            }

            Console.WriteLine("Avg Title Length: " + titlelengthsum / docNums);
            Console.WriteLine("Avg Leading Length: " + leadinglengthsum / docNums);
            Console.WriteLine("Avg Body Length: " + bodylengthsum / docNums);
            Console.ReadKey();
        }

        public static string GetLeadingParagraph(string plain)
        {
            var contents = plain.Split('.', '?', '!');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Constant.LeadingParaSentenseNum && i < contents.Length; i++)
            {
                sb.Append(contents[i]);
                sb.Append('.');
            }
            return sb.ToString();
        }

        public static void TestSearchingFromLuceneIndex()
        {
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\NYTimes\NYTIndex_Year\1987";
            IndexSearcher searcher = null;
            try
            {
                //searcher = new IndexSearcher(this.datapath + Constant.inputfilenames[this.dataset_index]);
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                searcher = new IndexSearcher(directory, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //QueryParser queryparser = new QueryParser("newsgroup", new StandardAnalyzer());
            Version version = Version.LUCENE_24;
            QueryParser queryparser = new QueryParser(version, "Taxonomic Classifiers", new StandardAnalyzer(version));

            string queryStr = "Pro AND football";

            try
            {
                Query query = queryparser.Parse(queryStr);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;

                Document testdoc = searcher.Doc(0);
                foreach (Field field in testdoc.GetFields())
                    Console.WriteLine(field.Name());

                Console.WriteLine("Documents found:" + docs.Length);

                foreach (ScoreDoc doc in docs)
                {
                    Document document = searcher.Doc(doc.doc);
                    string dayOfMonth = document.Get("Publication Day Of Month");
                    Console.WriteLine(dayOfMonth);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        public static void AnalysisLuceneIndexData()
        {
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\NYTimes\NYTIndex_Year\2006";
            string outputfilename = @"D:\Project\EvolutionaryR\NYTimes\Analysis.dat";

            IndexSearcher searcher = null;
            StreamWriter ofile = null;
            try
            {
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                searcher = new IndexSearcher(directory, true);
                ofile = new StreamWriter(outputfilename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Version version = Version.LUCENE_24;
            QueryParser queryparser = new QueryParser(version, "Cleaned Taxonomic Classifier", new StandardAnalyzer(version));
            string queryStr = "Top/Classifieds/Job\\ Market OR Top/Classifieds/Real\\ Estate OR " +
"Top/Features/Arts OR Top/Features/Style/ OR Top/Features/Travel/ OR " +
"Top/News/Business/ OR Top/News/Health/ OR Top/News/Science/ OR Top/News/Sports";
            //string queryStr = "(Top/News/Business OR Top/News/Sports) AND Publication\\ Month:1";
            //string queryStr = "Top/News/Science AND Publication\\ Month:1";

            //string queryStr = "*:*";// AND Cleaned\\ Taxonomic\\ Classifier:Top/News/Sports";

            HashSet<string> taxonomicHash = new HashSet<string>();
            Dictionary<string, int> taxonomicCounter = new Dictionary<string, int>();
            try
            {
                Query query = queryparser.Parse(queryStr);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;

                //Document testdoc = searcher.Doc(0);
                //foreach (Field field in testdoc.GetFields())
                //    Console.WriteLine(field.Name());

                Console.WriteLine("Documents found:" + docs.Length);

                foreach (ScoreDoc doc in docs)
                {
                    Document document = searcher.Doc(doc.doc);
                    //string taxomonicstr = document.Get("Taxonomic Classifiers");
                    string taxomonicstr = document.Get("Cleaned Taxonomic Classifier");
                    if (taxomonicstr != null)
                    {
                        //string[] tokens = taxomonicstr.Split(new string[]{"||"}, new StringSplitOptions());
                        //foreach (string token in tokens)
                            //taxonomicHash.Add(token);
                        taxonomicHash.Add(taxomonicstr);
                        if (taxonomicCounter.ContainsKey(taxomonicstr))
                            taxonomicCounter[taxomonicstr]++;
                        else
                            taxonomicCounter.Add(taxomonicstr, 1);
                    }

                    if ((doc.doc+1) % 10000 == 0)
                        Console.WriteLine("Finish parsing:" + doc.doc + ", HashTable length:" + taxonomicHash.Count);
                }

                List<string> taxonomyList = taxonomicHash.ToList<string>();
                taxonomyList.Sort();

                foreach (string taxonomy in taxonomyList)
                {
                    ofile.WriteLine(taxonomy + "\t\t\t" + taxonomicCounter[taxonomy]);
                }
                ofile.Flush();
                ofile.Close();

                Console.WriteLine("All Finished!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        public static void AnalysisLuceneIndexClassifierData()
        {
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\NYTimes\NYTIndex";
            string outputfilename = @"D:\Project\EvolutionaryRoseTreeData\NYTimes\Analysis.dat";

            IndexSearcher searcher = null;
            StreamWriter ofile = null;
            try
            {
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                searcher = new IndexSearcher(directory, true);
                ofile = new StreamWriter(outputfilename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Version version = Version.LUCENE_24;
            QueryParser queryparser = new QueryParser(version, "Cleaned Classifier Layer", new StandardAnalyzer(version));

            int[] years = new int[2006 - 1987 + 1];
            for (int i = 0; i < years.Length; i++)
                years[i] = i + 1987;
            int[] months = new int[12];
            for (int i = 0; i < months.Length; i++)
                months[i] = i + 1;

            List<string> prevTaxoList = new List<string>();
            for(int iyear=0;iyear<years.Length;iyear++)
                for (int imonth = 0; imonth < months.Length; imonth++)
                {
                    //ofile.WriteLine();
                    string queryStr = "Publication\\ Year:" + years[iyear] 
                        + " Publication\\ Month:" + months[imonth]; // AND Cleaned\\ Taxonomic\\ Classifier:Top/News/Sports";

                    ofile.WriteLine("===============Year " + years[iyear] + " Month " + months[imonth] + "===============");
                    Console.WriteLine("===============Year " + years[iyear] + " Month " + months[imonth] + "===============");

                    HashSet<string> taxonomicHash = new HashSet<string>();
                    try
                    {
                        Query query = queryparser.Parse(queryStr);
                        TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                        ScoreDoc[] docs = hits.scoreDocs;

                        //Document testdoc = searcher.Doc(0);
                        //foreach (Field field in testdoc.GetFields())
                        //    Console.WriteLine(field.Name());

                        Console.WriteLine("Documents found:" + docs.Length);

                        foreach (ScoreDoc doc in docs)
                        {
                            Document document = searcher.Doc(doc.doc);
                            //string taxomonicstr = document.Get("Taxonomic Classifiers");
                            string taxomonicstr = document.Get("Cleaned Taxonomic Classifier");
                            if (taxomonicstr != null)
                            {
                                //string[] tokens = taxomonicstr.Split(new string[]{"||"}, new StringSplitOptions());
                                //foreach (string token in tokens)
                                //taxonomicHash.Add(token);
                                taxonomicHash.Add(taxomonicstr);
                            }

                            //if ((doc.doc + 1) % 10000 == 0)
                            //    Console.WriteLine("Finish parsing:" + doc.doc + ", HashTable length:" + taxonomicHash.Count);
                        }

                        List<string> taxonomyList = taxonomicHash.ToList<string>();
                        taxonomyList.Sort();

                        foreach (string taxonomy in taxonomyList)
                        {
                            if (!prevTaxoList.Contains(taxonomy))
                                ofile.WriteLine("[I]" + taxonomy);
                        }
                        foreach (string prevtaxonomy in prevTaxoList)
                        {
                            if (!taxonomyList.Contains(prevtaxonomy))
                                ofile.WriteLine("[D]" + prevtaxonomy);
                        }

                        prevTaxoList = taxonomyList;
                        ofile.Flush();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }


            ofile.Close();

            Console.WriteLine("All Finished!");
            Console.ReadKey();
        }

        public static void SeekLuceneIndexClassifierData()
        {
            Version version = Version.LUCENE_24;

            //QueryParser queryparser = new QueryParser(version, "Cleaned Taxonomic Classifier", new StandardAnalyzer(version));
            //string[] SeekClassifier = new string[]{
            //    "Top/Classifieds/Automobiles",
            //    "Top/Classifieds/Job\\ Market",
            //    "Top/Classifieds/Real\\ Estate",
            //    "Top/Features/Arts",
            //    "Top/Features/Style",
            //    "Top/Features/Travel",
            //    "Top/News/Business",
            //    "Top/News/Health",
            //    "Top/News/Science",
            //    "Top/News/Sports"
            //};

            //QueryParser queryparser = new QueryParser(version, "Cleaned Taxonomic Classifier", new StandardAnalyzer(version));
            QueryParser queryparser = new QueryParser(version, "Taxonomic Classifiers", new StandardAnalyzer(version));
            string[] SeekClassifier = new string[]{
                //"NOT Opinion NOT Classifieds"
                //"Features News"
                //"NOT Classifieds"
                "*:*"
            };
            //((Features AND (NOT News)) OR (News AND (NOT Features)) AND (NOT Classifieds) AND (NOT Opinion)
                //"((Features AND (NOT News)) OR (News AND(NOT Features))) AND (NOT Classifieds) AND (NOT Opinion)"
                //"(News XOR Features) - Opinion - Classifieds"

            string outputfilename = @"D:\Project\EvolutionaryRoseTreeData\NYTimes\AnalysisSeek.dat";
            StreamWriter ofile = null;
            try
            {
                ofile = new StreamWriter(outputfilename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            //int[] years = new int[2006 - 1987 + 1];
            //for (int i = 0; i < years.Length; i++)
            //    years[i] = i + 1987;
            int[] years = new int[]{2006};
            int[] months = new int[12];
            for (int i = 0; i < months.Length; i++)
                months[i] = i + 1;

            for (int iyear = 0; iyear < years.Length; iyear++)
            {
                string indexpath = @"D:\Project\EvolutionaryRoseTreeData\NYTimes\NYTIndex_Year\" + years[iyear];
                IndexSearcher searcher = null;
                try
                {
                    LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                    searcher = new IndexSearcher(directory, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                for (int imonth = 0; imonth < months.Length; imonth++)
                {
                    //ofile.WriteLine();
                    string queryStrTail = " AND Publication\\ Month:" + months[imonth]; // AND Cleaned\\ Taxonomic\\ Classifier:Top/News/Sports";

                    ofile.WriteLine("===============Year " + years[iyear] + " Month " + months[imonth] + "===============");
                    Console.WriteLine("===============Year " + years[iyear] + " Month " + months[imonth] + "===============");

                    for (int iclassifier = 0; iclassifier < SeekClassifier.Length; iclassifier++)
                    {
                        string queryStr = SeekClassifier[iclassifier] + queryStrTail;
                        try
                        {
                            Query query = queryparser.Parse(queryStr);
                            TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                            ScoreDoc[] docs = hits.scoreDocs;
                            //Dictionary<string, int> sortedfreq = GetSortedContent(searcher, docs, "Taxonomic Classifiers");
                            //foreach (string content in sortedfreq.Keys)
                            {
                                //Console.WriteLine("{0}------{1}", content, sortedfreq[content]);
                                //if (content.Contains("Opinion") || content.Contains("Classifieds"))
                                //    Console.WriteLine("Error1!");
                                //if (!(content.Contains("Features") ^ content.Contains("News")))
                                //    Console.WriteLine("Error2!");
                            }

                            //Document testdoc = searcher.Doc(0);
                            //foreach (Field field in testdoc.GetFields())
                            //    Console.WriteLine(field.Name());

                            Console.WriteLine(SeekClassifier[iclassifier] + ": " + docs.Length);
                            ofile.WriteLine(SeekClassifier[iclassifier] + ": " + docs.Length);

                            ofile.Flush();

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }


            ofile.Close();

            Console.WriteLine("All Finished!");
        }

        public static void TestSearchIndexedBingNewsData()
        {
            //string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\BingNewsIndex_Microsoft";
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Merged\";
            string defaultfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            string rawqueryStr = "Microsoft";
            //string outputfilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\Analysis.dat";
            string outputfilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Merged\Analysis.dat";

            string startdate = "2012-05-09";
            int timespan = 7;
            int timeslotsnum = 1;

            Dictionary<string, int> discovertimehash = new  Dictionary<string, int>();
            DateTime startdatetime = EvolutionaryExperiments.GetDateTime(startdate);
            for (int itime = 0; itime < timeslotsnum; itime++)
            {
                string queryStr = rawqueryStr + " AND " + EvolutionaryExperiments.GetIndexedBingNewsDateQuery(startdatetime, timespan, itime);
                Console.WriteLine("------------------{0}--------------------", itime);

                try
                {
                    LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                    IndexSearcher searcher = new IndexSearcher(directory, true);

                    Version version = Version.LUCENE_24;
                    QueryParser queryparser = new QueryParser(version, defaultfield, new StandardAnalyzer(version));

                    Query query = queryparser.Parse(queryStr);
                    TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                    ScoreDoc[] docs = hits.scoreDocs;

                    //Document testdoc = searcher.Doc(0);
                    //foreach (Field field in testdoc.GetFields())
                    //    Console.WriteLine(field.Name());

                    Console.WriteLine("Documents found:" + docs.Length);

                    int idoc = 0;
                    foreach (ScoreDoc doc in docs)
                    {
                        Document document = searcher.Doc(doc.doc);
                        string discovertime = document.Get(Constant.IndexedBingNewsDataFields.DiscoveryStringTime);
                        discovertime = discovertime.Split(' ')[0];
                        //discovertimehash.Add(discovertime);
                        if (discovertimehash.ContainsKey(discovertime))
                            discovertimehash[discovertime]++;
                        else
                            discovertimehash.Add(discovertime, 1);

                        if (idoc % 5000 == 1)
                            Console.WriteLine(idoc);
                        idoc++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            StreamWriter ofile = new StreamWriter(outputfilename);
            List<string> discovertimelist = discovertimehash.Keys.ToList<string>();
            discovertimelist.Sort();
            foreach(string discovertime in discovertimelist)
                ofile.WriteLine(discovertime);
            ofile.Flush();
            ofile.Close();
        }

        public static void TestReadIndexedBingNewsData()
        {
            //string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\BingNewsIndex_Microsoft";
            string indexpath = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Merged\";
            string defaultfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
            //string outputfilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_2012\Analysis.dat";
            string outputfilename = @"D:\Project\EvolutionaryRoseTreeData\BingNewsData_Microsoft\BingNewsIndex_Microsoft_Merged\Analysis.dat";

            Dictionary<string, int> discovertimehash = new Dictionary<string, int>();
            try
            {
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(indexpath));
                IndexSearcher searcher = new IndexSearcher(directory, true);

                Version version = Version.LUCENE_24;
                QueryParser queryparser = new QueryParser(version, defaultfield, new StandardAnalyzer(version));

                Query query = queryparser.Parse("*:*");
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;

                Console.WriteLine("Documents found:" + docs.Length);

                int idoc = 0;
                foreach (ScoreDoc doc in docs)
                {
                    Document document = searcher.Doc(doc.doc);
                    string discovertime = document.Get(Constant.IndexedBingNewsDataFields.DiscoveryStringTime);
                    discovertime = discovertime.Split(' ')[0];
                    //discovertimehash.Add(discovertime);
                    if (discovertimehash.ContainsKey(discovertime))
                        discovertimehash[discovertime]++;
                    else
                        discovertimehash.Add(discovertime, 1);

                    if (idoc % 5000 == 1)
                        Console.WriteLine(idoc);
                    idoc++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            StreamWriter ofile = new StreamWriter(outputfilename);
            List<string> discovertimelist = discovertimehash.Keys.ToList<string>();
            discovertimelist.Sort();
            foreach (string discovertime in discovertimelist)
                ofile.WriteLine(discovertime + "\t" + discovertimehash[discovertime]);
            ofile.Flush();
            ofile.Close();
        }

        static Dictionary<string, int> GetSortedContent(IndexSearcher searcher, ScoreDoc[] docs, string field)
        {
            Dictionary<string, int> contentsfreq = new Dictionary<string, int>();
            foreach (ScoreDoc doc in docs)
            {
                string content = (searcher.Doc(doc.doc)).Get(field);
                if (content == null)
                    continue;
                if (contentsfreq.ContainsKey(content))
                    contentsfreq[content]++;
                else
                    contentsfreq.Add(content, 1);
            }

            List<string> contents = new List<string>();
            foreach (string content in contentsfreq.Keys)
                contents.Add(content);
            contents.Sort();

            Dictionary<string, int> sortedfreq = new Dictionary<string, int>();
            foreach (string content in contents)
                sortedfreq.Add(content, contentsfreq[content]);

            return sortedfreq;
        }

        private static void TestDuplicateIds()
        {
            StreamReader sr = new StreamReader(@"D:\Project\EvolutionaryRoseTreeData\Evolutionary\start[2012-1-8]span7slot28sample10000\0119_021412_gamma0.27alpha0.01KNN100merge5E-05split5E-05cos0.25newalpha1E-20_LooseOrder0.4_OCM_D\8.gv");
            string line;
            Regex r = new Regex("-([0-9]+)-");
            HashSet<int> hash = new HashSet<int>();
            while ((line = sr.ReadLine()) != null)
            {
                if(line.Contains("->"))
                    continue;

                Match match = r.Match(line);
                if (match.Success)
                {
                    int index = int.Parse(match.Value.Substring(1, match.Length - 2));
                    if (hash.Contains(index))
                        Console.WriteLine(index);
                    else
                        hash.Add(index);
                    Console.WriteLine(index);
                }
            }
            Console.ReadKey();
        }

    }
}
