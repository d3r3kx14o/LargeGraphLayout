using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RoseTreeTaxonomy.Algorithms;
using EvolutionaryRoseTree.Constraints;
namespace EvolutionaryRoseTree.Experiments
{
    abstract class ConstraintParameter
    {
        protected bool bCalculateSmoothness;

        public void Set(RoseTree ConstraintRoseTree, RoseTree ConstraintRoseTree2 = null)
        {
            List<RoseTree> ConstraintRoseTrees = new List<RoseTree>();
            ConstraintRoseTrees.Add(ConstraintRoseTree);

            Set(ConstraintRoseTrees, ConstraintRoseTree2);
        }

        public virtual void Set(List<RoseTree> ConstraintRoseTrees, RoseTree ConstraintRoseTree2 = null)
        {
        }

        public static void SetConstraintTree(List<RoseTree> ConstraintRoseTrees, RoseTree ConstraintRoseTree2 = null)
        {
            ExperimentParameters.ConstraintRoseTrees = ConstraintRoseTrees;
            if (ConstraintRoseTree2 == null)
                ExperimentParameters.ConstraintRoseTrees2 = ConstraintRoseTrees;
            else
                ExperimentParameters.ConstraintRoseTree2 = ConstraintRoseTree2;
        }
    }

    class NoConstraintParameter : ConstraintParameter
    {
        public NoConstraintParameter(bool bCalculateSmoothness = true)
        {
            this.bCalculateSmoothness = bCalculateSmoothness;
        }

        public override void Set(List<RoseTree> ConstraintRoseTrees, RoseTree ConstraintRoseTree2 = null)
        {
            ExperimentParameters.ConstraintType = ConstraintType.NoConstraint;
            if (bCalculateSmoothness)
            {
                ExperimentParameters.SmoothCostConstraintTypes
                    = new ConstraintType[] { ConstraintType.TreeDistance, ConstraintType.TreeOrder };
                SetConstraintTree(ConstraintRoseTrees, ConstraintRoseTree2);
            }
            else
            {
                ExperimentParameters.SmoothCostConstraintTypes = null;
                ExperimentParameters.ConstraintRoseTrees = null;
            }
        }

        public override string ToString()
        {
            return "<No>";
        }
    }

    class DistanceConstraintParameter : ConstraintParameter
    {
        double punishweight;
        public DistanceConstraintParameter(double punishweight, bool bCalculateSmoothness = true)
        {
            this.punishweight = punishweight;
            this.bCalculateSmoothness = bCalculateSmoothness;
        }

        public override void Set(List<RoseTree> ConstraintRoseTrees, RoseTree ConstraintRoseTree2 = null)
        {
            ExperimentParameters.ConstraintType = ConstraintType.TreeDistance;
            ExperimentParameters.TreeDistancePunishweight = punishweight;
            if (bCalculateSmoothness)
                if (ConstraintRoseTree2 == null)
                    ExperimentParameters.SmoothCostConstraintTypes = new ConstraintType[] { ConstraintType.TreeOrder };
                else
                    ExperimentParameters.SmoothCostConstraintTypes = new ConstraintType[] { ConstraintType.TreeOrder, ConstraintType.TreeDistance };
            else
                ExperimentParameters.SmoothCostConstraintTypes = null;
            SetConstraintTree(ConstraintRoseTrees, ConstraintRoseTree2);
        }

        public override string ToString()
        {
            return "<Distance:" + punishweight + ">";
        }
    }
    
    class OrderConstraintParameter : ConstraintParameter
    {
        protected double increaseorderpunish;
        protected double loseorderpunish;
        protected double affectleavecntpunish;
        public OrderConstraintParameter(double loseorderpunish, double increaseorderpunish, bool bCalculateSmoothness = true)
        {
            this.loseorderpunish = loseorderpunish;
            this.increaseorderpunish = increaseorderpunish;
            this.affectleavecntpunish = 0;
            this.bCalculateSmoothness = bCalculateSmoothness;
        }

        public OrderConstraintParameter(double loseorderpunish, double increaseorderpunish, double affectleavecntpunish)
        {
            this.loseorderpunish = loseorderpunish;
            this.increaseorderpunish = increaseorderpunish;
            this.affectleavecntpunish = affectleavecntpunish;
        }

        public override void Set(List<RoseTree> ConstraintRoseTrees, RoseTree ConstraintRoseTree2 = null)
        {
            ExperimentParameters.ConstraintType = ConstraintType.TreeOrder;
            ExperimentParameters.IncreaseOrderPunishweight = increaseorderpunish;
            ExperimentParameters.LoseOrderPunishweight = loseorderpunish;
            ExperimentParameters.AffectLeaveCntPunishWeight = affectleavecntpunish;
            if (bCalculateSmoothness)
                if(ConstraintRoseTree2==null)
                    ExperimentParameters.SmoothCostConstraintTypes = new ConstraintType[] { ConstraintType.TreeDistance };
                else
                    ExperimentParameters.SmoothCostConstraintTypes = new ConstraintType[] { ConstraintType.TreeDistance, ConstraintType.TreeOrder };
            else
                ExperimentParameters.SmoothCostConstraintTypes = null;
            SetConstraintTree(ConstraintRoseTrees, ConstraintRoseTree2);
        }

        public override string ToString()
        {
            return "<Order:" + loseorderpunish + "," + increaseorderpunish + ">";
        }
    }

    class LooseOrderConstraintParameter : OrderConstraintParameter
    {
        public LooseOrderConstraintParameter(double loseorderpunish, double increaseorderpunish, bool bCalculateSmoothness = true)
            : base(loseorderpunish, increaseorderpunish, bCalculateSmoothness)
        {
        }

        public LooseOrderConstraintParameter(double loseorderpunish, double increaseorderpunish, double affectleavecntpunish)
            : base(loseorderpunish, increaseorderpunish, affectleavecntpunish)
        {
        }

        public override void Set(List<RoseTree> ConstraintRoseTrees, RoseTree ConstraintRoseTree2 = null)
        {
            ExperimentParameters.ConstraintType = ConstraintType.LooseTreeOrder;
            ExperimentParameters.IncreaseOrderPunishweight = increaseorderpunish;
            ExperimentParameters.LoseOrderPunishweight = loseorderpunish;
            ExperimentParameters.AffectLeaveCntPunishWeight = affectleavecntpunish;
            if (bCalculateSmoothness)
                if (ConstraintRoseTree2 == null)
                    ExperimentParameters.SmoothCostConstraintTypes = new ConstraintType[] { ConstraintType.TreeDistance };
                else
                    ExperimentParameters.SmoothCostConstraintTypes = new ConstraintType[] { ConstraintType.TreeDistance, ConstraintType.TreeOrder };
            else
                ExperimentParameters.SmoothCostConstraintTypes = null;
            SetConstraintTree(ConstraintRoseTrees, ConstraintRoseTree2);
        }

        public override string ToString()
        {
            return "<LooseOrder:" + loseorderpunish + "," + increaseorderpunish + ">";
        }
    }
}
