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
        /// <summary>
        /// Global clock read and advanced by all
        /// </summary>
        private static readonly AtomicInteger Global = new AtomicInteger();

        public static int TimeStamp
        {
            get { return Global.Value; }
        }

        public static int IncrementClock()
        {
            return Global.IncrementValueAndReturn();
        }

    }
}


