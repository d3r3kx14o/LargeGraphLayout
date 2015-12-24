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

namespace EvolutionaryRoseTree.Experiments
{
    class Experiment
    {
        public static void ExperimentEntry()
        {
        }


        public static void RoseTreeExperiment()
        {
            RandomGenerator.SetSeedFromSystemTime();
            RoseTreeTaxonomy.Experiments.Experiment experiment = new RoseTreeTaxonomy.Experiments.Experiment();
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
        }

        public static void PrintDefines()
        {
            Console.WriteLine("-----------------------Defined--------------------------");
#if APPROXIMATE_LIKELIHOOD
            Console.Write("APP\t");
#endif
//#if SUPPRESS_WORD
//            Console.Write("SUPP\t");
//#endif
#if AVERAGE_ORDER_COST
            Console.Write("AvgO\t");
#endif
#if AVERAGE_ORDER_COST2
            Console.Write("AvgO2\t");
#endif
#if OPEN_LARGE_CLUSTER
            Console.Write("OLC\t");
#endif
#if OPEN_LARGE_CLUSTER_MOD_2
            Console.Write("OLC2\t");
#endif
#if COLLAPSE_SMALL_CLUSTER
            Console.Write("CSC\t");
#endif
#if SCALABILITY_TEST
            Console.Write("ScaTest\t");
#endif
#if NEW_CONSTRAINT_MODEL
                Console.Write("NCM\t");
#endif
#if NEW_MODEL_2
                Console.Write("NCM2\t");
#endif
#if NEW_MODEL_3
                Console.Write("NCM3\t");
#endif
#if UNSORTED_CACHE
                Console.Write("UnCache\t");
#endif
#if CONSTRAINT_CHANGE_UPDATE_ALL
                Console.Write("UpdateAll\t");
#endif
#if SMOOTHNESS_ANALYSE
                Console.Write("SMAnaly\t");
#endif
#if DISTANCE_CONSTRAINT_2
                Console.Write("DisC2\t");
#endif
#if NEW_YORK_TIMES_TEST_SMOOTHNESS
                Console.Write("NYTSmoothTest\t");
#endif
#if NORMALIZED_SMOOTHNESS_COST
                Console.Write("NormSmCost\t");
#endif
#if WRITE_PROJECTION_CONTENT
                Console.Write("WriteProj\t");
#endif
#if NYT_LEADING_PARAGRAPH 
                Console.Write("NYTLeading\t");
#endif

            Console.WriteLine("\n-----------------------Not Defined--------------------------");

#if !APPROXIMATE_LIKELIHOOD
            Console.Write("APP\t");
#endif
//#if !SUPPRESS_WORD
//            Console.Write("SUPP\t");
//#endif
#if !AVERAGE_ORDER_COST
            Console.Write("AvgO\t");
#endif
#if !AVERAGE_ORDER_COST2
            Console.Write("AvgO2\t");
#endif
#if !OPEN_LARGE_CLUSTER
            Console.Write("OLC\t");
#endif
#if !OPEN_LARGE_CLUSTER_MOD_2
            Console.Write("OLC2\t");
#endif
#if !COLLAPSE_SMALL_CLUSTER
            Console.Write("CSC\t");
#endif
#if !SCALABILITY_TEST
            Console.Write("ScaTest\t");
#endif
#if !NEW_CONSTRAINT_MODEL
                Console.Write("NCM\t");
#endif
#if !NEW_MODEL_2
                Console.Write("NCM2\t");
#endif
#if !NEW_MODEL_3
                Console.Write("NCM3\t");
#endif
#if !UNSORTED_CACHE
                Console.Write("UnCache\t");
#endif
#if !CONSTRAINT_CHANGE_UPDATE_ALL
                Console.Write("UpdateAll\t");
#endif
#if !SMOOTHNESS_ANALYSE
                Console.Write("SMAnaly\t");
#endif
#if !DISTANCE_CONSTRAINT_2
                Console.Write("DisC2\t");
#endif
#if !NEW_YORK_TIMES_TEST_SMOOTHNESS
                Console.Write("NYTSmoothTest\t");
#endif
#if !NORMALIZED_SMOOTHNESS_COST
                Console.Write("NormSmCost\t");
#endif
#if !WRITE_PROJECTION_CONTENT
                Console.Write("WriteProj\t");
#endif
#if !NYT_LEADING_PARAGRAPH
                Console.Write("NYTLeading\t");
#endif
            Console.WriteLine("\n--------------------------End-----------------------------");


        }
    }
}
