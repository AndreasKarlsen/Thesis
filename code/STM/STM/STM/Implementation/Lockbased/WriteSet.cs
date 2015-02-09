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
        private readonly Dictionary<BaseLockObject, object> _map; 

        private static readonly ThreadLocal<WriteSet> Locals
            = new ThreadLocal<WriteSet>(() => new WriteSet());


        public WriteSet()
        {
            _map = new Dictionary<BaseLockObject, object>();
        }

        public static WriteSet GetLocal()
        {
            return Locals.Value;
        }

        public bool Contains(BaseLockObject stmObject)
        {
            return _map.ContainsKey(stmObject);
        }

        public object Get(BaseLockObject stmObject)
        {
            return _map[stmObject];
        }

        public void Put(BaseLockObject stmObject, object value)
        {
            _map[stmObject] = value;
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool TryLock(int milisecs)
        {
            var objects = new List<BaseLockObject>(_map.Count);
            foreach (var lo in _map.Keys)
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
            foreach (var lo in _map.Keys)
            {
                lo.Unlock();
            }
        }
    }
}
