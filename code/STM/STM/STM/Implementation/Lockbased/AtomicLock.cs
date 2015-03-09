using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;
using STM.Interfaces;
using STM.Util;

namespace STM.Implementation.Lockbased
{
    public class AtomicLock : ILock
    {
#if DEBUG
        protected readonly long ID = IDGenerator.NextID;
#endif
        protected readonly ManualResetEvent ResetEvent = new ManualResetEvent(true);
        protected readonly ConcurrentQueue<Thread> Waiters = new ConcurrentQueue<Thread>(); 
        protected readonly AtomicReference<Thread> Holder = new AtomicReference<Thread>(null);
        protected volatile int HoldCounter = 0;

        public void Lock()
        {
            var current = Thread.CurrentThread;
            if (Holder.Value == current)
            {
                HoldCounter++;
            }
            else
            {
                Waiters.Enqueue(current);
                Thread first;
                while ((Waiters.TryPeek(out first) && first != current) || !Holder.CompareAndSet(null,current))
                {
                    ResetEvent.WaitOne();
                }
                TakeLock();
            }
        }

        public void UnLock()
        {
            var current = Thread.CurrentThread;
            if (Holder.Value == current)
            {
                if (HoldCounter > 1)
                {
                    HoldCounter--;
                    return;
                }

                while (!Holder.CompareAndSet(current, null)){ }
                ResetEvent.Set();
            }
            else
            {
                throw new Exception("Unlocked called from thread not holding the lock");
            }
        }

        public bool TryLock(int milisecs)
        {
            var start = DateTime.UtcNow;
            var current = Thread.CurrentThread;
            if (Holder.Value == current)
            {
                HoldCounter++;
            }
            else
            {
                Waiters.Enqueue(current);
                Thread first;
                while ((Waiters.TryPeek(out first) && first != current) || !Holder.CompareAndSet(null, current))
                {
                    if (!ResetEvent.WaitOne(milisecs))
                    {
                        return false;
                    }

                    var now = DateTime.UtcNow;
                    milisecs -= (int)now.Subtract(start).TotalMilliseconds;
                    if (milisecs < 0)
                    {
                        return false;
                    }
                }
                TakeLock();
            }
            return true;
        }

        public bool TryLock(TimeSpan span)
        {
            return TryLock((int)span.TotalMilliseconds);
        }

        private void TakeLock()
        {
            ResetEvent.Reset();
            HoldCounter = 1;
            Thread current;
            while (!Waiters.TryDequeue(out current)) { }
#if DEBUG
            Console.WriteLine("Transaction {0} aquired lock {1}", Transaction.LocalTransaction.ID, ID);
#endif
        }

        public virtual bool IsLocked
        {
            get { return Holder.Value != null; }
        }

        /*
        public bool IsLocked()
        {
            
            var result = Monitor.TryEnter(LockOjbect, 0);
            if (result)
            {
                Monitor.Exit(LockOjbect);
                return false;
            }

            return true;
            
        }*/

        public virtual bool IsLockedByCurrentThread
        {
            get { return Thread.CurrentThread == Holder.Value; }
        }
    }
}
