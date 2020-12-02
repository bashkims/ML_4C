using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ML
{
    class Program
    {
        static void Main(string[] args)
        {
            K_mean k = new K_mean();
            k.doIt(@"../../Data/GExpress.txt", "News", new List<Type>() { typeof(string) }, 5, 400, @"G_res.csv");
        }
    }
}
