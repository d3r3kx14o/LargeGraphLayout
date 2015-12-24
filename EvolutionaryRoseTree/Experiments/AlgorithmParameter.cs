using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Constants;
namespace EvolutionaryRoseTree.Experiments
{
    abstract class AlgorithmParameter
    {
        public virtual void Set()
        {
        }
    }

    class BRTAlgorithmParameter : AlgorithmParameter
    {
        public override void Set()
        {
            ExperimentParameters.RoseTreeParameters.algorithm_index = Constant.BRT;
        }

        public override string ToString()
        {
            return "<BRT>";
        }
    }

    class KNNAlgorithmParameter : AlgorithmParameter
    {
        int k;
        public KNNAlgorithmParameter(int K)
        {
            k = K;
        }

        public override void Set()
        {
            ExperimentParameters.RoseTreeParameters.algorithm_index = Constant.KNN_BRT;
            ExperimentParameters.RoseTreeParameters.k = k;
        }

        public override string ToString()
        {
            return "<KNN:" + k + ">";
        }
    }

    class SpillTreeAlgorithmParameter : AlgorithmParameter
    {
        int projectdimension;
        int k;
        double tau;
        public SpillTreeAlgorithmParameter(int ProjectDimension, int K, double Tau = 0.1)
        {
            projectdimension = ProjectDimension;
            k = K;
            tau = Tau;
        }

        public override void Set()
        {
            ExperimentParameters.RoseTreeParameters.algorithm_index = Constant.SPILLTREE_BRT;
            ExperimentParameters.RoseTreeParameters.projectdimension = projectdimension;
            ExperimentParameters.RoseTreeParameters.k = k;
            ExperimentParameters.RoseTreeParameters.tau = tau;
        }

        public override string ToString()
        {
            return "<Spill:" + projectdimension + " k:" + k + " tau:" + tau + ">";
        }
    }
}
