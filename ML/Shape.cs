using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace My_Shape
{

    [Serializable]
    public class Shape
    {


        private List<int> _shape;

        public Shape()
        {
        }

        public Shape(List<int> shapeList)
        {
            foreach (int i in shapeList)
            {
                _shape.Add(i);
            }
        }

        public bool IsSame(List<int> newShape)
        {
            return _shape.Equals(newShape);
        }

        public static double[][][][] Multiply(double[][][][] x, double[][][][] y, bool broadcast = true)
        {

            List<int> a = getShapeList(x);
            List<int> b = getShapeList(y);
            List<int> new_shape = new List<int>();
            for (int i = 0; i < a.Count; i++)
            {
                if ((a[i] == b[i]) || (a[i] == 1 || b[i] == 1))
                {
                    new_shape.Add(Math.Max(a[i], b[i]));
                    if (new_shape[i] != a[i])
                    {
                        x = tile(x, new_shape[i], i);
                    }
                    if (new_shape[i] != b[i])
                    {
                        y = tile(y, new_shape[i], i);
                    }
                }
                else throw new Exception("Array dimension can not be broadcasted");
            }
            double[][][][] container = (double[][][][])generateArray(new_shape);
            for (int i = 0; i < container.Length; i++)
            {
                for (int j = 0; j < container[i].Length; j++)
                {
                    for (int k = 0; k < container[i][j].Length; k++)
                    {
                        for (int l = 0; l < container[i][j][k].Length; l++)
                        {
                            container[i][j][k][l] = x[i][j][k][l] * y[i][j][k][l];
                        }
                    }
                }
            }

            return container;
        }
        public static double[][][][] reduce_sum(double[][][][] x, int axis)
        {
            List<int> new_shape = getShapeList(x);
            new_shape[axis] = 1;
            int r = 1;
            for (int i = 0; i < new_shape.Count; i++)
            {
                r *= new_shape[i];
            }

            double[] collector = new double[r];

            int counter = 0;
            for (int i = 0; i < x.Length; i++)
            {
                double[] t = flatten(x[i]);
                int lngth = x[i][0].Length * x[i][0][0].Length;

                for (int j = 0; j < lngth; j++)
                {
                    double val = 0;
                    for (int k = j; k < t.Length; k += lngth)
                    {
                        val += t[k];
                    }
                    collector[counter++] = val;
                }
            }
            return (double[][][][])reshape(collector, new_shape);
        }

        public static double reduce_sum(double[][][][] input)
        {
            double collector = 0;

            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            collector += input[i][j][k][l];
                        }
                    }
                }
            }
            return collector;
        }
        public static double[][][][] tile(double[][][][] input, int new_value, int dim = 0)
        {
            double[][][][] tmp;

            if (dim == 0)
            {
                tmp = new double[new_value][][][];
                for (int i = 0; i < new_value; i++)
                {
                    tmp[i] = input[0];
                }
            }
            else if (dim == 1)
            {
                tmp = new double[input.Length][][][];
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = new double[new_value][][];
                    for (int j = 0; j < new_value; j++)
                    {
                        tmp[i][j] = input[i % new_value][0];
                    }
                }
            }
            else if (dim == 2)
            {
                tmp = new double[input.Length][][][];
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = new double[input[i].Length][][];
                    for (int j = 0; j < tmp[i].Length; j++)
                    {
                        tmp[i][j] = new double[new_value][];
                        for (int k = 0; k < new_value; k++)
                        {
                            tmp[i][j][k] = input[i][k % new_value][0];
                        }
                    }
                }
            }
            else if (dim == 3)
            {
                tmp = new double[input.Length][][][];
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = new double[input[i].Length][][];
                    for (int j = 0; j < tmp[i].Length; j++)
                    {
                        tmp[i][j] = new double[input[i][j].Length][];
                        for (int k = 0; k < tmp[i][j].Length; k++)
                        {
                            tmp[i][j][k] = new double[new_value];
                            for (int l = 0; l < new_value; l++)
                            {
                                tmp[i][j][k][l] = input[i][j][l % (new_value)][0];
                            }
                        }
                    }
                }
            }
            else tmp = input;
            return tmp;
        }
        public static string getShapeString(Array arr)
        {
            List<int> s = new List<int>();
            int a = arr.Length;
            string tp = arr.GetType().Name;
            switch (tp)
            {
                case "Double[]":
                    double[] c1 = (double[])arr;
                    s.Add(c1.Length);
                    break;
                case "Double[][]":
                    double[][] c2 = (double[][])arr;
                    s.Add(c2.Length);
                    s.Add(c2[0].Length);
                    break;
                case "Double[][][]":
                    double[][][] c3 = (double[][][])arr;
                    s.Add(c3.Length);
                    s.Add(c3[0].Length);
                    s.Add(c3[0][0].Length);
                    break;
                case "Double[][][][]":
                    double[][][][] c4 = (double[][][][])arr;
                    s.Add(c4.Length);
                    s.Add(c4[0].Length);
                    s.Add(c4[0][0].Length);
                    s.Add(c4[0][0][0].Length);
                    break;
                case "Double[][][][][]":
                    double[][][][][] c5 = (double[][][][][])arr;
                    s.Add(c5.Length);
                    s.Add(c5[0].Length);
                    s.Add(c5[0][0].Length);
                    s.Add(c5[0][0][0].Length);
                    s.Add(c5[0][0][0][0].Length);
                    break;
                default: break;
            }
            return string.Join(", ", s);
        }

        public static double[] flatten(Array arr)
        {
            List<int> shape = getShapeList(arr);
            int rank = shape.Count;

            int agg = 1;
            for (int i = 0; i < shape.Count; i++)
            {
                agg *= shape[i];
            }

            int length = agg;
            double[] collector = new double[length];
            if (rank == 1)
            {
                return (double[])arr;
            }
            if (rank == 2)
            {
                double[][] c = (double[][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        collector[p] = c[i][j];
                        p++;
                    }
                }
            }
            if (rank == 3)
            {
                double[][][] c = (double[][][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        for (int k = 0; k < c[i][j].Length; k++)
                        {
                            collector[p] = c[i][j][k];
                            p++;
                        }
                    }
                }
            }
            if (rank == 4)
            {
                double[][][][] c = (double[][][][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        for (int k = 0; k < c[i][j].Length; k++)
                        {
                            for (int l = 0; l < c[i][j][k].Length; l++)
                            {
                                collector[p] = c[i][j][k][l];
                                p++;
                            }
                        }
                    }
                }
            }
            if (rank == 5)
            {
                double[][][][][] c = (double[][][][][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        for (int k = 0; k < c[i][j].Length; k++)
                        {
                            for (int l = 0; l < c[i][j][k].Length; l++)
                            {
                                for (int m = 0; m < c[i][j][k][l].Length; m++)
                                {
                                    collector[p] = c[i][j][k][l][m];
                                    p++;
                                }
                            }
                        }
                    }
                }
            }
            return collector;
        }

        public static float[] flattenF(Array arr)
        {
            List<int> shape = getShapeListF(arr);
            int rank = shape.Count;

            int agg = 1;
            for (int i = 0; i < shape.Count; i++)
            {
                agg *= shape[i];
            }

            int length = agg;
            float[] collector = new float[length];
            if (rank == 1)
            {
                return (float[])arr;
            }
            if (rank == 2)
            {
                float[][] c = (float[][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        collector[p] = c[i][j];
                        p++;
                    }
                }
            }
            if (rank == 3)
            {
                float[][][] c = (float[][][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        for (int k = 0; k < c[i][j].Length; k++)
                        {
                            collector[p] = c[i][j][k];
                            p++;
                        }
                    }
                }
            }
            if (rank == 4)
            {
                float[][][][] c = (float[][][][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        for (int k = 0; k < c[i][j].Length; k++)
                        {
                            for (int l = 0; l < c[i][j][k].Length; l++)
                            {
                                collector[p] = c[i][j][k][l];
                                p++;
                            }
                        }
                    }
                }
            }
            if (rank == 5)
            {
                float[][][][][] c = (float[][][][][])arr;
                int p = 0;
                for (int i = 0; i < c.Length; i++)
                {
                    for (int j = 0; j < c[i].Length; j++)
                    {
                        for (int k = 0; k < c[i][j].Length; k++)
                        {
                            for (int l = 0; l < c[i][j][k].Length; l++)
                            {
                                for (int m = 0; m < c[i][j][k][l].Length; m++)
                                {
                                    collector[p] = c[i][j][k][l][m];
                                    p++;
                                }
                            }
                        }
                    }
                }
            }
            return collector;
        }
        //public static Array changeType(Array arr, Type t)
        //{
        //    List<int> shape = getShapeList(arr);
        //    int rank = shape.Count;
        //    float[][] collector = (float[][])generateArrayF(shape);
        //    if (rank == 1)
        //    {
        //        return (float[])arr;
        //    }
        //    if (rank == 2)
        //    {
        //        double[][] c = (double[][])arr;
        //        int p = 0;
        //        for (int i = 0; i < c.Length; i++)
        //        {
        //            float[] c1 = Array.ConvertAll(c[i], x => (float)x);
        //            for (int j = 0; j < c1.Length; j++)
        //            {
        //                collector[i][j] = c1[j];
        //                p++;
        //            }
        //        }
        //    }
        //    return collector;
        //}

        public static List<int> getShapeList(Array arr)
        {
            List<int> s = new List<int>();
            int a = arr.Length;
            string tp = arr.GetType().Name;
            switch (tp)
            {
                case "Double[]":
                    double[] c1 = (double[])arr;
                    s.Add(c1.Length);
                    break;
                case "Double[][]":
                    double[][] c2 = (double[][])arr;
                    s.Add(c2.Length);
                    s.Add(c2[0].Length);
                    break;
                case "Double[][][]":
                    double[][][] c3 = (double[][][])arr;
                    s.Add(c3.Length);
                    s.Add(c3[0].Length);
                    s.Add(c3[0][0].Length);
                    break;
                case "Double[][][][]":
                    double[][][][] c4 = (double[][][][])arr;
                    s.Add(c4.Length);
                    s.Add(c4[0].Length);
                    s.Add(c4[0][0].Length);
                    s.Add(c4[0][0][0].Length);
                    break;
                case "Double[][][][][]":
                    double[][][][][] c5 = (double[][][][][])arr;
                    s.Add(c5.Length);
                    s.Add(c5[0].Length);
                    s.Add(c5[0][0].Length);
                    s.Add(c5[0][0][0].Length);
                    s.Add(c5[0][0][0][0].Length);
                    break;
                default: break;
            }
            return s;
        }
        public static List<int> getShapeListF(Array arr)
        {
            List<int> s = new List<int>();
            int a = arr.Length;
            string tp = arr.GetType().Name;
            switch (tp)
            {
                case "Single[]":
                    float[] c1 = (float[])arr;
                    s.Add(c1.Length);
                    break;
                case "Single[][]":
                    float[][] c2 = (float[][])arr;
                    s.Add(c2.Length);
                    s.Add(c2[0].Length);
                    break;
                case "Single[][][]":
                    float[][][] c3 = (float[][][])arr;
                    s.Add(c3.Length);
                    s.Add(c3[0].Length);
                    s.Add(c3[0][0].Length);
                    break;
                case "Single[][][][]":
                    float[][][][] c4 = (float[][][][])arr;
                    s.Add(c4.Length);
                    s.Add(c4[0].Length);
                    s.Add(c4[0][0].Length);
                    s.Add(c4[0][0][0].Length);
                    break;
                case "Single[][][][][]":
                    float[][][][][] c5 = (float[][][][][])arr;
                    s.Add(c5.Length);
                    s.Add(c5[0].Length);
                    s.Add(c5[0][0].Length);
                    s.Add(c5[0][0][0].Length);
                    s.Add(c5[0][0][0][0].Length);
                    break;
                default: break;
            }
            return s;
        }
        public static Array generateArray(List<int> newShape, double defValue = 0)
        {
            switch (newShape.Count)
            {
                case 1:
                    double[] b1 = new double[newShape[0]];
                    return b1;
                case 2:
                    double[][] b2 = new double[newShape[0]][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b2[i] = new double[newShape[1]];
                    }
                    return b2;
                case 3:
                    double[][][] b3 = new double[newShape[0]][][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b3[i] = new double[newShape[1]][];
                        for (int j = 0; j < newShape[1]; j++)
                        {
                            b3[i][j] = new double[newShape[2]];
                        }
                    }
                    return b3;
                case 4:
                    double[][][][] b4 = new double[newShape[0]][][][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b4[i] = new double[newShape[1]][][];
                        for (int j = 0; j < newShape[1]; j++)
                        {
                            b4[i][j] = new double[newShape[2]][];
                            for (int k = 0; k < newShape[2]; k++)
                            {
                                b4[i][j][k] = new double[newShape[3]];
                                if (defValue != 0)
                                {
                                    for (int l = 0; l < newShape[3]; l++)
                                    {
                                        b4[i][j][k][l] = defValue;
                                    }
                                }
                            }
                        }
                    }
                    return b4;

                case 5:
                    double[][][][][] b5 = new double[newShape[0]][][][][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b5[i] = new double[newShape[1]][][][];
                        for (int j = 0; j < newShape[1]; j++)
                        {
                            b5[i][j] = new double[newShape[2]][][];
                            for (int k = 0; k < newShape[2]; k++)
                            {
                                b5[i][j][k] = new double[newShape[3]][];
                                for (int l = 0; l < newShape[3]; l++)
                                {
                                    b5[i][j][k][l] = new double[newShape[4]];
                                }
                            }
                        }
                    }
                    return b5;

                default: return null;
            }
        }

        public static Array generateArrayF(List<int> newShape, float defValue = 0)
        {
            switch (newShape.Count)
            {
                case 1:
                    float[] b1 = new float[newShape[0]];
                    return b1;
                case 2:
                    float[][] b2 = new float[newShape[0]][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b2[i] = new float[newShape[1]];
                    }
                    return b2;
                case 3:
                    float[][][] b3 = new float[newShape[0]][][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b3[i] = new float[newShape[1]][];
                        for (int j = 0; j < newShape[1]; j++)
                        {
                            b3[i][j] = new float[newShape[2]];
                        }
                    }
                    return b3;
                case 4:
                    float[][][][] b4 = new float[newShape[0]][][][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b4[i] = new float[newShape[1]][][];
                        for (int j = 0; j < newShape[1]; j++)
                        {
                            b4[i][j] = new float[newShape[2]][];
                            for (int k = 0; k < newShape[2]; k++)
                            {
                                b4[i][j][k] = new float[newShape[3]];
                                if (defValue != 0)
                                {
                                    for (int l = 0; l < newShape[3]; l++)
                                    {
                                        b4[i][j][k][l] = defValue;
                                    }
                                }
                            }
                        }
                    }
                    return b4;

                case 5:
                    float[][][][][] b5 = new float[newShape[0]][][][][];
                    for (int i = 0; i < newShape[0]; i++)
                    {
                        b5[i] = new float[newShape[1]][][][];
                        for (int j = 0; j < newShape[1]; j++)
                        {
                            b5[i][j] = new float[newShape[2]][][];
                            for (int k = 0; k < newShape[2]; k++)
                            {
                                b5[i][j][k] = new float[newShape[3]][];
                                for (int l = 0; l < newShape[3]; l++)
                                {
                                    b5[i][j][k][l] = new float[newShape[4]];
                                }
                            }
                        }
                    }
                    return b5;

                default: return null;
            }
        }
        public static Array reshape(Array a, List<int> newShape)
        {
            Array emptyArr = generateArray(newShape);
            double[] tmp;
            if (a.Rank > 1)
            {
                //shendrroje ne vektore
                tmp = flatten(a);
            }
            else tmp = (double[])a;
            //mbledhi sa vlera jane
            int sum = 0;
            for (int i = 0; i < newShape.Count; i++)
            {
                sum += newShape[i];
            }
            int aLength = (int)Math.Ceiling(tmp.Length / (double)sum);

            dynamic c;
            int shp = newShape.Count;
            if (shp == 1)
            {
                c = tmp;
            }

            if (shp == 2)
            {
                c = (double[][])emptyArr;
                int cnt = 0;
                for (int i = 0; i < newShape[0]; i++)
                {
                    for (int j = 0; j < newShape[1] && cnt < tmp.Length; j++)
                    {
                        c[i][j] = tmp[cnt];
                        cnt++;
                    }
                }
            }

            else if (shp == 3)
            {
                c = (double[][][])emptyArr;

                int cnt = 0;
                for (int i = 0; i < newShape[0]; i++)
                {
                    for (int j = 0; j < newShape[1]; j++)
                    {
                        for (int k = 0; k < newShape[2] && cnt < tmp.Length; k++)
                        {
                            c[i][j][k] = tmp[cnt];
                            cnt++;
                        }
                    }
                }
            }
            else if (shp == 4)
            {
                c = (double[][][][])emptyArr;
                int cnt = 0;
                for (int i = 0; i < newShape[0]; i++)
                {
                    for (int j = 0; j < newShape[1]; j++)
                    {
                        for (int k = 0; k < newShape[2]; k++)
                        {
                            for (int l = 0; l < newShape[3] && cnt < tmp.Length; l++)
                            {
                                c[i][j][k][l] = tmp[cnt];
                                cnt++;
                            }
                        }
                    }
                }
            }
            else if (shp == 5)
            {
                c = (double[][][][][])emptyArr;
                int cnt = 0;
                for (int i = 0; i < newShape[0]; i++)
                {
                    for (int j = 0; j < newShape[1]; j++)
                    {
                        for (int k = 0; k < newShape[2]; k++)
                        {
                            for (int l = 0; l < newShape[3]; l++)
                            {
                                for (int m = 0; m < newShape[4] && cnt < tmp.Length; m++)
                                {
                                    c[i][j][k][l][m] = tmp[cnt];
                                    cnt++;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return null;
            }
            return c;
        }
        public static Array reshapeBroadcast(Array a, List<int> newShape, double broadcastValue = 0)
        {
            Array emptyArr = generateArray(newShape, broadcastValue);
            //shendrroje ne vektore
            double[] tmp = flatten(a);
            double[][][][] arr = (double[][][][])a;
            int sum = 0;
            for (int i = 0; i < newShape.Count; i++)
            {
                sum += newShape[i];
            }
            //mbledhi sa vlera jane
            int aLength = (int)Math.Ceiling(tmp.Length / (double)sum);

            dynamic c;
            int shp = newShape.Count;
            if (shp == 1)
            {
                c = tmp;
            }

            if (shp == 2)
            {
                c = (double[][])emptyArr;
                int dim1 = 0;
                for (int i = 0; i < newShape[0]; i++)
                {
                    for (int j = 0; j < newShape[1] && dim1 < tmp.Length; j++)
                    {

                        c[i][j] = tmp[dim1];
                        dim1++;
                    }
                }
            }

            else if (shp == 3)
            {
                c = (double[][][])emptyArr;

                int dim1 = 0;
                for (int i = 0; i < newShape[0]; i++)
                {
                    for (int j = 0; j < newShape[1]; j++)
                    {
                        for (int k = 0; k < newShape[2] && dim1 < tmp.Length; k++)
                        {
                            c[i][j][k] = tmp[dim1];
                            dim1++;
                        }
                    }
                }
            }
            else if (shp == 4)
            {
                c = (double[][][][])emptyArr;
                for (int i = 0; i < arr.Length; i++)
                {
                    for (int j = 0; j < arr[i].Length; j++)
                    {
                        for (int k = 0; k < arr[i][j].Length; k++)
                        {
                            for (int l = 0; l < arr[i][j][k].Length; l++)
                            {
                                c[i][j][k][l] = arr[i][j][k][l];
                            }
                        }
                    }
                }
            }
            else if (shp == 5)
            {
                c = (double[][][][][])emptyArr;
                int dim1 = 0;
                for (int i = 0; i < newShape[0]; i++)
                {
                    for (int j = 0; j < newShape[1]; j++)
                    {
                        for (int k = 0; k < newShape[2]; k++)
                        {
                            for (int l = 0; l < newShape[3]; l++)
                            {
                                for (int m = 0; m < newShape[4] && dim1 < tmp.Length; m++)
                                {
                                    c[i][j][k][l][m] = tmp[dim1];
                                    dim1++;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return null;
            }
            return c;
        }

        public static double[][][][] Merge(double[][][][] x, double[][][][] y, bool broadcast = true)
        {

            List<int> _x = getShapeList(x);
            List<int> _y = getShapeList(y);
            List<int> new_shape = new List<int>();
            for (int i = 0; i < _x.Count; i++)
            {
                if ((_x[i] == _y[i]) || (_x[i] == 1 || _y[i] == 1))
                {
                    new_shape.Add(Math.Max(_x[i], _y[i]));
                }
                else throw new Exception("Array dimension can not be broadcasted");
            }

            double[][][][] x_reshaped = (double[][][][])reshapeBroadcast(x, new_shape);
            double[][][][] y_reshaped = (double[][][][])reshapeBroadcast(y, new_shape);

            for (int i = 0; i < new_shape.Count; i++)
            {
                if (_x[i] != new_shape[i])
                    x_reshaped = tile(x_reshaped, new_shape[i], i);
                if (_y[i] != new_shape[i])
                    y_reshaped = tile(y_reshaped, new_shape[i], i);
            }

            double[][][][] collector = (double[][][][])generateArray(new_shape);
            for (int i = 0; i < new_shape[0]; i++)
            {
                for (int j = 0; j < new_shape[1]; j++)
                {
                    for (int k = 0; k < new_shape[2]; k++)
                    {
                        for (int l = 0; l < new_shape[3]; l++)
                        {
                            collector[i][j][k][l] = x_reshaped[i][j][k][l] + y_reshaped[i][j][k][l];
                        }
                    }
                }
            }
            return collector;
        }
        public Array bulkC(Array a, Array b)
        {
            Buffer.BlockCopy(a, 0, b, 0, a.Length);
            return b;
        }

        public static void Test1D(int axis)
        {
            List<Array> ld = new List<Array>();
            int counter = 0;
            for (int a = 0; a < 2; a++)
            {
                double[] arrayOutput = new double[1];

                for (int i = 0; i < arrayOutput.Length; i++)
                {
                    arrayOutput[i] = counter++;
                }
                ld.Add(arrayOutput);
            }
            Console.WriteLine("old: " + getShapeString(ld[0]));
            Console.WriteLine("new: " + getShapeString(Concatinate(ld, axis)));
        }

        public static void Test2D(int axis)
        {
            List<Array> ld = new List<Array>();
            int counter = 0;
            for (int a = 0; a < 2; a++)
            {
                double[][] arrayOutput = new double[1][];

                for (int i = 0; i < arrayOutput.Length; i++)
                {
                    arrayOutput[i] = new double[1];
                    for (int j = 0; j < arrayOutput[i].Length; j++)
                    {
                        arrayOutput[i][j] = counter++;
                    }
                }
                ld.Add(arrayOutput);
            }
            Console.WriteLine("old: " + getShapeString(ld[0]));
            Console.WriteLine("new: " + getShapeString(Concatinate(ld, axis)));
        }

        public static void Test3D(int axis)
        {
            List<Array> ld = new List<Array>();
            int counter = 0;
            for (int a = 0; a < 2; a++)
            {
                double[][][] arrayOutput = new double[1][][];

                for (int i = 0; i < arrayOutput.Length; i++)
                {
                    arrayOutput[i] = new double[1][];
                    for (int j = 0; j < arrayOutput[i].Length; j++)
                    {
                        arrayOutput[i][j] = new double[1];
                        for (int k = 0; k < arrayOutput[i][j].Length; k++)
                        {
                            arrayOutput[i][j][k] = counter++;
                        }
                    }
                }
                ld.Add(arrayOutput);
            }
            Console.WriteLine("old: " + getShapeString(ld[0]));
            Console.WriteLine("new: " + getShapeString(Concatinate(ld, axis)));
        }

        public static void Test4D(int axis)
        {
            List<Array> ld = new List<Array>();
            int counter = 0;
            for (int a = 0; a < 2; a++)
            {
                double[][][][] arrayOutput = new double[1][][][];

                for (int i = 0; i < arrayOutput.Length; i++)
                {
                    arrayOutput[i] = new double[1][][];
                    for (int j = 0; j < arrayOutput[i].Length; j++)
                    {
                        arrayOutput[i][j] = new double[1][];
                        for (int k = 0; k < arrayOutput[i][j].Length; k++)
                        {
                            arrayOutput[i][j][k] = new double[1];
                            for (int l = 0; l < arrayOutput[i][j][k].Length; l++)
                            {
                                arrayOutput[i][j][k][l] = counter++;

                            }
                        }
                    }
                }
                ld.Add(arrayOutput);
            }
            Console.WriteLine("old: " + getShapeString(ld[0]));
            Console.WriteLine("new: " + getShapeString(Concatinate(ld, axis)));
        }
        public static void Test5D(int axis)
        {
            List<Array> ld = new List<Array>();
            for (int a = 0; a < 8; a++)
            {
                double[][][][][] arrayOutput = new double[40][][][][];
                int counter = 0;
                for (int i = 0; i < arrayOutput.Length; i++)
                {
                    arrayOutput[i] = new double[1152][][][];
                    for (int j = 0; j < arrayOutput[i].Length; j++)
                    {
                        arrayOutput[i][j] = new double[1][][];
                        for (int k = 0; k < arrayOutput[i][j].Length; k++)
                        {
                            arrayOutput[i][j][k] = new double[1][];
                            for (int l = 0; l < arrayOutput[i][j][k].Length; l++)
                            {
                                arrayOutput[i][j][k][l] = new double[2];
                                for (int m = 0; m < arrayOutput[i][j][k][l].Length; m++)
                                {
                                    arrayOutput[i][j][k][l][m] = counter++;
                                }
                            }
                        }
                    }
                }
                ld.Add(arrayOutput);
            }

            Console.WriteLine("old: " + getShapeString(ld[0]));
            Console.WriteLine("new: " + getShapeString(Concatinate(ld, axis)));
        }
        public static dynamic Concatinate(List<Array> larr, int axis)
        {
            string tp = larr[0].GetType().Name;
            switch (tp)
            {
                case "Double[]":
                    List<double[]> input = new List<double[]>();
                    foreach (var c in larr)
                    {
                        if (axis > 0) throw new Exception("Invalid axis");
                        input.Add((double[])c);
                    }
                    return Concat(input);
                case "Double[][]":
                    if (axis > 1) throw new Exception("Invalid axis");
                    List<double[][]> input1 = new List<double[][]>();
                    foreach (var c in larr)
                    {
                        input1.Add((double[][])c);
                    }
                    switch (axis)
                    {
                        case 0:
                            return ConcatDim0(input1);
                        case 1:
                            return ConcatDim1(input1);
                    }
                    break;
                case "Double[][][]":
                    if (axis > 2) throw new Exception("Invalid axis");
                    List<double[][][]> input2 = new List<double[][][]>();
                    foreach (var c in larr)
                    {
                        input2.Add((double[][][])c);
                    }
                    switch (axis)
                    {
                        case 0:
                            return ConcatDim0(input2);
                        case 1:
                            return ConcatDim1(input2);
                        case 2:
                            return ConcatDim2(input2);
                    }
                    break;
                case "Double[][][][]":
                    if (axis > 3) throw new Exception("Invalid axis");
                    List<double[][][][]> input3 = new List<double[][][][]>();
                    foreach (var c in larr)
                    {
                        input3.Add((double[][][][])c);
                    }
                    switch (axis)
                    {
                        case 0:
                            return ConcatDim0(input3);
                        case 1:
                            return ConcatDim1(input3);
                        case 2:
                            return ConcatDim2(input3);
                        case 3:
                            return ConcatDim3(input3);
                    }
                    break;

                case "Double[][][][][]":
                    if (axis > 4) { throw new Exception("Invalid axis"); }
                    List<double[][][][][]> input4 = new List<double[][][][][]>();
                    foreach (var c in larr)
                    {
                        input4.Add((double[][][][][])c);
                    }
                    switch (axis)
                    {
                        case 0:
                            return ConcatDim0(input4);
                        case 1:
                            return ConcatDim1(input4);
                        case 2:
                            return ConcatDim2(input4);
                        case 3:
                            return ConcatDim3(input4);
                        case 4:
                            return ConcatDim4(input4);
                    }
                    break;
                default: return larr;
            }
            return larr;

        }
        public static dynamic Concat(List<double[]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[0] = new_size;
            double[] tmp = (double[])generateArray(shape);

            int dim1 = 0;
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    tmp[dim1] = input[i][j];
                    dim1++;
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim0(List<double[][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[0] = new_size;
            double[][] tmp = (double[][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        tmp[dim1][dim2] = input[i][j][k];

                        if (dim2 == input[i][j].Length - 1)
                        {
                            dim2 = 0;
                            dim1++;
                        }
                        else dim2++;
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim1(List<double[][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[1] = new_size;
            double[][] tmp = (double[][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        tmp[dim1][dim2] = input[i][j][k];

                        if (dim2 == new_size - 1)
                        {
                            dim2 = 0;
                            dim1++;
                        }
                        else dim2++;
                    }

                }
            }
            return tmp;
        }
        public static dynamic ConcatDim0(List<double[][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[0] = new_size;
            double[][][] tmp = (double[][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            tmp[dim1][dim2][dim3] = input[i][j][k][l];

                            if (dim3 == input[i][j][k].Length - 1)
                            {
                                dim3 = 0;
                            }
                            else dim3++;
                        }
                        if (dim2 == input[i][j].Length - 1)
                        {
                            dim1++;
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim1(List<double[][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[1] = new_size;
            double[][][] tmp = (double[][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            tmp[dim1][dim2][dim3] = input[i][j][k][l];

                            if (dim3 == input[i][j][k].Length - 1)
                            {
                                dim3 = 0;
                            }
                            else dim3++;
                        }
                        if (dim2 == new_size - 1)
                        {
                            dim1++;
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim2(List<double[][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[2] = new_size;
            double[][][] tmp = (double[][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            tmp[dim1][dim2][dim3] = input[i][j][k][l];
                            if (dim3 == new_size - 1)
                            {
                                dim3 = 0;
                                if (dim2 == input[i][j].Length - 1)
                                {
                                    dim2 = 0;
                                }
                                else dim2++;
                                if (dim1 == input[i].Length - 1)
                                {
                                    dim1 = 0;
                                }
                                else dim1++;
                            }
                            else dim3++;
                        }

                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim0(List<double[][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[0] = new_size;
            double[][][][] tmp = (double[][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                tmp[dim1][dim2][dim3][dim4] = input[i][j][k][l][p];

                                if (dim4 == input[i][j][k][l].Length - 1)
                                {
                                    dim4 = 0;
                                }
                                else dim4++;
                            }
                            if (dim3 == input[i][j][k].Length - 1)
                            {
                                dim3 = 0;
                            }
                            else dim3++;
                        }
                        if (dim2 == input[i][j].Length - 1)
                        {
                            dim1++;
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim1(List<double[][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[1] = new_size;
            double[][][][] tmp = (double[][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                tmp[dim1][dim2][dim3][dim4] = input[i][j][k][l][p];
                                if (dim4 == input[i][j][k][l].Length - 1)
                                {
                                    dim4 = 0;
                                }
                                else dim4++;
                            }
                            if (dim3 == input[i][j][k].Length - 1)
                            {
                                dim3 = 0;
                            }
                            else dim3++;
                        }
                        if (dim2 == new_size - 1)
                        {
                            dim1++;
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim2(List<double[][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[2] = new_size;
            double[][][][] tmp = (double[][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                tmp[dim1][dim2][dim3][dim4] = input[i][j][k][l][p];
                                if (dim4 == input[i][j][k][l].Length - 1)
                                {
                                    dim4 = 0;
                                    if (dim3 == new_size - 1)
                                    {
                                        dim3 = 0;
                                        if (dim2 == input[i][j].Length - 1)
                                        {
                                            dim2 = 0;
                                        }
                                        else dim2++;
                                        if (dim1 == input[i].Length - 1)
                                        {
                                            dim1 = 0;
                                        }
                                        else dim1++;
                                    }
                                    else dim3++;
                                }
                                else dim4++;
                            }

                        }

                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim3(List<double[][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0][0][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[3] = new_size;
            double[][][][] tmp = (double[][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                tmp[dim1][dim2][dim3][dim4] = input[i][j][k][l][p];
                                if (dim4 == new_size - 1)
                                {
                                    dim4 = 0;
                                    if (dim3 == input[i][j][k].Length - 1)
                                    {
                                        dim3 = 0;
                                        if (dim2 == input[i][j].Length - 1)
                                        {
                                            dim2 = 0;
                                        }
                                        else dim2++;
                                        if (dim1 == input[i].Length - 1)
                                        {
                                            dim1 = 0;
                                        }
                                        else dim1++;
                                    }
                                    else dim3++;
                                }
                                else dim4++;
                            }

                        }
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim0(List<double[][][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[0] = new_size;
            double[][][][][] tmp = (double[][][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;
            int dim5 = 0;
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                for (int q = 0; q < input[i][j][k][l][p].Length; q++)
                                {
                                    tmp[dim1][dim2][dim3][dim4][dim5] = input[i][j][k][l][p][q];

                                    if (dim5 == input[i][j][k][l][p].Length - 1)
                                    {
                                        dim5 = 0;
                                    }
                                    else dim5++;
                                }
                                if (dim4 == input[i][j][k][l].Length - 1)
                                {
                                    dim4 = 0;
                                }
                                else dim4++;
                            }
                            if (dim3 == input[i][j][k].Length - 1)
                            {
                                dim3 = 0;
                            }
                            else dim3++;
                        }
                        if (dim2 == input[i][j].Length - 1)
                        {
                            dim1++;
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim1(List<double[][][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[1] = new_size;
            double[][][][][] tmp = (double[][][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;
            int dim5 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                for (int q = 0; q < input[i][j][k][l].Length; q++)
                                {
                                    tmp[dim1][dim2][dim3][dim4] = input[i][j][k][l][p];
                                    if (dim5 == input[i][j][k][l][p].Length - 1)
                                    {
                                        dim5 = 0;
                                    }
                                    else dim5++;
                                }
                                if (dim4 == input[i][j][k][l].Length - 1)
                                {
                                    dim4 = 0;
                                }
                                else dim4++;
                            }
                            if (dim3 == input[i][j][k].Length - 1)
                            {
                                dim3 = 0;
                            }
                            else dim3++;
                        }
                        if (dim2 == new_size - 1)
                        {
                            dim1++;
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim2(List<double[][][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[2] = new_size;
            double[][][][][] tmp = (double[][][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;
            int dim5 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                for (int q = 0; q < input[i][j][k][l][p].Length; q++)
                                {
                                    tmp[dim1][dim2][dim3][dim4][dim5] = input[i][j][k][l][p][q];
                                    if (dim5 == input[i][j][k][l][p].Length - 1)
                                    {
                                        dim5 = 0;
                                    }
                                    else dim5++;
                                }
                                if (dim4 == input[i][j][k][l].Length - 1)
                                {
                                    dim4 = 0;
                                }
                                else dim4++;
                            }
                            if (dim3 == new_size - 1)
                            {
                                dim3 = 0;
                            }
                            else dim3++;
                        }
                        if (dim2 == input[i][j].Length - 1)
                        {
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                    if (dim1 == input[i].Length - 1)
                    {
                        dim1 = 0;
                    }
                    else dim1++;
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim3(List<double[][][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0][0][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[3] = new_size;
            double[][][][][] tmp = (double[][][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;
            int dim5 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                for (int q = 0; q < input[i][j][k][l][p].Length; q++)
                                {
                                    tmp[dim1][dim2][dim3][dim4][dim5] = input[i][j][k][l][p][q];

                                    if (dim5 == input[i][j][k][l][p].Length - 1)
                                    {
                                        dim5 = 0;
                                    }
                                    else dim5++;
                                }
                                if (dim4 == new_size - 1)
                                {
                                    dim4 = 0;
                                    if (dim3 == input[i][j][k].Length - 1)
                                    {
                                        dim3 = 0;
                                    }
                                    else dim3++;
                                }
                                else dim4++;
                            }
                        }
                        if (dim2 == input[i][j].Length - 1)
                        {
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                    if (dim1 == input[i].Length - 1)
                    {
                        dim1 = 0;
                    }
                    else dim1++;
                }
            }
            return tmp;
        }
        public static dynamic ConcatDim4(List<double[][][][][]> input)
        {
            int new_size = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i][0][0][0][0].Length; j++)
                {
                    new_size++;
                }
            }
            List<int> shape = getShapeList(input[0]);
            shape[4] = new_size;
            double[][][][][] tmp = (double[][][][][])generateArray(shape);

            int dim1 = 0;
            int dim2 = 0;
            int dim3 = 0;
            int dim4 = 0;
            int dim5 = 0;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            for (int p = 0; p < input[i][j][k][l].Length; p++)
                            {
                                for (int q = 0; q < input[i][j][k][l][p].Length; q++)
                                {
                                    tmp[dim1][dim2][dim3][dim4][dim5] = input[i][j][k][l][p][q];
                                    if (dim5 == new_size - 1)
                                    {
                                        dim5 = 0;
                                        if (dim4 == input[i][j][k][l].Length - 1)
                                        {
                                            dim4 = 0;
                                            if (dim3 == input[i][j][k].Length - 1)
                                            {
                                                dim3 = 0;
                                            }
                                            else dim3++;
                                        }
                                        else dim4++;
                                    }
                                    else dim5++;
                                }
                            }
                        }
                        if (dim2 == input[i][j].Length - 1)
                        {
                            dim2 = 0;
                        }
                        else dim2++;
                    }
                    if (dim1 == input[i].Length - 1)
                    {
                        dim1 = 0;
                    }
                    else dim1++;
                }
            }
            return tmp;
        }


        public static double[][][][] matmul4d(double[][][][] m1, double[][][][] m2)
        {
            if (m1[0][0][0].Length != m2[0][0].Length) throw new InvalidOperationException("Shumezimi i pamundur!!!\nnumri i kolonave te matrices se pare [" + m1[0][0][0].Length + "]"
           + " nuk perputhet me numrin e rreshtave [" + m2[0][0].Length + "] te matrices se dyte");

            double[][][][] rez;
            rez = new double[m1.Length][][][];
            for (int l = 0; l < m2.Length; l++)
            {
                rez[l] = new double[m2[0].Length][][];
                for (int p = 0; p < m2[0].Length; p++)
                {
                    rez[l][p] = new double[m1[0][0].Length][];
                    for (int i = 0; i < m1[0][0].Length; i++)
                    {
                        rez[l][p][i] = new double[m2[0][0][0].Length];
                        for (int j = 0; j < m2[0][0][0].Length; j++)
                        {
                            for (int k = 0; k < m2[0][0].Length; k++)
                            {
                                if (k == m2[0][0].Length - 1)
                                {
                                    double a = m1[l][p][i][k] * m2[l][p][k][j];
                                }
                                rez[l][p][i][j] += m1[l][p][i][k] * m2[l][p][k][j];
                            }
                        }
                    }
                }
            }
            return rez;
        }
        public static double[][][][] Transpose4d(double[][][][] m2)
        {
            double[][][][] rez;
            rez = new double[m2.Length][][][];
            for (int l = 0; l < m2.Length; l++)
            {
                rez[l] = new double[m2[0].Length][][];
                for (int p = 0; p < m2[0].Length; p++)
                {
                    rez[l][p] = new double[m2[0][0][0].Length][];
                    for (int i = 0; i < m2[0][0][0].Length; i++)
                    {
                        rez[l][p][i] = new double[m2[0][0].Length];
                        for (int j = 0; j < m2[0][0].Length; j++)
                        {
                            rez[l][p][i][j] = m2[l][p][j][i];
                        }
                    }
                }
            }
            return rez;
        }

        public static List<double[][][][]> split(double[][][][] data, List<int> size_splits)
        {
            double[][][][] _data = data;
            List<double[][][][]> rez = new List<double[][][][]>();
            for (int p = 0; p < size_splits.Count; p++)
            {
                double[][][][] b_I1 = new double[data.Length][][][];
                for (int j = 0; j < data.Length; j++)
                {
                    b_I1[j] = new double[_data[j].Length][][];
                    for (int k = 0; k < _data[j].Length; k++)
                    {
                        for (int l = 0; l < _data[j][k].Length; l++)
                        {
                            b_I1[j][k] = _data[j][k].Take(size_splits[p]).ToArray();
                        }
                    }
                }
                rez.Add(b_I1);
            }
            return rez;
        }

        public static double[][][][] sqrt(double[][][][] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            input[i][j][k][l] = Math.Sqrt(input[i][j][k][l]);
                        }
                    }
                }
            }
            return input;
        }

        public static double[][][][] square(double[][][][] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            input[i][j][k][l] = Math.Pow(input[i][j][k][l], 2);
                        }
                    }
                }
            }
            return input;
        }

        public static double[][][][] addConstant(double[][][][] input, double constant)
        {
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            input[i][j][k][l] = input[i][j][k][l] + constant;
                        }
                    }
                }
            }
            return input;
        }
        public static double[][][][] divide(double[][][][] m1, double[][][][] m2)
        {
            List<int> _x = getShapeList(m1);
            List<int> _y = getShapeList(m2);

            //if (!_x.Equals(_y)) throw new InvalidOperationException("Pjestimi i pamundur!!!\ndimensionet e matrices se pare "+getShapeString(m1)+"\nnuk perputhen me dimensionet e matrices se dyte"+getShapeString(m2));


            List<int> new_shape = new List<int>();
            for (int i = 0; i < _x.Count; i++)
            {
                if ((_x[i] == _y[i]) || (_x[i] == 1 || _y[i] == 1))
                {
                    new_shape.Add(Math.Max(_x[i], _y[i]));
                }
                else throw new Exception("Array dimension can not be broadcasted");
            }

            double[][][][] x_reshaped = (double[][][][])reshapeBroadcast(m1, new_shape);
            double[][][][] y_reshaped = (double[][][][])reshapeBroadcast(m2, new_shape);

            for (int i = 0; i < new_shape.Count; i++)
            {
                if (_x[i] != new_shape[i])
                    x_reshaped = tile(x_reshaped, new_shape[i], i);
                if (_y[i] != new_shape[i])
                    y_reshaped = tile(y_reshaped, new_shape[i], i);
            }

            double[][][][] collector = (double[][][][])generateArray(new_shape);
            for (int i = 0; i < new_shape[0]; i++)
            {
                for (int j = 0; j < new_shape[1]; j++)
                {
                    for (int k = 0; k < new_shape[2]; k++)
                    {
                        for (int l = 0; l < new_shape[3]; l++)
                        {
                            collector[i][j][k][l] = x_reshaped[i][j][k][l] / y_reshaped[i][j][k][l];
                        }
                    }
                }
            }
            return collector;
        }
        public static double[][][][] prod(double[][][][] m1, double[][][][] m2)
        {
            List<int> _x = getShapeList(m1);
            List<int> _y = getShapeList(m2);

            //if (!_x.Equals(_y)) throw new InvalidOperationException("Pjestimi i pamundur!!!\ndimensionet e matrices se pare "+getShapeString(m1)+"\nnuk perputhen me dimensionet e matrices se dyte"+getShapeString(m2));


            List<int> new_shape = new List<int>();
            for (int i = 0; i < _x.Count; i++)
            {
                if ((_x[i] == _y[i]) || (_x[i] == 1 || _y[i] == 1))
                {
                    new_shape.Add(Math.Max(_x[i], _y[i]));
                }
                else throw new Exception("Array dimension can not be broadcasted");
            }

            double[][][][] x_reshaped = (double[][][][])reshapeBroadcast(m1, new_shape);
            double[][][][] y_reshaped = (double[][][][])reshapeBroadcast(m2, new_shape);

            for (int i = 0; i < new_shape.Count; i++)
            {
                if (_x[i] != new_shape[i])
                    x_reshaped = tile(x_reshaped, new_shape[i], i);
                if (_y[i] != new_shape[i])
                    y_reshaped = tile(y_reshaped, new_shape[i], i);
            }

            double[][][][] collector = (double[][][][])generateArray(new_shape);
            for (int i = 0; i < new_shape[0]; i++)
            {
                for (int j = 0; j < new_shape[1]; j++)
                {
                    for (int k = 0; k < new_shape[2]; k++)
                    {
                        for (int l = 0; l < new_shape[3]; l++)
                        {
                            collector[i][j][k][l] = x_reshaped[i][j][k][l] * y_reshaped[i][j][k][l];
                        }
                    }
                }
            }
            return collector;
        }

        public static double[][][][] divide(double[][][][] m1, double m2)
        {
            double[][][][] collector = (double[][][][])generateArray(getShapeList(m1));
            List<int> new_shape = getShapeList(m1);
            for (int i = 0; i < new_shape[0]; i++)
            {
                for (int j = 0; j < new_shape[1]; j++)
                {
                    for (int k = 0; k < new_shape[2]; k++)
                    {
                        for (int l = 0; l < new_shape[3]; l++)
                        {
                            collector[i][j][k][l] = m1[i][j][k][l] / m2;
                        }
                    }
                }
            }
            return collector;
        }
        public static double[][][][] multConstant(double[][][][] input, double constant)
        {
            double[][][][] collector = (double[][][][])generateArray(getShapeList(input));
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < input[i].Length; j++)
                {
                    for (int k = 0; k < input[i][j].Length; k++)
                    {
                        for (int l = 0; l < input[i][j][k].Length; l++)
                        {
                            collector[i][j][k][l] = input[i][j][k][l] * constant;
                        }
                    }
                }
            }
            return collector;
        }

    }
}
