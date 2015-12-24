using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.DataStructures;

namespace RoseTreeTaxonomy.Tools
{
    public class CacheKey: IComparable
    {
        public double log_likelihood_ratio;
        public double tag;
        public int depth_difference;
        public virtual double keyvalue { get { return log_likelihood_ratio; } }
        public double secondarykey;

        public CacheKey(double log_likelihood_ratio, double tag, int depth_difference)
        {
            this.log_likelihood_ratio = log_likelihood_ratio;
            this.tag = tag;
            this.depth_difference = depth_difference;
        }

        public virtual int CompareTo(object o)
        {
            CacheKey ck = (CacheKey)o;

            if (this.log_likelihood_ratio > ck.log_likelihood_ratio)
                return -1;
            else if (this.log_likelihood_ratio < ck.log_likelihood_ratio)
                return 1;
            else if (this.depth_difference < ck.depth_difference)
                return -1;
            else if (this.depth_difference > ck.depth_difference)
                return 1;
            else if (this.tag < ck.tag)
                return -1;
            else if (this.tag > ck.tag)
                return 1;
            else
                throw new Exception("Error occur");
        }
    }

    public class CacheValue
    {
        public RoseTreeNode node1;
        public RoseTreeNode node2;
        public int m;
        public double cache_valuearray_plus_alpha;
        public double logf;
        public double log_likelihood_part1;
        public double log_likelihood_part2;

        public CacheValue(RoseTreeNode node1, RoseTreeNode node2, int m, double cache_valuearray_plus_alpha, double logf, double log_likelihood_part1, double log_likelihood_part2)
        {
            this.node1 = node1;
            this.node2 = node2;
            this.m = m;
            this.cache_valuearray_plus_alpha = cache_valuearray_plus_alpha;
            this.logf = logf;
            this.log_likelihood_part1 = log_likelihood_part1;
            this.log_likelihood_part2 = log_likelihood_part2;
        }
    }

    public class CacheSortedDictionary
    {
        public SortedDictionary<CacheKey, CacheValue> dict;

        public CacheSortedDictionary()
        {
            this.dict = new SortedDictionary<CacheKey, CacheValue>();
        }

        public void Insert(CacheKey newkey, CacheValue newvalue)
        {
            if (newkey == null || newvalue == null || newkey.log_likelihood_ratio == double.MinValue) return;
            this.dict.Add(newkey, newvalue);
        }

        public void ClearInvalidItems()
        {
            List<CacheKey> tobedelete = new List<CacheKey>();

            foreach (KeyValuePair<CacheKey, CacheValue> kvp in this.dict)
            {
                CacheKey ck = kvp.Key;
                CacheValue cv = kvp.Value;

                if (this.Valid(cv) == false)
                    tobedelete.Add(ck);
            }

            for (int i = 0; i < tobedelete.Count; i++)
                this.dict.Remove(tobedelete[i]);
        }

        public bool Valid(CacheValue cv)
        {
            return (cv.node1.valid == true) && (cv.node2.valid == true);
        }

        public virtual double getTopOne(out RoseTreeNode node1, out RoseTreeNode node2, out int m, out double log_likelihood_ratio, out double logf, out double cache_valuearray_plus_alpha, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
            {
                if (Valid(kvp.Value) == true)
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

        public CacheValue getTopValue()
        {
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
                return kvp.Value;
            return null;
        }

        public bool RemoveKey(CacheKey cachekey)
        {
            return dict.Remove(cachekey);
        }

        public void RemoveInvalidateNodePairs(RoseTreeNode node)
        {
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in node.CacheMergePairs)
            {
                RoseTreeNode othernode = kvp.Value.node1 == node ? kvp.Value.node2 : kvp.Value.node1;
                if (othernode.valid)
                    dict.Remove(kvp.Key);
            }
        }

        public void PrintAll(int PrintLength = int.MaxValue)
        {
            int printedlen = 0;
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
            {
                if (Valid(kvp.Value))
                {
                    Console.WriteLine("{0},{1},{2}  {3},{4}", kvp.Value.node1.MergeTreeIndex, kvp.Value.node2.MergeTreeIndex,
                        kvp.Value.m, kvp.Key.keyvalue, kvp.Key.secondarykey);
                    //Console.WriteLine("{0},{1},{2}  {3},{4},{5}, {6}", kvp.Value.node1.MergeTreeIndex, kvp.Value.node2.MergeTreeIndex,
                    //    kvp.Value.m, kvp.Key.log_likelihood_ratio, kvp.Key.keyvalue - kvp.Key.log_likelihood_ratio, kvp.Key.keyvalue, kvp.Key.secondarykey);
                    printedlen++;
                    if (printedlen > PrintLength)
                        break;
                }
            }
        }

        public void PrintSelected(int nodeindex1, int nodeindex2)
        {
            int order = 0;
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
            {
                if (Valid(kvp.Value))
                {
                    int index = kvp.Value.node1.MergeTreeIndex;
                    if (index == nodeindex1 || index == nodeindex2)
                    {
                        index = kvp.Value.node2.MergeTreeIndex;
                        if (index == nodeindex1 || index == nodeindex2)
                        {
                            Console.WriteLine("Order: {0}", order);
                            Console.WriteLine("{0},{1},{2}  {3},{4}", kvp.Value.node1.MergeTreeIndex, kvp.Value.node2.MergeTreeIndex,
                            kvp.Value.m, kvp.Key.keyvalue, kvp.Key.secondarykey);
                            //Console.WriteLine("{0},{1},{2}  {3},{4},{5}", kvp.Value.node1.MergeTreeIndex, kvp.Value.node2.MergeTreeIndex,
                            //    kvp.Value.m, kvp.Key.log_likelihood_ratio, kvp.Key.keyvalue - kvp.Key.log_likelihood_ratio, kvp.Key.keyvalue);
                        }
                    }
                    order++;
                }
            }
        }

        public static double RecordedStd;
        public string AllValidPairToString()
        {
            //string str = "";
            //foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
            //{
            //    if (Valid(kvp.Value))
            //    str += string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5};", kvp.Value.node1.MergeTreeIndex, kvp.Value.node2.MergeTreeIndex,
            //       kvp.Value.m, kvp.Key.log_likelihood_ratio, kvp.Key.keyvalue - kvp.Key.log_likelihood_ratio, kvp.Key.keyvalue);
            //}
            //return str;
            DataStatistic loglikelihoodStat = new DataStatistic();
            DataStatistic logtreeprobStat = new DataStatistic();
            DataStatistic logposterior = new DataStatistic();
            foreach (KeyValuePair<CacheKey, CacheValue> kvp in dict)
                if (Valid(kvp.Value))
                {
                    loglikelihoodStat.AddData(kvp.Key.log_likelihood_ratio);
                    logtreeprobStat.AddData(kvp.Key.keyvalue - kvp.Key.log_likelihood_ratio);
                    logposterior.AddData(kvp.Key.keyvalue);
                }
            RecordedStd = loglikelihoodStat.Max-loglikelihoodStat.Avg;
            return loglikelihoodStat.ToString() + logtreeprobStat.ToString() + logposterior.ToString();
        }

    }

    class DataStatistic
    {
        public double Max = double.MinValue;
        public double Min = double.MaxValue;
        public double Avg { get { return Sum / cnt; } }
        public double Std { get { return Math.Sqrt(SumSquare / cnt - Avg * Avg); } }
        private double Sum = 0;
        private double SumSquare = 0;
        private int cnt = 0;

        public void AddData(double data)
        {
            Sum += data;
            SumSquare += data * data;
            if (data > Max)
                Max = data;
            if (data < Min)
                Min = data;
            cnt++;
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3};", Max, Min, Avg, Std);
        }
    }
}
