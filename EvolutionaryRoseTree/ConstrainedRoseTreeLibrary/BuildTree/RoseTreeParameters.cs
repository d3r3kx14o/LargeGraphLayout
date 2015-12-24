using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstrainedRoseTreeLibrary.BuildTree
{
    public enum ConstraintType { TreeOrder, NoConstraint, LooseTreeOrder };

    public class RoseTreeParameters
    {
        /// <summary>
        /// k-NN
        /// </summary>
        public int k;   
        /// <summary>
        /// Larger alpha build tree with larger granularity
        /// </summary>
        public double alpha;
        /// <summary>
        /// Grid search different gamma and finds the best
        /// </summary>
        public double gamma;
                
        //constraint parameters
        public ConstraintType constrainttype;
        public double mergeparameter;
        public double splitparameter;
        public double abandonthreshold;

        //adjust structure
        public bool badjuststructure;

        //others
        public int algorithm_index;

        public int projectdimension;
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

            //constraint parameters
            constrainttype = ConstraintType.NoConstraint;
            mergeparameter = splitparameter = 0;
            abandonthreshold = 0.3;

            //adjust structure balance
            badjuststructure = true;
        }
    }
}
