using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.Tools;

namespace RoseTreeTaxonomy.DataStructures
{
    public class RoseTreeNode
    {
        public RoseTreeNode parent = null;
        public RoseTreeNode[] children;
        public SparseVectorList data;       
        public bool valid = true;
        public Indices indices = new Indices();
        public double log_likelihood;
        public CacheNodeValues cache_nodevalues = new CacheNodeValues();
        public List<SpillTreeNode> spilltree_positions = new List<SpillTreeNode>();

        public int tree_depth;

        public double[] projectdata;
        //public PPjoinTokenList ppjoinplus_token;

        //Xiting //For constrained rose tree
        public double LogTreeProbability = 0;
        public double log_likelihood_posterior = 0;
        public int DepthInTree = 0;
        public int MergeTreeIndex = -1;
        //For rule rose tree
        public int LeafCount = 1;
        //Store DocId
        public string DocId;    //Global doc id, used to identify which document it is 
        public Dictionary<CacheKey, CacheValue> CacheMergePairs = new Dictionary<CacheKey,CacheValue>();
        //Open some rose tree node
        public bool OpenedNode = false;
        public bool BOthers = false;

        public RoseTreeNode(RoseTreeNode[] children, SparseVectorList data, double[] projectdata, 
            int array_index, string docid = null)
        {
            this.children = children;
            this.data = data;
            this.projectdata = projectdata;
            this.indices.array_index = array_index;
            this.DocId = docid;

            if (children != null && children.Length != 0)
            {
                for (int i = 0; i < children.Length; i++)
                    children[i].parent = this;
            }
        }

        public class Indices
        {
            public int initial_index = -1;
            public int array_index = -1;
            public int tree_index = -1;
        }

        public class CacheNodeValues
        {
            public int subtree_leaf_count;         
            public double log_likelihood_part1, log_likelihood_part2;
            public double logf;
            public double logf_part1;
            //Xiting
            public double children_log_likelihood_sum;
            public double children_log_likelihood_posterior_sum;
            public double children_log_treeprobability_sum;
            public double children_log_incremental_cost_sum;
        }

        public void Invalidate()
        {
            for (int i = 0; i < spilltree_positions.Count; i++)
                spilltree_positions[i].InvalidatePoint(this);
            //this.data.Invalidate();
            //this.data = null;
            //this.projectdata = null;
            //this.indices = null;
            //this.cache_nodevalues = null;
            //this.spilltree_positions.Clear();//this.spilltree_positions = null;
            //Xiting
            this.CacheMergePairs = null;
            this.valid = false;
        }

        public double JoinLogLikelihood(CacheClass cacheclass, RoseTreeNode node1, RoseTreeNode node2, double logf, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            double log_likelihood = 0;

            log_likelihood_part1 = cacheclass.GetLogPi(2) + logf;
            log_likelihood_part2 = cacheclass.GetLogOneMinusPi(2) + (node1.log_likelihood + node2.log_likelihood);

            if (log_likelihood_part1 > log_likelihood_part2)
                log_likelihood = log_likelihood_part1 + Math.Log(1 + Math.Exp(log_likelihood_part2 - log_likelihood_part1));
            else
                log_likelihood = log_likelihood_part2 + Math.Log(1 + Math.Exp(log_likelihood_part1 - log_likelihood_part2));

            return log_likelihood;
        }

        public double AbsorbLogLikelihood(CacheClass cacheclass, RoseTreeNode node1, RoseTreeNode node2, double logf, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            log_likelihood_part1 = 0;
            log_likelihood_part2 = 0;

            if (!(node1.children != null && node1.children.Length != 0))
                return double.MinValue;

            //if (node1.tree_depth < node2.tree_depth + 1)
            //    return double.MinValue;

            double log_likelihood = 0;
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + 1) + logf;
            log_likelihood_part2 = cacheclass.GetLogOneMinusPi(node1.children.Length + 1);

            //double part = 0;
            //for (int k = 0; k < node1.children.Length; k++)
            //    part += node1.children[k].log_likelihood;
            //part += node2.log_likelihood;
            //log_likelihood_part2 += part;
            //if (Math.Abs(part - (node1.cache_nodevalues.children_log_likelihood_sum + node2.log_likelihood)) > 1e-8)
            //    Console.Write("");

            log_likelihood_part2 += node1.cache_nodevalues.children_log_likelihood_sum + node2.log_likelihood;

            if (log_likelihood_part1 > log_likelihood_part2)
                log_likelihood = log_likelihood_part1 + Math.Log(1 + Math.Exp(log_likelihood_part2 - log_likelihood_part1));
            else
                log_likelihood = log_likelihood_part2 + Math.Log(1 + Math.Exp(log_likelihood_part1 - log_likelihood_part2));

            return log_likelihood;
        }

        public double CollapseLogLikelihood(CacheClass cacheclass, RoseTreeNode node1, RoseTreeNode node2, double logf, out double log_likelihood_part1, out double log_likelihood_part2)
        {
            log_likelihood_part1 = 0;
            log_likelihood_part2 = 0;

            if (!(node1.children != null && node1.children.Length != 0) || !(node2.children != null && node2.children.Length != 0))
                return double.MinValue;

            double log_likelihood = 0;
            log_likelihood_part1 = cacheclass.GetLogPi(node1.children.Length + node2.children.Length) + logf;
            log_likelihood_part2 = cacheclass.GetLogOneMinusPi(node1.children.Length + node2.children.Length);

            //double part1 = 0;
            //double part2 = 0;
            //for (int k = 0; k < node1.children.Length; k++)
            //    part1 += node1.children[k].log_likelihood;
            //for (int k = 0; k < node2.children.Length; k++)
            //    part2 += node2.children[k].log_likelihood;
            //if (Math.Abs(part1 + part2 - (node1.cache_nodevalues.children_log_likelihood_sum + node2.cache_nodevalues.children_log_likelihood_sum)) > 1e-8)
            //    Console.Write("");
            //log_likelihood_part2 += (part1 + part2);

            log_likelihood_part2 += node1.cache_nodevalues.children_log_likelihood_sum + node2.cache_nodevalues.children_log_likelihood_sum;

            if (log_likelihood_part1 > log_likelihood_part2)
                log_likelihood = log_likelihood_part1 + Math.Log(1 + Math.Exp(log_likelihood_part2 - log_likelihood_part1));
            else
                log_likelihood = log_likelihood_part2 + Math.Log(1 + Math.Exp(log_likelihood_part1 - log_likelihood_part2));

            return log_likelihood;
        }

        public void AdjustChildrenOrder(List<int> orderedMergeTreeIndices)
        {
            Dictionary<int, int> unorderedhash = new Dictionary<int, int>();
            int index = 0;
            foreach (RoseTreeNode child in children)
                unorderedhash.Add(child.MergeTreeIndex, index++);
            RoseTreeNode[] new_children = new RoseTreeNode[children.Length];
            index = 0;
            foreach (int mergetreeindex in orderedMergeTreeIndices)
            {
                new_children[index] = children[unorderedhash[mergetreeindex]];
                index++;
            }
            children = new_children;
        }
    }
}
