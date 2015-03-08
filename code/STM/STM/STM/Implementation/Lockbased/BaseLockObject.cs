using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public abstract class BaseLockObject
    {
        protected readonly ILock ReentrantLock = new AtomicLock();
        internal readonly IList<ManualResetEvent> WaitHandles = new List<ManualResetEvent>();
        protected readonly object WaitHandlesLock = new object();
        //internal readonly Semaphore WaitHandle = new Semaphore(0, 99);
        protected int WaitCount = 0;

        private volatile int _stamp;

        public int TimeStamp
        {
            get { return _stamp; }
            internal set { _stamp = value; }
        }

        public abstract void CommitValue(object o);

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
            ReentrantLock.UnLock();
        }

        public bool IsLocked()
        {
            return ReentrantLock.IsLocked;
        }

        public bool IsLockedByCurrentThread()
        {
            return ReentrantLock.IsLockedByCurrentThread;
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
