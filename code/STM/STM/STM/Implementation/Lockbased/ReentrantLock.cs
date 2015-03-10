using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public class ReentrantLock : ILock
    {
        protected readonly object LockOjbect = new object();

        public virtual void Lock()
        {
            Monitor.Enter(LockOjbect);
        }

        public virtual bool TryLock(int milisecs)
        {
            return TryLock(new TimeSpan(0, 0, 0, 0, milisecs));
        }

        public virtual bool TryLock(TimeSpan span)
        {
            return Monitor.TryEnter(LockOjbect, span);
        }

        public virtual void UnLock()
        {
            Monitor.Exit(LockOjbect);
        }


        public virtual bool IsLocked
        {
            get { var result = Monitor.TryEnter(LockOjbect, 0);
                if (!result) return true;
                Monitor.Exit(LockOjbect);
                return false;
            }
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
            get { return Monitor.IsEntered(LockOjbect); }
        }
    }
}
