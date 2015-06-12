using System;
using Evaluation.Library;
using Evaluation.Locking;
using PerformanceTestModel;
using STMTester;

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
                const int updatePercent = 8;
                const int amountOfMappings = 4096;
                const int amountOfOperations = 100000;

                var hashMapInternalList = new HashmapTester(
                    new STMHashMapInternalList<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("STM hashmap", hashMapInternalList, resultWriter);

                var jvstmHashMap = new HashmapTester(
                    new JVSTMHashMap<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("JVSTM hashmap", jvstmHashMap, resultWriter);

                var lockingHashmap = new HashmapTester(
                    new LockingHashMap<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("Locking hashmap", lockingHashmap, resultWriter);

                var lockingDictionary = new HashmapTester(
                    new InstrumentedDictionary<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("Locking dictionary", lockingDictionary, resultWriter);
            }
            
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}