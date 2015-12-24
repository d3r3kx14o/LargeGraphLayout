using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using ConstrainedRoseTreeLibrary.BuildTree;
using ConstrainedRoseTreeLibrary.Data;
using ConstrainedRoseTreeLibrary.DrawTree;
using LargeGraphLayout.Algorithms;
using LargeGraphLayout.Algorithms.Graph;
using LargeGraphLayout.App_Start;
using LargeGraphLayout.Models;
using LargeGraphLayout.Models.HttpIO;

namespace LargeGraphLayout.Controllers
{
    public class DataController : ApiController
    {
        [HttpPost]
        [ActionName("retrieve_real")]
        public JsonResult<String> GetData(RequestBrtGraphModel model)
        {
            Trace.WriteLine("client requesting " + model.Dataset);
            Dataset dataset;
            if (!WebGlobalConfig.Datasets.TryGetValue(model.Dataset, out dataset))
                dataset = WebGlobalConfig.Datasets["demo"];
            int rootId;
            if (!int.TryParse(model.RootNodeId, out rootId)) rootId = 0;
            Node rootNode;
            if (!dataset.Nodes.TryGetValue(rootId, out rootNode)) rootNode = dataset.GetRoot();
            var childrenNodes = rootNode.Children;
            var primaryNodeIdx = childrenNodes.Select(n => n.Index).ToList();
            var adjacentLinks = Graph.GetLinks(childrenNodes).ToList();
            var startIds = (from link in adjacentLinks select link.source).ToList();
            var endIds = (from link in adjacentLinks select link.target).ToList();
            startIds.AddRange(endIds);
            var complementNodeIds = new HashSet<int>(startIds).Except(primaryNodeIdx).ToList();
            return Json(new JavaScriptSerializer().Serialize(new ResponseBrtGraphModel()
            {
                PrimaryNodes = primaryNodeIdx,
                SecondaryNodes = complementNodeIds,
                Links = adjacentLinks
            }));
        }

        [HttpPost]
        [ActionName("retrieve")]
        public JsonResult<String> GetDataDemo(RequestBrtGraphModel model)
        {
            Trace.WriteLine("client requesting " + model.Dataset);
            Dataset dataset;
            if (!WebGlobalConfig.Datasets.TryGetValue(model.Dataset, out dataset))
                dataset = WebGlobalConfig.Datasets["demo"];
            int rootId;
            if (!int.TryParse(model.RootNodeId, out rootId)) rootId = 0;
            Random random = new Random(rootId);
            var nodes = new List<int>();
            for (var i = 0; i < 40; i ++)
            {
                nodes.Add(random.Next(10000));
            }
            var adjacentLinks = new List<Link>();
            var division = 10 + random.Next(10);
            var primaryNodes = nodes.GetRange(0, division);
            var secondaryNodes = nodes.GetRange(division, nodes.Count - division);
            for (var i = 0; i < 100; i ++)
            {
                int source = 0, target = 0;
                if (i < 60)
                {
                    source = random.Next(division);
                    target = random.Next(nodes.Count - division);
                    if (i%2 == 0)
                    {
                        var t = source;
                        source = target;
                        target = t;
                    }
                }
                else
                {
                    source = random.Next(division);
                    target = random.Next(division);
                }
                adjacentLinks.Add(new Link()
                {
                    source = nodes[source],
                    target = nodes[target]
                });
            }
            return Json(new JavaScriptSerializer().Serialize(new ResponseBrtGraphModel()
            {
                PrimaryNodes = primaryNodes,
                SecondaryNodes = secondaryNodes,
                Links = adjacentLinks
            }));
        }

        [HttpPost]
        [ActionName("upload")]
        public async Task<HttpResponseMessage> PostFormData()
        {
            // Check if the request contains multipart/form-data.
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            string root = WebGlobalConfig.ServerDataRoot;
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                // Read the form data.
                await Request.Content.ReadAsMultipartAsync(provider);

                // This illustrates how to get the file names.
                foreach (MultipartFileData file in provider.FileData)
                {
                    Trace.WriteLine(file.Headers.ContentDisposition.FileName);
                    Trace.WriteLine("Server file path: " + file.LocalFileName);
                    var worker = new RoseTree.RoseTreeWorker(file.LocalFileName, "test");
                    new Thread(worker.ThreadStart).Start();
                    break; // process only the first file
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }
        
    }
}