using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoseTreeTaxonomy.DataStructures
{
    public class LinkedInvertedIndex
    {
        public Dictionary<int, LinkedPositions> positionsMap = new Dictionary<int, LinkedPositions>();

        public LinkedPositions get(int key)
        {
            if (positionsMap.ContainsKey(key) == true)
                return positionsMap[key];
            return null;
        }

        public void put(int key, RoseTreeNode node, int pointer)
        {
            LinkedPositions positions = null;
            if (positionsMap.ContainsKey(key) == true)
                positions = positionsMap[key];
            if (positions == null)
            {
                positions = new LinkedPositions();
                positionsMap.Add(key, positions);
            }
            positions.put(node, pointer);
        }

        public int size()
        {
            return positionsMap.Count;
        }

        public int[] keySet()
        {
            return positionsMap.Keys.ToArray();
        }
    }
}
