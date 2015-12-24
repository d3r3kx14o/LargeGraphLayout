using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseTreeTaxonomy.Tools
{
    public class Util
    {
        public static void Write(string message, int progress, int total, TimeSpan timeElaspsed)
        {
            Console.Write("\r" + message + " ({0}/{1}), ETA {2}", progress, total, new TimeSpan(timeElaspsed.Ticks * (total - progress) / (Math.Max(progress, 1))).TotalMinutes);
        }
    }
}
