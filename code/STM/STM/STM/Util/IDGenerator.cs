using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;

namespace STM.Util
{
    public static class IDGenerator
    {
        private static readonly AtomicLong AtomicID = new AtomicLong(0);

        public static long NextID {
            get { return AtomicID.IncrementValueAndReturn(); }
        }
    }
}
