using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Constraints
{
    class RemoveContraintTreeConflicts
    {
        List<ConstraintTree> ConstraintTrees;
        RemoveConflictParameters RemoveConflictParameters;

        public RemoveContraintTreeConflicts(List<ConstraintTree> constraintTrees,
            RemoveConflictParameters removeConflictParameters)
        {
            this.ConstraintTrees = constraintTrees;
            this.RemoveConflictParameters = removeConflictParameters;
        }

        public void Start()
        {
            if (ConstraintTrees == null || ConstraintTrees.Count < 2)
                return;

            //DrawConstraintTrees();

            var constraintTreesCnt = ConstraintTrees.Count;
            for (int i = 0; i < constraintTreesCnt - 1; i++)
            {
                RemoveConflicts(ConstraintTrees[i], ConstraintTrees[i + 1], RemoveConflictParameters);
            }

            var commonStructures = GetCommonStructures(ConstraintTrees[constraintTreesCnt - 1]);
            
            for (int i = 0; i < constraintTreesCnt - 1; i++)
            {
                SetCommonStructures(ConstraintTrees[i], commonStructures);
            }
        }

        private void DrawConstraintTrees()
        {
            for (int i = 0; i < ConstraintTrees.Count; i++)
            {
                this.ConstraintTrees[i].DrawConstraintTree(@"C:\Users\v-xitwan\Desktop\temp\RoseTree\CT" + i + ".gv");
            }
        }

        public void RemoveConflicts(ConstraintTree constraintTree1, ConstraintTree constraintTree2,
            RemoveConflictParameters removeConflictParameters)
        {
            //Get match result
            var treeMatch = new ConstraintTreeMatching(constraintTree1, constraintTree2,
                removeConflictParameters);
            var matchedIDs = treeMatch.Match();

            Console.WriteLine(this.RemoveConflictParameters);
            foreach(var matchedID in matchedIDs)
            {
                Console.Write(matchedID + " ");
            }
            Console.WriteLine();
            Console.WriteLine("-----------------------");

            //Set the matched nodes as free nodes
            constraintTree1.SetOthersAsFreeNodes(matchedIDs);
            constraintTree2.SetOthersAsFreeNodes(matchedIDs);
        }

        private int[] GetCommonStructures(ConstraintTree constraintTree)
        {
            return constraintTree.GetCommonStructures();
        }

        private void SetCommonStructures(ConstraintTree constraintTree, int[] commonStructures)
        {
            constraintTree.SetOthersAsFreeNodes(commonStructures);
        }

        internal void RemoveDistanceConstraintConflict(List<TreeDistanceConstraint> distanceConstraints)
        {
            if (distanceConstraints.Count < 2)
                return;
            if (distanceConstraints.Count != ConstraintTrees.Count)
                throw new Exception("Error!");

            for (int i = 0; i < ConstraintTrees.Count; i++)
            {
                distanceConstraints[i].RemoveConflicts(ConstraintTrees[i]);
            }
        }
    }

    class RemoveConflictParameters
    {
        public RemoveConflictParameters(
            double Ratio,
            double NodeDeleteCost, double NodeInsertCost, double NodeSubstituteCost,
            double EdgeDeleteCost, double EdgeInsertCost, double EdgeSubstituteCost)
        {
            this.Ratio = Ratio;
            this.NodeDeleteCost = NodeDeleteCost;
            this.NodeInsertCost = NodeInsertCost;
            this.NodeSubstituteCost = NodeSubstituteCost;
            this.EdgeDeleteCost = EdgeDeleteCost;
            this.EdgeInsertCost = EdgeInsertCost;
            this.EdgeSubstituteCost = EdgeSubstituteCost;
        }

        public double Ratio = 0.8;
        public double NodeDeleteCost = 1;
        public double NodeInsertCost = 1;
        public double NodeSubstituteCost = 70;
        public double EdgeDeleteCost = 10;
        public double EdgeInsertCost = 10;
        public double EdgeSubstituteCost = 10;

        public override string ToString()
        {
            string str = "";
            str += "Node: ";
            str += NodeDeleteCost + " " + NodeInsertCost + " " + NodeSubstituteCost + ";\t";
            str += "Edge: ";
            str += EdgeDeleteCost + " " + EdgeInsertCost + " " + EdgeSubstituteCost + ";";
            return str;
        }
    }
}
