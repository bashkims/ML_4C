using System;
using System.Collections.Generic;
using System.Data;
using MathNet.Numerics.LinearAlgebra;
using Accord.Math;

namespace ML
{    
    public class ML
    {         
        public static double getRSS(Vector<double> input_values, Vector<double> output_values, double intercept, double slope)
        {
            Vector<double> y_hat = input_values.Multiply(slope).Add(intercept);
            return Math.Pow(output_values.Subtract(y_hat).Sum(), 2);
        }
        public static double compute(double intercept, double slope, double value)
        {
            return intercept + (slope * value);
        }
        public static Tuple<double, double> simple_linear_regression(Vector<double> input_feature, Vector<double> output)
        {
            double sum_inputfeature = input_feature.Sum();
            double sum_output = output.Sum();

            int N = output.Count;
            double sum_input_output = output.PointwiseMultiply(input_feature).Sum();

            Vector<double> x2 = Vector<double>.Build.Dense(input_feature.Count);
            x2 = input_feature.PointwiseMultiply(input_feature);

            double numerator = sum_input_output - ((sum_output * sum_inputfeature) / N);
            double denominator = x2.Sum() - (Math.Pow(sum_inputfeature, 2) / N);

            double slope = numerator / denominator;
            double intercept = (sum_output / N - slope * (sum_inputfeature / N));
            return new Tuple<double, double>(intercept, slope);
        }

        public static Tuple<int[], Vector<double>> k_nearest_neighbor(int number_of_neighbors, Matrix<double> data, Vector<double> query_vector)
        {
            Vector<double> distances = calculateDistances(data, query_vector);
            int[] key = Common.argsort(distances.ToArray());
            int[] key_knn = new int[number_of_neighbors];

            for(int i = 0; i< number_of_neighbors; i++)
            {
                key_knn[i] = key[i];
            }
            double[] ll = new double[number_of_neighbors];
            for (int i = 0; i < number_of_neighbors; i++)
            {                
                ll[i] = distances[key[i]];
            }
            return new Tuple<int[], Vector<double>>(key_knn,Vector<double>.Build.DenseOfArray(ll));
        }
        /// <summary>
        ///  Funksioni per gjenerimin e Nearest Neighbor
        /// </summary>
        /// <param name="number_results"> Sa rezultate me u kthy </param>
        /// <param name="data">Tabela me shenime</param>
        /// <param name="recordId">id e cila e ka queryvector qe nderlidhet me idColumn</param>
        /// <param name="idColumn">Kolona e cila permban identifikuesin e shenimit</param>
        /// <param name="weights">Pesha per secilen kolone, duke e perjashtu id kolonen</param>
        /// <returns></returns>
        public static List<int> getKNN(int number_results, DataTable data, int recordId, int idColumn, double[] weights)
        {
            //shtoje njo per me hek veten
            number_results = number_results + 1;

            List<int> result = new List<int>();
            Common comon = new Common();
            double[,] array_data = comon.getMatrix(data);

            Tuple<Vector<double>, int> t_query_vector = comon.getVector(array_data, recordId, idColumn);
            
            int rowIndex = t_query_vector.Item2;
            if (rowIndex == -1) return result ;

            double[,] MainArray = array_data.MemberwiseClone();
            array_data = array_data.RemoveColumn(idColumn);
            array_data = comon.addColumn(1, array_data);
            array_data = NormalizeDataSimple(array_data);
                        
            //shtoja edhe kolonen qe u perdor per normalizim
            double[] temp = Vector<double>.Build.Dense(array_data.Columns(), 1).ToArray();
            weights.CopyTo(temp, 0);

            //multiply me kolone (1)
            array_data.ElementwiseMultiply(temp, 1);


            Matrix<double> matrix_data = Matrix<double>.Build.SparseOfArray(array_data);
            Vector<double> query_vector = matrix_data.Row(rowIndex);

            Tuple<int[], Vector<double>>  knn_result = k_nearest_neighbor(number_results, matrix_data, query_vector);
            if (knn_result.Item1 == null)
                return null;
            Vector<double> knn = knn_result.Item2;

            int[] knn_index = knn_result.Item1;

            //skip 0 self
            for (int i = 1; i < knn.Count; i++)
            {
                result.Add((int)MainArray[knn_index[i], idColumn]);
            }
            return result;
        }


        public static List<int> getKNN<T>(int number_results, List<T> data, int recordId, int idColumn, double[] weights)
        {                        
            //shtoje njo per me hek veten
            number_results = number_results+1;

            List<int> result = new List<int>();
            Common comon = new Common();
            double[,] array_data = comon.getMatrix(data);

            Tuple<Vector<double>, int> t_query_vector = comon.getVector(array_data, recordId, idColumn);

            int rowIndex = t_query_vector.Item2;
            if (rowIndex == -1) return result;

            double[,] MainArray = array_data.MemberwiseClone();
            array_data = array_data.RemoveColumn(idColumn);
            array_data = comon.addColumn(1, array_data);
            array_data = NormalizeDataSimple(array_data);

            //shtoja edhe kolonen qe u perdor per normalizim
            double[] temp = Vector<double>.Build.Dense(array_data.Columns(), 1).ToArray();
            weights.CopyTo(temp, 0);

            //multiply me kolone (1)
            array_data.ElementwiseMultiply(temp, 1);


            Matrix<double> matrix_data = Matrix<double>.Build.SparseOfArray(array_data);
            Vector<double> query_vector = matrix_data.Row(rowIndex);

            Tuple<int[], Vector<double>> knn_result = k_nearest_neighbor(number_results, matrix_data, query_vector);
            if (knn_result.Item1 == null)
                return null;
            Vector<double> knn = knn_result.Item2;

            int[] knn_index = knn_result.Item1;

            //skip 0 self
            for (int i = 0; i < knn.Count; i++)
            {
                if((int)MainArray[knn_index[i], idColumn] != recordId)
                result.Add((int)MainArray[knn_index[i], idColumn]);
                
            }
            return result;
        }
        public static Vector<double> calculateDistances(Matrix<double> src, Vector<double> query_vector)
        {
            Vector<double> dist;
            Vector<double> distances = Vector<double>.Build.Dense(src.RowCount);
            for (int i = 0; i < src.RowCount; i++)
            {
                dist = query_vector - src.Row(i);
                distances[i] = Math.Sqrt(dist.PointwiseMultiply(dist).Sum());
            }
            return distances;
        }

        public static Tuple<int, double> getSmallestDistance(Matrix<double> data, Vector<double> query_vector)
        {
            Vector<double> distancat = calculateDistances(data, query_vector);

            double min = double.PositiveInfinity;
            int index = 0;
            for (int i = 0; i < distancat.Count; i++)
            {
                if (distancat[i] < min)
                {
                    min = distancat[i];
                    index = i;
                }
            }

            return new Tuple<int, double>(index, min);
        }
        public static double euclidianDistance(Vector<double> src, Vector<double> query_vector)
        {
            Vector<double> dist = query_vector - src;
            dist = dist.PointwiseMultiply(dist);
            double distance = dist.Sum();
            distance = Math.Sqrt(distance);
            return distance;            
        }
        public static Tuple<double, int> getSmallestEuclidianDistance(Vector<double> src, Matrix<double> data)
        {
            double min_value = float.PositiveInfinity;
            int cc = 0;

            for (int i = 0; i < 10; i++)
            {
                double distance = euclidianDistance(src, data.Row(i));
                if (distance < min_value)
                {
                    min_value = distance;
                    cc = i;
                }
            }
            return new Tuple<double, int>(min_value, cc);
        }
        /// <summary>
        /// Normalizimi i shenimeve
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tdata"></param>
        /// <returns>Matrix me shenime te normalizuara</returns>
        public static double[,] NormalizeDataSimple(double[,] data)
        {
            double[] norm = data.Euclidean();

            double[,] temp = new double[data.Rows(), data.Columns()];
            for (int j = 0; j < data.Columns(); j++)
            {
                for (int i = 0; i < data.Rows(); i++)
                {
                    double cc = data[i, j];
                    temp[i, j] = cc / norm[j];
                }
            }
            return temp;
        }
    }
}
