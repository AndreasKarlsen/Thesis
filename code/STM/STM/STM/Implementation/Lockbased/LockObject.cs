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
            Lock();
            _version = value;
            Unlock();
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

        internal override void Commit(object o, int timestamp)
        {
            #if DEBUG
            Transaction me = Transaction.LocalTransaction;
            Console.WriteLine("Transaction: " + me.ID + " newtimestamp: " + timestamp + " oldtimestamp: " + TimeStamp + " commited:" + o);
            #endif
            _version = (T)o;
            TimeStamp = timestamp;
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
        
        public static implicit operator T(LockObject<T> tmObject)
        {
            return tmObject.Value;
        }

        #endregion Operators
    }
}
