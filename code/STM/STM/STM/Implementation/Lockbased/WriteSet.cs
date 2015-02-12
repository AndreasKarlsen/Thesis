using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Exceptions;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public class WriteSet
    {
        public readonly Dictionary<BaseLockObject, object> Map; 

        private static readonly ThreadLocal<WriteSet> Locals
            = new ThreadLocal<WriteSet>(() => new WriteSet());


        public WriteSet()
        {
            Map = new Dictionary<BaseLockObject, object>();
        }

        public static WriteSet GetLocal()
        {
            return Locals.Value;
        }

        public bool Contains(BaseLockObject stmObject)
        {
            return Map.ContainsKey(stmObject);
        }

        public object Get(BaseLockObject stmObject)
        {
            return Map[stmObject];
        }

        public void Put(BaseLockObject stmObject, object value)
        {
            Map[stmObject] = value;
        }

        public void Clear()
        {
            Map.Clear();
        }

        public bool TryLock(int milisecs)
        {
            var objects = new List<BaseLockObject>(Map.Count);
            foreach (var lo in Map.Keys)
            {
                if (!lo.TryLock(milisecs))
                {
                    foreach (var baseLockObject in objects)
                    {
                        baseLockObject.Unlock();
                    }
                    throw new STMException("Abort due to being unable to aquire locks on all objects");
                }

                objects.Add(lo);
            }

            return true;
        }

        public void Unlock()
        {
            foreach (var lo in Map.Keys)
            {
                lo.Unlock();
            }
        }
    }
}
