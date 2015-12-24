using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.DataStructures;

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

namespace EvolutionaryRoseTree.DataStructures
{
    class LoadGlobalFeatureVectors : LoadFeatureVectors
    {
        string indexpath;
        static string previousIndexPath = null;
        static Dictionary<string, int> previousLexicon = null;
        static Dictionary<int, string> previousInvertedLexicon = null;

        //public static string suppressword = "windows";
        //public static int suppresswordindex = -1;
        public static double suppressRatio = 1;

        public LoadGlobalFeatureVectors(int dataset_index, int model_index)
            : base(dataset_index, model_index)
        {
        }

        public LoadGlobalFeatureVectors(int dataset_index, int model_index, string news_filename,
            string sample_filename, string featurevector_path, int sample_num)
            : base(dataset_index, model_index, news_filename, sample_filename, featurevector_path, sample_num)
        {
        }

        public LoadGlobalFeatureVectors(int dataset_index, int model_index, string news_filename,
    string sample_filename, string featurevector_path, int sample_num,
            string defaultfield, string querystring)
            : base(dataset_index, model_index, news_filename, sample_filename, featurevector_path, sample_num, defaultfield, querystring)
        {
        }

        public override void Load(string outputpath)
        {
            if (dataset_index != Constant.TWENTY_NEWS_GROUP &&
                dataset_index != Constant.NEW_YORK_TIMES &&
                dataset_index != Constant.INDEXED_BING_NEWS)
                throw new Exception("Global Feature Vectors not supported in this dataset!");
            
            this.indexpath = outputpath;

            switch (dataset_index)
            {
                case Constant.TWENTY_NEWS_GROUP: Load20NewsGroupData(outputpath); break;
                case Constant.NEW_YORK_TIMES: LoadNewYorkTimesData(outputpath); break;
                case Constant.INDEXED_BING_NEWS: LoadIndexedBingNewsData(outputpath); break;
                default: break;
            }

#if SUPPRESS_WORDS
            SuppressWords();            
#endif
            //FeatureSelection();     //remove features with 0 occurrences
            
            ResizeFeatureVectors(); //remove feature vectors with 0 features
            SumUpFeatureVectors();  //calculate words' global occurrences, update maxdimensionvalue
            PostProcessData();      //change data from list to array, calculate sum(occurrences) (since data will not change any more)

            //List<int> wordappearancecount_keylist = new List<int>();
            //foreach (int key in wordappearancecount.Keys)
            //    wordappearancecount_keylist.Add(key);
            //wordappearancecount_keylist.Sort();
            //Dictionary<int, int> new_wordappearancecount = new Dictionary<int, int>();
            //foreach (int key in wordappearancecount_keylist)
            //    new_wordappearancecount.Add(key, wordappearancecount[key]);
            //wordappearancecount = new_wordappearancecount

            //ReLabelKeys();          //change index: key in feature vectors are now coordinated with keys in wordappearancecount, from 0 to end continously

            if (model_index == Constant.VMF)
                ComputeIDF();
            //this.lexiconsize = this.wordappearancecount.Count;
            //Xiting, lexiconsize = datadimension, modified to adapt to data prediction
            this.lexiconsize = this.lexicon.Count;
            GetNorm();              //calculate norm of vector value

            //WriteFeatureVectors();

            //Console.WriteLine("LexiconSize:" + lexicon.Count);
            //Console.WriteLine("LocalLexiconSize:" + wordappearancecount.Count);

            Console.WriteLine("Cv = {0}", featurevectors.Max(v=>v.count));
        }

        public override void PreProcessNewYorkTimesData()
        {
            IndexSearcher searcher = null;
            try
            {
                //searcher = new IndexSearcher(this.datapath + Constant.inputfilenames[this.dataset_index]);
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(this.datapath));
                searcher = new IndexSearcher(directory, true);
            }
            catch (Exception e)
            {
                this.featurevectorsnum = 0;
                Console.WriteLine(e.Message);
            }

            Version version = Version.LUCENE_24;
            QueryParser queryparser = new QueryParser(version, this.defaultqueryfield, new StandardAnalyzer(version));

            labelHash = new Dictionary<string, int>();
            int globalLabelID = 0;
            //int docIndex = 0;
            string queryStr = this.querystring;

            //try
            {
                Query query = queryparser.Parse(queryStr);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;

                //Document testdoc = searcher.Doc(0);
                //foreach (Field field in testdoc.GetFields())
                //    Console.WriteLine(field.Name());

                this.samplelines = new string[samplenum];
                this.samplelabels = new int[samplenum];
                this.sampledocids = new string[samplenum];

                for (int isample = 0; isample < sampleitems.Count; isample++)
                {
                    ScoreDoc doc = docs[sampleitems[isample]];
                    Document document = searcher.Doc(doc.doc);

#if NYT_LEADING_PARAGRAPH
                    string plain = document.Get(Constant.NewYorkTimesDataFields.Body);
                    string title = document.Get(Constant.NewYorkTimesDataFields.Headline);                   //this.unsampledlines.Add(" " + "\a" + title + "\a" + plain);
                    //samplelines[isample] = " " + "\a" + title + "\a" + plain;
                    //Hao
                    StringBuilder titles = new StringBuilder(title + " ");
                    titles.Append(title + " ");
                    titles.Append(title + " ");
                    //titles.Append(title + " ");
                    //titles.Append(title + " ");
                    var leadingPara = plain;
                    if (plain != null)
                         leadingPara = GetLeadingParagraph(plain);
                    //var remainbody = GetRemainBody(plain);
                    StringBuilder sb = new StringBuilder(" \a");
                    sb.Append(titles.ToString());
                    sb.Append("\a");
                    sb.Append(leadingPara);
                    sb.Append("\a");
                    //sb.Append(" ");
                    //sb.Append(plain);
                    samplelines[isample] = sb.ToString();
#else

                    string plain = document.Get(Constant.NewYorkTimesDataFields.Body);
                    string title = document.Get(Constant.NewYorkTimesDataFields.Headline);
                    //this.unsampledlines.Add(" " + "\a" + title + "\a" + plain);
                    samplelines[isample] = " " + "\a" + title + "\a" + plain + "\a";
#endif
                    //if (plain.Contains('\a') || title.Contains('\a'))
                    //{
                    //    //                        int stop;
                    //}

                    //this.unsampleddocids.Add(doc.doc + "");
                    sampledocids[isample] = doc.doc + "";


                    string labelStr = document.Get(Constant.NewYorkTimesDataFields.CleanedTaxonomicClassifier);
                    try
                    {
                        labelStr = GetTransferedNewYorkTimesLabel(labelStr);
                    }
                    catch
                    {
                        labelStr = "E.E";
                    }
                    int label = 0;
                    if (labelHash.ContainsKey(labelStr))
                    {
                        label = labelHash[labelStr];
                    }
                    else
                    {
                        label = globalLabelID;
                        labelHash.Add(labelStr, globalLabelID);
                        globalLabelID++;
                    }
                    //this.unsampledlabels.Add(label);
                    samplelabels[isample] = label;
#if PrintDetailedProcess
                    if (docIndex % 5000 == 0)
                    {
                        Console.WriteLine(">>>[LOG]: Loaded String " + docIndex + " documents.");
                    }
#endif
                    //docIndex++;
                }
            }
            //catch (Exception ex)
            //{
            //    this.unsampledlabels = null;
            //    this.unsampledlines = null;
            //    Console.WriteLine(ex.Message);
            //}
        }

        public override void ProcessBingNews20NewsGroupData()
        {
            LoadGlobalLexicon();

            this.featurevectors = new SparseVectorList[samplenum];

            for (int i = 0; i < samplenum; i++)
                this.featurevectors[i] = null;

            if (dataset_index == Constant.INDEXED_BING_NEWS)
            {
                if (StopWords.stopwords_BingNews_UserDefined != null)
                    stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_UserDefined);
                else if (querystring.Contains("Microsoft"))
                    stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_Microsoft);
                else if (querystring.Contains("Obama"))
                    stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_Obama);
                else if (querystring.Contains("Syria"))
                    stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_Syria);
                else if (querystring.Contains("debt"))
                    stophash = StopFilter.MakeStopSet(StopWords.stopwords_BingNews_DebtCrisis);
                else
                    throw new Exception("Cannot determine stop words!");
            }
            else
                stophash = StopFilter.MakeStopSet(StopWords.stopwords);

            for (int i = 0; i < samplenum; i++)
            {
                //if (i % 10000 == 0)
                //    Console.WriteLine("Processing the " + i + "th data into feature vector");

                this.featurevectors[i] = new SparseVectorList(model_index);
                char separator = (dataset_index == 1) ? '\t' : '\a';
                string[] querylinetokens = samplelines[i].Split(separator);

                if (querylinetokens.Length < 3) continue;
                this.featurevectors[i].querystring = querylinetokens[1];
                //StringReader reader = new StringReader(querylinetokens[2]);
                StringReader reader = new StringReader(querylinetokens[1] + " " + querylinetokens[2] + " " + querylinetokens[3]);
                TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_24, reader);

                //result = new StandardFilter(result);
                //result = new StopFilter(true, result, stophash, true);
                //result = new PorterStemFilter(result);
                //result = new LowerCaseFilter(result);
                //result = new StopFilter(true, result, stophash, true);
                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new StopFilter(true, result, stophash, true);

                /// Set up lexicon/invertlexicon, featurevectors, wordappearancecount ///
                result.Reset();
                TermAttribute termattr = (TermAttribute)result.GetAttribute(typeof(TermAttribute));
                while (result.IncrementToken())
                {
                    string termtext = termattr.Term();
                    int lexiconIndex = 0;
                    if (this.lexicon.TryGetValue(termtext, out lexiconIndex))
                    {
                        //if (i == 4)
                        //    Console.WriteLine("{0}----{1}", termtext, lexiconIndex);
                        if (!featurevectors[i].Increase(lexiconIndex, 1))
                        {
                            featurevectors[i].Insert(lexiconIndex, 1);
                            if (wordappearancecount.ContainsKey(lexiconIndex))
                                wordappearancecount[lexiconIndex]++;
                            else
                                wordappearancecount.Add(lexiconIndex, 1);
                        }
                    }
                    //else
                    //{
                    //    throw new Exception("[ERROR] Term does not exist in Global Lexicon!");
                    //}

                    this.wordnum++;
                }

                //if (i == 4)
                //{
                //    List<int> keylist = featurevectors[i].keylist;
                //    List<int> valuelist = featurevectors[i].valuelist;
                //    for (int j = 0; j < keylist.Count; j++)
                //    {
                //        Console.Write("<{0},{1}>", keylist[j], valuelist[j]);
                //    }
                //    Console.WriteLine();
                //}
            }

        }

        void LoadGlobalLexicon()
        {
            string globalLexiconFileName = indexpath + "\\GlobalLexicon.dat";

            //Create the global lexicon
            if (!File.Exists(globalLexiconFileName))
            {
                Console.WriteLine("Create GlobalLexicon...");
                stophash = StopFilter.MakeStopSet(StopWords.stopwords);

                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(this.indexpath));
                IndexReader indexreader = (new IndexSearcher(directory, true)).GetIndexReader();
                int docNum = indexreader.NumDocs();

                string titlefieldname, bodyfieldname;
                GetLuceneVitalFieldName(out bodyfieldname, out titlefieldname);
                
                HashSet<string> lexiconhash = new HashSet<string>();
                for (int idoc = 0; idoc < docNum; idoc++)
                {
                    if (idoc % 10000 == 0)
                        Console.WriteLine(idoc);
                    Document document = indexreader.Document(idoc);
                    string plain = document.Get(bodyfieldname);
                    string title = document.Get(titlefieldname);
                    if (plain == null) plain = "";

                    StringReader reader = new StringReader(title + " " + plain);
                    TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_24, reader);

                    List<int> featureSeq = new List<int>();

                    //result = new StandardFilter(result);
                    //result = new StopFilter(true, result, stophash, true);
                    //result = new PorterStemFilter(result);
                    //result = new LowerCaseFilter(result);
                    //result = new StopFilter(true, result, stophash, true);
                    result = new StandardFilter(result);
                    result = new LowerCaseFilter(result);
                    result = new StopFilter(true, result, stophash, true);



                    result.Reset();
                    TermAttribute termattr = (TermAttribute)result.GetAttribute(typeof(TermAttribute));
                    while (result.IncrementToken())
                    {
                        string termtext = termattr.Term();
                        lexiconhash.Add(termtext);
                    }
                }

                //Write
                StreamWriter sr = new StreamWriter(globalLexiconFileName);
                foreach (string term in lexiconhash)
                    sr.WriteLine(term);
                sr.Flush();
                sr.Close();

                lexiconhash = null;
                Console.WriteLine("Finish Create GlobalLexicon.");
            }

            if (indexpath != previousIndexPath)
            {
                //Read in lexicon from file
                StreamReader sr = new StreamReader(globalLexiconFileName);
                string term;
                int lexiconIndex = 0;
                while ((term = sr.ReadLine()) != null)
                {
                    lexicon.Add(term, lexiconIndex);
                    invertlexicon.Add(lexiconIndex, term);
                    lexiconIndex++;
                }
                this.lexiconindexcount = lexiconIndex;

                previousIndexPath = indexpath;
                previousLexicon = lexicon;
                previousInvertedLexicon = invertlexicon;
            }
            else
            {
                lexicon = previousLexicon;
                invertlexicon = previousInvertedLexicon;
            }
        }

        void GetLuceneVitalFieldName(out string bodyfield, out string titlefield)
        {
            switch (dataset_index)
            {
                case Constant.TWENTY_NEWS_GROUP:
                    bodyfield = Constant.PLAIN_TEXT_FIELD_NAME;
                    titlefield = Constant.TITLE;
                    break;
                case Constant.NEW_YORK_TIMES:
                    bodyfield = Constant.NewYorkTimesDataFields.Body;
                    titlefield = Constant.NewYorkTimesDataFields.Headline;
                    break;
                case Constant.INDEXED_BING_NEWS:
                    bodyfield = Constant.IndexedBingNewsDataFields.NewsArticleDescription;
                    titlefield = Constant.IndexedBingNewsDataFields.NewsArticleHeadline;
                    break;
                default:
                    throw new Exception("Global Feature Vectors not supported in this dataset!");
            }
        }


        private void SuppressWords()
        {
            if (indexpath.ToLower().Contains("obama"))
            {
                Console.WriteLine("Suppress obama keywords");
                Dictionary<int, double> suppresswordindices = GetObamaSuppressWords();

                foreach (SparseVectorList vector in featurevectors)
                {
                    int pointer = 0, newvalue = -1;
                    foreach (int wordindex in vector.keylist)
                    {
                        if (suppresswordindices.ContainsKey(wordindex))
                        {
                            newvalue = (int)Math.Round(vector.valuelist[pointer] / suppresswordindices[wordindex]);
                            if (newvalue == 0) newvalue = 1;
                            vector.valuelist[pointer] = newvalue;
                            //if (newvalue == 0)
                            //{
                            //    vector.keylist.RemoveAt(pointer);
                            //    vector.valuelist.RemoveAt(pointer);
                            //}
                        }
                        pointer++;
                    }
                }
            }
        }



        private void SuppressWords_Prev()
        {
            string suppressword = "windows";
            int suppresswordindex = -1;

            if (lexicon.ContainsKey(suppressword))
                suppresswordindex = lexicon[suppressword];
            else
                Console.WriteLine("Warning! Suppressword '{0}' does not exist!", suppressword);

            if (suppresswordindex < 0)
                return;

            foreach (SparseVectorList vector in featurevectors)
            {
                int pointer = 0, newvalue = -1;
                foreach (int wordindex in vector.keylist)
                {
                    if (wordindex == suppresswordindex)
                    {
                        newvalue = (int)Math.Round(vector.valuelist[pointer] / suppressRatio);
                        if (newvalue == 0) newvalue = 1;
                        vector.valuelist[pointer] = newvalue;
                        break;
                    }
                    pointer++;
                }
                if (newvalue == 0)
                {
                    vector.keylist.RemoveAt(pointer);
                    vector.valuelist.RemoveAt(pointer);
                }
            }
        }


        public Dictionary<int, double> GetObamaSuppressWords()
        {

            Dictionary<string, double> suppresswords = new Dictionary<string, double>();
            suppresswords.Add("romney", 3);
            suppresswords.Add("tax", 10);
            suppresswords.Add("fiscal", 10);
            suppresswords.Add("cliff", 10);

            ////suppresswords.Add("obama", 1972927);
            //suppresswords.Add("president", 1215267);
            //suppresswords.Add("romney", 1006085);
            //suppresswords.Add("election", 569790);
            //suppresswords.Add("state", 559556);
            //suppresswords.Add("percent", 458295);
            //suppresswords.Add("house", 408295);
            //suppresswords.Add("republican", 404427);
            ////suppresswords.Add("barack", 395930);
            //suppresswords.Add("campaign", 389160);
            //suppresswords.Add("tax", 380448);
            //suppresswords.Add("government", 367219);
            //suppresswords.Add("states", 358602);
            //suppresswords.Add("voters", 303500);
            //suppresswords.Add("american", 301155);
            //suppresswords.Add("presidential", 297396);
            //suppresswords.Add("debate", 295349);
            //suppresswords.Add("mitt", 294326);
            //suppresswords.Add("vote", 288695);
            //suppresswords.Add("political", 280993);
            //suppresswords.Add("republicans", 272842);
            //suppresswords.Add("country", 272528);
            //suppresswords.Add("party", 267784);
            //suppresswords.Add("national", 256737);
            //suppresswords.Add("white", 238914);
            //suppresswords.Add("million", 237431);
            //suppresswords.Add("world", 234854);
            //suppresswords.Add("week", 234490);
            //suppresswords.Add("federal", 231303);
            //suppresswords.Add("news", 223711);
            //suppresswords.Add("americans", 220446);
            //suppresswords.Add("america", 219852);
            //suppresswords.Add("washington", 219229);
            //suppresswords.Add("economy", 215763);
            //suppresswords.Add("work", 213001);
            ////suppresswords.Add("health", 210174);
            //suppresswords.Add("mr", 205638);
            //suppresswords.Add("public", 205320);
            ////suppresswords.Add("diary", 205314);
            //suppresswords.Add("united", 204791);
            //suppresswords.Add("senate", 204733);
            //suppresswords.Add("administration", 201704);
            //suppresswords.Add("fiscal", 199169);
            //suppresswords.Add("school", 198434);
            //suppresswords.Add("democrats", 198063);
            //suppresswords.Add("policy", 195799);
            //suppresswords.Add("york", 193421);
            //suppresswords.Add("long", 189541);
            //suppresswords.Add("office", 189089);
            //suppresswords.Add("democratic", 188556);
            //suppresswords.Add("2012", 188024);
            ////suppresswords.Add("care", 182364);
            ////suppresswords.Add("spending", 181341);
            //suppresswords.Add("congress", 180497);
            //suppresswords.Add("security", 179574);
            //suppresswords.Add("group", 178238);
            //suppresswords.Add("law", 178132);
            //suppresswords.Add("tuesday", 174148);
            //suppresswords.Add("economic", 172060);
            //suppresswords.Add("cliff", 171549);
            //suppresswords.Add("cuts", 170114);
            ////suppresswords.Add("race", 165898);
            //suppresswords.Add("part", 163333);
            ////suppresswords.Add("jobs", 161847);
            //suppresswords.Add("city", 161064);
            //suppresswords.Add("man", 160979);
            //suppresswords.Add("night", 160864);
            //suppresswords.Add("candidate", 159371);
            //suppresswords.Add("term", 156012);
            //suppresswords.Add("called", 156004);
            //suppresswords.Add("nation", 155324);

            Dictionary<int, double> suppresswordindices = new Dictionary<int, double>();
            foreach (KeyValuePair<string, double> kvp in suppresswords)
            {
                //double suppressratio = kvp.Value / 100000;
                //suppresswordindices.Add(lexicon[kvp.Key], suppressRatio);
                suppresswordindices.Add(lexicon[kvp.Key], kvp.Value);
            }

            return suppresswordindices;
        }

    }
}
