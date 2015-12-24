using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.DataStructures;

namespace EvolutionaryRoseTree.DataStructures
{
    class RuleCacheSortedDictionary : CacheSortedDictionary
    {
        Rule maxrule = new EmptyRule();
        Rule minrule = new EmptyRule();
        public void SetMaxRule(Rule rule)
        {
            this.maxrule = rule;
        }

        public void SetMinRule(Rule rule)
        {
            this.minrule = rule;
        }

        public override double getTopOne(out RoseTreeNode node1, out RoseTreeNode node2, out int m, out double log_likelihood_ratio, out double logf, out double cache_valuearray_plus_alpha, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
            {
                if (Valid(kvp.Value) && 
                    maxrule.PassRule(kvp.Value.node1, kvp.Value.node2, kvp.Value.m))
                {
                    if (minrule.PassRule(kvp.Value.node1, kvp.Value.node2, kvp.Value.m))
                    {
                        node1 = kvp.Value.node1;
                        node2 = kvp.Value.node2;
                        m = kvp.Value.m;
                        log_likelihood_ratio = kvp.Key.log_likelihood_ratio;
                        logf = kvp.Value.logf;
                        cache_valuearray_plus_alpha = kvp.Value.cache_valuearray_plus_alpha;
                        log_likelihood_part1 = kvp.Value.log_likelihood_part1;
                        log_likelihood_part2 = kvp.Value.log_likelihood_part2;
                        return kvp.Key.keyvalue;
                    }
                }
            }

            //if above failed, leave out minrule
            //foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
            //{
            //    if (Valid(kvp.Value) &&
            //        maxrule.PassRule(kvp.Value.node1, kvp.Value.node2, kvp.Value.m))
            //    {
            //        node1 = kvp.Value.node1;
            //        node2 = kvp.Value.node2;
            //        m = kvp.Value.m;
            //        log_likelihood_ratio = kvp.Key.log_likelihood_ratio;
            //        logf = kvp.Value.logf;
            //        cache_valuearray_plus_alpha = kvp.Value.cache_valuearray_plus_alpha;
            //        log_likelihood_part1 = kvp.Value.log_likelihood_part1;
            //        log_likelihood_part2 = kvp.Value.log_likelihood_part2;

            //        if (minrule is MinRule)
            //            (minrule as MinRule).OnMerge(node1, node2);
            //        return;
            //    }
            //}

            node1 = null;
            node2 = null;
            m = -1;
            log_likelihood_ratio = double.MinValue;
            logf = double.MinValue;
            cache_valuearray_plus_alpha = double.MinValue;
            log_likelihood_part1 = double.MinValue;
            log_likelihood_part2 = double.MinValue;

            return -1;
        }

        public void ClearAll()
        {
            dict.Clear();
        }

    }
}
