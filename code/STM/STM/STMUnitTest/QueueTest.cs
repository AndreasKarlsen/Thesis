using System;
using STM.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            for(var i = 0; i < 10; i++)
            {
                _queue.Enqueue(i);
            }
            Assert.AreEqual(10, _queue.Count);
            _queue.Dequeue();
            Assert.AreEqual(9, _queue.Count);
        }

        [TestMethod]
        public void TestEnqueue()
        {
            _queue.Enqueue(1);
            
            Assert.AreEqual(1, _queue.Dequeue());
        }
    }
}
