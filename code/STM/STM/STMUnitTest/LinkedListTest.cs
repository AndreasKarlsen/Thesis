using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Collections;
using STM.Implementation.Lockbased;
using System.Threading;

namespace STMUnitTest
{
    [TestClass]
    public class LinkedListTest
    {

        [TestMethod]
        public void SequentialTest()
        {
            LinkedList<int> list = new LinkedList<int>();

            for (int i = 0; i < 1000; i++)
            {
                list.Add(i);
            }

            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual(i, list.FirstWhere(item => i == item));
            }

            for (int i = 0; i < 1000; i++)
            {
                list.Remove(i);
            }

            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void ConcurrentTest()
        {
            LinkedList<int> list = new LinkedList<int>();

            var t1 = new Thread(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    list.Add(i);
                }
            });

            var t2 = new Thread(() =>
            {
                for (int i = 1000; i < 2000; i++)
                {
                    list.Add(i);
                }
            });

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Assert.AreEqual(2000, list.Count);
        }
    }
}
