using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STM.Implementation.Lockbased
{
    public abstract class BaseLockObject
    {
        protected readonly object StampLock  = new object();
        protected readonly ReentrantLock ReentrantLock = new ReentrantLock();

        private long _stamp;

        public long GetStamp()
        {
            long tmp;
            lock (StampLock)
            {
                tmp = _stamp;
            }

            return tmp;
        }

        public void SetStamp(long newStamp)
        {
            lock (StampLock)
            {
                _stamp = newStamp;
            }
        }


        public abstract void SetValueCommit(object o);

        public void Lock()
        {
            ReentrantLock.Lock();
        }

        public bool TryLock(int milisecs)
        {
            return ReentrantLock.TryLock(milisecs);
        }

        public void Unlock()
        {
            ReentrantLock.Unlock();
        }

        public bool IsLocked()
        {
            return ReentrantLock.IsLocked();
        }

        public bool IsLockedByCurrentThread()
        {
            return ReentrantLock.IsLockedByCurrentThread();
        }

    }
}
