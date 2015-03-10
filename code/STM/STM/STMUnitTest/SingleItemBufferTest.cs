using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Collections;

namespace STMUnitTest
{
    [TestClass]
    public class SingleItemBufferTest
    {
        private STM.Collections.SingleItemBuffer<int> _buffer;

        [TestInitialize]
        public void Setup()
        {
            _buffer = new SingleItemBuffer<int>();
        }

        [TestMethod]
        public void TestThreadedSetValue()
        {
            var t1 = new Thread(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    Console.WriteLine(_buffer.GetValue());
                }
            });

            var t2 = new Thread(() => 
            {
                for (var i = 0; i < 100; i++)
                {
                    _buffer.SetValue(i);
                }
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();
            Assert.AreEqual(false, _buffer.IsFull);
        }
    }
}
