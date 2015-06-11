using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PerformanceTestModel
{
    public class ResultWriter
    {

        public static void WriteResult(double mean, double stdv, string testName)
        {
            using (var sw = new StreamWriter("result.txt"))
            {
                sw.WriteLine(testName);
                sw.WriteLine("Mean: " + mean);
                sw.WriteLine("Stdv: " + stdv);
                sw.WriteLine();
            }
        }
    }
}
