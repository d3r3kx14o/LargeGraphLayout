using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LargeGraphLayout.Algorithms.IO;
using LargeGraphLayout.Models;

namespace LargeGraphLayout.App_Start
{
    public class WebGlobalConfig
    {
        public static string ServerDataRoot = HttpContext.Current.Server.MapPath("~/App_Data") + "/";

        public static Dictionary<string, Dataset> Datasets;
        public static void Initialize()
        {
            Datasets = new Dictionary<string, Dataset>();
            // Load test data
            Datasets.Add("demo", new Dataset(Brt.LoadBrt("demo"), Link.LoadLinks("demo")));
        }
    }
}