using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PerformanceTestModel
{
    public class ResultWriter : IDisposable
    {
        private readonly StreamWriter _sw = new StreamWriter("result.txt");


        public void WriteResult(double mean, double stdv, string testName)
        {
            _sw.WriteLine(testName);
            _sw.WriteLine("Mean: " + mean);
            _sw.WriteLine("Stdv: " + stdv);
            _sw.WriteLine();
        }

        public void Dispose()
        {
            _sw.Dispose();
        }
    }
}
