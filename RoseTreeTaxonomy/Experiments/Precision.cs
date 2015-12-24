using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.DataStructures;
using System.IO;

namespace RoseTreeTaxonomy.Experiments
{
    public class Precision
    {
        public int samplenum = 20;//100;
        public int[] sample_array;
        public int[] neighbors_nums = { 1, 5, 10 };
        public List<double>[] precision_array;
        public List<int>[] search_neighbors_num_array;
        public List<double>[] search_time_array;

        //
        public void SpillTreePrecision(SpillTree spilltree, RoseTreeNode[] nodearray, out double[] precision_avg, out double[] precision_var, out double[] search_neighbor_num_avg, out double[] search_neighbor_num_var, out double[] search_time_avg, out double[] search_time_var, Dictionary<int, int[]> relevant_nearest_neighbor_array)
        {
            this.precision_array = new List<double>[this.neighbors_nums.Length];
            this.search_neighbors_num_array = new List<int>[this.neighbors_nums.Length];
            this.search_time_array = new List<double>[this.neighbors_nums.Length];

            for (int i = 0; i < this.neighbors_nums.Length; i++)
            {
                this.precision_array[i] = new List<double>();
                this.search_neighbors_num_array[i] = new List<int>();
                this.search_time_array[i] = new List<double>();
            }

            int pt = 0;
            for (int i = 0; i < nodearray.Length; i++)
                if (i == sample_array[pt])
                {
                    Console.WriteLine("Testing precision of the " + pt + "th sample");
                    RoseTreeNode query = nodearray[i];
                    int[] label_indices = relevant_nearest_neighbor_array[i];
                    for (int j = 0; j < this.neighbors_nums.Length; j++)
                    {
                        bool brute_force_search;
                        int search_neighbor_num;
                        spilltree.k = this.neighbors_nums[j];
                        DateTime before = DateTime.Now;
                        int[] retrieve_indices = spilltree.Search(query, out brute_force_search, out search_neighbor_num);

                        DateTime after = DateTime.Now;
                        double precision = IR(RoseTreeMath.SubArray(label_indices, retrieve_indices.Length), retrieve_indices);

                        this.precision_array[j].Add(precision);
                        this.search_neighbors_num_array[j].Add(search_neighbor_num);
                        this.search_time_array[j].Add((after - before).Ticks);
                    }
                    pt++;
                    if (pt >= sample_array.Length) break;
                }

            precision_avg = new double[this.neighbors_nums.Length];
            precision_var = new double[this.neighbors_nums.Length];
            search_neighbor_num_avg = new double[this.neighbors_nums.Length];
            search_neighbor_num_var = new double[this.neighbors_nums.Length];
            search_time_avg = new double[this.neighbors_nums.Length];
            search_time_var = new double[this.neighbors_nums.Length];

            for (int i = 0; i < this.neighbors_nums.Length; i++)
            {
                RoseTreeMath.AverageVariance(this.precision_array[i], out precision_avg[i], out precision_var[i]);
                RoseTreeMath.AverageVariance(this.search_neighbors_num_array[i], out search_neighbor_num_avg[i], out search_neighbor_num_var[i]);
                RoseTreeMath.AverageVariance(this.search_time_array[i], out search_time_avg[i], out search_time_var[i]);
            }
        }

        public void RoseTreePrecision(RoseTree rosetree, out double[] precision_avg, out double[] precision_var, out double[] search_neighbor_num_avg, out double[] search_neighbor_num_var, out double[] search_time_avg, out double[] search_time_var, Dictionary<int, int[]> relevant_nearest_neighbor_array)
        {
            this.precision_array = new List<double>[this.neighbors_nums.Length];
            this.search_neighbors_num_array = new List<int>[this.neighbors_nums.Length];
            this.search_time_array = new List<double>[this.neighbors_nums.Length];

            for (int i = 0; i < this.neighbors_nums.Length; i++)
            {
                this.precision_array[i] = new List<double>();
                this.search_neighbors_num_array[i] = new List<int>();
                this.search_time_array[i] = new List<double>();
            }

            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(rosetree.root);
            int depth = 0;

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;

                for (int i = 0; i < nodelistcount; i++)
                {
                    RoseTreeNode node = nodelist[0];

                    if (node.children != null)
                        for (int j = 0; j < node.children.Length; j++)
                            nodelist.Add(node.children[j]);
                    else
                        TestRoseTreePrecision(node, rosetree, relevant_nearest_neighbor_array);

                    nodelist.RemoveAt(0);
                }
                depth++;
                Console.WriteLine("Precision Test in the " + depth + "th depth of the tree");
            }

            precision_avg = new double[this.neighbors_nums.Length];
            precision_var = new double[this.neighbors_nums.Length];
            search_neighbor_num_avg = new double[this.neighbors_nums.Length];
            search_neighbor_num_var = new double[this.neighbors_nums.Length];
            search_time_avg = new double[this.neighbors_nums.Length];
            search_time_var = new double[this.neighbors_nums.Length];

            for (int i = 0; i < this.neighbors_nums.Length; i++)
            {
                RoseTreeMath.AverageVariance(this.precision_array[i], out precision_avg[i], out precision_var[i]);
                RoseTreeMath.AverageVariance(this.search_neighbors_num_array[i], out search_neighbor_num_avg[i], out search_neighbor_num_var[i]);
                RoseTreeMath.AverageVariance(this.search_time_array[i], out search_time_avg[i], out search_time_var[i]);
            }
        }

        public void TestRoseTreePrecision(RoseTreeNode node, RoseTree rosetree, Dictionary<int, int[]> relevant_nearest_neighbor_array)
        {
            if (this.sample_array.Contains(node.indices.initial_index) == true)
            {
                int actual_search_neighbors_num;
                int[] label_indices = relevant_nearest_neighbor_array[node.indices.initial_index];//rosetree.SearchTreeNeighbors(node, this.neighbors_nums[this.neighbors_nums.Length - 1], rosetree.initial_clusternum, out actual_search_neighbors_num);
                for (int i = 0; i < this.neighbors_nums.Length; i++)
                {
                    int neighbors_num = (this.neighbors_nums[i] >= node.parent.children.Length - 1) ? node.parent.children.Length - 1 : this.neighbors_nums[i];
                    DateTime before = DateTime.Now;
                    int[] retrieve_indices = rosetree.SearchTreeNeighbors(node, neighbors_num, 3 * neighbors_num/** neighbors_nums[i]*/, out actual_search_neighbors_num);
                    DateTime after = DateTime.Now;
                    double precision = IR(RoseTreeMath.SubArray(label_indices, neighbors_num), retrieve_indices);

                    this.precision_array[i].Add(precision);
                    this.search_neighbors_num_array[i].Add(actual_search_neighbors_num);
                    this.search_time_array[i].Add((after - before).Ticks);
                }
            }
        }

        public double IR(int[] relevant, int[] retrieve)
        {
            Array.Sort(relevant);
            Array.Sort(retrieve);

            int overlap = RoseTreeMath.ArrayMaxOverlap(relevant, retrieve, -1, -1);
            return (double)overlap / retrieve.Length;
        }
    }
}
