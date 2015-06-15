using System;
using Evaluation.Library;
using Evaluation.Locking;
using PerformanceTestModel;
using Evaluation.Library.Collections;
using Evaluation.Library.Collections;
using Evaluation.Common;
using STMTester;
using STM.Implementation.JVSTM;

namespace PerformanceTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var resultWriter = new ResultWriter())
            {
                          
                const int eatCount = 2000;
                /*
                var dining = new DiningPhilosophers(eatCount);
                TestRunner.RunTest("STM dining", dining, resultWriter);

                var lockDining = new LockingDiningPhilosophers(eatCount);
                TestRunner.RunTest("Locking dining", lockDining, resultWriter);
                
                JVSTMSystem.StartGC();
                var jvDining = new JVDining(eatCount);
                TestRunner.RunTest("JV dining", jvDining, resultWriter);
                JVSTMSystem.StopGC();
                
                //const int nrOfThreads = 16;
                //const int updatePercent = 4;
                const int amountOfMappings = 4096;
                const int amountOfOperations = 100000;

                var updatePercentages = new int[] { 1, 8, 16};

                for (int i = 1; i <= 16; i = i * 2)
                {
                    var nrOfThreads = i;
                    for (int j = 0; j < updatePercentages.Length; j++)
                    {
                        var updatePercent = updatePercentages[j];

                        resultWriter.WriteUpdatePercentage(updatePercent);
                        resultWriter.WriteNrThreads(nrOfThreads);
                        var hashMapInternalList = new HashmapTester(
                            new STMHashMapInternalList<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                            amountOfOperations);
                        TestRunner.RunTest("STM hashmap", hashMapInternalList, resultWriter);


                        resultWriter.WriteUpdatePercentage(updatePercent);
                        resultWriter.WriteNrThreads(nrOfThreads);
                        JVSTMSystem.StartGC();
                        var jvstmHashMap = new HashmapTester(
                            new JVSTMHashMapInternalList<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                            amountOfOperations);
                        TestRunner.RunTest("JVSTM hashmap", jvstmHashMap, resultWriter);
                        JVSTMSystem.StopGC();

                        resultWriter.WriteUpdatePercentage(updatePercent);
                        resultWriter.WriteNrThreads(nrOfThreads);
                        var lockingHashmap = new HashmapTester(
                            new LockingHashMap<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                            amountOfOperations);
                        TestRunner.RunTest("Locking hashmap", lockingHashmap, resultWriter);

                        resultWriter.WriteUpdatePercentage(updatePercent);
                        resultWriter.WriteNrThreads(nrOfThreads);
                        var lockingDictionary = new HashmapTester(
                            new InstrumentedDictionary<int, int>(), nrOfThreads, updatePercent, amountOfMappings,
                            amountOfOperations);
                        TestRunner.RunTest("Locking dictionary", lockingDictionary, resultWriter);

                        resultWriter.Flush();
                        GC.Collect();
                    }
                }*/
                
                const int nrItems = 100000;
                JVSTMSystem.StartGC();
                for (int i = 1; i <= 8; i = i * 2)
                {
                    var nrOfThreads = i;
                    /*
                    resultWriter.WriteNrThreads(nrOfThreads);
                    var lockQueue = new IQueueTester(
                    new Evaluation.Locking.Collections.Queue<int>(), nrItems, nrOfThreads,
                    nrOfThreads);
                    TestRunner.RunTest("Locking queue", lockQueue, resultWriter);

                    resultWriter.WriteNrThreads(nrOfThreads);
                    var lockFreeQueue = new IQueueTester(
                        new MSQueue<int>(), nrItems, nrOfThreads,
                        nrOfThreads);
                    TestRunner.RunTest("Lock-free queue", lockFreeQueue, resultWriter);

                    resultWriter.WriteNrThreads(nrOfThreads);
                    var stmQueue = new STMQueueTester(
                        new Evaluation.Library.Collections.Queue<int>(), nrItems, nrOfThreads,
                        nrOfThreads);
                    TestRunner.RunTest("STM queue", stmQueue, resultWriter);
                    */
                    resultWriter.WriteNrThreads(nrOfThreads);
                    
                    var jvstmQueue = new STMQueueTester(
                        new Evaluation.Library.Collections.JVSTMQueue<int>(), nrItems, nrOfThreads,
                        nrOfThreads);
                    TestRunner.RunTest("JVSTM queue", jvstmQueue, resultWriter);
                    
                    resultWriter.Flush();
                }

                JVSTMSystem.StopGC();
                
            }
            
            Console.WriteLine("Done");
        }
    }
}