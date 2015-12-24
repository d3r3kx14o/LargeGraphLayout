using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;

using EvolutionaryRoseTree.DataStructures;
using EvolutionaryRoseTree.Constraints;
using System.Collections;

namespace EvolutionaryRoseTree.Accuracy
{
    enum AccuracyMeasure { NMI, Purity, ARI };
    class LabelAccuracy
    {
        public static bool BWriteConfusionMatrix = false;

        public static double[] OutputAllAccuracy(ConstraintTree constree, RoseTree rosetree, StreamWriter ofile)
        {
            if (rosetree.lfv.featurevectors.Length != constree.GetLeafCount())
                throw new Exception("Length do not match! Can not test accuracy!");
            double[] nmi = new double[2];
            double[] purity = new double[2];
            string[] str = new string[2];
            int N = rosetree.lfv.featurevectors.Length;
            for (int level = 1; level <= 2; level++)
            {
                int[] label = GetLabel(constree, level);
                int[] label_groundtruth = GetLabel(rosetree, level);
                double[,] confuseMat = ConfusionMatrix.GetConfuseMatrix(label_groundtruth, label);
                str[level - 1] = ConfusionMatrix.ToString(confuseMat);

                nmi[level - 1] = NMI.GetNormalizedMutualInfo(confuseMat, N);
                purity[level - 1] = Purity.GetPurity(confuseMat, N);
            }

            ofile.WriteLine("[{0}] NMI:{1}", 1, nmi[0]);
            ofile.WriteLine("[{0}] NMI:{1}", 2, nmi[1]);
            ofile.WriteLine("[{0}] Purity:{1}", 1, purity[0]);
            ofile.WriteLine("[{0}] Purity:{1}", 2, purity[1]);
            if (BWriteConfusionMatrix)
            {
                ofile.WriteLine(str[0]);
                ofile.WriteLine(str[1]);
            }

            ofile.Flush();

            return new double[] { nmi[0], nmi[1], purity[0], purity[1] };
        }

        public static double[] OutputAllAccuracy(RoseTree rosetree, RoseTree groundtruth, StreamWriter ofile)
        {
            if (rosetree.lfv.featurevectors.Length != groundtruth.lfv.featurevectors.Length)
                throw new Exception("Length do not match! Can not test accuracy!");
            double[] nmi = new double[4];
            double[] kmeans = new double[4];
            double[] purity = new double[4];
            string[] str = new string[2];
            int N = rosetree.lfv.featurevectors.Length;
            int resIndex = 0;
            for (int level = 1; level <= 2; level++)
            {
                int[] label = GetLabel(rosetree, level);
                int[] slabel = GetShrinkLabel(rosetree, label);
                int[] label_groundtruth = GetLabel(groundtruth, level);
                double[,] confuseMat = ConfusionMatrix.GetConfuseMatrix(label_groundtruth, label);
                double[,] shrinkConfuseMat = ConfusionMatrix.GetConfuseMatrix(label_groundtruth, slabel);
                str[level - 1] = ConfusionMatrix.ToString(confuseMat);

                nmi[resIndex] = NMI.GetNormalizedMutualInfo(confuseMat, N);
                kmeans[resIndex] = KmeanCost.GetKmeanCost(rosetree, label);
                purity[resIndex] = Purity.GetPurity(confuseMat, N);
                resIndex++;
                nmi[resIndex] = NMI.GetNormalizedMutualInfo(shrinkConfuseMat, N);
                kmeans[resIndex] = KmeanCost.GetKmeanCost(rosetree, slabel);
                purity[resIndex] = Purity.GetPurity(shrinkConfuseMat, N);
                resIndex++;
            }

            ofile.WriteLine("[{0}] NMI:{1}\t{2}", 1, nmi[0], nmi[1]);
            ofile.WriteLine("[{0}] NMI:{1}\t{2}", 2, nmi[2], nmi[3]);
            ofile.WriteLine("[{0}] Kmeans:{1}\t{2}", 1, kmeans[0], kmeans[1]);
            ofile.WriteLine("[{0}] Kmeans:{1}\t{2}", 2, kmeans[2], kmeans[3]);
            ofile.WriteLine("[{0}] Purity:{1}\t{2}", 1, purity[0], purity[1]);
            ofile.WriteLine("[{0}] Purity:{1}\t{2}", 2, purity[2], purity[3]);
            if (BWriteConfusionMatrix)
            {
                ofile.WriteLine(str[0]);
                ofile.WriteLine(str[1]);
            }

            ofile.Flush();

            return new double[] { nmi[0], nmi[2], kmeans[0], kmeans[2], purity[0], purity[2],  
                    nmi[1], nmi[3], kmeans[1], kmeans[3], purity[1], purity[3]};
        }

        public static double[] OutputAllAccuracy_(RoseTree rosetree, RoseTree groundtruth, StreamWriter ofile)
        {
            if (rosetree.lfv.featurevectors.Length != groundtruth.lfv.featurevectors.Length)
                throw new Exception("Length do not match! Can not test accuracy!");
            double[] nmi = new double[2];
            double[] purity = new double[2];
            string[] str = new string[2];
            int N = rosetree.lfv.featurevectors.Length;
            for (int level = 1; level <= 2; level++)
            {
                int[] label = GetLabel(rosetree, level);
                int[] label_groundtruth = GetLabel(groundtruth, level);
                double[,] confuseMat = ConfusionMatrix.GetConfuseMatrix(label_groundtruth, label);
                str[level - 1] = ConfusionMatrix.ToString(confuseMat);

                nmi[level - 1] = NMI.GetNormalizedMutualInfo(confuseMat, N);
                purity[level - 1] = Purity.GetPurity(confuseMat, N);
            }

            ofile.WriteLine("[{0}] NMI:{1}", 1, nmi[0]);
            ofile.WriteLine("[{0}] NMI:{1}", 2, nmi[1]);
            ofile.WriteLine("[{0}] Purity:{1}", 1, purity[0]);
            ofile.WriteLine("[{0}] Purity:{1}", 2, purity[1]);
            if (BWriteConfusionMatrix)
            {
                ofile.WriteLine(str[0]);
                ofile.WriteLine(str[1]);
            }

            ofile.Flush();

            return new double[] { nmi[0], nmi[1], purity[0], purity[1] };
        }

        //Only used in test
        public static double[] OutputAllAccuracy(RoseTree rosetree, RoseTree rosetree1)
        {
            if (rosetree.lfv.featurevectors.Length != rosetree1.lfv.featurevectors.Length)
                throw new Exception("Length do not match! Can not test accuracy!");
            double[] nmi = new double[2];
            double[] purity = new double[2];
            for (int level = 1; level <= 2; level++)
            {
                int[] label = GetLabel(rosetree, level);
                int[] label_groundtruth = GetLabel(rosetree1, level);

                nmi[level - 1] = NMI.GetNormalizedMutualInfo(label, label_groundtruth);
                purity[level - 1] = Purity.GetPurity(label, label_groundtruth);
            }

            Console.WriteLine("[{0}] NMI:{1}", 1, nmi[0]);
            Console.WriteLine("[{0}] NMI:{1}", 2, nmi[1]);
            Console.WriteLine("[{0}] Purity:{1}", 1, purity[0]);
            Console.WriteLine("[{0}] Purity:{1}", 2, purity[1]);

            return new double[] { nmi[0], nmi[1], purity[0], purity[1] };
        }

        public static double GetLabelAccuracy(RoseTree rosetree, RoseTree groundtruth, int level,
            AccuracyMeasure accuracymeasure)
        {
            if (rosetree.lfv.featurevectors.Length != groundtruth.lfv.featurevectors.Length)
                throw new Exception("Length do not match! Can not test accuracy!");
            int[] label = GetLabel(rosetree, level);
            int[] label_groundtruth = GetLabel(groundtruth, level);
            switch (accuracymeasure)
            {
                case AccuracyMeasure.NMI:
                    return NMI.GetNormalizedMutualInfo(label, label_groundtruth);
                case AccuracyMeasure.Purity:
                    return Purity.GetPurity(label, label_groundtruth);
                case AccuracyMeasure.ARI:
                    return ARI.GetAdjustedRandIndex(label, label_groundtruth);
                default:
                    return -1;
            }
        }

        //label by ancestor's array_index
        public static int[] GetLabel(RoseTree rosetree, int level)
        {
            if (rosetree is GroundTruthRoseTree)
            {
                return (rosetree as GroundTruthRoseTree).GetGroundTruthLabels(level);
            }

            IList<RoseTreeNode> leaves = rosetree.GetAllTreeLeaf();
            int N = leaves.Count;
            int[] labels = new int[N];

            if (rosetree is ConstrainedBayesionBinaryTree) // || rosetree is GroundTruthBinaryTree)
            {
                int clusternumber = ConstrainedBayesionBinaryTree.GTClusterNumber[level - 1];
                if (clusternumber > N)
                    clusternumber = N;
                SortedSet<int> sortedset = new SortedSet<int>();

                sortedset.Add(2 * N - 2);

                for (int i = 0; i < clusternumber - 1; i++)
                {
                    int maxnode = sortedset.Max;
                    sortedset.Remove(maxnode);

                    RoseTreeNode parent = rosetree.GetNodeByArrayIndex(maxnode);
                    sortedset.Add(parent.children[0].indices.array_index);
                    sortedset.Add(parent.children[1].indices.array_index);
                }

                int ilabel = 0;
                foreach (int clusterrootindex in sortedset)
                {
                    RoseTreeNode clusterroot = rosetree.GetNodeByArrayIndex(clusterrootindex);
                    foreach (RoseTreeNode leaf in RoseTree.GetSubTreeLeaf(clusterroot))
                        labels[leaf.indices.array_index] = ilabel;
                    ilabel++;
                }

                return labels;
            }

            RoseTreeNode[] nodebuffer = new RoseTreeNode[level];
            int bufferpt = 0;

            foreach (RoseTreeNode leaf in leaves)
            {
                RoseTreeNode node = leaf;
                while (node.parent != null)
                {
                    nodebuffer[bufferpt] = node;
                    node = node.parent;
                    bufferpt = (++bufferpt) % level;
                }

                node = nodebuffer[bufferpt];
                if (node == null) node = leaf;

                labels[leaf.indices.initial_index] = node.indices.array_index;
            }

            return labels;
        }

        //class reverseIntComparer : IComparer<int>
        //{
        //    int IComparer<int>.Compare(int a, int b)
        //    {
        //        return -a.CompareTo(b);
        //    }
        //}

        public static int[] GetTreeLabel(RoseTree rosetree, int level)
        {
            IList<RoseTreeNode> leaves = rosetree.GetAllTreeLeaf();
            int N = leaves.Count;
            int[] labels = new int[N];

            RoseTreeNode[] nodebuffer = new RoseTreeNode[level];
            int bufferpt = 0;

            foreach (RoseTreeNode leaf in leaves)
            {
                RoseTreeNode node = leaf;
                while (node.parent != null)
                {
                    nodebuffer[bufferpt] = node;
                    node = node.parent;
                    bufferpt = (++bufferpt) % level;
                }

                node = nodebuffer[bufferpt];
                if (node == null) node = leaf;

                labels[leaf.indices.initial_index] = node.indices.array_index;
            }

            return labels;
        }

        private static int[] GetShrinkLabel(RoseTree rosetree, int[] labels)
        {
            int N = labels.Length;
            int[] slabels = new int[N];

            Dictionary<int, int> labelCnt = new Dictionary<int, int>();
            for (int i = 0; i < labels.Length; i++)
            {
                int label = labels[i];
                if (labelCnt.ContainsKey(label))
                    labelCnt[label]++;
                else
                    labelCnt.Add(label, 1);
            }

            List<int> normallabels = new List<int>();
            List<int> shrinklabels = new List<int>();
            foreach (int label in labelCnt.Keys)
                if (labelCnt[label] == 1)
                    shrinklabels.Add(label);
                else
                    normallabels.Add(label);

            if (shrinklabels.Count == 0 || normallabels.Count == 0)
                return labels;

            //find nearest neighbour
            Dictionary<int, int> shrinklabelHash = new Dictionary<int, int>();
            foreach (int slabel in shrinklabels)
            {
                double maxCos = double.MinValue;
                int maxNlabel = -1;
                SparseVectorList vector0 = rosetree.GetNodeByArrayIndex(slabel).data;
                foreach (int nlabel in normallabels)
                {
                    double cos = vector0.Cosine(vector0, rosetree.GetNodeByArrayIndex(nlabel).data);
                    if (cos > maxCos)
                    {
                        maxCos = cos;
                        maxNlabel = nlabel;
                    }
                }
                shrinklabelHash.Add(slabel, maxNlabel);
            }

            //assign to nearestneighbor
            for (int i = 0; i < N; i++)
            {
                if (shrinklabels.Contains(labels[i]))
                    slabels[i] = shrinklabelHash[labels[i]];
                else
                    slabels[i] = labels[i];
            }

            return slabels;
        }

        //label is ancestor's mapindex
        private static int[] GetLabel(ConstraintTree constree, int level)
        {
            IList<ConstraintTreeNode> leaves = constree.GetAllTreeLeaves();
            int N = leaves.Count;
            int[] labels = new int[N];

            ConstraintTreeNode[] nodebuffer = new ConstraintTreeNode[level];
            int bufferpt = 0;

            int intialindex = 0, mapnodecnt = 0;
            Dictionary<ConstraintTreeNode, int> mapIndex = new Dictionary<ConstraintTreeNode,int>();
            foreach (ConstraintTreeNode leaf in leaves)
            {
                ConstraintTreeNode node = leaf;
                while (node.Parent != null)
                {
                    nodebuffer[bufferpt] = node;
                    node = node.Parent;
                    bufferpt = (++bufferpt) % level;
                }

                node = nodebuffer[bufferpt];
                if (node == null) node = leaf;

                if (!mapIndex.ContainsKey(node))
                {
                    mapIndex.Add(node, mapnodecnt++);
                }
                labels[intialindex] = mapIndex[node];
                intialindex++;
            }

            return labels;
        }

    }
}
