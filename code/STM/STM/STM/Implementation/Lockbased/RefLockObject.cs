using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;
using STM.Exceptions;

namespace STM.Implementation.Lockbased
{
    public class RefLockObject<T> : LockObject<T> where T : class, ICopyable<T>, new()
    {

        public RefLockObject(T value)
            : base(value)
        {

        }

        public override T GetValue()
        {
            T tmp = GetValueInternal();
            return tmp;
        }

        private T GetValueInternal()
        {
            Transaction me = Transaction.GetLocal();
            ReadSet readset = ReadSet.GetLocal();
            switch (me.GetStatus())
            {
                case Transaction.Status.Committed:
                    return base.GetValue();
                case Transaction.Status.Active:
                    WriteSet writeset = WriteSet.GetLocal();
                    if (!writeset.Contains(this))
                    {
                        T value = base.GetValue();
#if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " read:" + value);
#endif
                        /*
                        if (GetStamp() <= VersionClock.GetReadStamp())
                        {
                            throw new STMAbortException("Abort");
                        }*/
                        
                        if (IsLocked())
                        {
                            throw new STMAbortException("Aborted due to read from locked object");
                        }
                        ReadSet.GetLocal().Add(this);
                        return value;
                    }
                    else
                    {
                        T curVersion = (T)writeset.Get(this);
#if DEBUG
                        Console.WriteLine("Transaction: " + me.ID + " read:" + curVersion);
#endif
                        ReadSet.GetLocal().Add(this);
                        return curVersion;
                    }
                case Transaction.Status.Aborted:
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
            Transaction me = Transaction.GetLocal();
            switch (me.GetStatus())
            {
                case Transaction.Status.Committed:
                    base.SetValue(value);
                    break;
                case Transaction.Status.Active:
                    WriteSet writeset = WriteSet.GetLocal();
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
                case Transaction.Status.Aborted:
                    throw new STMException("Aborted transaction attempted to write.");
                default:
                    throw new Exception("Shits on fire yo!");
            }
        }

    }
}
