using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.DataStructures;

using RoseTreeTaxonomy.Constants;
namespace EvolutionaryRoseTree.Experiments
{
    class ExperimentParameters
    {
        /// rose tree parameters that we usually modify ///
        public static string MachineName = System.Environment.MachineName;
        public static string DataPath = 
            MachineName.StartsWith("lit-infovis00", StringComparison.CurrentCultureIgnoreCase) ? @"D:\Project\EvolutionaryRoseTreeData\" : 
            MachineName.StartsWith("msraiv", StringComparison.CurrentCultureIgnoreCase) ? @"D:\Projects\EvoTree\Project\EvolutionaryRoseTreeData\" : 
            @"D:\v-xitwan\EvolutionaryRoseTreeData\";
        public static string CodePath = MachineName.StartsWith("lit-infovis00", StringComparison.CurrentCultureIgnoreCase) ? @"D:\Project\ERT\" :
            MachineName.StartsWith("msraiv", StringComparison.CurrentCultureIgnoreCase) ? @"D:\Projects\EvoTree\Project\EvolutionaryRoseTreeData\Evolutionary\matlab\" : 
            @"D:\v-xitwan\ERT\";
        //public static string DataPath = @"D:\Project\EvolutionaryRoseTreeData\";
        //public static string CodePath = @"D:\Project\ERT\";
        
        public static int SampleTimes = 1;
        public static int SampleNumber = 1000;
        public static int RoseTreeKeyWordNumber = 1024;
        public static double SampleOverlapRatio = -1;

        public static string SamplePath = DataPath + @"sampledata\";
        public static string BingNewsPath = DataPath + @"BingNewsData\";
        public static string TwentyNewsGroupPath = DataPath + @"data\nmidata\textindex_17groups\";
        public static string NewYorkTimesPath = DataPath + @"data\NYTimes\NYTIndex";
        public static string IndexedBingNewsPath = DataPath + @"BingNewsData_2012\BingNewsIndex_Microsoft";
        //query string
        public static string LoadDataQueryDefaultField = Constant.NewYorkTimesDataFields.CleanedTaxonomicClassifier;
        public static string LoadDataQueryString = "*:*";
        // for debug use
        public static string LikelihoodPath = DataPath + @"likelyhood\";
        public static string OutputDebugPath = DataPath + @"outputpath\";
        
        public static int DatasetIndex = RoseTreeTaxonomy.Constants.Constant.TWENTY_NEWS_GROUP;
        public static int ModelIndex = RoseTreeTaxonomy.Constants.Constant.DCM;
        
        public static int Time = 1853;
        
        /// default rose tree parameters ///
        public static RoseTreeParameters RoseTreeParameters = new RoseTreeParameters();

        #region /// constraint parameters ///
        public static bool BNormalizeConstraintRoseTreeWeights = false;
        public static List<RoseTree> ConstraintRoseTrees;
        public static List<RoseTree> ConstraintRoseTrees2;
        //multiple constraint trees
        public static double[] ConstraintRoseTreeWeights = new double[] { 1 };
        public static RemoveConflictParameters RemoveConflictsParameters = null;
        #endregion

        public static ConstraintType ConstraintType = ConstraintType.NoConstraint;
        public static ConstraintType[] SmoothCostConstraintTypes = null;
        public static TreeDistanceType TreeDistanceType = TreeDistanceType.Sum;

        public static double TreeDistancePunishweight = 1;
        public static double LoseOrderPunishweight = 1;
        public static double IncreaseOrderPunishweight = 1;
        public static double AffectLeaveCntPunishWeight = 1;
        public static List<double> LoglikelihoodStdRecord = null;

        /// rules ///
        public static Rules Rules = null;

        /// Rose tree result data path ///
        public static string EvolutionaryRoseTreePath = DataPath + @"Evolutionary\";
        public static string TuneParameterResultPath = DataPath + @"TuneParameters\";
        public static string ScalabilityResultPath = DataPath + @"Scalability\";
        /// Accuracy data path ///
        public static string AccuracyResultPath = DataPath + @"Accuracy\";
        public static string TimeFormat = "{0:MMdd_HHmmss}";
        /// Matlab out function path ///
        public static string OutputMatlabFunctionPath = CodePath + @"MatlabCode\data\";
        /// cache value record file name ///
        public static string CacheValueRecordFileName = null;   //outputdebugpath
        public static string OriginalConstraintTreeFileName = null;

        /// Draw rose tree path ///
        public static string DrawRoseTreePath = DataPath + @"rosetree\";

        /// Description of present experiment ///
        public static string Description = String.Format(TimeFormat, DateTime.Now);
        public static void ResetDescription() { Description = String.Format(TimeFormat, DateTime.Now); }

        /// Open node cluster size ratio ///
        public static double OpenNodeClusterSizeRatio = 0.05;
        public static double OpenNodeClusterAlphaRatio = 0.5;
        public static double OpenNodeClusterGamma = 0.1;

        /// Draw Violation Curve ///
        public static StreamWriter ViolationCurveFile = null;

        public static void UpdateBasePathRelatedPaths(string basepath)
        {
            SamplePath = basepath + @"sampledata\";
            BingNewsPath = basepath + @"BingNewsData\";
            TwentyNewsGroupPath = basepath + @"nmidata\textindex_17groups\";
            NewYorkTimesPath = basepath + @"NYTimes\NYTIndex_Year\2005";
            IndexedBingNewsPath = basepath + @"BingNewsData_2012\BingNewsIndex_Microsoft";
            LikelihoodPath = basepath + @"likelyhood\";
            OutputDebugPath = basepath + @"outputpath\";
            EvolutionaryRoseTreePath = basepath + @"Evolutionary\";
            TuneParameterResultPath = basepath + @"TuneParameters\";
            ScalabilityResultPath = basepath + @"Scalability\";
            AccuracyResultPath = basepath + @"Accuracy\";
            DrawRoseTreePath = basepath + @"rosetree\";

            OutputMatlabFunctionPath = CodePath + @"MatlabCode\data\";
        }

        public static RoseTree ConstraintRoseTree
        {
            set
            {
                if (value == null)
                    ConstraintRoseTrees = null;
                else
                {
                    ConstraintRoseTrees = new List<RoseTree>();
                    ConstraintRoseTrees.Add(value);
                }
            }
        }

        public static RoseTree ConstraintRoseTree2
        {
            set
            {
                if (value == null)
                    ConstraintRoseTrees2 = null;
                else
                {
                    ConstraintRoseTrees2 = new List<RoseTree>();
                    ConstraintRoseTrees2.Add(value);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////
        ////////                     Parameter Usage Guides                    ////////
        ///////////////////////////////////////////////////////////////////////////////
        //  Do pay attention to SampleNumber, DatasetIndex, ConstraintType
        //
        //  If constraints are to be used, do pay attention to ConstraintRoseTree.
        //      For GroundTruth, we may want to change SampleTimes;
        //      for other two constraints, Ww may want to adjust punishweight
        //
        //  If Rules are to be used
        //
        //
        //
        //
        //

    }

}
