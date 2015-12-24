using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvolutionaryRoseTree.Util;
using RoseTreeTaxonomy.DataStructures;

namespace EvolutionaryRoseTree.Constraints
{
    class ConstraintTreeMatching
    {
        ConstraintTree constraintTree1;
        ConstraintTree constraintTree2;
        RemoveConflictParameters editCosts;

        ConstraintTreeNode[] nodeArray1, nodeArray2;
        SparseVectorList[] vectorArray1, vectorArray2;
        Dictionary<ConstraintTreeNode, int> nodeToIndexDict1, nodeToIndexDict2;
        int leafCnt1, leafCnt2;

        int nodeCnt1, nodeCnt2, nodeCnt;
        public ConstraintTreeMatching(ConstraintTree constraintTree1,
            ConstraintTree constraintTree2,
            RemoveConflictParameters nodeEditCosts)
        {
            this.constraintTree1 = constraintTree1;
            this.constraintTree2 = constraintTree2;
            this.editCosts = nodeEditCosts;

            #region Initialization
            //var docNum = constraintTree1.GetLeafCount();
            var nodeList1 = constraintTree1.GetAllValidTreeNodes();
            var nodeList2 = constraintTree2.GetAllValidTreeNodes();
            var vectorDict1 = constraintTree1.GetFeatureVectorDict();
            var vectorDict2 = constraintTree2.GetFeatureVectorDict();
            nodeCnt1 = nodeList1.Count;
            nodeCnt2 = nodeList2.Count;
            nodeCnt = nodeCnt1 + nodeCnt2;

            //ConstraintTreeNode[] nodeArray1, nodeArray2;
            //SparseVectorList[] vectorArray1, vectorArray2;
            //Dictionary<ConstraintTreeNode, int> nodeToIndexDict1, nodeToIndexDict2;

            TransformToArray(nodeList1, vectorDict1, 
                out nodeArray1, out vectorArray1, out nodeToIndexDict1, out leafCnt1);
            TransformToArray(nodeList2, vectorDict2,
                out nodeArray2, out vectorArray2, out nodeToIndexDict2, out leafCnt2);
            #endregion

        }


        public int[] Match()
        {
            if (leafCnt1 == 0 || leafCnt2 == 0)
                return new int[0];
            if (leafCnt1 != leafCnt2)
                throw new Exception("Error!");

            var subCosts = new Dictionary<int, double>();

            for (int i = 0; i < leafCnt1; i++)
            {
                var constraintNode1 = nodeArray1[i];
                var constraintNode2 = nodeArray2[i];
                if (constraintNode1.IsFreeNode || constraintNode2.IsFreeNode)
                    continue;
                subCosts.Add(i, GetSubstituteCost(i, i, constraintNode1, constraintNode2));
            }

            var matchNodeCnt = (int)Math.Round(subCosts.Count * editCosts.Ratio);
            if (matchNodeCnt == 0)
                return new int[0];

            var matchedNodes = new int[matchNodeCnt];
            int nodeIndex = 0;
            foreach (var kvp in subCosts.OrderBy(kvp=>kvp.Value))
            {
                matchedNodes[nodeIndex] = kvp.Key;
                if (++nodeIndex >= matchNodeCnt)
                    break;
            }
            return matchedNodes;
        }


        public int[] Match2()
        {

            #region Calculate cost matrix
            double[,] costMatrix = new double[nodeCnt, nodeCnt];
            for (int i = 0; i < nodeCnt1; i++)
            {
                var node1 = nodeArray1[i];
                for (int j = 0; j < nodeCnt2; j++)
                {
                    var node2 = nodeArray2[j];
                    costMatrix[i, j] = GetSubstituteCost(i, j, node1, node2);
                    //Trace.WriteLine(string.Format("Test cache data: {0}, {1}", costMatrix[i, j],
                    //    GetSubstituteCost_Previous(node0, node1)));
                }
            }

            for (int i = nodeCnt1; i < nodeCnt; i++)
                for (int j = 0; j < nodeCnt2; j++)
                    costMatrix[i, j] = double.MaxValue;
            for (int i = 0; i < nodeCnt1; i++)
                for (int j = nodeCnt2; j < nodeCnt1 + nodeCnt2; j++)
                    costMatrix[i, j] = double.MaxValue;


            for (int i = 0; i < nodeCnt2; i++)
            {
                var node = nodeArray2[i];
                costMatrix[nodeCnt1 + i, i] = editCosts.NodeInsertCost;
            }
            for (int i = 0; i < nodeCnt1; i++)
            {
                var node = nodeArray1[i];
                costMatrix[i, nodeCnt2 + i] = editCosts.NodeDeleteCost;
            }
            #endregion

            #region Set hard constraints
            //leaf with id 0 must be matched to leaf with id 1
            for (int iDoc = 0; iDoc < nodeCnt1; iDoc++)
            {
                for (int jDoc = 0; jDoc < nodeCnt2; jDoc++)
                {
                    if (iDoc == jDoc)
                        continue;
                    if (iDoc >= leafCnt1 && jDoc >= leafCnt2)
                        continue;
                    costMatrix[iDoc, jDoc] = double.MaxValue;
                }
            }
            #endregion

            #region Hungarian matching & final results
            double cost;
            int[] result;
            HungarianMatchingHelper.GetMinimumWeightMatchingCost(costMatrix,
                out result, out cost);
            List<int> matchedNodes = new List<int>();
            for (int iDoc = 0; iDoc < leafCnt1; iDoc++)
            {
                var map = result[iDoc];
                if (map >= 0 && map < leafCnt1)
                    matchedNodes.Add(iDoc);
            }
            #endregion

            return matchedNodes.ToArray<int>();
        }

        private double GetSubstituteCost(
            int nodeIndex1, int nodeIndex2,
            ConstraintTreeNode node1, ConstraintTreeNode node2)
        {
            double cost = GetNodeSubstituteDistanceCosine(nodeIndex1, nodeIndex2) * 
                editCosts.NodeSubstituteCost;

            cost += GetNodeNeighbourEditDistance(nodeIndex1, nodeIndex2, node1, node2);
            return cost;
        }

        private double GetNodeNeighbourEditDistance(int nodeIndex1, int nodeIndex2,
            ConstraintTreeNode node1, ConstraintTreeNode node2)
        {
            if (editCosts.EdgeSubstituteCost == 0 &&
               editCosts.EdgeInsertCost == 0 &&
               editCosts.EdgeDeleteCost == 0)
                return 0;

            int m1 = (node1.Children == null ? 0 : node1.Children.Count) + (node1.Parent == null ? 0 : 1);
            int m2 = (node2.Children == null ? 0 : node2.Children.Count) + (node2.Parent == null ? 0 : 1);

            if (m1 == 0 || m2 == 0)
                return double.MaxValue;
            double[,] edgeCostMatrix = new double[m1 + m2, m1 + m2];
            var neighArray1 = new int[m1];
            var neighArray2 = new int[m2];
            if (node1.Children != null)
            {
                for (int i = 0; i < node1.Children.Count; i++)
                {
                    neighArray1[i] = nodeToIndexDict1[node1.Children[i]];
                }
            }
            if(node1.Parent != null)
                neighArray1[m1 - 1] = nodeToIndexDict1[node1.Parent];
            if (node2.Children != null)
            {
                for (int i = 0; i < node2.Children.Count; i++)
                {
                    neighArray2[i] = nodeToIndexDict2[node2.Children[i]];
                }
            }
            if (node2.Parent != null)
                neighArray2[m2 - 1] = nodeToIndexDict2[node2.Parent];

            //Substitute
            for (int i = 0; i < m1; i++)
            {
                for (int j = 0; j < m2; j++)
                {
                    edgeCostMatrix[i, j] = GetEdgeSubstituteDistanceCosine(nodeIndex1, neighArray1[i],
                        nodeIndex2, neighArray2[j]) * editCosts.EdgeSubstituteCost;
                }
            }

            for (int i = m1; i < m1 + m2; i++)
                for (int j = 0; j < m2; j++)
                    edgeCostMatrix[i, j] = double.MaxValue;
            for (int i = 0; i < m1; i++)
                for (int j = m2; j < m1 + m2; j++)
                    edgeCostMatrix[i, j] = double.MaxValue;

            for (int i = 0; i < m2; i++)
            {
                edgeCostMatrix[m1 + i, i] = editCosts.EdgeInsertCost;
            }

            for (int i = 0; i < m1; i++)
            {
                edgeCostMatrix[i, m2 + i] = editCosts.EdgeDeleteCost;
            }

            double minCost;
            int[] result;
            HungarianMatchingHelper.GetMinimumWeightMatchingCost(edgeCostMatrix, out result, out minCost);
            return minCost;
        }


        private double GetEdgeSubstituteDistanceCosine(int nodeIndex1_1, int nodeIndex1_2,
            int nodeIndex2_1, int nodeIndex2_2)
        {
            return
                Math.Min(
                (GetNodeSubstituteDistanceCosine(nodeIndex1_1, nodeIndex2_1) +
                GetNodeSubstituteDistanceCosine(nodeIndex1_2, nodeIndex2_2)) / 2,
                (GetNodeSubstituteDistanceCosine(nodeIndex1_2, nodeIndex2_1) +
                GetNodeSubstituteDistanceCosine(nodeIndex1_1, nodeIndex2_2)) / 2);
        }

        Dictionary<int, Dictionary<int, double>> nodeDistanceDict = 
            new Dictionary<int, Dictionary<int, double>>();
        private double GetNodeSubstituteDistanceCosine(int nodeIndex1, int nodeIndex2)
        {
            //find from cache
            Dictionary<int, double> value1;
            double value2;
            if (nodeDistanceDict.TryGetValue(nodeIndex1, out value1))
            {
                if (value1.TryGetValue(nodeIndex2, out value2))
                    return value2;
            }
            else
            {
                value1 = new Dictionary<int, double>();
                nodeDistanceDict.Add(nodeIndex1, value1);
            }

            //calculate
            var vector1 = vectorArray1[nodeIndex1];
            var vector2 = vectorArray2[nodeIndex2];
            value2 = Math.Acos(Math.Min(vector1.Cosine(vector1, vector2), 1)) / Math.PI * 2;

            //cache
            value1.Add(nodeIndex2, value2);

            return value2;
        }


        private void TransformToArray(List<ConstraintTreeNode> nodeList, 
            Dictionary<ConstraintTreeNode, SparseVectorList> vectorDict, 
            out ConstraintTreeNode[] nodeArray, out SparseVectorList[] vectorArray, 
            out Dictionary<ConstraintTreeNode, int> nodeToIndexDict,
            out int leafCnt)
        {
            nodeArray = new ConstraintTreeNode[nodeList.Count];
            vectorArray = new SparseVectorList[nodeList.Count];
            nodeToIndexDict = new Dictionary<ConstraintTreeNode, int>();
            leafCnt = 0;

            int nodeIndex = 0;
            foreach (var node in nodeList.Reverse<ConstraintTreeNode>())
            {
                if (node.Children == null || node.Children.Count == 0)
                {
                    var iniIndex = node.InitialIndex;
                    nodeArray[iniIndex] = node;
                    nodeToIndexDict.Add(node, iniIndex);
                    vectorArray[iniIndex] = vectorDict[node];
                    nodeIndex++;
                }
            }
            leafCnt = nodeIndex;

            for (int i = 0; i < leafCnt; i++)
            {
                if (nodeArray[i] == null)
                    throw new Exception("Error!");
            }

            foreach (var node in nodeList.Reverse<ConstraintTreeNode>())
            {
                if (node.Children != null && node.Children.Count > 0)
                {
                    nodeArray[nodeIndex] = node;
                    nodeToIndexDict.Add(node, nodeIndex);
                    vectorArray[nodeIndex] = vectorDict[node];
                    nodeIndex++;
                }
            }
        }
    }
}
