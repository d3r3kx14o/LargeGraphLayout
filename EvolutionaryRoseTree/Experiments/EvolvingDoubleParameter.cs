using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Experiments
{
    class EvolvingDouble
    {
        double para = double.NaN;
        double[] paralist = null;

        public EvolvingDouble(double para)
        {
            this.para = para;
        }

        public EvolvingDouble(double[] paralist)
        {
            this.paralist = paralist;
        }

        public double GetValue(int itime)
        {
            if (paralist == null)
                return this.para;
            else
                return this.paralist[itime];
        }

        public override string ToString()
        {
            if (paralist == null)
                return para.ToString();
            else
            {
                double sum = 0;
                foreach (double doublepara in paralist)
                    sum += doublepara;
                sum /= paralist.Length;
                return sum.ToString();
            }
        }
    }
}
