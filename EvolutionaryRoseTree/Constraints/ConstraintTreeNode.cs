using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
namespace EvolutionaryRoseTree.Constraints
{

    class ConstraintTreeNode
    {
        public ConstraintTreeNode Parent;
        public List<ConstraintTreeNode> Children { get; protected set; }

        public int InitialIndex = -1;   //same as merge tree index
        public int DrawTreeIndex;

        //public int LeafNumber { get; protected set; }                   //sum(a)
        //public int ChildLeafSquareSum { get; protected set; }  //sum(a^2)
        //public int MergedLeafNumber { get { return ActiveMergedTree.LeafNumber; } }
        //public int MergedChildLeafSquareSum { get { return ActiveMergedTree.ChildLeafSquareNumber; } }
        public double LeafNumber { get; protected set; }                   //sum(a)
        public double ChildLeafSquareSum { get; protected set; }  //sum(a^2)
        public double MergedLeafNumber { get { return ActiveMergedTree.LeafNumber; } }
        public double MergedChildLeafSquareSum { get { return ActiveMergedTree.ChildLeafSquareNumber; } }
#if NORMALIZE_PROJ_WEIGHT
        public int NWLeafNumber { get; protected set; }                   //sum(a)
        public int NWChildLeafSquareSum { get; protected set; }  //sum(a^2)
        public int NWMergedLeafNumber { get { return ActiveMergedTree.NWLeafNumber; } }
        public int NWMergedChildLeafSquareSum { get { return ActiveMergedTree.NWChildLeafSquareNumber; } }
#endif

        public Dictionary<int, MergedTree> MergedChildren = null;
        public MergedTree ActiveMergedTree { get; protected set; }

        public int NearestNeighbourArrayIndex = -1;
        public int NewDocumentNearestNeighbourArrayIndex = -1;
        public Dictionary<int, double> ContainedInformation { get; protected set; }
        public bool IsFreeNode { get; protected set; }
        //public bool IsOpenedNode;

        public int DocumentNumber = 1;

        public ConstraintTreeNode(bool bFreeNode = false)
        {
            Parent = null;
            Children = null;
            IsFreeNode = bFreeNode;
        }

        public ConstraintTreeNode(ConstraintTreeNode parent)
        {
            Parent = parent;
            Children = null;
            IsFreeNode = false;
        }

        public ConstraintTreeNode(ConstraintTreeNode parent, MergedTree splitMergedTree)
        {
            this.Parent = parent;
            this.AddChildren(splitMergedTree);
            //this.AddChildren(splitMergedTree.MergedChildren, splitMergedTree.LeafNumber, splitMergedTree.ChildLeafSquareNumber);

            SetActiveMergedTree(splitMergedTree);
            this.InitialIndex = splitMergedTree.MergeTreeIndex;
            IsFreeNode = false;
        }

        public bool IsUnfinished()
        {
            if (Children == null)
                return false;
            if (OriginalLinkedNodeIndex < 0)
                return false;
            if (MergedChildren != null && MergedChildren.Count == 1)
            {
                foreach (MergedTree mergedtree in MergedChildren.Values)
                    if (mergedtree.MergedChildren.Count == Children.Count)
                        return false;
            }
            return true;
        }

        public void SetAsFreeNode()
        {
            if (!IsFreeNode)
            {
                IsFreeNode = true;

                var parent = this.Parent;
                parent.Children.Remove(this);
                this.Parent = null;
                NewDocumentNearestNeighbourArrayIndex = NearestNeighbourArrayIndex;
                NearestNeighbourArrayIndex = -1;

                UpdateLeafNumbers();
            }
        }

        public void CreateChildren(int number)
        {
            if (Children == null)
                Children = new List<ConstraintTreeNode>();
            for (int i = 0; i < number; i++)
            {
                Children.Add(NewConstraintNode(this));
            }
        }

        public void AddFreeChildren(ConstraintTreeNode node0, ConstraintTreeNode node1,
            bool addbranch0, bool addbranch1)
        {
            if (!IsFreeNode || !node0.IsFreeNode || !node1.IsFreeNode || this.Children != null)
                throw new Exception("Only Free Nodes can call this function!");
            if (addbranch0)
                AddChild(node0);
            else
                foreach (ConstraintTreeNode child_node0 in node0.Children)
                    AddChild(child_node0);

            if (addbranch1)
                AddChild(node1);
            else
                foreach (ConstraintTreeNode child_node1 in node1.Children)
                    AddChild(child_node1);

            UpdateLeafNumbers();
        }

        public ConstraintTreeNode AttachFreeNode(ConstraintTreeNode freenode, bool addbranch)//,ConstraintTreeNode root, double rootDocumentCount)
        {
            /// Attach free node ///
            //if (freenode.Children == null)
            //{
            //    freenode.SetLeafMergedTree(freenode.InitialIndex);
            //    AddFreeChild(freenode);
            //}
            if (addbranch)
            {
                if (freenode.Children == null)
                    freenode.SetLeafMergedTree(freenode.InitialIndex);
                else
                {
                    MergedTree mergedtree = new MergedTree(freenode.InitialIndex);
                    foreach (ConstraintTreeNode child_freenode in freenode.Children)
                        mergedtree.AddNode(child_freenode);
                    freenode.MergedChildren = new Dictionary<int, MergedTree>();
                    freenode.MergedChildren.Add(freenode.InitialIndex, mergedtree);
                }
                AddFreeChild(freenode);
                return freenode;
            }
            else
            {
                MergedTree mergedtree = new MergedTree(freenode.InitialIndex);
                foreach (ConstraintTreeNode child_freenode in freenode.Children)
                {
                    AddFreeChild(child_freenode);
                    mergedtree.AddNode(child_freenode);
                }
                if (MergedChildren == null)
                    MergedChildren = new Dictionary<int, MergedTree>();
                this.MergedChildren.Add(freenode.InitialIndex, mergedtree);
                return this;
            }

            //freenode.Parent = this;
            //freenode.IsFreeNode = false;

            ///// calculate contained information for every child ///
            //int rootNearestNeighbor = root.NearestNeighbourArrayIndex;
            //List<ConstraintTreeNode> queue = new List<ConstraintTreeNode>();
            //queue.Add(freenode);
            //while (queue.Count != 0)
            //{
            //    ConstraintTreeNode ctnode = queue[0];
            //    queue.RemoveAt(0);

            //    if (ctnode.Children != null)
            //    {
            //        Dictionary<int, double> info = new Dictionary<int, double>();
            //        info.Add(rootNearestNeighbor, ctnode.DocumentNumber / rootDocumentCount);
            //        ctnode.InitializeContainedInformation(info);

            //        queue.AddRange(ctnode.Children);
            //    }
            //}

            ///// pass contained info to ancestors ///
            //Dictionary<int, double> attached_freeinfo;
            //if (freenode.Children == null)
            //{
            //    attached_freeinfo = new Dictionary<int, double>();
            //    attached_freeinfo.Add(rootNearestNeighbor, 1 / rootDocumentCount);
            //}
            //else
            //    attached_freeinfo = freenode.ContainedInformation;
            //ConstraintTreeNode addinfo_ctnode = this;
            //while (addinfo_ctnode != root)
            //{
            //    addinfo_ctnode.AddContainedInformation(attached_freeinfo);
            //    addinfo_ctnode = addinfo_ctnode.Parent;
            //}
        }

        public void AddChildren(MergedTree splitMergedTree)
        {
            List<ConstraintTreeNode> children = splitMergedTree.MergedChildren;

            if (Children == null)
                Children = new List<ConstraintTreeNode>();
            Children.AddRange(children);
            foreach (ConstraintTreeNode child in children)
                child.Parent = this;

            this.LeafNumber += splitMergedTree.LeafNumber;
            this.ChildLeafSquareSum += splitMergedTree.ChildLeafSquareNumber;
#if NORMALIZE_PROJ_WEIGHT
            this.NWLeafNumber += splitMergedTree.NWLeafNumber;
            this.NWChildLeafSquareSum += splitMergedTree.NWChildLeafSquareNumber;
#endif
        }


        public ConstraintTreeNode CreateChild()
        {
            if (Children == null)
                Children = new List<ConstraintTreeNode>();
            ConstraintTreeNode child = NewConstraintNode(this);
            Children.Add(child);
            return child;
        }

        public ConstraintTreeNode AddCousin()
        {
            ConstraintTreeNode cousin = null;
            if (Parent == null)
                cousin = CreateChild();
            else
                cousin = Parent.CreateChild();
            return cousin;
        }

        public void AddChild(ConstraintTreeNode child)
        {
            if (Children == null)
                Children = new List<ConstraintTreeNode>();
            child.Parent = this;
            Children.Add(child);
        }

        void AddFreeChild(ConstraintTreeNode freechild)
        {
            AddChild(freechild);
            //freechild.IsFreeNode = false;

            if (freechild.Children != null)
            {
                Dictionary<int, double> containedinfo = new Dictionary<int, double>();
                freechild.InitializeContainedInformation(containedinfo);
            }
        }

        protected ConstraintTreeNode newTopicNode = null;
        public ConstraintTreeNode AddNewTopic()
        {
            if (newTopicNode == null)
            {
                newTopicNode = NewConstraintNode();
                return newTopicNode;
            }
            else
            {
                if (newTopicNode.Children == null)
                {
                    ConstraintTreeNode newTopicLeaf = newTopicNode;
                    newTopicNode = NewConstraintNode();
                    newTopicNode.AddChild(newTopicLeaf);
                    //Create a new child
                    ConstraintTreeNode newNode = newTopicNode.CreateChild();
                    return newNode;
                }
                else
                {
                    ConstraintTreeNode newNode = newTopicNode.CreateChild();
                    return newNode;
                }
            }
        }

        public void AddNewTopicNodesToChildren()
        {
            if (newTopicNode != null)
                AddChild(newTopicNode);
        }

        public void SetLeafMergedTree(int mergetreeindex)
        {
            if (this.Children != null)
                throw new Exception("Only deal with leaf node!");

            //this.UpdateLeafNumbers();
            MergedChildren = new Dictionary<int, MergedTree>();
            MergedTree mergedtree = new MergedTree(mergetreeindex);
            mergedtree.AddNode(this);
            MergedChildren.Add(mergetreeindex, mergedtree);
        }

        public void UpdateLeafNodeLeafNumbers(double weight)
        {
            if (Children != null)
                throw new Exception("Call this function only when it is leaf nodes!");
            LeafNumber = weight;
            ChildLeafSquareSum = weight * weight;
#if NORMALIZE_PROJ_WEIGHT
            NWLeafNumber = 1;
            NWChildLeafSquareSum = 1;
#endif
        }

        public void UpdateLeafNumbers()
        {
            if (Children == null)
            {
                LeafNumber = 1;
                ChildLeafSquareSum = 1;
            }
            else
            {
                LeafNumber = 0;
                DocumentNumber = 0;
                ChildLeafSquareSum = 0;
#if NORMALIZE_PROJ_WEIGHT
                NWLeafNumber = 0;
                NWChildLeafSquareSum = 0;
#endif
                foreach (ConstraintTreeNode child in Children)
                {
                    LeafNumber += child.LeafNumber;
                    ChildLeafSquareSum += child.LeafNumber * child.LeafNumber;
                    DocumentNumber += child.DocumentNumber;
#if NORMALIZE_PROJ_WEIGHT
                    NWLeafNumber += child.NWLeafNumber;
                    NWChildLeafSquareSum += child.NWLeafNumber * child.NWLeafNumber;
#endif
                }
            }
        }

        public void SetActiveMergedTree(int mergeindex)
        {
            //if (this.Children == null)
            //    return;
            if (MergedChildren == null || !MergedChildren.ContainsKey(mergeindex))
                throw new Exception("Cannot find merged tree!");
            ActiveMergedTree = MergedChildren[mergeindex];
            //ActiveMergedTree.Container = this;
        }

        public void SetActiveMergedTree(MergedTree mergedtree)
        {
            ActiveMergedTree = mergedtree;
            //ActiveMergedTree.Container = this;
        }

        //only to be used when building constraint tree
        public void CollapseLinkWithParentNoUpdate()
        {
            Parent.Children.InsertRange(Parent.Children.IndexOf(this), Children);
            foreach (ConstraintTreeNode child in Children)
                if (child.Children == null && child.NearestNeighbourArrayIndex == this.NearestNeighbourArrayIndex)
                    child.NearestNeighbourArrayIndex = Parent.NearestNeighbourArrayIndex; 
            Parent.Children.Remove(this);
            foreach (ConstraintTreeNode child in Children)
                child.Parent = Parent;
            
            this.Parent = null;
        }

        //static StreamWriter ofile_splitmerge = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\splitmerge.dat");
        public virtual ConstraintTreeNode CollapseLinkWithChild(ConstraintTreeNode child)
        {
            //Console.WriteLine("Collapse Link: Parent {0}, Child {1}", this.OriginalLinkedNodeIndex, child.OriginalLinkedNodeIndex);
            //double newnodeinforatio = child.LeafNumber / this.LeafNumber;

            //collapse Children's field
            //if (Children.IndexOf(child) < 0)
            //    Console.Write("");
            Children.InsertRange(Children.IndexOf(child), child.Children);
            Children.Remove(child);
            foreach (ConstraintTreeNode addchild in child.Children)
                addchild.Parent = this;
            //child.Parent = null;
            ChildLeafSquareSum += child.ChildLeafSquareSum - child.LeafNumber * child.LeafNumber;
#if NORMALIZE_PROJ_WEIGHT
            NWChildLeafSquareSum += child.NWChildLeafSquareSum - child.NWLeafNumber * child.NWLeafNumber;
#endif

            if (child.MergedChildren != null)
            {
                if (MergedChildren == null)
                    MergedChildren = new Dictionary<int, MergedTree>();
                foreach (int mergeindex in child.MergedChildren.Keys)
                    MergedChildren.Add(mergeindex, child.MergedChildren[mergeindex]);
                SetActiveMergedTree(child.ActiveMergedTree);
            }

            child.Parent = null;
            //ofile_splitmerge.WriteLine("Collapse: Parent{0}  Child{1}<{2}>", ContainedInfoToString(), child.ContainedInfoToString(), child.InitialIndex);
            //ofile_splitmerge.Flush();

            //calculate information			
            //child.ShrinkInformation(newnodeinforatio);
            //Console.WriteLine("Collapse: Parent{0}  Child{1}", ContainedInfoToString(), child.ContainedInfoToString());
            //Console.WriteLine("AfterC: Parent{0}", ContainedInfoToString());

            if (this.OriginalLinkedNodeIndex >= 0 && child.OriginalLinkedNodeIndex >= 0)
                this.AddContainedInformation(child.ContainedInformation);

            return this;
        }

        public virtual ConstraintTreeNode Split(InheritParentInfo inheritParentInfos, bool bPutInEnd = false)
        {
            //Console.WriteLine("Split: Parent {0}, Child Merge Tree index: [{1}]", this.OriginalLinkedNodeIndex, ActiveMergedTree.MergeTreeIndex);
            //if (ActiveMergedTree.MergeTreeIndex == 188)
            //    Console.Write("");
            //double newnodeinforatio = (double)MergedLeafNumber / this.LeafNumber;

            //Find the child index to be split from
            List<ConstraintTreeNode> mergedchildren = ActiveMergedTree.MergedChildren;
            int index;
            if (!bPutInEnd)
            {
                ConstraintTreeNode largestChild = null; //mergedchildren[0];//
                double largestLeafNumber = double.MinValue;
                foreach (ConstraintTreeNode mergedchild in mergedchildren)
                    if (mergedchild.LeafNumber > largestLeafNumber)
                    {
                        largestChild = mergedchild;
                        largestLeafNumber = mergedchild.LeafNumber;
                    }
                index = Children.IndexOf(largestChild);
            }
            else
            {
                index = Children.Count;
            }
            //while (!mergedchildren.Contains(Children[index])) index++;

            //put activate merged tree to new node, remove from old node
            ConstraintTreeNode newnode = NewConstraintNode(this, ActiveMergedTree);
#if OPEN_LARGE_CLUSTER || ADJUST_TREE_STRUCTURE
            if (this.MergedChildren != null)
                this.MergedChildren.Remove(ActiveMergedTree.MergeTreeIndex);
#else
            this.MergedChildren.Remove(ActiveMergedTree.MergeTreeIndex);
#endif
            //try
            //{
            Children.Insert(index, newnode);
            //}
            //catch
            //{
            //    Console.Write("");
            //}
            //remove from old node's Children field
            foreach (ConstraintTreeNode child in mergedchildren)
                Children.Remove(child);
            ChildLeafSquareSum += MergedLeafNumber * MergedLeafNumber - MergedChildLeafSquareSum;
#if NORMALIZE_PROJ_WEIGHT
            NWChildLeafSquareSum += NWMergedLeafNumber * NWMergedLeafNumber - NWMergedChildLeafSquareSum;
#endif
            //ofile_splitmerge.WriteLine("Split: Parent{0}  Child{1}<{2}>", ContainedInfoToString(), newnode.ContainedInformation, newnode.InitialIndex);
            //ofile_splitmerge.Flush();

            //calculate information
            //Console.WriteLine("Split: Parent{0}", ContainedInfoToString());
            //foreach (ConstraintTreeNode child in mergedchildren)
            //    Console.Write("{0}\t", child.ContainedInfoToString());
            //Console.WriteLine();
            UpdateSplitContainedInformation(newnode, inheritParentInfos);
            //Console.WriteLine("AfterS: Parent{0}  New Child{1}", ContainedInfoToString(), newnode.ContainedInfoToString());

            return newnode;
        }

        public virtual void MergeTree(int mergetreeindex,
            int mergetreeindex0, ConstraintTreeNode node0, ConstraintTreeNode container0, 
            int mergetreeindex1, ConstraintTreeNode node1, ConstraintTreeNode container1)
        {
            MergedTree mergedtree = new MergedTree(mergetreeindex);
            if (MergedChildren == null)
                MergedChildren = new Dictionary<int, MergedTree>();
            MergedChildren.Add(mergetreeindex, mergedtree);

            if (this.Equals(container0))
            {
                mergedtree.AddMergedTree(this.MergedChildren[mergetreeindex0]);
                this.MergedChildren.Remove(mergetreeindex0);
            }
            else
                mergedtree.AddNode(container0);

            if (this.Equals(container1))
            {
                mergedtree.AddMergedTree(this.MergedChildren[mergetreeindex1]);
                this.MergedChildren.Remove(mergetreeindex1);
            }
            else
                mergedtree.AddNode(container1);
        }

        #region record information change
        public int OriginalLinkedNodeIndex = -1;
        public void InitializeCorrespondingInformation(int mergetreeindex)
        {
            ContainedInformation = new Dictionary<int, double>();
            ContainedInformation.Add(mergetreeindex, 1);

            OriginalLinkedNodeIndex = mergetreeindex;
        }

        protected void UpdateSplitContainedInformation(ConstraintTreeNode newnode,
            InheritParentInfo inheritParentInfos)
        {
            ////calculate newnode's information
            Dictionary<int, double> information = new Dictionary<int, double>();
            foreach (ConstraintTreeNode child_newnode in newnode.Children)
            {
                if (child_newnode.Children == null)
                {
                    //document
                    int parentIndex = inheritParentInfos.DocumentInheritParentInfo_Index[child_newnode.InitialIndex];
                    double parentWeight = inheritParentInfos.DocumentInheritParentInfo_Weight[child_newnode.InitialIndex];
                    if (parentWeight > 0) AddToInformation(information, parentIndex, parentWeight);
                }
                else if (child_newnode.OriginalLinkedNodeIndex >= 0)
                {
                    //topic: original node. structure != content.
                    int contentIndex = child_newnode.OriginalLinkedNodeIndex;
                    double contentWeight = child_newnode.ContainedInformation[contentIndex];
                    KeyValuePair<int, double> parentInfo = inheritParentInfos.TopicInheritParentInfos[contentIndex];
                    AddToInformation(information, parentInfo.Key, contentWeight * parentInfo.Value);
                }
                else
                {
                    //topic: split node (newly generated node). structure = content.
                    foreach (KeyValuePair<int, double> info in child_newnode.ContainedInformation)
                    {
                        if (info.Key == this.OriginalLinkedNodeIndex)
                            AddToInformation(information, info.Key, info.Value);
                        else
                        {
                            //if (!inheritParentInfos.TopicInheritParentInfos.ContainsKey(info.Key))
                            //    Console.Write("");
                            KeyValuePair<int, double> parentInfo = inheritParentInfos.TopicInheritParentInfos[info.Key];
                            if (parentInfo.Key == this.OriginalLinkedNodeIndex)
                                AddToInformation(information, info.Key, info.Value);
                            else
                                AddToInformation(information, parentInfo.Key, info.Value * parentInfo.Value);
                        }
                    }
                }
            }

            newnode.InitializeContainedInformation(information);
#if OPEN_LARGE_CLUSTER
            if (this.OriginalLinkedNodeIndex >= 0)
                this.RemoveContainedInformation(information);
#else
            this.RemoveContainedInformation(information);
#endif
        }

        static void AddToInformation(Dictionary<int, double> information, int index, double infoweight)
        {
            if (information.ContainsKey(index))
                information[index] += infoweight;
            else
                information.Add(index, infoweight);
        }

        public void InitializeContainedInformation(Dictionary<int, double> information)
        {
            this.ContainedInformation = information;
        }

        public void RemoveContainedInformation(Dictionary<int, double> information)
        {
            foreach(KeyValuePair<int, double> info in information)
                if (info.Key != this.OriginalLinkedNodeIndex)   //do not substract A itself
                {
                    //if (!ContainedInformation.ContainsKey(info.Key))
                    //    if (info.Value < 1e-8)
                    //        continue;
                    //    else
                    //        Console.Write("");
                    if (ContainedInformation.ContainsKey(info.Key))
                    {
                        ContainedInformation[info.Key] -= info.Value;
                        if (ContainedInformation[info.Key] < 1e-8)
                            ContainedInformation.Remove(info.Key);
                    }
                }
        }

        public void AddContainedInformation(Dictionary<int, double> information)
        {
            if (information == null)
                return;
            if (ContainedInformation == null)
            {
                ContainedInformation = new Dictionary<int, double>();
                foreach (int treeindex in information.Keys)
                    ContainedInformation.Add(treeindex, information[treeindex]);
            }
            else
            {
                foreach (int treeindex in information.Keys)
                {
                    if (ContainedInformation.ContainsKey(treeindex))
                    {
                        ContainedInformation[treeindex] += information[treeindex];
                        //if (ContainedInformation[treeindex] > 1)
                        //    ContainedInformation[treeindex] = 1;
                    }
                    else
                        ContainedInformation.Add(treeindex, information[treeindex]);
                }
            }
        }

        //public void ShrinkInformation(double factor)
        //{
        //    if (ContainedInformation == null)
        //        return;
        //    List<int> treeindices = new List<int>(ContainedInformation.Keys);
        //    foreach (int treeindex in treeindices)
        //        ContainedInformation[treeindex] *= factor;
        //}

        //public void PostProcessContainedInformation(InheritParentInfo inheritParentInfo)
        //{
        //    Dictionary<int, double> postContainedInformation = new Dictionary<int, double>();
        //    foreach (KeyValuePair<int, double> info in ContainedInformation)
        //        postContainedInformation.Add(info.Key,
        //            info.Value * inheritParentInfo.TopicSize[info.Key] / this.LeafNumber);
        //    this.ContainedInformation = postContainedInformation;
        //}

        public string ContainedInfoToString()
        {
            string str = string.Format("[({0})    ", OriginalLinkedNodeIndex);
            if (ContainedInformation != null)
                foreach (KeyValuePair<int, double> kvp in ContainedInformation)
                    str += string.Format(",~{0} ({1})~", kvp.Key, kvp.Value);
            else
                str += "null";
            str += "]";
            return str;
        }
        #endregion record information change

        #region for loose order constraint inherit
        protected virtual ConstraintTreeNode NewConstraintNode()
        {
            return new ConstraintTreeNode();
        }

        protected virtual ConstraintTreeNode NewConstraintNode(ConstraintTreeNode parent)
        {
            return new ConstraintTreeNode(parent);
        }

        protected virtual ConstraintTreeNode NewConstraintNode(ConstraintTreeNode parent, MergedTree splitMergedTree)
        {
            return new ConstraintTreeNode(parent, splitMergedTree);
        }

        #endregion 
    }

    class MergedTree
    {
        public MergedTree(int mergetreeindex)
        {
            MergeTreeIndex = mergetreeindex;

            MergedChildren = new List<ConstraintTreeNode>();
            LeafNumber = 0;
            ChildLeafSquareNumber=0;
#if NORMALIZE_PROJ_WEIGHT
            NWLeafNumber = 0;
            NWChildLeafSquareNumber = 0;
#endif
        }

        public void AddNode(ConstraintTreeNode node)
        {
            MergedChildren.Add(node);
            LeafNumber += node.LeafNumber;
            ChildLeafSquareNumber += node.LeafNumber * node.LeafNumber;
#if NORMALIZE_PROJ_WEIGHT
            NWLeafNumber += node.NWLeafNumber;
            NWChildLeafSquareNumber += node.NWLeafNumber * node.NWLeafNumber;
#endif
        }

        public void AddMergedTree(MergedTree mergedtree)
        {
            MergedChildren.AddRange(mergedtree.MergedChildren);
            LeafNumber += mergedtree.LeafNumber;
            ChildLeafSquareNumber += mergedtree.ChildLeafSquareNumber;
#if NORMALIZE_PROJ_WEIGHT
            NWLeafNumber += mergedtree.NWLeafNumber;
            NWChildLeafSquareNumber += mergedtree.NWChildLeafSquareNumber;
#endif
        }

        public int MergeTreeIndex;

        public List<ConstraintTreeNode> MergedChildren;
        public double LeafNumber;
        public double ChildLeafSquareNumber;
#if NORMALIZE_PROJ_WEIGHT
        public int NWLeafNumber;
        public int NWChildLeafSquareNumber;
#endif

        //public ConstraintTreeNode Container;    //Kept track of when activated
    }
}
