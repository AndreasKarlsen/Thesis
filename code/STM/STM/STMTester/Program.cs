using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM;
using STM.Collections;
using STM.Interfaces;
using STM.Implementation.Obstructionfree;
using STM.Implementation.Lockbased;
using System.Diagnostics;

namespace STMTester
{
    class Program
    {
        private static readonly int MAX_EAT_COUNT = 1000;

        static void Main(string[] args)
        {
            //Test1();
            //Test2();
            //Test3();
            //Test4();
            //TestRetry();
            //TestRetry2();
            //SingleItemBufferTest();
            //QueueTest();
            DinningPhilosophersTest();
            Console.ReadKey();
        }

        private static void DinningPhilosophersTest()
        {
            var eatCounter = new LockCounter();
            var fork1 = new RefLockObject<bool>(true);
            var fork2 = new RefLockObject<bool>(true);
            var fork3 = new RefLockObject<bool>(true);
            var fork4 = new RefLockObject<bool>(true);
            var fork5 = new RefLockObject<bool>(true);

            var t1 = StartPhilosopher(eatCounter, fork1, fork2);
            var t2 = StartPhilosopher(eatCounter, fork2, fork3);
            var t3 = StartPhilosopher(eatCounter, fork3, fork4);
            var t4 = StartPhilosopher(eatCounter, fork4, fork5);
            var t5 = StartPhilosopher(eatCounter, fork5, fork1);

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();
            t5.Join();
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
        private static Thread StartPhilosopher(LockCounter eatCounter, RefLockObject<bool> left, RefLockObject<bool> right)
        {
            var t1 = new Thread(() =>
            {
                while (eatCounter.Get() < MAX_EAT_COUNT)
                {
                    LockSTMSystem.Atomic(() =>
                    {
                        if (!left|| !right)
                        {
                            LockSTMSystem.Retry();
                        }

                        left.Value = false;
                        right.Value = false;
                    });

                    Console.WriteLine("Thread: " + Thread.CurrentThread.ManagedThreadId + " eating.");
                    Thread.Sleep(100);
                    Console.WriteLine("Eat count: "+eatCounter.IncrementAndGet()); 


                    LockSTMSystem.Atomic(() =>
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

        private static void QueueTest()
        {
            Console.WriteLine("QueueTest:");
            var buffer = new STM.Collections.Queue<int>();
            var t1 = new Thread(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    Console.WriteLine(buffer.Dequeue());
                }
            });

            var t2 = new Thread(() => LockSTMSystem.Atomic(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    buffer.Enqueue(i);
                }
            }));

            t1.Start();
            t2.Start();

            t1.Join();
            t1.Join();
        }

        private static void SingleItemBufferTest()
        {
            Console.WriteLine("SingleItemBufferTest:");
            var buffer = new SingleItemBuffer<int>();
            var t1 = new Thread(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    Console.WriteLine(buffer.GetValue());
                }
            });

            var t2 = new Thread(() => LockSTMSystem.Atomic(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    buffer.SetValue(i);
                }
            }));

            t1.Start();
            t2.Start();

            t1.Join();
            t1.Join();
        }

        private static void TestRetry2()
        {
            Console.WriteLine("Retry as only operation:");
            var t1 = new Thread(() =>
            {
                LockSTMSystem.Atomic(() =>
                {
                    LockSTMSystem.Retry();
                });
            });

            t1.Start();
            t1.Join();
        }

        private static void TestRetry()
        {
            Console.WriteLine("Retry block:");
            var result = new RefLockObject<int>(10);
            var t1 = new Thread(() =>
            {
                var r1 = LockSTMSystem.Atomic(() =>
                {
                    var tmp = result.Value;
                    if (tmp != 12)
                    {
                        LockSTMSystem.Retry();
                    }
                    result.Value = tmp*10;
                    return result.Value;
                });
            });

            var t2 = new Thread(() => LockSTMSystem.Atomic(() =>
            {
                Thread.Sleep(100);
                result.Value = 12;
            }));

            t1.Start();
            t2.Start();

            t1.Join();
            t1.Join();

        }

        private static void Test4()
        {
            for (int i = 0  ; i < 10000; i++)
            {
                var result = Test4Internal();
                Console.WriteLine("Result: " + result);
                Debug.Assert(result != 120);
            }
        }

        private static int Test4Internal()
        {
            var result = new RefLockObject<ValueHolder>(new ValueHolder(10));

            var t1 = new Thread(() =>
            {
                var r1 = LockSTMSystem.Atomic(() =>
                {
                    if (result.GetValue().Value == 10)
                    {
                        Thread.Yield();
                        result.SetValue(new ValueHolder(result.GetValue().Value * 10));
                    }

                    return result.GetValue();
                });
                Debug.Assert(r1.Value != 120, "String value: "+r1.Value);
            });

            var t2 = new Thread(() => LockSTMSystem.Atomic(() => result.SetValue(new ValueHolder(12))));

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            return result.GetValue().Value;
        }

        private static void Test3()
        {
            STMObject<ValueHolder> result = new FreeObject<ValueHolder>(new ValueHolder(10));
            var system = FreeSTMSystem.GeInstance();

            var t1 = new Thread(() =>
            {
                var r1 = system.Atomic(() =>
                {
                    if (result.GetValue().Value == 10)
                    {
                        Thread.Yield();
                        result.SetValue(new ValueHolder(0));
                    }
                    else
                    {
                        result.SetValue(new ValueHolder(1));
                    }
                    return result.GetValue();
                });
                Console.WriteLine("Result: "+r1);
            });

            var t2 = new Thread(() => system.Atomic(() => result.SetValue(new ValueHolder(12))));

            t1.Start();
            t2.Start();
            
            t1.Join();
            t2.Join();
        }

        private static void Test2()
        {
            var system = FreeSTMSystem.GeInstance();
            STMObject<ValueHolder> fo = new FreeObject<ValueHolder>(new ValueHolder(5));
            var result = system.Atomic(() =>
            {
                fo.SetValue(new ValueHolder(12));
                return fo.GetValue();
            });

        }

        private static void Test1()
        {
            var system = FreeSTMSystem.GeInstance();
            STMObject<ValueHolder> fo = new FreeObject<ValueHolder>(new ValueHolder(15));

            var t1 = new Thread(() =>
            {
                var result = system.Atomic(() =>
                {
                    fo.SetValue(new ValueHolder(1));
                    return fo.GetValue();
                });
            });

            var t2 = new Thread(() =>
            {
                var result = system.Atomic(() =>
                {
                    fo.SetValue(new ValueHolder(2));
                    return fo.GetValue();
                });
            });

            var t3 = new Thread(() =>
            {
                var result = system.Atomic(() =>
                {
                    fo.SetValue(new ValueHolder(3));
                    return fo.GetValue();
                });
            });

            var t4 = new Thread(() =>
            {
                var result = system.Atomic(() =>
                {
                    fo.SetValue(new ValueHolder(4));
                    return fo.GetValue();
                });
            });

            var t5 = new Thread(() =>
            {
                var result = system.Atomic(() =>
                {
                    fo.SetValue(new ValueHolder(5));
                    return fo.GetValue();
                });
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

        private class ValueHolder : ICopyable<ValueHolder>
        {
            public int Value { get; set; }

            public ValueHolder()
            {
                Value = default(int);
            }

            public ValueHolder(int value)
            {
                Value = value;
            }

            public void CopyTo(ValueHolder other)
            {
                other.Value = Value;
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}
