using System;
using Evaluation.Library;
using Evaluation.Locking;
using PerformanceTestModel;

namespace PerformanceTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var resultWriter = new ResultWriter())
            {
                /*             
                const int eatCount = 1000;


                var dining = new DiningPhilosophers(eatCount);
                TestRunner.RunTest("STM dining", dining, resultWriter);

                var lockDining = new LockingDiningPhilosophers(eatCount);
                TestRunner.RunTest("Locking dining", lockDining, resultWriter);

                var jvDining = new JVDining(eatCount);
                TestRunner.RunTest("JV dining", jvDining, resultWriter);
                */

                const int nrOfThreads = 4;
                const int updatePercent = 1;
                const int amountOfMappings = 4096;
                const int amountOfOperations = 1000000;

                var hashMapInternalList = new HashmapTester(
                    new STMHashMapInternalList<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("STM internal", hashMapInternalList, resultWriter);

                var jvstmHashMap = new HashmapTester(
                    new JVSTMHashMap<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("JVSTM", jvstmHashMap, resultWriter);

                var lockingHashmap = new HashmapTester(
                    new LockingHashMap<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("STM locking", lockingHashmap, resultWriter);
            }
            
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}