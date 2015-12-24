using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Constants;

using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.Accuracy;
namespace EvolutionaryRoseTree.DataStructures
{
    /// <summary>
    /// To use this class, input lfv's sampleitems must be ordered
    /// </summary>
    class GroundTruthRoseTree : RuleRoseTree
    {
        public GroundTruthRoseTree(int dataset_index,                          //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET                             
            int algorithm_index,                        //BRT,KNN_BRT,SPILLTREE_BRT
            int experiment_index,                       //0 (ROSETREE_PRECISION)
            int random_projection_algorithm_index,      //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
            int model_index,                            //DCM,VMF,BERNOULLI
            int projectdimension,                       //projectdimensions[1]:50
            int k,                                      //k nearest neighbour
            LoadFeatureVectors lfv,                     //load feature vector
            double alpha, double gamma,                 //parameters, see top of this file
            double tau, double kappa, double R_0,       //parameters, see top of this file
            string outputpath) :
            base(dataset_index, algorithm_index, experiment_index, random_projection_algorithm_index, model_index, projectdimension, k, lfv, alpha, gamma, tau, kappa, R_0, outputpath)
        {
            this.algorithm_index = Constant.BRT;
        }

        protected GroundTruthMergeOrder GTMergeOrder;
        public static bool BBulidGroundTruthTree = true;

        public override void Run(
            int interval,               //Constant.intervals[0]:30
            out int depth,
            out double log_likelihood)
        {
            InitializeMergeOrder();
            mergedtreepointer = lfv.featurevectors.Length;

            if (BBulidGroundTruthTree)
            {
                CacheCacheClass();              //Cache log values as dictionaries
                //Initialize();                   //Initialize (random projection,) nodes, (spill tree)
                if (model_index == Constant.VMF)
                    InitializeNodesvMF();
                else
                    InitializeNodes();

                //CacheNearestNeighborsForAll();
                MergeLoop(interval);
                FindRoot();
                LabelTreeIndices(out depth);
                UpdateDepthInTree();

                this.spilltree = null;
                //this.cacheclass = null;

                log_likelihood = this.root.log_likelihood;

                //TestIfGroundTruthTreeCorrectlyBuilt();
            }
            else
            {
                depth = 4;
                log_likelihood = 0;
            }
        }

        public override void MergeLoop(int interval)
        {
            StreamWriter sw = InitializeMergeRecordWriter();

            while (clusternum > 1)
            {
                if (clusternum % 100 == 0)
                    Console.WriteLine("In the " + clusternum + "th cluster");

                RoseTreeNode node1, node2;

                int index0, index1;
                MergeType mergeType;
                GTMergeOrder.GetNextMergePair(out index0, out index1, out mergeType);

                int m = mergeType == MergeType.Join ? 0 : 1;
                node1 = nodearray[index0];
                node2 = nodearray[index1];

                /// Calculate logf and loglikelihood ///
                double log_likelihood_part1, log_likelihood_part2;
                double cache_valuearray_plus_alpha, log_likelihood_ratio;

                double logf = GetLogF(node1, node2, out cache_valuearray_plus_alpha);
                if (mergeType == MergeType.Join)
                    log_likelihood_ratio = node1.JoinLogLikelihood(this.cacheclass, node1, node2, logf, out log_likelihood_part1, out log_likelihood_part2) - (node1.log_likelihood + node2.log_likelihood);
                else  //AbsorbL
                    log_likelihood_ratio = node1.AbsorbLogLikelihood(this.cacheclass, node1, node2, logf, out log_likelihood_part1, out log_likelihood_part2) - (node1.log_likelihood + node2.log_likelihood);
                
                /// Merge single step ///
                OutputMergeRecord(sw, m, node1, node2, log_likelihood_ratio, log_likelihood_ratio);
                RoseTreeNode newnode = MergeSingleStep(node1, node2, m, log_likelihood_ratio + (node1.log_likelihood + node2.log_likelihood), logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2);
                UpdateLeafCount(newnode);            
            }

            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }

        public override void CacheNearestNeighbors(RoseTreeNode newnode, int[] nearestneighborlist)
        {
        }

        protected int mergedtreepointer = 0;
        public override RoseTreeNode MergeSingleStep(RoseTreeNode node1, RoseTreeNode node2, int m, double log_likelihood, double logf, double cache_valuearray_plus_alpha, double log_likelihood_part1, double log_likelihood_part2)
        {
            RoseTreeNode newnode = null;
            switch (m)
            {
                case 0: newnode = JoinMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2); break;
                case 1: newnode = AbsorbMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2); break;
                case 2: newnode = AbsorbMerge(node2, node1, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2); break;
                case 3: newnode = CollapseMerge(node1, node2, log_likelihood, logf, cache_valuearray_plus_alpha, log_likelihood_part1, log_likelihood_part2); break;
            }

            newnode.MergeTreeIndex = mergedtreepointer;
            mergedtreepointer++;
            
            return newnode;
        }

        private void InitializeMergeOrder()
        {
            if (this.lfv.dataset_index == Constant.NEW_YORK_TIMES)
                GTMergeOrder = new GroundTruthUnorderedLabelsMergeOrder(this.lfv);
            else
                GTMergeOrder = new GroundTruthMergeOrder(this.lfv);
        }

        public string LabelsCountToString()
        {
            return GTMergeOrder.LabelCountToString();
        }

        public int[] GetGroundTruthLabels(int level)
        {
            return GTMergeOrder.GetLabels(level);
        }

        private void TestIfGroundTruthTreeCorrectlyBuilt()
        {
            for (int level = 1; level < 3; level++)
            {
                int[] labeltree = LabelAccuracy.GetTreeLabel(this, level);
                int[] label = LabelAccuracy.GetLabel(this, level);
                double[,] confmat = ConfusionMatrix.GetConfuseMatrix(labeltree, label);
                double nmi = NMI.GetNormalizedMutualInfo(confmat, label.Length);
                if (nmi != 1)
                    throw new Exception("");
            }
        }
    }


    class GroundTruthMergeOrder
    {
        LoadFeatureVectors lfv;

        protected int[] labels0;  //first level labels
        protected int[] labels1;  //second level labels
        protected Dictionary<string, int> labelHash0;
        protected Dictionary<string, int> labelHash1;
        protected Dictionary<int, int> label1to0Hash;

        public GroundTruthMergeOrder(LoadFeatureVectors lfv)
        {
            this.lfv = lfv;

            InitializeLabels();
            if (GroundTruthRoseTree.BBulidGroundTruthTree)
                InitializeMergeData();
        }

        #region intialize
        protected virtual void InitializeLabels()
        {
            lfv.GetSampleLabels(out labels1, out labelHash1);

            int sampledNumber = labels1.Length;
            if (lfv.featurevectors.Length != sampledNumber)
                throw new Exception("Sample number not match!");


            labels0 = new int[sampledNumber];
            labelHash0 = new Dictionary<string, int>();
            label1to0Hash = new Dictionary<int, int>();
            int label0cnt = 0;
            foreach (string fulllabel in labelHash1.Keys)
            {
                string prefixlabel = fulllabel.Split('.')[0];
                if (!labelHash0.ContainsKey(prefixlabel))
                {
                    labelHash0.Add(prefixlabel, label0cnt);
                    label0cnt++;
                }
                label1to0Hash.Add(labelHash1[fulllabel], labelHash0[prefixlabel]);
            }
            //remove label1s that does not contain in sampled data
            HashSet<int> labellist = new HashSet<int>();
            for (int i = 0; i < sampledNumber; i++)
                labellist.Add(labels1[i]);
            Dictionary<int, int> label1to0Hashbuffer = new Dictionary<int, int>();
            foreach (int label1 in labellist)
                label1to0Hashbuffer.Add(label1, label1to0Hash[label1]);
            label1to0Hash = label1to0Hashbuffer;

            //initialize label0
            for (int i = 0; i < sampledNumber; i++)
                labels0[i] = label1to0Hash[labels1[i]];
        }

        private void InitializeMergeData()
        {
            this.N = labels0.Length;
            this.mergedindex = N;
            this.pointer = 0;

            SmallGroupMergeIndex = new List<int>();
            SmallGroupLabel0 = new List<int>();
            BigGroupMergeIndex = new List<int>();
        }
        #endregion intialize

        #region get next merge pair
        int N;
        List<int> SmallGroupMergeIndex;
        List<int> SmallGroupLabel0;
        List<int> BigGroupMergeIndex;
        int mergedindex;
        int pointer;
        int level = 0;
        //This needs labels to be "clustered"
        public virtual void GetNextMergePair(out int index0, out int index1, out MergeType mergeType)
        {
            index0 = index1 = -1;
            mergeType = MergeType.Collapse;
            switch (level)
            {
                case 0:
                    GetLevel0MergePair(out index0, out index1, out mergeType);
                    break;
                case 1:
                    GetLevel1MergePair(out index0, out index1, out mergeType);
                    break;
                case 2:
                    GetLevel2MergePair(out index0, out index1, out mergeType);
                    break;
                default:
                    index0 = index1 = -1;
                    break;
            }

            mergedindex++;
        }

        int mergedtreeindex = -1;
        private void GetLevel0MergePair(out int index0, out int index1, out MergeType mergeType)
        {
            if (pointer < N - 1 && labels1[pointer] == labels1[pointer + 1])
            {
                    if (mergedtreeindex < 0)
                    {
                        index0 = pointer;
                        index1 = pointer + 1;
                        mergeType = MergeType.Join;
                    }
                    else
                    {
                        index0 = mergedtreeindex;
                        index1 = pointer + 1;
                        mergeType = MergeType.AbsorbL;
                    }
                    mergedtreeindex = mergedindex;
                    pointer++;
                    return;
            }
            else
            {
                if (mergedtreeindex < 0)
                    SmallGroupMergeIndex.Add(pointer);
                else
                    SmallGroupMergeIndex.Add(mergedtreeindex);
                SmallGroupLabel0.Add(label1to0Hash[labels1[pointer]]);
                mergedtreeindex = -1;

                pointer++;
            }

            if (pointer < N)
                GetLevel0MergePair(out index0, out index1, out mergeType);
            else
            {
                level = 1;
                pointer = 0;
                N = SmallGroupMergeIndex.Count;
                mergedtreeindex = -1;
                GetLevel1MergePair(out index0, out index1, out mergeType);
            }
        }

        private void GetLevel1MergePair(out int index0, out int index1, out MergeType mergeType)
        {
            if (pointer < N - 1 && SmallGroupLabel0[pointer] == SmallGroupLabel0[pointer + 1])
            {
                if (mergedtreeindex < 0)
                {
                    index0 = SmallGroupMergeIndex[pointer];
                    index1 = SmallGroupMergeIndex[pointer + 1];
                    mergeType = MergeType.Join;
                }
                else
                {
                    index0 = mergedtreeindex;
                    index1 = SmallGroupMergeIndex[pointer + 1];
                    mergeType = MergeType.AbsorbL;
                }
                mergedtreeindex = mergedindex;
                pointer++;
                return;
            }
            else
            {
                if (mergedtreeindex < 0)
                    BigGroupMergeIndex.Add(SmallGroupMergeIndex[pointer]);
                else
                    BigGroupMergeIndex.Add(mergedtreeindex);

                mergedtreeindex = -1;
                pointer++;
            }

            if (pointer < N)
                GetLevel1MergePair(out index0, out index1, out mergeType);
            else
            {
                level = 2;
                pointer = 1;
                mergedtreeindex = BigGroupMergeIndex[0];
                //N = BigGroupMergeIndex.Count;
                GetLevel2MergePair(out index0, out index1, out mergeType);
            }
        }

        private void GetLevel2MergePair(out int index0, out int index1, out MergeType mergeType)
        {
            index0 = mergedtreeindex;
            index1 = BigGroupMergeIndex[pointer];
            if(pointer>1)
                mergeType = MergeType.AbsorbL;
            else
                mergeType = MergeType.Join;
            mergedtreeindex = mergedindex;
            pointer++;
        }
        #endregion get next merge pair

        public string LabelCountToString()
        {
            /// Count Labels ///
            int sampledNumber = labels1.Length;

            //cnt correspontding labels
            Dictionary<int, int> label0cntHash = new Dictionary<int, int>();
            for (int i = 0; i < sampledNumber; i++)
            {
                int label0 = labels0[i];

                if (label0cntHash.ContainsKey(label0))
                    label0cntHash[label0]++;
                else
                    label0cntHash.Add(label0, 1);
            }

            Dictionary<int, int> label1cntHash = new Dictionary<int, int>();
            for (int i = 0; i < sampledNumber; i++)
            {
                int label1 = labels1[i];

                if (label1cntHash.ContainsKey(label1))
                    label1cntHash[label1]++;
                else
                    label1cntHash.Add(label1, 1);
            }

            /// Output Results ///
            string str = "<Label Count>\n";
            foreach (int label in label0cntHash.Keys)
                str += label + ":" + label0cntHash[label] + "\t";
            str += "\n";
            foreach (int label in label1cntHash.Keys)
                str += label + ":" + label1cntHash[label] + "\t";
            //str += "<End Label Count>";

            return str;
        }

        public void GetLabel(int initialindex, out int level1, out int level2)
        {
            level1 = labels0[initialindex];
            level2 = labels1[initialindex];
        }

        public virtual int[] GetLabels(int level)
        {
            switch (level)
            {
                case 1:
                    return labels0;
                case 2:
                    return labels1;
                default:
                    throw new Exception("Can only deal with level0 & level1");
            }
        }

        public Dictionary<int, int> GetLevelHash(int level)
        {
            switch (level)
            {
                case 2:
                    return label1to0Hash;
                case 1:
                    Dictionary<int, int> hash = new Dictionary<int,int>();
                    foreach (int label0 in label1to0Hash.Values)
                        if(!hash.ContainsKey(label0))
                            hash.Add(label0, 0);
                    return hash;
                case 0:
                    hash = new Dictionary<int, int>();
                    hash.Add(0, 0);
                    return hash;
                default:
                    throw new Exception("Can only deal with level0 & level1");
            }
        }
    }

    class GroundTruthUnorderedLabelsMergeOrder : GroundTruthMergeOrder
    {
        public GroundTruthUnorderedLabelsMergeOrder(LoadFeatureVectors lfv)
            : base(lfv)
        {
        }

        int[] inilabels0;
        int[] inilabels1;
        int[] index2IniIndex;
        int N;
        protected override void InitializeLabels()
        {
            base.InitializeLabels();
            OrderAllLabels();
        }

        private void OrderAllLabels()
        {
            //record initial labels here
            N = labels0.Length;
            inilabels0 = new int[N];
            inilabels1 = new int[N];
            for (int i = 0; i < N; i++)
            {
                inilabels0[i] = labels0[i];
                inilabels1[i] = labels1[i];
            }
            index2IniIndex = new int[N];

            //sort labels0, labels1
            Dictionary<int, List<int>> label1cluster = new Dictionary<int,List<int>>();
            foreach (int label1 in labelHash1.Values)
                label1cluster.Add(label1, new List<int>());
            for (int ilabel = 0; ilabel < N; ilabel++)
            {
                label1cluster[labels1[ilabel]].Add(ilabel);
            }
            int index = 0;
            foreach (int label1 in label1cluster.Keys)
            {
                List<int> cluster = label1cluster[label1];
                if (cluster.Count == 0)
                    continue;
                int label0 = label1to0Hash[label1];
                foreach (int iniindex in cluster)
                {
                    index2IniIndex[index] = iniindex;
                    labels1[index] = label1;
                    labels0[index] = label0;
                    index++;
                }
            }
        }

        public override void GetNextMergePair(out int index0, out int index1, out MergeType mergeType)
        {
            base.GetNextMergePair(out index0, out index1, out mergeType);
            if (index0 < N)
                index0 = index2IniIndex[index0];
            if (index1 < N)
                index1 = index2IniIndex[index1];
        }

        public override int[] GetLabels(int level)
        {
            switch (level)
            {
                case 1:
                    return inilabels0;
                case 2:
                    return inilabels1;
                default:
                    throw new Exception("Can only deal with level0 & level1");
            }
        }
    }
}
