using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;
using System.Threading;
using STM.Implementation.Exceptions;
using System.Diagnostics;
using STM.Implementation.Common;

namespace STM.Implementation.Lockbased
{
    public static class STMSystem
    {
        internal static readonly int TIME_OUT = 100;
        /// <summary>
        /// Max attempts. Set to same as Clojure
        /// </summary>
        internal static readonly int MAX_ATTEMPTS = 10000;

        #region Atomic

        public static T Atomic<T>(Func<T> stmAction)
        {
            return AtomicInternal(new List<Func<T>> {stmAction});
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
            var ovarAllReadstamp = -1;
            var nrAttempts = 0;

            while (true)
            {
                var stmAction = stmActions[index];

                //Start new or nested transaction
                var prevTransaction = Transaction.LocalTransaction;
                var me = prevTransaction.Status == TransactionStatus.Active ? 
                    Transaction.StartNestedTransaction(prevTransaction) : 
                    Transaction.StartTransaction();

                if (index == 0)
                {
                    ovarAllReadstamp = me.ReadStamp;
                }

                try
                {
                    //Execute transaction body
                    result = stmAction();

                    if (me.Commit())
                    {
                        return result;
                    }

                }
                catch (STMAbortException)
                {
                    //Skips straight to abort a reexecute
                }
                catch (STMRetryException)
                {
                    index = HandleRetry(stmActions, me, index, overAllReadSet, ovarAllReadstamp);
                }
                catch (Exception) //Catch non stm related exceptions which occurs in transactions
                {
                    //Throw exception of transaction can commit
                    //Else abort and rerun transaction
                    if (me.Commit())
                    {
                        throw;
                    }
                }

                nrAttempts++;
                me.Abort();

                //If the transaction is nested restore the parent as current transaction before retrying
                if (me.IsNested)
                {
                    Transaction.LocalTransaction = me.Parent;
                }

                if (nrAttempts == MAX_ATTEMPTS)
                {
                    throw new STMMaxAttemptException("Fatal error: max attempts reached");
                }
            }
        }

        #endregion Atomic   

        #region Retry

        public static void Retry()
        {
            var transaction = Transaction.LocalTransaction;
            if (transaction.Status == TransactionStatus.Committed)
                return;

            throw new STMRetryException();
        }

        private static int HandleRetry<T>(IList<Func<T>> stmActions, Transaction me, int index, ReadSet overAllReadSet, int originalReadstamp)
        {
            if (stmActions.Count == 1) //Optimized for when there are no orelse blocks
            {
                WaitOnReadset(me, me.ReadSet);
            }
            else if (stmActions.Count == index + 1) //Final orelse block
            {
                overAllReadSet.Merge(me.ReadSet);
                WaitOnReadset(me, overAllReadSet,originalReadstamp);
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

            return index;
        }

        private static void WaitOnReadset(Transaction me, ReadSet readSet)
        {
            WaitOnReadset(me,readSet,me.ReadStamp);
        }

        private static void WaitOnReadset(Transaction me, ReadSet readSet, int readstamp)
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
                waiton[i] = item.Key.RegisterWaitHandle();
                i++;
            }

            if (!readSet.Validate(me,readstamp))
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
        }

        #endregion Retry

    }
}
