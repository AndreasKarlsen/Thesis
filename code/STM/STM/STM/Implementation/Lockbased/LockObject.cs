using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace STM.Implementation.Lockbased
{
    public class LockObject<T> : BaseLockObject
    {
        private T _version;

        public LockObject(T value)
        {
            _version = value;
        }

        public virtual T Value
        {
            get { return GetValue(); }
            set { SetValue(value); }
        }

        public static implicit operator T(LockObject<T> tmObject)
        {
            return tmObject.Value;
        }

        public virtual void SetValue(T value)
        {
            _version = value;
        }

        public virtual T GetValue()
        {
            Lock();
            var tmp = _version;
            Unlock();
            
            return tmp;
        }

        public virtual bool Validate()
        {
            Transaction me = Transaction.GetLocal();
            switch (me.GetStatus())
            {
                case Transaction.Status.Committed:
                    return true;
                case Transaction.Status.Active:
                    return GetStamp() <= VersionClock.GetReadStamp();
                case Transaction.Status.Aborted:
                    return false;
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }

        public override void CommitValue(object o)
        {

#if DEBUG
            Transaction me = Transaction.GetLocal();
            Console.WriteLine("Transaction: " + me.ID + " commited:" + o);
#endif
            _version = (T)o;
            lock (WaitHandlesLock)
            {
                foreach (var waitHandle in WaitHandles)
                {
                    waitHandle.Set();
                }
                ClearWaitHandles();
            }
        }
    }
}
