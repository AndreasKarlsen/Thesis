using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;

namespace STMUnitTest
{
    [TestClass]
    public class RetryTest
    {
        [TestMethod]
        public void TestRetry()
        {
            var result = new TMVar<int>(10);
            var t1 = new Thread(() =>
            {
                var r1 = STMSystem.Atomic(() =>
                {
                    var tmp = result.Value;
                    if (tmp != 12)
                    {
                        STMSystem.Retry();
                    }
                    result.Value = tmp * 10;
                    return result.Value;
                });
            });

            var t2 = new Thread(() => STMSystem.Atomic(() =>
            {
                Thread.Sleep(100);
                result.Value = 12;
            }));

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join(); 
        }
    }
}
