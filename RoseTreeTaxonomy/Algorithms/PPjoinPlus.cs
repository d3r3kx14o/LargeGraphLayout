using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.DataStructures;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.Constants;

namespace RoseTreeTaxonomy.Algorithms
{
    public class PPjoinPlus
    {
        public double t;
        public double coefficient;
        public LinkedInvertedIndex index = new LinkedInvertedIndex();
        public int[] prefixLengths;
        public int[] alpha;
        public int ppjoin_similarity_type;

        public PPjoinPlus(int ppjoin_similarity_type, double t)
        {
            this.ppjoin_similarity_type = ppjoin_similarity_type;
            this.t = t;
            this.coefficient = t / (1 + t);
        }

        public void DecreaseT(double multiplier)
        {
            this.t *= multiplier;
        }

        //public List<int> innerExtractPairs(int xDataSetID, RoseTreeNode[] nodearray)
        //{
        //    PPjoinPlusItems x = nodearray[xDataSetID].ppjoinplus_token.item;
        //    int[] A = new int[xDataSetID];
        //    int xSize = x.size();
        //    if (xSize == 0)
        //        return null;
        //    int maxPrefixLength = nodearray[xDataSetID].ppjoinplus_token.probingprefixLength; // p : max-prefix-length
        //    int midPrefixLength = nodearray[xDataSetID].ppjoinplus_token.indexPrefixLength;

        //    int maxoverlap = RoseTreeMath.ArrayMaxOverlap(x.tokens, nodearray[0].ppjoinplus_token.item.tokens, -1, -1);
        //    int LB = OverlapLowerBound(x.size(), nodearray[0].ppjoinplus_token.item.size());

        //    for (int xPos = 0; xPos < maxPrefixLength; xPos++)
        //    {
        //        int w = x.get(xPos);
        //        LinkedPositions positions = index.get(w);

        //        if (positions != null)
        //        {
        //            LinkedPositions.linkedNode node = positions.getRootlinkedNode();
        //            while (true)
        //            {
        //                LinkedPositions.linkedNode next = node.next;
        //                if (next == null)
        //                    break;

        //                int yID = next.node.packageData.treerootIndex;
        //                if (yID > xDataSetID)
        //                {
        //                    node = next;
        //                    continue;
        //                }
        //                if (A[yID] == int.MinValue)
        //                {
        //                    node = next;
        //                    continue;
        //                }

        //                PPjoinPlusItems y = getTokenList(yID).item;
        //                int yPos = next.position;
        //                int ySize = y.size();

        //                RoseTreeNode sss = currenttreesRef.nodes[xDataSetID];

        //                if (ySize < getTokenList(xDataSetID).overlapLowerbound)
        //                {
        //                    node = next;
        //                    continue;
        //                }

        //                alpha[yID] = positionalFilteringThreshold(xSize, ySize);
        //                A[yID]++;
        //                int ubound = Math.Min(xSize - xPos, ySize - yPos) - 1;
        //                if (A[yID] + ubound < alpha[yID])
        //                    A[yID] = int.MinValue;
        //                else
        //                {
        //                    if (A[yID] == 1)
        //                    {
        //                        int hmax = hammingdistanceThreshold(xSize, ySize, xPos, yPos);
        //                        int h = hammingDistanceLowerBound(x.tokens, y.tokens, xSize, ySize, xPos, yPos);
        //                        if (hmax < h)
        //                            A[yID] = int.MinValue;
        //                    }
        //                }
        //                node = next;
        //            }
        //        }
        //    }
        //    prefixLengths[xDataSetID] = midPrefixLength;
        //    List<int> ret = veryfy(x, maxPrefixLength, A, alpha);
        //    for (int xPos = 0; xPos < midPrefixLength; xPos++)
        //    {
        //        int w = x.get(xPos);
        //        index.put(w, currenttreesRef.nodes[xDataSetID], xPos);
        //    }
        //    return ret;
        //}

        public int OverlapLowerBound(int xSize, int ySize)
        {
            switch (this.ppjoin_similarity_type)
            {
                case (Constant.JACCARD): return (int)Math.Ceiling(coefficient * (ySize + xSize));
                case (Constant.COSINE): return (int)Math.Ceiling(t * Math.Sqrt(xSize) * Math.Sqrt(ySize));
                default: return -1;
            }
        }
    }
}
