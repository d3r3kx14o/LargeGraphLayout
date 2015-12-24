using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Constants;
using System.Collections;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Tokenattributes;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace RoseTreeTaxonomy.ReadData
{
    public class LoadFeatureVectors
    {
        StreamReader filereader;
        StreamReader sampleitemsreader;
        StreamReader samplenumreader;

        public Dictionary<int, int> wordfrequencycount = new Dictionary<int, int>();
        public Dictionary<int, int> wordappearancecount = new Dictionary<int, int>();
        public Dictionary<string, int> lexicon = new Dictionary<string, int>();
        public Dictionary<int, string> invertlexicon = new Dictionary<int, string>();

        protected List<int> sampleitems = new List<int>();
        List<string> unsampledlines = new List<string>();
        List<int> unsampledlabels = new List<int>();
        List<string> unsampleddocids = new List<string>();

        protected string[] samplelines;
        protected int[] samplelabels;
        protected string[] sampledocids;
        protected Dictionary<string, int> labelHash;

        public int dataset_index;
        public int model_index;
        public bool bdefaultpath;
        public int samplenum;
        public int samplelineindexcount = 0;
        public int lexiconindexcount = 0;
        public int wordnum = 0;
        public int maxdimensionvalue;
        public int featurevectorsnum;
        public int lexiconsize;

        public string samplepath;
        public string samplefilename;
        public string datapath;

        public string defaultqueryfield;
        public string querystring;

        public SparseVectorList[] featurevectors;
        //public double[] idf, idf_norm;
        public Dictionary<int, double> idf;

        public Hashtable stophash;
        Version version = Version.LUCENE_29;

        //To ensure that the filename is unique
        public string featurevectorfilename { get; protected set; }

        public LoadFeatureVectors(int dataset_index, int model_index)
        {
            this.dataset_index = dataset_index;
            this.model_index = model_index;
            this.bdefaultpath = true; //total
        }

        public LoadFeatureVectors(int dataset_index, int model_index, string news_filename,
            string sample_filename, string featurevector_path, int sample_num)
        {
            this.dataset_index = dataset_index;
            this.model_index = model_index;
            this.bdefaultpath = false;

            this.samplefilename = sample_filename;
            //initialize by give file name
            if (dataset_index == Constant.BING_NEWS)
                this.filereader = new StreamReader(news_filename);
            this.sampleitemsreader = new StreamReader(sample_filename);
            this.samplenum = sample_num;
            this.samplepath = featurevector_path;
            InitializeSampleItems();
            InitializeFeatureVectors();
        }

        public LoadFeatureVectors(int dataset_index, int model_index, string news_filename,
    string sample_filename, string featurevector_path, int sample_num,
            string defaultfield, string querystring)
        {
            this.dataset_index = dataset_index;
            this.model_index = model_index;
            this.bdefaultpath = false;

            this.samplefilename = sample_filename;
            this.defaultqueryfield = defaultfield;
            this.querystring = querystring;
            //initialize by give file name
            if (dataset_index == Constant.BING_NEWS)
                this.filereader = new StreamReader(news_filename);
            this.sampleitemsreader = new StreamReader(sample_filename);
            this.samplenum = sample_num;
            this.samplepath = featurevector_path;
            InitializeSampleItems();
            InitializeFeatureVectors();
        }

        public virtual void Load(string outputpath)
        {
            switch (dataset_index)
            {
                case Constant.CONCEPTUALIZE: LoadConceptData(outputpath); break;
                case Constant.BING_NEWS: LoadBingNewsData(outputpath); break;
                case Constant.TWENTY_NEWS_GROUP: Load20NewsGroupData(outputpath); break;
                case Constant.HAOS_DATA_SET: LoadHaosData(outputpath); break;
                case Constant.NEW_YORK_TIMES: LoadNewYorkTimesData(outputpath); break;
                case Constant.INDEXED_BING_NEWS: LoadIndexedBingNewsData(outputpath); break;
                default: break;
            }
            FeatureSelection();     //remove features with 0 occurrences
            ResizeFeatureVectors(); //remove feature vectors with 0 features
            SumUpFeatureVectors();  //calculate words' global occurrences, update maxdimensionvalue
            PostProcessData();      //change data from list to array, calculate sum(occurrences) (since data will not change any more)
            ReLabelKeys();          //change index: key in feature vectors are now coordinated with keys in wordappearancecount, from 0 to end continously
            if (model_index == Constant.VMF)
                ComputeIDF();
            //this.lexiconsize = this.wordappearancecount.Count;
            //Xiting, lexiconsize = datadimension, modified to adapt to data prediction
            this.lexiconsize = this.lexicon.Count;
            GetNorm();              //calculate norm of vector value
            //WriteFeatureVectors();//Xiting
        }

        public void LoadConceptData(string outputpath)
        {
            Initialize(outputpath);
            LoadSampleLinesConceptBingNews();
            ProcessConceptData();
        }

        public void LoadBingNewsData(string outputpath)
        {
            if (bdefaultpath)
                Initialize(outputpath);
            LoadSampleLinesConceptBingNews();
            ProcessBingNews20NewsGroupData();
        }

        public void Load20NewsGroupData(string outputpath)
        {
            if(bdefaultpath)
                Initialize(outputpath);
            else
                this.datapath = outputpath;
            PreProcess20NewsGroupData();
            LoadSampleLines20NewsGroup();
            ProcessBingNews20NewsGroupData();

            for (int i = 0; i < samplenum; i++)
                this.featurevectors[i].label = this.samplelabels[i];
        }

        public void LoadHaosData(string outputpath)
        {
            InitializePaths(outputpath);
            ProcessHaosData();
        }

        public void LoadNewYorkTimesData(string outputpath)
        {
            if (bdefaultpath)
                Initialize(outputpath);
            else
                this.datapath = outputpath;
            PreProcessNewYorkTimesData();
            //LoadSampleLines20NewsGroup();
            ProcessBingNews20NewsGroupData();

            for (int i = 0; i < samplenum; i++)
                this.featurevectors[i].label = this.samplelabels[i];
        }

        public void LoadIndexedBingNewsData(string outputpath)
        {
            if (bdefaultpath)
                Initialize(outputpath);
            else
                this.datapath = outputpath;
            PreProcessIndexedBingNewsData();
            //LoadSampleLines20NewsGroup();
            ProcessBingNews20NewsGroupData();

            //for (int i = 0; i < samplenum; i++)
            //    this.featurevectors[i].label = this.samplelabels[i];
        }
        #region Initialize

        public void Initialize(string outputpath)
        {
            InitializePaths(outputpath);
            InitializeReaders(this.datapath + Constant.inputfilenames[this.dataset_index], this.samplepath + "sampleitems.txt", this.samplepath + "samplenum.txt");
            InitializeSampleNum();
            InitializeSampleItems();
            InitializeFeatureVectors();
        }

        public void InitializePaths(string samplepath)
        {
            this.datapath = Constant.pathnames[this.dataset_index];
            this.samplepath = samplepath;
        }

        public void InitializeReaders(string file_reader_path, string sample_items_reader_path, string sample_num_reader_path)
        {
            if (dataset_index != 2)
                this.filereader = new StreamReader(file_reader_path);
            this.sampleitemsreader = new StreamReader(sample_items_reader_path);
            this.samplenumreader = new StreamReader(sample_num_reader_path);
        }

        public void InitializeSampleNum()
        {
            this.samplenum = int.Parse(this.samplenumreader.ReadLine());
            this.samplenumreader.Close();
        }

        public void InitializeSampleItems()
        {
            while (this.sampleitemsreader.Peek() != -1)
            {
                int sampleitem = int.Parse(this.sampleitemsreader.ReadLine());
                this.sampleitems.Add(sampleitem);
            }
            this.sampleitemsreader.Close();
        }

        public void InitializeFeatureVectors()
        {
            this.featurevectors = new SparseVectorList[this.samplenum];
        }

        #endregion

        public void LoadSampleLinesConceptBingNews()
        {
            this.samplelines = new string[samplenum];

            for (int i = 0; i < this.samplenum; i++)
            {
                int sampleitem = this.sampleitems[i];
                string line;

                while (this.samplelineindexcount < sampleitem)
                {
                    line = this.filereader.ReadLine();
                    this.samplelineindexcount++;
                }
#if PrintDetailedProcess
                if (i % 10000 == 0)
                    Console.WriteLine("Loading the " + i + "th sample line");
#endif
                line = this.filereader.ReadLine();
                this.samplelineindexcount++;
                this.samplelines[i] = line;
            }

            this.filereader.Close();
        }

        public void LoadSampleLines20NewsGroup()
        {
            this.samplelines = new string[samplenum];
            this.samplelabels = new int[samplenum];
            this.sampledocids = new string[samplenum];

            for (int i = 0; i < this.samplenum; i++)
            {
                int sampleitem = this.sampleitems[i];
                //while (this.samplelineindexcount < sampleitem)
                //    this.samplelineindexcount++;
                //if (this.samplelineindexcount < sampleitem) //xiting
                //    this.samplelineindexcount = sampleitem;

                //this.samplelines[i] = this.unsampledlines[this.samplelineindexcount];
                //this.samplelabels[i] = this.unsampledlabels[this.samplelineindexcount];
                //this.sampledocids[i] = this.unsampleddocids[this.samplelineindexcount];
                //this.samplelineindexcount++;
                //    this.samplelineindexcount++;

                //xiting
                this.samplelines[i] = this.unsampledlines[sampleitem];
                this.samplelabels[i] = this.unsampledlabels[sampleitem];
                this.sampledocids[i] = this.unsampleddocids[sampleitem];
                //this.samplelineindexcount++;
            }
        }

        public void PreProcess20NewsGroupData()
        {
            IndexSearcher searcher = null;
            try
            {
                //searcher = new IndexSearcher(this.datapath + Constant.inputfilenames[this.dataset_index]);
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(this.datapath + Constant.inputfilenames[this.dataset_index]));
                searcher = new IndexSearcher(directory, true);
            }
            catch (Exception e)
            {
                this.featurevectorsnum = 0;
                Console.WriteLine(e.Message);
            }

            //QueryParser queryparser = new QueryParser("newsgroup", new StandardAnalyzer());
            Version version = Version.LUCENE_24;
            QueryParser queryparser = new QueryParser(version, "newsgroup", new StandardAnalyzer(version));
            
            labelHash = new Dictionary<string, int>();
            int globalLabelID = 0;
            int docIndex = 0;
            string queryStr = "*:*";

            try
            {
                Query query = queryparser.Parse(queryStr);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;
                
                //Document testdoc = searcher.Doc(0);
                //foreach (Field field in testdoc.GetFields())
                //    Console.WriteLine(field.Name());

                foreach (ScoreDoc doc in docs)
                {
                    Document document = searcher.Doc(doc.doc);
                    string plain = document.Get(Constant.PLAIN_TEXT_FIELD_NAME);
                    string title = document.Get(Constant.TITLE);
                    this.unsampledlines.Add(" " + "\a" + title + "\a" + plain + "\a");

                    if (plain.Contains('\a') || title.Contains('\a'))
                    {
//                        int stop;
                    }

                    this.unsampleddocids.Add(doc.doc + "");

                    string labelStr = document.Get("newsgroup");
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
                    this.unsampledlabels.Add(label);
#if PrintDetailedProcess
                    if (docIndex % 5000 == 0)
                    {
                        Console.WriteLine(">>>[LOG]: Loaded String " + docIndex + " documents.");
                    }
#endif
                    docIndex++;
                }
            }
            catch (Exception ex)
            {
                this.unsampledlabels = null;
                this.unsampledlines = null;
                Console.WriteLine(ex.Message);
            }
        }

        public virtual void PreProcessNewYorkTimesData()
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

                    string plain = document.Get(Constant.NewYorkTimesDataFields.Body);
                    string title = document.Get(Constant.NewYorkTimesDataFields.Headline);
                    //this.unsampledlines.Add(" " + "\a" + title + "\a" + plain);
                    samplelines[isample] = " " + "\a" + title + "\a" + plain + "\a";

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


        public void PreProcessIndexedBingNewsData()
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
            //int globalLabelID = 0;
#if PrintDetailedProcess
            int docIndex = 0;
#endif
            string queryStr = this.querystring;

            try
            {
                Query query = queryparser.Parse(queryStr);
                //TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                TopDocs hits = searcher.Search(query, null, (int)Math.Min(searcher.MaxDoc(), Constant.SampleNumberMultiplier * samplenum));
                ScoreDoc[] docs = hits.scoreDocs;

                //Document testdoc = searcher.Doc(0);
                //foreach (Field field in testdoc.GetFields())
                //    Console.WriteLine(field.Name());

                this.samplelines = new string[samplenum];
                //this.samplelabels = new int[samplenum];
                this.sampledocids = new string[samplenum];

                for (int isample = 0; isample < sampleitems.Count; isample++)
                {
                    ScoreDoc doc = docs[sampleitems[isample]];
                    Document document = searcher.Doc(doc.doc);
                    string plain = document.Get(Constant.IndexedBingNewsDataFields.NewsArticleDescription);
                    string title = document.Get(Constant.IndexedBingNewsDataFields.NewsArticleHeadline);
                    //this.unsampledlines.Add(" " + "\a" + title + "\a" + plain);
                    //samplelines[isample] = " " + "\a" + title + "\a" + plain;
                    //Hao
                    int titleweight = Constant.BingNewsTitleWeight;
                    int leadingweight = Constant.BingNewsLeadingParagraphWeight;
                    int bodyweight = Constant.BingNewsBodyWeight;
                    if (Constant.BingNewsWeightLengthNormalization)
                    {
                        if (title == null) title = "";
                        if (plain == null) plain = "";
                        string leading = GetLeadingParagraph(plain);
                        if(leading == null) leading = "";
                        int titlelength = title.Length + 1;
                        int leadinglength = leading.Length + 1;
                        int bodylength = plain.Length - leading.Length + 2;
                        titleweight = (int)((titleweight + titleweight * 56.4155 / titlelength) / 2);
                        leadingweight = (int)((leadingweight + (leadingweight * 784.095678 / leadinglength)) / 2);
                        bodyweight = (int)((bodyweight + (bodyweight * 1601.4903 / bodylength)) / 2);
                        if (titleweight == 0) titleweight = 1;
                        if (titleweight > 10) titleweight = 10;
                        if (bodyweight == 0 && Constant.BingNewsBodyWeight != 0) bodyweight = 1;
                        if (bodyweight > 10) bodyweight = 10;
                        if (leadingweight < bodyweight) leadingweight = bodyweight;
                        if (leadingweight > 10) leadingweight = 10;
                        //Console.WriteLine("{0},{1},{2}", titleweight, leadingweight, bodyweight);
                    }
                    StringBuilder titles = new StringBuilder(title + " ");
                    if (titleweight > 1)
                        for (int i = 1; i < Constant.BingNewsTitleWeight; i++)
                            titles.Append(title + " ");
                    StringBuilder sb = new StringBuilder(" \a");
                    sb.Append(titles.ToString());
                    sb.Append("\a");
                    if (leadingweight > bodyweight)
                    {
                        var leadingPara = GetLeadingParagraph(plain);
                        for (int i = bodyweight; i < leadingweight; i++)
                            sb.Append(leadingPara + " ");
                        sb.Append("\a");
                    }
                    if (bodyweight > 0)
                        for (int i = 0; i < bodyweight; i++)
                            sb.Append(plain + " ");
                    samplelines[isample] = sb.ToString();

                    string docid = string.Format("[{0}]", doc.doc);
                    docid += document.Get(Constant.IndexedBingNewsDataFields.DocumentId);
                    sampledocids[isample] = docid;

                    //this.unsampledlabels.Add(docIndex);
#if PrintDetailedProcess
                    if (isample % 5000 == 0)
                    {
                        Console.WriteLine(">>>[LOG]: Loaded String " + docIndex + " documents.");
                    }
#endif
                    //docIndex++;
                }
            }
            catch (Exception ex)
            {
                this.samplelines = null;
                this.samplelabels = null;
                this.sampledocids = null;
                Console.WriteLine(ex.Message);
            }
        }

        protected string GetLeadingParagraph(string plain)
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
        private string GetRemainBody(string plain)
        {
            var contents = plain.Split('.', '?', '!');
            StringBuilder sb = new StringBuilder();
            for (int i = Constant.LeadingParaSentenseNum; i < contents.Length; i++)
            {
                sb.Append(contents[i]);
                sb.Append('.');
            }
            return sb.ToString();
        }

        protected string GetTransferedNewYorkTimesLabel(string labelStr)
        {
            string[] tokens = labelStr.Split('/');
            return tokens[1] + "." + tokens[2];
        }

        public void ProcessConceptData()
        {
            this.featurevectors = new SparseVectorList[samplenum];
            for (int i = 0; i < samplenum; i++)
                this.featurevectors[i] = null;

            for (int i = 0; i < samplenum; i++)
            {
                string[] queryconceptcontexttokens = this.samplelines[i].Split('\t');
                if (queryconceptcontexttokens[1].Length == 0 && queryconceptcontexttokens[2].Length == 0) continue;

                this.featurevectors[i] = new SparseVectorList(model_index);
                this.featurevectors[i].querystring = queryconceptcontexttokens[0];

                for (int j = 1; j < 2; j++)
                {
                    string[] tokens = queryconceptcontexttokens[j].Split(';');

                    for (int k = 0; k < tokens.Length - 1; k++)
                    {
                        if (tokens[k].Length == 0) continue;

                        string key = null;
                        int value = -1;
                        int lexiconindex_count_out;

                        for (int l = tokens[k].Length - 1; l >= 0; l--)
                            if (tokens[k][l].Equals(':') == true)
                            {
                                key = tokens[k].Substring(0, l);
                                value = int.Parse(tokens[k].Substring(l + 1));
                                break;
                            }

                        if (key.Length == 0) continue;

                        if (this.lexicon == null || this.lexicon.TryGetValue(key, out lexiconindex_count_out) == false)
                        {
                            this.lexicon.Add(key, this.lexiconindexcount);
                            this.invertlexicon.Add(this.lexiconindexcount, key);
                            this.featurevectors[i].Add(this.lexiconindexcount, value);
                            this.wordappearancecount.Add(this.lexiconindexcount, 1);
                            this.lexiconindexcount++;
                        }
                        else
                        {
                            this.featurevectors[i].Insert(lexiconindex_count_out, value);
                            this.wordappearancecount[lexiconindex_count_out]++;
                        }
                        this.wordnum += value;
                    }
                }
            }
        }

        public void ProcessHaosData()
        {
            string[] filenames = System.IO.Directory.GetFiles(this.datapath);
            this.featurevectors = new SparseVectorList[filenames.Length];
            this.samplenum = filenames.Length;

            for (int i = 0; i < filenames.Length; i++)
            {
                StreamReader cluster_reader = new StreamReader(filenames[i]);

                this.featurevectors[i] = new SparseVectorList(this.model_index);
                this.featurevectors[i].querystring = filenames[i].Replace(this.datapath, "");

                while (cluster_reader.Peek() != -1)
                {
                    string line = cluster_reader.ReadLine();
                    string[] tokens = line.Split(' ');
                    string key = tokens[0];
                    int value = int.Parse(tokens[1]);
                    int lexiconindex_count_out;

                    if (this.lexicon == null || this.lexicon.TryGetValue(key, out lexiconindex_count_out) == false)
                    {
                        this.lexicon.Add(key, this.lexiconindexcount);
                        this.invertlexicon.Add(this.lexiconindexcount, key);
                        this.featurevectors[i].Add(this.lexiconindexcount, value);
                        this.wordappearancecount.Add(this.lexiconindexcount, 1);
                        this.lexiconindexcount++;
                    }
                    else
                    {
                        this.featurevectors[i].Insert(lexiconindex_count_out, value);
                        this.wordappearancecount[lexiconindex_count_out]++;
                    }
                    this.wordnum += value;
                }
            }
        }

        //Modified by Xiting to get rid of obsolete usages
        public virtual void ProcessBingNews20NewsGroupData()
        {
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
                {
                    throw new Exception("Cannot determine stop words!");
                }
            }
            else
                stophash = StopFilter.MakeStopSet(StopWords.stopwords);

            for (int i = 0; i < samplenum; i++)
            {
#if PrintDetailedProcess
                if (i % 10000 == 0)
                    Console.WriteLine("Processing the " + i + "th data into feature vector");
#endif
                this.featurevectors[i] = new SparseVectorList(model_index);
                char separator = (dataset_index == 1) ? '\t' : '\a';
                string[] querylinetokens = samplelines[i].Split(separator);

                if (querylinetokens.Length < 3) continue;
                this.featurevectors[i].querystring = querylinetokens[1];
                StringReader reader = new StringReader(querylinetokens[2]);
                TokenStream result = new StandardTokenizer(Lucene.Net.Util.Version.LUCENE_24, reader);

                List<int> featureSeq = new List<int>();
                result = new StandardFilter(result);
                result = new LowerCaseFilter(result);
                result = new StopFilter(true, result, stophash, true);

                //Lucene.Net.Analysis.Token token = result.Next();
                result.Reset();
                TermAttribute termattr = (TermAttribute) result.GetAttribute(typeof(TermAttribute));
                while (result.IncrementToken())
                {
                    string termtext = termattr.Term();
                    int value = 0;
                    if (lexicon == null || this.lexicon.TryGetValue(termtext, out value) == false)
                    {
                        this.lexicon.Add(termtext, this.lexiconindexcount);
                        this.invertlexicon.Add(this.lexiconindexcount, termtext);
                        featureSeq.Add(this.lexiconindexcount);
                        this.lexiconindexcount++;
                    }
                    else
                    {
                        featureSeq.Add(value);
                    }
                    
                    this.wordnum++;
                }

                for (int j = 0; j < featureSeq.Count; j++)
                {
                    if(!featurevectors[i].Increase(featureSeq[j], 1))
                    {
                        featurevectors[i].Insert(featureSeq[j], 1);
                        if (wordappearancecount.ContainsKey(featureSeq[j]) == true)
                            wordappearancecount[featureSeq[j]]++;
                        else
                            wordappearancecount.Add(featureSeq[j], 1);
                    }
                }
            }
        }

        public void FeatureSelection()
        {
            for (int i = 0; i < this.samplenum; i++)
                if (this.featurevectors[i] != null)
                {
                    for (int j = 0; j < this.featurevectors[i].keylist.Count; j++)
                    {
                        int key = this.featurevectors[i].keylist[j];

                        if (/*this.wordappearancecount[key] > 0.6 * this.samplenum || */this.featurevectors[i].valuelist[j] == 0)
                        {
                            this.featurevectors[i].RemoveAt(j);
                            j--;
                        }
                    }
                }
        }

        //Xiting, adjust samplelabels together with feature vectors
        public List<int> remainedindexes = new List<int>();
        public void ResizeFeatureVectors()
        {
            if (samplelabels == null)
            {
                ResizeFeatureVectorsNoLabel();
            }
            else
            {
                List<SparseVectorList> featurevectorsbuffer = new List<SparseVectorList>();
                List<int> samplelabelsbuffer = new List<int>();
                //List<int> sampleitemsbuffer = new List<int>();
                //List<string> samplelinesbuffer = new List<string>();

                for (int i = 0; i < this.samplenum; i++)
                {
                    if (this.featurevectors[i] != null && this.featurevectors[i].keylist.Count != 0)
                    {
                        featurevectorsbuffer.Add(this.featurevectors[i]);
                        samplelabelsbuffer.Add(samplelabels[i]);
                        //sampleitemsbuffer.Add(sampleitems[i]);
                        //samplelinesbuffer.Add(samplelines[i]);
                        //remainedindexes.Add(i);
                    }
                }

                this.featurevectors = featurevectorsbuffer.ToArray();
                this.featurevectorsnum = this.featurevectors.Length;

                this.samplelabels = samplelabelsbuffer.ToArray();
                //this.sampleitems = sampleitemsbuffer;
                //this.samplelines = samplelinesbuffer.ToArray();
            }
        }

        public void ResizeFeatureVectorsNoLabel()
        {
            List<SparseVectorList> featurevectorsbuffer = new List<SparseVectorList>();

            for (int i = 0; i < this.samplenum; i++)
            {
                if (this.featurevectors[i] != null && this.featurevectors[i].keylist.Count != 0)
                    featurevectorsbuffer.Add(this.featurevectors[i]);
            }

            this.featurevectors = featurevectorsbuffer.ToArray();
            this.featurevectorsnum = this.featurevectors.Length;
        }

        public void SumUpFeatureVectors()
        {
            for (int i = 0; i < this.featurevectorsnum; i++)
            {
                for (int j = 0; j < this.featurevectors[i].keylist.Count; j++)
                {
                    int key = this.featurevectors[i].keylist[j];
                    int value = this.featurevectors[i].valuelist[j];

                    if (this.wordfrequencycount.ContainsKey(key) == true)
                        this.wordfrequencycount[key] += value;
                    else
                        this.wordfrequencycount.Add(key, value);
                }
            }

            foreach (KeyValuePair<int, int> kvp in this.wordfrequencycount)
            {
                if (kvp.Value > maxdimensionvalue)
                    maxdimensionvalue = kvp.Value;
            }
        }

        public void PostProcessData()
        {
            for (int i = 0; i < featurevectorsnum; i++)
            {
                this.featurevectors[i].ListToArray();
                this.featurevectors[i].count = this.featurevectors[i].keyarray.Length;
                this.featurevectors[i].SumUpValueArray();
                this.featurevectors[i].InvalidateList();
            }
        }

        public void ReLabelKeys()
        {
            Dictionary<int, int> new_key_map = new Dictionary<int, int>();
            int pt = 0;
            int[] keyarray = this.wordappearancecount.Keys.ToArray();
            int[] valuearray = this.wordappearancecount.Values.ToArray();
            for (int i = 0; i < this.wordappearancecount.Count; i++)
                //if (!(false/*valuearray[i] > 0.6 * this.samplenum*/))
                {
                    new_key_map.Add(keyarray[i], pt);
                    pt++;
                }
            for (int i = 0; i < this.featurevectorsnum; i++)
                for (int j = 0; j < this.featurevectors[i].count; j++)
                    this.featurevectors[i].keyarray[j] = new_key_map[this.featurevectors[i].keyarray[j]];
            
            Dictionary<int, int> new_wordapperancecount = new Dictionary<int, int>();
            for(int i = 0; i < wordappearancecount.Count; i++)
                if (!(false/*valuearray[i] > 0.6 * this.samplenum*/))
                    new_wordapperancecount.Add(new_key_map[keyarray[i]], valuearray[i]);
            this.wordappearancecount = new_wordapperancecount;
        }

        //Xiting: Previous IDF by Xueqing
        public void ComputeIDF_Org()
        {
            //this.idf = new double[this.wordappearancecount.Count];
            //this.idf_norm = new double[this.wordappearancecount.Count];

            //this.idf = new double[this.wordappearancecount.Count];
            //this.idf_norm = new double[this.wordappearancecount.Count];
            //double norm = 0;
            //for (int i = 0; i < this.wordappearancecount.Count; i++)
            //{
            //    this.idf[i] = 1;// Math.Log((double)this.featurevectorsnum / this.wordappearancecount[i]);
            //    norm += this.idf[i] * this.idf[i];
            //}
            //norm = Math.Sqrt(norm);
            //if (norm != 0)
            //    for (int i = 0; i < this.wordappearancecount.Count; i++)
            //        this.idf_norm[i] = this.idf[i] / norm;
        }

        public void ComputeIDF()
        {
            this.idf = new Dictionary<int, double>();
            double N = featurevectors.Length;

            foreach (int termindex in wordappearancecount.Keys)
                //this.idf.Add(termindex, 1);
                this.idf.Add(termindex, Math.Log(N / wordappearancecount[termindex]));
        }

        public void GetNorm()
        {
            for (int i = 0; i < featurevectorsnum; i++)
                this.featurevectors[i].GetNorm(this.idf, this.model_index);
        }

        public void WriteFeatureVectors()
        {
            if (model_index == Constant.VMF)
            {
                WriteFeatureVectorsvMF();
                return;
            }

            try
            {
                featurevectorfilename = "featurevectors_" + DateTime.Now.Ticks + ".txt";
                StreamWriter writer = new StreamWriter(this.samplepath + featurevectorfilename);
                for (int i = 0; i < featurevectors.Length; i++)
                {
#if PrintDetailedProcess
                    if (i % 10000 == 0)
                        Console.WriteLine("Writing the " + i + "th feature vector to disk");
#endif
                    for (int j = 0; j < featurevectors[i].count; j++)
                        writer.Write(featurevectors[i].keyarray[j] + ":" + featurevectors[i].valuearray[j] + ";");
                    writer.WriteLine();
                }
                writer.Flush();
                writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Warning: Cannot write feature vectors!");
            }
        }

        public void WriteFeatureVectorsvMF()
        {
            try
            {
                featurevectorfilename = "featurevectors_" + DateTime.Now.Ticks + ".txt";
                StreamWriter writer = new StreamWriter(this.samplepath + featurevectorfilename);
                for (int i = 0; i < featurevectors.Length; i++)
                {
#if PrintDetailedProcess
                    if (i % 10000 == 0)
                        Console.WriteLine("Writing the " + i + "th feature vector to disk");
#endif
                    for (int j = 0; j < featurevectors[i].count; j++)
                        writer.Write(featurevectors[i].keyarray[j] + ":" + featurevectors[i].l2normedvaluearray[j] + ";");
                    writer.WriteLine();
                }
                writer.Flush();
                writer.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Warning: Cannot write feature vectors!");
            }
        }

        //public void Nullify()
        //{
        //    this.wordfrequencycount = null;
        //    this.wordappearancecount = null;
        //    this.lexicon = null;
        //    this.sampleitems = null;
        //    this.unsampledlines = null;
        //    this.unsampledlabels = null;
        //    this.samplelines = null;
        //    this.samplelabels = null;
        //    this.featurevectors = null;
        //    this.stophash = null;
        //}

        //Xiting
        public void GetSampleLabels(out int[] labels, out Dictionary<string,int> labelhash)
        {
            labels = this.samplelabels;
            labelhash = this.labelHash;
        }

        public string[] GetSampleDocIds()
        {
            return sampledocids;
        }

        public IList<int> GetSampleItems()
        {
            return sampleitems.AsReadOnly();
        }

        public string[] GetSampleLines()
        {
            return samplelines;
        }

        public string GetSampleLine(int initialindex)
        {
            return samplelines[initialindex];
        }
    }
}
