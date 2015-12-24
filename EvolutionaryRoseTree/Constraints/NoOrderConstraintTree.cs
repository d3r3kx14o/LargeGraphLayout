using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;
using RoseTreeTaxonomy.DataStructures;
using EvolutionaryRoseTree.DataStructures;
namespace EvolutionaryRoseTree.Constraints
{
    class NoOrderConstraintTree : ConstraintTree
    {
        public NoOrderConstraintTree(RoseTree rosetree, LoadFeatureVectors lfv)
            : base(rosetree, lfv)
        {
        }

        #region return probability: broken order numbers
        public override void GetMergeBrokenOrderNumbers(RoseTreeNode rtnode0, RoseTreeNode rtnode1,
            bool addbranch0, bool addbranch1,
            out int order2unorder, out int unorder2order)
        {
            //try
            {
                int order2unorder0, unorder2order0, order2unorder1, unorder2order1;
                ConstraintTreeNode node0 = MergedTrees[rtnode0.MergeTreeIndex];
                ConstraintTreeNode node1 = MergedTrees[rtnode1.MergeTreeIndex];
                ConstraintTreeNode commonancestor = GetCommonAncestor(node0, node1);

                //leafcnt = 0 only when there is no uncertainty
                int leafcnt1 = TryMergeToAncestor(rtnode0.MergeTreeIndex, node0, commonancestor, addbranch0, out order2unorder0, out unorder2order0);
                int leafcnt2 = TryMergeToAncestor(rtnode1.MergeTreeIndex, node1, commonancestor, addbranch1, out order2unorder1, out unorder2order1);

                order2unorder = order2unorder0 + order2unorder1;
                unorder2order = unorder2order0 + unorder2order1;
            }
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    throw new Exception("Error");
            //}
        }

        private int TryMergeToAncestor(int mergetreeindex,
            ConstraintTreeNode node, ConstraintTreeNode ancestor,
            bool addbranch, out int order2unorder, out int unorder2order)
        {
            node.SetActiveMergedTree(mergetreeindex);

            order2unorder = unorder2order = 0;
            int leaf = 0, leafssum;

            if (addbranch)
            {
                if (node.MergedLeafNumber < node.LeafNumber)  //they are equal with or without this judge
                {
                    int A = node.MergedLeafNumber;
                    int B = node.LeafNumber - node.MergedLeafNumber;
                    unorder2order += (A * A - node.MergedChildLeafSquareSum) / 2 * B;
                    leafssum = A * A + node.ChildLeafSquareSum - node.MergedChildLeafSquareSum;
                }
                else
                {
                    node = node.Parent;
                    leafssum = node.ChildLeafSquareSum;
                }
            }
            else
                leafssum = node.ChildLeafSquareSum;

            while (!node.Equals(ancestor))
            {
                leaf = node.LeafNumber;
                //calculate collapse cost with parent
                ConstraintTreeNode parent = node.Parent;
                order2unorder += (leaf * leaf - leafssum) / 2 * (parent.LeafNumber - leaf);
                //update leaf&leafssum
                leafssum = parent.ChildLeafSquareSum - leaf * leaf + leafssum;
                node = parent;
            }
            return leaf;
        }
        #endregion return probability: broken order numbers

    }
}
