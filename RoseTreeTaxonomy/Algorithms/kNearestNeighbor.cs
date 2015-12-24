using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;

namespace RoseTreeTaxonomy.Algorithms
{
    public class kNearestNeighbor
    {
        //By Xiting, robust
        public int[] Search(int query_index, RoseTreeNode[] nodearray, int k)
        {
            MinHeapDouble mhd = new MinHeapDouble(k);
            for (int i = 0; i < k; i++)
                mhd.insert(-1, double.MinValue);

            int validcnt = 0;
            for (int i = 0; i < nodearray.Length; i++)
                if (nodearray[i] != null && nodearray[i].valid == true && i != query_index)
                {
                    SparseVectorList featurevector1 = nodearray[query_index].data;
                    SparseVectorList featurevector2 = nodearray[i].data;

                    double cosine = featurevector1.Cosine(featurevector1, featurevector2);

                    if (cosine > mhd.min() || (cosine == mhd.min() && nodearray[i].indices.array_index < mhd.getIndices()[0]))
                        mhd.changeMin(nodearray[i].indices.array_index, cosine);
                    validcnt++;
                }

            MinHeapDouble.heapSort(mhd);

            if (validcnt < k)
            {
                int[] searchres = new int[validcnt];
                for (int i = 0; i < validcnt; i++)
                    searchres[i] = mhd.getIndices()[i];
                return searchres;
            }
            else
                return mhd.getIndices();
        }

        //public int[] Search(int query_index, RoseTreeNode[] nodearray, int k)
        //{
        //    MinHeapDouble mhd = new MinHeapDouble(k);
        //    for (int i = 0; i < k; i++)
        //        mhd.insert(-1, double.MinValue);

        //    for (int i = 0; i < nodearray.Length; i++)
        //        if (nodearray[i] != null && nodearray[i].valid == true && i != query_index)
        //        {
        //            SparseVectorList featurevector1 = nodearray[query_index].data;
        //            SparseVectorList featurevector2 = nodearray[i].data;

        //            double cosine = featurevector1.Cosine(featurevector1, featurevector2);

        //            if (cosine > mhd.min() || (cosine == mhd.min() && nodearray[i].indices.array_index < mhd.getIndices()[0]))
        //                mhd.changeMin(nodearray[i].indices.array_index, cosine);
        //        }

        //    MinHeapDouble.heapSort(mhd);
        //    return mhd.getIndices().ToArray();
        //}

        public int[] SearchTree(RoseTreeNode query, RoseTreeNode[] nodearray, int k, RoseTreeNode[] all_nodearray)
        {
            MinHeapDouble mhd = new MinHeapDouble(k);
            for (int i = 0; i < k; i++)
                mhd.insert(-1, double.MinValue);

            for (int i = 0; i < nodearray.Length; i++)
                if (nodearray[i] != null && nodearray[i].Equals(query) == false)
                {
                    SparseVectorList featurevector1 = query.data;
                    SparseVectorList featurevector2 = nodearray[i].data;

                    double cosine = featurevector1.Cosine(featurevector1, featurevector2);

                    if (cosine > mhd.min() || (cosine == mhd.min() && nodearray[i].indices.initial_index < mhd.getIndices()[0]))
                        mhd.changeMin(nodearray[i].indices.initial_index, cosine);
                }

            MinHeapDouble.heapSort(mhd);

            return mhd.getIndices().ToArray();
        }

        public int[] SearchSparseVectorList(SparseVectorList query, SparseVectorList[] nodearray, int k)
        {
            MinHeapDouble mhd = new MinHeapDouble(k);
            for (int i = 0; i < k; i++)
                mhd.insert(-1, double.MinValue);

            for (int i = 0; i < nodearray.Length; i++)
                if (nodearray[i] != null && nodearray[i].Equals(query) == false)
                {
                    double cosine = query.Cosine(query, nodearray[i]);

                    if (cosine > mhd.min() || (cosine == mhd.min() && i < mhd.getIndices()[0]))
                        mhd.changeMin(i, cosine);
                }

            MinHeapDouble.heapSort(mhd);

            return mhd.getIndices().ToArray();
        }

        public int[] SearchProject(int query_index, RoseTreeNode[] nodearray, int k, int projectdimension)
        {
            MinHeapDouble mhd = new MinHeapDouble(k);
            for (int i = 0; i < k; i++)
                mhd.insert(-1, double.MinValue);

            for(int i = 0; i < nodearray.Length; i++)
                if (nodearray[i] != null && nodearray[i].valid == true && i != query_index)
                {
                    double[] project_featurevector1 = nodearray[query_index].projectdata;
                    double[] project_featurevector2 = nodearray[i].projectdata;

                    double cosine = RoseTreeMath.ProjectData_Cosine(project_featurevector1, project_featurevector2, projectdimension);

                    if (cosine > mhd.min() || (cosine == mhd.min() && i < mhd.getIndices()[0]))
                        mhd.changeMin(i, cosine);
                }
            MinHeapDouble.heapSort(mhd);
            return mhd.getIndices().ToArray();
        }
    }
}
