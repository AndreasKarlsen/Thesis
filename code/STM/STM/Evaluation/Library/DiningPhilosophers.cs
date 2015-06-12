using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;
using PerformanceTestModel;

namespace Evaluation.Library
{
    public class DiningPhilosophers : ITestable
    {
        private readonly int MAX_EAT_COUNT;
        private Thread t1;
        private Thread t2;
        private Thread t3;
        private Thread t4;
        private Thread t5;


        public DiningPhilosophers(int maxEats)
        {
            MAX_EAT_COUNT = maxEats;
        }

        public void Start()
        {
            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            t5.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();
            t5.Join();
        }

        private Thread CreatePhilosopher(TMInt eatCounter, TMVar<bool> left, TMVar<bool> right)
        {
            var t1 = new Thread(() =>
            {
                while (eatCounter < MAX_EAT_COUNT)
                {
                    STMSystem.Atomic(() =>
                    {
                        if (!left || !right)
                        {
                            STMSystem.Retry();
                        }

                        left.Value = false;
                        right.Value = false;
                    });

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: " + ++eatCounter);

                    STMSystem.Atomic(() =>
                    {
                        left.Value = true;
                        right.Value = true;
                    });

                    Thread.Sleep(100);
                }
            });

            return t1;
        }

        
        public void Setup()
        {
 	        var eatCounter = new TMInt(0);
            var fork1 = new TMVar<bool>(true);
            var fork2 = new TMVar<bool>(true);
            var fork3 = new TMVar<bool>(true);
            var fork4 = new TMVar<bool>(true);
            var fork5 = new TMVar<bool>(true);

            t1 = CreatePhilosopher(eatCounter, fork1, fork2);
            t2 = CreatePhilosopher(eatCounter, fork2, fork3);
            t3 = CreatePhilosopher(eatCounter, fork3, fork4);
            t4 = CreatePhilosopher(eatCounter, fork4, fork5);
            t5 = CreatePhilosopher(eatCounter, fork5, fork1);
        }

    
        public double Perform()
        {
            Start();
 	        return 0;
        }
    }
}
