using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.Tools
{
    public static class RoseTreeMath
    {
        public static double GetNorm(double[] array)
        {
            double sum = 0;
            double scale = 1000;

            if (array.Max() < scale)
            {
                for (int i = 0; i < array.Length; i++)
                    sum += array[i] * array[i];
                return Math.Sqrt(sum);
            }
            else
            {
                double[] scaledarray = new double[array.Length];

                for (int i = 0; i < array.Length; i++)
                    scaledarray[i] = array[i] / scale;

                for (int i = 0; i < array.Length; i++)
                    sum += scaledarray[i] * scaledarray[i];
                return Math.Sqrt(sum) * scale;
            }
        }

        public static double GetNorm(int[] array)
        {
            double sum = 0;
            double scale = 1000;

            if (array.Max() < scale)
            {
                for (int i = 0; i < array.Length; i++)
                    sum += array[i] * array[i];
                return Math.Sqrt(sum);
            }
            else
            {
                double[] scaledarray = new double[array.Length];

                for (int i = 0; i < array.Length; i++)
                    scaledarray[i] = array[i] / scale;

                for (int i = 0; i < array.Length; i++)
                    sum += scaledarray[i] * scaledarray[i];
                return Math.Sqrt(sum) * scale;
            }
        }

        public static double[] Normalize(double[] array, double normvalue)
        {
            double[] norm_array = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                norm_array[i] = array[i] / normvalue;
            return norm_array;
        }

        public static double[] Normalize(int[] array, double normvalue)
        {
            double[] norm_array = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
                norm_array[i] = array[i] / normvalue;
            return norm_array;
        }

        public static double ProjectData_Cosine(double[] normalized_data1, double[] normalized_data2, int dimension)
        {
            double cosine = 0;

            for (int i = 0; i < dimension; i++)
                cosine += normalized_data1[i] * normalized_data2[i];

            return (cosine > 1) ? 1 : cosine;
        }

        public static double ProjectData_EuclideanDist(double[] normalized_data1, double[] normalized_data2, int dimension)
        {
            double distance = 2 - 2 * ProjectData_Cosine(normalized_data1, normalized_data2, dimension);
            if (distance >= 0)
                return Math.Sqrt(distance);
            else
            {
                throw new Exception("Euclidean Distance Out of Boundary!");
            }
        }

        public static double[] ProjectData_MidPoint(double[] normalized_data1, double[] normalized_data2, int dimension)
        {
            double[] midpoint = new double[dimension];
            for (int i = 0; i < dimension; i++)
                midpoint[i] = (normalized_data1[i] + normalized_data2[i]) / 2;
            return midpoint;
        }

        public static double[] ProjectData_Minus(double[] normalized_data1, double[] normalized_data2, int dimension)
        {
            double[] difference = new double[dimension];
            for(int i = 0; i < dimension; i++)
                difference[i] = normalized_data1[i] - normalized_data2[i];
            return difference;
        }

        public static double[] ProjectData_Plus(double[] normalized_data1, double[] normalized_data2, int dimension)
        {
            double[] difference = new double[dimension];
            for (int i = 0; i < dimension; i++)
                difference[i] = normalized_data1[i] + normalized_data2[i];
            return difference;
        }

        public static double[] ProjectData_Fractional(double[] vector, double fraction, int dimension)
        {
            double[] fractional_vector = new double[dimension];
            for (int i = 0; i < dimension; i++)
                fractional_vector[i] = vector[i] * fraction;
            return fractional_vector;
        }

        public static double ProjectData_DotProd(double[] normalized_data1, double[] normalized_data2, int dimension)
        {
            double prod = 0;
            for (int i = 0; i < dimension; i++)
                prod += normalized_data1[i] * normalized_data2[i];
            return prod;
        }

        //public static int ArrayMaxOverlap(int[] sortedlist1, int[] sortedlist2)
        //{
        //    int pt1 = 0;
        //    int pt2 = 0;
        //    int overlapNum = 0;
        //    int length1 = sortedlist1.Length;
        //    int length2 = sortedlist2.Length;

        //    while (true)
        //    {
        //        while (pt1 < length1 && sortedlist1[pt1] < sortedlist2[pt2]) pt1++;
        //        if (pt1 == length1) break;
        //        if (sortedlist1[pt1] == sortedlist2[pt2])
        //        {
        //            pt1++;
        //            pt2++;
        //            overlapNum++;
        //        }
        //        else
        //        {
        //            while (pt2 < length2 && sortedlist2[pt2] < sortedlist1[pt1]) pt2++;
        //            if (pt2 == length2) break;
        //            if (sortedlist1[pt1] == sortedlist2[pt2])
        //            {
        //                pt1++;
        //                pt2++;
        //                overlapNum++;
        //            }
        //        }
        //        if (pt1 == length1 || pt2 == length2) break;
        //    }

        //    return overlapNum;
        //}

        public static void AverageVariance(List<double> list, out double avg, out double var)
        {
            avg = 0;
            var = 0;

            for (int i = 0; i < list.Count; i++)
                avg += list[i];
            avg /= list.Count;

            for (int i = 0; i < list.Count; i++)
                var += (list[i] - avg) * (list[i] - avg);
            var /= list.Count;
            var = Math.Sqrt(var);
        }

        public static void AverageVariance(List<int> list, out double avg, out double var)
        {
            avg = 0;
            var = 0;

            for (int i = 0; i < list.Count; i++)
                avg += list[i];
            avg /= list.Count;

            for (int i = 0; i < list.Count; i++)
                var += (list[i] - avg) * (list[i] - avg);
            var /= list.Count;
            var = Math.Sqrt(var);
        }

        public static int[] SubArray(int[] array, int num)
        {
            int[] subarray = new int[num];
            for (int i = 0; i < num; i++)
                subarray[i] = array[i];
            return subarray;
        }

        public static int ArrayMaxOverlap(int[] x, int[] y, int xPos, int yPos)
        {
            int xLen = x.Length;
            int yLen = y.Length;
            int xsuffixlen = xLen - 1 - xPos;
            int ysuffixlen = yLen - 1 - yPos;
            if (xsuffixlen <= 0 || ysuffixlen <= 0) return 0;
            int ptx = xPos + 1;
            int pty = yPos + 1;
            int ret = 0;

            while (true)
            {
                while (ptx < xLen && x[ptx] < y[pty]) ptx++;
                if (ptx == xLen) break;
                if (x[ptx] == y[pty])
                {
                    ret++;
                    ptx++;
                    pty++;
                }
                else
                {
                    while (pty < yLen && y[pty] < x[ptx]) pty++;
                    if (pty == yLen) break;
                    if (y[pty] == x[ptx])
                    {
                        ret++;
                        ptx++;
                        pty++;
                    }
                }
                if (ptx == xLen || pty == yLen) break;
            }
            return ret;
        }

        #region Calulate the dth largest value in the array
        //Xiting, From the book Introduction to Algorithms
        public static double GetDLargestValue(double[] A, int d)
        {
            double[] Ac = new double[A.Length];
            for (int i = 0; i < A.Length; i++)
                Ac[i] = A[i];
            return RandomizedSelect(Ac, 0, Ac.Length - 1, d);
        }

        static double RandomizedSelect(double[] A, int p, int r, int i)
        {
            if (p == r)
                return A[p];
            int q = RandomizedPartition(A, p, r);
            int k = q - p + 1;
            if (i == k)
                return A[q];
            else if (i < k)
                return RandomizedSelect(A, p, q - 1, i);
            else
                return RandomizedSelect(A, q + 1, r, i - k);
        }

        static int RandomizedPartition(double[] A, int p, int r)
        {
            //random
            double unitrand = RandomGenerator.GetUniform();
            int i = (int)Math.Floor(p + unitrand * (r - p + 1));
            if (i < p)
                throw new Exception("Error RandomizedPartition!");
            if(i > r)
                throw new Exception("Error RandomizedPartition!");

            double temp = A[i];
            A[i] = A[r];
            A[r] = temp;

            //partition
            double x = A[r];
            i = p - 1;
            for (int j = p; j < r; j++)
            {
                if (A[j] <= x)
                {
                    i++;
                    temp = A[i];
                    A[i] = A[j];
                    A[j] = temp;
                }
            }
            temp = A[i + 1];
            A[i + 1] = A[r];
            A[r] = temp;

            return i + 1;
        }
        #endregion Calulate the dth largest value in the array

    }
}
