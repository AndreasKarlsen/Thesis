using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluation.Common
{
    public interface ISTMQueue<T>
    {
        void Enqueue(T item);
        T Dequeue();
    }
}
