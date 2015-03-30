using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Exceptions;

namespace STM.Implementation.Lockbased 
{
    public class ReadSet : IEnumerable<BaseLockObject>
    {
        private readonly HashSet<BaseLockObject> _lockObjects;

        internal ReadSet()
        {
            _lockObjects = new HashSet<BaseLockObject>();
        }

        public void Add(BaseLockObject blo)
        {
            _lockObjects.Add(blo);
        }

        public void Merge(ReadSet items)
        {
            _lockObjects.UnionWith(items);
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
            return this.All(lo => lo.Validate(transaction));
        }

        #region Locking

        public bool TryLock(int milisecs)
        {
            var objects = new List<BaseLockObject>(_lockObjects.Count);
            foreach (var lo in _lockObjects)
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
            foreach (var lo in _lockObjects)
            {
                lo.Unlock();
            }
        }

#endregion Locking

        #region IEnumerable

        public IEnumerator<BaseLockObject> GetEnumerator()
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
