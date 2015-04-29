using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;

namespace Evaluation.Library
{
    public class DiningPhilosophers
    {
        private const int MAX_EAT_COUNT = 1000;

        public static void Start()
        {
            var eatCounter = new TMInt(0);
            var fork1 = new TMVar<bool>(true);
            var fork2 = new TMVar<bool>(true);
            var fork3 = new TMVar<bool>(true);
            var fork4 = new TMVar<bool>(true);
            var fork5 = new TMVar<bool>(true);

            var t1 = StartPhilosopher(eatCounter, fork1, fork2);
            var t2 = StartPhilosopher(eatCounter, fork2, fork3);
            var t3 = StartPhilosopher(eatCounter, fork3, fork4);
            var t4 = StartPhilosopher(eatCounter, fork4, fork5);
            var t5 = StartPhilosopher(eatCounter, fork5, fork1);

            Task.WaitAll(t1, t2, t3, t4, t5);
        }

        private static Task StartPhilosopher(TMInt eatCounter, TMVar<bool> left, TMVar<bool> right)
        {
            var t1 = new Task(() =>
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

            t1.Start();

            return t1;
        }

    }
}
