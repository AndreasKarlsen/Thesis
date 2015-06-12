using System;
using Evaluation.Library;
using Evaluation.Locking;
using PerformanceTestModel;
using Evaluation.Library.Collections;
using Evaluation.Library.Collections;
using Evaluation.Common;
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
                /*
                var hashMapInternalList = new HashmapTester(
                    new STMHashMapInternalList<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("STM hashmap", hashMapInternalList, resultWriter);
                */
                
                var jvstmHashMap = new HashmapTester(
                    new JVSTMHashMapInternalList<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("JVSTM hashmap", jvstmHashMap, resultWriter);
                /*
                var jvstmNonHashMap = new HashmapTester(
                    new JVSTMHashmapNonAtomic<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("JVSTM non atomic hashmap", jvstmNonHashMap, resultWriter);

                var lockingHashmap = new HashmapTester(
                    new LockingHashMap<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("Locking hashmap", lockingHashmap, resultWriter);
                
                var lockingDictionary = new HashmapTester(
                    new InstrumentedDictionary<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                    amountOfOperations);
                TestRunner.RunTest("Locking dictionary", lockingDictionary, resultWriter);
                */
                
                /*
                const int nrConsumers = 2;
                const int nrProducers = 2;
                const int nrItems = 100000;

                var lockQueue = new IQueueTester(
                    new Evaluation.Locking.Collections.Queue<int>(), nrItems,nrConsumers, 
                    nrProducers);
                TestRunner.RunTest("Locking queue", lockQueue, resultWriter);

                var lockFreeQueue = new IQueueTester(
                    new MSQueue<int>(), nrItems, nrConsumers,
                    nrProducers);
                TestRunner.RunTest("Lock-free queue", lockFreeQueue, resultWriter);

                var stmQueue = new STMQueueTester(
                    new Evaluation.Library.Collections.Queue<int>(), nrItems, nrConsumers,
                    nrProducers);
                TestRunner.RunTest("STM queue", stmQueue, resultWriter);

                var jvstmQueue = new STMQueueTester(
                    new Evaluation.Library.Collections.JVSTMQueue<int>(), nrItems, nrConsumers,
                    nrProducers);
                TestRunner.RunTest("JVSTM queue", jvstmQueue, resultWriter);
                */
            }
            
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}