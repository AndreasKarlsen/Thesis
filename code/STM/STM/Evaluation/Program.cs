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
            HashMapTest();
            Console.ReadKey();
        }

        private static void HashMapTest()
        {
            IHashMap<int, int> map = new HashMap<int, int>();

            for (var i = -50; i < 50; i++)
            {
                map.Add(i,i);
            }
            Console.WriteLine(map.Size);

            for (var i = -50; i < 50; i++)
            {
                map.AddIfAbsent(i, i);
            }

            Console.WriteLine(map.Size);

            for (var i = -50; i < 50; i++)
            {
                if (map.Get(i) != i)
                {
                    Console.WriteLine("Error on key: " + i);
                }
            }

            for (var i = -50; i < 50; i++)
            {
                map.Remove(i);
            }
            Console.WriteLine(map.Size);

           
        }
    }
}
