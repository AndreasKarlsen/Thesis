using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.JVSTM;
using System.Threading;
using PerformanceTestModel;

namespace Evaluation.Library
{
    public class JVDining : ITestable
    {
        private readonly int MAX_EAT_COUNT;
        private Thread t1;
        private Thread t2;
        private Thread t3;
        private Thread t4;
        private Thread t5;

        public JVDining(int maxEatCount)
        {
            MAX_EAT_COUNT = maxEatCount;
        }

        private static readonly VBox<int> EatCounter = new VBox<int>(0);

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

        private Thread CreatePhilosopher(VBox<int> eatCounter, Fork left, Fork right)
        {
            var t1 = new Thread(() =>
            {
                while (JVSTMSystem.Atomic((t) => eatCounter.Read(t) < MAX_EAT_COUNT))
                {
                    JVSTMSystem.Atomic((t) =>
                    {
                        if (!left.State.Read(t) || !right.State.Read(t))
                        {
                            JVSTMSystem.Retry();
                        }
                        left.State.Put(t, false);
                        right.State.Put(t, false);
                    });

                    Thread.Sleep(100);
                    JVSTMSystem.Atomic((t) =>
                    {
                        eatCounter.Put(t, eatCounter.Read(t) + 1);
                    });

                    JVSTMSystem.Atomic((t) =>
                    {
                        left.State.Put(t, true);
                        right.State.Put(t, true);
                    });

                    Thread.Sleep(100);
                }
            });


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

        public void Setup()
        {
            var eatCounter = new VBox<int>(0);
            var fork1 = new Fork();
            var fork2 = new Fork();
            var fork3 = new Fork();
            var fork4 = new Fork();
            var fork5 = new Fork();

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
