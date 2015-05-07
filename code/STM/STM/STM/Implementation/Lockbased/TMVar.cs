using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;
using STM.Implementation.Exceptions;

namespace STM.Implementation.Lockbased
{
    public class TMVar<T> : LockObject<T>
    {

        public TMVar()
            : base()
        {

        }

        public TMVar(T value)
            : base(value)
        {

        }

        public override T GetValue()
        {
            var tmp = GetValueInternal();
            return tmp;
        }

        private T GetValueInternal()
        {
            var me = Transaction.LocalTransaction;
            switch (me.Status)
            {
                case Transaction.TransactionStatus.Committed:
                    return base.GetValue();
                case Transaction.TransactionStatus.Active:
                    T value;
                    if (!me.WriteSet.Contains(this))
                    {
                        var preStamp = TimeStamp;
                        value = base.GetValue();
                        //IsLocked() || 
                        if (preStamp != TimeStamp ||  me.ReadStamp < preStamp)
                        {
                            throw new STMAbortException("Aborted due to inconsistent read");
                        }
                    }
                    else
                    {
                        value = (T)me.WriteSet.Get(this);
                    }

                    #if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " read:" + value);
                    #endif

                    me.ReadSet.Add(this);
                    return value;
                case Transaction.TransactionStatus.Aborted:
                    throw new STMException("Aborted transaction attempted to read.");
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }

        public override void SetValue(T value)
        {
            SetValueInternal(value);
        }

        private void SetValueInternal(T value)
        {
            var me = Transaction.LocalTransaction;
            switch (me.Status)
            {
                case Transaction.TransactionStatus.Committed:
                    SetValueNonTransactional(value);
                    break;
                case Transaction.TransactionStatus.Active:
                    me.WriteSet.Put(this, value);
                    #if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " wrote:" + value);
                    #endif
                    break;
                case Transaction.TransactionStatus.Aborted:
                    throw new STMException("Aborted transaction attempted to write.");
                default:
                    throw new Exception("Unkown transaction state!");
            }
        }

        private void SetValueNonTransactional(T value)
        {
            Lock();
            Commit(value,VersionClock.IncrementClock());
            Unlock();
        }

        
    }
}
