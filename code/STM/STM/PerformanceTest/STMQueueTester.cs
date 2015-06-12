using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Evaluation.Common;

namespace PerformanceTest
{
    public class STMQueueTester : BaseQueueTester
    {
        private ISTMQueue<int> _queue;

        public STMQueueTester(ISTMQueue<int> queue, int nrItems, int nrConsumers, int nrProducers)
            : base(nrItems, nrConsumers, nrProducers)
        {
            _queue = queue;
        }

        protected override ThreadStart CreateConsumer(int index)
        {
            return new ThreadStart(() =>
            {
                while (_count > _itemsToConsume)
                {
                    _queue.Dequeue();
                    IncrementCount();
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
