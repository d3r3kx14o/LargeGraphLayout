using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.DataStructures;
using System.IO;

namespace RoseTreeTaxonomy.DrawTree
{
    public class DrawRoseTree
    {
        RoseTree rosetree;
        SparseVectorList[] featurevectors;
        string drawpath;
        int initial_clusternum;
        int sizeofprintlist;
        bool bDrawNode;
        bool bDrawAttribute;
        StreamWriter drawtree;

        public DrawRoseTree(RoseTree rosetree, string drawpath, int sizeofprintlist)
        {
            this.rosetree = rosetree;
            this.initial_clusternum = rosetree.initial_clusternum;
            this.drawpath = drawpath;
            this.sizeofprintlist = sizeofprintlist;
            this.bDrawAttribute = false;
        }

        public DrawRoseTree(RoseTree rosetree, string drawpath, int sizeofprintlist, bool bDrawAttribute)
        {
            this.rosetree = rosetree;
            this.initial_clusternum = rosetree.initial_clusternum;
            this.drawpath = drawpath;
            this.sizeofprintlist = sizeofprintlist;
            this.bDrawAttribute = bDrawAttribute;
        }

        public DrawRoseTree(RoseTree rosetree, string drawpath, int sizeofprintlist, 
            bool bDrawNode, bool bDrawAttribute)
        {
            this.rosetree = rosetree;
            this.initial_clusternum = rosetree.initial_clusternum;
            this.drawpath = drawpath;
            this.sizeofprintlist = sizeofprintlist;
            this.bDrawNode = bDrawNode;
            this.bDrawAttribute = bDrawAttribute;
        }

        public delegate void DrawNodeFunction(int depth, RoseTreeNode rosetreenode);
        public DrawNodeFunction DrawNode;

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
                //ReadFeatureVectors();//Xiting
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
                //ReadFeatureVectors();//Xiting
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

        public void ReadFeatureVectors()
        {
            if (rosetree.model_index == 1)
            {
                ReadFeatureVectorsvMF();
                return;
            }

            this.featurevectors = new SparseVectorList[initial_clusternum];
            StreamReader reader = new StreamReader(this.rosetree.lfv.samplepath + this.rosetree.lfv.featurevectorfilename);

            for (int i = 0; i < initial_clusternum; i++)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(';');

                this.featurevectors[i] = new SparseVectorList(this.rosetree.model_index);
                this.featurevectors[i].keyarray = new int[tokens.Length - 1];
                this.featurevectors[i].valuearray = new int[tokens.Length - 1];
                this.featurevectors[i].count = tokens.Length - 1;

                for (int j = 0; j < tokens.Length - 1; j++)
                {
                    string[] keyandvalue = tokens[j].Split(':');
                    this.featurevectors[i].keyarray[j] = int.Parse(keyandvalue[0]);
                    this.featurevectors[i].valuearray[j] = int.Parse(keyandvalue[1]);
                }
            }

            reader.Close();
        }

        public void ReadFeatureVectorsvMF()
        {
            this.featurevectors = new SparseVectorList[initial_clusternum];
            StreamReader reader = new StreamReader(this.rosetree.lfv.samplepath + this.rosetree.lfv.featurevectorfilename);

            for (int i = 0; i < initial_clusternum; i++)
            {
                string line = reader.ReadLine();
                string[] tokens = line.Split(';');

                this.featurevectors[i] = new SparseVectorList(this.rosetree.model_index);
                this.featurevectors[i].keyarray = new int[tokens.Length - 1];
                this.featurevectors[i].l2normedvaluearray = new double[tokens.Length - 1];
                this.featurevectors[i].count = tokens.Length - 1;

                for (int j = 0; j < tokens.Length - 1; j++)
                {
                    string[] keyandvalue = tokens[j].Split(':');
                    this.featurevectors[i].keyarray[j] = int.Parse(keyandvalue[0]);
                    this.featurevectors[i].l2normedvaluearray[j] = double.Parse(keyandvalue[1]);
                }
            }

            reader.Close();
        }

        public void DrawTree(string filename)
        {
            if (rosetree.model_index == Constants.Constant.DCM)
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

                    if(bDrawNode)
                        DrawNode(depth, node);

                    drawtree.WriteLine("\"" + ", shape=\"record\"];");

                    nodelist.RemoveAt(0);
                }
                //Console.WriteLine(depth);
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

                        drawtree.Write("-" + node.MergeTreeIndex + "-\\n");
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

        public void DrawNode_DCM(int depth, RoseTreeNode node)
        {
            //SparseVectorList data = GetNodeData(node);//Xiting
            SparseVectorList data = node.data;
            int[] keyarray = data.keyarray;
            int[] valuearray = data.valuearray;
            int sizeofprintlist = (keyarray.Length > this.sizeofprintlist) ? this.sizeofprintlist : keyarray.Length;

            MinHeapInt mh = new MinHeapInt(this.sizeofprintlist);
            for (int j = 0; j < this.sizeofprintlist; j++)
                mh.insert(-1, int.MinValue);

            for (int j = 0; j < data.count; j++)
            {
                //if (keyarray[j] > mh.min())//Xiting
                if (valuearray[j] > mh.min())
                    mh.changeMin(keyarray[j], valuearray[j]);
            }
            MinHeapInt.heapSort(mh);

            int[] indices = mh.getIndices();
            int[] values = mh.getValues();

            //this.drawtree.Write("-----data----- \\n");
            if (node.children == null || node.children.Length == 0)
            {
                this.drawtree.Write("-{0}-\\n", node.indices.initial_index);
                //for (int i = 0; i < 1; i++)
                //    if (indices[i] >= 0)
                //        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
                this.drawtree.Write("{0}\\n", node.DocId);
            }
            else
            {
                this.drawtree.Write("-{0}-\\n", node.MergeTreeIndex);
                if (node.BOthers)
                    this.drawtree.Write("{0}({1})\\n", "OTHERS", 0);
                if(node.OpenedNode)
                    this.drawtree.Write("{0}({1})\\n", "OPENED", 0);
                for (int i = 0; i < indices.Length; i++)
                    if (indices[i] >= 0)
                        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
            }
            
            //if (node.children != null)
            //    this.drawtree.Write("childcnt=" + node.children.Length+"\\n");
            //else
            //    this.drawtree.Write("childcnt=0\\n");
            //if (node.indices.initial_index != -1)
            //{
            //    string output_string = Process(this.rosetree.querystrings[node.indices.initial_index]);
            //    this.drawtree.Write(" str={0}\\n intialindex={1}\\n", output_string, node.indices.initial_index);
            //}
        }

        public void DrawNode_vMF(int depth, RoseTreeNode node)
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
                //if (keyarray[j] > mh.min()) //Xiting
                if (valuearray[j] > mh.min())
                    mh.changeMin(keyarray[j], valuearray[j]);
            }
            MinHeapDouble.heapSort(mh);

            int[] indices = mh.getIndices();
            double[] values = mh.getValues();

            //this.drawtree.Write("-----data----- \\n");
            if (node.children == null || node.children.Length == 0)
            {
                this.drawtree.Write("-{0}-\\n", node.indices.initial_index);
                //for (int i = 0; i < 1; i++)
                //    if (indices[i] >= 0)
                //        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
                this.drawtree.Write("{0}\\n", node.DocId);
            }
            else
            {
                this.drawtree.Write("-{0}-\\n", node.MergeTreeIndex);
                for (int i = 0; i < indices.Length; i++)
                    if (indices[i] >= 0)
                        this.drawtree.Write("{0}({1})\\n", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
            }

            //if (node.children != null)
            //    this.drawtree.Write("childcnt=" + node.children.Length+"\\n");
            //else
            //    this.drawtree.Write("childcnt=0\\n");
            //if (node.indices.initial_index != -1)
            //{
            //    string output_string = Process(this.rosetree.querystrings[node.indices.initial_index]);
            //    this.drawtree.Write(" str={0}\\n intialindex={1}\\n", output_string, node.indices.initial_index);
            //}
        }

        //public void DrawNode(int depth, RoseTreeNode node)
        //{
        //    SparseVectorList data = GetNodeData(node);
        //    int[] keyarray = data.keyarray;
        //    int[] valuearray = data.valuearray;
        //    this.sizeofprintlist = (keyarray.Length > this.sizeofprintlist) ? this.sizeofprintlist : keyarray.Length;

        //    MinHeapInt mh = new MinHeapInt(this.sizeofprintlist);
        //    for (int j = 0; j < this.sizeofprintlist; j++)
        //        mh.insert(-1, int.MinValue);

        //    for (int j = 0; j < data.count; j++)
        //    {
        //        if (keyarray[j] > mh.min())
        //            mh.changeMin(keyarray[j], valuearray[j]);
        //    }
        //    MinHeapInt.heapSort(mh);

        //    int[] indices = mh.getIndices();
        //    int[] values = mh.getValues();

        //    this.drawtree.Write("-----data-----\\n ");
        //    for (int i = 0; i < indices.Length; i++)
        //        if (indices[i] >= 0)
        //            this.drawtree.Write("{0}({1}) \\n ", this.rosetree.lfv.invertlexicon[indices[i]], values[i]);
        //    if (node.children != null)
        //        this.drawtree.Write(node.children.Length);
        //    else
        //        this.drawtree.Write(0);
        //    if (node.indices.initial_index != -1)
        //    {
        //        string output_string = Process(this.rosetree.querystrings[node.indices.initial_index]);
        //        this.drawtree.Write("\\n {0} \\ n {1}", output_string, node.indices.initial_index);
        //    }
        //    this.drawtree.WriteLine("\"" + ", shape=\"record\"];");
        //}


        public SparseVectorList GetNodeData(RoseTreeNode node)
        {
            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(node);
            SparseVectorList data = new SparseVectorList(rosetree.model_index);

            while (nodelist.Count != 0)
            {
                int nodelistcount = nodelist.Count;
                for (int i = 0; i < nodelistcount; i++)
                {
                    RoseTreeNode subnode = nodelist[0];
                    if (subnode.indices.initial_index != -1)
                    {
                        List<int> overlapping_keylist;
                        int newvectorcount;
                        data = data.Add(true, this.rosetree.model_index, data, this.featurevectors[subnode.indices.initial_index], out overlapping_keylist, out newvectorcount);
                        data.Resize(newvectorcount);
                    }

                    if (subnode.children != null)
                        for (int j = 0; j < subnode.children.Length; j++)
                            nodelist.Add(subnode.children[j]);

                    nodelist.RemoveAt(0);
                }
            }
            return data;
        }

        public void OutputHaosTree()
        {
            ReadFeatureVectors();
            StreamWriter haos_tree_writer = new StreamWriter(drawpath + "\\Haos_output.txt");

            List<RoseTreeNode> nodelist = new List<RoseTreeNode>();
            nodelist.Add(rosetree.root);

            while (nodelist.Count != 0)
            {
                int nodelist_count = nodelist.Count;

                for (int i = 0; i < nodelist_count; i++)
                {
                    RoseTreeNode node = nodelist[0];

                    if (node.children != null)
                        for (int j = 0; j < node.children.Length; j++)
                            nodelist.Add(node.children[j]);

                    if (node.children == null)
                    {
                        haos_tree_writer.Write(rosetree.querystrings[node.indices.initial_index]);
                        RoseTreeNode this_node = node.parent;

                        while (this_node.Equals(rosetree.root) == false)
                        {
                            haos_tree_writer.Write("->" + this_node.indices.tree_index);
                            this_node = this_node.parent;
                        }

                        haos_tree_writer.Write("->" + rosetree.root.indices.tree_index);

                        haos_tree_writer.WriteLine();
                    }

                    nodelist.RemoveAt(0);
                }
            }

            haos_tree_writer.Flush();
            haos_tree_writer.Close();
        }

        public string Process(string input_string)
        {
            string output_string = input_string;

            for (int i = 0; i < output_string.Length; i++)
            {
                if (output_string[i] == '\"' || output_string[i] == '>' || output_string[i] == '\'' || output_string[i] == '<' || output_string[i] == '{' || output_string[i] == '}')
                    output_string = output_string.Substring(0, i) + ' ' + output_string.Substring(i + 1);
            }
            return output_string;
        }
    }
}
