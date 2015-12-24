using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConstrainedRoseTreeLibrary.Data;
using EvolutionaryRoseTree.Constraints;
using EvolutionaryRoseTree.DataStructures;
using RoseTreeTaxonomy.Algorithms;
using RoseTreeTaxonomy.ReadData;

namespace ConstrainedRoseTreeLibrary.BuildTree
{
    public class BuildRoseTree
    {
        /// <summary>
        /// Build a simple BRT (no constraint) using feature vectors Dictionary<string,int> as input
        /// </summary>
        /// <returns></returns>
        public static RoseTree GetRoseTree(List<RawDocument> rawDocuments, RoseTreeParameters rtPara)
        {
            Dictionary<string, int> vocabDict = new Dictionary<string, int>();
            int wordIndex = 0;
            foreach(var rawDoc in rawDocuments)
            {
                foreach(var word in rawDoc.DocumentContentVector.Keys)
                {
                    if(!vocabDict.ContainsKey(word))
                    {
                        vocabDict.Add(word, wordIndex++);
                    }
                }
            }
                    
            var lexicon = new GlobalLexicon(vocabDict);

            RoseTreeData rtData = new RoseTreeData(rawDocuments, lexicon);

            return GetRoseTree(rtData, rtPara, null);
        }

        public static RoseTree GetRoseTree(RoseTreeData rtData, RoseTreeParameters rtPara, List<RoseTree> constraintRoseTrees)
        {
            InitializeSettings();

            var rosetree = new ConstrainedRoseTree(
                          -1,                                       //CONCEPTUALIZE,BING_NEWS,TWENTY_NEWS_GROUP,HAOS_DATA_SET
                          rtPara.algorithm_index,                   //BRT,KNN_BRT,SPILLTREE_BRT
                          rtPara.experiment_index,                  //0
                          rtPara.random_projection_algorithm_index, //GAUSSIAN_RANDOM,SQRT_THREE_RANDOM
                          rtData.modelIndex,                       //DCM,VMF,BERNOULLI
                          rtPara.projectdimension,                  //projectdimensions[1]:50
                          rtPara.k,                                 //k nearest neighbour
                          rtData.lfv,                               //load feature vector
                          rtPara.alpha, rtPara.gamma, rtPara.tau, rtPara.kappa, rtPara.R_0,      //parameters, see top of this file
                          null, rtPara.sizepunishminratio, rtPara.sizepunishmaxratio);

            var constraint = GetParameterConstraint(rtPara, rtData.lfv, constraintRoseTrees); 
            
            int depth;
            double loglikelihood;
            rosetree.Run(constraint, rtPara.interval, out depth, out loglikelihood);
            
            if(rtPara.badjuststructure)
                (rosetree as ConstrainedRoseTree).AdjustTreeStructureProject();

            return rosetree;
        }

        private static void InitializeSettings()
        {
            Constraint.DataProjectionType = DataProjectionType.MaxSimilarityDocumentContentVector;
        }

        #region set up constraints
        private static Constraint GetParameterConstraint(RoseTreeParameters rtPara,
    LoadFeatureVectors lfv, List<RoseTree> constraintRoseTrees)
        {
            if (rtPara.constrainttype == ConstraintType.NoConstraint)
                return null;
            DataProjection.AbandonCosineThreshold = rtPara.abandonthreshold;
            List<Constraint> constraints = new List<Constraint>();
            foreach (RoseTree constraintRoseTree in constraintRoseTrees)
            {
                Constraint constraint = GetParameterConstraint(rtPara, lfv, constraintRoseTree);
                constraints.Add(constraint);
            }
            double[] weights = new double[constraintRoseTrees.Count];
            for (int i = 0; i < weights.Length; i++)
                weights[i] = 1;
            Constraint multiconstraint = new MultipleConstraints(constraints,
                GetOriginalConstraintType(rtPara.constrainttype), 
                weights, false);
            return multiconstraint;
        }

        private static Constraint GetParameterConstraint(RoseTreeParameters rtPara,
            LoadFeatureVectors lfv, RoseTree constraintRoseTree)
        {
            Constraint constraint = null;
            switch (rtPara.constrainttype)
            {
                case ConstraintType.TreeOrder:
                    constraint = new TreeOrderConstraint(
                        constraintRoseTree,
                        lfv,
                        rtPara.mergeparameter,
                        rtPara.splitparameter);
                    break;
                case ConstraintType.LooseTreeOrder:
                    constraint = new LooseTreeOrderConstraint(
                       constraintRoseTree,
                       lfv,
                       rtPara.mergeparameter,
                       rtPara.splitparameter);
                    break;
            }

            return constraint;
        }

        private static EvolutionaryRoseTree.Constraints.ConstraintType GetOriginalConstraintType(ConstraintType constraintType)
        {
            switch (constraintType)
            {
                case ConstraintType.LooseTreeOrder:
                    return EvolutionaryRoseTree.Constraints.ConstraintType.LooseTreeOrder;
                case ConstraintType.TreeOrder:
                    return EvolutionaryRoseTree.Constraints.ConstraintType.TreeOrder;
                case ConstraintType.NoConstraint:
                    return EvolutionaryRoseTree.Constraints.ConstraintType.NoConstraint;
            }
            throw new Exception("Unexpected constraint input!");
        }
        #endregion
    }
}
