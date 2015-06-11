using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Library;
using PerformanceTestModel;
using Evaluation.Locking;

namespace PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var resultWriter = new ResultWriter())
            {
                const int eatCount = 1000;
                
                var dining = new DiningPhilosophers(eatCount);
                TestRunner.RunTest("STM dining", dining, resultWriter);

                var lockDining = new LockingDiningPhilosophers(eatCount);
                TestRunner.RunTest("Locking dining", lockDining, resultWriter);
                
                var jvDining = new JVDining(eatCount);
                TestRunner.RunTest("JV dining", jvDining, resultWriter);
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
