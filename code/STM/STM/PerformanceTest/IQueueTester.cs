using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Evaluation.Common;

namespace PerformanceTest
{
    public class IQueueTester : BaseQueueTester
    {
        private IQueue<int> _queue;

        public IQueueTester(IQueue<int> queue, int nrItems, int nrConsumers, int nrProducers) : base(nrItems, nrConsumers,nrProducers)
        {
            _queue = queue;
        }

        protected override ThreadStart CreateConsumer(int index)
        {
            return new ThreadStart(() =>
            {
                while (_count < _itemsToConsume)
                {
                    int res;
                    if (_queue.Dequeue(out res))
                    {
                        IncrementCount();
                    }
                }
            });
        }

        protected override ThreadStart CreateProducer(int index)
        {
            return new ThreadStart(() =>
            {
                for (int i = 0; i < _nrItems; i++)
                {
                    _queue.Enqueue(i);
                }
            });
        }
    }
}
