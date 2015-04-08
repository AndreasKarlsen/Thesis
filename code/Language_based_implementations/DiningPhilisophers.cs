using System;
using System.Threading;
using STM.Implementation.Lockbased;

namespace LanguagedBasedDining
{
    public class DiningPhilosopher
    {
        private static readonly int MAX_EAT_COUNT = 1000;
        private static atomic int eatCounter = 0;
        private static atomic bool fork1 = true;
        private static atomic bool fork2 = true;
        private static atomic bool fork3 = true;
        private static atomic bool fork4 = true;
        private static atomic bool fork5 = true;

        public static void Main()
        {
            var t1 = new Thread(() =>
            {
                while (eatCounter < MAX_EAT_COUNT)
                {
                    atomic
                    {
                        if (!fork1 || !fork2)
                        {
                            retry;
                        }

                        fork1 = false;
                        fork2 = false;
                    }

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: " + ++eatCounter);

                    atomic
                    {
                        fork1 = true;
                        fork2 = true;
                    }

                    Thread.Sleep(100);
                }
            });

            var t2 = new Thread(() =>
            {
                while (eatCounter < MAX_EAT_COUNT)
                {
                    atomic
                    {
                        if (!fork2 || !fork3)
                        {
                            retry;
                        }

                        fork2 = false;
                        fork3 = false;
                    }

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: " + ++eatCounter);

                    atomic
                    {
                        fork2 = true;
                        fork3 = true;
                    }

                    Thread.Sleep(100);
                }
            });

            var t3 = new Thread(() =>
            {
                while (eatCounter < MAX_EAT_COUNT)
                {
                    atomic
                    {
                        if (!fork3 || !fork4)
                        {
                            retry;
                        }

                        fork3 = false;
                        fork4 = false;
                    }

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: " + ++eatCounter);

                    atomic
                    {
                        fork3 = true;
                        fork4 = true;
                    }

                    Thread.Sleep(100);
                }
            });

            var t4 = new Thread(() =>
            {
                while (eatCounter < MAX_EAT_COUNT)
                {
                    atomic
                    {
                        if (!fork4 || !fork5)
                        {
                            retry;
                        }

                        fork4 = false;
                        fork5 = false;
                    }

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: " + ++eatCounter);

                    atomic
                    {
                        fork4 = true;
                        fork5 = true;
                    }

                    Thread.Sleep(100);
                }
            });

            var t5 = new Thread(() =>
            {
                while (eatCounter < MAX_EAT_COUNT)
                {
                    atomic
                    {
                        if (!fork5 || !fork1)
                        {
                            retry;
                        }

                        fork5 = false;
                        fork1 = false;
                    }

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: " + ++eatCounter);

                    atomic
                    {
                        fork5 = true;
                        fork1 = true;
                    }

                    Thread.Sleep(100);
                }
            });

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
    }
}