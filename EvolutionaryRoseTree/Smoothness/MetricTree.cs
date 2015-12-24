using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;

using EvolutionaryRoseTree.Constraints;
namespace EvolutionaryRoseTree.Smoothness
{
    class MetricTree
    {
        public MetricTreeNode root { get; protected set; }
        public int rootParentLabel = -1;
        public int LeafCount = 1;
        public int InternalNodeCount = 0;

        public MetricTree(int testtreeid)
        {
            rootParentLabel = 1;
            switch (testtreeid)
            {
                case 0:
                    root = new MetricTreeNode(this);
                    root.AddLeafChild(2);
                    root.AddLeafChild(3);
                    MetricTreeNode node1 = root.AddInternalChild();
                    node1.AddLeafChild(4);
                    node1.AddLeafChild(5);
                    node1.AddLeafChild(6);
                    MetricTreeNode node2 = root.AddInternalChild();
                    node2.AddLeafChild(7);
                    node2.AddLeafChild(8);
                    node2.AddLeafChild(9);
                    break;
                case 1:
                    root = new MetricTreeNode(this);
                    root.AddLeafChild(2);
                    node1 = root.AddInternalChild();
                    node1.AddLeafChild(3);
                    node1.AddLeafChild(4);
                    node1.AddLeafChild(5);
                    node1.AddLeafChild(6);
                    node2 = root.AddInternalChild();
                    node2.AddLeafChild(7);
                    node2.AddLeafChild(8);
                    node2.AddLeafChild(9);
                    break;
                default:
                    throw new Exception("No such tested tree specified");
            }
        }

        public MetricTree(RoseTree rosetree, RoseTreeNode rootParent)
        {
            if (rootParent.children != null)
                throw new Exception("root parent must be leaf node!");
            rootParentLabel = rootParent.indices.initial_index;

            //BFS to get the edge hash table
            List<RoseTreeNode> OrgBFSNodeQueue = new List<RoseTreeNode>();
            OrgBFSNodeQueue.Add(rootParent.parent);
            Dictionary<RoseTreeNode, RoseTreeNode> metricTreeParentHash = new Dictionary<RoseTreeNode, RoseTreeNode>();
            metricTreeParentHash.Add(rootParent.parent, rootParent);

            this.root = new MetricTreeNode(this);
            List<MetricTreeNode> BFSNodeQueue = new List<MetricTreeNode>();
            BFSNodeQueue.Add(root);

            while (BFSNodeQueue.Count != 0)
            {
                RoseTreeNode orgnode = OrgBFSNodeQueue[0];
                OrgBFSNodeQueue.RemoveAt(0);
                MetricTreeNode node = BFSNodeQueue[0];
                BFSNodeQueue.RemoveAt(0);

                RoseTreeNode metricTreeParent = metricTreeParentHash[orgnode];
                foreach (RoseTreeNode child in orgnode.children)
                {
                    if (child != metricTreeParent)
                    {
                        if (child.children == null)
                        {
                            node.AddLeafChild(child.indices.initial_index);
                        }
                        else
                        {
                            OrgBFSNodeQueue.Add(child);
                            metricTreeParentHash.Add(child, orgnode);
                            MetricTreeNode internalnode = node.AddInternalChild();
                            BFSNodeQueue.Add(internalnode);
                        }
                    }
                }

                if (orgnode.parent != null && orgnode.parent != metricTreeParent)
                {
                    RoseTreeNode child = orgnode.parent;
                    if (child != metricTreeParent)
                    {
                        if (child.children == null)
                        {
                            node.AddLeafChild(child.indices.initial_index);
                        }
                        else
                        {
                            OrgBFSNodeQueue.Add(child);
                            metricTreeParentHash.Add(child, orgnode);
                            MetricTreeNode internalnode = node.AddInternalChild();
                            BFSNodeQueue.Add(internalnode);
                        }
                    }
                }

                if (node.ChildrenCount == 1)
                    SubstituteWithOnlyChild(node);
            }
        }

        public MetricTree(ConstraintTree constrainttree, ConstraintTreeNode rootParent)
        {
            if (rootParent.Children != null)
                throw new Exception("root parent must be leaf node!");
            rootParentLabel = rootParent.InitialIndex;

            //BFS to get the edge hash table
            List<ConstraintTreeNode> OrgBFSNodeQueue = new List<ConstraintTreeNode>();
            OrgBFSNodeQueue.Add(rootParent.Parent);
            Dictionary<ConstraintTreeNode, ConstraintTreeNode> metricTreeParentHash = new Dictionary<ConstraintTreeNode, ConstraintTreeNode>();
            metricTreeParentHash.Add(rootParent.Parent, rootParent);

            this.root = new MetricTreeNode(this);
            List<MetricTreeNode> BFSNodeQueue = new List<MetricTreeNode>();
            BFSNodeQueue.Add(root);

            while (BFSNodeQueue.Count != 0)
            {
                ConstraintTreeNode orgnode = OrgBFSNodeQueue[0];
                OrgBFSNodeQueue.RemoveAt(0);
                MetricTreeNode node = BFSNodeQueue[0];
                BFSNodeQueue.RemoveAt(0);

                ConstraintTreeNode metricTreeParent = metricTreeParentHash[orgnode];
                foreach (ConstraintTreeNode child in orgnode.Children)
                {
                    if (child != metricTreeParent)
                    {
                        if (child.Children == null)
                        {
                            node.AddLeafChild(child.InitialIndex);
                        }
                        else
                        {
                            OrgBFSNodeQueue.Add(child);
                            metricTreeParentHash.Add(child, orgnode);
                            MetricTreeNode internalnode = node.AddInternalChild();
                            BFSNodeQueue.Add(internalnode);
                        }
                    }
                }

                if (orgnode.Parent != null && orgnode.Parent != metricTreeParent)
                {
                    ConstraintTreeNode child = orgnode.Parent;
                    if (child != metricTreeParent)
                    {
                        if (child.Children == null)
                        {
                            node.AddLeafChild(child.InitialIndex);
                        }
                        else
                        {
                            OrgBFSNodeQueue.Add(child);
                            metricTreeParentHash.Add(child, orgnode);
                            MetricTreeNode internalnode = node.AddInternalChild();
                            BFSNodeQueue.Add(internalnode);
                        }
                    }
                }

                if (node.ChildrenCount == 1)
                    SubstituteWithOnlyChild(node);
            }
        }

        private void SubstituteWithOnlyChild(MetricTreeNode node)
        {
            if (node.Parent == null)
            {
                this.root = node.GetChild(0);
                this.root.Parent = null;
                this.InternalNodeCount--;
            }
            else
            {
                node.Parent.SubstitueChild(node);
            }
        }

        #region draw tree
        public void DrawTree(string filename)
        {
            StreamWriter drawtree = new StreamWriter(filename);
            drawtree.WriteLine("digraph G \n {graph[ \n rankdir = \"TD\"];");

            //width-first traversal
            List<MetricTreeNode> cnodelist = new List<MetricTreeNode>();
            cnodelist.Add(this.root);
            int treeindex = 0;
            while (cnodelist.Count != 0)
            {
                MetricTreeNode treenode = cnodelist[0];
                if (!treenode.IsLeaf)
                    cnodelist.AddRange(treenode.GetChildren());
                treenode.DrawTreeIndex = treeindex;
                if (treenode.Parent != null)
                    drawtree.WriteLine(treenode.Parent.DrawTreeIndex + "->" + treenode.DrawTreeIndex);
                DrawNode(drawtree, treenode);
                cnodelist.RemoveAt(0);
                treeindex++;
            }

            drawtree.WriteLine("}");
            drawtree.Flush();
            drawtree.Close();
        }

        private void DrawNode(StreamWriter drawtree, MetricTreeNode node)
        {
            drawtree.Write(node.DrawTreeIndex + "[color = grey, label = \"");
            if (node.IsLeaf)
            {
                drawtree.Write("-{0}-\\n", node.Label);
                drawtree.Write(node.MetricLabel);
            }
            drawtree.WriteLine("\", shape=\"record\"];");
        }
        #endregion draw tree
    }


}
