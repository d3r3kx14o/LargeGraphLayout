using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.ReadData;
namespace EvolutionaryRoseTree.DataStructures
{
    class ExpandedCacheClass : CacheClass
    {
        CacheClass baseCacheClass;

        public ExpandedCacheClass(CacheClass baseCacheClass)
        {
            alpha = baseCacheClass.alpha;
            gamma = baseCacheClass.gamma;
            alphasum = baseCacheClass.alphasum;
            wordnum = baseCacheClass.wordnum;

            this.baseCacheClass = baseCacheClass;

            //log alpha items
            logalphaitems = baseCacheClass.logalphaitems;
            logalphaitems_length = logalphaitems.Length;
            last_logalphaitems = logalphaitems[logalphaitems_length - 1];

            //log alpha sum iem
            logalphasumitems = baseCacheClass.logalphasumitems;
            int index1 = (int)((wordnum - 1) / 10e6);
            int index2 = (wordnum - 1) % (int)10e6;
            last_logalphasumitems = logalphasumitems[index1][index2];

            //log factorials
            logfactorials = baseCacheClass.logfactorials;
            last_logfactorials = logfactorials[index1][index2];
        }

        List<double> expanded_logalphaitems = new List<double>();
        int logalphaitems_length;
        double last_logalphaitems;
        public override double GetLogAlphaItem(int value)
        {
            if (value <= logalphaitems_length)
                return logalphaitems[value - 1];
            else
            {
                if (value > logalphaitems_length + expanded_logalphaitems.Count)
                {
                    //expand
                    double logalphaitem = last_logalphaitems;
                    for (int i = logalphaitems_length + expanded_logalphaitems.Count + 1; i <= value; i++)
                    {
                        logalphaitem += Math.Log(alpha + i - 1);
                        expanded_logalphaitems.Add(logalphaitem);
                    }
                    //set next base
                    last_logalphaitems = logalphaitem;
                }

                return expanded_logalphaitems[value - logalphaitems_length -1];
            }
        }

        public override double GetLogPi(int value)
        {
            return baseCacheClass.GetLogPi(value);
        }

        public override double GetLogOneMinusPi(int value)
        {
            return baseCacheClass.GetLogOneMinusPi(value);
        }

        List<double> expanded_logalphasumitems = new List<double>();
        double last_logalphasumitems;
        public override double GetLogAlphaSumItem(int value)
        {
            if (value <= wordnum)
            {
                int index1 = (int)((value - 1) / 10e6);
                int index2 = (value - 1) % (int)10e6;
                return this.logalphasumitems[index1][index2];
            }
            else
            {
                if (value > wordnum + expanded_logalphasumitems.Count)
                {
                    //expand
                    double logalphasumitem = last_logalphasumitems;
                    for (int i = wordnum + expanded_logalphasumitems.Count + 1; i <= value; i++)
                    {
                        logalphasumitem += Math.Log(alphasum + i - 1);
                        expanded_logalphasumitems.Add(logalphasumitem);
                    }
                    last_logalphasumitems = logalphasumitem;
                }

                return expanded_logalphasumitems[value - wordnum - 1];
            }
        }

        List<double> expanded_logfactorials = new List<double>();
        double last_logfactorials;
        public override double GetLogFactorials(int value)
        {
            if (value <= wordnum)
            {
                int index1 = (int)((value - 1) / 10e6);
                int index2 = (value - 1) % (int)10e6;
                return this.logfactorials[index1][index2];
            }
            else
            {
                if (value > wordnum + expanded_logfactorials.Count)
                {
                    //expand
                    double logfactorial = last_logfactorials;
                    for (int i = wordnum + expanded_logfactorials.Count + 1; i <= value; i++)
                    {
                        logfactorial += Math.Log((double)i);
                        expanded_logfactorials.Add(logfactorial);
                    }
                    last_logfactorials = logfactorial;
                }

                return expanded_logfactorials[value - wordnum - 1];
            }
        }
    }
}
