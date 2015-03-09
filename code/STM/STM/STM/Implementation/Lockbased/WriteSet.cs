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
    public class WriteSet : IEnumerable<KeyValuePair<BaseLockObject,object>>
    {
        private readonly Dictionary<BaseLockObject, object> _map; 

        internal WriteSet()
        {
            _map = new Dictionary<BaseLockObject, object>();
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
                    throw new STMAbortException("Abort due to being unable to aquire locks on all objects");
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

        public IEnumerator<KeyValuePair<BaseLockObject, object>> GetEnumerator()
        {
            foreach (var item in _map)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object this[BaseLockObject key]
        {
            get { return _map[key]; }
            set { _map[key] = value; }
        }

        public void Merge(WriteSet other)
        {
            foreach (var kvpair in other)
            {
                _map[kvpair.Key] = kvpair.Value;
            }
        }
    }
}
