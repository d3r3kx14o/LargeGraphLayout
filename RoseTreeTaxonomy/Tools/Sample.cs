using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.Tools
{
    public class Sample
    {
        public void Run(int num, int samplenum, out int[] sample_array)
        {
            List<int> samplelist = new List<int>();
            double threshold = (double)samplenum / num;

            for (int i = 0; i < num; i++)
                if (RandomGenerator.GetUniform() < threshold)
                    samplelist.Add(i);

            while (samplelist.Count < samplenum)
            {
                int index = (int)(num * RandomGenerator.GetUniform());
                if (samplelist.Contains(index) == false)
                    samplelist.Add(index);
            }
            while (samplelist.Count > samplenum)
            {
                int index = (int)(samplelist.Count * RandomGenerator.GetUniform());
                samplelist.RemoveAt(index);
            }

            samplelist.Sort();
            sample_array = samplelist.ToArray();
        }

        //Xiting //Generate overlap data
        public void Run(int num, int[] sample_array_org, double overlapratio, out int[] sample_array)
        {
            List<int> samplelist = new List<int>();

            int samplenumber_org = sample_array_org.Length;
            int remainnumber = (int)(overlapratio * samplenumber_org);
            int samplenumber = samplenumber_org - remainnumber;

            if (samplenumber + samplenumber_org > num)
                throw new Exception("Could not achieve this overlapratio! Too Small");

            //a little easier to be outnumbered
            double threshold = (samplenumber + 5.0) / (num - samplenumber_org);

            //initialize non-overlap part
            int org_pointer = 0;
            int org_number = sample_array_org[org_pointer];
            for (int i = 0; i < num; i++)
            {
                if (i > org_number && ++org_pointer < samplenumber_org)
                    org_number = sample_array_org[org_pointer];
                if (i == org_number)
                    continue;
                if (RandomGenerator.GetUniform() < threshold)
                    samplelist.Add(i);
            }
            if (samplelist.Count < samplenumber)
            {
                List<int> samplelist_org = sample_array_org.ToList<int>();
                while (samplelist.Count < samplenumber)
                {
                    int index = (int)(num * RandomGenerator.GetUniform());
                    if (!samplelist.Contains(index) && !samplelist_org.Contains(index))
                        samplelist.Add(index);
                }
            }
            else
            {
                while (samplelist.Count > samplenumber)
                {
                    int index = (int)(samplelist.Count * RandomGenerator.GetUniform());
                    samplelist.RemoveAt(index);
                }
            }
             
            //initialize overlap part
            List<int> overlapindexlist = new List<int>();
            for (int i = 0; i < samplenumber_org; i++)
                if (RandomGenerator.GetUniform() < overlapratio)
                    overlapindexlist.Add(i);
            while (overlapindexlist.Count < remainnumber)
            {
                int index = (int)(samplenumber_org * RandomGenerator.GetUniform());
                if (!overlapindexlist.Contains(index))
                    overlapindexlist.Add(index);
            }
            while (overlapindexlist.Count > remainnumber)
            {
                int index = (int)(overlapindexlist.Count * RandomGenerator.GetUniform());
                overlapindexlist.RemoveAt(index);
            }

            //add overlap part to non-overlap part
            foreach (int index in overlapindexlist)
                samplelist.Add(sample_array_org[index]);

            samplelist.Sort();
            sample_array = samplelist.ToArray();
        }
    }
}
