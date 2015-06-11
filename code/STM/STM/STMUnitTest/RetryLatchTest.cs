using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Common;

namespace STMUnitTest
{
    [TestClass]
    public class RetryLatchTest
    {
        [TestMethod]
        public void RetryLatchTest1()
        {
            var latch1 = new RetryLatch();
            var latch2 = new RetryLatch();
            var t1Done = false;
            var t2Done = false;
            var t1 = new Thread(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    latch1.Await(latch1.Era);
                    latch1.Reset();;
                    latch2.Open(latch2.Era);
                }
                t1Done = true;
            });

            var t2 = new Thread(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    latch1.Open(latch1.Era);
                    latch2.Await(latch2.Era);
                    latch2.Reset();
                }

                t2Done = true;
            }); 

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Assert.IsTrue(t1Done && t2Done);
        }
    }
}
