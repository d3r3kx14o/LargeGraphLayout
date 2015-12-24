using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Constraints
{
    class InheritParentInfo
    {
        public Dictionary<int, KeyValuePair<int, double>> TopicInheritParentInfos;    //inter
        public int[] DocumentInheritParentInfo_Index;
        public double[] DocumentInheritParentInfo_Weight;

        public Dictionary<int, double> TopicSize;
    }
}
