using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Accuracy
{
    class ARI
    {
        public static double GetAdjustedRandIndex(int[] label1, int[] label2)
        {
            double[,] confuseMatrix = ConfusionMatrix.GetConfuseMatrix(label1, label2);
            return CalculateARI(confuseMatrix, label1.Length);
        }

        private static double CalculateARI(double[,] confuseMatrix, double N)
        {
            int dim1 = confuseMatrix.GetLength(0);
            int dim2 = confuseMatrix.GetLength(1);

            //intialize nk & ny
            double[] nk = new double[dim2];   //sum(cmat, 1)
            double[] ny = new double[dim1];   //sum(cmat, 2)

            for (int i = 0; i < dim2; i++)
            {
                double sum = 0;
                for (int j = 0; j < dim1; j++)
                    sum += confuseMatrix[j, i];
                nk[i] = sum;
            }

            for (int i = 0; i < dim1; i++)
            {
                double sum = 0;
                for (int j = 0; j < dim2; j++)
                    sum += confuseMatrix[i, j];
                ny[i] = sum;
            }

            //calculate ARI
            double nij2 = 0;
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    nij2 += Nsel2(confuseMatrix[i, j]);
                }
            }
            double ny2 = 0;
            for (int i = 0; i < ny.Length; i++)
            {
                ny2 += Nsel2(ny[i]);
            }
            double nk2 = 0;
            for (int i = 0; i < nk.Length; i++)
            {
                nk2 += Nsel2(nk[i]);
            }
            double e = (ny2 * nk2) / Nsel2(N);
            return (nij2 - e) / (0.5 * (ny2 + nk2) - e);

        }

        private static double Nsel2(double nij)
        {
            if (nij < 2)
                return 0;
            return nij * (nij - 1) / 2.0;
        }
    }
}
