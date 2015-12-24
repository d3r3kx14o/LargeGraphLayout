using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;
using System.Diagnostics;

namespace ConstrainedRoseTreeLibrary.Data
{
    public class RoseTreeData
    {
        public int modelIndex = Constant.DCM;
        public LoadFeatureVectors lfv;

        public RoseTreeData(List<RawDocument> rawDocuments, GlobalLexicon globalLexicon)
        {
            lfv = new LoadRawDocumentFeatureVectors(rawDocuments, globalLexicon, modelIndex);
            (lfv as LoadRawDocumentFeatureVectors).Load();            
        }
    }

    class LoadRawDocumentFeatureVectors : LoadFeatureVectors
    {
        public int DeltaIndex { get; protected set; }
        List<RawDocument> rawDocuments;

        public LoadRawDocumentFeatureVectors(List<RawDocument> rawDocuments, 
            GlobalLexicon globalLexicon, int modelIndex)
            : base(-1, modelIndex)
        {
            lexicon = globalLexicon.Lexicon;
            invertlexicon = globalLexicon.InvertedLexicon;
            samplenum = rawDocuments.Count;
            this.rawDocuments = rawDocuments;
        }

        public void Load()
        {
            this.featurevectors = new SparseVectorList[samplenum];
            
            //Initialize feature vectors
            for (int i = 0; i < samplenum; i++)
            {
                SparseVectorList vector = new SparseVectorList(model_index);
                var rawDocument = rawDocuments[i];
                var docVectors = new Dictionary<string, int>[]{
                    rawDocument.DocumentContentVector,rawDocument.DocumentComplementVector};
                foreach(var docVector in docVectors)
                    foreach (var kvp in docVector)
                    {
                        int lexiconIndex;
                        string word = kvp.Key;
                        if (!lexicon.TryGetValue(word, out lexiconIndex))
                        {
                            lexiconIndex = lexicon.Count;
                            lexicon.Add(word, lexiconIndex);
                            invertlexicon.Add(lexiconIndex, word);
                            if (!word.StartsWith("TopoInfoTopic"))
                                Trace.WriteLine("Added To Lexicon: {0}", word);
                        }
                        if (!vector.Increase(lexiconIndex, kvp.Value))
                        {
                            vector.Insert(lexiconIndex, kvp.Value);
                            if (wordappearancecount.ContainsKey(lexiconIndex))
                                wordappearancecount[lexiconIndex]++;
                            else
                                wordappearancecount.Add(lexiconIndex, 1);
                        }
                        this.wordnum += kvp.Value;
                    }

                vector.contentvectorlen = rawDocument.DocumentContentVector.Count;
                vector.documentid = rawDocument.DocumentId;
                featurevectors[i] = vector;
            }

            //other steps
            ResizeFeatureVectors();
            SumUpFeatureVectors(); 
            PostProcessData();     

            if (model_index == Constant.VMF)
                ComputeIDF();
            this.lexiconsize = this.lexicon.Count;
            GetNorm();

            if (featurevectors.Length != 0)
            {
                var maxDocumentID = featurevectors.Max<SparseVectorList>(vector => { return vector.documentid; });
                DeltaIndex = maxDocumentID - featurevectors.Length + 1;
            }
            else
            {
                DeltaIndex = 0;
            }
        }
    }
}
