//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using RoseTreeTaxonomy.ReadData;
//using RoseTreeTaxonomy.Constants;
//using RoseTreeTaxonomy.DataStructures;

//using Lucene.Net.Analysis;
//using Lucene.Net.Analysis.Standard;
//using Lucene.Net.Search;
//using Lucene.Net.QueryParsers;
//using Lucene.Net.Documents;
//using Lucene.Net.Store;
//using Lucene.Net.Analysis.Tokenattributes;
//using Lucene.Net.Index;
//using LuceneDirectory = Lucene.Net.Store.Directory;
//using Version = Lucene.Net.Util.Version;

//namespace EvolutionaryRoseTree.DataStructures
//{
//    class LoadSubTreeFeatureVectors : LoadGlobalFeatureVectors
//    {
//        LoadFeatureVectors MainLfv;
//        RoseTreeNode SubRoseTreeRoot;

//        public LoadSubTreeFeatureVectors(LoadFeatureVectors mainlfv, RoseTreeNode subrosetreeroot):
//            base(mainlfv.dataset_index, mainlfv.model_index, null, mainlfv.samplefilename, null, mainlfv.samplenum, 
//            mainlfv.defaultqueryfield, mainlfv.querystring)
//        {
//            if (subrosetreeroot.tree_depth != 2)
//                throw new Exception("[LoadSubTreeFeatureVectors] Error building sub rose tree!");

//            MainLfv = mainlfv;
//            SubRoseTreeRoot = subrosetreeroot;

//            Load();
//        }

//        public void Load()
//        {

//        }
//    }
//}
