using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.Common
{
    public interface IRetryLatch
    {
        bool IsOpen { get; }

        void Open(int expectedEra);

        int Era { get; }

        void Await(int expectedEra);

        void Reset();
    }
}
