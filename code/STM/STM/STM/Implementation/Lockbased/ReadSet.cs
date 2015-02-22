using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Exceptions;

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

        public int Count()
        {
            return LockObjects.Count;
        }

        public bool TryLock(int milisecs)
        {
            var objects = new List<BaseLockObject>(LockObjects.Count);
            foreach (var lo in LockObjects)
            {
                if (!lo.TryLock(milisecs))
                {
                    foreach (var baseLockObject in objects)
                    {
                        baseLockObject.Unlock();
                    }
                    throw new STMAbortException("Abort due to being unable to aquire locks on all objects");
                }

                objects.Add(lo);
            }

            return true;
        }

        public void Unlock()
        {
            foreach (var lo in LockObjects)
            {
                lo.Unlock();
            }
        }
    }
}
