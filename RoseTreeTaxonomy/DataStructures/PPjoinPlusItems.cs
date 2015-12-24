using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.DataStructures
{
    public class Pairs
    {
        public PPjoinPlusItems item1;
        public PPjoinPlusItems item2;
    }

    public class PPjoinPlusItems
    {
        public int id;
        public int[] tokens;

        public PPjoinPlusItems()
        {
        }

        public PPjoinPlusItems(int[] tokens, int id)
        {
            this.tokens = tokens;
            this.id = id;
        }

        public int size()
        {
            return tokens.Length;
        }

        public int get(int i)
        {
            return tokens[i];
        }

        public int CompareTo(PPjoinPlusItems o)
        {
            int num1 = this.size();
            int num2 = o.size();
            if (num1 < num2)
                return -1;
            else if (num2 < num1)
                return 1;
            else
                return 0;
        }
    }
}
