using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.Constants;

namespace RoseTreeTaxonomy.Algorithms
{
    public class SpillTree
    {
        public int k;
        public int projectdimension;
        public double tau;
        public SpillTreeNode root;
        public int force_rebuild_spilltree = 0;

        public SpillTree(int k, int projectdimension, double tau)
        {
            this.k = k;
            this.projectdimension = projectdimension;
            this.tau = tau;
        }

        public void Build(List<RoseTreeNode> data)
        {
            this.root = new SpillTreeNode(data, Constant.NOT_A_CHILD);
            RecursiveBuild(this.root);
            LabelTreeIndices();
            MaxHeapDouble mhd = new MaxHeapDouble(this.k);
        }

        public static int OverlappingNodesNumber = 0;
        public static int OverallNodesNumber = 0;
        public void RecursiveBuild(SpillTreeNode node)
        {
            if (node.points.Count <= this.k + 1)
            {
                for (int i = 0; i < node.points.Count; i++)
                    node.points[i].spilltree_positions.Add(node);
                return;
            }

            int index = (int)(RandomGenerator.GetUint()) % node.points.Count;
            index = (index < 0) ? (index + node.points.Count) : index;
            RoseTreeNode p = node.points[index];

            double maxdistance;
            bool overlapping = true;
            int lpv_index, rpv_index;
            SearchFarestNeighbor(index, node.points[index].projectdata, node.points, out maxdistance, out lpv_index);
            SearchFarestNeighbor(lpv_index, node.points[lpv_index].projectdata, node.points, out maxdistance, out rpv_index);

            node.lpv = node.points[lpv_index].projectdata;
            node.rpv = node.points[rpv_index].projectdata;

            double[] mid_point = RoseTreeMath.ProjectData_MidPoint(node.lpv, node.rpv, this.projectdimension);

            node.center = mid_point;
            double[] half_u = RoseTreeMath.ProjectData_Minus(node.lpv, mid_point, this.projectdimension);

            if (RoseTreeMath.GetNorm(half_u) == 0) return;

            List<RoseTreeNode> leftchild_pointlist, rightchild_pointlist;
            node.half_u = half_u;

            OverlappingPartition(node, node.points, out leftchild_pointlist, out rightchild_pointlist);

            if (leftchild_pointlist.Count > 0.7 * node.points.Count || rightchild_pointlist.Count > 0.7 * node.points.Count)
            {
                overlapping = false;
                NonOverlappingPartition(node, node.points, out leftchild_pointlist, out rightchild_pointlist);
            }

            SpillTreeNode left_child = new SpillTreeNode(leftchild_pointlist, Constant.LEFT_CHILD);
            SpillTreeNode right_child = new SpillTreeNode(rightchild_pointlist, Constant.RIGHT_CHILD);

            int index2;
            node.overlapping = overlapping;
            SearchFarestNeighbor(-1, mid_point, node.points, out node.radius, out index2);
            node.left_child = left_child;
            node.right_child = right_child;
            left_child.parent = node;
            right_child.parent = node;
            node.parent = null;
            node.points = null;
            
            //test overlapping number 
            if (node.overlapping)
                OverlappingNodesNumber++;
            OverallNodesNumber++;

            RecursiveBuild(left_child);
            RecursiveBuild(right_child);
        }

        private void SearchFarestNeighbor(int index, double[] querypoint, List<RoseTreeNode> nodelist, out double maxdistance, out int farest_neighbor_index)
        {
            maxdistance = double.MinValue;
            farest_neighbor_index = -1;

            for (int i = 0; i < nodelist.Count; i++)
                if (i != index)
                {
                    double dist = RoseTreeMath.ProjectData_EuclideanDist(querypoint, nodelist[i].projectdata, this.projectdimension);
                    if (dist > maxdistance)
                    {
                        maxdistance = dist;
                        farest_neighbor_index = i;
                    }
                }
        }

        private void OverlappingPartition(SpillTreeNode node, List<RoseTreeNode> pointlist, out List<RoseTreeNode> leftchild_pointlist, out List<RoseTreeNode> rightchild_pointlist)
        {
            leftchild_pointlist = new List<RoseTreeNode>();
            rightchild_pointlist = new List<RoseTreeNode>();
            double half_u_norm = RoseTreeMath.GetNorm(node.half_u);
            double fraction1 = 1 - this.tau / half_u_norm;
            double fraction2 = 1 + this.tau / half_u_norm;

            double[] lpv_minus_left_point = RoseTreeMath.ProjectData_Fractional(node.half_u, fraction1, this.projectdimension);
            double[] lpv_minus_right_point = RoseTreeMath.ProjectData_Fractional(node.half_u, fraction2, this.projectdimension);

            double[] left_point = RoseTreeMath.ProjectData_Plus(RoseTreeMath.ProjectData_Fractional(node.rpv, 0.5 * fraction1, this.projectdimension), RoseTreeMath.ProjectData_Fractional(node.lpv, 0.5 * fraction2, this.projectdimension), this.projectdimension);
            double[] right_point = RoseTreeMath.ProjectData_Plus(RoseTreeMath.ProjectData_Fractional(node.rpv, 0.5 * fraction2, this.projectdimension), RoseTreeMath.ProjectData_Fractional(node.lpv, 0.5 * fraction1, this.projectdimension), this.projectdimension);

            for (int i = 0; i < pointlist.Count; i++)
            {
                double[] point_minus_left_point = RoseTreeMath.ProjectData_Minus(pointlist[i].projectdata, left_point, this.projectdimension);
                double[] point_minus_right_point = RoseTreeMath.ProjectData_Minus(pointlist[i].projectdata, right_point, this.projectdimension);

                double left_prod = RoseTreeMath.ProjectData_DotProd(lpv_minus_left_point, point_minus_left_point, this.projectdimension);
                double right_prod = RoseTreeMath.ProjectData_DotProd(lpv_minus_right_point, point_minus_right_point, this.projectdimension);

                if (right_prod > 0)
                    leftchild_pointlist.Add(pointlist[i]);
                if (left_prod < 0)
                    rightchild_pointlist.Add(pointlist[i]);
            }
        }

        private void NonOverlappingPartition(SpillTreeNode node, List<RoseTreeNode> pointlist, out List<RoseTreeNode> leftchild_pointlist, out List<RoseTreeNode> rightchild_pointlist)
        {
            leftchild_pointlist = new List<RoseTreeNode>();
            rightchild_pointlist = new List<RoseTreeNode>();

            for (int i = 0; i < pointlist.Count; i++)
            {
                double[] point_minus_mid_point = RoseTreeMath.ProjectData_Minus(pointlist[i].projectdata, node.center, this.projectdimension);

                double mid_prod = RoseTreeMath.ProjectData_DotProd(node.half_u, point_minus_mid_point, this.projectdimension);

                if (mid_prod > 0)
                    leftchild_pointlist.Add(pointlist[i]);
                else if (mid_prod < 0)
                    rightchild_pointlist.Add(pointlist[i]);
                else
                    leftchild_pointlist.Add(pointlist[i]);
            }
        }

        public void Insert(RoseTreeNode newpoint)
        {
            RecursiveInsert(newpoint, this.root);
        }

        public void RecursiveInsert(RoseTreeNode newpoint, SpillTreeNode node)
        {
            if (node.left_child == null && node.right_child == null)
            {
                node.points.Add(newpoint);
                newpoint.spilltree_positions.Add(node);
                return;
            }
            else
            {
                List<RoseTreeNode> newpoint_list = new List<RoseTreeNode>(), leftpoint_list, rightpoint_list;
                newpoint_list.Add(newpoint);

                if (node.overlapping == true)
                    OverlappingPartition(node, newpoint_list, out leftpoint_list, out rightpoint_list);
                else
                    NonOverlappingPartition(node, newpoint_list, out leftpoint_list, out rightpoint_list);

                if (leftpoint_list.Count == 1)
                    RecursiveInsert(newpoint, node.left_child);
                if (rightpoint_list.Count == 1)
                    RecursiveInsert(newpoint, node.right_child);
            }
        }

        public int[] Search(RoseTreeNode query, out bool force_brute_force_search, out int search_neighbor_num)
        {
            MaxHeapDouble mhd = new MaxHeapDouble(this.k);
            search_neighbor_num = 0;
            for (int i = 0; i < this.k; i++)
                mhd.insert(-1, double.MaxValue);

            List<SpillTreeNode> contain_query_nodelist = new List<SpillTreeNode>();
            RecursiveSearch(query, this.root, mhd, contain_query_nodelist, out search_neighbor_num);

            if (NonZeroItemNum(mhd) < this.k)
            {
                for (int i = 0; i < query.spilltree_positions.Count; i++)
                    if (contain_query_nodelist.Contains(query.spilltree_positions[i]) == false)
                    {
                        bool contain_query;
                        int search_neighbor_num_singlenode;
                        SearchSingleNode(query, query.spilltree_positions[i], mhd, out contain_query, out search_neighbor_num_singlenode);
                        search_neighbor_num += search_neighbor_num_singlenode;
                    }
            }

            MaxHeapDouble.heapSort(mhd);

            if (NonZeroItemNum(mhd) == 0)
            {
                force_rebuild_spilltree++;
                force_brute_force_search = true;//BruteForceSearch(query, mhd);
            }
            else
                force_brute_force_search = false;

            int[] nearestneighbor_indices = mhd.getIndices();
            List<int> nearestneighbor_list = new List<int>();
            for (int i = 0; i < this.k; i++)
                if (nearestneighbor_indices[i] != -1 && nearestneighbor_list.Contains(nearestneighbor_indices[i]) == false)
                    nearestneighbor_list.Add(nearestneighbor_indices[i]);

            return nearestneighbor_list.ToArray();
        }

        public void RecursiveSearch(RoseTreeNode query, SpillTreeNode node, MaxHeapDouble mhd, List<SpillTreeNode> contain_query_nodelist, out int search_neighbor_num)
        {
            if (node.left_child == null && node.right_child == null)
            {
                bool contain_query;
                SearchSingleNode(query, node, mhd, out contain_query, out search_neighbor_num);
                if (contain_query == true)
                    contain_query_nodelist.Add(node);
            }
            else
            {
                double[] query_minus_node_center = RoseTreeMath.ProjectData_Minus(query.projectdata, node.center, this.projectdimension);
                double prod = RoseTreeMath.ProjectData_DotProd(query_minus_node_center, node.half_u, this.projectdimension);

                if (RoseTreeMath.ProjectData_EuclideanDist(node.center, query.projectdata, this.projectdimension) - node.radius < mhd.max())
                {
                    if (node.overlapping == true)
                    {
                        if (prod >= 0)
                            RecursiveSearch(query, node.left_child, mhd, contain_query_nodelist, out search_neighbor_num);
                        else
                            RecursiveSearch(query, node.right_child, mhd, contain_query_nodelist, out search_neighbor_num);
                    }
                    else
                    {
                        int search_neighbor_num_left, search_neighbor_num_right;
                        if (prod >= 0)
                        {
                            RecursiveSearch(query, node.left_child, mhd, contain_query_nodelist, out search_neighbor_num_left);
                            RecursiveSearch(query, node.right_child, mhd, contain_query_nodelist, out search_neighbor_num_right);
                        }
                        else
                        {
                            RecursiveSearch(query, node.right_child, mhd, contain_query_nodelist, out search_neighbor_num_right);
                            RecursiveSearch(query, node.left_child, mhd, contain_query_nodelist, out search_neighbor_num_left);
                        }
                        search_neighbor_num = search_neighbor_num_left + search_neighbor_num_right;
                    }
                }
                else
                    search_neighbor_num = 0;
            }
        }

        public void SearchSingleNode(RoseTreeNode query, SpillTreeNode node, MaxHeapDouble mhd, out bool contain_query, out int search_neighbor_num)
        {
            contain_query = false;
            search_neighbor_num = 0;

            for (int i = 0; i < node.points.Count; i++)
                if (node.points[i] != null && node.points[i].valid == true)
                {
                    if (node.points[i].indices.array_index != query.indices.array_index)
                    {
                        double dist = RoseTreeMath.ProjectData_EuclideanDist(query.projectdata, node.points[i].projectdata, this.projectdimension);

                        if (dist < mhd.max() || (dist == mhd.max() && node.points[i].indices.array_index < mhd.getIndices()[0]))
                            mhd.changeMax(node.points[i].indices.array_index, dist);
                        search_neighbor_num++;
                    }
                    else
                        contain_query = true;
                }
        }

        public int NonZeroItemNum(MaxHeapDouble mhd)
        {
            int[] indices = mhd.getIndices();
            int nonzeronum = 0;

            for (int i = 0; i < indices.Length; i++)
                if (indices[i] != -1)
                    nonzeronum++;
            return nonzeronum;
        }

        public void BruteForceSearch(RoseTreeNode query, MaxHeapDouble mhd)
        {
            List<SpillTreeNode> nodelist = new List<SpillTreeNode>();
            nodelist.Add(this.root);
            bool contain_query;
            int search_neighbor_num;

            while (nodelist.Count != 0)
            {
                int nodelist_count = nodelist.Count;
                for (int i = 0; i < nodelist_count; i++)
                {
                    SpillTreeNode node = nodelist[0];

                    if (node.left_child != null)
                        nodelist.Add(node.left_child);
                    if (node.right_child != null)
                        nodelist.Add(node.right_child);

                    if (node.points != null)
                        SearchSingleNode(query, node, mhd, out contain_query, out search_neighbor_num);

                    nodelist.RemoveAt(0);
                }
            }
        }

        public void LabelTreeIndices()
        {
            List<SpillTreeNode> nodelist = new List<SpillTreeNode>();
            nodelist.Add(this.root);
            int tree_index = 0;

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;
                for (int i = 0; i < nodelistcount; i++)
                {
                    SpillTreeNode node = nodelist[0];
                    node.tree_index = tree_index;
                    tree_index++;

                    if (node.left_child != null)
                        nodelist.Add(node.left_child);
                    if (node.right_child != null)
                        nodelist.Add(node.right_child);

                    nodelist.RemoveAt(0);
                }
            }
        }
    }
}
