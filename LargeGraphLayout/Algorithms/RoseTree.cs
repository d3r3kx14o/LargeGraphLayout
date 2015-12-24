using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Web;
using ConstrainedRoseTreeLibrary.BuildTree;
using ConstrainedRoseTreeLibrary.Data;
using ConstrainedRoseTreeLibrary.DrawTree;
using LargeGraphLayout.App_Start;
using LargeGraphLayout.Models;

namespace LargeGraphLayout.Algorithms
{
    public class RoseTree
    {
        public class RoseTreeWorker
        {
            private readonly string _outputPath;
            private readonly List<Link> _data;
            public RoseTreeWorker(string dataPath, string outputPath)
            {
                this._data = Link.LoadLinks(dataPath);
                this._outputPath = outputPath;
            }

            public void ThreadStart()
            {
                RoseTree.CalculateAndCacheBrt(_data, _outputPath);
            }
        }
        public static void CalculateAndCacheBrt(List<Link> links, string outputPath)
        {
            DateTime start = DateTime.Now;
            Console.WriteLine("start " + start);
            var startIds = (from link in links select link.source).ToList();
            var endIds = (from link in links select link.target).ToList();
            startIds.AddRange(endIds);
            string[] lex = new HashSet<string>(from id in startIds select id + "").ToArray();
            int dataCount = lex.Length;
            Dictionary<string, int> invertedIdMap = new Dictionary<string, int>(dataCount);
            List<RawDocument> docs = new List<RawDocument>(dataCount);
            for (var i = 0; i < dataCount; i ++)
            {
                invertedIdMap.Add(lex[i], i);
                docs.Add(new RawDocument(new Dictionary<string, int>(3), i));
            }

            foreach (var link in links)
            {
                var doc = docs[invertedIdMap[link.target + ""]];
                if (!doc.DocumentContentVector.ContainsKey(link.source + ""))
                    doc.DocumentContentVector.Add(link.source + "", 1);
                else
                    doc.DocumentContentVector[link.source + ""] += 1;
            }
            
            var rtp = new RoseTreeParameters()
            {
                alpha = 0.08,
                gamma = 0.15,
                k = 25
            };
            Console.WriteLine("data loaded");
            var rt = BuildRoseTree.GetRoseTree(docs, rtp);
            var dt = new DrawRoseTree(rt, "", lex.Length);
            dt.DrawTree(outputPath);
            Console.WriteLine("data saved");
        }
    }
}