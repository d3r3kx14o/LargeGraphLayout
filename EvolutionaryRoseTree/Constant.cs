using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Constants
{
    class Constant
    {
        public static string BING_NEWS = "BingNewsData.txt";
        public const string DATA_PATH = @"D:\Project\EvolutionaryRoseTreeData\";
        public const string OUTPUT_PATH = @"D:\Project\EvolutionaryRoseTreeData\";


        public static string[] inputfilenames = { "ConceptualizeData_Select_2.txt",
                                           "BingNewsData.txt",
                                           "",
                                           ""};

        ////dataset_index
        //public const int conceptualize = 0;
        //public const int bing_news = 1;
        //public const int twenty_news_group = 2;
        //public const int haos_data_set = 3;

        ////model_index
        //public const int dcm = 0;
        //public const int vmf = 1;
        //public const int bernoulli = 2;

        ////algorithm_index
        //public const int brt = 0;
        //public const int knn_brt = 1;
        //public const int SPILLTREE_BRT = 2;

    }
}
