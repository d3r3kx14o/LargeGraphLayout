using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.Algorithms;
using System.IO;
using RoseTreeTaxonomy.DataStructures;

namespace RoseTreeTaxonomy.DrawTree
{
    public class DrawSpillTree
    {
        SpillTree spilltree;
        StreamWriter drawtree;
        string drawpath;

        public DrawSpillTree(SpillTree spilltree, string drawpath)
        {
            this.spilltree = spilltree;
            this.drawpath = drawpath;
        }

        public void Run()
        {
            DrawTree();
        }

        public void DrawTree()
        {
            drawtree = new StreamWriter(drawpath + "drawspilltree.gv");
            drawtree.WriteLine("digraph G {\n graph[\n rankdir = \"TD\" \n];");

            List<SpillTreeNode> nodelist = new List<SpillTreeNode>();
            nodelist.Add(spilltree.root);
            int depth = 0;

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;

                for (int i = 0; i < nodelistcount; i++)
                {
                    SpillTreeNode node = nodelist[0];

                    if (node.overlapping == true)
                        drawtree.Write(node.tree_index + "[style = filled, color = grey, shape=\"record\"");
                    else
                        drawtree.Write(node.tree_index + "[color = black, shape=\"record\"");

                    DrawNode(depth, node);
                    drawtree.WriteLine("];");

                    if (node.left_child != null)
                    {
                        drawtree.WriteLine(node.tree_index + "->" + node.left_child.tree_index);
                        nodelist.Add(node.left_child);
                    }
                    if (node.right_child != null)
                    {
                        drawtree.WriteLine(node.tree_index + "->" + node.right_child.tree_index);
                        nodelist.Add(node.right_child);
                    }

                    nodelist.RemoveAt(0);
                }
                depth++;
            }
            drawtree.WriteLine("}");
            drawtree.Flush();
            drawtree.Close();
        }

        public void DrawNode(int depth, SpillTreeNode node)
        {
            if (node.left_child == null && node.right_child == null)
            {
                drawtree.Write(",label=\"");
                int count = 0;
                while (count < node.points.Count)
                {
                    drawtree.Write("(" + node.points[count].indices.array_index + " % " + node.points[count].data.querystring + ")");
                    count++;
                    if (count % 3 == 0)
                        drawtree.Write(" \\n ");
                }
                drawtree.Write("\"");
            }
        }
    }
}
