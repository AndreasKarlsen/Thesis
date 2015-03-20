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

        public LockObject()
        {
            _version = default(T);
        }

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
            var me = Transaction.LocalTransaction;
            switch (me.Status)
            {
                case Transaction.TransactionStatus.Committed:
                    return true;
                case Transaction.TransactionStatus.Active:
                    return TimeStamp <= me.ReadStamp;
                case Transaction.TransactionStatus.Aborted:
                    return false;
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }

        public override void CommitValue(object o)
        {

#if DEBUG
            Transaction me = Transaction.LocalTransaction;
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
