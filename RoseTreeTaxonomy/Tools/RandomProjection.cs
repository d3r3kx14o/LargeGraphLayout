using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RoseTreeTaxonomy.Constants;
using RoseTreeTaxonomy.DataStructures;

namespace RoseTreeTaxonomy.Tools
{
    public class RandomProjection
    {
        public int datadimension;
        public int projectdimension;
        public int random_projection_algorithm;
        public string random_matrix_path;
        public double[] random_matrix;

        //string ramdommatrixfilename = null;
        public RandomProjection(int datadimension, int projectdimension, int random_projection_algorithm, string random_matrix_path)
        {
            this.datadimension = datadimension;
            this.projectdimension = projectdimension;
            this.random_projection_algorithm = random_projection_algorithm;
            this.random_matrix_path = random_matrix_path;
        }

        public void GenerateRandomMatrix()
        {
            //ramdommatrixfilename = "randommatrix_" + DateTime.Now.Ticks + ".txt";
            //StreamWriter random_matrix_writer = new StreamWriter(this.random_matrix_path + ramdommatrixfilename);
            random_matrix = new double[this.projectdimension * this.datadimension];

            for (int i = 0; i < this.datadimension; i++)
            {
                double norm = 0;
                for (int j = 0; j < this.projectdimension; j++)
                {
                    double data = RandomValue();
                    random_matrix[j * this.datadimension + i] = data;
                    norm += data * data;
                }
                norm = Math.Sqrt(norm);

                if (norm != 0)
                    for (int j = 0; j < this.projectdimension; j++)
                        random_matrix[j * this.datadimension + i] /= norm;
            }


            //for (int i = 0; i < this.projectdimension; i++)
            //{
            //    for (int j = 0; j < this.datadimension - 1; j++)
            //    {
            //        random_matrix_writer.Write(random_matrix[i * this.datadimension + j] + ",");
            //    }
            //    random_matrix_writer.Write(random_matrix[i * this.datadimension + this.datadimension - 1]);
            //    random_matrix_writer.WriteLine();
            //}

            //random_matrix_writer.Flush();
            //random_matrix_writer.Close();
        }

        public double RandomValue()
        {
            if (this.random_projection_algorithm == Constant.GAUSSIAN_RANDOM)
            {
                return RandomGenerator.GetNormal();
            }
            else if (this.random_projection_algorithm == Constant.SQRT_THREE_RANDOM)
            {
                double ran = RandomGenerator.GetUniform();

                if (ran < 1.0 / 6)
                    return Math.Sqrt(3);
                else if (ran > 5.0 / 6)
                    return -Math.Sqrt(3);
                else
                    return 0;
            }
            else
                return double.MinValue;
        }

        public void ReadRandomMatrix()
        {
            //StreamReader random_matrix_reader = new StreamReader(random_matrix_path + ramdommatrixfilename);
            //this.random_matrix = new double[this.projectdimension * this.datadimension];

            //for (int i = 0; i < this.projectdimension; i++)
            //{
            //    string line = random_matrix_reader.ReadLine();
            //    string[] tokens = line.Split(',');

            //    for (int j = 0; j < this.datadimension; j++)
            //        this.random_matrix[i * this.datadimension + j] = double.Parse(tokens[j]);
            //}

            //random_matrix_reader.Close();
        }

        public double[] GenerateProjectData(SparseVectorList data)
        {
            double[] projectdata = new double[this.projectdimension];

            if (data.normvalue == 0)
                data.normvalue = RoseTreeMath.GetNorm(data.valuearray);

            double[] norm_data = RoseTreeMath.Normalize(data.valuearray, data.normvalue);
            double project_norm_value = 0;

            for (int i = 0; i < projectdimension; i++)
            {
                double project_data = 0;
                for (int j = 0; j < norm_data.Length; j++)
                    project_data += norm_data[j] * this.random_matrix[i * this.datadimension + data.keyarray[j]];
                projectdata[i] = project_data;
                project_norm_value += project_data * project_data;
            }
            project_norm_value = Math.Sqrt(project_norm_value);

            if (project_norm_value != 0)
                for (int i = 0; i < projectdimension; i++)
                    projectdata[i] /= project_norm_value;

            return projectdata;
        }
    }
}
