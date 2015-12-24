using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Smoothness
{
    /// <summary>
    ///  This function is implemented from MATLAB code bghungar1.m written by Alex Melin 30 June 2006
    ///  provided by Yangqiu
    ///  Perf: a M*N Edge weight matrix 
    /// </summary>
    class HungarianMatching
    {
        public static double GetMinimumWeightMatchingCost(double[,] Perf)
        {
            int m = Perf.GetLength(0);
            int n = Perf.GetLength(1);

            int[] num_y = new int[n];
            int[] num_x = new int[m];
            /// Find the number in each column that are connected ///
            for (int j = 0; j < n; j++)
            {
                num_y[j] = 0;
                for (int i = 0; i < m; i++)
                    if (Perf[i, j] != double.MaxValue)
                        num_y[j]++;
            }
            /// Find the number in each row that are connected ///
            for (int i = 0; i < m; i++)
            {
                num_x[i] = 0;
                for (int j = 0; j < n; j++)
                    if (Perf[i, j] != double.MaxValue)
                        num_x[i]++;
            }

            /// Find the columns(vertices) and rows(vertices) that are isolated ///
            int[] x_con, y_con;
            {
                List<int> x_con_list = new List<int>();
                List<int> y_con_list = new List<int>();
                for (int i = 0; i < m; i++)
                    if (num_x[i] != 0)
                        x_con_list.Add(i);
                for (int j = 0; j < n; j++)
                    if (num_y[j] != 0)
                        y_con_list.Add(j);
                x_con = x_con_list.ToArray();
                y_con = y_con_list.ToArray();
            }

            /// Assemble Condensed Performance Matrix ///
            if (x_con.Length == 0 || y_con.Length == 0) return 0;
            int P_size = Math.Max(x_con.Length, y_con.Length);
            double[,] P_cond = new double[P_size, P_size];
            for (int i = 0; i < x_con.Length; i++)
            {
                int x_con_i = x_con[i];
                for (int j = 0; j < y_con.Length; j++)
                    P_cond[i, j] = Perf[x_con_i, y_con[j]];
            }

            /// Ensure that a perfect matching exists ///
            // Calculate a form of the Edge Matrix //
            double[,] Edge = new double[P_size, P_size];
            for (int i = 0; i < P_size; i++)
                for (int j = 0; j < P_size; j++)
                    Edge[i, j] = P_cond[i, j] == double.MaxValue ? double.MaxValue : 0;
            // Find the deficiency(CNUM) in the Edge Matrix //
            int cnum = min_line_cover(Edge);

            /// Project additional vertices and edges so that a perfect matching exists ///
            double Pmax = double.MinValue;
            for (int i = 0; i < P_size; i++)
                for (int j = 0; j < P_size; j++)
                    if (P_cond[i, j] != double.MaxValue &&
                        P_cond[i, j] > Pmax)
                        Pmax = P_cond[i, j];
            P_size = P_cond.GetLength(0) + cnum;
            P_cond = new double[P_size, P_size];
            for (int i = 0; i < P_size; i++)
                for (int j = 0; j < P_size; j++)
                    P_cond[i, j] = Pmax;
            for (int i = 0; i < x_con.Length; i++)
            {
                int x_con_i = x_con[i];
                for (int j = 0; j < y_con.Length; j++)
                    P_cond[i, j] = Perf[x_con_i, y_con[j]];
            }



            //*************************************************
            // MAIN PROGRAM: CONTROLS WHICH STEP IS EXECUTED
            //*************************************************
            bool exit_flag = true;
            int stepnum = 1;
            int[] r_cov = null, c_cov = null;
            List<int> Z_r = null, Z_c = null;
            int[,] M = null;
            while (exit_flag)
            {
                switch (stepnum)
                {
                    case 1:
                        step1(P_cond, out stepnum);
                        break;
                    case 2:
                        step2(P_cond, out r_cov, out c_cov, out M, out stepnum);
                        break;
                    case 3:
                        step3(M, P_size, out c_cov, out stepnum);
                        break;
                    case 4:
                        step4(P_cond, r_cov, c_cov, M, out Z_r, out Z_c, out stepnum);
                        break;
                    case 5:
                        step5(M, Z_r, Z_c, r_cov, c_cov, out stepnum);
                        break;
                    case 6:
                        step6(P_cond, r_cov, c_cov, out stepnum);
                        break;
                    case 7:
                        exit_flag = false;
                        break;
                }
            }

            /// Remove all the virtual satellites and targets and uncondense the
            /// Matching to the size of the original performance matrix.
            bool[,] Matching = new bool[m, n];
            for(int i=0;i<x_con.Length;i++)
                for (int j = 0; j < y_con.Length; j++)
                    if (M[i, j] == 1)
                        Matching[x_con[i], y_con[j]] = true;
            double Cost = 0;
            for(int i=0;i<m;i++)
                for (int j = 0; j < n; j++)
                {
                    if (Matching[i, j])
                        Cost += Perf[i, j];
                }

            return Cost;
        }

        #region steps
        //********************************************************  
        //   STEP 1: Find the smallest number of zeros in each row
        //           and subtract that minimum from its row
        //********************************************************

        private static void step1(double[,] P_cond, out int stepnum)
        {
            int P_size = P_cond.GetLength(0);
            
            // Loop throught each row
            for (int i = 0; i < P_size; i++)
            {
                double rmin = double.MaxValue;
                for (int j = 0; j < P_size; j++)
                {
                    if (P_cond[i, j] < rmin)
                        rmin = P_cond[i, j];
                }
                for (int j = 0; j < P_size; j++)
                    P_cond[i, j] -= rmin;
            }

            stepnum = 2;
        }

        //**************************************************************************  
        //   STEP 2: Find a zero in P_cond. If there are no starred zeros in its
        //           column or row start the zero. Repeat for each zero
        //**************************************************************************
        private static void step2(double[,] P_cond,
            out int[] r_cov, out int[] c_cov, out int[,] M, out int stepnum)
        {
            int P_size = P_cond.GetLength(0);
            r_cov = new int[P_size];
            c_cov = new int[P_size];
            M = new int[P_size, P_size];

            for (int i = 0; i < P_size; i++)
                for (int j = 0; j < P_size; j++)
                    if (P_cond[i, j] == 0 && r_cov[i] == 0 && c_cov[j] == 0)
                    {
                        M[i, j] = 1;
                        r_cov[i] = 1;
                        c_cov[j] = 1;
                    }

            // Re-initialize the cover vectors
            r_cov = new int[P_size];
            c_cov = new int[P_size];
            stepnum = 3;
        }

          
        //**************************************************************************
        //   STEP 3: Cover each column with a starred zero. If all the columns are
        //           covered then the matching is maximum
        //**************************************************************************
        private static void step3(int[,] M, int P_size,
            out int[] c_cov, out int stepnum)
        {
            c_cov = new int[M.GetLength(1)];
            for (int j = 0; j < M.GetLength(1); j++)
                for (int i = 0; i < M.GetLength(0); i++)
                    c_cov[j] += M[i, j];

            int c_cov_sum = 0;
            for (int i = 0; i < c_cov.Length; i++)
                c_cov_sum += c_cov[i];
            if (c_cov_sum == P_size)
                stepnum = 7;
            else
                stepnum = 4;
        }

          
        //**************************************************************************
        //   STEP 4: Find a noncovered zero and prime it.  If there is no starred
        //           zero in the row containing this primed zero, Go to Step 5.  
        //           Otherwise, cover this row and uncover the column containing 
        //           the starred zero. Continue in this manner until there are no 
        //           uncovered zeros left. Save the smallest uncovered value and 
        //           Go to Step 6.
        //**************************************************************************
        private static void step4(double[,] P_cond, int[] r_cov, int[] c_cov, int[,] M,
            out List<int> Z_r, out List<int> Z_c, out int stepnum)
        {
            stepnum = -1;
            Z_r = new List<int>();
            Z_c = new List<int>();
            Z_r.Add(-1);
            Z_c.Add(-1);

            int P_size = P_cond.GetLength(0);

            bool zflag = true;
            while (zflag)
            {
                // Find the first uncovered zero //
                int row = -1, col = -1;
                bool exit_flag = true;
                int i = 0, j = 0;
                while (exit_flag)
                {
                    if (P_cond[i, j] == 0 && r_cov[i] == 0 && c_cov[j] == 0)
                    {
                        row = i;
                        col = j;
                        exit_flag = false;
                    }
                    j++;
                    if (j == P_size)
                    {
                        j = 0;
                        i++;
                    }
                    if (i == P_size)
                        exit_flag = false;
                }

                if (row == -1)
                {
                    // If there are no uncovered zeros go to step 6 //
                    stepnum = 6;
                    zflag = false;
                    Z_r[0] = Z_c[0] = 0;
                }
                else
                {
                    // Prime the uncovered zero //
                    M[row, col] = 2;
                    // If there is a starred zero in that row //
                    // Cover the row and uncover the column containing the zero //
                    List<int> find = new List<int>();
                    for (int jj = 0; jj < M.GetLength(1); jj++)
                    {
                        if (M[row, jj] == 1)
                            find.Add(jj);
                    }

                    if (find.Count != 0)
                    {
                        r_cov[row] = 1;
                        foreach (int findi in find)
                            c_cov[findi] = 0;
                    }
                    else
                    {
                        stepnum = 5;
                        zflag = false;
                        Z_r[0] = row;
                        Z_c[0] = col;
                    }
                }

            }

        }

        //**************************************************************************
        // STEP 5: Construct a series of alternating primed and starred zeros as
        //         follows.  Let Z0 represent the uncovered primed zero found in Step 4.
        //         Let Z1 denote the starred zero in the column of Z0 (if any). 
        //         Let Z2 denote the primed zero in the row of Z1 (there will always
        //         be one).  Continue until the series terminates at a primed zero
        //         that has no starred zero in its column.  Unstar each starred 
        //         zero of the series, star each primed zero of the series, erase 
        //         all primes and uncover every line in the matrix.  Return to Step 3.
        //**************************************************************************
        private static void step5(int[,] M, List<int> Z_r, List<int> Z_c, int[] r_cov, int[] c_cov,
            out int stepnum)
        {
            bool zflag = true;
            int ii = 0;
            while (zflag)
            {
                // Find the index number of the starred zero in the column //
                int rindex = -1;
                int z_c_ii = Z_c[ii];
                for (int i = 0; i < M.GetLength(0); i++)
                    if (M[i, z_c_ii] == 1)
                    {
                        rindex = i;
                        break;
                    }
                if (rindex >= 0)
                {
                    // Save the starred zero
                    ii++;
                    // Save the row of the starred zero
                    if (Z_r.Count != ii || Z_c.Count != ii)
                        throw new Exception("");
                    Z_r.Add(rindex);
                    // The column of the starred zero is the same as the column of the 
                    // primed zero
                    Z_c.Add(Z_c[ii - 1]);
                }
                else
                    zflag = false;

                // Continue if there is a starred zero in the column of the primed zero
                if (zflag)
                {
                    // Find the column of the primed zero in the last starred zeros row
                    int cindex = -1;
                    int z_r_ii = Z_r[ii];
                    for (int j = 0; j < M.GetLength(1); j++)
                        if (M[z_r_ii, j] == 2)
                        {
                            cindex = j;
                            break;
                        }
                    ii++;
                    if (Z_r.Count != ii || Z_c.Count != ii)
                        throw new Exception("");
                    Z_r.Add(Z_r[ii - 1]);
                    Z_c.Add(cindex);
                }
            }

            // UNSTAR all the starred zeros in the path and STAR all primed zeros //
            for (int i = 0; i < Z_r.Count; i++)
                if (M[Z_r[i], Z_c[i]] == 1)
                    M[Z_r[i], Z_c[i]] = 0;
                else
                    M[Z_r[i], Z_c[i]] = 1;

            // Clear the covers
            for (int i = 0; i < r_cov.Length; i++)
                r_cov[i] = 0;
            for (int i = 0; i < c_cov.Length; i++)
                c_cov[i] = 0;

            // Remove all the primes
            for (int i = 0; i < M.GetLength(0); i++)
                for (int j = 0; j < M.GetLength(1); j++)
                    if (M[i, j] == 2)
                        M[i, j] = 0;

            stepnum = 3;
        }

        //**************************************************************************
        // STEP 6: Add the minimum uncovered value to every element of each covered
        //         row, and subtract it from every element of each uncovered column.  
        //         Return to Step 4 without altering any stars, primes, or covered lines.
        //**************************************************************************

        private static void step6(double[,] P_cond, int[] r_cov, int[] c_cov,
            out int stepnum)
        {
            List<int> a = new List<int>();  // find(r_cov == 0)
            List<int> b = new List<int>();  // find(c_cov == 0)
            List<int> a1 = new List<int>(); // find(r_cov == 1)
            for (int i = 0; i < r_cov.Length; i++)
                if (r_cov[i] == 0)
                    a.Add(i);
                else if (r_cov[i] == 1)
                    a1.Add(i);
            for (int j = 0; j < c_cov.Length; j++)
                if (c_cov[j] == 0)
                    b.Add(j);
            double minval = double.MaxValue;
            foreach(int ai in a)
                foreach (int bj in b)
                    if (P_cond[ai, bj] < minval)
                        minval = P_cond[ai, bj];

            foreach (int a1i in a1)
                for (int j = 0; j < P_cond.GetLength(1); j++)
                    P_cond[a1i, j] += minval;
            foreach (int bj in b)
                for (int i = 0; i < P_cond.GetLength(0); i++)
                    P_cond[i, bj] -= minval;

            stepnum = 4;
        }


        #endregion steps


        private static int min_line_cover(double[,] Edge)
        {
            int[] r_cov, c_cov;
            List<int> Z_r, Z_c;
            int[,] M;
            int stepnum;
            //step 2
            step2(Edge, out r_cov, out c_cov, out M, out stepnum);
            //step 3
            step3(M, Edge.GetLength(0), out c_cov, out stepnum);
            //step 4
            step4(Edge, r_cov, c_cov, M, out Z_r, out Z_c, out stepnum);

            int r_cov_sum = 0, c_cov_sum = 0;
            for (int i = 0; i < r_cov.Length; i++)
                r_cov_sum += r_cov[i];
            for (int i = 0; i < c_cov.Length; i++)
                c_cov_sum += c_cov[i];

            return Edge.GetLength(0) - r_cov_sum - c_cov_sum;
        }
    }
}
