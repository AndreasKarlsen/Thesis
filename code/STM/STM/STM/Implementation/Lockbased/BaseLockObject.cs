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
        internal readonly IList<ManualResetEvent> WaitHandles = new List<ManualResetEvent>();
        protected readonly object WaitHandlesLock = new object();
        internal readonly Semaphore WaitHandle = new Semaphore(0, 99);
        protected int WaitCount = 0;

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

        internal WaitHandle RegisterWaitHandle()
        {
            ManualResetEvent wh;
            lock (WaitHandlesLock)
            {
                wh = new ManualResetEvent(false);
                WaitHandles.Add(wh);
            }

            return wh;
        }

        protected void ClearWaitHandles()
        {
            lock (WaitHandlesLock)
            {
                WaitHandles.Clear();
            }
        }
    }
}
