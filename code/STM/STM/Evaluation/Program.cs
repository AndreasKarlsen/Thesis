using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;
using Evaluation.Library;
using Evaluation.Locking;
using System.IO;

namespace Evaluation
{
    class Program
    {
        static void Main(string[] args)
        {
            //DiningPhilosophers.Start();
            //LockingDiningPhilosophers.Start();
            //SantaClausProblem.Start();
            //LockingSantaClausProblem.Start();
            //HashMapTest();
            //STMHashMapSequentialTest();
            //LockingHashMapSequentialTest();

            int nrThreads = 4;
            using(var s = new FileStream("output.txt",FileMode.Create))
            {
                using(var sw = new StreamWriter(s))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        LockingHashMapConcurrent(nrThreads, sw);
                    }

                    Console.WriteLine("Locking done");

                    for (int i = 0; i < 10; i++)
                    {
                        NaiveLockingHashMapConcurrent(nrThreads, sw);
                    }

                    Console.WriteLine("Naive locking done");

                    for (int i = 0; i < 10; i++)
                    {
                        STMHashMapConcurrent(nrThreads, sw);
                    }

                    Console.WriteLine("STM done");
                }
            }

            Console.ReadKey();
        }

        public static void STMHashMapConcurrent(int nrThreads, StreamWriter writer)
        {
            writer.WriteLine("STM hashmap");
            var map = new StmHashMap<int, int>();
            TestMapConcurrent(map, nrThreads, writer);
        }

        public static void STMHashMapRetryConcurrent(int nrThreads, StreamWriter writer)
        {
            writer.WriteLine("STM hashmap retry");
            var map = new StmHashMapRetry<int, int>();
            TestMapConcurrent(map, nrThreads, writer);
        }

        public static void NaiveLockingHashMapConcurrent(int nrThreads, StreamWriter writer)
        {
            writer.WriteLine("NaiveLocking hashmap");
            var map = new NaiveLockingHashMap<int, int>();
            TestMapConcurrent(map, nrThreads, writer);
        }

        public static void LockingHashMapConcurrent(int nrThreads, StreamWriter writer)
        {
            writer.WriteLine("Locking hashmap");
            var map = new LockingHashMap<int, int>();
            TestMapConcurrent(map, nrThreads, writer);
        }


        private static void TestMapConcurrent(IMap<int, int> map, int nrThreads, StreamWriter writer)
        {


            var threads = new List<Thread>();
            var results = new List<ResultHolder>();

            for (int i = 0; i < nrThreads; i++)
            {
                var from = 0 + (i * 1000);
                var to = 1000 + (i * 1000);
                var thread = new Thread(() =>
                {
                    var res = ExecuteTest(map, from, to);

                    lock (results)
                    {
                        results.Add(res);
                    }
                });

                threads.Add(thread);


            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            var result = new ResultHolder();
            foreach (var res in results)
            {
                result = result.Merge(res);
            }

            writer.WriteLine("Count: " + map.Count);
            writer.WriteLine("Add milisecs: " + result.AddTime);
            writer.WriteLine("Get milisecs: " + result.GetTime);
            writer.WriteLine("Foreach: " + result.ForeachTime);
            writer.WriteLine("Remove milisecs: " + result.RemoveTime);
            writer.WriteLine("Time: "+result.TotalTime);
            writer.WriteLine();

        }

        private static ResultHolder ExecuteTest(IMap<int,int> map, int from, int to)
        {
            var totalSW = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();
            MapAdd(map, from, to);
            sw.Stop();
            var addTime = sw.ElapsedMilliseconds;
            
            //MapAddIfAbsent(map, from, to);

            sw.Restart();
            MapGet(map, from, to);
            sw.Stop();
            var getTime = sw.ElapsedMilliseconds;

            sw.Restart();
            MapForeach(map);
            sw.Stop();
            var foreachTime = sw.ElapsedMilliseconds;

            sw.Restart();
            MapRemove(map, from, to);
            sw.Stop();
            var removeTime = sw.ElapsedMilliseconds;

            totalSW.Stop();

            var totalTime = totalSW.ElapsedMilliseconds;

            return new ResultHolder() { AddTime = addTime, GetTime = getTime, ForeachTime = foreachTime, RemoveTime = removeTime, TotalTime = totalTime };
        }

        private class ResultHolder
        {
            public long AddTime { get; set; }
            public long GetTime { get; set; }
            public long ForeachTime { get; set; }
            public long RemoveTime { get; set; }
            public long TotalTime { get; set; }


            public ResultHolder Merge(ResultHolder other)
            {
                return new ResultHolder() 
                { 
                    AddTime = this.AddTime+other.AddTime,
                    GetTime = this.GetTime+other.GetTime, 
                    ForeachTime = this.ForeachTime+other.ForeachTime, 
                    RemoveTime = this.RemoveTime+other.RemoveTime, 
                    TotalTime = this.TotalTime+other.TotalTime 
                };
            }
        }

        public static void MapAdd(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                map[i] = i;
            }
        }

        public static void MapAddIfAbsent(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                map.AddIfAbsent(i, i);
            }
        }

        public static void MapRemove(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                map.Remove(i);
            }
        }

        public static void MapGet(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                if (i != map.Get(i))
                {
                    Console.WriteLine("Different: " + i);
                }
            }
        }


        public static void MapForeach(IMap<int, int> map)
        {
            foreach (var kvPair in map)
            {
                if (kvPair.Key != kvPair.Value)
                {
                    Console.WriteLine("Different: " + kvPair.Key);
                }
            }
        }

        private static void HashMapTest()
        {
            MapTest(new HashMap<int, int>());
        }

        private static void MapTest(IMap<int, int> map)
        {
            for (var i = -50; i < 50; i++)
            {
                map.Add(i, i);
            }
            Console.WriteLine(map.Count);

            for (var i = -50; i < 50; i++)
            {
                map.AddIfAbsent(i, i);
            }

            Console.WriteLine(map.Count);

            for (var i = -50; i < 50; i++)
            {
                if (map.Get(i) != i)
                {
                    Console.WriteLine("Error on key: " + i);
                }
            }

            foreach (var kvPair in map)
            {
                if (kvPair.Key != kvPair.Value)
                {
                    Console.WriteLine("Error on key: " + kvPair.Key);
                }
            }

            for (var i = -50; i < 50; i++)
            {
                map.Remove(i);
            }
            Console.WriteLine(map.Count);

        }

        private static void STMHashMapSequentialTest()
        {
            MapTest(new StmHashMap<int, int>());
        }

        private static void LockingHashMapSequentialTest()
        {
            MapTest(new LockingHashMap<int, int>());
        }
    }
}
