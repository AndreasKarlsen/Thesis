using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.JVSTM;

namespace STMUnitTest
{
    [TestClass]
    public class RaceConditionTests
    {
        [TestMethod]
        public void RaceTest1()
        {
            for (var i = 0; i < 10000; i++)
            {
                var result = RaceTest1Internal();
                Assert.IsTrue(result != 120);
            }
        }

        private int RaceTest1Internal()
        {
            var result = new TMVar<int>(10);

            var t1 =  new Task(() =>
            {
                STMSystem.Atomic(() =>
                {
                    if (result.Value == 10)
                    {
                        Thread.Yield();
                        result.SetValue(result.Value * 10);
                    }

                    return result.GetValue();
                });

            });

            var t2 = new Task(() => STMSystem.Atomic(() => 
                {
                    result.Value = 12;
                }));

            t1.Start();
            t2.Start();

            t1.Wait();
            t2.Wait();

            return result.Value;
        }

        [TestMethod]
        public void NonTransactionalWriteTest()
        {
            for (var i = 0; i < 10000; i++)
            {
                var result = NonTransactionalWriteTestInternal();
                Assert.IsTrue(result != 120);
            }
        }

        private int NonTransactionalWriteTestInternal()
        {
            var result = new TMVar<int>(10);

            var t1 = new Thread(() =>
            {
                var r1 = STMSystem.Atomic(() =>
                {
                    if (result.Value == 10)
                    {
                        Thread.Yield();
                        result.SetValue(result.Value * 10);
                    }

                    return result.GetValue();
                });

            });

            var t2 = new Thread(() => result.Value = 12);

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            return result.Value;
        }

        [TestMethod]
        public void JVRaceTest1()
        {/*
            for (int i = 0; i < 10000; i++)
            {
                var result = JVRaceTest1Internal();
                Assert.IsTrue(result != 120);
            }*/
        }

        private int JVRaceTest1Internal()
        {
            var result = new VBox<int>(10);

            var t1 = new Task(() =>
            {
                JVSTMSystem.Atomic((transaction) =>
                {
                    if (result.Read(transaction) == 10)
                    {
                        Thread.Yield();
                        result.Put(transaction, result.Read(transaction) * 10);
                    }

                    return result.Read(transaction);
                });

            });

            var t2 = new Task(() => JVSTMSystem.Atomic((transaction) =>
            {
                result.Put(transaction,12);
            }));

            t1.Start();
            t2.Start();

            t1.Wait();
            t2.Wait();

            var res = JVSTMSystem.Atomic((transaction) => result.Read(transaction));
            return res;
        }
    }


}
