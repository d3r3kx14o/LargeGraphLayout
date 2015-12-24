using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.Tools
{
    public class CacheClass
    {
        public double[] logalphaitems;     
        public double[] logpi;

        public double[][] logalphasumitems;
        public double[][] logfactorials;

        public double alpha;
        public int alphaint;
        public double alphafrac;

        public double alphasum;
        public int alphasumint;
        public double alphasumfrac;

        public double gamma;

        public int maxdimensionvalue;
        public int wordnum;
        public int datasize;

        public CacheClass()
        {
        }

        public CacheClass(double alpha, double gamma, 
            int maxdimensionvalue,  //max(wordfrequency.value)  148550
            int wordnum,            //sum(wordoccurrences)      7812282
            int datasize,           //featurevector.length      1000
            int datadimension)      //lexiconsize               47989   (wordfrequency.count:47748)
        {
            this.alpha = alpha;
            this.alphaint = (int)alpha;
            this.alphafrac = alpha - alphaint;
            this.alphasum = this.alpha * datadimension;
            this.alphasumint = (int)this.alphasum;
            this.alphasumfrac = this.alphasum - this.alphasumint;
            this.gamma = gamma;
            this.maxdimensionvalue = maxdimensionvalue;
            this.wordnum = wordnum;
            this.datasize = datasize;
        }

        public void Cache()
        {
#if PrintDetailedProcess
            Console.WriteLine("Caching log values for computing ...");
#endif
            CacheLogAlphaItems();
            CacheLogPi();
            CacheLogAlphaSumItems();
            CacheLogFactorials();
#if PrintDetailedProcess
            Console.WriteLine("Done with caching log values");
#endif
        }

        public void CacheLogAlphaItems()
        {
            double logalphaitem = 0;
            this.logalphaitems = new double[maxdimensionvalue];

            if (alphaint + 1 < alphaint + maxdimensionvalue)
            {
                for (int i = this.alphaint + 1; i <= alphaint + maxdimensionvalue; i += 1)
                {
                    logalphaitem += Math.Log(i - 1 + this.alphafrac);
                    this.logalphaitems[i - this.alphaint - 1] = logalphaitem;
                }
            }
            //else
            //{
            //    Trace.WriteLine(string.Format("alphaint:{0}", alphaint));
            //    Trace.WriteLine(string.Format("maxdimensionvalue:{0}", maxdimensionvalue));
            //    throw new Exception("error occur in input");
            //}
        }

        public void CacheLogPi()
        {
            double prod = 1.0;
            if (datasize < 2) return;
            int boundary = datasize + 1;
            this.logpi = new double[datasize - 1];

            for (int i = 2; i <= datasize; i++)
            {
                prod *= 1 - this.gamma;
                if (prod < double.Epsilon)
                {
                    boundary = i;
                    break;
                }
                else
                {
                    logpi[i - 2] = Math.Log(1.0 - prod);
                }
            }
            for (int i = boundary; i <= datasize; i++)
                logpi[i - 2] = 0;
        }

        public void CacheLogAlphaSumItems()
        {
            double logalphasumitem = 0;
            int index1 = 0;
            int index2 = 0;
            int upperbound1 = (int)((wordnum + 10e6 - 1) / 10e6);

            this.logalphasumitems = new double[upperbound1][];

            for (int i = 0; i < upperbound1; i++)
                this.logalphasumitems[i] = new double[(int)10e6];

            if (alphasumint + 1 < alphasumint + wordnum)
            {
                for (int i = alphasumint + 1; i <= alphasumint + wordnum; i++)
                {
                    logalphasumitem += Math.Log(i - 1 + alphasumfrac);
                    logalphasumitems[index1][index2] = logalphasumitem;
                    index2++;

                    if (index2 == 10e6)
                    {
                        index1++;
                        index2 = 0;
                    }
                }
            }
            //else
            //{
            //    Trace.WriteLine(string.Format("alphasumint:{0}", alphasumint));
            //    Trace.WriteLine(string.Format("wordnum:{0}", wordnum));
            //    throw new Exception("error occur in input");
            //}
        }

        public void CacheLogFactorials()
        {
            double logfactorial = 0;
            int index1 = 0;
            int index2 = 0;
            int upperbound1 = (int)((wordnum + 10e6 - 1) / 10e6);

            this.logfactorials = new double[upperbound1][];

            for (int i = 0; i < upperbound1; i++)
                this.logfactorials[i] = new double[(int)10e6];

            for (int i = 1; i <= wordnum; i++)
            {
                logfactorial += Math.Log((double)i);
                logfactorials[index1][index2] = logfactorial;
                index2++;

                if (index2 == 10e6)
                {
                    index1++;
                    index2 = 0;
                }
            }
        }

        public virtual double GetLogAlphaItem(int value)
        {
            try
            {
                return this.logalphaitems[value - 1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return double.MinValue;
            }
        }

        public virtual double GetLogPi(int value)
        {
            try
            {
                return this.logpi[value - 2];
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return double.MinValue;
            }
        }

        public virtual double GetLogOneMinusPi(int value)
        {
            return (value - 1) * Math.Log(1 - this.gamma);
        }

        public virtual double GetLogAlphaSumItem(int value)
        {
            int index1 = (int)((value - 1) / 10e6);
            int index2 = (value - 1) % (int)10e6;

            try
            {
                return this.logalphasumitems[index1][index2];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return double.MinValue;
            }
        }

        public virtual double GetLogFactorials(int value)
        {
            int index1 = (int)((value - 1) / 10e6);
            int index2 = (value - 1) % (int)10e6;

            try
            {
                return this.logfactorials[index1][index2];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return double.MinValue;
            }
        }
    }
}
