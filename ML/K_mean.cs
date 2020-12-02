using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data;
using MathNet.Numerics.LinearAlgebra;
using TF_IDF;
using System.IO;

namespace ML
{
    public class K_mean
    {
        private Dictionary<string, int> countWordsInFile(string content)
        {
            Dictionary<string, int> words = new Dictionary<string, int>(StringComparer.CurrentCultureIgnoreCase);
            var wordPattern = new Regex(@"\w+");

            foreach (Match match in wordPattern.Matches(content))
            {
                int currentCount = 0;
                words.TryGetValue(match.Value, out currentCount);

                currentCount++;
                words[match.Value] = currentCount;
            }
            return words;
        }

        private Vector<double> calculateDistance(Matrix<double> src, Vector<double> query_vector)
        {
            // Vector<double> dist;
            Vector<double> distances = Vector<double>.Build.Dense(src.RowCount);
            for (int i = 0; i < src.RowCount; i++)
            {
                //dist = query_vector - src.Row(i);
                //distances[i] = Math.Sqrt(dist.PointwiseMultiply(dist).Sum());
                distances[i] = ML.euclidianDistance(src.Row(i), query_vector);
            }
            return distances;
        }

        private Vector<double> get_initial_centroids(Matrix<double> dt, int k, int seed)
        {

            int n = dt.RowCount;
            Vector<int> rand_indices = Vector<int>.Build.Dense(k);
            Vector<double> centroids = Vector<double>.Build.Dense(k);
            Random r = new Random();
            if (seed != 0)
            {
                r = new Random(seed);
            }
            for (int i = 0; i < k; i++)
            {
                rand_indices[i] = r.Next(0, n);
                centroids[i] = dt[0, rand_indices[i]];
            }
            return centroids;
        }
        private Matrix<double> get_initial_centroids_Matrix(Matrix<double> dt, int k, int seed)
        {                     
            int n = dt.RowCount;
            Vector<int> rand_indices = Vector<int>.Build.Dense(k);
            Matrix<double> centroids = Matrix<double>.Build.Dense(k, dt.ColumnCount);
            Random r = new Random();
            if (seed != 0)
            {
                r = new Random(seed);
            }
            for (int i = 0; i < k; i++)
            {
                rand_indices[i] = r.Next(0, n);
                Vector<double> temp = dt.Row(rand_indices[i]);
                centroids.InsertRow(i, temp);
            }
            return centroids;
        }

        private double[][] get_initial_centroids_Matrix(double[][] dt1, int k, int seed)
        {
            int n = dt1.Length;
            double[][] centroids = new double[k][];
            int[] rand_indices = new int[k];
            Random r = new Random();
            if (seed != 0)
            {
                r = new Random(seed);
            }

            for (int i = 0; i < k; i++)
            {
                rand_indices[i] = r.Next(0, n);
                double[] temp = dt1[rand_indices[i]];
                centroids[i] = temp;
            }
            return centroids;
        }
        private  int[] assign_clusters(double[][] m, double[] centroids)
        {
            double[] distances_from_centroids = Common.pairwise_distance(m, centroids);
            int[] cluster_assignments = Common.argmin(distances_from_centroids);
            return cluster_assignments;
        }
        private int[] assign_clusters(double[][] m, double[][] centroids)
        {
            int[] cluster_assignments = new int[m.Length];          
            double[][] distances_from_centroids = Common.pairwise_distance(m, centroids);            
            cluster_assignments = Common.argmin(distances_from_centroids);

            return cluster_assignments;
        }
        private double[][] revise_centroids(double[][] data, int k, int[] cluster_assignments)
        {           
            List<double[]> new_centroids = new List<double[]>();

            for (int i = 0; i < k; i++)
            {
                List<double[]> lst = new List<double[]>();
                
                int[] member_data_points = cluster_assignments.Select((b, ind) => b.Equals(i) ? ind : -1).ToArray();
                foreach (int member in member_data_points)
                {
                    if (member != -1)
                        lst.Add(data[member]);
                }

                new_centroids.Add(Accord.Statistics.Tools.Mean(lst.ToArray()));
            }
            return new_centroids.ToArray();
        }
        /// <summary>
        /// This method tries to spread out the initial set of centroids so that they are not too close together. It is known to improve the quality of local optima and lower average runtime
        /// </summary>
        /// <param name="data">dataset</param>
        /// <param name="k">num of centroids</param>
        /// <param name="seed">sed for Random function</param>
        /// <returns>set of centroids</returns>
        private double[][] smart_centroid_initalization(double[][] data, int k, int seed)
        {
            int n = data.Length;
            double[][] centroids = new double[k][];
            int[] rand_indices = new int[k];
            Random r = new Random();
            if (seed != 0)
            {
                r = new Random(seed);
            }
            centroids[0] = data[r.Next(data.Length)];

            double[] distances = Common.pairwise_distance(data, centroids[0]);

            for (int i = 1; i < k; i++)
            {
                double[] chance = Vector<double>.Build.DenseOfArray(distances).Divide(distances.Sum()).ToArray();

                int idx = 0;
                //fixme infinite loop if value 0.
                do
                {
                    idx = Common.chooseWithChance(chance);
                }
                while (centroids.Contains(data[idx]));

                centroids[i] = data[idx];

                double[][] distanca = Common.pairwise_distance_square(data, Common.getArrayRange(centroids, 0, 1, i + 1) );

              
                int[] t = Common.argmin(distanca);
                distances = distanca[t[t[0]]];//.Min();
#if verbose
                Console.WriteLine("distance vector: ");
                for(int y = 0; y< distances.Length; y++)
                {
                    Console.Write(distances[y] + " \n");
                }
                Console.WriteLine("\n=============================================================================\n");                
#endif            
            }
            return centroids;            
        }
        /// <summary>
        /// Function to check if the sum of all squared distances between data points. Smaller the distance, more homogeneous clusters are.
        /// </summary>
        /// <param name="data">dataset</param>
        /// <param name="k">num of clusters</param>
        /// <param name="centroids">centroids</param>
        /// <param name="cluster_assignments">cluster assignments</param>
        /// <returns>Squared distance between data points of the same cluster</returns>
        private double compute_heterogeneity(double[][] data, int k, double[][] centroids, int[] cluster_assignments)
        {
            double heterogeneity = 0.0;
            
            for (int i = 0; i < k; i++)
            {
                List<double[]> lst = new List<double[]>();
                var kkk = cluster_assignments.Select((b, ind) => b.Equals(i) ? ind : -1);
                foreach (int ii in kkk)
                {
                    if (ii != -1)
                        lst.Add(data[ii]);
                }
                double[] distance = Common.pairwise_distance(lst.ToArray(), centroids[i]);
                double[] sq_distance = Vector<double>.Build.DenseOfArray(distance).PointwisePower(2).ToArray();
                heterogeneity += sq_distance.Sum();

            }
            return heterogeneity;
        }
        /// <summary>
        /// after text preparation do the Kmean Process.
        /// </summary>
        /// <param name="data">double array of dataset</param>
        /// <param name="k">num of clusters</param>
        /// <param name="initial_centroids">initial centroids</param>
        /// <param name="maxiter">Maximum iteration</param>
        /// <param name="record_heterogeneity">heterogenity collection</param>
        /// <returns>Centroids and Cluster assignmets</returns>
        public Tuple<double[][], int[]> kmeans(double[][] data, int k, double[][] initial_centroids, int maxiter, ref List<double> record_heterogeneity)
        {
            K_mean km = new K_mean(); 
            double[][] centroids = initial_centroids;
            int[] prev_cluster_assignment = new int[] { -1 };

            for(int i = 0; i< maxiter; i++)
            {

 #if verbose             
                if (i % 100 == 0)
                {
                    Console.WriteLine("iter:\t" + i + " - "+(i/100 + 1)*100 +" \tof " + maxiter);                 
                }
#endif
                int[] cluster_assignment = assign_clusters(data, centroids);
              

                centroids = revise_centroids(data, k, cluster_assignment);
                          
                if((prev_cluster_assignment[0] != -1) && (Common.arraysEqual(cluster_assignment, prev_cluster_assignment)))
                {
                    break;
                }
#if verbose
                if(prev_cluster_assignment[0] != -1)
                {
                    int changed_clusters = Common.getArrayChanges(cluster_assignment,prev_cluster_assignment);
                    Console.WriteLine(changed_clusters + " elements changed their clusters.");                   
                }
#endif
                double score = compute_heterogeneity(data, k, centroids, cluster_assignment);
                record_heterogeneity.Add(score);
                prev_cluster_assignment = cluster_assignment;
            }
#if verbose
            for(int i = 0; i< record_heterogeneity.Count; i++)
            {
                Console.WriteLine("Heterogenity : " + record_heterogeneity[i]);
            }
#endif

            return new Tuple<double[][], int[]>(centroids, prev_cluster_assignment);
        }

        /// <summary>
        /// Generate KMean Clustering from text file and save to file
        /// </summary>
        /// <param name="filePath"> path of CVS file that contains data</param>
        /// <param name="columnname">Name of Column that has text to be considerd</param>
        /// <param name="ct">Column type</param>
        /// <param name="k">Number of clusters</param>
        /// <param name="maxiter">Maximun number of iterations</param>
        /// <param name="outputfile">Path where to save clustered data.</param>
        public void doIt(string filePath, string columnname, List<Type> ct, int k, int maxiter, string outputfile)
        {
            Common c = new Common();
            string[,] dt = c.getStringMatrix(filePath, ',', ct);
            string[] inp = c.getStringVector(dt, columnname);

            List<string> vocab = new List<string>();
            double[][] inputs = TFIDF.Transform(inp, ref vocab);

            inputs = TFIDF.Normalize(inputs);


            double[][] tf_labels = Common.getArrayRange(inputs, 0, 1, dt.Length);


            double[][] tf_centroids = smart_centroid_initalization(inputs, k, 0);


            List<double> heter = new List<double>();
            Tuple<double[][], int[]> result = kmeans(tf_labels, k, tf_centroids, maxiter, ref heter);


            List<List<double[]>> lld = new List<List<double[]>>();

            List<List<string>> lll = new List<List<string>>();
            for (int i = 0; i < k; i++)
            {
                lld.Add(new List<double[]>());
                lll.Add(new List<string>());
            }

            for (int i = 0; i < result.Item2.Length; i++)
            {
                lld[result.Item2[i]].Add(tf_labels[i]);
            }

            string[] vocabul = new string[k];
            for (int i = 0; i < k; i++)
            {
                List<double> ccc = result.Item1[i].ToList();
                ccc.Sort();
                ccc.Reverse();

                for (int j = 0; j < k; j++)
                {
                    vocabul[i] += vocab[Array.IndexOf(result.Item1[i], ccc[j])] + ": " + result.Item1[i][Array.IndexOf(result.Item1[i], ccc[j])] + "  ";
                }

            }

            using (TextWriter writer = File.CreateText(outputfile))
            {
                for (int i = 0; i < result.Item2.Length; i++)
                {
                    writer.WriteLine(inp[i] + "," + vocabul[result.Item2[i]]);
                    lll[result.Item2[i]].Add(inp[i]);
                }
            }

            Console.WriteLine("Finished!!!");
            Console.ReadKey();


        }

        public void test()
        {
            K_mean k = new K_mean();
            k.doIt("data254.csv", "News", new List<Type>() { typeof(string) }, 5, 400, "res123.csv");
        }
    }
}
