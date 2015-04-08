using System;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;

namespace LanguagedBasedDining
{
    public class DiningPhilosopher
    {
        private static readonly int MAX_EAT_COUNT = 1000;
        private static atomic int eatCounter = 0;

        public static void Main()
        {
            var dinning = new DiningPhilosopher();
            dinning.Start();
        }

        public void Start()
        {
            var fork1 = new Fork();
            var fork2 = new Fork();
            var fork3 = new Fork();
            var fork4 = new Fork();
            var fork5 = new Fork();

            var t1 = StartPhilosopher(fork1, fork2);
            var t2 = StartPhilosopher(fork2, fork3);
            var t3 = StartPhilosopher(fork3, fork4);
            var t4 = StartPhilosopher(fork4, fork5);
            var t5 = StartPhilosopher(fork5, fork1);

            Task.WaitAll(t1, t2, t3, t4, t5);
        }

        private Task StartPhilosopher(Fork left, Fork right)
        {
            var t1 = new Task(() =>
            {
                while (eatCounter < MAX_EAT_COUNT)
                {
                    atomic
                    {
                        left.AttemptToPickUp();
                        right.AttemptToPickUp();
                        /*
                        if (!left.State || !right.State)
                        {
                            retry;
                        }

                        left.State = false;
                        right.State = false;
                        */
                    }

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: " + ++eatCounter);

                    atomic
                    {
                        left.State = true;
                        right.State = true;
                    }

                    Thread.Sleep(100);
                }
            });

            t1.Start();

            return t1;
        }

        public class Fork
        {
            public atomic bool State { get; set; }

            public Fork()
            {
                State = true;
            }

            public void AttemptToPickUp()
            {
                atomic{
                    if (!State)
                    {
                        retry;
                    }
                    State = true;
                }
            }
        }
    }
}