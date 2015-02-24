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
    public abstract class LockSTMSystem
    {
        internal static readonly int TIME_OUT = 100;


        public static void Retry()
        {
            throw new STMRetryException();
        }


        private static void OnAbort()
        {
            var writeset = WriteSet.GetLocal();
            var readset = ReadSet.GetLocal();
            writeset.Clear();
            readset.Clear();
        }

        private static void OnCommit()
        {
            var writeset = WriteSet.GetLocal();
            var readset = ReadSet.GetLocal();
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

        private static bool OnValidate()
        {
            var me = Transaction.GetLocal();
            if (me.GetStatus() == Transaction.Status.Aborted)
            {
                return false;
            } 


            var writeset = WriteSet.GetLocal();
            var readset = ReadSet.GetLocal();
            if (!writeset.TryLock(TIME_OUT))
            {
                return false;
            }

            if (ValidateReadset(readset))
            {
                return true;
            }

            writeset.Unlock();
            return false;
        }


        private static bool ValidateReadset(ReadSet readset)
        {
            foreach (var lo in readset.LockObjects)
            {
                if (lo.IsLocked() && !lo.IsLockedByCurrentThread())
                {
                    return false;
                }

                var loStamp = lo.GetStamp();
                var readStamp = VersionClock.GetReadStamp();
                if (loStamp > readStamp)
                {
                    return false;
                }
            }

            return true;
        }

        public static T Atomic<T>(Func<T> stmAction)
        {
            var result = default(T);
            while (true)
            {
                var me = new Transaction();
                Transaction.SetLocal(me);
                VersionClock.SetReadStamp();
                try
                {
                    result = stmAction();

                    if (OnValidate() && me.Commit())
                    {
                        OnCommit();
                        return result;
                    }

                    me.Abort();
                    OnAbort();
                }
                catch (STMAbortException)
                {
                    me.Abort();
                    OnAbort();
                }
                catch (STMRetryException)
                {
                    WaitOnReadset(me);
                    me.Abort();
                    OnAbort();
                }
                catch (STMException) { }
                    /*
                catch (Exception ex)
                {
                    Debug.Assert(false, "STM fail: " + ex.Message);
                }*/
                

            }
        }

        public static void Atomic(Action stmAction)
        {
            Atomic(() =>
            {
                stmAction();
                return true;
            });
        }

        private static void WaitOnReadset(Transaction me)
        {
            
            var readset = ReadSet.GetLocal();
            if (readset.Count() != 0)
            {
                var waiton = new WaitHandle[readset.Count()];

                var i = 0;
                foreach (var item in readset.LockObjects)
                {
                    waiton[i] = item.RegisterWaitHandle();
                    i++;
                }

                if (!ValidateReadset(readset))
                {
                    return;
                }

#if DEBUG
                Console.WriteLine("Transaction: " + me.ID + " waiting for retry.");
#endif
                WaitHandle.WaitAny(waiton);
#if DEBUG
                Console.WriteLine("Transaction: " + me.ID + " awoken from retry.");
#endif
                /*
                //Attempt to block the transaction waiting for some value to change
                //If unable simply return and rerun the transaction from the start
                if (readset.TryLock(TIME_OUT))
                {
                    if (!ValidateReadset(readset))
                    {
                        readset.Unlock();
                        return true;
                    }

                    var waiton = new WaitHandle[readset.Count()];

                    var i = 0;
                    foreach (var item in readset.LockObjects)
                    {
                        waiton[i] = item.RegisterWaitHandle();
                        i++;
                    }
                    readset.Unlock();
#if DEBUG
                    Console.WriteLine("Transaction: " + me.ID + " waiting for retry.");
#endif
                    WaitHandle.WaitAny(waiton);               
#if DEBUG
                    Console.WriteLine("Transaction: " + me.ID + " awoken from retry.");
#endif
                }*/
                
                return;
            }
            else
            {
                throw new STMInvalidRetryException();
            }
        }
    }
}
