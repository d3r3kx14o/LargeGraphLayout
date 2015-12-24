using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.Constants;

using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.DataStructures;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Tokenattributes;
using SystemDiretory = System.IO.Directory;
using LuceneDirectory = Lucene.Net.Store.Directory;
using Version = Lucene.Net.Util.Version;

namespace EvolutionaryRoseTree
{
    class BuildRoseTree
    {
        static Dictionary<int, int> NewsNumber = null;

        public static int SampleNumber = 1000;
        public static double SampleNumberRatio = 0;
        static bool BLoadSampleDataFromFile = true;
        public static string CurrentTimeStamp() { return String.Format("{0:MMdd_HHmmss}", DateTime.Now);  }
        public static StreamWriter ViolationCurveFile = null;

        public static string CacheValueRecordFileName = null;
        public static List<Constraint> SmoothCostConstraints = null;
        public static int BingNewsTitleWeight = 3;
        public static int BingNewsLeadingParagraphWeight = 1;
        public static int BingNewsBodyWeight = 0;
        public static bool BingNewsWeightLengthNormalization = false;

        public static bool BRestrictBinary = false;

        public static LoadDataInfo LoadBingNewsData(string news_path, string sample_path, int time,
            int dataset_index, int model_index, int sample_times, int total_num_news = -1)
        {
            /// sampling    ///
            if (total_num_news < 0)
            {
                if (NewsNumber == null)
                    InitializeNewsNumber();
                total_num_news = NewsNumber[time];
            }

            int sample_num = SampleNumber;
            if (sample_num > total_num_news)
                sample_num = total_num_news;
            string sample_filename = InitializeSamplingData(sample_path, sample_num, total_num_news, 
                sample_times);            


            /// load feature vector ///
            string news_filename = news_path + time + ".txt";
            string featurevector_path = sample_path;
            LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index,
                news_filename, sample_filename, featurevector_path, sample_num);
            lfv.Load(null);

            LoadDataInfo info = new LoadDataInfo(news_path, sample_path, time, total_num_news,
            dataset_index, model_index, sample_times, lfv);
            return info;
        }

        public static LoadDataInfo LoadTwentyNewsGroupData(string twentyNG_path, string sample_path,
            int dataset_index, int model_index, int sample_times, double overlap = -1)
        {
            /// sampling    ///
            int total_num_news = 19997;
            if (twentyNG_path.Contains("s7"))
                total_num_news = 7000;
            else if (twentyNG_path.Contains("9"))
                total_num_news = 9000;
            else if (twentyNG_path.Contains("13"))
                total_num_news = 13000;
            else if (twentyNG_path.Contains("17"))
                total_num_news = 17000;

            int sample_num = SampleNumber;
            if (sample_num > total_num_news)
                sample_num = total_num_news;

            string sample_filename = InitializeSamplingData
                (sample_path, sample_num, total_num_news, sample_times, overlap);

            /// load feature vector ///
            string featurevector_path = sample_path;
            //LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index,
            //    null, sample_filename, featurevector_path, sample_num);
            LoadFeatureVectors lfv = new LoadGlobalFeatureVectors(dataset_index, model_index,
                    null, sample_filename, featurevector_path, sample_num);
            lfv.Load(twentyNG_path);

            LoadDataInfo info = new LoadDataInfo(twentyNG_path, sample_path, -1, total_num_news,
            dataset_index, model_index, sample_times, lfv);
            return info;
        }

        public static LoadDataInfo LoadNewYorkTimesGroupData(string nytindex_path, string sample_path,
            int dataset_index, int model_index, int sample_times,
            string defaultfield, string queryString, double overlapratio = -1)
        {
            if (dataset_index != RoseTreeTaxonomy.Constants.Constant.NEW_YORK_TIMES)
                throw new Exception("Only deal with new york times data!");

            /// get total_num_news of specific query///
            IndexSearcher searcher = null;
            try
            {
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(nytindex_path));
                searcher = new IndexSearcher(directory, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Version version = Version.LUCENE_24;
            QueryParser queryparser = new QueryParser(version, defaultfield, new StandardAnalyzer(version));

            int total_num_news = -1;
            try
            {
                Query query = queryparser.Parse(queryString);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;
                total_num_news = docs.Length;
                Console.WriteLine(total_num_news);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            /// get sample file name ///
            int sample_num = SampleNumber;
            if (sample_num > total_num_news)
                sample_num = total_num_news;
            else
                sample_num = (int)Math.Round((total_num_news - sample_num) * SampleNumberRatio) + sample_num;
            string sample_filename = InitializeSamplingData(sample_path, sample_num, total_num_news,
                sample_times, overlapratio);
            Console.WriteLine(sample_filename);

            /// load feature vector ///            
            string featurevector_path = sample_path;
            //LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index,
            //    nytindex_path, sample_filename, featurevector_path, sample_num, defaultfield, queryString);
            LoadFeatureVectors lfv = new LoadGlobalFeatureVectors(dataset_index, model_index,
                nytindex_path, sample_filename, featurevector_path, sample_num, defaultfield, queryString);
            lfv.Load(nytindex_path);

            LoadDataInfo info = new LoadDataInfo(nytindex_path, sample_path, -1, total_num_news,
            dataset_index, model_index, sample_times, lfv);
            
            info.default_field = defaultfield;
            info.query_string = queryString;
            return info;

        }

        public static LoadDataInfo LoadLuceneIndexedBingNewsData(string bingnews_index_path, string sample_path,
            int dataset_index, int model_index, int sample_times,
            string defaultfield, string queryString, double overlapratio = -1)
        {
            if (dataset_index != RoseTreeTaxonomy.Constants.Constant.INDEXED_BING_NEWS)
                throw new Exception("Only deal with new york times data!");
            RoseTreeTaxonomy.Constants.Constant.BingNewsTitleWeight = BingNewsTitleWeight;
            RoseTreeTaxonomy.Constants.Constant.BingNewsLeadingParagraphWeight = BingNewsLeadingParagraphWeight;
            RoseTreeTaxonomy.Constants.Constant.BingNewsBodyWeight = BingNewsBodyWeight;
            RoseTreeTaxonomy.Constants.Constant.BingNewsWeightLengthNormalization = BingNewsWeightLengthNormalization;
            
            /// get total_num_news of specific query///
            IndexSearcher searcher = null;
            try
            {
                LuceneDirectory directory = FSDirectory.Open(new DirectoryInfo(bingnews_index_path));
                searcher = new IndexSearcher(directory, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new Exception("Cannot open lucene index: " + bingnews_index_path);
            }

            Version version = Version.LUCENE_24;
            QueryParser queryparser = new QueryParser(version, defaultfield, new StandardAnalyzer(version));

            int total_num_news = -1;
            try
            {
                Query query = queryparser.Parse(queryString);
                TopDocs hits = searcher.Search(query, null, searcher.MaxDoc());
                ScoreDoc[] docs = hits.scoreDocs;
                total_num_news = docs.Length;
                Console.WriteLine(total_num_news);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            /// get sample file name ///
            // first pick up 10*sample_num most relavant document, then random sample
            int sample_num = SampleNumber;
            if (total_num_news > Constant.SampleNumberMultiplier * sample_num)
                total_num_news = Constant.SampleNumberMultiplier * sample_num;
            if (sample_num > total_num_news)
                sample_num = total_num_news;
            else
                sample_num = (int)Math.Round((total_num_news - sample_num) * SampleNumberRatio) + sample_num;
            string sample_filename = InitializeSamplingData(sample_path, sample_num, total_num_news,
                sample_times, overlapratio);


            /// load feature vector ///            
            string featurevector_path = sample_path;
            //LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index,
            //    bingnews_index_path, sample_filename, featurevector_path, sample_num, defaultfield, queryString);
            LoadFeatureVectors lfv = new LoadGlobalFeatureVectors(dataset_index, model_index,
                    bingnews_index_path, sample_filename, featurevector_path, sample_num, defaultfield, queryString);
            lfv.Load(bingnews_index_path);

            LoadDataInfo info = new LoadDataInfo(bingnews_index_path, sample_path, -1, total_num_news,
            dataset_index, model_index, sample_times, lfv);

            info.default_field = defaultfield;
            info.query_string = queryString;
            return info;
        }

        public static RoseTree BuildTree(LoadDataInfo ldinfo,
            string likelihood_path, string outputdebugpath)
        {
            return BuildTree(ldinfo, null, likelihood_path, outputdebugpath);
        }

        public static RoseTree BuildTree(LoadDataInfo ldinfo, Constraint constraint,
    string likelihood_path, string outputdebugpath)
        {
            RoseTreeParameters para = new RoseTreeParameters(); //default
            return BuildTree(ldinfo, constraint, para, likelihood_path, outputdebugpath);
        }

        public static RoseTree BuildTree(LoadDataInfo ldinfo, Constraint constraint,
            RoseTreeParameters para, string likelihood_path, string outputdebugpath)
        {
            return BuildTree(ldinfo, constraint, null, para, likelihood_path, outputdebugpath);
        }

        public static RoseTree BuildTree(LoadDataInfo ldinfo, Constraint constraint, Rules rules, 
            RoseTreeParameters para, string likelihood_path, string outputdebugpath)
        {
            int total_num_news = ldinfo.total_number_news;
            if (para.k >= total_num_news)
                para.k = total_num_news - 1;

            ConstrainedRoseTree rosetree = null;
            SystemDiretory.CreateDirectory(likelihood_path);
            //for (int i = 0; i < ldinfo.sample_times; i++)
            {
                int i = ldinfo.sample_times - 1;
                //for (model_index = Constant.DCM; model_index <= Constant.VMF; model_index++)
                {
                    //for (algorithm_index = Constant.BRT; algorithm_index <= Constant.SPILLTREE_BRT; algorithm_index++)
                    {
                        int depth;
                        double likelihood2;
                        if (!SystemDiretory.Exists(outputdebugpath))
                            SystemDiretory.CreateDirectory(outputdebugpath);
                        if (BRestrictBinary)
                        {
                            rosetree = new ConstrainedBayesionBinaryTree(
                                ldinfo.dataset_index,                      //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                                para.algorithm_index,                    //BRT,KNN_BRT,SPILLTREE_BRT
                                para.experiment_index,              //0
                                para.random_projection_algorithm_index,  //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                                ldinfo.model_index,                        //DCM,VMF,BERNOULLI
                                para.projectdimension,                   //projectdimensions[1]:50
                                para.k,                                  //k nearest neighbour
                                ldinfo.lfv,                                //load feature vector
                                para.alpha, para.gamma, para.tau, para.kappa, para.R_0,      //parameters, see top of this file
                                outputdebugpath, para.sizepunishminratio, para.sizepunishmaxratio);

                        }
                        else
                        {
                            rosetree = new ConstrainedRoseTree(
                                ldinfo.dataset_index,                      //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                                para.algorithm_index,                    //BRT,KNN_BRT,SPILLTREE_BRT
                                para.experiment_index,              //0
                                para.random_projection_algorithm_index,  //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                                ldinfo.model_index,                        //DCM,VMF,BERNOULLI
                                para.projectdimension,                   //projectdimensions[1]:50
                                para.k,                                  //k nearest neighbour
                                ldinfo.lfv,                                //load feature vector
                                para.alpha, para.gamma, para.tau, para.kappa, para.R_0,      //parameters, see top of this file
                                outputdebugpath, para.sizepunishminratio, para.sizepunishmaxratio);
                        }
                        if (CacheValueRecordFileName != null)
                            rosetree.InitializeCacheValueRecord(outputdebugpath + "CacheValue_" + CacheValueRecordFileName + ".dat");
                        if(rules!=null)
                            rosetree.SetUpRules(rules);
                        if (SmoothCostConstraints != null)
                            foreach(Constraint smoothconstraint in SmoothCostConstraints)
                                (rosetree as ConstrainedRoseTree).AddSmoothCostConstraint(smoothconstraint);
                        rosetree.Run(constraint, para.interval, out depth, out likelihood2);

                        #region output likelyhood
                        //likelihood[dataset_index][model_index][algorithm_index][i] = likelihood2;
                        //StreamWriter sw = null;
                        //try
                        //{
                        //    sw = new StreamWriter(likelihood_path + "_" +
                        //        ldinfo.dataset_index + "_" + i + "_" + ldinfo.model_index + "_" +
                        //        para.algorithm_index + ".dat");
                        //}
                        //catch 
                        //{
                        //    sw = new StreamWriter(likelihood_path + "_" +
                        //        ldinfo.dataset_index + "_" + i + "_" + ldinfo.model_index + "_" +
                        //        para.algorithm_index + "_" + CurrentTimeStamp() + ".dat");
                        //}
                        //sw.WriteLine(likelihood2);

                        //sw.Flush();
                        //sw.Close();
                        #endregion output likelyhood
                    }
                }
            }

            Console.WriteLine("likelyhood calculation done: data_index " + 
                ldinfo.dataset_index + ", model_index " + ldinfo.model_index + ", algorithm_index " + 
                para.algorithm_index);

            return rosetree;
        }

        public static RoseTree BuildGroundTruthRoseTree(LoadDataInfo ldinfo,
            string likelihood_path, string outputdebugpath)
        {
            return BuildGroundTruthRoseTree(ldinfo, new RoseTreeParameters(), likelihood_path, outputdebugpath);
        }

        public static RoseTree BuildGroundTruthRoseTree(LoadDataInfo ldinfo, 
            RoseTreeParameters para, string likelihood_path, string outputdebugpath)
        {
            int total_num_news = ldinfo.total_number_news;
            if (para.k >= total_num_news)
                para.k = total_num_news - 1;

            GroundTruthRoseTree rosetree = null;
            SystemDiretory.CreateDirectory(likelihood_path);
            //for (int i = 0; i < ldinfo.sample_times; i++)
            {
                int i = ldinfo.sample_times - 1;
                //for (model_index = Constant.DCM; model_index <= Constant.VMF; model_index++)
                {
                    //for (algorithm_index = Constant.BRT; algorithm_index <= Constant.SPILLTREE_BRT; algorithm_index++)
                    {
                        int depth;
                        double likelihood2;
                        if (!SystemDiretory.Exists(outputdebugpath))
                            SystemDiretory.CreateDirectory(outputdebugpath);
                        if (BRestrictBinary)
                        {
                            rosetree = new GroundTruthBinaryTree(
                                ldinfo.dataset_index,                      //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                                para.algorithm_index,                    //BRT,KNN_BRT,SPILLTREE_BRT
                                para.experiment_index,              //0
                                para.random_projection_algorithm_index,  //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                                ldinfo.model_index,                        //DCM,VMF,BERNOULLI
                                para.projectdimension,                   //projectdimensions[1]:50
                                para.k,                                  //k nearest neighbour
                                ldinfo.lfv,                                //load feature vector
                                para.alpha, para.gamma, para.tau, para.kappa, para.R_0,      //parameters, see top of this file
                                outputdebugpath);
                        }
                        else
                        {
                            rosetree = new GroundTruthRoseTree(
                                ldinfo.dataset_index,                      //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                                para.algorithm_index,                    //BRT,KNN_BRT,SPILLTREE_BRT
                                para.experiment_index,              //0
                                para.random_projection_algorithm_index,  //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                                ldinfo.model_index,                        //DCM,VMF,BERNOULLI
                                para.projectdimension,                   //projectdimensions[1]:50
                                para.k,                                  //k nearest neighbour
                                ldinfo.lfv,                                //load feature vector
                                para.alpha, para.gamma, para.tau, para.kappa, para.R_0,      //parameters, see top of this file
                                outputdebugpath);
                        }
                        rosetree.Run(para.interval, out depth, out likelihood2);

                        #region output likelyhood
                        //likelihood[dataset_index][model_index][algorithm_index][i] = likelihood2;
                        //StreamWriter sw = null;
                        //try
                        //{
                        //    sw = new StreamWriter(likelihood_path + "_" +
                        //        ldinfo.dataset_index + "_" + i + "_" + ldinfo.model_index + "_" +
                        //        para.algorithm_index + ".dat");
                        //}
                        //catch
                        //{
                        //    sw = new StreamWriter(likelihood_path + "_" +
                        //        ldinfo.dataset_index + "_" + i + "_" + ldinfo.model_index + "_" +
                        //        para.algorithm_index + "_" + CurrentTimeStamp() + ".dat");
                        //}
                        //sw.WriteLine(likelihood2);

                        //sw.Flush();
                        //sw.Close();
                        #endregion output likelyhood
                    }
                }
            }

            return rosetree;
        }

        #region initialization
        //make sure data is ready in sample file
        static string InitializeSamplingData(string sample_path, int sample_num, int total_num_news, int sample_times)
        {
            RoseTreeTaxonomy.Tools.Sample sample1 = new RoseTreeTaxonomy.Tools.Sample();
            int[] samplearray_news = null;;

            string sample_filename = sample_path + "_sampleitems_" + (sample_num) + "_" + (total_num_news) + "_" + (sample_times - 1) + ".txt";
            //if no sample file exists, generate and write one
            if (!File.Exists(sample_filename) || !BLoadSampleDataFromFile)
            {
                RandomGenerator.SetSeedFromSystemTime();
                sample1.Run(total_num_news, sample_num, out samplearray_news);

                SystemDiretory.CreateDirectory(sample_path);
                StreamWriter sw_news = new StreamWriter(sample_filename);

                for (int j = 0; j < sample_num; j++)
                    sw_news.WriteLine(samplearray_news[j]);

                sw_news.Flush();
                sw_news.Close();
            }

            return sample_filename;
        }

        //make sure data is ready in sample file
        public static string InitializeSamplingData(string sample_path, int sample_num, 
            int total_num_news, int sample_times, double overlap)
        {
            if (overlap > 1 || overlap < 0)
                return InitializeSamplingData(sample_path, sample_num, total_num_news, sample_times);

            RoseTreeTaxonomy.Tools.Sample sample1 = new RoseTreeTaxonomy.Tools.Sample();

            string sample_filename = sample_path + "_sampleitems_" + (sample_num) + "_" + (total_num_news) + "_" + (sample_times - 1) + ".txt";
            string sample_filename_overlap = sample_filename.Substring(0, sample_filename.Length - 4) + "_overlap_" + (int)(100 * overlap) + ".txt";
            //if no sample file exists, generate and write one
            if (!File.Exists(sample_filename) || !File.Exists(sample_filename_overlap) 
                || !BLoadSampleDataFromFile)
            {
                RandomGenerator.SetSeedFromSystemTime();

                int[] sample_array0;
                if (!File.Exists(sample_filename))
                {
                    sample1.Run(total_num_news, sample_num, out sample_array0);
                    StreamWriter sw = new StreamWriter(sample_filename);
                    for (int j = 0; j < sample_num; j++)
                        sw.WriteLine(sample_array0[j]);

                    sw.Flush();
                    sw.Close();
                }
                else
                {
                    sample_array0 = new int[sample_num];
                    StreamReader sr = new StreamReader(sample_filename);
                    for (int j = 0; j < sample_num; j++)
                        sample_array0[j] = Int32.Parse(sr.ReadLine());
                }

                SystemDiretory.CreateDirectory(sample_path);
                StreamWriter sw_overlap = new StreamWriter(sample_filename_overlap);

                int[] sample_array1;
                Sample sample = new Sample();
                sample.Run(total_num_news, sample_array0, overlap, out sample_array1);

                for (int j = 0; j < sample_num; j++)
                    sw_overlap.WriteLine(sample_array1[j]);

                sw_overlap.Flush();
                sw_overlap.Close();
            }

            return sample_filename_overlap;
        }

        static void InitializeNewsNumber()
        {
            NewsNumber = new Dictionary<int, int>();
            NewsNumber.Add(0, 8);
            NewsNumber.Add(1, 8);
            NewsNumber.Add(1841, 4);
            NewsNumber.Add(1843, 1);
            NewsNumber.Add(1845, 1);
            NewsNumber.Add(1846, 3);
            NewsNumber.Add(1849, 7);
            NewsNumber.Add(1850, 2);
            NewsNumber.Add(1851, 1);
            NewsNumber.Add(1852, 3);
            NewsNumber.Add(1853, 34539);
            NewsNumber.Add(1854, 131608);
            NewsNumber.Add(1855, 121719);
            NewsNumber.Add(1856, 90224);
            NewsNumber.Add(1857, 77158);
            NewsNumber.Add(1858, 107494);
            NewsNumber.Add(1859, 125076);
            NewsNumber.Add(1860, 137207);
            NewsNumber.Add(1861, 139190);
            NewsNumber.Add(1862, 128137);
            NewsNumber.Add(1863, 95263);
            NewsNumber.Add(1864, 83997);
            NewsNumber.Add(1865, 133232);
            NewsNumber.Add(1866, 117235);
            NewsNumber.Add(1867, 111668);
            NewsNumber.Add(1868, 66361);
        }
        #endregion initialization
    }

    class RoseTreeParameters
    {
        public int algorithm_index;

        public int projectdimension;
        public int k;
        public double alpha;
        public double gamma;
        public double kappa;
        public double R_0;

        public int experiment_index;
        public int random_projection_algorithm_index;
        public int interval;

        public double tau;
        public double sizepunishminratio;
        public double sizepunishmaxratio;

        public RoseTreeParameters()
        {
            algorithm_index = RoseTreeTaxonomy.Constants.Constant.KNN_BRT;

            RoseTreeTaxonomy.Experiments.Experiment experiment = new RoseTreeTaxonomy.Experiments.Experiment();
            //APP
            double alpha = 3;

            projectdimension = experiment.projectdimensions[1];
            k = experiment.ks[0];
            alpha = experiment.alphas[0];
            gamma = experiment.gammas[0];
            kappa = experiment.kappas[0];
            R_0 = experiment.R_0s[0];

            experiment_index = RoseTreeTaxonomy.Constants.Constant.ROSETREE_PRECISION;
            random_projection_algorithm_index = RoseTreeTaxonomy.Constants.Constant.GAUSSIAN_RANDOM;
            interval = RoseTreeTaxonomy.Constants.Constant.intervals[0];

            tau = 0.1;

            sizepunishminratio = 0.05;
            sizepunishmaxratio = 0.12;
        }
    }

    class LoadDataInfo
    {
        public string news_path;    //path/file name of data
        public string sample_path;
        public int time;
        public int dataset_index;
        public int model_index;
        public int sample_times;
        public int total_number_news;
        public LoadFeatureVectors lfv;

        public string default_field;
        public string query_string;

        public LoadDataInfo(string news_path, string sample_path, int time, int total_number_news,
            int dataset_index, int model_index, int sample_times, LoadFeatureVectors lfv)
        {
            // TODO: Complete member initialization
            this.news_path = news_path;
            this.sample_path = sample_path;
            this.time = time;
            this.total_number_news = total_number_news;
            this.dataset_index = dataset_index;
            this.model_index = model_index;
            this.sample_times = sample_times;
            this.lfv = lfv;
        }
    }
}
