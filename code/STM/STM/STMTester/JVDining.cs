using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;
using STM.Implementation.JVSTM;

namespace STMTester
{
    public class JVDining
    {
        private static readonly int MAX_EAT_COUNT = 10000;
        private static readonly VBox<int> EatCounter = new VBox<int>(0);

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
                while (JVSTMSystem.Atomic((t) => EatCounter.Read(t) < MAX_EAT_COUNT))
                {
                    JVSTMSystem.Atomic((t) =>
                    {
                        left.AttemptToPickUp(t);
                        right.AttemptToPickUp(t);
                    });

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    var count = JVSTMSystem.Atomic((t) =>
                    {
                        EatCounter.Put(t,EatCounter.Read(t)+1);
                        return EatCounter.Read(t);
                    });

                    Console.WriteLine("Eat count: " + count);

                    JVSTMSystem.Atomic((t) =>
                    {
                        left.State.Put(t,true);
                        right.State.Put(t,true);
                    });

                    Thread.Sleep(100);
                }
            });

            t1.Start();

            return t1;
        }

        public class Fork
        {
            public VBox<bool> State { get; set; }

            public Fork()
            {
                State = new VBox<bool>(true);
            }

            public void AttemptToPickUp(JVTransaction t)
            {

                if (!State.Read(t))
                {
                    JVSTMSystem.Retry();
                }
                State.Put(t, false);

            }
        }
    }
}
