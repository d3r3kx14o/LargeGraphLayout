using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Constants;
using EvolutionaryRoseTree.Constraints;
namespace EvolutionaryRoseTree.Experiments
{
    class BingNewsParameters
    {
        public int model_index { get; protected set; }
        public double[] gammas { get; protected set; }
        public double[] alphas { get; protected set; }
        public double[] kappas { get; protected set; }
        public double[] kappaR0s { get; protected set; }
        public double[] mergeparas { get; protected set; }
        public double[] splitparas { get; protected set; }
        public int[] knnparameters { get; protected set; }
        public int[] samplenumbers { get; protected set; }
        public string ConstraintType;
        public DataProjectionType InputDataProjectionType;
        public bool IsBinaryStructure;
        
        bool bAnalysisSmoothness;
        int timeslotsnum;

        public BingNewsParameters(bool bAnalysisSmoothness = false)
        {
            this.bAnalysisSmoothness = bAnalysisSmoothness;
            model_index = ConfigEvolutionary.ModelIndex;
            ConstraintType = ConfigEvolutionary.ConstraintTypeStr;
            timeslotsnum = ConfigEvolutionary.TimesplotsNum;
            InputDataProjectionType = (DataProjectionType)System.Enum.Parse(typeof(DataProjectionType), ConfigEvolutionary.DataProjectionTypeStr);
            /// Sample Number ///
            if (ConfigEvolutionary.SampleNums.Count >= timeslotsnum)
                samplenumbers = ConfigEvolutionary.SampleNums.ToArray<int>();
            else if (ConfigEvolutionary.SampleNums.Count == 1)
            {
                samplenumbers = new int[timeslotsnum];
                for (int i = 0; i < timeslotsnum; i++)
                    samplenumbers[i] = ConfigEvolutionary.SampleNums[0];
            }
            else
                throw new Exception("Error loading config parameters! SampleNumber number does not match!");
            /// Gamma ///
            if (ConfigEvolutionary.Gammas.Count >= timeslotsnum)
                gammas = ConfigEvolutionary.Gammas.ToArray<double>();
            else if (ConfigEvolutionary.Gammas.Count == 1)
            {
                gammas = new double[timeslotsnum];
                for (int i = 0; i < timeslotsnum; i++)
                    gammas[i] = ConfigEvolutionary.Gammas[0];
            }
            else
                throw new Exception("Error loading config parameters! Gamma number does not match!");
            /// Sample Numbers ///
            if (ConfigEvolutionary.KNNParameters.Count >= timeslotsnum)
                knnparameters = ConfigEvolutionary.KNNParameters.ToArray<int>();
            else if (ConfigEvolutionary.KNNParameters.Count == 1)
            {
                knnparameters = new int[timeslotsnum];
                for (int i = 0; i < timeslotsnum; i++)
                    knnparameters[i] = ConfigEvolutionary.KNNParameters[0];
            }
            else
                throw new Exception("Error loading config parameters! sample number does not match!");
     
            /// Merge & Split ///
            if (ConfigEvolutionary.MergeParameters.Count >= timeslotsnum - 1)
                mergeparas = ConfigEvolutionary.MergeParameters.ToArray<double>();
            else if (ConfigEvolutionary.MergeParameters.Count == 1)
            {
                mergeparas = new double[timeslotsnum - 1];
                for (int i = 0; i < timeslotsnum - 1; i++)
                    mergeparas[i] = ConfigEvolutionary.MergeParameters[0];
            }
            else
                throw new Exception("Error loading config parameters! MergeParameters number does not match!");
            if (ConfigEvolutionary.SplitParameters.Count >= timeslotsnum - 1)
                splitparas = ConfigEvolutionary.SplitParameters.ToArray<double>();
            else if (ConfigEvolutionary.SplitParameters.Count == 1)
            {
                splitparas = new double[timeslotsnum - 1];
                for (int i = 0; i < timeslotsnum - 1; i++)
                    splitparas[i] = ConfigEvolutionary.SplitParameters[0];
            }
            else
                throw new Exception("Error loading config parameters! SplitParameters number does not match!");
       
            if (ConfigEvolutionary.ModelIndex == Constant.DCM)
            {
                /// Alpha ///
                if (ConfigEvolutionary.Alphas.Count >= timeslotsnum)
                    alphas = ConfigEvolutionary.Alphas.ToArray<double>();
                else if (ConfigEvolutionary.Alphas.Count == 1)
                {
                    alphas = new double[timeslotsnum];
                    for (int i = 0; i < timeslotsnum; i++)
                        alphas[i] = ConfigEvolutionary.Alphas[0];
                }
                else
                    throw new Exception("Error loading config parameters! Alpha number does not match!");
            }
            else //vMF
                throw new NotImplementedException();

            /// Is Binary ///
            IsBinaryStructure = ConfigEvolutionary.IsBinaryStructure;
        }

        public void Set(int itime)
        {
            if (model_index == Constant.DCM)
            {
                ExperimentParameters.RoseTreeParameters.gamma = gammas[itime];
                ExperimentParameters.RoseTreeParameters.alpha = alphas[itime];
                ExperimentParameters.SampleNumber = samplenumbers[itime];
                //KNN
                ExperimentParameters.RoseTreeParameters.algorithm_index = Constant.KNN_BRT;
                ExperimentParameters.RoseTreeParameters.k = knnparameters[itime];
                //SpillTree
                if (ConfigEvolutionary.SpeedUpAlgorithmStr == "SpillTree")
                {
                    AlgorithmParameter algorithmParameter = new SpillTreeAlgorithmParameter(10, ExperimentParameters.RoseTreeParameters.k);
                    algorithmParameter.Set();
                }
            }
            else //vMF
                throw new NotImplementedException();
        }

        public ConstraintParameter GetConstraintParameter(int itime)
        {
            if (itime == timeslotsnum - 1)
                return null;
            switch (ConstraintType)
            {
                case "Order":
                    return new OrderConstraintParameter(mergeparas[itime], splitparas[itime], bAnalysisSmoothness);
                case "LooseOrder":
                    return new LooseOrderConstraintParameter(mergeparas[itime], splitparas[itime], bAnalysisSmoothness);
                case "Distance":
                    return new DistanceConstraintParameter(mergeparas[itime], bAnalysisSmoothness);
                case "No":
                    return new NoConstraintParameter(bAnalysisSmoothness);
                default:
                    throw new Exception("Unknown ConstraintType in config files!");
            }
        }

    }
}
