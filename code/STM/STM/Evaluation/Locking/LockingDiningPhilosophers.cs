using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PerformanceTestModel;

namespace Evaluation.Locking
{
    public class LockingDiningPhilosophers : ITestable
    {
        private readonly int MAX_EAT_COUNT;
        private Thread t1;
        private Thread t2;
        private Thread t3;
        private Thread t4;
        private Thread t5;

        public LockingDiningPhilosophers(int maxEatCount)
        {
            MAX_EAT_COUNT = maxEatCount;
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

        private Thread CreatePhilosopher(LockCounter eatCounter, object left, object right)
        {
            var t1 = new Thread(() =>
            {
                while (eatCounter.Get() < MAX_EAT_COUNT)
                {
                    lock (left)
                    {
                        var lockTaken = false;
                        try
                        {
                            Monitor.TryEnter(right, 100, ref lockTaken);
                            if (lockTaken)
                            {
                                Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                                Thread.Sleep(100);
                                Console.WriteLine("Eat count: " + eatCounter.IncrementAndGet());
                            }
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Monitor.Exit(right);
                            }
                        }
                    }

                    Thread.Sleep(100);
                }
            });


            return t1;
        }

        private class LockCounter
        {
            protected int Count = 0;
            protected readonly object LockObject = new object();

            public int IncrementAndGet()
            {
                int tmp = 0;
                lock (LockObject)
                {
                    tmp = ++Count;
                }

                return tmp;
            }

            public void Increment()
            {
                lock (LockObject)
                {
                    ++Count;
                }
            }

            public int Get()
            {
                int tmp = 0;
                lock (LockObject)
                {
                    tmp = Count;
                }

                return tmp;
            }

        }

        public void Setup()
        {
            var eatCounter = new LockCounter();
            var fork1 = new object();
            var fork2 = new object();
            var fork3 = new object();
            var fork4 = new object();
            var fork5 = new object();

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
