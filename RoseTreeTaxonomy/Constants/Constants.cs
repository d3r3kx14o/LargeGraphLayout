using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.Constants
{
    public static class Constant
    {
        public const string DATA_PATH = @"D:\Project\RoseTreeTaxonomyData\datapath\";
        public const string OUTPUT_PATH = @"D:\Project\RoseTreeTaxonomyData\outputpath\";

        public static int SampleNumberMultiplier = 10;
        public static int LeadingParaSentenseNum = 6;
        public static int BingNewsTitleWeight = 5;
        public static int BingNewsLeadingParagraphWeight = 3;
        public static int BingNewsBodyWeight = 1;
        public static bool BingNewsWeightLengthNormalization = false;

        public const int ROSETREE_PRECISION = 0;
        public const int SPILLTREE_PRECISION = 1;
        public const int RANDOM_PROJECTION_PRECISION = 2;
        public const int TIME_EXPERIMENT = 3;
        public const int NMI = 4;
        public const int HAOS_EXPERIMENT = 5;
        public const int LIKELIHOOD_EXPERIMENT = 6;
        public const int LIKELIHOOD_EXPERIMENT_STAT = 7;

        // data_index
        public const int CONCEPTUALIZE = 0;
        public const int BING_NEWS = 1;
        public const int TWENTY_NEWS_GROUP = 2;
        public const int HAOS_DATA_SET = 3;
        public const int NEW_YORK_TIMES = 4;
        public const int INDEXED_BING_NEWS= 5;

        // algorithm index
        public const int BRT = 0;
        public const int KNN_BRT = 1;
        public const int SPILLTREE_BRT = 2;

        // model index
        public const int DCM = 0;
        public const int VMF = 1;
        public const int BERNOULLI = 2;

        public const int NOT_A_CHILD = 0;
        public const int LEFT_CHILD = 1;
        public const int RIGHT_CHILD = 2;

        public const int GAUSSIAN_RANDOM = 0;
        public const int SQRT_THREE_RANDOM = 1;

        public const int COSINE = 0;
        public const int JACCARD = 1;

        public static int[] intervals = { 30, 30, 30, 300, 300, 300, 3000, 3000, 3000, 3000 };

        public static string[] inputfilenames = { "ConceptualizeData_Select_2.txt",
                                           "BingNewsData.txt",
                                           "",
                                           ""};
        public static string[] pathnames = { DATA_PATH,
                                            DATA_PATH,
                                            DATA_PATH+@"textindex",
                                            DATA_PATH+@"ClusterKeywordExtractor\"
                                           };

        public static int[] datasize = { 111193, 
                                         1700130,
                                         19997
                                       };

        public static string URI_FIELD_NAME = "uri";
        public static string PLAIN_TEXT_FIELD_NAME = "plain";
        public static string DATE_FIELD_NAME = "cdate"; // to be consistent with cca	
        public static string TITLE = "Subject";
        public static string SENDER = "Sender";
        public static string RECEIVER = "Receiver";
        public static string FOWARD = "CC";
        public static string AGE_FIELD_NAME = "age";
        public static string GENDER_FIELD_NAME = "gender";
        public static string ICAUSE_FIELD_NAME = "icause";
        public static string VCAUSE_FIELD_NAME = "vcause";
        public static string DIAGNOSIS_FIELD_NAME = "diagnosis";        

        public static string[] insurance_keyword_select = 
        {
        "health",
        "life",
        "auto",
        "car",
        "home",
        "medical",
        "state",
        "liability",
        "dental",
        "farm",
        "term",
        "healthcare",
        "farmers",
        "cars",
        "travel",
        "group",
        "business",
        "general",
        "family",
        "homeowners",
        "homes",
        "disability",
        "house",
        "property",
        "medicare",
        "renters",
        "financial",
        "renter",
        "rental",
        "mortgage",
        "vehicle",
        "casualty"
        };

        public class NewYorkTimesDataFields
        {
            public static String AlternateURL = "Alternate URL";				//URL, single
            public static String ArticleAbstract = "Article Abstract"; 		//String, single
            public static String AuthorBiography = "Author Biography";	//String, single
            public static String Banner = "Banner"; 									//String, single
            public static String BiographicalCategories = "Biographical Categories";	//String, multiple
            public static String Body = "Body";	//String, single
            public static String Byline = "Byline";	//String, single
            public static String ColumnName = "Column Name";	//String, single
            public static String ColumnNumber = "Column Number";	//String, single
            public static String CorrectionDate = "Correction Date";
            public static String CorrectionText = "Correction Text";
            public static String Credit = "Credit";
            public static String Dateline = "Dateline";
            public static String DayOfWeek = "Day Of Week";
            public static String Descriptors = "Descriptors";
            public static String FeaturePage = "Feature Page";
            public static String GeneralOnlineDescriptors = "General Online Descriptors";
            public static String Guid = "Guid";
            public static String Headline = "Headline";
            public static String Kicker = "Kicker";
            public static String LeadParagraph = "Lead Paragraph";
            public static String Locations = "Locations";
            public static String Names = "Names";
            public static String NewsDesk = "News Desk";
            public static String NormalizedByline = "Normalized Byline";
            public static String OnlineDescriptors = "Online Descriptors";
            public static String OnlineHeadline = "Online Headline";
            public static String OnlineLeadParagraph = "Online Lead Paragraph";
            public static String OnlineLocations = "Online Locations";
            public static String OnlineOrganizations = "Online Organizations";
            public static String OnlinePeople = "Online People";
            public static String OnlineSection = "Online Section";
            public static String OnlineTitles = "Online Titles";
            public static String Organization = "Organization";
            public static String Page = "Page";
            public static String People = "People";
            public static String PublicationDate = "Publication Date";
            public static String PublicationDayOfMonth = "Publication Day Of Month";
            public static String PublicationMonth = "Publication Month";
            public static String PublicationYear = "Publication Year";
            public static String Section = "Section";
            public static String SeriesName = "Series Name";
            public static String Slug = "Slug";
            public static String TaxonomicClassifiers = "Taxonomic Classifiers";
            public static String Titles = "Titles";
            public static String TypesOfMaterial = "Types Of Material";
            public static String Url = "Url";
            public static String WordCount = "Word Count";
            //added fields
            public static String CleanedTaxonomicClassifier = "Cleaned Taxonomic Classifier";
            public static String CleanedClassifierLayer = "Cleaned Classifier Layer";
        }

        public class IndexedBingNewsDataFields
        {
            public static string DocumentId = "DocId";
            public static String DocumentURL = "DocumentURL";
            public static String Country = "Country";
            public static String NewsArticleCategoryData = "NewsArticleCategoryData";
            public static String NewsArticleHeadline = "NewsArticleHeadline";
            public static String NewsArticleHeadNEMap = "NewsArticleHeadNEMap";
            public static String NewsArticleDescription = "NewsArticleDescription";
            public static String NewsArticleBodyNEMap = "NewsArticleBodyNEMap";
            public static String DiscoveryStringTime = "DiscoveryStringTime";
            public static String PublishedDateTime = "PublishedDateTime";
            public static String NewsSource = "NewsSource";
        }
    }
}
