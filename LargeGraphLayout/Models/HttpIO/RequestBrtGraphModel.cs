using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LargeGraphLayout.Models.HttpIO
{
    public class RequestBrtGraphModel
    {
        public string Dataset { get; set; }
        public string RootNodeId { get; set; }
    }

    public class ResponseBrtGraphModel
    {
        public List<int> PrimaryNodes { get; set; }
        public List<int> SecondaryNodes { get; set; } 
        public List<Link> Links { get; set; }
    }
}