using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Constants;
using System.IO;
namespace EvolutionaryRoseTree.Experiments
{
    class ConfigEvolutionary
    {
        public static string BasePath = null;
        public static string DataPath { get; set; }
        public static string CodePath { get; set; }
        public static string SamplePath { get; set; }

        public static List<double> Gammas { get; set; }
        public static List<double> Alphas { get; set; }

        public static string IndexPath { get; set; }
        public static string RawQueryStr { get; set; }

        public static string StartDate { get; set; }
        public static int Timespan { get; set; }
        public static int DeltaTime = -1;
        public static int TimesplotsNum { get; set; }
        public static List<int> SampleNums { get; set; }

        public static List<int> KNNParameters { get; set; }
        public static List<double> MergeParameters { get; set; }
        public static List<double> SplitParameters { get; set; }

        public static int ModelIndex = Constant.DCM;
        public static string ConstraintTypeStr;
        public static string DataProjectionTypeStr;
        public static string SpeedUpAlgorithmStr;

        public static double AbandonCosineThreshold;
        public static double NewTopicAlpha;
        public static double LooseOrderDeltaRatio = double.NaN;
        public static double DepthDifferenceWeight = double.NaN;
        public static double SuppressWordRatio = double.NaN;
        public static double ClusterSizeWeight = double.NaN;
        public static double LargeClusterRelaxExp = double.NaN;
        public static double OpenNodeClusterAlphaRatio = double.NaN;
        public static double SampleNumberRatio = double.NaN;
        public static int ClusterCollapseDocumentNumber = -1;

        public static string StopWordFile = null;

        public static int TitleWeight = 5;
        public static int LeadingParagraphWeight = 2;
        public static int BodyWeight = 0;

        public static bool IsBinaryStructure = false;
        
        public static void Load(string[] content)
        {
            foreach (var str in content)
            {
                var arrs = str.Split('\t');
                //if (arrs.Length < 2)
                //    throw new Exception("Config file format error.");

                switch (arrs[0])
                {
                    case "BasePath": BasePath = arrs[1]; break;
                    case "DataPath": DataPath = arrs[1]; break;
                    case "CodePath": CodePath = arrs[1]; break;
                    case "SamplePath": SamplePath = arrs[1]; break;

                    case "Gamma":
                        Gammas = new List<double>();
                        for (int i = 1; i < arrs.Length; i++)
                            Gammas.Add(double.Parse(arrs[i]));
                        break;
                    case "Alpha":
                        Alphas = new List<double>();
                        for (int i = 1; i < arrs.Length; i++)
                            Alphas.Add(double.Parse(arrs[i]));
                        break;

                    case "IndexPath": IndexPath = arrs[1]; break;
                    case "RawQueryStr": RawQueryStr = arrs[1]; break;

                    case "StartDate": StartDate = arrs[1]; break;
                    case "Timespan": Timespan = int.Parse(arrs[1]); break;
                    case "DeltaTime": DeltaTime = int.Parse(arrs[1]); break;
                    case "TimesplotsNum": TimesplotsNum = int.Parse(arrs[1]); break;
                    case "SampleNum":
                        SampleNums = new List<int>();
                        for (int i = 1; i < arrs.Length; i++)
                            SampleNums.Add(int.Parse(arrs[i]));
                        break;

                    case "KNNParameter":
                        KNNParameters = new List<int>();
                        for (int i = 1; i < arrs.Length; i++)
                            KNNParameters.Add(int.Parse(arrs[i]));
                        break;
                    case "MergeParameter":
                        MergeParameters = new List<double>();
                        for (int i = 1; i < arrs.Length; i++)
                            MergeParameters.Add(double.Parse(arrs[i]));
                        break;
                    case "SplitParameter":
                        SplitParameters = new List<double>();
                        for (int i = 1; i < arrs.Length; i++)
                            SplitParameters.Add(double.Parse(arrs[i]));
                        break;
                    case "ConstraintType": ConstraintTypeStr = arrs[1]; break;
                    case "DataProjectionType": DataProjectionTypeStr = arrs[1]; break;
                    case "AbandonCosineThreshold": AbandonCosineThreshold = double.Parse(arrs[1]); break;
                    case "NewTopicAlpha": NewTopicAlpha = double.Parse(arrs[1]); break;
                    case "SpeedUpAlgorithm": SpeedUpAlgorithmStr = arrs[1]; break;
                    case "LooseOrderDeltaRatio": LooseOrderDeltaRatio = double.Parse(arrs[1]); break;
                    case "DepthDifferenceWeight": DepthDifferenceWeight = double.Parse(arrs[1]); break;
                    case "SuppressWordRatio": SuppressWordRatio = double.Parse(arrs[1]); break;
                    case "ClusterSizeWeight": ClusterSizeWeight = double.Parse(arrs[1]); break;
                    case "LargeClusterRelaxExp": LargeClusterRelaxExp = double.Parse(arrs[1]); break;
                    case "OpenNodeClusterAlphaRatio": OpenNodeClusterAlphaRatio = double.Parse(arrs[1]); break;
                    case "ClusterCollapseDocumentNumber": ClusterCollapseDocumentNumber = int.Parse(arrs[1]); break;
                    case "SampleNumberRatio": SampleNumberRatio = double.Parse(arrs[1]); break;
                    case "StopWordFile": StopWordFile = arrs[1]; SetUserSpecifiedStopWords();  break;
                    case "TitleWeight": TitleWeight = int.Parse(arrs[1]); break;
                    case "LeadingParagraphWeight": LeadingParagraphWeight = int.Parse(arrs[1]); break;
                    case "BodyWeight": BodyWeight = int.Parse(arrs[1]); break;
                    case "Binary": IsBinaryStructure = bool.Parse(arrs[1]); break;
                }
            }

            if (DeltaTime < 0) DeltaTime = Timespan;
        }

        public static void SetUserSpecifiedStopWords(string stopwordfile = null)
        {
            if (stopwordfile == null)
                stopwordfile = StopWordFile;
            string[] lines = File.ReadAllLines(stopwordfile);
            string mode = lines[0];
            List<string> stopwords = null;

            if (mode.ToLower() == "append")
            {
                stopwords = StopWords.stopwords.ToList<string>();
            }
            else if (mode.ToLower() == "overwrite")
            {
                stopwords = new List<string>();
            }
            else
                throw new Exception("First line of stopword file should be \"overwrite\" or \"append\"");

            for (int i = 1; i < lines.Length; i++)
            {
                string term = lines[i].ToLower();
                if (!stopwords.Contains(term))
                    stopwords.Add(term);
            }

            StopWords.stopwords_BingNews_UserDefined = stopwords.ToArray<string>();
        }
    }
}
