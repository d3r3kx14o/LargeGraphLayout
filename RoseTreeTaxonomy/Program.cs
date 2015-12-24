//#define PrintDetailedProcess

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DrawTree;
using RoseTreeTaxonomy.Experiments;
using System.IO;
using RoseTreeTaxonomy.DataStructures;

namespace RoseTreeTaxonomy
{
    class Program
    {   
        static void Main(string[] args)
        {
            RandomGenerator.SetSeedFromSystemTime();
            Experiment experiment = new Experiment();
            int experiment_index = Constant.LIKELIHOOD_EXPERIMENT;

            switch (experiment_index)
            {
                case Constant.SPILLTREE_PRECISION: experiment.SpillTreePrecision(); break;
                case Constant.ROSETREE_PRECISION: experiment.RoseTreePrecision(); break;
                case Constant.RANDOM_PROJECTION_PRECISION: experiment.RandomProjectionPrecision(); break;
                case Constant.TIME_EXPERIMENT: experiment.TimeExperiment(); break;
                case Constant.HAOS_EXPERIMENT: experiment.HaosExperiment(); break;
                case Constant.NMI: experiment.NMI(); break;
                case Constant.LIKELIHOOD_EXPERIMENT: experiment.Likelihood(); break;
                case Constant.LIKELIHOOD_EXPERIMENT_STAT: experiment.LikelihoodStat(); break;
                default: break;
            }

            Console.ReadKey();  
        }
    }
}
