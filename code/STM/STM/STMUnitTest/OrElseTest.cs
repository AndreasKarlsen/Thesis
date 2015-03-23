using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;

namespace STMUnitTest
{
    [TestClass]
    public class OrElseTest
    {
        [TestMethod]
        public void TestOrElse()
        {
            var result = 0;
            var tm1 = new TMVar<bool>(false);
            var tm2 = new TMVar<bool>(false);

            var t1 = new Thread(() =>
            {
                result = STMSystem.Atomic(() =>
                {
                    if (!tm1)
                    {
                        STMSystem.Retry();
                    }

                    return 1;
                },
                    () =>
                    {
                        if (!tm2)
                        {
                            STMSystem.Retry();
                        }

                        return 2;
                    }
                    );
            });


            t1.Start();
            Thread.Sleep(100);
            STMSystem.Atomic(() => tm2.Value = true);
            t1.Join();
            Assert.AreEqual(2, result);
        }
    }
}
