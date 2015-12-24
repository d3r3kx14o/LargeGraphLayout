using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.DataStructures;
namespace EvolutionaryRoseTree.DataStructures
{
    class ConstrainedCacheKey : CacheKey
    {
        public static double similarityBalanceWeight = 0;
        public static int keycutofftaillength = 8;
        public static double logsimilarityWeight = 1 / Math.Log(2);
        public static double depthdifferenceWeight = 1;
        
        public double log_posterior_ratio;
#if APPROXIMATE_LIKELIHOOD
        public double similarity;
        public double compoundkey;
#endif
        public override double keyvalue { get { return log_posterior_ratio; } }


        public ConstrainedCacheKey(double log_posterior_ratio, double log_likelihood_ratio,
double tag, int depth_difference, double similarity) :
            base(log_likelihood_ratio, tag, depth_difference)
        {
#if APPROXIMATE_LIKELIHOOD
            log_posterior_ratio = Math.Round(log_posterior_ratio, keycutofftaillength);
            this.log_posterior_ratio = log_posterior_ratio;
            this.similarity = similarity;

            this.secondarykey = Math.Log(similarity) * logsimilarityWeight - depthdifferenceWeight * depth_difference;
            this.compoundkey = log_posterior_ratio + similarityBalanceWeight * secondarykey;
#else
            this.log_posterior_ratio = log_posterior_ratio;
#endif
        }

        public void UpdatePosteriorRatio(double log_posterior_ratio)
        {
#if NEW_CONSTRAINT_MODEL
            log_likelihood_ratio = log_posterior_ratio;
#endif

#if APPROXIMATE_LIKELIHOOD
            log_posterior_ratio = Math.Round(log_posterior_ratio, keycutofftaillength);
            this.log_posterior_ratio = log_posterior_ratio;

            this.compoundkey = log_posterior_ratio + similarityBalanceWeight * secondarykey;
#else
            this.log_posterior_ratio = log_posterior_ratio;
#endif
        }

        public override int CompareTo(object o)
        {
            ConstrainedCacheKey ck = (ConstrainedCacheKey)o;
#if APPROXIMATE_LIKELIHOOD
            if (this.compoundkey > ck.compoundkey)
                return -1;
            else if (this.compoundkey < ck.compoundkey)
                return 1;
            else if (this.secondarykey > ck.secondarykey)
                return -1;
            else if (this.secondarykey < ck.secondarykey)
                return 1;
            else if (this.similarity > ck.similarity)
                return -1;
            else if (this.similarity < ck.similarity)
                return 1;
#else
            if (this.log_posterior_ratio > ck.log_posterior_ratio)
                return -1;
            else if (this.log_posterior_ratio < ck.log_posterior_ratio)
                return 1;
#endif
            else if (this.depth_difference < ck.depth_difference)
                return -1;
            else if (this.depth_difference > ck.depth_difference)
                return 1;
            else if (this.tag < ck.tag)
                return -1;
            else if (this.tag > ck.tag)
                return 1;
            else
            {
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine(this.tag);
                Console.WriteLine(ck.tag);
                Console.WriteLine(this.Equals(ck));
                Console.WriteLine(this.depth_difference);
                Console.WriteLine(ck.depth_difference);
                Console.WriteLine("Error Occur");
                Console.ReadKey();
                throw new Exception("Error occur");
            }
        }
    }

    class ConstrainedCacheValue : CacheValue
    {
#if NEW_CONSTRAINT_MODEL
        public double log_treeprobability;
        public ConstrainedCacheValue(RoseTreeNode node1, RoseTreeNode node2, int m, double cache_valuearray_plus_alpha, double logf, double log_likelihood_part1, double log_likelihood_part2,
            double log_treeprobability)
            : base(node1, node2, m, cache_valuearray_plus_alpha, logf, log_likelihood_part1, log_likelihood_part2)
        {
            this.log_treeprobability = log_treeprobability;
        }

        public void UpdateCacheValue(double log_likelihood_part1, double log_likelihood_part2, double log_treeprobability)
        {
            this.log_likelihood_part1 = log_likelihood_part1;
            this.log_likelihood_part2 = log_likelihood_part2;
            this.log_treeprobability = log_treeprobability;
        }
#else
        public double log_treeprobability_ratio { get; protected set; }
        public ConstrainedCacheValue(RoseTreeNode node1, RoseTreeNode node2, int m, double cache_valuearray_plus_alpha, double logf, double log_likelihood_part1, double log_likelihood_part2,
            double log_treeprobability_ratio)
            : base(node1, node2, m, cache_valuearray_plus_alpha, logf, log_likelihood_part1, log_likelihood_part2)
        {
            this.log_treeprobability_ratio = log_treeprobability_ratio;
        }
        public void UpdateCacheValue(double log_treeprobability_ratio)
        {
            this.log_treeprobability_ratio = log_treeprobability_ratio;
        }
#endif
    }
}
