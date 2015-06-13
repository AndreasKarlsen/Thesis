using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.JVSTM;

namespace STMUnitTest
{
    [TestClass]
    public class JVSTMTest
    {
        [TestMethod]
        public void GarbageCollection()
        {
            JVSTMSystem.StartGC();
            var box1 = new VBox<int>(0);
            for (var i = 0; i < 100; i++)
            {
                JVSTMSystem.Atomic(t =>
                {
                    box1.Put(t, i);
                });
            }

            var box2 = new VBox<int>(0);
            for (var i = 0; i < 100; i++)
            {
                JVSTMSystem.Atomic(t =>
                {
                    box2.Put(t, i);
                });
            }


            Thread.Sleep(10);
            Assert.AreEqual(1, box1.GetNrBodies());
            Assert.AreEqual(1, box2.GetNrBodies());
        }

        [TestMethod]
        public void GarbageCollectionConcurrent()
        {
            var box1 = new VBox<int>(0);
            var t1 = new Thread(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    JVSTMSystem.Atomic(t =>
                    {
                        box1.Put(t, i);
                    });
                }
            });

            var t2 = new Thread(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    JVSTMSystem.Atomic(t =>
                    {
                        box1.Put(t, i);
                    });
                }
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            Assert.AreEqual(1, box1.GetNrBodies());
        }
    }
}
