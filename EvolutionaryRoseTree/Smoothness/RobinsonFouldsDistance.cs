using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using RoseTreeTaxonomy.Algorithms;
using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.Experiments;
using EvolutionaryRoseTree.Util;
namespace EvolutionaryRoseTree.Smoothness
{
    class RobinsonFouldsDistance
    {
        //static StreamWriter ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\Evolutionary\distance_"+ 
        //     String.Format(ExperimentParameters.TimeFormat, DateTime.Now) + ".dat");

        public static StreamWriter ofile = null;

        public static double CalculateDistance(RoseTree tree0, RoseTree tree1)
        {
            if (tree0.lfv == tree1.lfv)
            {
                MetricTree metrictree0 = new MetricTree(tree0, tree0.GetNodeByArrayIndex(0));
                MetricTree metrictree1 = new MetricTree(tree1, tree1.GetNodeByArrayIndex(0));

                return CalculateDistance(metrictree0, metrictree1);
            }
            else
            {
                //make sure the proj method is the same
                SetRFDistanceConstraint();

                //string drawtreepath = @"D:\Project\EvolutionaryRoseTreeData\Evolutionary\Evolutionary_RoseTree\";

                ConstraintTree ctree1 = new ConstraintTree(tree1, tree0.lfv);   //project tree1 to tree0's data
                MetricTree metrictree0_0 = new MetricTree(tree0, tree0.GetNodeByArrayIndex(0));
                MetricTree metrictree0_1 = new MetricTree(ctree1, ctree1.GetLeafByInitialIndex(0));
                double distance0 = CalculateDistance(metrictree0_0, metrictree0_1);
                //ctree1.DrawConstraintTree(drawtreepath + "ctree1_i.gv", true);
                //metrictree0_0.DrawTree(drawtreepath + "metrictree0_0.gv");
                //metrictree0_1.DrawTree(drawtreepath + "metrictree0_1.gv");

                ConstraintTree ctree0 = new ConstraintTree(tree0, tree1.lfv);   //project tree0 to tree1's data
                MetricTree metrictree1_0 = new MetricTree(ctree0, ctree0.GetLeafByInitialIndex(0));
                MetricTree metrictree1_1 = new MetricTree(tree1, tree1.GetNodeByArrayIndex(0));
                double distance1 = CalculateDistance(metrictree1_0, metrictree1_1);
                //ctree0.DrawConstraintTree(drawtreepath + "ctree0_i.gv", true);
                //metrictree1_0.DrawTree(drawtreepath + "metrictree1_0.gv");
                //metrictree1_1.DrawTree(drawtreepath + "metrictree1_1.gv");

                //if (distance0 < distance1)
                //    throw new Exception("");
                //ofile.WriteLine("Distance: {0}, {1}", distance0, distance1);
                //ofile.Flush();

                ResetRFDistancePrevConstraint();

                if (double.IsNaN(distance0 + distance1))
                    Console.Write("");

                if (ofile != null)
                {
                    ofile.WriteLine("<MetricTree> {0}, {1}, {2}, {3}", metrictree0_0.InternalNodeCount, metrictree0_1.InternalNodeCount, metrictree1_0.InternalNodeCount, metrictree1_1.InternalNodeCount);
                    ofile.WriteLine("[RF]{0}\t{1}\t{2}\t", distance0, distance1, (distance0 + distance1) / 2);
                }
                return (distance0 + distance1) / 2;
                //return distance1;
            }
        }

        public static double CalculateDistance(MetricTree tree0, MetricTree tree1)
        {
            if (tree0.InternalNodeCount < tree1.InternalNodeCount)
                return CalculateDistance(tree1, tree0);

            CheckTwoTreesConsistent(tree0, tree1);

            Dictionary<int, int> nodeHash, edgeHash;
            RelabelBaseTree(tree0, out nodeHash, out edgeHash);

            //Console.WriteLine("-----------node hash-----------");
            //foreach (int key in nodeHash.Keys)
            //    Console.WriteLine("{0}->{1}", key, nodeHash[key]);
            //Console.WriteLine("-----------edge hash-----------");
            //foreach (int key in edgeHash.Keys)
            //    Console.WriteLine("{0}->{1}", key, edgeHash[key]);

            double inconsistentNumber = GetInconsistentPartitionNumber(tree1, nodeHash, edgeHash);
            //Console.WriteLine(inconsistentNumber);
            int n0 = tree0.InternalNodeCount - 1, n1 = tree1.InternalNodeCount - 1;
            //if (inconsistentNumber + consistentNumber != n1)
            //    throw new Exception("Error!");
            double m = n1 - inconsistentNumber;
            //ofile.WriteLine("n0: {0}  n1:{1}  m:{2}", n0, n1, m);
            return ((n0 - m) / n0 + (n1 - m) / n1) / 2;
            //return 5 * Math.Min((n0 - m) / n0, (n1 - m) / n1);
            //return 1 - m / Math.Sqrt(n0 * n1);
            //return 1 - 2 * m / (n0 + n1);
            //return 1 - m / n1;
        }

        static DataProjectionType prevDataProjectionType;
        static double prevAbandonCosineThreshold;
        static int prevAbandonTreeDepthThreshold;
        static int prevDocumentSkipPickedCount;
        static double prevNewTopicAlpha;
        static double prevDocumentCutGain;
        static double prevDocumentTolerateCosine;
        public static DataProjectionType RFDataProjectionType = DataProjectionType.DataPredictionSearchDown;
        private static void SetRFDistanceConstraint()
        {
            prevDataProjectionType = Constraint.DataProjectionType;
            prevAbandonCosineThreshold = DataProjection.AbandonCosineThreshold;
            prevAbandonTreeDepthThreshold = DataProjection.AbandonTreeDepthThreshold;
            prevDocumentSkipPickedCount = DataProjection.DocumentSkipPickedCount;
            prevNewTopicAlpha = DataProjection.NewTopicAlpha;
            prevDocumentCutGain = DataProjection.DocumentCutGain;
            prevDocumentTolerateCosine = DataProjection.DocumentTolerateCosine;

            Constraint.DataProjectionType = RFDataProjectionType;
            DataProjection.AbandonCosineThreshold = -1;
            DataProjection.AbandonTreeDepthThreshold = int.MaxValue;
            //DataProjection.DocumentSkipPickedCount = 2;
            //DataProjection.NewTopicAlpha = 1e-100;
            DataProjection.DocumentCutGain = 1;
            DataProjection.DocumentTolerateCosine = 0.2;
        }

        private static void ResetRFDistancePrevConstraint()
        {
            Constraint.DataProjectionType = prevDataProjectionType;
            DataProjection.AbandonCosineThreshold = prevAbandonCosineThreshold;
            DataProjection.AbandonTreeDepthThreshold = prevAbandonTreeDepthThreshold;
            DataProjection.DocumentSkipPickedCount = prevDocumentSkipPickedCount;
            DataProjection.NewTopicAlpha = prevNewTopicAlpha;
            DataProjection.DocumentCutGain = prevDocumentCutGain;
            DataProjection.DocumentTolerateCosine = prevDocumentTolerateCosine;
        }


        static void CheckTwoTreesConsistent(MetricTree tree0, MetricTree tree1)
        {
            if (tree0.rootParentLabel != tree1.rootParentLabel)
                throw new Exception("Do not match!");
            if (tree0.LeafCount != tree1.LeafCount)
                throw new Exception("Do not match!");
        }

        static void RelabelBaseTree(MetricTree tree,
            out Dictionary<int, int> nodeHash, out Dictionary<int, int> edgeHash)
        {
            nodeHash = new Dictionary<int, int>();
            edgeHash = new Dictionary<int, int>();

            //DFS traversal to relabel nodes
            List<MetricTreeNode> DFSNodeStack = new List<MetricTreeNode>();
            List<int> DFSToBeTraversedChildIndex = new List<int>();

            DFSNodeStack.Add(tree.root);
            DFSToBeTraversedChildIndex.Add(0);

            int metricLabel = 0;
            while (DFSNodeStack.Count != 0)
            {
                int end = DFSNodeStack.Count - 1;
                MetricTreeNode node = DFSNodeStack[end];
                DFSNodeStack.RemoveAt(end);
                int childindex = DFSToBeTraversedChildIndex[end];
                DFSToBeTraversedChildIndex.RemoveAt(end);

                if (node.IsLeaf)
                {
                    node.SetMetricLabel(metricLabel);
                    nodeHash.Add(node.Label, metricLabel);
                    node.Parent.UpdateMetricLabel(node);
                    metricLabel++;
                }
                else
                {
                    if (childindex == node.ChildrenCount)
                    {
                        if (node.Parent != null)
                            node.Parent.UpdateMetricLabel(node);
                    }
                    else
                    {
                        DFSNodeStack.Add(node);
                        DFSToBeTraversedChildIndex.Add(childindex + 1);
                        DFSNodeStack.Add(node.GetChild(childindex));
                        DFSToBeTraversedChildIndex.Add(0);
                    }
                }
            }

            //BFS to get the edge hash table
            List<MetricTreeNode> BFSNodeQueue = new List<MetricTreeNode>();
            BFSNodeQueue.Add(tree.root);

            while (BFSNodeQueue.Count != 0)
            {
                MetricTreeNode node = BFSNodeQueue[0];
                BFSNodeQueue.RemoveAt(0);

                if (node.MaxLabel - node.MinLabel + 1 != node.LabelCount)
                    throw new Exception("Not Continuous!");
                
                bool bleft = true;
                for (int i = 0; i < node.ChildrenCount; i++)
                {
                    MetricTreeNode child = node.GetChild(i);
                    if (!child.IsLeaf)
                    {
                        AddToEdgeHash(edgeHash, bleft, child);
                        BFSNodeQueue.Add(child);
                    }
                    bleft = false;
                }
            }
        }

        static void AddToEdgeHash(Dictionary<int,int> edgehash, bool bleft, MetricTreeNode node)
        {
            if (bleft)
            {
                if (edgehash.ContainsKey(node.MaxLabel))
                    throw new Exception("Error! Imperfect Edge Hash!");
                edgehash.Add(node.MaxLabel, node.MinLabel);
            }
            else
            {
                if (edgehash.ContainsKey(node.MinLabel))
                    throw new Exception("Error! Imperfect Edge Hash!");
                edgehash.Add(node.MinLabel, node.MaxLabel);
            }
        }

        static int consistentNumber;
        static double GetInconsistentPartitionNumber(MetricTree ctree,
            Dictionary<int, int> nodeHash, Dictionary<int, int> edgeHash)
        {
            int m = edgeHash.Count;
            int n = ctree.InternalNodeCount - 1;
            double[,] edgeWeight = new double[m, n];
            int iedge = 0;

            //DFS traversal to label the edges
            List<MetricTreeNode> DFSNodeStack = new List<MetricTreeNode>();
            List<int> DFSToBeTraversedChildIndex = new List<int>();

            DFSNodeStack.Add(ctree.root);
            DFSToBeTraversedChildIndex.Add(0);

            while (DFSNodeStack.Count != 0)
            {
                int end = DFSNodeStack.Count - 1;
                MetricTreeNode node = DFSNodeStack[end];
                DFSNodeStack.RemoveAt(end);
                int childindex = DFSToBeTraversedChildIndex[end];
                DFSToBeTraversedChildIndex.RemoveAt(end);

                if (node.IsLeaf)
                {
                    node.SetMetricLabel(nodeHash[node.Label]);
                    node.Parent.UpdatePartition(node);
                }
                else
                {
                    if (childindex == node.ChildrenCount)
                    {
                        if (node.Parent != null)
                            node.Parent.UpdatePartition(node);
                        //Calculate distance to the other tree's partition
                        if (node.Parent != null)    //not root
                        {
                            SetEdgeWeight(edgeWeight, edgeHash, node.Partition, iedge, ctree.LeafCount);
                            iedge++;
                        }
                    }
                    else
                    {
                        DFSNodeStack.Add(node);
                        DFSToBeTraversedChildIndex.Add(childindex + 1);
                        DFSNodeStack.Add(node.GetChild(childindex));
                        DFSToBeTraversedChildIndex.Add(0);
                    }
                }
            }

            int[] result;
            double mincost;
            HungarianMatchingHelper.GetMinimumWeightMatchingCost(edgeWeight, out result, out mincost);
            //double mincost2 = HungarianMatchingHelperOld.GetMinimumWeightMatchingCost(edgeWeight);
            //Console.WriteLine("Test hungarian cost: {0}, {1}", mincost, mincost2);

            return mincost / ctree.LeafCount;
        }

        static private void SetEdgeWeight(double[,] edgeWeight, Dictionary<int, int> edgeHash,
            List<int> Partition, int iedge, int N)
        {
            int i = 0;
            int partitionLen = Partition.Count;
            foreach (int bound0 in edgeHash.Keys)
            {
                int bound1 = edgeHash[bound0];
                int min = Math.Min(bound0, bound1);
                int max = Math.Max(bound0, bound1);
                int share = 0;
                foreach (int item in Partition)
                    if (item <= max && item >= min)
                        share++;
                int distance = max - min + 1 + partitionLen - 2 * share;
                if (distance > N / 2)
                    distance = N - distance;

                edgeWeight[i, iedge] = distance;
                i++;
            }

        }

        static int GetInconsistentPartitionNumber_Org(MetricTree ctree,
            Dictionary<int, int> nodeHash, Dictionary<int, int> edgeHash)
        {
            int inconsistentNumber = 0;
            consistentNumber = 0;

            //DFS traversal to label the edges
            List<MetricTreeNode> DFSNodeStack = new List<MetricTreeNode>();
            List<int> DFSToBeTraversedChildIndex = new List<int>();

            DFSNodeStack.Add(ctree.root);
            DFSToBeTraversedChildIndex.Add(0);

            while (DFSNodeStack.Count != 0)
            {
                int end = DFSNodeStack.Count - 1;
                MetricTreeNode node = DFSNodeStack[end];
                DFSNodeStack.RemoveAt(end);
                int childindex = DFSToBeTraversedChildIndex[end];
                DFSToBeTraversedChildIndex.RemoveAt(end);

                if (node.IsLeaf)
                {
                    node.SetMetricLabel(nodeHash[node.Label]);
                    node.Parent.UpdateMetricLabel(node);
                }
                else
                {
                    if (childindex == node.ChildrenCount)
                    {
                        if (node.Parent != null)
                            node.Parent.UpdateMetricLabel(node);
                        //check if the edge is consistent with base tree
                        if (node.MaxLabel - node.MinLabel + 1 != node.LabelCount)
                            inconsistentNumber++;
                        else if (!(edgeHash.ContainsKey(node.MinLabel) && edgeHash[node.MinLabel] == node.MaxLabel
                            || edgeHash.ContainsKey(node.MaxLabel) && edgeHash[node.MaxLabel] == node.MinLabel))
                            inconsistentNumber++;
                        else
                            consistentNumber++;
                    }
                    else
                    {
                        DFSNodeStack.Add(node);
                        DFSToBeTraversedChildIndex.Add(childindex + 1);
                        DFSNodeStack.Add(node.GetChild(childindex));
                        DFSToBeTraversedChildIndex.Add(0);
                    }
                }
            }

            return inconsistentNumber - 1;  //root will be considered as an inconsistent edge
        }

    }
}
