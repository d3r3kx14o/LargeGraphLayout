
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Smoothness
{
    class MetricTreeNode
    {
        public MetricTreeNode Parent;
        List<MetricTreeNode> Children = new List<MetricTreeNode>();
        MetricTree ContainedTree;

        public int Label { get; protected set; }
        public int MetricLabel { get; protected set; }

        public bool IsLeaf { get { return Children.Count == 0; } }
        public int ChildrenCount { get { return Children.Count; } }

        public int MinLabel = int.MaxValue;
        public int MaxLabel = int.MinValue;
        public int LabelCount = 0;

        public List<int> Partition = null;

        public int DrawTreeIndex;

        public MetricTreeNode(MetricTree containedtree)
        {
            Label = -1;
            ContainedTree = containedtree;
            ContainedTree.InternalNodeCount++;
        }

        public MetricTreeNode(int label, MetricTree containedtree)
        {
            Label = label;
            ContainedTree = containedtree;
            ContainedTree.LeafCount++;
        }

        public void AddLeafChild(int label)
        {
            MetricTreeNode child = new MetricTreeNode(label, ContainedTree);
            Children.Add(child);
            child.Parent = this;
        }

        public MetricTreeNode AddInternalChild()
        {
            MetricTreeNode child = new MetricTreeNode(ContainedTree);
            Children.Add(child);
            child.Parent = this;
            return child;
        }

        public void SetMetricLabel(int metriclabel)
        {
            this.MetricLabel = metriclabel;
            this.MinLabel = metriclabel;
            this.MaxLabel = metriclabel;
            this.LabelCount = 1;
        }

        public void UpdateMetricLabel(MetricTreeNode child)
        {
            if (child.MinLabel < this.MinLabel)
                this.MinLabel = child.MinLabel;
            if (child.MaxLabel > this.MaxLabel)
                this.MaxLabel = child.MaxLabel;
            this.LabelCount += child.LabelCount;
        }

        public void UpdatePartition(MetricTreeNode child)
        {
            if (Partition == null)
                Partition = new List<int>();
            if (child.Partition == null)
                Partition.Add(child.MinLabel);
            else
                Partition.AddRange(child.Partition);
        }

        public MetricTreeNode GetChild(int index)
        {
            return Children[index];
        }

        public IList<MetricTreeNode> GetChildren()
        {
            return Children.AsReadOnly();
        }

        public void SubstitueChild(MetricTreeNode node)
        {
            if (node.ChildrenCount != 1)
                throw new Exception("SubstitueChild error!");
            MetricTreeNode child = node.GetChild(0);
            int index = Children.IndexOf(node);
            Children[index] = child;
            child.Parent = this;
            ContainedTree.InternalNodeCount--;
        }
    }
}
