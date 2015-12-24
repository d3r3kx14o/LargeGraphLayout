using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.DataStructures;
using System.IO;

namespace RoseTreeTaxonomy.Algorithms
{
    public class RoseTreeCut
    {
        RoseTree rosetree;

        public RoseTreeCut(RoseTree rosetree)
        {
            this.rosetree = rosetree;
        }

        public void CutByDepth(int depth)
        {
            int countdepth = 0;
            int treelabel = 0;

            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();

            nodelist.Add(rosetree.root);

            while (nodelist.Count != 0)
            {
                int nodelist_count = nodelist.Count;

                for (int i = 0; i < nodelist_count; i++)
                {
                    if (nodelist[0].children == null)
                    {
                        LabelTreeWithLabel(nodelist[0], treelabel);
                        treelabel++;
                    }
                    else
                        for (int j = 0; j < nodelist[0].children.Length; j++)
                            nodelist.Add(nodelist[0].children[j]);
                    nodelist.RemoveAt(0);
                }

                countdepth++;
                if (countdepth == depth)
                {
                    for (int i = 0; i < nodelist.Count; i++)
                    {
                        LabelTreeWithLabel(nodelist[i], treelabel);
                        treelabel++;
                    }
                    break;
                }
            }
        }

        public void CutByProbability()
        {
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();

            nodelist.Add(rosetree.root);

            int tree_label = 0;

            while (nodelist.Count != 0)
            {
                int nodelist_count = nodelist.Count;

                for (int i = 0; i < nodelist_count; i++)
                {
                    if (nodelist[0].cache_nodevalues.log_likelihood_part1 >
                        nodelist[0].cache_nodevalues.log_likelihood_part2)
                    {
                        LabelTreeWithLabel(nodelist[0], tree_label);
                        tree_label++;
                    }
                    else
                    {
                        if (nodelist[0].children != null)
                        {
                            for (int j = 0; j < nodelist[0].children.Length; j++)
                                nodelist.Add(nodelist[0].children[j]);
                        }
                    }

                    nodelist.RemoveAt(0);
                }
            }
        }

        public void LabelTreeWithLabel(RoseTreeNode tree_root, int label)
        {
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(tree_root);

            while (nodelist.Count != 0)
            {
                int nodelist_count = nodelist.Count;

                for (int i = 0; i < nodelist_count; i++)
                {
                    if (nodelist[0].children == null)
                        nodelist[0].data.tree_label = label;
                    else
                        for (int j = 0; j < nodelist[0].children.Length; j++)
                        {
                            nodelist.Add(nodelist[0].children[j]);
                        }
                    nodelist.RemoveAt(0);
                }
            }
        }

        public void WriteLabels(StreamWriter label_writer, StreamWriter tree_label_writer)
        {
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();

            nodelist.Add(rosetree.root);

            while (nodelist.Count != 0)
            {
                int nodelist_count = nodelist.Count;

                for (int i = 0; i < nodelist_count; i++)
                {
                    if (nodelist[0].children == null)
                    {
                        label_writer.Write(nodelist[0].data.label + ",");
                        tree_label_writer.Write(nodelist[0].data.tree_label + ",");
                    }
                    else
                        for (int j = 0; j < nodelist[0].children.Length; j++)
                            nodelist.Add(nodelist[0].children[j]);
                    nodelist.RemoveAt(0);
                }
            }

            label_writer.WriteLine();
            tree_label_writer.WriteLine();
        }
    }
}
