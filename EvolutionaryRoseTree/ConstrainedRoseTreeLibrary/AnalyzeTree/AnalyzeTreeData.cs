using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ConstrainedRoseTreeLibrary.Data;
using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.DataStructures;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;

namespace ConstrainedRoseTreeLibrary.AnalyzeTree
{
    public class AnalyzeTreeData
    {
        public static double GetContentVectorDotProduct(
            RoseTree rosetree1, RoseTreeNode rtnode1, 
            RoseTree rosetree2, RoseTreeNode rtnode2)
        {
            var vector1 = rosetree1.GetNodeByArrayIndex(rtnode1.indices.array_index).data;
            var vector2 = rosetree2.GetNodeByArrayIndex(rtnode2.indices.array_index).data;

            if (vector1.contentvectorlen < 0 || vector2.contentvectorlen < 0)
                throw new Exception(string.Format("Error Calculate DotProduct! vector1.contentvectorlen < 0 {0}, vector2.contentvectorlen < 0 {1}",
                    vector1.contentvectorlen < 0, vector2.contentvectorlen < 0));
            
            return MaxSimilarityContentVectorDataProjection.ContentVectorDotProduct(vector1, vector2);
        }

        public static int GetNodeID(RoseTree rosetree, RoseTreeNode node)
        {
            if (node.children == null)
            {
                var vector = rosetree.lfv.featurevectors[node.indices.initial_index];
                return vector.documentid;
            }
            else
            {
                return node.MergeTreeIndex + (rosetree.lfv as LoadRawDocumentFeatureVectors).DeltaIndex;
            }
        }

        public static Dictionary<string, int> GetNodeFeatureVector(RoseTree rosetree, RoseTreeNode node)
        {
            var vector = new Dictionary<string, int>();
            var invertedLexicon = rosetree.lfv.invertlexicon;
            var keyarray = node.data.keyarray;
            var valuearray = node.data.valuearray;
            for (int i = 0; i < node.data.count; i++)
            {
                vector.Add(invertedLexicon[keyarray[i]], valuearray[i]);
            }
            return vector;
        }

        public static void TestContentVectorLength(RoseTree rosetree, RoseTreeNode rtnode)
        {
            var vector = rtnode.data;
            if (vector.contentvectorlen != vector.count)
            {
                var lexicon = rosetree.lfv.invertlexicon;
                var contentlen = vector.contentvectorlen;
                Trace.WriteLine(string.Format("Test content vector len: {0}, {1}",
                    lexicon[vector.keyarray[contentlen - 1]], lexicon[vector.keyarray[contentlen]]));
            }
        }

        public static void UpdateTreeDepthInfo(RoseTree rosetree)
        {
            (rosetree as ConstrainedRoseTree).UpdateDepthInTree();
            foreach (var rtnode in rosetree.GetAllValidTreeNodes())
            {
                if (rtnode.children == null || rtnode.children.Length == 0)
                    rtnode.tree_depth = 1;
                else
                    rtnode.tree_depth = rtnode.children.Max(child => child.tree_depth) + 1;
            }
        }
    }

}
