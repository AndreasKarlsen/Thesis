using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;

namespace STMUnitTest
{
    [TestClass]
    public class LockTest
    {
        [TestMethod]
        public void AtomicLockTest()
        {
            var alock = new AtomicLock();
            var t1 = new Thread(() =>
            {
                alock.Lock();
                Thread.Sleep(100);
                Console.WriteLine("T1 has the lock");
                alock.UnLock();
            });

            var t2 = new Thread(() =>
            {
                alock.Lock();
                Thread.Sleep(100);
                Console.WriteLine("T2 has the lock");
                alock.UnLock();
            });

            var t3 = new Thread(() =>
            {
                alock.Lock();
                Thread.Sleep(100);
                Console.WriteLine("T3 has the lock");
                alock.UnLock();
            });

            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();

            Assert.IsFalse(alock.IsLocked);
        }
    }
}
