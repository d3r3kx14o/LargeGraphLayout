using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace EvolutionaryRoseTree.Data
{
    class DataAnalysis
    {
        public static void Entry()
        {
            //PrintBestParameters();
            //CountLeafTopicNumber();
            AnalysisFromEvoDatFile();
        }

        private static void AnalysisFromEvoDatFile()
        {
            var filename = File.ReadAllText("configAnalysisEvoDatFile.txt");
            StreamReader sr = new StreamReader(filename);

            var loglikelihoodToken = "loglikelihood: ";
            var smoothnessToken = "Smoothness: ";
            var runtimeToken = "RunTime: ";

            var loglikelihoodStat = new EvolutionaryRoseTree.Tests.Test.Statistics();
            var smoothnessStat0 = new EvolutionaryRoseTree.Tests.Test.Statistics();
            var smoothnessStat1 = new EvolutionaryRoseTree.Tests.Test.Statistics();
            var smoothnessStat2 = new EvolutionaryRoseTree.Tests.Test.Statistics();
            var runtimeStat = new EvolutionaryRoseTree.Tests.Test.Statistics();

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith(loglikelihoodToken))
                {
                    var str = line.Substring(loglikelihoodToken.Length);
                    var loglikelihood = double.Parse(str);
                    loglikelihoodStat.ObserveNumber(loglikelihood);
                }
                else if (line.StartsWith(smoothnessToken))
                {
                    var str = line.Substring(smoothnessToken.Length);
                    var tokens = str.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    var smoothness = new double[tokens.Length];
                    bool bInvalid = true;
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        smoothness[i] = double.Parse(tokens[i]);
                        if (smoothness[i] != 0)
                            bInvalid = false;
                    }

                    if (!bInvalid)
                    {
                        smoothnessStat0.ObserveNumber(smoothness[0]);
                        smoothnessStat1.ObserveNumber(smoothness[1]);
                        smoothnessStat2.ObserveNumber(smoothness[2]);
                    }
                }
                else if (line.StartsWith(runtimeToken))
                {
                    var str = line.Substring(runtimeToken.Length);
                    str = str.Substring(0, str.Length - 1);
                    var runtime = double.Parse(str);
                    runtimeStat.ObserveNumber(runtime);
                }
            }

            loglikelihoodStat.PrintResult("logLikelihood");
            smoothnessStat0.PrintResult("Sm_Dist");
            smoothnessStat1.PrintResult("Sm_Order");
            smoothnessStat2.PrintResult("Sm_RF");
            runtimeStat.PrintResult("runtime");


            Console.Write(loglikelihoodStat.GetAverage().ToString("#.##") + "\t");
            Console.Write(smoothnessStat0.GetAverage().ToString("#.##") + "\t");
            Console.Write(smoothnessStat1.GetAverage().ToString("#.##") + "\t");
            Console.Write(smoothnessStat2.GetAverage().ToString("#.######") + "\t");
            Console.Write((runtimeStat.GetSum()).ToString("#.##") + "\t");

            sr.Close();

            Console.ReadLine();
        }

        private static void CountLeafTopicNumber()
        {
            string inputpath = @"C:\Users\v-xitwan\Desktop\0103_194432_gamma0.4alpha0.03KNN100merge1E-06split1E-06cos0.25newalpha1E-20_LooseOrder0.2_OCM_CSW0.5_520\";

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

        private static void PrintBestParameters()
        {
            string resultfolder = File.ReadAllText("configBestParas.txt");

            StreamWriter sw = new StreamWriter(resultfolder + "\\" + "BestParas.dat");
            string[] directories = Directory.GetDirectories(resultfolder);

            HashSet<double> alphas = new HashSet<double>();
            List<Dictionary<double, Tuple<double, double>>> BestParasOverTime = new List<Dictionary<double, Tuple<double, double>>>();
            foreach (var directory in directories)
            {
                string[] tokens = directory.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string foldername = tokens[tokens.Length - 1];

                sw.WriteLine(foldername);

                Dictionary<double, Tuple<double, double>> bestParas = new Dictionary<double, Tuple<double, double>>();
                foreach (var file in Directory.GetFiles(directory))
                {
                    double alpha, gamma, likelihood;
                    GetAlphaGamma(file, out alpha, out gamma);
                    likelihood = GetLikelihood(file);

                    if (!bestParas.ContainsKey(alpha))
                    {
                        bestParas.Add(alpha, new Tuple<double, double>(-1, double.MinValue));
                        alphas.Add(alpha);
                    }
                    if (likelihood > bestParas[alpha].Item2)
                        bestParas[alpha] = new Tuple<double, double>(gamma, likelihood);
                }

                BestParasOverTime.Add(bestParas);
                foreach (var kvp in bestParas)
                {
                    sw.WriteLine("alpha {0}, gamma {1}, likelihood {2}",
                        kvp.Key, kvp.Value.Item1, kvp.Value.Item2);
                }

                sw.WriteLine();
            }

            sw.WriteLine("----------------------------------------------------------------");
            foreach (var alpha in alphas)
            {
                sw.WriteLine("---Alpha {0}---", alpha);
                foreach (var bestParas in BestParasOverTime)
                {
                    if (bestParas.ContainsKey(alpha))
                        sw.Write(bestParas[alpha].Item1 + "\t");
                    else
                        sw.Write("NaN\t");
                }
                sw.WriteLine();
            }

            sw.Flush();
            sw.Close();
        }

        private static double GetLikelihood(string file)
        {
            string text = File.ReadAllText(file);
            int index0 = text.IndexOf("loglikelihood: ");
            index0 = text.IndexOf("-", index0);
            int index1 = text.IndexOf("\n", index0);
            return double.Parse(text.Substring(index0, index1 - index0));
        }

        private static void GetAlphaGamma(string file, out double alpha, out double gamma)
        {
            int index0 = file.IndexOf("gamma");
            int index1 = file.IndexOf("alpha");
            int index2 = file.IndexOf("KNN");

            alpha = double.Parse(file.Substring(index1 + 5, index2 - index1 - 5));
            gamma = double.Parse(file.Substring(index0 + 5, index1 - index0 - 5));
        }
    }
}
