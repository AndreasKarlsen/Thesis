using System;
using System.Threading;
using Evaluation.Library;
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

            Assert.AreEqual(0, map.Size);

            MapAdd(map, from, to);
            Assert.AreEqual(100, map.Size);

            MapAddIfAbsent(map, from, to);
            Assert.AreEqual(100, map.Size);

            MapGet(map,from,to);

            MapRemove(map, from,to);
            Assert.AreEqual(0, map.Size);
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

        [TestMethod]
        public void STMHashMapConcurrent()
        {
            const int t1From = 0;
            const int t1To = 1000;
            const int t2From = -1000;
            const int t2To = 0;
            const int expectedSize = 2000;
            var map = new StmHashMap<int, int>();

            var t1 = new Thread(() => MapAdd(map, t1From, t1To));
            var t2 = new Thread(() => MapAdd(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.AreEqual(expectedSize, map.Size);

            t1 = new Thread(() => MapAddIfAbsent(map, t1From, t1To));
            t2 = new Thread(() => MapAddIfAbsent(map, t2From, t2To));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
            Assert.AreEqual(expectedSize, map.Size);

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

            Assert.AreEqual(0, map.Size);
        }
    }
}
