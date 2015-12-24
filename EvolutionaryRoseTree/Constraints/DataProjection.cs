using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.ReadData;

using EvolutionaryRoseTree.DataStructures;
using System.Diagnostics;
namespace EvolutionaryRoseTree.Constraints
{
    enum DataProjectionType
    {
        MaxSimilarityDocument, MaxSimilarityNode, MaxSimilarityInternalNode, MaxSimilaritySearchDown, MaxSimilarityNodeDepthWeighted, MaxSimilarityDocumentContentVector, //MaxSimilarityNewTopic, 
        RoseTreePredictionDocument, RoseTreePredictionNode,
        DataPredictionDocument, DataPredictionNode, DataPredictionInternalNode, DataPredictionSearchDown, DataPredictionNodeDepthWeighted
    };

    enum NodeProjectionType { Cousin, InCluster, Abandon };

    abstract class DataProjection
    {
        //public static double AbandonCosineThreshold = -1;//0
        //public static int AbandonTreeDepthThreshold = 10;//4
        //public static int DocumentSkipPickedCount = 2;
        //public static double NewTopicAlpha = 0;
        //public static double DocumentCutGain = 0;//1
        //public static double DocumentTolerateCosine = 0;//0.2

        public static double AbandonCosineThreshold = 0.3;//0
        public static int AbandonTreeDepthThreshold = 2;//10
        public static int DocumentSkipPickedCount = int.MaxValue; //2;
        public static double NewTopicAlpha = 1e-100;
        public static double NewTopicAlphaCosine = 1; //0.9;
        public static double DocumentCutGain = 0.8;//0.5
        public static double DocumentTolerateCosine = 0;//0
        public static double ClusterSizeWeight = 1;
        public virtual int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            projType = NodeProjectionType.Cousin;
            return -1;
        }
    }

    class MaxSimilarityDataProjection : DataProjection
    {
        RoseTree rosetree;
        IList<RoseTreeNode> rosetreenodes;
        protected Dictionary<RoseTreeNode, int> nodepickedcount;
        bool bConsiderInternalNode;
        bool bOnlyInternalNode;

        public MaxSimilarityDataProjection(RoseTree rosetree, bool bConsiderInternalNode,
            bool bOnlyInternalNode = false)
        {
            this.rosetree = rosetree;
            this.bConsiderInternalNode = bConsiderInternalNode;
            this.bOnlyInternalNode = bOnlyInternalNode;

            Initialize();
        }

        void Initialize()
        {
            if (bConsiderInternalNode)
            {
                if (bOnlyInternalNode)
                    this.rosetreenodes = rosetree.GetAllValidInternalTreeNodes();
                else
                    this.rosetreenodes = rosetree.GetAllValidTreeNodes();
            }
            else
            {
                this.rosetreenodes = rosetree.GetAllTreeLeaf();
            }

            nodepickedcount = new Dictionary<RoseTreeNode, int>();
            foreach (RoseTreeNode rtnode in rosetreenodes)
                nodepickedcount.Add(rtnode, 0);
        }

        public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            RoseTreeNode maxCosNode = null;
            double maxCosine = Double.MinValue;

            foreach (KeyValuePair<RoseTreeNode, int> kvp in nodepickedcount)
            {
                if (kvp.Value >= DocumentSkipPickedCount)
                    continue;
                double cosine = vector.Cosine(vector, kvp.Key.data);
                if (cosine > maxCosine)
                {
                    maxCosine = cosine;
                    maxCosNode = kvp.Key;
                }
            }

            if (maxCosNode == null)
            {
                foreach (KeyValuePair<RoseTreeNode, int> kvp in nodepickedcount)
                {
                    double cosine = vector.Cosine(vector, kvp.Key.data);
                    if (cosine > maxCosine)
                    {
                        maxCosine = cosine;
                        maxCosNode = kvp.Key;
                    }
                }
            }

            if (maxCosNode.tree_depth > AbandonTreeDepthThreshold ||
                maxCosine < AbandonCosineThreshold)
                projType = NodeProjectionType.Abandon;
            else
            {
                if (maxCosNode.children == null)
                {
                    projType = NodeProjectionType.Cousin;
                }
                else
                {
                    projType = NodeProjectionType.InCluster;
                    //projType = NodeProjectionType.Cousin;
                    //if (maxCosNode.tree_depth == 2 && bOnlyInternalNode)
                    //{
                    //    double maxChildCosine = double.MinValue;
                    //    RoseTreeNode maxChildCosNode = null;
                    //    foreach (RoseTreeNode child in maxCosNode.children)
                    //    {
                    //        double cosine = child.data.Cosine(child.data, vector);
                    //        if (cosine > maxChildCosine)
                    //        {
                    //            maxChildCosine = cosine;
                    //            maxChildCosNode = child;
                    //        }
                    //    }
                    //    if (maxChildCosine > maxCosine)
                    //        maxCosNode = maxChildCosNode;
                    //}
                }
                //Console.Write("{0}\t",maxCosNode.MergeTreeIndex);
                nodepickedcount[maxCosNode]++;
            }
            return maxCosNode.MergeTreeIndex <0 ? maxCosNode.indices.initial_index : maxCosNode.MergeTreeIndex;
        }
    }


    class MaxSimilarityContentVectorDataProjection : DataProjection
    {
        RoseTree rosetree;
        IList<RoseTreeNode> rosetreenodes;
        protected Dictionary<RoseTreeNode, int> nodepickedcount;

        public MaxSimilarityContentVectorDataProjection(RoseTree rosetree)
        {
            this.rosetree = rosetree;

            Initialize();
        }

        void Initialize()
        {
            this.rosetreenodes = rosetree.GetAllTreeLeaf();
            nodepickedcount = new Dictionary<RoseTreeNode, int>();
            foreach (RoseTreeNode rtnode in rosetreenodes)
                nodepickedcount.Add(rtnode, 0);
        }

        public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            RoseTreeNode maxCosNode = null;
            double maxCosine = Double.MinValue;

            foreach (KeyValuePair<RoseTreeNode, int> kvp in nodepickedcount)
            {
                if (kvp.Value >= DocumentSkipPickedCount)
                    continue;
                double cosine = ContentVectorCosine(vector, kvp.Key.data);
                //Trace.WriteLine(string.Format("ContentVector Cosine: {0}, Org Cosine: {1}", cosine, vector.Cosine(vector, kvp.Key.data)));
                if (cosine > maxCosine)
                {
                    maxCosine = cosine;
                    maxCosNode = kvp.Key;
                }
            }

            if (maxCosNode == null)
            {
                foreach (KeyValuePair<RoseTreeNode, int> kvp in nodepickedcount)
                {
                    double cosine = ContentVectorCosine(vector, kvp.Key.data);
                    if (cosine > maxCosine)
                    {
                        maxCosine = cosine;
                        maxCosNode = kvp.Key;
                    }
                }
            }

            if (maxCosNode.tree_depth > AbandonTreeDepthThreshold ||
                maxCosine < AbandonCosineThreshold)
                projType = NodeProjectionType.Abandon;
            else
            {
                if (maxCosNode.children == null)
                {
                    projType = NodeProjectionType.Cousin;
                }
                else
                {
                    projType = NodeProjectionType.InCluster;
                }
                nodepickedcount[maxCosNode]++;
                //PrintProjectedNodePair(vector, maxCosNode.data);
                //Trace.WriteLine(maxCosine);
            }
            return maxCosNode.MergeTreeIndex < 0 ? maxCosNode.indices.initial_index : maxCosNode.MergeTreeIndex;
        }

        private void PrintProjectedNodePair(SparseVectorList v1, SparseVectorList v2)
        {
            var keywords1 = GetTopKeyWords(v1);
            var keywords2 = GetTopKeyWords(v2);
            //foreach (var keyword in keywords1)
            //    Trace.Write(keyword + ",");
            //Trace.Write(" --- ");
            //foreach (var keyword in keywords2)
            //    Trace.Write(keyword + ",");
            //Trace.WriteLine("");
        }

        private List<string> GetTopKeyWords(SparseVectorList v)
        {
            int sizeofprintlist = 5;
            MinHeapInt mh = new MinHeapInt(sizeofprintlist);
            for (int j = 0; j < sizeofprintlist; j++)
                mh.insert(-1, int.MinValue);

            for (int j = 0; j < v.contentvectorlen; j++)
            {
                if (v.valuearray[j] > mh.min())
                    mh.changeMin(v.keyarray[j], v.valuearray[j]);
            }
            MinHeapInt.heapSort(mh);
            List<string> topkeywords = new List<string>();
            var indices = mh.getIndices();
            for (int i = 0; i < indices.Length; i++)
                if (indices[i] >= 0)
                    topkeywords.Add(rosetree.lfv.invertlexicon[indices[i]]);
            return topkeywords;
        }

        public static double ContentVectorCosine(SparseVectorList v1, SparseVectorList v2)
        {
            double cosine;
            //if (v1.contentvectorlen > v2.contentvectorlen)
            //{
            //    cosine = ContentVectorCosine(v2, v1);
            //}
            //else
            {
                long t = DateTime.Now.Ticks;
                var norm1 = GetContentVectorNorm(v1);
                var norm2 = GetContentVectorNorm(v2);
                cosine = ContentVectorDotProduct(v1, v2) / norm1 / norm2;
            }
            return cosine;
        }

        public static double ContentVectorDotProduct(SparseVectorList v1, SparseVectorList v2)
        {
            if (v1.contentvectorlen > v2.contentvectorlen)
                return ContentVectorDotProduct(v2, v1);

            int pt1 = 0;
            int pt2 = 0;
            int length1 = v1.contentvectorlen;
            int length2 = v2.contentvectorlen;
            double ret = 0;
            int[] keys1 = v1.keyarray;
            int[] values1 = v1.valuearray;
            int[] keys2 = v2.keyarray;
            int[] values2 = v2.valuearray;

            while (true)
            {
                while (pt1 < length1 && keys1[pt1] < keys2[pt2]) pt1++;
                if (pt1 == length1) break;
                if (keys1[pt1] == keys2[pt2])
                {
                    ret += (double)values1[pt1] * values2[pt2];
                    pt1++;
                    pt2++;
                }
                else
                {
                    while (pt2 < length2 && keys2[pt2] < keys1[pt1]) pt2++;
                    if (pt2 == length2) break;
                    if (keys2[pt2] == keys1[pt1])
                    {
                        ret += (double)values1[pt1] * values2[pt2];
                        pt1++;
                        pt2++;
                    }
                }
                if (pt1 == length1 || pt2 == length2) break;
            }

            return ret;
        }

        private static double GetContentVectorNorm(SparseVectorList v)
        {
            if (v.contentvectornorm < 0)
            {
                double sum = 0;
                for (int i = 0; i < v.contentvectorlen; i++)
                {
                    sum += (double)v.valuearray[i] * v.valuearray[i];
                }
                v.contentvectornorm = Math.Sqrt(sum);

                if (double.IsNaN(v.contentvectornorm))
                {
                    double factor = v.contentvectorlen * v.contentvectorlen;
                    sum = 0;
                    for (int i = 0; i < v.contentvectorlen; i++)
                    {
                        sum += (double)v.valuearray[i] * v.valuearray[i] / factor;
                    }
                    v.contentvectornorm = Math.Sqrt(sum * factor);
                    if (double.IsNaN(v.contentvectorlen))
                    {
                        Trace.WriteLine("Error! IsNaN for GetContentVectorNorm!");
                        throw new Exception("Error! IsNaN for GetContentVectorNorm!");
                    }
                }
            }
            return v.contentvectornorm;
        }
    }

    class MaxSimilarityDataProjectionDepthWeighted : MaxSimilarityDataProjection
    {
        public MaxSimilarityDataProjectionDepthWeighted(RoseTree rosetree, bool bConsiderInternalNode,
            bool bOnlyInternalNode = false) :
            base(rosetree, bConsiderInternalNode, bOnlyInternalNode)
        {
        }

        public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            RoseTreeNode maxCosNode = null;
            double maxCosine = Double.MinValue;

            foreach (KeyValuePair<RoseTreeNode, int> kvp in nodepickedcount)
            {
                if (kvp.Value >= DocumentSkipPickedCount)
                    continue;
                double cosine = vector.Cosine(vector, kvp.Key.data) / Math.Pow(NewTopicAlphaCosine, kvp.Key.DepthInTree);
                if (cosine > maxCosine)
                {
                    maxCosine = cosine;
                    maxCosNode = kvp.Key;
                }
            }

            maxCosine *= Math.Pow(NewTopicAlphaCosine, maxCosNode.DepthInTree);
            if (maxCosNode.tree_depth > AbandonTreeDepthThreshold ||
                maxCosine < AbandonCosineThreshold)
                projType = NodeProjectionType.Abandon;
            else
            {
                if (maxCosNode.children == null)
                {
                    projType = NodeProjectionType.Cousin;
                }
                else
                {
                    projType = NodeProjectionType.InCluster;
                }
            }

            ////test
            //NodeProjectionType baseprojType;
            //int projindex = maxCosNode.MergeTreeIndex < 0 ? maxCosNode.indices.initial_index : maxCosNode.MergeTreeIndex;
            //int baseprojindex = base.GetProjectedArrayIndex(vector, out baseprojType);
            //if (baseprojindex != projindex || baseprojType != projType)
            //    Console.WriteLine("Error! proj result not the same with base class!");


            return maxCosNode.MergeTreeIndex < 0 ? maxCosNode.indices.initial_index : maxCosNode.MergeTreeIndex;

        }
    }

    class MaxSimilaritySearchDown : DataProjection
    {
        RoseTree rosetree;
        //IList<RoseTreeNode> rosetreenodes;

        public MaxSimilaritySearchDown(RoseTree rosetree)
        {
            this.rosetree = rosetree;
            //Initialize();
        }

        //void Initialize()
        //{
        //    //this.rosetreenodes = rosetree.GetAllValidTreeNodes();
        //}

        public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            RoseTreeNode rtnode = rosetree.root;
            double maxCos = double.MinValue;
            RoseTreeNode maxRtnode = null;
            while (rtnode.children != null)
            {
                double maxCos2;
                rtnode = GetMaxCosineRoseTreeNode(vector, rtnode.children, out maxCos2);
                if (maxCos2 >= maxCos * NewTopicAlphaCosine)
                {
                    maxRtnode = rtnode;
                    maxCos = maxCos2;
                }
                else
                    break;
            }

            if (rtnode.children == null)
                projType = NodeProjectionType.Cousin;
            else
                projType = NodeProjectionType.InCluster;
            return rtnode.indices.array_index;
        }

        public RoseTreeNode GetMaxCosineRoseTreeNode(SparseVectorList vector, RoseTreeNode[] rtnodearray, out double maxCos)
        {
            maxCos = double.MinValue;
            RoseTreeNode maxCosNode = null;
            foreach (RoseTreeNode rtnode in rtnodearray)
            {
                double cos = vector.Cosine(vector, rtnode.data);
                if (cos > maxCos)
                {
                    maxCos = cos;
                    maxCosNode = rtnode;
                }
            }
            return maxCosNode;
        }
    }


    //class MaxSimilarityNewTopic : DataProjection
    //{
    //    RoseTree rosetree;
    //    IList<RoseTreeNode> rosetreenodes;

    //    public MaxSimilarityNewTopic(RoseTree rosetree, double newTopicCosine)
    //    {
    //        this.rosetree = rosetree;

    //        Initialize();
    //    }

    //    void Initialize()
    //    {
    //        this.rosetreenodes = rosetree.GetAllValidTreeNodes();
    //    }

    //    public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
    //    {
    //        int maxIndex = -1;
    //        double maxCosine = Double.MinValue;

    //        foreach (RoseTreeNode rtnode in rosetreenodes)
    //        {
    //            double cosine = vector.Cosine(vector, rtnode.data);
    //            if (cosine > maxCosine)
    //            {
    //                maxCosine = cosine;
    //                maxIndex = rtnode.indices.array_index;
    //            }
    //        }

    //        projType = NodeProjectionType.Cousin;
    //        return maxIndex;
    //    }
    //}

    class RoseTreePredictionDataProjection : DataProjection
    {
        protected RoseTree rosetree;
        protected LoadFeatureVectors lfv;
        protected IList<RoseTreeNode> rosetreenodes;
        protected CacheClass cacheclass;
        protected NodeCacheData[] nodecachedatas;
        protected bool bConsiderInternalNode;
        protected bool bOnlyDataPrediction;
        protected bool bOnlyInternalNode;
#if WRITE_PROJECTION_CONTENT
        static int ofileIndex = 0;
        public StreamWriter ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\dataprojection" + ofileIndex++ + ".dat");
        static string doubleFormatString = "{0,10:0.00}"; 
#endif

        public RoseTreePredictionDataProjection(RoseTree rosetree,
            LoadFeatureVectors lfv, bool bConsiderInternalNode, bool bOnlyDataPrediction = false,
            bool bOnlyInternalNode = false)
        {
            this.rosetree = rosetree;
            this.lfv = lfv;
            this.bConsiderInternalNode = bConsiderInternalNode;
            this.bOnlyDataPrediction = bOnlyDataPrediction;
            this.bOnlyInternalNode = bOnlyInternalNode;

            Initialize();
        }

        void Initialize()
        {
            if (bConsiderInternalNode)
            {
                if (bOnlyInternalNode)
                    this.rosetreenodes = rosetree.GetAllValidInternalTreeNodes();
                else
                    this.rosetreenodes = rosetree.GetAllValidTreeNodes();
            }
            else
            {
                this.rosetreenodes = rosetree.GetAllTreeLeaf();
            }

            //rosetreenodes = rosetree.GetAllValidInternalTreeNodes();
            nodecachedatas = new NodeCacheData[2 * rosetree.lfv.featurevectors.Length];
            cacheclass = rosetree.GetCacheClass();
            /// Expand cache class ///
            if (lfv.lexiconsize != rosetree.lfv.lexiconsize)
                throw new Exception("Error in prediction: set standard granularity to larger value!");
            cacheclass = new ExpandedCacheClass(cacheclass);

            if (bOnlyDataPrediction)
            {
                InitializeOnlyDataPrediction();
                return;
            }

            /// Calculate all Ws using width-first trasversal ///
            List<RoseTreeNode> queue = new List<RoseTreeNode>();
            queue.Add(rosetree.root);
            //cache root
            NodeCacheData cachedata = new NodeCacheData();
            cachedata.treelevel = 0; cachedata.log_cachevalue = 0;//log(1)
            nodecachedatas[rosetree.root.indices.array_index] = cachedata;
            while (queue.Count != 0)
            {
                //pop
                RoseTreeNode n = queue[0];
                queue.RemoveAt(0);
                cachedata = nodecachedatas[n.indices.array_index];
                if (n.children != null)
                {
                    //calculate w_s for n
                    cachedata.log_r = cacheclass.GetLogPi(n.children.Length) //log_pi
                        + n.cache_nodevalues.logf - n.log_likelihood;
                    //if(cachedata.log_r > Math.Log(0.9999))
                    //    Console.WriteLine(n.MergeTreeIndex);
                    //cachedata.log_r = cacheclass.GetLogPi(n.chilsdren.Length); //log_pi
                    cachedata.log_ws = cachedata.log_r + cachedata.log_cachevalue - cachedata.treelevel * Math.Log(n.LeafCount);
                    //cache data for children
                    double logoneminusr = n.cache_nodevalues.log_likelihood_part2 - n.log_likelihood;
                    //double logoneminusr1 = Math.Log(1 - Math.Exp(cachedata.log_r));
                    //if (logoneminusr1 > -2)
                    //{
                    //    double logoneminusr2 = GetTylorSeriesLogOneMinusR(cachedata.log_r);
                    //    if (Math.Abs(logoneminusr1 - logoneminusr2) > 1e-10)
                    //    {
                    //        //throw new Exception("Numeric Problem!");
                    //        Console.WriteLine("Numeric Problem! Difference: {0}", Math.Abs(logoneminusr1 - logoneminusr2));
                    //        logoneminusr1 = logoneminusr2;
                    //    }
                    //}
                    double cachevalue_part1 = cachedata.log_cachevalue + logoneminusr;
                    foreach (RoseTreeNode c in n.children)
                    {
                        NodeCacheData childcachedata = new NodeCacheData();
                        nodecachedatas[c.indices.array_index] = childcachedata;
                        childcachedata.log_cachevalue = cachevalue_part1 + Math.Log(c.LeafCount);
                        childcachedata.treelevel = cachedata.treelevel + 1;
                        queue.Add(c);
                    }
                }
                else
                {
                    cachedata.log_r = 0; //log(1)
                    cachedata.log_ws = cachedata.log_r + cachedata.log_cachevalue;
                }
                cachedata.log_prediction_spart = cachedata.log_ws -
                    (n.cache_nodevalues.logf - n.cache_nodevalues.logf_part1);
            }

            //PrintAllCacheData();
        }

        // Do not include w_s, return only p(x|D_s)
        void InitializeOnlyDataPrediction()
        {
            foreach (RoseTreeNode node in rosetreenodes)
            {
                NodeCacheData cachedata = new NodeCacheData();
                cachedata.log_prediction_spart = -(node.cache_nodevalues.logf - node.cache_nodevalues.logf_part1);
                nodecachedatas[node.indices.array_index] = cachedata;
            }
        }

        private double GetTylorSeriesLogOneMinusR(double log_r)
        {
            double r = Math.Exp(log_r);
            double r_exp = r;
            double res = 0;
            int index = 1;
            while (r_exp >= 1e-10)
            {
                res -= r_exp / index;
                index++;
                r_exp *= r;
            }
            return res;
        }

        public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            //try
            {
                RoseTreeNode maxRtNode = null;
                double maxProb = Double.MinValue;

                foreach (RoseTreeNode rtnode in rosetreenodes)
                {
                    double predictionProb = GetPredictionProb(vector, rtnode);
                    if (predictionProb > maxProb)
                    {
                        maxProb = predictionProb;
                        maxRtNode = rtnode;
                    }
                }

                //if (maxRtNode.children == null)
                //    projType = NodeProjectionType.Cousin;
                //else
                //    projType = NodeProjectionType.InCluster;]
                if (maxRtNode.tree_depth > AbandonTreeDepthThreshold ||
                    vector.Cosine(vector, maxRtNode.data) < AbandonCosineThreshold)
                    projType = NodeProjectionType.Abandon;
                else
                {
                    projType = (maxRtNode.children == null) ? NodeProjectionType.Cousin : NodeProjectionType.InCluster;
                }
                return maxRtNode.indices.array_index;
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return -1;
            //}
        }

        protected double GetPredictionProb(SparseVectorList vector, RoseTreeNode rtnode)
        {
            NodeCacheData cachedata = nodecachedatas[rtnode.indices.array_index];

            //try
            {
                //Calculate delta((alpha+sum(s_i^j))+x)
                double delta_s_x = 0;
                delta_s_x -= cacheclass.GetLogAlphaSumItem(rtnode.data.valuearray_sum + vector.valuearray_sum);

                List<int> overlapping_keylist;
                int vectorlength;
                SparseVectorList addvector = SparseVectorList.TryAddValue(rtnode.data, vector, out overlapping_keylist, out vectorlength);
                int[] valuearray = addvector.valuearray;
                for (int i = 0; i < vectorlength; i++)
                    delta_s_x += cacheclass.GetLogAlphaItem(valuearray[i]);
#if WRITE_PROJECTION_CONTENT
                ofile.WriteLine("({0})\t{1}\t{2}\t{3}",
                    string.Format("{0,3}", rtnode.indices.array_index),
                    string.Format(doubleFormatString, cachedata.log_ws),
                    string.Format(doubleFormatString, cachedata.log_prediction_spart - cachedata.log_ws + delta_s_x),
                    string.Format(doubleFormatString, cachedata.log_prediction_spart + delta_s_x));
                ofile.Flush();
#endif
                return cachedata.log_prediction_spart + delta_s_x;
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    //Console.WriteLine("Error calculating prediction prob for x,s");
            //    return double.MinValue;
            //}
        }

        protected double GetVectorLogFPart1(SparseVectorList vector)
        {
            double logf_part1 = cacheclass.GetLogFactorials(vector.valuearray_sum);
            double part = 0;
            for (int j = 0; j < vector.count; j++)
                part += cacheclass.GetLogFactorials(vector.valuearray[j]);
            logf_part1 -= part;
            return logf_part1;
        }

        private NodeCacheData GetCacheDataByArrayIndex(int arrayIndex)
        {
            return nodecachedatas[arrayIndex];
        }

        private void PrintAllCacheData()
        {
#if WRITE_PROJECTION_CONTENT
            ofile.WriteLine("|||||||||Start Print All Cache Data|||||||||");
            ofile.WriteLine("index\ttreelevel\tcachevalue\t\tr\t\t\tws\t\tspart\t\t\tloglike_p1\t\tloglike_p2");
            for (int i = 0; i < nodecachedatas.Length; i++)
            {
                NodeCacheData cachedata = nodecachedatas[i];
                RoseTreeNode rtnode = rosetree.GetNodeByArrayIndex(i);
                if (cachedata != null)
                    ofile.WriteLine("{0}\t\t\t{1}\t{2}\t{3}\t{4}\t{5}\t\t\t{6}\t{7}",
                        i, cachedata.treelevel,
                        string.Format(doubleFormatString, cachedata.log_cachevalue),
                        string.Format(doubleFormatString, cachedata.log_r),
                        string.Format(doubleFormatString, cachedata.log_ws),
                        string.Format(doubleFormatString, cachedata.log_prediction_spart),
                        string.Format(doubleFormatString, rtnode.cache_nodevalues.log_likelihood_part1),
                        string.Format(doubleFormatString, rtnode.cache_nodevalues.log_likelihood_part2));
            }
            ofile.WriteLine("|||||||||End Print All Cache Data|||||||||");
#endif
        }

        protected RoseTreeNode SeekBestMatchDocument(RoseTreeNode rtnode, SparseVectorList vector,
   double documentCutGain, double documentTolerateCosine)
        {
            if (rtnode.children == null)
                return rtnode;

            RoseTreeNode maxCosineDocument = null;
            double maxCosine = -1;
            List<RoseTreeNode> queue = new List<RoseTreeNode>();
            queue.Add(rtnode);
            while (queue.Count != 0)
            {
                RoseTreeNode node = queue[0];
                queue.RemoveAt(0);

                foreach (RoseTreeNode child in node.children)
                {
                    if (child.children == null)
                    {
                        double cosine = vector.Cosine(vector, child.data);
                        if (cosine > maxCosine)
                        {
                            maxCosine = cosine;
                            maxCosineDocument = child;
                        }
                    }
                    else
                        queue.Add(child);
                }
            }

            double tolerateCosine = documentCutGain * vector.Cosine(vector, rtnode.data);
            tolerateCosine = Math.Max(documentTolerateCosine, tolerateCosine);
            if (maxCosine >= tolerateCosine)
                return maxCosineDocument;
            else
                return rtnode;
        }

        protected class NodeCacheData
        {
            public int treelevel;
            public double log_cachevalue;
            public double log_r;
            public double log_ws;
            public double log_prediction_spart;
        }
    }

    class RoseTreePredictionDataProjectionDepthWeighted : RoseTreePredictionDataProjection
    {
        public RoseTreePredictionDataProjectionDepthWeighted(RoseTree rosetree,
            LoadFeatureVectors lfv, bool bConsiderInternalNode, bool bOnlyDataPrediction = false,
            bool bOnlyInternalNode = false):
            base(rosetree, lfv, bConsiderInternalNode, bOnlyDataPrediction, bOnlyInternalNode)
        {
        }

        public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            double lognewalpha = Math.Log(NewTopicAlpha);
            RoseTreeNode maxRtNode = null;
            double maxProb = Double.MinValue;

            foreach (RoseTreeNode rtnode in rosetreenodes)
            {
                double predictionProb = GetPredictionProb(vector, rtnode) - lognewalpha * rtnode.DepthInTree;
                if (predictionProb > maxProb)
                {
                    maxProb = predictionProb;
                    maxRtNode = rtnode;
                }
            }

            //if (maxRtNode.children == null)
            //    projType = NodeProjectionType.Cousin;
            //else
            //    projType = NodeProjectionType.InCluster;]

            //maxRtNode = SeekBestMatchDocument(maxRtNode, vector, DocumentCutGain, DocumentTolerateCosine);
            
            if (maxRtNode.tree_depth > AbandonTreeDepthThreshold ||
                vector.Cosine(vector, maxRtNode.data) < AbandonCosineThreshold)
                projType = NodeProjectionType.Abandon;
            else
            {
                projType = (maxRtNode.children == null) ? NodeProjectionType.Cousin : NodeProjectionType.InCluster;
            }
            return maxRtNode.indices.array_index;
        }
    }

    class RoseTreePredictionSearchDown : RoseTreePredictionDataProjection
    {
        public RoseTreePredictionSearchDown(RoseTree rosetree,
            LoadFeatureVectors lfv)
            : base(rosetree, lfv, true, true)
        {
        }

        /// <summary>
        ///  Slice 21 http://www.stats.ox.ac.uk/~teh/teaching/npbayes/toronto2009.pdf
        /// </summary>
        //static StreamWriter ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\DPSearchDown.dat");
        static int vectorIndex = 0;
        public override int GetProjectedArrayIndex(SparseVectorList vector, out NodeProjectionType projType)
        {
            //if (vectorIndex == 1877)
            //    Console.Write("");
            //ofile.WriteLine("\n------------------------------");
            //ofile.WriteLine("Vector [{0}]", vectorIndex);
            double logf_part1 = GetVectorLogFPart1(vector);

            RoseTreeNode rtnode = rosetree.root;
            RoseTreeNode maxProbNode = rosetree.root;
            double maxProb = GetPredictionProb(vector, rtnode);
            double log_alpha = Math.Log(NewTopicAlpha);
            //ofile.WriteLine("[Max] (w){0}, {1}, {2}", maxProb + log_alpha, maxProb + logf_part1, maxProbNode.indices.array_index);
            while (rtnode.children != null)
            {
                RoseTreeNode children_maxProbNode;
                double children_maxProb;
                double children_maxWeightedProb = GetMaxWeightProb(vector, rtnode.children, out children_maxProbNode, out children_maxProb);

                if (children_maxWeightedProb > maxProb + log_alpha) //compare weighted prob
                {
                    maxProb = children_maxProb;
                    maxProbNode = children_maxProbNode;
                    rtnode = children_maxProbNode;
                }
                else
                    break;
                //ofile.WriteLine("\n[Max] (w){0}|{1}, {2}, {3}", children_maxWeightedProb, maxProb + log_alpha, maxProb + logf_part1, maxProbNode.indices.array_index);
            }

            //ofile.Flush();
            vectorIndex++;

#if PROJ_SEEK_BEST_MATCH_DOC
            maxProbNode = SeekBestMatchDocument(maxProbNode, vector, DocumentCutGain, DocumentTolerateCosine);
#endif
            if (maxProbNode.tree_depth > AbandonTreeDepthThreshold ||
                vector.Cosine(vector, maxProbNode.data) < AbandonCosineThreshold)
            {
                //have to match to document because topic nodes may disappear
                maxProbNode = SeekBestMatchDocument(maxProbNode, vector, 0, 0);
                projType = NodeProjectionType.Abandon;
            }
            else
            {
                projType = (maxProbNode.children == null) ? NodeProjectionType.Cousin : NodeProjectionType.InCluster;
            }

            return maxProbNode.indices.array_index;
        }       

        private double GetMaxWeightProb(SparseVectorList vector, RoseTreeNode[] roseTreeNodes,
            out RoseTreeNode maxProbNode, out double maxProbPredictionProb)
        {
            maxProbNode = null;
            double maxProb = double.MinValue;
            maxProbPredictionProb = double.MinValue;

            foreach (RoseTreeNode rtnode in roseTreeNodes)
            {
                double predprob = GetPredictionProb(vector, rtnode);
                double prob = predprob + ClusterSizeWeight * Math.Log(rtnode.LeafCount);
                //ofile.Write("{0}\t", prob);
                if (prob > maxProb)
                {
                    maxProb = prob;
                    maxProbPredictionProb = predprob;
                    maxProbNode = rtnode;
                }
            }
            return maxProb;
        }

        public void PrintAllCosinePredValue(SparseVectorList[] vectors)
        {
            StreamWriter ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\TestLeavesPredCosine.dat");
            IList<RoseTreeNode> leaves = rosetree.GetAllTreeLeaf();
            foreach(SparseVectorList vector in vectors)
            {
                double part1 = GetVectorLogFPart1(vector);
                foreach (RoseTreeNode rtnode in leaves)
                    ofile.WriteLine("{0}\t{1}\t{2}\t{3}", part1 + GetPredictionProb(vector, rtnode), vector.Cosine(vector, rtnode.data), vector.count, rtnode.data.count);
            }
            ofile.Flush();
            ofile.Close();

            ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\TestAllNodesPredCosine.dat");
            IList<RoseTreeNode> rtnodes = rosetree.GetAllValidTreeNodes();
            foreach (SparseVectorList vector in vectors)
            {
                double part1 = GetVectorLogFPart1(vector);
                foreach (RoseTreeNode rtnode in rtnodes)
                    ofile.WriteLine("{0}\t{1}\t{2}\t{3}", part1 + GetPredictionProb(vector, rtnode), vector.Cosine(vector, rtnode.data), vector.count, rtnode.data.count);
            }
            ofile.Flush();
            ofile.Close();
        }

        //public double GetWholeDataPredictionProb(SparseVectorList vector, RoseTreeNode rtnode)
        //{
        //    return GetPredictionProb(vector, rtnode) + GetVectorLogFPart1(vector);
        //}

        #region previous
    //    public int GetProjectedArrayIndex_Previous(SparseVectorList vector, out bool bCousin)
    //    {
    //        //ofile.WriteLine("------------------------------");
    //        //ofile.WriteLine("Vector [{0}]", vectorIndex);
    //        double logf_part1 = GetVectorLogFPart1(vector);

    //        RoseTreeNode rtnode = rosetree.root;
    //        RoseTreeNode maxProbNode = rosetree.root;
    //        double maxProb = GetPredictionProb(vector, rtnode);
    //        //ofile.WriteLine("[Max] {0}, {1}", maxProb + logf_part1, maxProbNode.indices.array_index);
    //        while (rtnode.children != null)
    //        {
    //            RoseTreeNode children_maxProbNode;
    //            double children_maxProb = GetMaxProb_Previous(vector, rtnode.children, out children_maxProbNode);

    //            rtnode = children_maxProbNode;
    //            if (children_maxProb > maxProb)
    //            {
    //                maxProb = children_maxProb;
    //                maxProbNode = children_maxProbNode;
    //            }
    //            //ofile.WriteLine("\n[Max] {0}, {1}", maxProb + logf_part1, maxProbNode.indices.array_index);
    //        }

    //        //ofile.Flush();
    //        vectorIndex++;
    //        bCousin = (maxProbNode.children == null);
    //        return maxProbNode.indices.array_index;
    //    }

    //    private double GetMaxProb_Previous(SparseVectorList vector, RoseTreeNode[] roseTreeNodes,
    //out RoseTreeNode maxProbNode)
    //    {
    //        maxProbNode = null;
    //        double maxProb = double.MinValue;

    //        foreach (RoseTreeNode rtnode in roseTreeNodes)
    //        {
    //            double prob = GetPredictionProb(vector, rtnode);
    //            //ofile.Write(prob + "\t");
    //            if (prob > maxProb)
    //            {
    //                maxProb = prob;
    //                maxProbNode = rtnode;
    //            }
    //        }
    //        return maxProb;
    //    }
        #endregion
    }

    class SucceedDataProjection : DataProjection
    {
        DataProjectionRelation dataProjRelation;
        int docCnt;
        public SucceedDataProjection(DataProjectionRelation projrelation)
        {
            //if (projrelation == null)
            //    Console.Write("");
            dataProjRelation = projrelation;
            docCnt = projrelation.PrevDocCnt;
        }

        public int GetProjectedArrayIndex(int vectorId, out NodeProjectionType projType)
        {
            bool isfree = dataProjRelation.IsFreenode[vectorId];
            int nnarrayindex = dataProjRelation.NearestNeighbourIndex[vectorId];
            if (isfree)
                projType = NodeProjectionType.Abandon;
            else
            {
                if (nnarrayindex < docCnt)
                    projType = NodeProjectionType.Cousin;
                else
                    projType = NodeProjectionType.InCluster;
            }
            return nnarrayindex;
        }
    }
}
