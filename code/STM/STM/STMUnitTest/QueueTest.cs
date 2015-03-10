using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace STMUnitTest
{
    [TestClass]
    public class QueueTest
    {
        private STM.Collections.Queue<int> _queue;
        [TestInitialize]
        public void Setup()
        {
            _queue = new STM.Collections.Queue<int>();
        }

        [TestMethod]
        public void TestCount()
        {
            Assert.AreEqual(0, _queue.Count);
            Enqueue(10);
            Assert.AreEqual(10, _queue.Count);
            _queue.Dequeue();
            Assert.AreEqual(9, _queue.Count);
        }

        private void Enqueue(int amount)
        {
            for (var i = 0; i < amount; i++)
            {
                _queue.Enqueue(i);
            }
        }

        [TestMethod]
        public void TestEnqueue()
        {
            _queue.Enqueue(1);
            
            Assert.AreEqual(1, _queue.Dequeue());
        }

        [TestMethod]
        public void Test2ThreadsEnqueuing()
        {
            var thread1 = new Thread(() => Enqueue(10000));
            var thread2 = new Thread(() => Enqueue(10000));
            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

            Assert.AreEqual(20000, _queue.Count);
        }

        [TestMethod]
        public void Test10ThreadsEnqueuing()
        {
            var threads = new List<Thread>(10);
            for (int i = 0; i < 10; i++)
            {
                var thread = new Thread(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        _queue.Enqueue(j);
                        _queue.Dequeue();
                    }
                });
                threads.Add(thread);
            }
            threads.ForEach(thread => thread.Start());
            threads.ForEach(thread => thread.Join());
            Assert.AreEqual(0, _queue.Count);
        }
    }
}
