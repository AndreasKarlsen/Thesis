using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            SantaClausProblem.Start();
            //LockingSantaClausProblem.Start();
            Console.ReadKey();
        }
    }
}
