using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using LargeGraphLayout.App_Start;
using LargeGraphLayout.Models;

namespace LargeGraphLayout.Algorithms.IO
{
    public class Brt
    {
        public static List<Link> LoadLinks(string datasetName)
        {
            string[] lines = File.ReadAllLines(WebGlobalConfig.ServerDataRoot + datasetName + ".ln");
            var links = new List<Link>();
            foreach (var line in lines)
            {
                var tokens = line.Split('\t');
                int i, j;
                if (int.TryParse(tokens[0], out i) && int.TryParse(tokens[1], out j))
                    links.Add(new Link
                    {
                        source = i,
                        target = j
                    });
            }
            return links;
        }
        public static Dictionary<int, Node> LoadBrt(string datasetName)
        {
            string[] lines = File.ReadAllLines(WebGlobalConfig.ServerDataRoot + datasetName + "_rt.gv");
            Dictionary<int, Node> dic = new Dictionary<int, Node>(lines.Length / 2);
            Dictionary<int, Node> res = new Dictionary<int, Node>(lines.Length / 2);
            HashSet<int> nodeDefineLine = new HashSet<int>();
            for (int i = 0; i < lines.Length - 1; i++)
            {
                string line = lines[i];
                if (!line.Contains('-'))
                    continue;
                if (line.Contains('['))
                {
                    string[] items = line.Split('[', '-');
                    int nodeId = int.Parse(items[0]);
                    int itemId = int.Parse(items[2]);
                    string[] content = items[3].Split('n', '\\', '(', ')');
                    List<int> feature = new List<int>();
                    for (var j = 2; j < content.Length - 1; j += 4)
                    {
                        if (String.IsNullOrEmpty(content[j]))
                            continue;
                        var word = int.Parse(content[j]);
                        var count = int.Parse(content[j + 1]);
                        feature.Add(word);
                    }
                    var node = new Node()
                    {
                        Index = itemId,
                        Feature = feature,
                    };
                    dic.Add(nodeId, node);
                    res.Add(itemId, node);
                    nodeDefineLine.Add(i);
                }
            }

            for (var i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                if (!line.Contains('-')) continue;
                if (nodeDefineLine.Contains(i)) continue;

                string[] items = line.Split('-', '>');
                var parent = dic[int.Parse(items[0])];
                var child = dic[int.Parse(items[2])];
                parent.Children.Add(child);
                child.Parent = parent;
            }
            Trace.WriteLine(datasetName + " loaded");
            return res;
        }
    }
}