using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STM.Implementation.Lockbased
{
    public class ReentrantLock
    {
        protected readonly object LockOjbect = new object();

        public void Lock()
        {
            Monitor.Enter(LockOjbect);
        }

        public bool TryLock(int milisecs)
        {
            return Monitor.TryEnter(LockOjbect, milisecs);
        }

        public void Unlock()
        {
            Monitor.Exit(LockOjbect);
        }

        public bool IsLocked()
        {
            bool result = Monitor.TryEnter(LockOjbect, 0);
            if (result)
            {
                Monitor.Exit(LockOjbect);
                return false;
            }

            return true;
        }

        public bool IsLockedByCurrentThread()
        {
            return Monitor.IsEntered(LockOjbect);
        }
    }
}
