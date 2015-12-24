using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.DataStructures
{
    public class LinkedPositions
    {
        public linkedNode root;
        public linkedNode last;
        public int size;

        public LinkedPositions()
        {
            linkedNode linkedNode = new linkedNode();
            root = linkedNode;
            last = linkedNode;
        }

        public class linkedNode
        {
            public int position;
            public RoseTreeNode node;
            public linkedNode next;
            public linkedNode pre;
        }

        public void remove(linkedNode removenode)
        {
            if (removenode.pre != null)
                removenode.pre.next = removenode.next;
            if (removenode.next != null)
                removenode.next.pre = removenode.pre;
            else
                this.last = removenode.pre;
            this.size--;
        }

        public void put(RoseTreeNode node, int pointer)
        {
            linkedNode linkedNode = new linkedNode();
            linkedNode.node = node;
            linkedNode.position = pointer;
            linkedNode.pre = last;
            last.next = linkedNode;
            last = linkedNode;
            size++;
        }

        public linkedNode getRootlinkedNode()
        {
            return root;
        }

        public bool contains(RoseTreeNode node)
        {
            bool ret = false;
            linkedNode start = last;

            while (start != root)
            {
                if (start.node.Equals(node) == true)
                {
                    ret = true;
                    break;
                }
                start = start.pre;
            }
            return ret;
        }
    }
}
