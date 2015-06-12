using System;
using System.Collections.Generic;
using System.Threading;
using Evaluation.Common;
using PerformanceTestModel;

namespace PerformanceTest
{
    internal class HashmapTester : ITestable
    {
        private readonly int _amountOfMappings;
        private readonly int _amountOfOperations;
        private readonly BaseHashMap<int, int> _hashMap;
        private readonly int _nrOfThreads;
        private readonly Random _random;
        private readonly List<Thread> _threads;
        private readonly int _updatePercent;

        public HashmapTester(BaseHashMap<int, int> hashMap, int nrOfThreads, int updatePercent, int amountOfMappings,
            int amountOfOperations)
        {
            _hashMap = hashMap;
            _nrOfThreads = nrOfThreads;
            _updatePercent = updatePercent;
            _amountOfMappings = amountOfMappings;
            _amountOfOperations = amountOfOperations;
            _threads = new List<Thread>();
            _random = new Random();
            FillHashmap(amountOfMappings);
        }

        public void Setup()
        {
            _threads.Clear();
            for (var i = 0; i < _nrOfThreads; i++)
            {
                _threads.Add(new Thread(() =>
                {
                    var random = new Random(Guid.NewGuid().GetHashCode());

                    for (var j = 0; j < _amountOfOperations; j++)
                    {
                        var readOrUpdate = random.Next(0, 100);
                        var keyToOperateOn = random.Next(_amountOfMappings);
                        if (readOrUpdate < _updatePercent)
                        {
                            _hashMap.Add(keyToOperateOn, readOrUpdate);
                        }
                        else
                        {
                            _hashMap.Get(keyToOperateOn);
                        }
                    }
                }));
            }
        }

        public double Perform()
        {
            foreach (var thread in _threads)
            {
                thread.Start();
            }
            foreach (var thread in _threads)
            {
                thread.Join();
            }
            return 0;
        }

        private void FillHashmap(int count)
        {
            for (var i = 0; i < count; i++)
            {
                _hashMap.Add(i, _random.Next());
            }
        }
    }
}