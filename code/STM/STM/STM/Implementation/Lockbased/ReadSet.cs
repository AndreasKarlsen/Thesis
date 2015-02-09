using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STM.Implementation.Lockbased
{
    public class ReadSet
    {
        public readonly HashSet<BaseLockObject> LockObjects;

        private static readonly ThreadLocal<ReadSet> Locals
            = new ThreadLocal<ReadSet>(() => new ReadSet());

        public ReadSet()
        {
            LockObjects = new HashSet<BaseLockObject>();
        }

        public static ReadSet GetLocal()
        {
            return Locals.Value;
        }

        public void Add(BaseLockObject blo)
        {
            LockObjects.Add(blo);
        }

        public void Remove(BaseLockObject blo)
        {
            LockObjects.Remove(blo);
        }

        public void Clear()
        {
            LockObjects.Clear();
        }

    }
}
