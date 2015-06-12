using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PerformanceTestModel;

namespace PerformanceTest
{
    public abstract class BaseQueueTester : ITestable
    {
        protected List<Thread> _producers;
        protected List<Thread> _consumers;
        protected int _nrConsumers;
        protected int _nrProducers;
        protected int _nrItems;
        protected readonly int _itemsToConsume;
        protected int _count = 0;

        public BaseQueueTester(int nrItems, int nrConsumers, int nrProducers)
        {
            _producers = new List<Thread>();
            _consumers = new List<Thread>();
            _nrProducers = nrProducers;
            _nrConsumers = nrConsumers;
            _nrItems = nrItems;
            _itemsToConsume = _nrItems * _nrConsumers;
        }

        protected void IncrementCount()
        {
            Interlocked.Increment(ref _count);
        }

        public void Setup()
        {
            _consumers.Clear();
            _producers.Clear();
            for (int i = 0; i < _nrConsumers; i++)
            {
                _consumers.Add(new Thread(CreateConsumer(i)));
            }

            for (int i = 0; i < _nrProducers; i++)
            {
                _producers.Add(new Thread(CreateProducer(i)));
            }
        }

        protected abstract ThreadStart CreateConsumer(int index);
        protected abstract ThreadStart CreateProducer(int index);

        public double Perform()
        {
            foreach (var producer in _producers)
            {
                producer.Start();
            }

            foreach (var consumer in _consumers)
            {
                consumer.Start();
            }

            foreach (var producer in _producers)
            {
                producer.Join();
            }

            foreach (var consumer in _consumers)
            {
                consumer.Join();
            }

            return 0;
        }
    }
}
