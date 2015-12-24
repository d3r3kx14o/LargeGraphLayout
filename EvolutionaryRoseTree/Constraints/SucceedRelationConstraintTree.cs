using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;

using EvolutionaryRoseTree.DataStructures;
namespace EvolutionaryRoseTree.Constraints
{
    class SucceedRelationConstraintTree : ConstraintTree
    {
        public SucceedRelationConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv,
            DataProjectionRelation projectRelation)
            : base(rosetree, lfv, projectRelation)
        {
            //for (int inode = 0; inode < MergedTrees.Length / 2; inode++)
            //{
            //    ConstraintTreeNode cnode = MergedTrees[inode];
            //    if (cnode.InitialIndex < 0)
            //        Console.Write("");
            //}
            //Console.Write("");
        }

        protected override DataProjection InitializeDataProjection(RoseTree rosetree, LoadFeatureVectors lfv)
        {
            return new SucceedDataProjection(projectRelation);
        }

        protected override int GetProjectedArrayIndex(SparseVectorList vector, int vectorid, out NodeProjectionType projType)
        {
            return (dataprojection as SucceedDataProjection).
                GetProjectedArrayIndex(vectorid, out projType);
        }

        protected override void RemoveOriginalLeaves()
        {
            try
            {
                List<int> openednodeindices = new List<int>();
                foreach (RoseTreeNode rtnode in (rosetree as ConstrainedRoseTree).OpenedNodeList)
                    openednodeindices.Add(rtnode.MergeTreeIndex);

                for (int i = 0; i < rosetreeleaves.Count; i++)
                {
                    ConstraintTreeNode orgtreenode = OriginalConstraintTreeNodes[i];
                    if (orgtreenode != null && orgtreenode.Children == null)
                    {
                        //remove this node from constrainttree
                        ConstraintTreeNode parent = orgtreenode.Parent;
                        if (parent != null)
                            if (parent.Children.Count > 2)
                                parent.Children.Remove(orgtreenode);
                            else
                            {
                                parent.Children.Remove(orgtreenode);
                                if (parent.Parent != null)
                                {
                                    if (parent.Children[0].Children == null ||
                                        !openednodeindices.Contains(parent.NearestNeighbourArrayIndex))
                                    {
                                        parent.CollapseLinkWithParentNoUpdate();
                                    }
                                    else
                                    {
                                        parent.Children[0].CollapseLinkWithParentNoUpdate();
                                    }
                                }
                                else
                                    if (parent.Children.Count == 0)
                                        this.Root = null;
                                    else
                                    {
                                        this.Root = parent.Children[0];
                                        this.Root.Parent = null;
                                    }
                            }
                        else
                            this.Root = null;

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    class SucceedRelationLooseConstraintTree : LooseConstraintTree
    {
        public SucceedRelationLooseConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv,
            DataProjectionRelation projectRelation)
            : base(rosetree, lfv, projectRelation)
        {
        }

        protected override DataProjection InitializeDataProjection(RoseTree rosetree, LoadFeatureVectors lfv)
        {
            return new SucceedDataProjection(projectRelation);
        }

        protected override int GetProjectedArrayIndex(SparseVectorList vector, int vectorid, out NodeProjectionType projType)
        {
            //if ((dataprojection as SucceedDataProjection) == null)
            //    Console.Write("");
            return (dataprojection as SucceedDataProjection).
                GetProjectedArrayIndex(vectorid, out projType);
        }

        protected override void RemoveOriginalLeaves()
        {
            try
            {
                List<int> openednodeindices = new List<int>();
                foreach (RoseTreeNode rtnode in (rosetree as ConstrainedRoseTree).OpenedNodeList)
                    openednodeindices.Add(rtnode.MergeTreeIndex);

                for (int i = 0; i < rosetreeleaves.Count; i++)
                {
                    ConstraintTreeNode orgtreenode = OriginalConstraintTreeNodes[i];
                    if (orgtreenode != null && orgtreenode.Children == null)
                    {
                        //remove this node from constrainttree
                        ConstraintTreeNode parent = orgtreenode.Parent;
                        if (parent != null)
                            if (parent.Children.Count > 2)
                                parent.Children.Remove(orgtreenode);
                            else
                            {
                                parent.Children.Remove(orgtreenode);
                                if (parent.Parent != null)
                                {
                                    //if (parent.Children.Count == 0)
                                    //    Console.WriteLine("parent Count is 0!");
                                    if (parent.Children[0].Children == null ||
                                        !openednodeindices.Contains(parent.NearestNeighbourArrayIndex))
                                    {
                                        parent.CollapseLinkWithParentNoUpdate();
                                    }
                                    else
                                    {
                                        parent.Children[0].CollapseLinkWithParentNoUpdate();
                                    }
                                }
                                else
                                    if (parent.Children.Count == 0)
                                        this.Root = null;
                                    else
                                    {
                                        this.Root = parent.Children[0];
                                        this.Root.Parent = null;
                                    }
                            }
                        else
                            this.Root = null;

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
