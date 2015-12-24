using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Accuracy
{
    class Purity
    {
        public static double GetPurity(int[] label, int[] groundtruthlabel)
        {
            double[,] confuseMatrix = ConfusionMatrix.GetConfuseMatrix(groundtruthlabel, label);
            return GetPurity(confuseMatrix, label.Length);
        }

        public static double GetPurity(double[,] confuseMatrix, double N)
        {
            double[] maxv = new double[confuseMatrix.GetLength(1)];
            for (int i = 0; i < confuseMatrix.GetLength(1); i++)
            {
                double max = double.MinValue;
                for (int j = 0; j < confuseMatrix.GetLength(0); j++)
                    if (confuseMatrix[j, i] > max)
                        max = confuseMatrix[j, i];
                maxv[i] = max;
            }

            double maxsum = 0;
            for (int i = 0; i < maxv.Length; i++)
                maxsum += maxv[i];

            return maxsum / N;
        }
    }
}
