using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.Constants;
namespace EvolutionaryRoseTree.DataStructures
{
    class MergedLoadGlobalFeatureVectors : LoadGlobalFeatureVectors
    {
        List<LoadGlobalFeatureVectors> lfvs;
        public MergedLoadGlobalFeatureVectors(List<LoadGlobalFeatureVectors> lfvs):
            base(lfvs[0].dataset_index, lfvs[0].dataset_index)
        {
            this.lfvs = lfvs;
            
            Merge();
        }

        private void Merge()
        {
            MergeFeatureVectors();
            MergeSampleLabels();
            MergeSampleDocIds();
            MergeSampleLines();

            MergeWordFrequencyCount();
            MergeWordAppearanceCount();
            this.lexicon = lfvs[0].lexicon;
            this.invertlexicon = lfvs[0].invertlexicon;

            foreach (LoadFeatureVectors lfv in lfvs)
            {
                samplenum += lfv.samplenum;
                wordnum += lfv.wordnum;
                featurevectorsnum += lfv.featurevectorsnum;
            }
            lexiconsize = this.lexicon.Count;
            UpdateMaxdimensionValue();

            if (model_index == Constant.VMF)
                ComputeIDF();

            lfvs = null; 
        }

        private void UpdateMaxdimensionValue()
        {
            maxdimensionvalue = 0;
            foreach (KeyValuePair<int, int> kvp in wordfrequencycount)
            {
                if (kvp.Value > maxdimensionvalue)
                    maxdimensionvalue = kvp.Value;
            }
        }

        private void MergeWordFrequencyCount()
        {
            foreach (LoadFeatureVectors lfv in lfvs)
            {
                Dictionary<int, int> lfv_wordfrequencycount = lfv.wordfrequencycount;
                foreach (KeyValuePair<int, int> kvp in lfv_wordfrequencycount)
                    if (wordfrequencycount.ContainsKey(kvp.Key))
                        wordfrequencycount[kvp.Key] += kvp.Value;
                    else
                        wordfrequencycount.Add(kvp.Key, kvp.Value);
            }
        }
        private void MergeWordAppearanceCount()
        {
            foreach (LoadFeatureVectors lfv in lfvs)
            {
                Dictionary<int, int> lfv_wordappearancecount = lfv.wordappearancecount;
                foreach (KeyValuePair<int, int> kvp in lfv_wordappearancecount)
                    if (wordappearancecount.ContainsKey(kvp.Key))
                        wordappearancecount[kvp.Key] += kvp.Value;
                    else
                        wordappearancecount.Add(kvp.Key, kvp.Value);
            }
        }


        private void MergeFeatureVectors()
        {
            List<SparseVectorList> featurevectorsList = new List<SparseVectorList>();
            foreach (LoadFeatureVectors lfv in lfvs)
            {
                featurevectorsList.AddRange(lfv.featurevectors);
            }

            this.featurevectors = featurevectorsList.ToArray<SparseVectorList>();
        }

        private void MergeSampleLabels()
        {
            try
            {
                List<int> samplelabelsList = new List<int>();
                this.labelHash = new Dictionary<string, int>();
                foreach (LoadFeatureVectors lfv in lfvs)
                {
                    int[] lfv_samplelabels;
                    Dictionary<string, int> lfv_labelHash;
                    lfv.GetSampleLabels(out lfv_samplelabels, out lfv_labelHash);
                    samplelabelsList.AddRange(lfv_samplelabels);
                    foreach (KeyValuePair<string, int> kvp in lfv_labelHash)
                        if (!labelHash.ContainsKey(kvp.Key))
                            labelHash.Add(kvp.Key, kvp.Value);
                }

                this.samplelabels = samplelabelsList.ToArray<int>();
            }
            catch
            {
                this.samplelabels = null;
                this.labelHash = null;
            }
        }

        private void MergeSampleDocIds()
        {
            try
            {
                List<string> sampledocidsList = new List<string>();
                foreach (LoadFeatureVectors lfv in lfvs)
                {
                    sampledocidsList.AddRange(lfv.GetSampleDocIds());
                }

                this.sampledocids = sampledocidsList.ToArray<string>();
            }
            catch
            {
                this.sampledocids = null;
            }
        }

        private void MergeSampleLines()
        {
            try
            {
                List<string> samplelinesList = new List<string>();
                foreach (LoadFeatureVectors lfv in lfvs)
                {
                    samplelinesList.AddRange(lfv.GetSampleLines());
                }

                this.samplelines = samplelinesList.ToArray<string>();
            }
            catch
            {
                this.samplelines = null; 
            }
        }
    }
}
