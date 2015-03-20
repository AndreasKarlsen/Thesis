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

        public override string ToString()
        {
            return _version.ToString();
        }

        internal virtual bool Validate(Transaction transaction)
        {
            switch (transaction.Status)
            {
                case Transaction.TransactionStatus.Committed:
                    return true;
                case Transaction.TransactionStatus.Active:
                    return TimeStamp <= transaction.ReadStamp;
                case Transaction.TransactionStatus.Aborted:
                    return false;
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }

        internal override void CommitValue(object o)
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

        #region Operators

        public static bool operator ==(LockObject<T> tmvar, T other)
        {
            return (dynamic)tmvar.Value == (dynamic)other;
        }

        public static bool operator !=(LockObject<T> tmvar, T other)
        {
            return (dynamic)tmvar.Value != (dynamic)other;
        }

        public static bool operator ==(LockObject<T> tmvar, LockObject<T> other)
        {
            return (dynamic)tmvar.Value == (dynamic)other.Value;
        }

        public static bool operator !=(LockObject<T> tmvar, LockObject<T> other)
        {
            return (dynamic)tmvar.Value != (dynamic)other.Value;
        }

        public static implicit operator T(LockObject<T> tmObject)
        {
            return tmObject.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is LockObject<T>)
            {
                return this == (LockObject<T>)obj;
            }
            else if (obj is T)
            {
                return this == (T)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return _version.GetHashCode();
        }

        #endregion Operators
    }
}
