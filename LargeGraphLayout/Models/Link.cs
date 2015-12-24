using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using LargeGraphLayout.App_Start;

namespace LargeGraphLayout.Models
{
    public class Link
    {
        public int source { get; set; }
        public int target { get; set; }

        public static List<Link> LoadLinks(string dataPath)
        {
            string[] lines = File.ReadAllLines(WebGlobalConfig.ServerDataRoot + dataPath + ".ln").Skip(1).ToArray();
            List<Link> res = new List<Link>(lines.Length);
            foreach (var line in lines)
            {
                string[] tokens = line.Split('\t');
                if (tokens.Length != 2) continue;
                int i, j;
                if (int.TryParse(tokens[0], out i) && int.TryParse(tokens[1], out j))
                    res.Add(new Link()
                    {
                        source = i,
                        target = j
                    });
            }
            return res;
        }
    }
}