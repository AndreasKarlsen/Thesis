using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Exceptions;

namespace STM.Implementation.Lockbased 
{
    public class ReadSet : IEnumerable<KeyValuePair<BaseLockObject,int>>
    {
        private readonly Dictionary<BaseLockObject,int> _lockObjects;

        internal ReadSet()
        {
            _lockObjects = new Dictionary<BaseLockObject,int>();
        }

        public void Add(BaseLockObject blo, int timeStamp)
        {
            _lockObjects[blo] = timeStamp;
        }

        public void Merge(ReadSet other)
        {
            foreach (var kvpair in other)
            {
                if(!_lockObjects.ContainsKey(kvpair.Key))
                {
                    _lockObjects[kvpair.Key] = kvpair.Value;
                }
            }
        }

        public void Remove(BaseLockObject blo)
        {
            _lockObjects.Remove(blo);
        }

        public void Clear()
        {
            _lockObjects.Clear();
        }

        public int Count {
            get { return _lockObjects.Count; }
        }

        public bool Validate(Transaction transaction)
        {
            return this.All(kvpair => kvpair.Key.Validate(transaction, kvpair.Value));
        }

        public bool Validate(Transaction transaction, int readstamp)
        {
            return this.All(kvpair => kvpair.Key.Validate(transaction, readstamp));
        }

        #region Locking

        public bool TryLock(int milisecs)
        {
            var objects = new List<BaseLockObject>(_lockObjects.Count);
            foreach (var kvpair in _lockObjects)
            {
                if (!kvpair.Key.TryLock(milisecs))
                {
                    foreach (var baseLockObject in objects)
                    {
                        baseLockObject.Unlock();
                    }
                    throw new STMAbortException("Abort due to being unable to aquire locks on all objects");
                }

                objects.Add(kvpair.Key);
            }

            return true;
        }

        public void Unlock()
        {
            foreach (var kvpair in _lockObjects)
            {
                kvpair.Key.Unlock();
            }
        }

#endregion Locking

        #region IEnumerable

        public IEnumerator<KeyValuePair<BaseLockObject,int>> GetEnumerator()
        {
            foreach (var baseLockObject in _lockObjects)
            {
                yield return baseLockObject;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable
    }
}
