using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;

using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Constraints;

namespace EvolutionaryRoseTree.Accuracy
{
    //See Paper:   Revisiting k-means: New Algorithms via Bayesian Nonparametrics
    class KmeanCost
    {
        //DCM
        public static double GetKmeanCost(RoseTree rosetree, int[] labels)
        {
            double cost = 0;
            try
            {
                int model_index = rosetree.model_index;
                if (model_index != RoseTreeTaxonomy.Constants.Constant.DCM &&
                    model_index != RoseTreeTaxonomy.Constants.Constant.VMF)
                {
                    Console.WriteLine("Currently only support DCM or VMF!");
                    return -1;
                }

                Dictionary<int, int> labelCnt = new Dictionary<int, int>();
                for (int i = 0; i < labels.Length; i++)
                    if (labelCnt.ContainsKey(labels[i]))
                        labelCnt[labels[i]]++;
                    else
                        labelCnt.Add(labels[i], 1);

                Dictionary<int, ClusterCenterData> labelData = new Dictionary<int, ClusterCenterData>();
                try
                {
                    foreach (int label in labelCnt.Keys)
                    {
                        ClusterCenterData data = new ClusterCenterData();
                        data.Vector = rosetree.GetNodeByArrayIndex(label).data;
                        //if (data.Vector.normvalue == 0)
                        //    if (model_index == RoseTreeTaxonomy.Constants.Constant.DCM)
                        //        data.Vector.GetNormDCM();
                        //    else
                        //        data.Vector.GetNormvMF(rosetree.idf);
                        data.Norm = data.Vector.normvalue / labelCnt[label];
                        labelData.Add(label, data);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                IList<RoseTreeNode> leaves = rosetree.GetAllTreeLeaf();
                int leafindex = 0;
                foreach (RoseTreeNode leaf in leaves)
                {
                    SparseVectorList leafvector = leaf.data;
                    SparseVectorList centervector = labelData[labels[leafindex]].Vector;
                    double centernorm = labelData[labels[leafindex]].Norm;

                    cost += leafvector.normvalue * leafvector.normvalue + centernorm * centernorm
                        - 2 * leafvector.Cosine(leafvector, centervector) * leafvector.normvalue * centernorm;

                    leafindex++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }

            return cost;
        }

        class ClusterCenterData
        {
            public double Norm;
            public SparseVectorList Vector;
        }
    }
}
