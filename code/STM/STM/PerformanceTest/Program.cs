using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Library;
using PerformanceTestModel;

namespace PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var dining = new DiningPhilosophers(10);
            TestRunner.RunTest("STM dining", dining);

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
