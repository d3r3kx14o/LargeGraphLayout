using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LargeGraphLayout.Models;

namespace LargeGraphLayout.Algorithms.Graph
{
    public class Graph
    {
        public static List<Node> GetChildrenNodes(Dictionary<int, Node> graph, string rootLabel)
        {
            int rootNodeId = 0;
            int.TryParse(rootLabel, out rootNodeId);
            Node rootNode;
            if (!graph.TryGetValue(rootNodeId, out rootNode)) return new List<Node>();
            return rootNode.Children;
        }

        public static IEnumerable<Link> GetLinks(List<Node> nodes)
        {
            return nodes.SelectMany(node => node.Links);
        }
    }
}