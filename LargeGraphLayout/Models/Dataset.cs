using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace LargeGraphLayout.Models
{
    public class Dataset
    {
        public Dictionary<int, Node> Nodes;
        public List<Link> Links;
        public int RootId;

        public Dataset(Dictionary<int, Node> nodes, List<Link> links)
        {
            Trace.WriteLine(string.Join(",", nodes.Keys));
            this.Nodes = nodes;
            this.Links = links;
            foreach (var link in this.Links)
            {
                Node start, end;
                if (!this.Nodes.TryGetValue(link.source, out start) || !this.Nodes.TryGetValue(link.target, out end))
                {
                    Trace.WriteLine("Link not found: " + link.source + "/" + link.target);
                }
                else
                {
                    start.Links.Add(link);
                    end.Links.Add(link);
                }
            }
            this.RootId = this.Nodes.Min(n => n.Key);
        }

        public Node GetRoot()
        {
            return this.Nodes[this.RootId];
        }
    }
}