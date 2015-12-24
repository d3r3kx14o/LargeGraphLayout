using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LargeGraphLayout.Models
{
    public class Node
    {
        public int Index;
        public string Label;
        // Brt related attributes
        public List<Node> Children = new List<Node>();
        public List<int> Feature = new List<int>();
        public Node Parent = null;

        // Original graph structure
        public List<Link> Links = new List<Link>();
    }
}