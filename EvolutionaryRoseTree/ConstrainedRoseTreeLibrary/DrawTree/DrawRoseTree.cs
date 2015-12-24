using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ConstrainedRoseTreeLibrary.AnalyzeTree;
using ConstrainedRoseTreeLibrary.Data;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;

namespace ConstrainedRoseTreeLibrary.DrawTree
{
    public class DrawRoseTree
    {
        RoseTree rosetree;
        string drawpath;
        int initial_clusternum;
        int sizeofprintlist;
        bool bDrawNode;
        StreamWriter drawtree;
        int deltaIndex;

        public DrawRoseTree(RoseTree rosetree)
            : this(rosetree, "", 10, true)
        {
            
        }

        public DrawRoseTree(RoseTree rosetree, string drawpath, int sizeofprintlist)
            :this(rosetree, drawpath, sizeofprintlist, true)
        {
        }

        public DrawRoseTree(RoseTree rosetree, string drawpath, int sizeofprintlist, bool bDrawLeaf)
        {
            this.rosetree = rosetree;
            this.initial_clusternum = rosetree.initial_clusternum;
            this.drawpath = drawpath;
            this.sizeofprintlist = sizeofprintlist;
            this.bDrawNode = bDrawLeaf;

            //var maxDocumentID = rosetree.lfv.featurevectors.Max<SparseVectorList>(vector => { return vector.documentid; });
            //deltaIndex = maxDocumentID - rosetree.lfv.featurevectors.Length + 1;
            //Trace.WriteLine(string.Format("deltaIndex:{0}", deltaIndex));
            deltaIndex = (rosetree.lfv as LoadRawDocumentFeatureVectors).DeltaIndex;

            if (drawpath != null && !Directory.Exists(drawpath) && drawpath.Length > 0)
                Directory.CreateDirectory(drawpath);
        }

        delegate void DrawNodeFunction(int depth, RoseTreeNode rosetreenode);
        DrawNodeFunction DrawNode;

        public void Run()
        {
            try
            {
                //ReadFeatureVectors();//Xiting
                DrawTree("drawtree.gv");
            }
            catch
            {
                Console.WriteLine("Error drawing tree");
            }
        }

        public void Run(string filename)
        {
            try
            {
                DrawTree(filename);
            }
            catch
            {
                Console.WriteLine("Error drawing tree");
            }
        }

        public void Run(string filename, bool bDrawInternalNodesOnly)
        {
            try
            {
                if (bDrawInternalNodesOnly)
                    DrawInternalTree(filename);
                else
                    DrawTree(filename);
            }
            catch
            {
                Console.WriteLine("Error drawing tree");
            }
        }

        public void DrawTree(string filename)
        {
            if (rosetree.model_index == RoseTreeTaxonomy.Constants.Constant.DCM)
                DrawNode = new DrawNodeFunction(DrawNode_DCM);
            else
                DrawNode = new DrawNodeFunction(DrawNode_vMF);

            this.drawtree = new StreamWriter(drawpath + filename);
            drawtree.WriteLine("digraph G \n {graph[ \n rankdir = \"TD\"];");

            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(rosetree.root);

            int depth = 0;

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;
                for (int i = 0; i < nodelistcount; i++)
                {
                    RoseTreeNode node = nodelist[0];

                    if (node.children != null)
                    {
                        for (int j = 0; j < node.children.Length; j++)
                        {
                            drawtree.WriteLine(node.indices.tree_index + "->" + node.children[j].indices.tree_index);
                            nodelist.Add(node.children[j]);
                        }
                    }
                    drawtree.Write(node.indices.tree_index + "[color = grey, label =\"");

                    if (bDrawNode)
                        DrawNode(depth, node);

                    drawtree.WriteLine("\"" + ", shape=\"record\"];");

                    nodelist.RemoveAt(0);
                }
                depth++;
            }

            this.drawtree.WriteLine("}");
            this.drawtree.Flush();
            this.drawtree.Close();
        }

        public void DrawInternalTree(string filename)
        {
            this.drawtree = new StreamWriter(drawpath + filename);
            drawtree.WriteLine("digraph G \n {graph[ \n rankdir = \"TD\"];");

            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(rosetree.root);

            int depth = 0;

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;
                for (int i = 0; i < nodelistcount; i++)
                {
                    RoseTreeNode node = nodelist[0];

                    if (node.children != null)
                    {
                        nodelist.AddRange(node.children);
                    }

                    //do not draw leaf nodes
                    if (node.children != null)
                    {
                        if (node.parent != null)
                            drawtree.WriteLine(node.parent.indices.tree_index + "->"
                                + node.indices.tree_index);

                        drawtree.Write(node.indices.tree_index + "[color = grey, label =\"");

                        drawtree.Write("-" + GetNodeTreeID(node) + "-\\n");
                        drawtree.Write("(" + node.children.Length + ")");

                        drawtree.WriteLine("\"" + ", shape=\"record\"];");
                    }

                    nodelist.RemoveAt(0);
                }
                //Console.WriteLine(depth);
                depth++;
            }

            this.drawtree.WriteLine("}");
            this.drawtree.Flush();
            this.drawtree.Close();
        }

        void DrawNode_DCM(int depth, RoseTreeNode node)
        {
            //SparseVectorList data = GetNodeData(node);//Xiting
            SparseVectorList data = node.data;
            int[] keyarray = data.keyarray;
            int[] valuearray = data.valuearray;
            int sizeofprintlist = (keyarray.Length > this.sizeofprintlist) ? this.sizeofprintlist : keyarray.Length;

            MinHeapInt mh = new MinHeapInt(this.sizeofprintlist);
            for (int j = 0; j < this.sizeofprintlist; j++)
                mh.insert(-1, int.MinValue);

            for (int j = 0; j < data.contentvectorlen; j++)
            {
                if (valuearray[j] > mh.min())
                    mh.changeMin(keyarray[j], valuearray[j]);
            }
            MinHeapInt.heapSort(mh);

            int[] indices = mh.getIndices();
            int[] values = mh.getValues();

            //this.drawtree.Write("-----data----- \\n");
            if (node.children == null || node.children.Length == 0)
            {
                this.drawtree.Write("-{0}-\\n", GetNodeTreeID(node));
                //this.drawtree.Write("{0}({1})\\n", "Depth", node.DepthInTree);
                for (int i = 0; i < indices.Length; i++)
                    if (indices[i] >= 0)
                        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
                this.drawtree.Write("{0}\\n", node.DocId);
            }
            else
            {
                this.drawtree.Write("-{0}-\\n", GetNodeTreeID(node));
                if (node.BOthers)
                    this.drawtree.Write("{0}({1})\\n", "OTHERS", 0);
                if (node.OpenedNode)
                    this.drawtree.Write("{0}({1})\\n", "OPENED", 0);
                //this.drawtree.Write("{0}({1})\\n", "Depth", node.DepthInTree);
                for (int i = 0; i < indices.Length; i++)
                    if (indices[i] >= 0)
                        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
            }
        }

        void DrawNode_vMF(int depth, RoseTreeNode node)
        {
            //SparseVectorList data = GetNodeData(node);//Xiting
            SparseVectorList data = node.data;
            int[] keyarray = data.keyarray;
            double[] valuearray = data.l2normedvaluearray;
            this.sizeofprintlist = (keyarray.Length > this.sizeofprintlist) ? this.sizeofprintlist : keyarray.Length;

            MinHeapDouble mh = new MinHeapDouble(this.sizeofprintlist);
            for (int j = 0; j < this.sizeofprintlist; j++)
                mh.insert(-1, int.MinValue);

            for (int j = 0; j < data.count; j++)
            {
                if (valuearray[j] > mh.min())
                    mh.changeMin(keyarray[j], valuearray[j]);
            }
            MinHeapDouble.heapSort(mh);

            int[] indices = mh.getIndices();
            double[] values = mh.getValues();

            if (node.children == null || node.children.Length == 0)
            {
                this.drawtree.Write("-{0}-\\n", GetNodeTreeID(node));
                //this.drawtree.Write("{0}({1})\\n", "Depth", node.DepthInTree);
                for (int i = 0; i < 1; i++)
                    if (indices[i] >= 0)
                        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
                this.drawtree.Write("{0}\\n", node.DocId);
            }
            else
            {
                this.drawtree.Write("-{0}-\\n", GetNodeTreeID(node));
                //this.drawtree.Write("{0}({1})\\n", "Depth", node.DepthInTree);
                for (int i = 0; i < indices.Length; i++)
                    if (indices[i] >= 0)
                        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
            }
        }

        private int GetNodeTreeID(RoseTreeNode node)
        {
            return AnalyzeTreeData.GetNodeID(rosetree, node);
            //if (node.children == null)
            //{
            //    var vector = rosetree.lfv.featurevectors[node.indices.initial_index];
            //    return vector.documentid;
            //}
            //else
            //{
            //    return node.MergeTreeIndex + deltaIndex;
            //}
        }

    }
}
