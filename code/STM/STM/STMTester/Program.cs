using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM;
using STM.Interfaces;
using STM.Implementation.Obstructionfree;

namespace STMTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Test1();
            //Test2();
            Test3();
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
