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
    public static class STMSystem
    {
        internal static readonly int TIME_OUT = 100;

        #region Atomic

        public static T Atomic<T>(Func<T> stmAction)
        {
            return AtomicInternal(new List<Func<T>> {stmAction});

            /*
            var result = default(T);
            while (true)
            {

                var me = Transaction.StartTransaction();
                try
                {
                    result = stmAction();

                    if (OnValidate(me) && me.Commit())
                    {
                        OnCommit(me);
                        return result;
                    }

                    me.Abort();
                }
                catch (STMAbortException)
                {
                    me.Abort();
                }
                catch (STMRetryException)
                {
                    WaitOnReadset(me);
                    me.Abort();
                }
                catch (STMException) { }

            }*/
        }

        public static T Atomic<T>(Func<T> stmAction, params Func<T>[] orElses)
        {
            var atomics = new List<Func<T>>(orElses.Length+1) {stmAction};
            atomics.AddRange(orElses);
            return AtomicInternal(atomics);
        }

        public static void Atomic(Action stmAction)
        {
            AtomicInternal(new List<Func<bool>>()
            {
                () =>
                {
                    stmAction();
                    return true;
                }
            });
        }

        public static void Atomic(Action stmAction, params Action[] orElses)
        {
            var atomics = new List<Func<bool>>(orElses.Length + 1)
            {
                () =>
                {
                    stmAction();
                    return true;
                }
            };
            
            //Convert orElseblocks from Action to Func<bool>
            atomics.AddRange(orElses.Select<Action,Func<bool>>(orElseBlcok => 
                () =>
                {
                    orElseBlcok();
                    return true;
                }
            ));

            AtomicInternal(atomics);
        }

        private static T AtomicInternal<T>(IList<Func<T>> stmActions)
        {
            Debug.Assert(stmActions.Count > 0);
            var result = default(T);
            var index = 0;
            var overAllReadSet = new ReadSet();
            while (true)
            {
                var stmAction = stmActions[index];

                var prevTransaction = Transaction.LocalTransaction;
                var me = prevTransaction.Status == Transaction.TransactionStatus.Active ? 
                    Transaction.StartNestedTransaction(prevTransaction) : 
                    Transaction.StartTransaction();

                try
                {
                    result = stmAction();

                    if (OnValidate(me) && me.Commit())
                    {
                        if (me.IsNested)
                        {
                            me.WriteSet.Unlock();
                            me.MergeWithParent();
                            Transaction.LocalTransaction = me.Parent;
                        }
                        else
                        {
                            OnCommit(me);
                        }

                        return result;
                    }

                    me.Abort();
                }
                catch (STMAbortException)
                {
                    me.Abort();
                }
                catch (STMRetryException)
                {
                    index = HandleRetry(stmActions, me, index, overAllReadSet);
                }
                catch (STMException)
                { }

                //If the transaction is nested restore the parent as current transaction before retrying
                if (me.IsNested)
                {
                    Transaction.LocalTransaction = me.Parent;
                }
            }
        }

        #endregion Atomic   

        #region Commit

        private static void OnCommit(Transaction transaction)
        {
            var writeStamp = VersionClock.IncrementClock();
            foreach (var entry in transaction.WriteSet)
            {
                var lo = entry.Key;
                var value = entry.Value;
                lo.CommitValue(value);
                lo.TimeStamp = writeStamp;
            }

            transaction.WriteSet.Unlock();
            transaction.WriteSet.Clear();
            transaction.ReadSet.Clear();
        }

        private static bool OnValidate(Transaction transaction)
        {

            if (transaction.Status == Transaction.TransactionStatus.Aborted)
            {
                return false;
            }

            if (!transaction.WriteSet.TryLock(TIME_OUT))
            {
                return false;
            }

            if (ValidateReadset(transaction, transaction.ReadSet))
            {
                return true;
            }

            transaction.WriteSet.Unlock();
            return false;
        }

        private static bool ValidateReadset(Transaction transaction, ReadSet readSet)
        {
            foreach (var lo in readSet)
            {
                if (lo.IsLocked() && !lo.IsLockedByCurrentThread())
                {
                    return false;
                }

                if (lo.TimeStamp > transaction.ReadStamp)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion Commit

        #region Retry

        public static void Retry()
        {
            var transaction = Transaction.LocalTransaction;
            if (transaction.Status == Transaction.TransactionStatus.Committed)
                return;

            throw new STMRetryException();
        }

        private static int HandleRetry<T>(IList<Func<T>> stmActions, Transaction me, int index, ReadSet overAllReadSet)
        {
            if (stmActions.Count == 1) //Optimized for when there are no orelse blocks
            {
                WaitOnReadset(me, me.ReadSet);
            }
            else if (stmActions.Count == index + 1) //Final orelse block
            {
                overAllReadSet.Merge(me.ReadSet);
                WaitOnReadset(me, overAllReadSet);
                index = 0;
            }
            else //Non final atomic or orelse blocks
            {
#if DEBUG
                Console.WriteLine("Transaction: " + me.ID + " ORELSE jump");
#endif
                overAllReadSet.Merge(me.ReadSet);
                index++;
            }

            me.Abort();
            return index;
        }

        private static void WaitOnReadset(Transaction me, ReadSet readSet)
        {

#if DEBUG
            Console.WriteLine("ENTERED WAIT ON RETRY: " + me.ID);
#endif

            if (readSet.Count == 0)
            {
                throw new STMInvalidRetryException();
            }

            var waiton = new WaitHandle[readSet.Count()];

            var i = 0;
            foreach (var item in readSet)
            {
                waiton[i] = item.RegisterWaitHandle();
                i++;
            }

            if (!ValidateReadset(me, readSet))
            {
                return;
            }

#if DEBUG
            Console.WriteLine("WAIT ON RETRY: " + me.ID);
#endif
            WaitHandle.WaitAny(waiton);
#if DEBUG
            Console.WriteLine("WAIT ON RETRY: " + me.ID);
#endif
            /*
                //Attempt to block the transaction waiting for some value to change
                //If unable simply return and rerun the transaction from the start
                if (readset.TryLock(TIME_OUT))
                {
                    if (!ValidateReadset(readset))
                    {
                        readset.UnLock();
                        return true;
                    }

                    var waiton = new WaitHandle[readset.Count()];

                    var i = 0;
                    foreach (var item in readset.LockObjects)
                    {
                        waiton[i] = item.RegisterWaitHandle();
                        i++;
                    }
                    readset.UnLock();
#if DEBUG
                    Console.WriteLine("Transaction: " + me.ID + " waiting for retry.");
#endif
                    WaitHandle.WaitAny(waiton);               
#if DEBUG
                    Console.WriteLine("Transaction: " + me.ID + " awoken from retry.");
#endif
                }*/


        }

        #endregion Retry

    }
}
