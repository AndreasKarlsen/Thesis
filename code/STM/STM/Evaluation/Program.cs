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

            for (int i = 0; i < 10; i++)
            {
                LockingHashMapConcurrent();
            }

            for (int i = 0; i < 10; i++)
            {
                STMHashMapConcurrent();
            }

            for (int i = 0; i < 10; i++)
            {
                STMHashMapRetryConcurrent();
            }

            Console.ReadKey();
        }

        public static void STMHashMapConcurrent()
        {
            Console.WriteLine("STM hashmap");
            var sw = Stopwatch.StartNew();
            var map = new StmHashMap<int, int>();
            TestMapConcurrent(map);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        public static void STMHashMapRetryConcurrent()
        {
            Console.WriteLine("STM hashmap retry");
            var sw = Stopwatch.StartNew();
            var map = new StmHashMapRetry<int, int>();
            TestMapConcurrent(map);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        public static void LockingHashMapConcurrent()
        {
            Console.WriteLine("Locking hashmap");
            var sw = Stopwatch.StartNew();
            var map = new LockingHashMap<int, int>();
            TestMapConcurrent(map);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }


        private static void TestMapConcurrent(IMap<int, int> map)
        {
            const int t1From = 0;
            const int t1To = 1000;
            const int t2From = -1000;
            const int t2To = 0;
            const int expectedSize = 2000;

            var t1 = new Thread(() => MapAdd(map, t1From, t1To));
            var t2 = new Thread(() => MapAdd(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Console.WriteLine("Count: " + map.Count);
            
            t1 = new Thread(() => MapAddIfAbsent(map, t1From, t1To));
            t2 = new Thread(() => MapAddIfAbsent(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            
            
            var sw = Stopwatch.StartNew();
            t1 = new Thread(() => MapGet(map, t1From, t1To));
            t2 = new Thread(() => MapGet(map, t2From, t2To));
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            sw.Stop();
            Console.WriteLine("Get milisecs: "+sw.ElapsedMilliseconds);

            t1 = new Thread(() => MapForeach(map));
            t2 = new Thread(() => MapForeach(map));
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            t1 = new Thread(() => MapRemove(map, t1From, t1To));
            t2 = new Thread(() => MapRemove(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Console.WriteLine("Count: " + map.Count);
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
