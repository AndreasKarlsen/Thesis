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
           // var readset = ReadSet.GetLocal();
            switch (me.Status)
            {
                case Transaction.TransactionStatus.Committed:
                    return base.GetValue();
                case Transaction.TransactionStatus.Active:
                    var writeset = me.WriteSet;
                    if (!writeset.Contains(this))
                    {
                        var value = base.GetValue();
#if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " read:" + value);
#endif
                        
                        if (IsLocked() || me.ReadStamp < TimeStamp)
                        {
                            throw new STMAbortException("Aborted due to read from locked object");
                        }

                        me.ReadSet.Add(this);
                        return value;
                    }
                    else
                    {
                        var curVersion = (T)writeset.Get(this);
#if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " read:" + curVersion);
#endif
                        me.ReadSet.Add(this);
                        return curVersion;
                    }
                case Transaction.TransactionStatus.Aborted:
                    throw new STMException("Aborted transaction attempted to read.");
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }

        public override void SetValue(T value)
        {
            SetValueInternal(value);
            /*
            if (!Validate())
            {
                throw new STMAbortException("Failed validation");
            }*/
        }
        private void SetValueInternal(T value)
        {
            var me = Transaction.LocalTransaction;
            switch (me.Status)
            {
                case Transaction.TransactionStatus.Committed:
                    base.SetValue(value);
                    break;
                case Transaction.TransactionStatus.Active:
                    var writeset = me.WriteSet;
                    if (!writeset.Contains(this))
                    {

                        if (IsLocked())
                        {
                            throw new STMAbortException("Aborted due to read from locked object");
                        }
#if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " wrote:" + value);
#endif
                        writeset.Put(this, value);
                    }
                    else
                    {
#if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " wrote:" + value);
#endif
                        writeset.Put(this, value);
                    }
                    break;
                case Transaction.TransactionStatus.Aborted:
                    throw new STMException("Aborted transaction attempted to write.");
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }

        
    }
}
