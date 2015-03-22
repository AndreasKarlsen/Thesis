using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;
using System.Threading;
using System.Threading.Tasks;

namespace STMUnitTest
{
    [TestClass]
    public class RaceConditionTests
    {
        [TestMethod]
        public void RaceTest1()
        {
            for (int i = 0; i < 10000; i++)
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

            var t2 = new Task(() => STMSystem.Atomic(() => result.Value = 12));

            t1.Start();
            t2.Start();

            t1.Wait();
            t2.Wait();

            return result.Value;
        }

        [TestMethod]
        public void NonTransactionalWriteTest()
        {
            for (int i = 0; i < 10000; i++)
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
    }


}
