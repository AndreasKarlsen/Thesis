using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;

namespace STM.Implementation.Lockbased
{
    public class VersionClock
    {
        // global clock read and advanced by all
        private static readonly AtomicLong Global = new AtomicLong();
        // thread-local cached copy of global clock
        private static readonly ThreadLocal<long> Local = new ThreadLocal<long>(() => 0L);

        public static void SetReadStamp()
        {
            Local.Value = Global.Value;
        }

        public static long GetReadStamp()
        {
            return Local.Value;
        }

        public static void SetWriteStamp()
        {
            Local.Value = Global.IncrementValueAndReturn();
        }

        public static long GetWriteStamp()
        {
            return Local.Value;
        }
    }
}


