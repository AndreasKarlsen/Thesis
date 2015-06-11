using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class AtomicInteger
    {
        private int _value = 0;

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref _value);
        }
    }
}
