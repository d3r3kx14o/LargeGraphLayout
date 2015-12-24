using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Util
{
    class Utils
    {
        public static List<string> GetTopWords(Dictionary<string, double> dict,
            int wordNumber, bool bDescend = true)
        {
            var factor = bDescend ? -1 : 1;
            List<string> topwords = new List<string>();
            int wordIndex = 0;
            foreach (var kvp in dict.OrderBy(kvp => factor * kvp.Value))
            {
                topwords.Add(kvp.Key);
                if (++wordIndex >= wordNumber)
                    break;
            }
            return topwords;
        }

    }
}
