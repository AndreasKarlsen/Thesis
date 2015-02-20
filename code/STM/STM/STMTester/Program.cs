using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM;
using STM.Interfaces;
using STM.Implementation.Obstructionfree;
using STM.Implementation.Lockbased;
using System.Diagnostics;

namespace STMTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test1();
            //Test2();
            //Test3();
            //Test4();
            TestRetry();
            Console.ReadKey();
        }

        private static void TestRetry()
        {
            RefLockObject<ValueHolder> result = new RefLockObject<ValueHolder>(new ValueHolder(10));
            var system = LockSTMSystem.GetInstance();
            var t1 = new Thread(() =>
            {
                var r1 = system.Atomic(() =>
                {
                    var tmp = result.GetValue().Value;
                    if (tmp != 12)
                    {
                        system.Retry();
                    }
                    result.SetValue(new ValueHolder(tmp * 10));
                    return result.GetValue();
                });
            });

            var t2 = new Thread(() => system.Atomic(() => {
                Thread.Sleep(100); 
                result.SetValue(new ValueHolder(12));
            }));

            t1.Start();
            t2.Start();

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
            RefLockObject<ValueHolder> result = new RefLockObject<ValueHolder>(new ValueHolder(10));
            var system = LockSTMSystem.GetInstance();

            var t1 = new Thread(() =>
            {
                var r1 = system.Atomic(() =>
                {
                    if (result.GetValue().Value == 10)
                    {
                        result.SetValue(new ValueHolder(result.GetValue().Value * 10));
                    }

                    return result.GetValue();
                });
                Debug.Assert(r1.Value != 120, "String value: "+r1.Value);
            });

            var t2 = new Thread(() => system.Atomic(() => result.SetValue(new ValueHolder(12))));

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
