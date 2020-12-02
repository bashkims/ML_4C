using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using Accord.Math;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace ML
{

    public static class Extensions
    {

        private static Random random = new Random();
		/// https://social.msdn.microsoft.com/Forums/en-US/b832b9ff-5e1a-490f-bcf6-3e72070b5879/shuffle-datatable-rows?forum=csharplanguage
		/// Shuffle dhe values of collection
        /// </summary>
        /// <typeparam name="T"> type of collection</typeparam>
        /// <param name="items">items of collection</param>
        /// <returns>Returns shuffled collection</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            List<T> randomly = new List<T>(items);

            while (randomly.Count > 0)
            {
                Int32 index = random.Next(randomly.Count);
                yield return randomly[index];

                randomly.RemoveAt(index);
            }
        }

        public static IEnumerable<string> SplitCSV(this string input)
        {
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);

            foreach (Match match in csvSplit.Matches(input))
            {
                yield return match.Value.TrimStart(',');
            }
        }
    }
    public class Common
    {
        public static Dictionary<string, int> col_name_pair;
       
        
        public Common()
        {
            col_name_pair = new Dictionary<string, int>();
        }

       /// <summary>
       /// Function to split Datatable in to two DataTables based on percentage
       /// </summary>
       /// <param name="originalTable"> DataTable to be splited</param>
       /// <param name="percentage">percentage for the first Item of returning tuple</param>
       /// <returns>two datatables based on spliting values</returns>
        public static Tuple<DataTable, DataTable> SplitTable(DataTable originalTable, double percentage)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            DataTable dt1 = originalTable.Clone();
            DataTable dt2 = originalTable.Clone();                       
            int rownumber = (int)(originalTable.Rows.Count * percentage);            
            originalTable.AsEnumerable().Take(rownumber).CopyToDataTable(dt1, LoadOption.OverwriteChanges);
            originalTable.AsEnumerable().Skip(rownumber).CopyToDataTable(dt2, LoadOption.OverwriteChanges);
            watch.Stop();
            Console.WriteLine("SplitTable execution time: \t" + watch.ElapsedMilliseconds);
            return new Tuple<DataTable, DataTable>(dt1, dt2);
        }
        /// <summary>
        /// Balance data in Datatable by getting smallest value of groupby
        /// </summary>
        /// <param name="dt"> DataTable</param>
        /// <param name="column"> column to be used for grouping</param>
        /// <returns></returns>
        public DataTable balanceData_simple(DataTable dt, int column)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            DataTable _dt = dt.Clone();
            IEnumerable<IGrouping<object, DataRow>> _group;
            _group = dt.AsEnumerable().GroupBy(test => test[column]);
            int smallest_groupby_count = int.MaxValue;

            foreach (var groupingByClassA in _group)
            {
                if (dt.AsEnumerable().Count(t => t.Field<int>(column) == (int)groupingByClassA.Key) < smallest_groupby_count)
                    smallest_groupby_count = dt.AsEnumerable().Count(t => t.Field<int>(column) == (int)groupingByClassA.Key);
            }

            foreach(var groupingByClassA in _group)
            {
                dt.AsEnumerable().Where(t => t.Field<int>(column) == (int)groupingByClassA.Key).Take(smallest_groupby_count).CopyToDataTable(_dt, LoadOption.PreserveChanges);               
            }
            watch.Stop();
            Console.WriteLine("Balance Data execution time: \t" + watch.ElapsedMilliseconds);  
            return _dt;
        }       
        /// <summary>
        /// Binarize the DataTable, based on groupby of columns
        /// </summary>
        /// <param name="dt"> DataTable </param>
        /// <returns>DataTable</returns>
        public DataTable binarizeDataTable(DataTable dt, string matching_column)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int[] target = dt.AsEnumerable().Select(r => r.Field<int>(matching_column)).ToArray();
            Type coltype = dt.Columns[matching_column].DataType;
            dt.Columns.Remove(matching_column);
            int remove_count = dt.Columns.Count;
            

            IEnumerable<IGrouping<object, DataRow>> _group;
            int colCount = dt.Columns.Count; //save colcount because it changes.
            
            for (int i = 0; i < colCount; i++)
            {
                _group = dt.AsEnumerable().GroupBy(test => test[i]);                
                foreach (var groupingByClassA in _group)
                {
                    dt.Columns.Add(dt.Columns[i].ColumnName.Trim() + "." + groupingByClassA.Key.ToString().Trim(), typeof(int));

                    foreach (DataRow dr in dt.Rows)
                    {
                        dr[dt.Columns[i].ColumnName.Trim() + "." + groupingByClassA.Key.ToString().Trim()] = dr[i].ToString().Trim() == groupingByClassA.Key.ToString().Trim() ? 1 : 0;
                    }
                }
            }
            for (int i = 0; i < remove_count; i++)
            {
                dt.Columns.RemoveAt(0);
            }
            dt.Columns.Add(matching_column,coltype);

            for(int i = 0; i< target.Length; i++)
            {
                dt.Rows[i][matching_column] = target[i];
            }
            // dt.AcceptChanges();   
            watch.Stop();
            Console.WriteLine("Binarize datatable execution time: \t" + watch.ElapsedMilliseconds);
            return dt;
        }

       

        /// <summary>
        /// Function to evaluate the prediction accuracy
        /// </summary>
        /// <param name="main"> vector of values values</param>
        /// <param name="predictions"> vector of predicted values </param>
        /// <returns>accuracy of predictions</returns>
        public double evaluate(Vector<double> main, Vector<double> predictions)
        {                       
            return 1 - main.PointwiseDivide(predictions).Where(t => t == 1).Sum()/main.Count;
        }
        /// <summary>
        /// Insert Column on matrix
        /// </summary>
        /// <param name="defaultValue">default value of column</param>
        /// <param name="data">Matrix with data</param>
        /// <returns> new Matrix with added column</returns>
        /// 
        public double[,] addColumn(double defaultValue, double[,] data)
        {
            double[,] tmp = new double[data.Rows(), data.Columns() + 1];
            for (int i = 0; i < data.Rows(); i++)
            {
                for (int j = 0; j < data.Columns(); j++)
                {
                    tmp[i, j] = data[i, j];
                }
                tmp[i, data.Columns()] = defaultValue;
            }
            return tmp;
        }
        public Vector<double> getVector(double[,] m, string column)
        {
            int columnId = col_name_pair.Single(c => c.Key == column).Value;
            return Vector<double>.Build.SparseOfArray(m.GetColumn(columnId));
        }
        public string[] getStringVector(string[,] m, string column)
        {
            int columnId = col_name_pair.Single(c => c.Key == column).Value;            
            return m.GetColumn(columnId);
        }
        /// <summary>
        /// GetVector from matrix by finding with rowID parameter
        /// </summary>
        /// <param name="m">Matrix dataset</param>
        /// <param name="rowId">id value for query</param>
        /// <param name="idcolumn">id of column for query</param>
        /// <returns>Tuple of Vector and index of the row in Matrix</returns>
        public Tuple<Vector<double>, int> getVector(double[,] m, int rowId, int idcolumn)
        {
            //int columnId = col_name_pair.Single(c => c.Key == column).Value;

            int rowIndex = Array.FindIndex(m.GetColumn(idcolumn), t => t == rowId);

            //if no record found return 
            if (rowIndex < 0)
            {
                return new Tuple<Vector<double>, int>(null, -1);
            }
            return new Tuple<Vector<double>, int>(Vector<double>.Build.SparseOfArray(m.GetRow(rowIndex)), rowIndex);
        }
        /// <summary>
        /// Get double values Matrix from DataTable
        /// </summary>
        /// <param name="dtable">Datatable</param>
        /// <returns>Matrix with double values</returns>
        public double[,] getMatrix(DataTable dtable)
        {
            Regex digitsOnly = new Regex(@"/^\s*\d*\s*$/"); //new Regex(@"[^\d]");

            for (int i = 0; i < dtable.Columns.Count; i++)
            {
                col_name_pair.Add(dtable.Columns[i].ColumnName, i);
            }
            double c = 0;
            for (int i = 0; i < dtable.Rows.Count; i++)
            {
                for (int j = 0; j < dtable.Columns.Count; j++)
                {
                    if(!Double.TryParse(dtable.Rows[i][j].ToString(), out c))
                        {
                        Console.WriteLine(j + "\t" + dtable.Columns[j].ColumnName +"\tEu\t" + dtable.Rows[i][j].ToString());
                    }
                    dtable.Rows[i][j] = digitsOnly.Replace(dtable.Rows[i][j].ToString(), "");
                }
            }
            return dtable.ToMatrix();
        }

        public double[,] getMatrix<T>(IList<T> data)
        {
            DataTable dtable = ToDataTable(data);

            Regex digitsOnly = new Regex(@"/^\s*\d*\s*$/"); //new Regex(@"[^\d]");

            for (int i = 0; i < dtable.Columns.Count; i++)
            {
                col_name_pair.Add(dtable.Columns[i].ColumnName, i);
            }

            for (int i = 0; i < dtable.Rows.Count; i++)
            {
                for (int j = 0; j < dtable.Columns.Count; j++)
                {
                    dtable.Rows[i][j] = digitsOnly.Replace(dtable.Rows[i][j].ToString(), "");
                }
            }
            return dtable.ToMatrix();
        }

        /// <summary>
        /// Get Matrix from csv File
        /// </summary>
        /// <param name="strFilePath">Path of the File</param>
        /// <param name="colspliter">Column spliter</param> 
        /// <param name="dtype">List of column types</param>
        /// <returns></returns>
        public double[,] getMatrix(string strFilePath, char colspliter, List<Type> dtype)
        {
            StreamReader sr = new StreamReader(strFilePath);
            string[] headers = sr.ReadLine().Split(colspliter);
            DataTable dt = new DataTable();
            Regex digitsOnly = new Regex(@"[^\d]");

            for (int i = 0; i < dtype.Count; i++)
            {
                col_name_pair.Add(headers[i], i);
                dt.Columns.Add(headers[i], dtype[i]);
            }
            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split(colspliter);
                DataRow dr = dt.NewRow();
                for (int i = 0; i < headers.Length; i++)
                {
                    //  dr[i] = rows[i];
                    dr[i] = digitsOnly.Replace(rows[i], "");
                }
                dt.Rows.Add(dr);
            }

            return dt.ToMatrix();
        }

        /// <summary>
        /// Get String Matrix from CSV file
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <param name="colspliter"></param>
        /// <param name="dtype"></param>
        /// <returns></returns>
        public string[,] getStringMatrix(string strFilePath, char colspliter, List<Type> dtype)
        {
            StreamReader sr = new StreamReader(strFilePath);
            string[] headers = sr.ReadLine().Split(colspliter);
            DataTable dt = new DataTable();
            Regex wordsOnly = new Regex(@"/\b($word)\b/i");
            
            for (int i = 0; i < dtype.Count; i++)
            {
                col_name_pair.Add(headers[i], i);
                dt.Columns.Add(headers[i], dtype[i]);
            }
            while (!sr.EndOfStream)
            {
                string[] line = sr.ReadLine().SplitCSV().ToArray();                 
                dt.Rows.Add(line);
                
                if (dt.Rows.Count % 100 == 0)
                {
                    Console.WriteLine("reading line: \t" + dt.Rows.Count);
                }
                //Console.WriteLine("reading line: \t" + dt.Rows.Count);
            }

            Console.WriteLine("returning Matrix");
            return dt.ToMatrix<string>();
        }

        public DataTable getDataTableAllTypes(string strFilePath, char colspliter, List<Type> dtype, bool hasHeader)
        {
            StreamReader sr = new StreamReader(strFilePath);


            DataTable dt = new DataTable();
            Regex digitsOnly = new Regex(@"[^\d]");
            if (!hasHeader)
            {
                for (int i = 0; i < dtype.Count; i++)
                {
                    col_name_pair.Add((i).ToString(), i);
                    dt.Columns.Add((i).ToString(), dtype[i]);
                }
            }
            else
            {
                string[] headers = sr.ReadLine().Split(colspliter);
                for (int i = 0; i < dtype.Count; i++)
                {
                    col_name_pair.Add(headers[i], i);
                    dt.Columns.Add(headers[i], dtype[i]);                    
                }
            }
            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split(colspliter);
                DataRow dr = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    //  dr[i] = rows[i];

                    dr[i] = rows[i]; ;                    
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public double[,] getMatrix(string strFilePath, char colspliter, List<Type> dtype, bool hasHeader)
        {
            StreamReader sr = new StreamReader(strFilePath);


            DataTable dt = new DataTable();
            Regex digitsOnly = new Regex(@"[^\d]");
            if (!hasHeader)
            {
                for (int i = 0; i < dtype.Count; i++)
                {
                    col_name_pair.Add((i).ToString(), i);
                    dt.Columns.Add((i).ToString(), dtype[i]);
                }
            }
            else
            {
                string[] headers = sr.ReadLine().Split(colspliter);
                for (int i = 0; i < dtype.Count; i++)
                {
                    col_name_pair.Add(headers[i], i);
                    dt.Columns.Add(headers[i], dtype[i]);
                }
            }
            while (!sr.EndOfStream)
            {
                string[] rows = sr.ReadLine().Split(colspliter);
                DataRow dr = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    //  dr[i] = rows[i];
                    dr[i] = digitsOnly.Replace(rows[i], "");
                }
                dt.Rows.Add(dr);
            }

            return dt.ToMatrix();
        }
        public DataTable ToDataTable<T>(IList<T> data)// T is any generic type
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {                    
                    values[i] = double.Parse(props[i].GetValue(item)==null? "0.0": props[i].GetValue(item).ToString());
                }
                table.Rows.Add(values);
            }
            return table;
        }

        #region static functions

        /// <summary>
        /// Get the range of array
        /// </summary>
        /// <param name="data">data</param>
        /// <param name="step">count step</param>
        /// <param name="rows">num of rows</param>
        /// <returns>new array with length=rows, and values from steps</returns>
        public static double[][] getArrayRange(double[][] data, int startingRow, int step, int rows)
        {
            if (data.Length < (rows * step) + startingRow)
            {
                step -= 1;
                return getArrayRange(data, startingRow, step, rows);
            }
            else
            {
                double[][] temp = new double[rows][];
                int j = 0;
                for (int i = startingRow; i < rows * step; i += step)
                {
                    temp[j] = data[i];
                    j++;
                }
                return temp;
            }
        }
        /// <summary>
        /// Compare arrays are they identitical
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first array "></param>
        /// <param name="second array"></param>
        /// <returns>Boolean</returns>
        public static bool arraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Length; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }
        /// <summary>
        /// Get changes in array in comparison with first one
        /// </summary>
        /// <param name="numbersA"> first array</param>
        /// <param name="numbersB">second array</param>
        /// <returns>number of elements that are different</returns>
        public static int getArrayChanges(int[] numbersA, int[] numbersB)
        {
            int iA = 0;
            int iB = 0;
            List<int> inA = new List<int>();
            List<int> inB = new List<int>();
            List<int> inBoth = new List<int>();
            while (iA < numbersA.Length && iB < numbersB.Length)
            {
                if (numbersA[iA] < numbersB[iB])
                {
                    inA.Add(numbersA[iA++]);
                }
                else if (numbersA[iA] == numbersB[iB])
                {
                    inBoth.Add(numbersA[iA++]);
                    ++iB;
                }
                else
                {
                    inB.Add(numbersB[iB++]);
                }
            }
            while (iA < numbersA.Length)
            {
                inA.Add(numbersA[iA++]);
            }
            while (iB < numbersB.Length)
            {
                inB.Add(numbersB[iB++]);
            }

            return numbersA.Length - inBoth.Count;
        }
        /// <summary>
        /// Chosse number from collection in random maner by defined percentage on parameter
        /// </summary>
        /// <param name="values">collection of percentage for chossen value</param>
        /// <returns>value qith highest percentage to be chossen</returns>
        public static int chooseWithChance(double[] values)
        {
            int countParam = values.Length;
            Random random = new Random();
            double chance = 0;

            for (int i = 0; i < countParam; i++)
            {
                chance += values[i];
            }

            double randomDouble = random.NextDouble() * chance;

            while (chance > randomDouble)
            {
                chance -= values[countParam - 1];
                countParam--;
            }
            return countParam;
        }

        /// <summary>
        /// Sort array and save indexes
        /// </summary>
        /// <param name="value"> vector for sort</param>
        /// <returns>sorted keys</returns>
        public static int[] argsort(double[] value)
        {
            int[] key = new int[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                key[i] = i;
            }
            Array.Sort(value, key);
            //Array.Sort<int>(key, (a, b) => value[a].CompareTo(value[b]));
            return key;
        }
        /// <summary>
        /// get indexes of Min values
        /// </summary>
        /// <param name="d"></param>
        /// <returns>array of indexes of Min Values</returns>
        public static int[] argmin(double[][] d)
        {
            int[] c = new int[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                c[i] = Array.IndexOf(d[i], d[i].Min());
            }
            return c;
        }
        /// <summary>
        /// get indexes of Min values
        /// </summary>
        /// <param name="d"></param>
        /// <returns>array of indexes of Min Values</returns>    
        public static int[] argmin(double[] d)
        {
            int[] c = new int[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                c[i] = Array.IndexOf(d, d.Min());
            }
            return c;
        }
        /// <summary>
        /// get index of Min value
        /// </summary>
        /// <param name="d"></param>
        /// <returns>index of Min Values</returns>
        public static int plain_argmin(double[] d)
        {
            int c = 0;
            c = Array.IndexOf(d, d.Min());
            return c;
        }
        /// <summary>
        /// get indexes of Max values
        /// </summary>
        /// <param name="d"></param>
        /// <returns>array of indexes of Max Values</returns>
        public static int[] argmax(double[][] d)
        {
            int[] c = new int[d.Length];
            for (int i = 0; i < d.Length; i++)
            {
                c[i] = Array.IndexOf(d[i], d[i].Max());
            }
            return c;
        }

        /// <summary>
        /// calculate distances between matrix and vector
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v2"></param>
        /// <returns>array of distances between matrix and a vector</returns>
        public static double[] pairwise_distance(double[][] m, double[] v2)
        {
            double[] result = new double[m.Length];
            for (int i = 0; i < m.Length; i++)
            {
                result[i] = Math.Sqrt(m[i].Zip(v2, (a, b) => (a - b) * (a - b)).Sum());
            }
            return result;
        }
        /// <summary>
        /// calculate distances between matrix and matrix
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v2"></param>
        /// <returns>matrix of distances between matrix and a matrix</returns>
        public static double[][] pairwise_distance(double[][] m, double[][] v2)
        {

            double[][] result_main = new double[m.Length][];
            Common c = new Common();

            for (int i = 0; i < m.Length; i++)
            {
                double[] result = new double[v2.Length];

                for (int j = 0; j < v2.Length; j++)
                {
                    result[j] = pairwise_distance(m[i], v2[j]);
                }
                result_main[i] = result;
            }

            return result_main;
        }
        /// <summary>
        /// calculate distances square between matrix and matrix
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v2"></param>
        /// <returns>matrix of distances between matrix and a matrix</returns>
        public static double[][] pairwise_distance_square(double[][] m, double[][] v2)
        {

            double[][] result_main = new double[m.Length][];
            Common c = new Common();

            for (int i = 0; i < m.Length; i++)
            {
                double[] result = new double[v2.Length];

                for (int j = 0; j < v2.Length; j++)
                {
                    result[j] = pairwise_distance_square(m[i], v2[j]);
                }
                result_main[i] = result;
            }

            return result_main;
        }
        /// <summary>
        /// calculate distances between vectors
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v2"></param>
        /// <returns>double value of distance between vectors</returns>
        public static double pairwise_distance(double[] m, double[] v2)
        {
            return Math.Sqrt(m.Zip(v2, (a, b) => (a - b) * (a - b)).Sum());
        }
        /// <summary>
        /// calculate distances square between vectors
        /// </summary>
        /// <param name="m"></param>
        /// <param name="v2"></param>
        /// <returns>double value of distance between vectors</returns>
        public static double pairwise_distance_square(double[] m, double[] v2)
        {
            return Math.Pow(Math.Sqrt(m.Zip(v2, (a, b) => (a - b) * (a - b)).Sum()),2);
        }
        /// <summary>
        /// Compare array's within tolerance
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="tol"></param>
        /// <returns>If arrays are close True else False</returns>
        public static bool allclose(double[] a, double[] b, double tol)
        {
            for (int i = 0; i < a.Length; i++)
            {
                double v1 = a[i];
                double v2 = b[i];
                if (!(Math.Abs(v1 - v2) < tol))
                    return false;
            }
            return true;
        }

        #endregion
    }

    }
