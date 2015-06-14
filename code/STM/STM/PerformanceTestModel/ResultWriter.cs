using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PerformanceTestModel
{
    public class ResultWriter : StreamWriter
    {
        public ResultWriter() : base("result.txt")
        {

        }

        public void WriteResult(double mean, double stdv, string testName)
        {
            WriteLine(testName);
            WriteLine("Mean: " + mean);
            WriteLine("Stdv: " + stdv);
            WriteLine();
        }

        public void WriteUpdatePercentage(int updatePercentage)
        {
            WriteLine("UpdatePercentage: "+updatePercentage);
        }

        public void WriteNrThreads(int nrThreads)
        {
            WriteLine("NrThreads: " + nrThreads);
        }
    }
}
