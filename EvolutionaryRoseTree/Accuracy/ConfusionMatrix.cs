using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Accuracy
{
    class ConfusionMatrix
    {
        public static double[,] GetConfuseMatrix(int[] label1, int[] label2)
        {
            int N = label1.Length;

            List<int>[] cluster1 = GetCluster(label1);
            List<int>[] cluster2 = GetCluster(label2);
            int clusterNum1 = cluster1.Length;
            int clusterNum2 = cluster2.Length;

            double[,] confuseMat = new double[clusterNum1, clusterNum2];
            for (int i = 0; i < clusterNum1; i++)
                for (int j = 0; j < clusterNum2; j++)
                    confuseMat[i, j] = GetOverlapNumber(cluster1[i], cluster2[j]);

            return confuseMat;
        }

        //ordered list
        public static int GetOverlapNumber(List<int> group0, List<int> group1)
        {
            int overlapcnt = 0;

            List<int>.Enumerator listenum0 = group0.GetEnumerator();
            List<int>.Enumerator listenum1 = group1.GetEnumerator();
            listenum0.MoveNext();
            listenum1.MoveNext();
            int member0 = listenum0.Current;
            int member1 = listenum1.Current;
            while (true)
            {
                if (member0 == member1)
                {
                    overlapcnt++;
                    if (!listenum0.MoveNext())
                        break;
                    if (!listenum1.MoveNext())
                        break;
                }
                else if (member0 < member1)
                {
                    while (listenum0.MoveNext())
                        if (listenum0.Current >= member1)
                            break;
                    if (listenum0.Current < member1)
                        break;
                }
                else //if (member0 > member1)
                {
                    while (listenum1.MoveNext())
                        if (listenum1.Current >= member0)
                            break;
                    if (listenum1.Current < member0)
                        break;
                }

                member0 = listenum0.Current;
                member1 = listenum1.Current;
            }


            return overlapcnt;
        }

        private static List<int>[] GetCluster(int[] label)
        {
            int N = label.Length;
            int clusternum = 0;
            Dictionary<int, int> label2indexmap = new Dictionary<int, int>();

            for (int i = 0; i < N; i++)
                if (!label2indexmap.ContainsKey(label[i]))
                {
                    label2indexmap.Add(label[i], clusternum);
                    clusternum++;
                }

            List<int>[] cluster = new List<int>[clusternum];
            for (int i = 0; i < clusternum; i++)
                cluster[i] = new List<int>();

            for (int i = 0; i < N; i++)
                cluster[label2indexmap[label[i]]].Add(i);

            return cluster;
        }

        public static string ToString(double[,] confusemat)
        {
            string str = "<Confusion Matrix> [" + confusemat.GetLength(0) + "," + confusemat.GetLength(1) + "]\n";
            //bool bPrevNewLine = true;
            if (confusemat.GetLength(0) > 10 || confusemat.GetLength(1) > 10)
            {
                //for (int i = 0; i < confusemat.GetLength(0); i++)
                //{
                //    string substr = "";
                //    int cnt = 0;
                //    for (int j = 0; j < confusemat.GetLength(1); j++)
                //        if (confusemat[i, j] != 0)
                //        {
                //            substr += "<" + i + "," + j + "," + (int)confusemat[i, j] + ">\t";
                //            cnt += (int)confusemat[i, j];
                //        }
                //    bool bNewLine = cnt > 1;
                //    if (bNewLine)
                //    {
                //        substr = "[" + cnt + "]\t" + substr;
                //        if (bPrevNewLine)
                //            str += substr + "\n";
                //        else
                //            str += "\n" + substr + "\n";
                //    }
                //    else
                //        str += substr;
                //    bPrevNewLine = bNewLine;
                //}
            }
            else
            {
                for (int i = 0; i < confusemat.GetLength(0); i++)
                {
                    for (int j = 0; j < confusemat.GetLength(1); j++)
                        str += (int)confusemat[i, j] + "\t";
                    str += "\n";
                }
            }
            str += "<End Confusion Matrix>";
            return str;
        }

    }
}
