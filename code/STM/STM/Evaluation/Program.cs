using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            LockingHashMapSequentialTest();
            Console.ReadKey();
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
