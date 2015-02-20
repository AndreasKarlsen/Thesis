using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;
using System.Threading;
using STM.Exceptions;
using System.Diagnostics;

namespace STM.Implementation.Lockbased
{
    public class LockSTMSystem : STMSystem
    {
        public int TIME_OUT = 100;

        private static readonly Lazy<LockSTMSystem> LazySystem = new Lazy<LockSTMSystem>(true);

        public LockSTMSystem()
        {

        }

        public static STMSystem GetInstance()
        {
            return LazySystem.Value;
        }

        protected override void OnAbort()
        {
            WriteSet writeset = WriteSet.GetLocal();
            ReadSet readset = ReadSet.GetLocal();
            writeset.Clear();
            readset.Clear();
        }

        protected override void OnCommit()
        {
            WriteSet writeset = WriteSet.GetLocal();
            ReadSet readset = ReadSet.GetLocal();
            VersionClock.SetWriteStamp();
            long writeStamp = VersionClock.GetWriteStamp();
            foreach (var entry in writeset.Map)
            {
                var lo = entry.Key;
                var value = entry.Value;
                lo.SetValueCommit(value);
                lo.SetStamp(writeStamp);
            }
            writeset.Unlock();
            writeset.Clear();
            readset.Clear();
        }

        protected override bool OnValidate()
        {
            Transaction me = Transaction.GetLocal();
            if (me.GetStatus() == Transaction.Status.Aborted)
            {
                return false;
            } 


            WriteSet writeset = WriteSet.GetLocal();
            ReadSet readset = ReadSet.GetLocal();
            if (!writeset.TryLock(TIME_OUT))
            {
                return false;
            }

            foreach (var lo in readset.LockObjects)
            {
                if (lo.IsLocked() && !lo.IsLockedByCurrentThread())
                {
                    writeset.Unlock();
                    return false;   
                }

                long loStamp = lo.GetStamp();
                long readStamp = VersionClock.GetReadStamp();
                if (loStamp > readStamp)
                {
                    writeset.Unlock();
                    return false;
                }
            }

            return true;
        }

        public override T Atomic<T>(Func<T> stmAction)
        {
            T result = default(T);
            while (true)
            {
                var me = new Transaction();
                Transaction.SetLocal(me);
                VersionClock.SetReadStamp();
                try
                {
                    result = stmAction();
                }
                catch (ThreadAbortException) { }
                catch (STMAbortException) { me.Abort();  }
                catch (STMRetryException)
                {
                    WaitOnReadset(me);
                }
                catch (STMException) { }
                catch (Exception ex)
                {
                    Debug.Assert(false, "STM fail: " + ex.Message);
                }

                if (OnValidate())
                {
                    if (me.Commit())
                    {
                        OnCommit();
                        return result;
                    }
                }
                me.Abort();
                OnAbort();
            }
        }

        public override void Atomic(Action stmAction)
        {
            Atomic(() =>
            {
                stmAction();
                return true;
            });
        }

        private void WaitOnReadset(Transaction me)
        {
            
            ReadSet readset = ReadSet.GetLocal();
            if (readset.Count() != 0)
            {
                WaitHandle[] waiton = new WaitHandle[readset.Count()];

                int i = 0;
                foreach (var item in readset.LockObjects)
                {
                    waiton[i] = item.WaitHandle;
                    i++;
                }
#if DEBUG
                Console.WriteLine("Transaction: " + me.ID + " waiting for retry.");
#endif
                WaitHandle.WaitAny(waiton);
#if DEBUG
                Console.WriteLine("Transaction: " + me.ID + " awoken from retry.");
#endif
            }
            me.Abort();
        }
    }
}
