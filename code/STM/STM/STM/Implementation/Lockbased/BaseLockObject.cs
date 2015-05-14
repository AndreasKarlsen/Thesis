using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Common;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public abstract class BaseLockObject
    {
        protected readonly ILock ReentrantLock = new ReentrantLock();
        protected readonly IList<ManualResetEvent> WaitHandles = new List<ManualResetEvent>();
        protected readonly object WaitHandlesLock = new object();
        protected int WaitCount = 0;

        private volatile int _stamp;

        internal int TimeStamp
        {
            get { return _stamp; }
            set { _stamp = value; }
        }

        internal abstract void Commit(object o, int timestamp);

        internal void Lock()
        {
            ReentrantLock.Lock();
        }

        internal bool TryLock(int milisecs)
        {
            return ReentrantLock.TryLock(milisecs);
        }

        internal void Unlock()
        {
            ReentrantLock.UnLock();
        }

        internal bool IsLocked()
        {
            return ReentrantLock.IsLocked;
        }

        internal bool IsLockedByCurrentThread()
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

        internal virtual bool Validate(Transaction transaction, int readstamp)
        {
            switch (transaction.Status)
            {
                case TransactionStatus.Committed:
                    return true;
                case TransactionStatus.Active:
                    /*
                    if (IsLocked() && !IsLockedByCurrentThread())
                    {
                        return false;
                    }*/
#if DEBUG
                    Console.WriteLine("Validating! transaction: " + transaction.ID + " timeStamp: " + TimeStamp + " readstamp: " + readstamp);
#endif
                    return TimeStamp <= readstamp;
                case TransactionStatus.Aborted:
                    return false;
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }
    }
}
