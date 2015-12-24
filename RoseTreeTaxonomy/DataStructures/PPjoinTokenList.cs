using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.DataStructures
{
    public class PPjoinTokenList
    {
        public List<int> keyList = new List<int>();
        public PPjoinPlusItems item = new PPjoinPlusItems();
        //public int count = 0;
        public double ppjoin_norm_value;
        public double overlap_lowerbound;
        public int probingprefixLength;
        public int indexPrefixLength;
        public int ppjoin_similarity_type;

        public PPjoinTokenList(int ppjoin_similarity_type)
        {
            this.ppjoin_similarity_type = ppjoin_similarity_type;
        }


    }
}
