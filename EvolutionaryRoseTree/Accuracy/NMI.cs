using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Accuracy
{
    class NMI
    {
        public static double GetNormalizedMutualInfo(int[] label1, int[] label2)
        {
            double[,] confuseMatrix = ConfusionMatrix.GetConfuseMatrix(label1, label2);
            return GetNormalizedMutualInfo(confuseMatrix, label1.Length);
        }

        public static double GetNormalizedMutualInfo(double[,] confuseMatrix, double N)
        {
            //Console.WriteLine(ConfusionMatrix.ToString(confuseMatrix));

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

            double Iky = 0;//mutual information I(Y,K)
		    double EPS = 1e-19;
    		for(int i = 0; i < ny.Length; i++){
	    		for(int j = 0; j < nk.Length; j++){
                    if (confuseMatrix[i,j] > EPS)
                    {// =0
                        Iky += confuseMatrix[i, j] * Math.Log(N * confuseMatrix[i,j] / 
                            (nk[j] * ny[i]));
			    	}
			    }
		    }

		    double Hk = 0;
		    for(int i = 0; i < nk.Length; i++){
			    if(nk[i] > EPS)
				    Hk += - nk[i] * Math.Log(nk[i]/N);
		    }
		    double Hc = 0;
		    for(int i = 0; i < ny.Length; i++){
			    if(ny[i] > EPS)
				    Hc += - ny[i] * Math.Log(ny[i]/N);
		    }

            //Console.WriteLine("I(X,Y)\t" + Iky);
            //Console.WriteLine("H(X)\t" + Hk);
            //Console.WriteLine("H(Y)\t" + Hc);
            //Console.WriteLine("I(X,Y)/sqrt(H(Y))\t" + Iky/Math.Sqrt(Hc));

            if (Hk != 0 && Hc != 0)
                return Iky / Math.Sqrt(Hk * Hc);
            else
                return 2 * Iky / (Hk + Hc);
        }

        public static double GetNormalizedMutualInfo_(double[,] confuseMatrix, double N)
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

            double Iky = 0;//mutual information I(Y,K)
            double EPS = 1e-19;
            for (int i = 0; i < ny.Length; i++)
            {
                for (int j = 0; j < nk.Length; j++)
                {
                    if (confuseMatrix[i, j] > EPS)
                    {// =0
                        Iky += confuseMatrix[i, j] / N * Math.Log(N * confuseMatrix[i, j] /
                            (nk[j] * ny[i]));
                    }
                }
            }

            double Hk = 0;
            for (int i = 0; i < nk.Length; i++)
            {
                if (nk[i] > EPS)
                    Hk += -nk[i] / N * Math.Log(nk[i] / N);
            }
            double Hc = 0;
            for (int i = 0; i < ny.Length; i++)
            {
                if (ny[i] > EPS)
                    Hc += -ny[i] / N * Math.Log(ny[i] / N);
            }

            Console.WriteLine("I(X,Y)\t" + Iky);
            Console.WriteLine("H(X)\t" + Hk);
            Console.WriteLine("H(Y)\t" + Hc);
            Console.WriteLine("I(X,Y)/sqrt(H(Y))\t" + Iky / Math.Sqrt(Hc));

            return 2 * Iky / (Hk + Hc);
        }

    }
}
