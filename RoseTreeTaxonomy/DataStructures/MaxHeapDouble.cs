using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.DataStructures
{
    public class MaxHeapDouble
    {
        private int[] indices = null;
        private double[] values = null;
        private int size = 0;
        private int capacity = 0;

        public MaxHeapDouble(int[] indices, double[] values, int cap)
        {

            if (indices.Length != values.Length)
            {
                Console.WriteLine("Dimensions does not match!");
            }
            this.indices = (int[])indices.Clone();
            this.values = (double[])values.Clone();
            size = indices.Length;
            capacity = cap;
        }
        public MaxHeapDouble(int cap)
        {
            this.indices = new int[cap];
            this.values = new double[cap];
            capacity = cap;
            size = 0;
        }

        public int heapSize()
        {
            return size;
        }
        public bool isLeaf(int pos)
        {
            return (pos >= (size / 2)) && (pos < size);
        }
        public int leftChild(int pos)
        {
            if (pos > size / 2)
            {
                Console.WriteLine("Position has no left child");
            }
            return 2 * pos + 1;
        }
        public int rightChild(int pos)
        {
            if (pos > (size - 1) / 2)
            {
                Console.WriteLine("Position has no right child");
            }
            return 2 * pos + 2;
        }
        public int parent(int pos)
        { // Return position for parent
            if (pos < 0)
            {
                Console.WriteLine("Position has no parent");
            }
            return ((pos - 1) / 2);
        }
        public void swap(int index1, int index2)
        {
            int index = indices[index1];
            double value = values[index1];
            indices[index1] = indices[index2];
            values[index1] = values[index2];
            indices[index2] = index;
            values[index2] = value;
        }
        public void insert(int index, double value)
        {
            if (size >= capacity)
            {
                Console.WriteLine("Get heap max capacity!");
                return;
            }
            int curr = size++;
            if (size > indices.Length)
            {
                int[] newindices = new int[size];
                double[] newvalues = new double[size];
                for (int i = 0; i < size - 1; ++i)
                {
                    newindices[i] = indices[i];
                    newvalues[i] = values[i];
                }
                indices = newindices;
                values = newvalues;
            }
            indices[size - 1] = index;
            values[size - 1] = value;

            while (curr != 0 && values[curr] > values[parent(curr)])
            {
                swap(curr, parent(curr));
                curr = parent(curr);
            }
        }

        public void buildheap()
        {
            for (int i = (size / 2) - 1; i >= 0; --i)
            {
                heapify(i);
            }
        }

        private void heapify(int pos)
        {

            if (pos > size)
                return;
            int left = (pos + 1) * 2 - 1;
            int right = (pos + 1) * 2;

            int minidx = pos;
            if (left < size && cmp(left, pos) > 0)
                minidx = left;
            if (right < size && cmp(right, pos) >= 0 && cmp(right, left) >= 0)
                minidx = right;
            if (minidx != pos)
            {
                // swap them and recurse on the subtree rooted at minidx
                swap(minidx, pos);
                heapify(minidx);
            }
        }

        protected int cmp(int pos1, int pos2)
        {
            if (values[pos1] > values[pos2])
                return 1;
            else if (values[pos1] < values[pos2])
                return -1;
            else if (indices[pos1] > indices[pos2])
                return 1;
            else if (indices[pos1] < indices[pos2])
                return -1;
            else return 0;
        }

        public void changeMax(int index, double value)
        {
            if (size < 0)
            {
                Console.WriteLine("Changing: Empty heap");
            }
            indices[0] = index;
            values[0] = value;
            heapify(0);
        }

        public int removeMax()
        {
            if (size < 0)
            {
                Console.WriteLine("Removing: Empty heap");
            }
            --size;
            swap(0, size); // Swap maximum with last value
            if (size != 0)      // Not on last element
                heapify(0);   // Put new heap root val in correct place
            return indices[size];
        }

        public double max()
        {
            return values[0];
        }

        public int[] getIndices()
        {
            return indices;
        }

        public double[] getValues()
        {
            return values;
        }

        public static int[] heapSort(MaxHeapDouble maxHeap)
        {
            int[] newIndices = new int[maxHeap.getIndices().Length];
            int orgsize = maxHeap.size;
            for (int i = 0; i < orgsize; ++i)
            { // Now sort
                newIndices[i] = maxHeap.removeMax();   // removeMax places max value at end of heap
            }
            return newIndices;
        }
    }
}
