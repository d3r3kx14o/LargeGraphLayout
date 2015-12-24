using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Tools;
namespace RoseTreeTaxonomy.DataStructures
{
    public class SpillTreeNode
    {
        public List<RoseTreeNode> points = new List<RoseTreeNode>();       
        public double radius;

        public SpillTreeNode parent;
        public SpillTreeNode left_child = null;
        public SpillTreeNode right_child = null;

        public double[] center;
        public double[] lpv;
        public double[] rpv;
        public double[] half_u;

        public int tree_index;
        public bool overlapping;
        public int left_or_right_son_of_parent = -1;

        public double lcut_projlen;
        public double rcut_projlen;
        public double center_projlen;

        public SpillTreeNode()
        {
        }

        public SpillTreeNode(List<RoseTreeNode> points, int left_or_right_son_of_parent)
        {
            this.points = points;
            this.left_or_right_son_of_parent = left_or_right_son_of_parent;
        }

        public void InvalidatePoint(RoseTreeNode node)
        {
            for(int i = 0; i < points.Count; i++)
                if (points[i] != null && points[i].Equals(node) == true)
                {
                    points[i] = null;
                    break;
                }
        }

        public void InsertNode(RoseTreeNode node)
        {
            if (left_child != null)
            {
                double distance = RoseTreeMath.ProjectData_EuclideanDist(this.center, node.projectdata, this.center.Length);
                if (distance > radius)
                    radius = distance;
            }
            else
            {
                this.points.Add(node);
            }
        }
    }
}
