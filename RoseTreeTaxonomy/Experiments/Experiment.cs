using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.DrawTree;
using System.IO;
using RoseTreeTaxonomy.DataStructures;

namespace RoseTreeTaxonomy.Experiments
{
    // BRT, kNN-BRT, Spilltree-BRT的所有precision/likelihood/NMI实验
    public class Experiment
    {
        //指定输出路径为outputpath

        public static string outputdebugpath = Constant.OUTPUT_PATH+@"vMF\debug\";
        // read in samples or generate new samples
        public static bool BLoadSampleDataFromFile = true;

        //指定数据集为第dataset_index个数据集
        public int dataset_index;// = Constant.TWENTY_NEWS_GROUP;

        //指定算法为第algorithm_index种算法
        public int algorithm_index;// = Constant.SPILLTREE_BRT;

        //指定模型为第model_index种算法
        public int model_index;// = Constant.VMF;

        //指定随机投影方法所用的随机算法为RandomProjection类里的第random_projection_algorithm_index种随机算法
        public int random_projection_algorithm_index = Constant.GAUSSIAN_RANDOM;

        //画rosetree时每个node打印出word list的size
        public int sizeofprintlist = 5;

        //最近邻个数k的数组
        public int[] ks = { 10 };

        //投影维度projectdimension的数组
        public int[] projectdimensions = { 10, 50, 100, 500 };

        //concept数据集sample的数据量大小
        public int[] sample_num_concept = { 1000, 2000, 5000, 10000, 20000, 50000, 100000 };

        //news数据集sample的数据量大小
        public int[] sample_num_news = { 1000, 2000, 5000, 10000, 20000, 50000, 100000, 200000, 500000, 1000000 };

        //alpha（DCM, rosetree参数）的数组
        public double[] alphas = { 3 };

        //gamma（DCM, rosetree参数）的数组
        public double[] gammas = { 0.1 };

        //tau (spilltree参数）的数组
        public double tau = 0.1;

        //kappa (vMF, rosetree参数）的数组
        public double[] kappas = { 14000 };

        //R_0 (vMF, rosetree参数）的数组
        public double[] R_0s = { 0.05 };

        #region previous Experiment
        //BRT, kNN-BRT, Spilltree-BRT的precision实验
        public void RoseTreePrecision()
        {
            Precision precision = new Precision();
            Dictionary<int, int[]> relevant_nearest_neighbor_array = null;

            LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index);
            lfv.Load(outputdebugpath);

            Sample sample = new Sample();
            sample.Run(lfv.featurevectorsnum, precision.samplenum, out precision.sample_array);

            double[] array_1 = null;
            double[] array_2 = null;
            double[] array_3 = null;

            if (model_index == Constant.DCM)
            {
                array_1 = alphas;
                array_2 = gammas;
                array_3 = new double[1] { 0.05 };
            }
            else if (model_index == Constant.VMF)
            {
                array_1 = kappas;
                array_2 = gammas;//R_0s;
                array_3 = R_0s;
            }

            //遍历投影维度
            for (int j = 0; j < projectdimensions.Length; j++)
                //遍历最近邻个数k
                for (int i = 0; i < ks.Length; i++)
                {
                    int k = ks[i];
                    int projectdimension = projectdimensions[j];
                    StreamWriter[] precision_writer = new StreamWriter[precision.neighbors_nums.Length];

                    for (int s = 0; s < precision.neighbors_nums.Length; s++)
                        precision_writer[s] = new StreamWriter(outputdebugpath + "projectdimension=" + projectdimension + "k=" + k + "neighbor" + precision.neighbors_nums[s] + ".txt");
                    Console.WriteLine("k=" + k + ", dimension = " + projectdimension);

                    double[][][][] precision_avg, precision_var;
                    double[][][][] search_neighbors_num_avg, search_neighbors_num_var;
                    double[][][][] search_time_avg, search_time_var;

                    precision_avg = new double[array_1.Length][][][];
                    precision_var = new double[array_1.Length][][][];
                    search_neighbors_num_avg = new double[array_1.Length][][][];
                    search_neighbors_num_var = new double[array_1.Length][][][];
                    search_time_avg = new double[array_1.Length][][][];
                    search_time_var = new double[array_1.Length][][][];

                    for (int s = 0; s < array_1.Length; s++)
                    {
                        precision_avg[s] = new double[array_2.Length][][];
                        precision_var[s] = new double[array_2.Length][][];
                        search_neighbors_num_avg[s] = new double[array_2.Length][][];
                        search_neighbors_num_var[s] = new double[array_2.Length][][];
                        search_time_avg[s] = new double[array_2.Length][][];
                        search_time_var[s] = new double[array_2.Length][][];
                    }

                    for (int s = 0; s < array_1.Length; s++)
                        for (int t = 0; t < array_2.Length; t++)
                        {
                            precision_avg[s][t] = new double[array_3.Length][];
                            precision_var[s][t] = new double[array_3.Length][];
                            search_neighbors_num_avg[s][t] = new double[array_3.Length][];
                            search_neighbors_num_var[s][t] = new double[array_3.Length][];
                            search_time_avg[s][t] = new double[array_3.Length][];
                            search_time_var[s][t] = new double[array_3.Length][];
                        }

                    //以下s,t,c遍历每个model下的3个不同参数，如果是DCM则是alpha和gamma；如果是vMF则是kappa, gamma和R_0
                    for (int s = 0; s < array_1.Length; s++)
                        for (int t = 0; t < array_2.Length; t++)
                            for (int c = 0; c < array_3.Length; c++)
                            {
                                //double kappa = kappas[s];
                                //double R_0 = R_0s[t];
                                double alpha = double.MinValue;
                                double gamma = double.MinValue;
                                double kappa = double.MinValue;
                                double R_0 = double.MinValue;

                                if (model_index == Constant.DCM)
                                {
                                    alpha = alphas[s];
                                    gamma = gammas[t];
                                    R_0 = R_0s[c];
                                }
                                else if (model_index == Constant.VMF)
                                {
                                    kappa = kappas[s];
                                    gamma = gammas[t];//0.5;
                                    R_0 = R_0s[c];
                                }

                                Console.WriteLine("s=" + s + ",t=" + t);

                                DateTime dt1 = DateTime.Now;
                                int depth;

                                RoseTree rosetree = new RoseTree(dataset_index, algorithm_index, Constant.ROSETREE_PRECISION, random_projection_algorithm_index, model_index, projectdimension, k, lfv, alpha, gamma, tau, kappa, R_0, outputdebugpath, (precision == null) ? null : precision.sample_array);
                                if (i == 0)
                                    rosetree.Run(Constant.intervals[0], precision.neighbors_nums.Max(), out relevant_nearest_neighbor_array, out depth);
                                else
                                    rosetree.Run(Constant.intervals[0], out depth);

                                DateTime dt2 = DateTime.Now;

                                DrawRoseTree drawtree = new DrawRoseTree(rosetree, outputdebugpath, sizeofprintlist);
                                drawtree.Run();

                                precision.RoseTreePrecision(rosetree, out precision_avg[s][t][c], out precision_var[s][t][c], out search_neighbors_num_avg[s][t][c], out search_neighbors_num_var[s][t][c], out search_time_avg[s][t][c], out search_time_var[s][t][c], relevant_nearest_neighbor_array);
                            }


                    for (int l = 0; l < precision.neighbors_nums.Length; l++)
                    {
                        precision_writer[l].WriteLine("Precision");

                        for (int s = 0; s < array_1.Length; s++)
                        {
                            for (int t = 0; t < array_2.Length; t++)
                            {
                                for (int c = 0; c < array_3.Length; c++)
                                    precision_writer[l].Write(precision_avg[s][t][c][l] + "+" + precision_var[s][t][c][l] + ",");
                                precision_writer[l].WriteLine();
                            }

                            precision_writer[l].WriteLine();
                        }

                        precision_writer[l].WriteLine("Search Neighbor Number");
                        for (int s = 0; s < array_1.Length; s++)
                        {
                            for (int t = 0; t < array_2.Length; t++)
                            {
                                for (int c = 0; c < array_3.Length; c++)
                                    precision_writer[l].Write(search_neighbors_num_avg[s][t][c][l] + "+" + search_neighbors_num_var[s][t][c][l] + ",");
                                precision_writer[l].WriteLine();
                            }

                            precision_writer[l].WriteLine();
                        }

                        precision_writer[l].WriteLine("Search Time");
                        for (int s = 0; s < array_1.Length; s++)
                        {
                            for (int t = 0; t < array_2.Length; t++)
                            {
                                for (int c = 0; c < array_3.Length; c++)
                                    precision_writer[l].Write(search_time_avg[s][t][c][l] + "+" + search_time_var[s][t][c][l] + ",");
                                precision_writer[l].WriteLine();
                            }

                            precision_writer[l].WriteLine();
                        }
                    }

                    for (int s = 0; s < precision.neighbors_nums.Length; s++)
                    {
                        precision_writer[s].Flush();
                        precision_writer[s].Close();
                    }
                }
        }

        //Spilltree的precision实验
        public void SpillTreePrecision()
        {
            Precision precision = new Precision();

            LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index);
            Dictionary<int, int[]> relevant_nearest_neighbor_array = new Dictionary<int, int[]>();
            lfv.Load(outputdebugpath);
            kNearestNeighbor kNN = new kNearestNeighbor();

            Sample sample = new Sample();
            sample.Run(lfv.featurevectorsnum, precision.samplenum, out precision.sample_array);

            for (int i = 0; i < precision.neighbors_nums.Length; i++)
            {
                relevant_nearest_neighbor_array.Add(precision.sample_array[i], kNN.SearchSparseVectorList(lfv.featurevectors[precision.sample_array[i]], lfv.featurevectors, precision.neighbors_nums.Max()));
                Console.WriteLine("Looking for the " + i + "th relevant nearest neighbor array");
            }

            //遍历投影维度
            for (int l = 0; l < projectdimensions.Length; l++)
            {
                int projectdimension = projectdimensions[l];
                RandomProjection projection = new RandomProjection(lfv.lexiconsize, projectdimension, this.random_projection_algorithm_index, lfv.samplepath);
                projection.GenerateRandomMatrix();
                projection.ReadRandomMatrix();
                Console.WriteLine("Project dimension = " + projectdimension);

                //遍历最近邻个数
                for (int j = 0; j < ks.Length; j++)
                {                  
                    int k = ks[j];
                    List<RoseTreeNode> nodearray = new List<RoseTreeNode>();
                    Console.WriteLine("k=" + k);

                    
                    for (int i = 0; i < lfv.featurevectorsnum; i++)
                    {
                        if (i % 10000 == 0)
                            Console.WriteLine("Initializing the " + i + "th node");
                        RoseTreeNode newnode = new RoseTreeNode(null, lfv.featurevectors[i], projection.GenerateProjectData(lfv.featurevectors[i]), i);
                        newnode.indices.initial_index = i;
                        nodearray.Add(newnode);
                    }

                    SpillTree spilltree = new SpillTree(k, projectdimension, tau);
                    Console.WriteLine("Start to build spill tree");
                    spilltree.Build(nodearray);
                    Console.WriteLine("Done with building spill tree");

                    //DrawSpillTree drawtree = new DrawSpillTree(spilltree, outputpath);
                    //drawtree.Run();

                    StreamWriter precision_writer = new StreamWriter(outputdebugpath + "k=" + k + "_d=" + projectdimension + ".txt");

                    double[] precision_avg, precision_var;
                    double[] search_neighbors_num_avg, search_neighbors_num_var;
                    double[] search_time_avg, search_time_var;

                    precision.SpillTreePrecision(spilltree, nodearray.ToArray(), out precision_avg, out precision_var, out search_neighbors_num_avg, out search_neighbors_num_var, out search_time_avg, out search_time_var, relevant_nearest_neighbor_array);

                    for (int i = 0; i < precision_avg.Length; i++)
                    {
                        precision_writer.WriteLine("Precision: (Avg: " + precision_avg[i] + "); (Var: " + precision_var[i] + ");");
                        precision_writer.WriteLine("Search Neighbor Number: (Avg: " + search_neighbors_num_avg[i] + "); (Var: " + search_neighbors_num_var[i] + ");");
                        precision_writer.WriteLine("Search Time: (Avg: " + search_time_avg[i] + "); (Var: " + search_time_var[i] + ");");
                        precision_writer.WriteLine();
                    }

                    precision_writer.Flush();
                    precision_writer.Close();
                }
            }
        }

        //随机投影的precision实验
        public void RandomProjectionPrecision()
        {
            Precision precision = new Precision();

            LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index);
            Dictionary<int, int[]> relevant_nearest_neighbor_array = new Dictionary<int, int[]>();
            lfv.Load(outputdebugpath);
            kNearestNeighbor kNN = new kNearestNeighbor();

            Sample sample = new Sample();
            sample.Run(lfv.featurevectorsnum, precision.samplenum, out precision.sample_array);

            for (int i = 0; i < precision.neighbors_nums.Length; i++)
                relevant_nearest_neighbor_array.Add(precision.sample_array[i], kNN.SearchSparseVectorList(lfv.featurevectors[precision.sample_array[i]], lfv.featurevectors, precision.neighbors_nums.Max()));

            for (int l = 2; l < projectdimensions.Length; l++)
            {
                int projectdimension = projectdimensions[l];
                RandomProjection projection = new RandomProjection(lfv.lexiconsize, projectdimension, this.random_projection_algorithm_index, lfv.samplepath);
                //projection.GenerateRandomMatrix();
                projection.ReadRandomMatrix();
                List<RoseTreeNode> nodearray = new List<RoseTreeNode>();
                List<double> precisionlist = new List<double>();
                double precision_avg, precision_var;

                for (int i = 0; i < lfv.featurevectorsnum; i++)
                {
                    RoseTreeNode newnode = new RoseTreeNode(null, lfv.featurevectors[i], (this.algorithm_index == Constant.SPILLTREE_BRT) ? projection.GenerateProjectData(lfv.featurevectors[i]) : null, i);
                    newnode.indices.initial_index = i;
                    nodearray.Add(newnode);
                }

                for (int i = 0; i < precision.neighbors_nums.Length; i++)
                {
                    int[] label_indices = relevant_nearest_neighbor_array[precision.sample_array[i]];
                    int[] retrieve_indices = kNN.SearchProject(precision.sample_array[i], nodearray.ToArray(), precision.neighbors_nums.Max(), projectdimension);

                    precisionlist.Add(precision.IR(label_indices, retrieve_indices));
                }

                RoseTreeMath.AverageVariance(precisionlist, out precision_avg, out precision_var);
            }
        }

        //BRT, kNN-BRT, Spilltree-BRT的时间开销实验
        public void TimeExperiment()
        {
            Sample sample = new Sample();
            int[] sample_num = null;

            if (dataset_index == Constant.CONCEPTUALIZE)
                sample_num = sample_num_concept;
            else if (dataset_index == Constant.BING_NEWS)
                sample_num = sample_num_news;

            double alpha = alphas[0];
            double gamma = gammas[0];
            double kappa = kappas[0];
            double R_0 = R_0s[0];

            for (int i = 0; i < sample_num.Length; i++)
            {
                int[] sample_array;
                int depth;
                TimeSpan timespan_preprocess, timespan_merge, timespan_total;
//                Dictionary<int, int[]> relevant_nearest_neighbor_array;
                sample.Run(Constant.datasize[dataset_index], sample_num[i], out sample_array);

                //StreamWriter sampleitems_writer = new StreamWriter(outputdebugpath + "sampleitems.txt");
                //StreamWriter samplenum_writer = new StreamWriter(outputdebugpath + "samplenum.txt");
                //for (int j = 0; j < sample_num[i]; j++)
                //    sampleitems_writer.WriteLine(sample_array[j]);
                //samplenum_writer.WriteLine(sample_num[i]);
                //sampleitems_writer.Flush();
                //sampleitems_writer.Close();
                //samplenum_writer.Flush();
                //samplenum_writer.Close();

                LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index);
                lfv.Load(outputdebugpath);

                RoseTree rosetree = new RoseTree(dataset_index, algorithm_index, Constant.TIME_EXPERIMENT, random_projection_algorithm_index, model_index, 10, 10, lfv, alpha, gamma, tau, kappa, R_0, outputdebugpath, null);
                rosetree.Run(Constant.intervals[i],  out depth, out timespan_preprocess, out timespan_merge, out timespan_total);

                DrawRoseTree drawtree = new DrawRoseTree(rosetree, outputdebugpath, sizeofprintlist);
                drawtree.Run();

                StreamWriter time_writer = new StreamWriter(outputdebugpath + "datasize = " + sample_num[i] + "time.txt");
                time_writer.WriteLine("Preprocess" + timespan_preprocess);
                time_writer.WriteLine("Merge" + timespan_merge);
                time_writer.WriteLine("Total" + timespan_total);
                time_writer.Flush();
                time_writer.Close();

                Console.ReadKey();
            }
        }

        //魏昊的InfoVis Project有关RoseTree部分的实验
        public void HaosExperiment()
        {
            int depth;
//            TimeSpan timespan_preprocess, timespan_merge, timespan_total;
 //           Dictionary<int, int[]> relevant_nearest_neighbor_array;

            double alpha = alphas[0];
            double gamma = gammas[0];
            double kappa = kappas[0];
            double R_0 = R_0s[0];

            LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index);
            lfv.Load(outputdebugpath);

            RoseTree rosetree = new RoseTree(dataset_index, algorithm_index, Constant.TIME_EXPERIMENT, random_projection_algorithm_index, model_index, 10, 10, lfv, alpha, gamma, tau, kappa, R_0, outputdebugpath, null);
            rosetree.Run(10, out depth);

            DrawRoseTree drawtree = new DrawRoseTree(rosetree, outputdebugpath, 5);
            drawtree.OutputHaosTree();

            drawtree.Run();
        }

        //BRT, kNN-BRT, Spilltree-BRT的NMI实验的前半部分,即输出由rosetree cut后对叶节点进行标记的分类信息
        public void NMI()
        {
            int sample_num = 1;

            double[] array_1 = null;
            double[] array_2 = null;

            int projectdimension = projectdimensions[0];
            int k = ks[0];

            if (model_index == Constant.DCM)
            {
                array_1 = alphas;
                array_2 = gammas;
            }
            else if (model_index == Constant.VMF)
            {
                array_1 = kappas;
                array_2 = gammas;//R_0s;
            }

            for (int r = 0; r < sample_num; r++)
            {
                //Sample sample1 = new Sample();
                //int[] samplearray;
                //sample1.Run(/*26562*/19997, 1000, out samplearray);
                //StreamWriter sw1 = new StreamWriter(this.outputdebugpath + "sampleitems.txt");
                //StreamWriter sw2 = new StreamWriter(this.outputdebugpath + "samplenum.txt");
                //for (int i = 0; i < 1000; i++)
                //    sw1.WriteLine(samplearray[i]);
                //sw2.WriteLine(1000);
                //sw1.Flush();
                //sw1.Close();
                //sw2.Flush();
                //sw2.Close();

                StreamWriter sw_label = new StreamWriter(outputdebugpath + "label_"+r+".txt");
                StreamWriter sw_tree_label = new StreamWriter(outputdebugpath + "tree_label_"+r+".txt");

                LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index);
                lfv.Load(outputdebugpath);

                StreamWriter sw_number = new StreamWriter(outputdebugpath + "label_num_" + r + ".txt");
               

                for (int s = 0; s < array_1.Length; s++)
                    for (int t = 0; t < array_2.Length; t++)
                    {
                        //double kappa = kappas[s];
                        //double R_0 = R_0s[t];
                        double alpha = double.MinValue;
                        double gamma = double.MinValue;
                        double kappa = double.MinValue;
                        double R_0 = double.MinValue;

                        if (model_index == Constant.DCM)
                        {
                            alpha = alphas[s];
                            gamma = gammas[t];
                        }
                        else if (model_index == Constant.VMF)
                        {
                            kappa = kappas[s];
                            gamma = gammas[t];//0.5;
                            R_0 = R_0s[0];
                        }

                        int depth;

                        Console.WriteLine("s=" + s + ",t=" + t);
                        RoseTree rosetree = new RoseTree(dataset_index, algorithm_index, Constant.ROSETREE_PRECISION, random_projection_algorithm_index, model_index, projectdimension, k, lfv, alpha, gamma, tau, kappa, R_0, outputdebugpath);
                        rosetree.Run(Constant.intervals[0], out depth);

                        DrawRoseTree drawtree = new DrawRoseTree(rosetree, outputdebugpath, sizeofprintlist);
                        drawtree.Run();

                        RoseTreeCut cut = new RoseTreeCut(rosetree);

                        cut.CutByProbability();
                        cut.WriteLabels(sw_label, sw_tree_label);
                        sw_number.WriteLine(lfv.featurevectors.Length);
                    }

                sw_label.Flush();
                sw_label.Close();

                sw_tree_label.Flush();
                sw_tree_label.Close();

                sw_number.Flush();
                sw_number.Close();
            }
        }

        //BRT, kNN-BRT, Spilltree-BRT的likelihood实验的前半部分，即输出每种模型，每个算法，每个数据集，每次sample下的likelihood值到不同的txt文件里
        public void Likelihood()
        {
            int projectdimension = projectdimensions[1];
            int k = ks[0];
            double alpha = alphas[0];
            double gamma = gammas[0];
            double kappa = kappas[0];
            double R_0 = R_0s[0];
       
            string likelihood_path = outputdebugpath+"likelihood\\";
            Directory.CreateDirectory(likelihood_path);

            Sample sample1 = new Sample();
            int[][] samplearray_concept = new int[10][];
            int[][] samplearray_news = new int[10][];

            int total_num_concept = 26562;
            int total_num_news = 1700130;

            int sample_num = 1000;
            int sample_times = 10;

            // get sample data: samplearray_concept,samplearray_news
            if (BLoadSampleDataFromFile)
            {
                StreamReader[] sr_concept = new StreamReader[sample_times];
                StreamReader[] sr_news = new StreamReader[sample_times];

                string concept_path = outputdebugpath + @"concept\";
                string news_path = outputdebugpath + @"news\";

                //if no sample file exists, generate and write one
                if (!Directory.Exists(concept_path) || !Directory.Exists(news_path))
                {
                    for (int i = 0; i < sample_times; i++)
                    {
                        sample1.Run(total_num_concept, sample_num, out samplearray_concept[i]);
                        sample1.Run(total_num_news, sample_num, out samplearray_news[i]);
                    }

                    StreamWriter[] sw_concept = new StreamWriter[sample_times];
                    StreamWriter[] sw_news = new StreamWriter[sample_times];

                    Directory.CreateDirectory(outputdebugpath + "concept\\");
                    Directory.CreateDirectory(outputdebugpath + "news\\");

                    for (int i = 0; i < sample_times; i++)
                    {
                        sw_concept[i] = new StreamWriter(concept_path + "sampleitems" + i + ".txt");
                        sw_news[i] = new StreamWriter(news_path + "sampleitems" + i + ".txt");
                    }

                    for (int j = 0; j < sample_times; j++)
                        for (int i = 0; i < sample_num; i++)
                        {
                            sw_concept[j].WriteLine(samplearray_concept[j][i]);
                            sw_news[j].WriteLine(samplearray_news[j][i]);
                        }

                    for (int j = 0; j < sample_times; j++)
                    {
                        sw_concept[j].Flush();
                        sw_concept[j].Close();

                        sw_news[j].Flush();
                        sw_news[j].Close();
                    }
                }

                //read from sample files
                for (int i = 0; i < sample_times; i++)
                {
                    sr_concept[i] = new StreamReader(concept_path + "sampleitems" + i + ".txt");
                    sr_news[i] = new StreamReader(news_path + "sampleitems" + i + ".txt");

                    samplearray_concept[i] = new int[sample_num];
                    samplearray_news[i] = new int[sample_num];

                    for (int j = 0; j < sample_num; j++)
                    {
                        samplearray_concept[i][j] = int.Parse(sr_concept[i].ReadLine());
                        samplearray_news[i][j] = int.Parse(sr_news[i].ReadLine());
                    }

                    sr_concept[i].Close();
                    sr_news[i].Close();
                }
            }
            else
            {
                for (int i = 0; i < sample_times; i++)
                {
                    sample1.Run(total_num_concept, sample_num, out samplearray_concept[i]);
                    sample1.Run(total_num_news, sample_num, out samplearray_news[i]);
                }
            }

            //double[][][][] likelihood = new double[2][][][];

            //for (int i = 0; i < 2; i++)
            //    likelihood[i] = new double[2][][];

            //for (int i = 0; i < 2; i++)
            //    for (int j = 0; j < 2; j++)
            //        likelihood[i][j] = new double[3][];

            //for (int i = 0; i < 2; i++)
            //    for (int j = 0; j < 2; j++)
            //        for (int p = 0; p < 3; p++)
            //            likelihood[i][j][p] = new double[10];

            //for (dataset_index = Constant.CONCEPTUALIZE; dataset_index <= Constant.BING_NEWS; dataset_index++)
            //for (dataset_index = Constant.TWENTY_NEWS_GROUP; dataset_index <= Constant.TWENTY_NEWS_GROUP; dataset_index++)
            for (dataset_index = Constant.BING_NEWS; dataset_index <= Constant.BING_NEWS; dataset_index++)
            {
                for (int i = 0; i < sample_times; i++)
                {
//                    StreamWriter sw_ind = new StreamWriter(outputdebugpath + "indicator" + dataset_index + "i" + i + ".txt");

                    StreamWriter sw_writer = new StreamWriter(outputdebugpath + "sampleitems.txt");
                    StreamWriter sw_num = new StreamWriter(outputdebugpath + "samplenum.txt");

                    for (int j = 0; j < sample_num; j++)
                    {
                        if (dataset_index == Constant.CONCEPTUALIZE)
                            sw_writer.WriteLine(samplearray_concept[i][j]);
                        else
                            sw_writer.WriteLine(samplearray_news[i][j]);
                    }

                    sw_num.WriteLine(sample_num);

                    sw_writer.Flush();
                    sw_writer.Close();

                    sw_num.Flush();
                    sw_num.Close();

                    //for (model_index = Constant.DCM; model_index <= Constant.VMF; model_index++)
                    for (model_index = Constant.DCM; model_index <= Constant.DCM; model_index++)
                    {
                        LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index);
                        lfv.Load(outputdebugpath);

                        //for (algorithm_index = Constant.BRT; algorithm_index <= Constant.SPILLTREE_BRT; algorithm_index++)
                        for (algorithm_index = Constant.KNN_BRT; algorithm_index <= Constant.KNN_BRT; algorithm_index++)
                        {
                            int depth;
                            double likelihood2;
                            RoseTree rosetree = new RoseTree(
                                dataset_index,                      //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                                algorithm_index,                    //BRT,KNN_BRT,SPILLTREE_BRT
                                Constant.ROSETREE_PRECISION,        //0
                                random_projection_algorithm_index,  //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                                model_index,                        //DCM,VMF,BERNOULLI
                                projectdimension,                   //projectdimensions[1]:50
                                k,                                  //k nearest neighbour
                                lfv,                                //load feature vector
                                alpha, gamma, tau, kappa, R_0,      //parameters, see top of this file
                                outputdebugpath);                   
                            rosetree.Run(Constant.intervals[0], out depth, out likelihood2);

                            //likelihood[dataset_index][model_index][algorithm_index][i] = likelihood2;
                            StreamWriter sw = new StreamWriter(likelihood_path + "_" + dataset_index + "_" + i + "_" + model_index + "_" + algorithm_index);
                            sw.WriteLine(likelihood2);

                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
            }

            Console.WriteLine("all likelyhood calculation done");

            //StreamWriter avg_var_writer = new StreamWriter(this.outputdebugpath + "avg_var_writer.txt");
            //  for (dataset_index = Constant.CONCEPTUALIZE; dataset_index <= Constant.BING_NEWS; dataset_index++)
            //       for (model_index = Constant.DCM; model_index <= Constant.VMF; model_index++)
            //           for (algorithm_index = Constant.BRT; algorithm_index <= Constant.SPILLTREE_BRT; algorithm_index++)
            //           {
            //               double avg, var;
            //               RoseTreeMath.AverageVariance(likelihood[dataset_index][model_index][algorithm_index].ToList(), out avg, out var);

            //               avg_var_writer.WriteLine("dataset" + dataset_index + "model" + model_index + "algorithm" + algorithm_index + ";avg:" + avg + ";var:" + var);
            //           }

            //  avg_var_writer.Flush();
            //  avg_var_writer.Close();
        }

        //BRT, kNN-BRT, Spilltree-BRT的likelihood实验的后半部分，即根据前面输出的不同的txt文件里的likelihood值来计算不同sample下的average和variance
        public void LikelihoodStat()
        {
            double[][][][] likelihood = new double[4][][][];

            for (int i = 0; i < 4; i++)
                likelihood[i] = new double[2][][];

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 2; j++)
                    likelihood[i][j] = new double[2][];

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 2; j++)
                    for (int k = 0; k < 2; k++)
                        likelihood[i][j][k] = new double[10];

            string[] filenames = Directory.GetFiles(outputdebugpath+@"likelihood");

            for (int i = 0; i < filenames.Length; i++)
            {
                string[] tokens = filenames[i].Substring(filenames[i].Length - 8).Split('_');
                likelihood[int.Parse(tokens[4])][int.Parse(tokens[1])][int.Parse(tokens[3])][int.Parse(tokens[2])] = double.Parse(new StreamReader(filenames[i]).ReadLine());
            }

            StreamWriter sw = new StreamWriter(outputdebugpath+@"result.txt");


            for (int j = 0; j < 2; j++)
                for (int k = 0; k < 2; k++)
                    for (int i = 0; i < 4; i++)
                    {
                        double avg, var;
                        RoseTreeMath.AverageVariance(likelihood[i][j][k].ToList(), out avg, out var);

                        sw.WriteLine("Algorithm" + i + "dataset" + j + "model" + k + ":" + avg + "+" + var);
                    }

            sw.Flush();
            sw.Close();
        }
        #endregion previous Experiment

        ///////////////////////////////////Constrained Rose Tree///////////////////////////////////
        //test single run of likelihood //Xiting
        public RoseTree BingNewsRoseTree(string news_path, string likelihood_path, string sample_path,
            string outputdebugpath, int time, int dataset_index, int model_index, int algorithm_index, int sample_times)
        {
            //if (dataset_index != Constant.BING_NEWS)
            //    throw new Exception("Wrong Data!");

            #region sampling
            //string likelihood_path = outputdebugpath + "likelihood\\";

            Sample sample1 = new Sample();
            int[][] samplearray_news = new int[sample_times][];

            Dictionary<int, int> news_num = new Dictionary<int, int>();
            news_num.Add(0, 8);
            news_num.Add(1, 8);
            news_num.Add(1841, 4);
            news_num.Add(1843, 1);
            news_num.Add(1845, 1);
            news_num.Add(1846, 3);
            news_num.Add(1849, 7);
            news_num.Add(1850, 2);
            news_num.Add(1851, 1);
            news_num.Add(1852, 3);
            news_num.Add(1853, 34539);
            news_num.Add(1854, 131608);
            news_num.Add(1855, 121719);
            news_num.Add(1856, 90224);
            news_num.Add(1857, 77158);
            news_num.Add(1858, 107494);
            news_num.Add(1859, 125076);
            news_num.Add(1860, 137207);
            news_num.Add(1861, 139190);
            news_num.Add(1862, 128137);
            news_num.Add(1863, 95263);
            news_num.Add(1864, 83997);
            news_num.Add(1865, 133232);
            news_num.Add(1866, 117235);
            news_num.Add(1867, 111668);
            news_num.Add(1868, 66361);

            int total_num_news = news_num[time];

            int sample_num = 1000;
            if (sample_num > total_num_news)
                sample_num = total_num_news;
            //int sample_times = 10;

            // get sample data: samplearray_concept,samplearray_news
            if (BLoadSampleDataFromFile)
            {
                StreamReader[] sr_news = new StreamReader[sample_times];

                //if no sample file exists, generate and write one
                for (int i = 0; i < sample_times; i++)
                {
                    string sample_filename = sample_path + time + "_sampleitems_" + i + ".txt";
                    if (!File.Exists(sample_filename))
                    {
                        RandomGenerator.SetSeedFromSystemTime();
                        sample1.Run(total_num_news, sample_num, out samplearray_news[i]);

                        StreamWriter sw_news = new StreamWriter(sample_filename);

                        Directory.CreateDirectory(sample_path);

                        for (int j = 0; j < sample_num; j++)
                            sw_news.WriteLine(samplearray_news[i][j]);

                        sw_news.Flush();
                        sw_news.Close();
                    }
                }


                //read from sample files
                for (int i = 0; i < sample_times; i++)
                {
                    string sample_filename = sample_path + time + "_sampleitems_" + i + ".txt";
                    sr_news[i] = new StreamReader(sample_filename);

                    samplearray_news[i] = new int[sample_num];

                    for (int j = 0; j < sample_num; j++)
                    {
                        samplearray_news[i][j] = int.Parse(sr_news[i].ReadLine());
                    }

                    sr_news[i].Close();
                }
            }
            else
            {
                for (int i = 0; i < sample_times; i++)
                {
                    RandomGenerator.SetSeedFromSystemTime();
                    sample1.Run(total_num_news, sample_num, out samplearray_news[i]);
                }
            }

            //for (int i = 0; i < sample_times; i++)
            //{
            //    StreamWriter sw_writer = new StreamWriter(outputdebugpath + "sampleitems.txt");
            //    StreamWriter sw_num = new StreamWriter(outputdebugpath + "samplenum.txt");

            //    if (dataset_index == Constant.CONCEPTUALIZE)
            //        Console.WriteLine("Only deal with bing news data!");
            //    else
            //        for (int j = 0; j < sample_num; j++)
            //            sw_writer.WriteLine(samplearray_news[i][j]);

            //    sw_num.WriteLine(sample_num);

            //    sw_writer.Flush();
            //    sw_writer.Close();

            //    sw_num.Flush();
            //    sw_num.Close();
            //}
            #endregion sampling

            this.dataset_index = dataset_index;
            this.model_index = model_index;
            this.algorithm_index = algorithm_index;

            int projectdimension = projectdimensions[1];
            int k = ks[0];
            double alpha = alphas[0];
            double gamma = gammas[0];
            double kappa = kappas[0];
            double R_0 = R_0s[0];

            if (k >= total_num_news)
                k = total_num_news - 1;
            RoseTree rosetree = null;
            Directory.CreateDirectory(likelihood_path);
            for (int i = 0; i < sample_times; i++)
            {
                //for (model_index = Constant.DCM; model_index <= Constant.VMF; model_index++)
                {
                    string news_filename = news_path + time + ".txt";
                    string sample_filename = sample_path + time + "_sampleitems_" + i + ".txt";
                    string featurevector_path = sample_path;
                    LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index,
                        news_filename, sample_filename, featurevector_path, sample_num);
                    lfv.Load(null);

                    //for (algorithm_index = Constant.BRT; algorithm_index <= Constant.SPILLTREE_BRT; algorithm_index++)
                    {
                        int depth;
                        double likelihood2;
                        if (!Directory.Exists(outputdebugpath))
                            Directory.CreateDirectory(outputdebugpath);
                        rosetree = new RoseTree(
                            dataset_index,                      //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                            algorithm_index,                    //BRT,KNN_BRT,SPILLTREE_BRT
                            Constant.ROSETREE_PRECISION,        //0
                            random_projection_algorithm_index,  //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                            model_index,                        //DCM,VMF,BERNOULLI
                            projectdimension,                   //projectdimensions[1]:50
                            k,                                  //k nearest neighbour
                            lfv,                                //load feature vector
                            alpha, gamma, tau, kappa, R_0,      //parameters, see top of this file
                            outputdebugpath);
                        rosetree.Run(Constant.intervals[0], out depth, out likelihood2);

                        //likelihood[dataset_index][model_index][algorithm_index][i] = likelihood2;
                        StreamWriter sw = new StreamWriter(likelihood_path + "_" + dataset_index + "_" + i + "_" + model_index + "_" + algorithm_index + ".dat");
                        sw.WriteLine(likelihood2);

                        sw.Flush();
                        sw.Close();
                    }
                }
            }

            Console.WriteLine("likelyhood calculation done: data_index " + dataset_index + ", model_index " + model_index + ", algorithm_index " + algorithm_index);

            return rosetree;
        }

        //test build a tree using 20 news group data
        public RoseTree TwentyNewsGroupRoseTree(string twentyNG_path, string likelihood_path, string sample_path,
     string outputdebugpath, int dataset_index, int model_index, int algorithm_index, int sample_num)
        {
            if (dataset_index != Constant.TWENTY_NEWS_GROUP)
                throw new Exception("Wrong Data!");

            #region sampling
            //string likelihood_path = outputdebugpath + "likelihood\\";

            Sample sample1 = new Sample();
            int[] samplearray_news = null;

            int total_num_news = 17000;// 19997;

            if (sample_num > total_num_news)
                sample_num = total_num_news;
            //int sample_times = 10;

            // get sample data: samplearray_concept,samplearray_news
            if (BLoadSampleDataFromFile)
            {
                StreamReader sr_news = null;

                //if no sample file exists, generate and write one
                string sample_filename = sample_path + "_sampleitems_" + sample_num + "_" + total_num_news + ".txt";
                if (!File.Exists(sample_filename))
                {
                    RandomGenerator.SetSeedFromSystemTime();
                    sample1.Run(total_num_news, sample_num, out samplearray_news);

                    StreamWriter sw_news = new StreamWriter(sample_filename);

                    Directory.CreateDirectory(sample_path);

                    for (int j = 0; j < sample_num; j++)
                        sw_news.WriteLine(samplearray_news[j]);

                    sw_news.Flush();
                    sw_news.Close();
                }


                //read from sample files
                sr_news = new StreamReader(sample_filename);

                samplearray_news = new int[sample_num];

                for (int j = 0; j < sample_num; j++)
                {
                    samplearray_news[j] = int.Parse(sr_news.ReadLine());
                }

                sr_news.Close();
            }
            else
            {
                RandomGenerator.SetSeedFromSystemTime();
                sample1.Run(total_num_news, sample_num, out samplearray_news);
            }
            #endregion sampling

            this.dataset_index = dataset_index;
            this.model_index = model_index;
            this.algorithm_index = algorithm_index;

            int projectdimension = projectdimensions[1];
            int k = ks[0];
            double alpha = alphas[0];
            double gamma = gammas[0];
            double kappa = kappas[0];
            double R_0 = R_0s[0];

            if (k >= total_num_news)
                k = total_num_news - 1;
            RoseTree rosetree = null;
            Directory.CreateDirectory(likelihood_path);
            {
                //for (model_index = Constant.DCM; model_index <= Constant.VMF; model_index++)
                {
                    string sample_filename = sample_path + "_sampleitems_" + sample_num + "_" + total_num_news + ".txt";
                    string featurevector_path = sample_path;
                    LoadFeatureVectors lfv = new LoadFeatureVectors(dataset_index, model_index,
                        null, sample_filename, featurevector_path, sample_num);
                    lfv.Load(twentyNG_path);

                    //for (algorithm_index = Constant.BRT; algorithm_index <= Constant.SPILLTREE_BRT; algorithm_index++)
                    {
                        int depth;
                        double likelihood2;
                        if (!Directory.Exists(outputdebugpath))
                            Directory.CreateDirectory(outputdebugpath);
                        rosetree = new RoseTree(
                            dataset_index,                      //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                            algorithm_index,                    //BRT,KNN_BRT,SPILLTREE_BRT
                            Constant.ROSETREE_PRECISION,        //0
                            random_projection_algorithm_index,  //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                            model_index,                        //DCM,VMF,BERNOULLI
                            projectdimension,                   //projectdimensions[1]:50
                            k,                                  //k nearest neighbour
                            lfv,                                //load feature vector
                            alpha, gamma, tau, kappa, R_0,      //parameters, see top of this file
                            outputdebugpath);
                        rosetree.Run(Constant.intervals[0], out depth, out likelihood2);

                        //likelihood[dataset_index][model_index][algorithm_index][i] = likelihood2;
                        StreamWriter sw = new StreamWriter(likelihood_path + "_" + dataset_index + "_" + 0 + "_" + model_index + "_" + algorithm_index + ".dat");
                        sw.WriteLine(likelihood2);

                        sw.Flush();
                        sw.Close();
                    }
                }
            }

            Console.WriteLine("likelyhood calculation done: data_index " + dataset_index + ", model_index " + model_index + ", algorithm_index " + algorithm_index);

            return rosetree;
        }
    }
}
