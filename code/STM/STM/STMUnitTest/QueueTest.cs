using System;
using STM.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace STMUnitTest
{
    [TestClass]
    public class QueueTest
    {
        private Queue<int> _queue;
        [TestInitialize]
        public void Setup()
        {
            _queue = new Queue<int>();
        }

        [TestMethod]
        public void TestCount()
        {
            Assert.AreEqual(0, _queue.Count);
            this.Enqueue(10);
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
        public void TestThreadedEnqueuing()
        {
            var thread1 = new Thread(() => Enqueue(100000));
            var thread2 = new Thread(() => Enqueue(100000));

            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

            Assert.AreEqual(200000, _queue.Count);
        }
    }
}
