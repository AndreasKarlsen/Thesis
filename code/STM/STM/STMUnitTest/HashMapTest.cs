using System;
using System.Threading;
using Evaluation.Library;
using Evaluation.Locking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluation.Common;

namespace STMUnitTest
{
    [TestClass]
    public class HashMapTest
    {
        [TestMethod]
        public void HashMapTestAll()
        {
            TestMap(new HashMap<int, int>());
        }

        private void TestMap(IMap<int, int> map)
        {
            const int from = -50;
            const int to = 50;

            Assert.AreEqual(0, map.Count);

            MapAdd(map, from, to);
            Assert.AreEqual(100, map.Count);

            MapAddIfAbsent(map, from, to);
            Assert.AreEqual(100, map.Count);

            MapGet(map,from,to);

            MapRemove(map, from,to);
            Assert.AreEqual(0, map.Count);
        }

        [TestMethod]
        public void STMHashMapTestAll()
        {
            TestMap(new StmHashMap<int, int>());
        }

        public void MapAdd(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                map[i] = i;
            }
        }

        public void MapAddIfAbsent(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                map.AddIfAbsent(i, i);
            }
        }

        public void MapRemove(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                map.Remove(i);
            }
        }

        public void MapGet(IMap<int, int> map, int from, int to)
        {
            for (var i = from; i < to; i++)
            {
                Assert.AreEqual(i, map.Get(i));
            }
        }


        public void MapForeach(IMap<int, int> map)
        {
            foreach (var kvPair in map)
            {
                Assert.AreEqual(kvPair.Key,kvPair.Value);
            }
        }

        private void TestMapConcurrent(IMap<int, int> map)
        {
            const int t1From = 0;
            const int t1To = 1000;
            const int t2From = -1000;
            const int t2To = 0;
            const int expectedSize = 2000;

            var t1 = new Thread(() => MapAdd(map, t1From, t1To));
            var t2 = new Thread(() => MapAdd(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.AreEqual(expectedSize, map.Count);

            t1 = new Thread(() => MapAddIfAbsent(map, t1From, t1To));
            t2 = new Thread(() => MapAddIfAbsent(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.AreEqual(expectedSize, map.Count);

            t1 = new Thread(() => MapGet(map, t1From, t1To));
            t2 = new Thread(() => MapGet(map, t2From, t2To));
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            t1 = new Thread(() => MapForeach(map));
            t2 = new Thread(() => MapForeach(map));
            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            t1 = new Thread(() => MapRemove(map, t1From, t1To));
            t2 = new Thread(() => MapRemove(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();

            Assert.AreEqual(0, map.Count);
        }

        [TestMethod]
        public void STMHashMapConcurrent()
        {
            var map = new StmHashMap<int, int>();
            TestMapConcurrent(map);
        }

        [TestMethod]
        public void LockingHashMapConcurrent()
        {
            var map = new LockingHashMap<int, int>();
            TestMapConcurrent(map);
        }

        [TestMethod]
        public void NaiveLockingHashMapConcurrent()
        {
            var map = new NaiveLockingHashMap<int, int>();
            TestMapConcurrent(map);
        }
    }
}
