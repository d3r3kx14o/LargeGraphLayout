using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoseTreeTaxonomy.Tools;
using RoseTreeTaxonomy.Constants;

using System.IO;
using System.Diagnostics;
namespace RoseTreeTaxonomy.DataStructures
{
    public class SparseVectorList
    {
        public List<int> keylist = new List<int>();
        public List<int> valuelist = new List<int>();

        public int[] keyarray;
        public int[] valuearray;

        public double[] l2normedvaluearray;

        public int valuearray_sum = 0;
        public double cache_valuearray_plus_alpha = 0;
        public double normvalue = 0;

        public int count = 0;

        public string querystring;
        public int documentid = -1;
        public int contentvectorlen = -1;
        public double contentvectornorm = -1;

        public int label;
        public int tree_label;

        public int model_index { get; protected set; }
        
        public SparseVectorList(int model_index)
        {
            this.model_index = model_index;
            if (model_index == Constant.DCM)
                this.Cosine = new CosineFunction(Cosine_DCM);
            else
                this.Cosine = new CosineFunction(Cosine_vMF);
        }

        public delegate double CosineFunction(SparseVectorList featurevector1,
            SparseVectorList featurevector2);
        public CosineFunction Cosine = null;

        public void GetNorm(double[] idf, int model_index)
        {
            if (model_index == Constant.DCM)
                GetNormDCM();
            else if (model_index == Constant.VMF)
                GetNormvMF(idf);
        }

        public void GetNorm(Dictionary<int, double> idf, int model_index)
        {
            if (model_index == Constant.DCM)
                GetNormDCM();
            else if (model_index == Constant.VMF)
                GetNormvMF(idf);
        }

        public void GetNormDCM()
        {
            this.normvalue = RoseTreeMath.GetNorm(this.valuearray);
        }

        public void GetNormvMF(double[] idf)
        {
            double[] idf_valuearray = new double[this.valuearray.Length];
            for (int i = 0; i < this.valuearray.Length; i++)
                idf_valuearray[i] = this.valuearray[i] * idf[this.keyarray[i]];
            this.normvalue = RoseTreeMath.GetNorm(idf_valuearray);
            this.l2normedvaluearray = RoseTreeMath.Normalize(idf_valuearray, this.normvalue);
        }

        public void GetNormvMF(Dictionary<int, double> idf)
        {
            double[] idf_valuearray = new double[this.valuearray.Length];
            for (int i = 0; i < this.valuearray.Length; i++)
                idf_valuearray[i] = this.valuearray[i] * idf[this.keyarray[i]];
            this.normvalue = RoseTreeMath.GetNorm(idf_valuearray);
            this.l2normedvaluearray = RoseTreeMath.Normalize(idf_valuearray, this.normvalue);
            //Xiting
            this.valuearray = null;
        }

        public void GetNormvMF()
        {
            this.normvalue = RoseTreeMath.GetNorm(l2normedvaluearray);
            //this.l2normedvaluearray = RoseTreeMath.Normalize(l2normedvaluearray, this.normvalue);
        }

        public bool ContainsKey(int key)
        {
            bool contains;
            Search(key, out contains);

            return contains;
        }

        public void RemoveAt(int index)
        {
            this.keylist.RemoveAt(index);
            this.valuelist.RemoveAt(index);
        }

        public void Resize(int newsize)
        {
            Array.Resize(ref this.keyarray, newsize);
            Array.Resize(ref this.valuearray, newsize);
            if (this.l2normedvaluearray != null)
                Array.Resize(ref this.l2normedvaluearray, newsize);
        }

        public void InvalidateList()
        {
            this.keylist = null;
            this.valuelist = null;
        }

        public void Invalidate()
        {
            this.keylist = null;
            this.valuelist = null;
            this.keyarray = null;
            this.valuearray = null;
            this.l2normedvaluearray = null;
            this.querystring = null;
        }

        public bool Increase(int key, int incamount)
        {
            bool contains;
            int idx = Search(key, out contains);
            if (!contains)
                return false;

//            if (idx == -1)
//            {
//                Console.WriteLine("error occur");
////                int stop;
//            }
//            if (contains == false)
//            {
//                Console.WriteLine("doesn't contain the key");
////                int stop;
//            }
            valuelist[idx] += incamount;
            return true;
        }

        public void ListToArray()
        {
            this.keyarray = keylist.ToArray();
            this.valuearray = valuelist.ToArray();
        }

        public int  Search(int key, out bool contains)
        {
            if (keylist.Count == 0)
            {
                contains = false;
                return -1;
            }
            if (key > keylist[keylist.Count - 1])
            {
                contains = false;
                return -2;
            }
            if (key < keylist[0])
            {
                contains = false;
                return -3;
            }

            int leftindex = 0;
            int rightindex = keylist.Count - 1;
            int midindex = (leftindex + rightindex) / 2;

            while (leftindex + 1 < rightindex)
            {
                if (keylist[midindex] < key)
                    leftindex = midindex;
                else if (keylist[midindex] > key)
                    rightindex = midindex;
                else
                {
                    leftindex = midindex;
                    rightindex = midindex + 1;
                }
                midindex = (leftindex + rightindex) / 2;
            }
            if (keylist[leftindex] == key)
            {
                contains = true;
                return leftindex;
            }
            if (keylist[rightindex] == key)
            {
                contains = true;
                return rightindex;
            }
            contains = false;
            return rightindex;
        }

        public void SumUpValueArray()
        {
            this.valuearray_sum = 0;

            for (int i = 0; i < this.count; i++)
                this.valuearray_sum += this.valuearray[i];
        }

        public void Add(int key, int value)
        {
            keylist.Add(key);
            valuelist.Add(value);
        }

        public void Insert(int key, int value)
        {
            if (this.keylist.Count == 0)
            {
                this.Add(key, value);
                return;
            }
            bool contains;
            int idx = Search(key, out contains);

            if (idx == -2)
            {
                keylist.Add(key);
                valuelist.Add(value);
                return;
            }
            else if (idx == -3)
            {
                keylist.Insert(0, key);
                valuelist.Insert(0, value);
                return;
            }
            if (contains == true)
            {
                Console.WriteLine("contains the key");
//                int stop;
            }
            keylist.Insert(idx, key);
            valuelist.Insert(idx, value);
        }

        public void Insert(int idx, int key, int value)
        {
            keylist.Insert(idx, key);
            valuelist.Insert(idx, value);
        }

        public SparseVectorList TryAdd(int modeltype, SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            ////Xiting, TryAddValue returns the some pointer each time, with correct values
            //switch (modeltype)
            //{
            //    //case Constant.VMF: return AddL2normvalue(featurevector1, featurevector2, out overlapping_keylist, out length);
            //    case Constant.VMF: return TryAddL2normvalue(featurevector1, featurevector2, out overlapping_keylist, out length);
            //    //case Constant.DCM: return AddValue(featurevector1, featurevector2, out overlapping_keylist, out length);
            //    case Constant.DCM: return TryAddValue(featurevector1, featurevector2, out overlapping_keylist, out length);
            //    default: overlapping_keylist = null; length = int.MinValue; return null;
            //}

            switch (modeltype)
            {
                case Constant.VMF: return TryAddL2normvalue(featurevector1, featurevector2, out overlapping_keylist, out length);
                case Constant.DCM: return TryAddValue(featurevector1, featurevector2, out overlapping_keylist, out length);
                default: overlapping_keylist = null; length = int.MinValue; return null;
            }

            //return TryAddValue(featurevector1, featurevector2, out overlapping_keylist, out length);
        }

        public SparseVectorList Add(bool isPrint, int modeltype, SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            if (modeltype == Constant.VMF && isPrint == false)    
                return AddValueL2normvalue(featurevector1, featurevector2, out overlapping_keylist, out length);
            else
                return AddValue(featurevector1, featurevector2, out overlapping_keylist, out length);
        }

        public double AddNorm(SparseVectorList featurevector, int new_vector_length, int lexiconsize, double mu_0_each_dim, double mu_0_each_dim_sqr)
        {
            double[] valuearray = featurevector.l2normedvaluearray;
            double norm = 0;
            for (int i = 0; i < new_vector_length; i++)
            {
                double plus = valuearray[i] + mu_0_each_dim;
                norm += plus * plus;
            }

            norm += (lexiconsize - new_vector_length) * mu_0_each_dim_sqr;
            norm = Math.Sqrt(norm);

            if ((!(norm < double.MaxValue)) || (!(norm > double.MinValue)))
                throw new Exception("Invalid Value!");

            return norm;
        }

        public SparseVectorList AddValueL2normvalue(SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            overlapping_keylist = new List<int>();
            if (featurevector1.count == 0)
            {
                length = featurevector2.count;
                return featurevector2;
            }
            if (featurevector2.count == 0)
            {
                length = featurevector1.count;
                return featurevector1;
            }
            SparseVectorList newVector = new SparseVectorList(featurevector1.model_index);

            int pt1 = 0;
            int pt2 = 0;
            int pt = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;

            int[] newindex = new int[length1 + length2];
            double[] l2_norm_newvalue = new double[length1 + length2];

            int[] keyarray1 = featurevector1.keyarray;
            int[] keyarray2 = featurevector2.keyarray;
            double[] l2_norm_valuearray1 = featurevector1.l2normedvaluearray;
            double[] l2_norm_valuearray2 = featurevector2.l2normedvaluearray;

            int index1 = keyarray1[pt1];
            int index2 = keyarray2[pt2];
            double l2_norm_value1 = l2_norm_valuearray1[pt1];
            double l2_norm_value2 = l2_norm_valuearray2[pt2];

            while (true)
            {
                while (pt1 < length1 && index1 < index2)
                {
                    newindex[pt] = index1;
                    l2_norm_newvalue[pt] = l2_norm_value1;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keyarray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (index1 == index2)
                {
                    overlapping_keylist.Add(index1);
                    newindex[pt] = index1;
                    l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                    pt2++;
                    if (pt2 < length2)
                    {
                        index2 = keyarray2[pt2];
                        l2_norm_value2 = l2_norm_valuearray2[pt2];
                    }
                }
                else
                {
                    while (pt2 < length2 && index2 < index1)
                    {
                        newindex[pt] = index2;
                        l2_norm_newvalue[pt] = l2_norm_value2;
                        pt++;
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                    if (pt2 == length2)
                    {
                        for (int j = pt1; j < length1; j++)
                        {
                            newindex[pt] = keyarray1[j];
                            l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                            pt++;
                        }
                        break;
                    }
                    if (index2 == index1)
                    {
                        overlapping_keylist.Add(index2);
                        newindex[pt] = index2;
                        l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                        pt++;
                        pt1++;
                        if (pt1 < length1)
                        {
                            index1 = keyarray1[pt1];
                            l2_norm_value1 = l2_norm_valuearray1[pt1];
                        }
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keyarray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (pt2 == length2)
                {
                    for (int j = pt1; j < length1; j++)
                    {
                        newindex[pt] = keyarray1[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                        pt++;
                    }
                    break;
                }
            }
            length = pt;

            newVector.keyarray = newindex;
            newVector.l2normedvaluearray = l2_norm_newvalue;
            newVector.count = pt;
            SetContentVectorLength(newVector, featurevector1, featurevector2);
            return newVector;
        }

        //Xueqing's
        public SparseVectorList AddValueL2normvalue_Prev(SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            overlapping_keylist = new List<int>();
            if (featurevector1.count == 0)
            {
                length = featurevector2.count;
                return featurevector2;
            }
            if (featurevector2.count == 0)
            {
                length = featurevector1.count;
                return featurevector1;
            }
            SparseVectorList newVector = new SparseVectorList(featurevector1.model_index);

            int pt1 = 0;
            int pt2 = 0;
            int pt = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;

            int[] newindex = new int[length1 + length2];
            int[] newvalue = new int[length1 + length2];
            double[] l2_norm_newvalue = new double[length1 + length2];

            int[] keyarray1 = featurevector1.keyarray;
            int[] keyarray2 = featurevector2.keyarray;
            int[] valuearray1 = featurevector1.valuearray;
            int[] valuearray2 = featurevector2.valuearray;
            double[] l2_norm_valuearray1 = featurevector1.l2normedvaluearray;
            double[] l2_norm_valuearray2 = featurevector2.l2normedvaluearray;

            int index1 = keyarray1[pt1];
            int index2 = keyarray2[pt2];
            int value1 = valuearray1[pt1];
            int value2 = valuearray2[pt2];
            double l2_norm_value1 = l2_norm_valuearray1[pt1];
            double l2_norm_value2 = l2_norm_valuearray2[pt2];

            while (true)
            {
                while (pt1 < length1 && index1 < index2)
                {
                    newindex[pt] = index1;
                    newvalue[pt] = value1;
                    l2_norm_newvalue[pt] = l2_norm_value1;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        value1 = valuearray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keyarray2[j];
                        newvalue[pt] = valuearray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (index1 == index2)
                {
                    overlapping_keylist.Add(index1);
                    newindex[pt] = index1;
                    newvalue[pt] = value1 + value2;
                    l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        value1 = valuearray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                    pt2++;
                    if (pt2 < length2)
                    {
                        index2 = keyarray2[pt2];
                        value2 = valuearray2[pt2];
                        l2_norm_value2 = l2_norm_valuearray2[pt2];
                    }
                }
                else
                {
                    while (pt2 < length2 && index2 < index1)
                    {
                        newindex[pt] = index2;
                        newvalue[pt] = value2;
                        l2_norm_newvalue[pt] = l2_norm_value2;
                        pt++;
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            value2 = valuearray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                    if (pt2 == length2)
                    {
                        for (int j = pt1; j < length1; j++)
                        {
                            newindex[pt] = keyarray1[j];
                            newvalue[pt] = valuearray1[j];
                            l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                            pt++;
                        }
                        break;
                    }
                    if (index2 == index1)
                    {
                        overlapping_keylist.Add(index2);
                        newindex[pt] = index2;
                        newvalue[pt] = value1 + value2;
                        l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                        pt++;
                        pt1++;
                        if (pt1 < length1)
                        {
                            index1 = keyarray1[pt1];
                            value1 = valuearray1[pt1];
                            l2_norm_value1 = l2_norm_valuearray1[pt1];
                        }
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            value2 = valuearray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keyarray2[j];
                        newvalue[pt] = valuearray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (pt2 == length2)
                {
                    for (int j = pt1; j < length1; j++)
                    {
                        newindex[pt] = keyarray1[j];
                        newvalue[pt] = valuearray1[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                        pt++;
                    }
                    break;
                }
            }
            length = pt;

            newVector.keyarray = newindex;
            newVector.valuearray = newvalue;
            newVector.l2normedvaluearray = l2_norm_newvalue;
            newVector.count = pt;

            return newVector;
        }

        //static StreamWriter ofile = new StreamWriter(@"D:\Project\EvolutionaryRoseTreeData\outputpath\AddVaule.dat");
        //static int AddCount = 1;
        public SparseVectorList AddValue(SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            //if (AddCount++ == 399)
            //    Console.WriteLine();

            overlapping_keylist = new List<int>();
            if (featurevector1.count == 0)
            {
                length = featurevector2.count;
                return featurevector2;
            }
            if (featurevector2.count == 0)
            {
                length = featurevector1.count;
                return featurevector1;
            }
            SparseVectorList newVector = new SparseVectorList(featurevector1.model_index);

            int pt1 = 0;
            int pt2 = 0;
            int pt = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;

            int[] newindex = new int[length1 + length2];
            int[] newvalue = new int[length1 + length2];

            int[] keylist1 = featurevector1.keyarray;
            int[] keylist2 = featurevector2.keyarray;
            int[] valuelist1 = featurevector1.valuearray;
            int[] valuelist2 = featurevector2.valuearray;

            int index1 = keylist1[pt1];
            int index2 = keylist2[pt2];
            int value1 = valuelist1[pt1];
            int value2 = valuelist2[pt2];

            while (true)
            {
                while (pt1 < length1 && index1 < index2)
                {
                    newindex[pt] = index1;
                    newvalue[pt] = value1;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keylist1[pt1];
                        value1 = valuelist1[pt1];
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keylist2[j];
                        newvalue[pt] = valuelist2[j];
                        pt++;
                    }
                    break;
                }
                if (index1 == index2)
                {
                    overlapping_keylist.Add(index1);
                    newindex[pt] = index1;
                    newvalue[pt] = value1 + value2;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keylist1[pt1];
                        value1 = valuelist1[pt1];
                    }
                    pt2++;
                    if (pt2 < length2)
                    {
                        index2 = keylist2[pt2];
                        value2 = valuelist2[pt2];
                    }
                }
                else
                {
                    while (pt2 < length2 && index2 < index1)
                    {
                        newindex[pt] = index2;
                        newvalue[pt] = value2;
                        pt++;
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keylist2[pt2];
                            value2 = valuelist2[pt2];
                        }
                    }
                    if (pt2 == length2)
                    {
                        for (int j = pt1; j < length1; j++)
                        {
                            newindex[pt] = keylist1[j];
                            newvalue[pt] = valuelist1[j];
                            pt++;
                        }
                        break;
                    }
                    if (index2 == index1)
                    {
                        overlapping_keylist.Add(index2);
                        newindex[pt] = index2;
                        newvalue[pt] = value1 + value2;
                        pt++;
                        pt1++;
                        if (pt1 < length1)
                        {
                            index1 = keylist1[pt1];
                            value1 = valuelist1[pt1];
                        }
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keylist2[pt2];
                            value2 = valuelist2[pt2];
                        }
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keylist2[j];
                        newvalue[pt] = valuelist2[j];
                        pt++;
                    }
                    break;
                }
                if (pt2 == length2)
                {
                    for (int j = pt1; j < length1; j++)
                    {
                        newindex[pt] = keylist1[j];
                        newvalue[pt] = valuelist1[j];
                        pt++;
                    }
                    break;
                }
            }
            length = pt;

            newVector.keyarray = newindex;
            newVector.valuearray = newvalue;
            newVector.count = pt;
            SetContentVectorLength(newVector, featurevector1, featurevector2);

            //ofile.Write("[pt] {0}\n", pt); ofile.Flush();
            return newVector;
        }

        //Xiting, When GetLogF(), we do not need to generate a new node, which saves large space
        static SparseVectorList newVectorDCM = new SparseVectorList(Constant.DCM);
        static int[] newindex = new int[1];
        static int[] newvalue = new int[1];
        public static SparseVectorList TryAddValue(SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            overlapping_keylist = null;
            if (featurevector1.count == 0)
            {
                length = featurevector2.count;
                return featurevector2;
            }
            if (featurevector2.count == 0)
            {
                length = featurevector1.count;
                return featurevector1;
            }

            int pt1 = 0;
            int pt2 = 0;
            int pt = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;

            if (length1 + length2 > newindex.Length)
            {
                newindex = new int[length1 + length2];
                newvalue = new int[length1 + length2];
            }
            //else
            //{
            //    for (int i = length1 + length2; i < newindex.Length; i++)
            //    {
            //        newindex[i] = -1;
            //        newvalue[i] = 0;
            //    }
            //}

            int[] keylist1 = featurevector1.keyarray;
            int[] keylist2 = featurevector2.keyarray;
            int[] valuelist1 = featurevector1.valuearray;
            int[] valuelist2 = featurevector2.valuearray;

            int index1 = keylist1[pt1];
            int index2 = keylist2[pt2];
            int value1 = valuelist1[pt1];
            int value2 = valuelist2[pt2];

            while (true)
            {
                while (pt1 < length1 && index1 < index2)
                {
                    newindex[pt] = index1;
                    newvalue[pt] = value1;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keylist1[pt1];
                        value1 = valuelist1[pt1];
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keylist2[j];
                        newvalue[pt] = valuelist2[j];
                        pt++;
                    }
                    break;
                }
                if (index1 == index2)
                {
                    //overlapping_keylist.Add(index1);
                    newindex[pt] = index1;
                    newvalue[pt] = value1 + value2;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keylist1[pt1];
                        value1 = valuelist1[pt1];
                    }
                    pt2++;
                    if (pt2 < length2)
                    {
                        index2 = keylist2[pt2];
                        value2 = valuelist2[pt2];
                    }
                }
                else
                {
                    while (pt2 < length2 && index2 < index1)
                    {
                        newindex[pt] = index2;
                        newvalue[pt] = value2;
                        pt++;
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keylist2[pt2];
                            value2 = valuelist2[pt2];
                        }
                    }
                    if (pt2 == length2)
                    {
                        for (int j = pt1; j < length1; j++)
                        {
                            newindex[pt] = keylist1[j];
                            newvalue[pt] = valuelist1[j];
                            pt++;
                        }
                        break;
                    }
                    if (index2 == index1)
                    {
                        //overlapping_keylist.Add(index2);
                        newindex[pt] = index2;
                        newvalue[pt] = value1 + value2;
                        pt++;
                        pt1++;
                        if (pt1 < length1)
                        {
                            index1 = keylist1[pt1];
                            value1 = valuelist1[pt1];
                        }
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keylist2[pt2];
                            value2 = valuelist2[pt2];
                        }
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keylist2[j];
                        newvalue[pt] = valuelist2[j];
                        pt++;
                    }
                    break;
                }
                if (pt2 == length2)
                {
                    for (int j = pt1; j < length1; j++)
                    {
                        newindex[pt] = keylist1[j];
                        newvalue[pt] = valuelist1[j];
                        pt++;
                    }
                    break;
                }
            }
            length = pt;

            newVectorDCM.keyarray = newindex;
            newVectorDCM.valuearray = newvalue;
            newVectorDCM.count = pt;

            return newVectorDCM;
        }

        static SparseVectorList newVectorvMF = new SparseVectorList(Constant.VMF);
        static int[] newindexvMF = new int[1];
        static double[] l2_norm_newvalue = new double[1];
        public SparseVectorList TryAddL2normvalue(SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            overlapping_keylist = new List<int>();
            if (featurevector1.count == 0)
            {
                length = featurevector2.count;
                return featurevector2;
            }
            if (featurevector2.count == 0)
            {
                length = featurevector1.count;
                return featurevector1;
            }

            int pt1 = 0;
            int pt2 = 0;
            int pt = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;

            if (length1 + length2 > newindexvMF.Length)
            {
                newindexvMF = new int[length1 + length2];
                l2_norm_newvalue = new double[length1 + length2];
            }

            int[] keyarray1 = featurevector1.keyarray;
            int[] keyarray2 = featurevector2.keyarray;
            double[] l2_norm_valuearray1 = featurevector1.l2normedvaluearray;
            double[] l2_norm_valuearray2 = featurevector2.l2normedvaluearray;

            int index1 = keyarray1[pt1];
            int index2 = keyarray2[pt2];
            double l2_norm_value1 = l2_norm_valuearray1[pt1];
            double l2_norm_value2 = l2_norm_valuearray2[pt2];

            while (true)
            {
                while (pt1 < length1 && index1 < index2)
                {
                    newindexvMF[pt] = index1;
                    l2_norm_newvalue[pt] = l2_norm_value1;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindexvMF[pt] = keyarray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (index1 == index2)
                {
                    overlapping_keylist.Add(index1);
                    newindexvMF[pt] = index1;
                    l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                    pt2++;
                    if (pt2 < length2)
                    {
                        index2 = keyarray2[pt2];
                        l2_norm_value2 = l2_norm_valuearray2[pt2];
                    }
                }
                else
                {
                    while (pt2 < length2 && index2 < index1)
                    {
                        newindexvMF[pt] = index2;
                        l2_norm_newvalue[pt] = l2_norm_value2;
                        pt++;
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                    if (pt2 == length2)
                    {
                        for (int j = pt1; j < length1; j++)
                        {
                            newindexvMF[pt] = keyarray1[j];
                            l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                            pt++;
                        }
                        break;
                    }
                    if (index2 == index1)
                    {
                        overlapping_keylist.Add(index2);
                        newindexvMF[pt] = index2;
                        l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                        pt++;
                        pt1++;
                        if (pt1 < length1)
                        {
                            index1 = keyarray1[pt1];
                            l2_norm_value1 = l2_norm_valuearray1[pt1];
                        }
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindexvMF[pt] = keyarray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (pt2 == length2)
                {
                    for (int j = pt1; j < length1; j++)
                    {
                        newindexvMF[pt] = keyarray1[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                        pt++;
                    }
                    break;
                }
            }
            length = pt;

            newVectorvMF.keyarray = newindexvMF;
            newVectorvMF.l2normedvaluearray = l2_norm_newvalue;
            newVectorvMF.count = pt;

            return newVectorvMF;
        }

        public SparseVectorList AddL2normvalue(SparseVectorList featurevector1, SparseVectorList featurevector2, out List<int> overlapping_keylist, out int length)
        {
            overlapping_keylist = new List<int>();
            if (featurevector1.count == 0)
            {
                length = featurevector2.count;
                return featurevector2;
            }
            if (featurevector2.count == 0)
            {
                length = featurevector1.count;
                return featurevector1;
            }
            SparseVectorList newVector = new SparseVectorList(featurevector1.model_index);

            int pt1 = 0;
            int pt2 = 0;
            int pt = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;

            int[] newindex = new int[length1 + length2];
            double[] l2_norm_newvalue = new double[length1 + length2];

            int[] keyarray1 = featurevector1.keyarray;
            int[] keyarray2 = featurevector2.keyarray;
            double[] l2_norm_valuearray1 = featurevector1.l2normedvaluearray;
            double[] l2_norm_valuearray2 = featurevector2.l2normedvaluearray;

            int index1 = keyarray1[pt1];
            int index2 = keyarray2[pt2];
            double l2_norm_value1 = l2_norm_valuearray1[pt1];
            double l2_norm_value2 = l2_norm_valuearray2[pt2];

            while (true)
            {
                while (pt1 < length1 && index1 < index2)
                {
                    newindex[pt] = index1;
                    l2_norm_newvalue[pt] = l2_norm_value1;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keyarray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (index1 == index2)
                {
                    overlapping_keylist.Add(index1);
                    newindex[pt] = index1;
                    l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                    pt++;
                    pt1++;
                    if (pt1 < length1)
                    {
                        index1 = keyarray1[pt1];
                        l2_norm_value1 = l2_norm_valuearray1[pt1];
                    }
                    pt2++;
                    if (pt2 < length2)
                    {
                        index2 = keyarray2[pt2];
                        l2_norm_value2 = l2_norm_valuearray2[pt2];
                    }
                }
                else
                {
                    while (pt2 < length2 && index2 < index1)
                    {
                        newindex[pt] = index2;
                        l2_norm_newvalue[pt] = l2_norm_value2;
                        pt++;
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                    if (pt2 == length2)
                    {
                        for (int j = pt1; j < length1; j++)
                        {
                            newindex[pt] = keyarray1[j];
                            l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                            pt++;
                        }
                        break;
                    }
                    if (index2 == index1)
                    {
                        overlapping_keylist.Add(index2);
                        newindex[pt] = index2;
                        l2_norm_newvalue[pt] = l2_norm_value1 + l2_norm_value2;
                        pt++;
                        pt1++;
                        if (pt1 < length1)
                        {
                            index1 = keyarray1[pt1];
                            l2_norm_value1 = l2_norm_valuearray1[pt1];
                        }
                        pt2++;
                        if (pt2 < length2)
                        {
                            index2 = keyarray2[pt2];
                            l2_norm_value2 = l2_norm_valuearray2[pt2];
                        }
                    }
                }
                if (pt1 == length1)
                {
                    for (int j = pt2; j < length2; j++)
                    {
                        newindex[pt] = keyarray2[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray2[j];
                        pt++;
                    }
                    break;
                }
                if (pt2 == length2)
                {
                    for (int j = pt1; j < length1; j++)
                    {
                        newindex[pt] = keyarray1[j];
                        l2_norm_newvalue[pt] = l2_norm_valuearray1[j];
                        pt++;
                    }
                    break;
                }
            }
            length = pt;

            newVector.keyarray = newindex;
            newVector.l2normedvaluearray = l2_norm_newvalue;
            newVector.count = pt;
            SetContentVectorLength(newVector, featurevector1, featurevector2);

            return newVector;
        }

        public static double DotProduct(SparseVectorList featurevector1, SparseVectorList featurevector2)
        {
            int pt1 = 0;
            int pt2 = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;
            double ret = 0;
            int[] keys1 = featurevector1.keyarray;
            int[] values1 = featurevector1.valuearray;
            int[] keys2 = featurevector2.keyarray;
            int[] values2 = featurevector2.valuearray;

            while (true)
            {
                while (pt1 < length1 && keys1[pt1] < keys2[pt2]) pt1++;
                if (pt1 == length1) break;
                if (keys1[pt1] == keys2[pt2])
                {
                    ret += (double)values1[pt1] * values2[pt2];
                    pt1++;
                    pt2++;
                }
                else
                {
                    while (pt2 < length2 && keys2[pt2] < keys1[pt1]) pt2++;
                    if (pt2 == length2) break;
                    if (keys2[pt2] == keys1[pt1])
                    {
                        ret += (double)values1[pt1] * values2[pt2];
                        pt1++;
                        pt2++;
                    }
                }
                if (pt1 == length1 || pt2 == length2) break;
            }
            return ret;
        }

        public static double DotProduct_vMF(SparseVectorList featurevector1, SparseVectorList featurevector2)
        {
            int pt1 = 0;
            int pt2 = 0;
            int length1 = featurevector1.count;
            int length2 = featurevector2.count;
            double ret = 0;
            int[] keys1 = featurevector1.keyarray;
            double[] values1 = featurevector1.l2normedvaluearray;
            int[] keys2 = featurevector2.keyarray;
            double[] values2 = featurevector2.l2normedvaluearray;

            while (true)
            {
                while (pt1 < length1 && keys1[pt1] < keys2[pt2]) pt1++;
                if (pt1 == length1) break;
                if (keys1[pt1] == keys2[pt2])
                {
                    ret += (double)values1[pt1] * values2[pt2];
                    pt1++;
                    pt2++;
                }
                else
                {
                    while (pt2 < length2 && keys2[pt2] < keys1[pt1]) pt2++;
                    if (pt2 == length2) break;
                    if (keys2[pt2] == keys1[pt1])
                    {
                        ret += (double)values1[pt1] * values2[pt2];
                        pt1++;
                        pt2++;
                    }
                }
                if (pt1 == length1 || pt2 == length2) break;
            }

            return ret;
        }

        //public static long CosineCalculateTimeSum = 0;
        //public static long FeatureVectorCount = 0;
        //public static long FeatureVectorLength = 0;
        //public static int CosineCalculateTimeCnt = 0;
        public static double Cosine_DCM(SparseVectorList featurevector1, SparseVectorList featurevector2)
        {
            double cosine;
            if (featurevector1.count > featurevector2.count)
            {
                cosine = Cosine_DCM(featurevector2, featurevector1);
            }
            else
            {
                long t = DateTime.Now.Ticks;
                cosine = DotProduct(featurevector1, featurevector2) / featurevector1.normvalue / featurevector2.normvalue;
                //CosineCalculateTimeSum += (DateTime.Now.Ticks - t);
                //FeatureVectorCount += featurevector1.count + featurevector2.count;
                //FeatureVectorLength += featurevector1.keyarray.Length + featurevector2.keyarray.Length;
                //CosineCalculateTimeCnt++;
            }
            return cosine;
        }

        public static double Cosine_vMF(SparseVectorList featurevector1, SparseVectorList featurevector2)
        {
            double cosine;
            if (featurevector1.count > featurevector2.count)
            {
                cosine = Cosine_vMF(featurevector2, featurevector1);
            }
            else
            {
                long t = DateTime.Now.Ticks;
                cosine = DotProduct_vMF(featurevector1, featurevector2) / featurevector1.normvalue / featurevector2.normvalue;
                //CosineCalculateTimeSum += (DateTime.Now.Ticks - t);
                //FeatureVectorCount += featurevector1.count + featurevector2.count;
                //FeatureVectorLength += featurevector1.keyarray.Length + featurevector2.keyarray.Length;
                //CosineCalculateTimeCnt++;
            }
            return cosine;
        }

        public static void SetContentVectorLength(SparseVectorList v, SparseVectorList v1, SparseVectorList v2)
        {
            if ( v1.contentvectorlen < 0 || v2.contentvectorlen < 0 || v.contentvectorlen >=0)
                return;
            if (v1.contentvectorlen == v1.count && v2.contentvectorlen == v2.count)
            {
                v.contentvectorlen = v.count;
                return;
            }
            if (v1.contentvectorlen == 0 && v2.contentvectorlen == 0)
            {
                v.contentvectorlen = 0;
                return;
            }
            var tolkey = Math.Max(v1.keyarray[v1.contentvectorlen - 1], v2.keyarray[v2.contentvectorlen - 1]);
            int len = v.count;
            for (int i = Math.Max(v1.contentvectorlen, v2.contentvectorlen); i < v.keyarray.Length; i++)
            {
                if (v.keyarray[i] > tolkey)
                {
                    len = i;
                    break;
                }
            }
            v.contentvectorlen = len;
        }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < count; i++)
            {
                str += string.Format("({0},{1})", keyarray[i], valuearray[i]);
            }
            return str;
        }
    }
}
