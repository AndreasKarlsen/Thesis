using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using STM.Implementation.Common;
using STM.Implementation.Exceptions;
using STM.Implementation.Lockbased;

namespace STM.Implementation.JVSTM
{
    public static class JVSTMSystem
    {
        internal static bool GC_Status = false;

        /// <summary>
        /// Max attempts. Set to same as Clojure
        /// </summary>
        internal static readonly int MAX_ATTEMPTS = 10000;

        #region Atomic

        public static T Atomic<T>(Func<JVTransaction,T> stmAction)
        {
            return AtomicInternal(new List<Func<JVTransaction,T>> { stmAction });
        }
        
        public static T Atomic<T>(Func<JVTransaction,T> stmAction, params Func<JVTransaction,T>[] orElses)
        {
            var atomics = new List<Func<JVTransaction,T>>(orElses.Length + 1) { stmAction };
            atomics.AddRange(orElses);
            return AtomicInternal(atomics);
        }

        public static void Atomic(Action<JVTransaction> stmAction)
        {
            AtomicInternal(new List<Func<JVTransaction, bool>>()
            {
                (transaction) =>
                {
                    stmAction(transaction);
                    return true;
                }
            });
        }

        
        public static void Atomic(Action<JVTransaction> stmAction, params Action<JVTransaction>[] orElses)
        {
            var atomics = new List<Func<JVTransaction,bool>>(orElses.Length + 1)
            {
                (t) =>
                {
                    stmAction(t);
                    return true;
                }
            };

            //Convert orElseblocks from Action to Func<bool>
            atomics.AddRange(orElses.Select<Action<JVTransaction>, Func<JVTransaction,bool>>(orElseBlcok =>
                (transaction) =>
                {
                    orElseBlcok(transaction);
                    return true;
                }
            ));

            AtomicInternal(atomics);
        }

        private static T AtomicInternal<T>(IList<Func<JVTransaction,T>> stmActions)
        {
            Debug.Assert(stmActions.Count > 0);
            var result = default(T);
            var index = 0;
            var nrAttempts = 0;
            var overAllReadSet = new ReadMap();

            while (true)
            {
                var stmAction = stmActions[index];

                var localTransaction = JVTransaction.LocalTransaction;
                var transaction = localTransaction.Status == TransactionStatus.Active
                    ? JVTransaction.StartNested(localTransaction)
                    : JVTransaction.Start();
                JVTransaction.LocalTransaction = transaction;

                try
                {
                    //Execute transaction body
                    result = stmAction(transaction);

                    if (transaction.Commit())
                    {
                        return result;
                    }

                    transaction.Abort();
                }
                catch (STMRetryException)
                {
                    index = HandleRetry(stmActions, transaction, index, overAllReadSet);
                }/*
                catch (Exception ex) //Catch non stm related exceptions which occurs in transactions
                {
                    //Throw exception of transaction can commit
                    //Else abort and rerun transaction
                    if (transaction.Commit())
                    {
                        throw;
                    }
                }*/

                nrAttempts++;
                //transaction.Abort();

                if (transaction.IsNested)
                {
                    JVTransaction.LocalTransaction = transaction.Parent;
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
            throw new STMRetryException();
        }

        private static int HandleRetry<T>(IList<Func<JVTransaction, T>> stmActions, JVTransaction transaction, int index, ReadMap overAllReadSet)
        {
            if (stmActions.Count == 1) //Optimized for when there are no orelse blocks
            {
                WaitOnReadset(transaction, transaction.ReadMap);
            }
            else if (stmActions.Count == index + 1) //Final orelse block
            {
                overAllReadSet.Merge(transaction.ReadMap);
                WaitOnReadset(transaction, overAllReadSet);
                index = 0;
            }
            else //Non final atomic or orelse blocks
            {
#if DEBUG
                Console.WriteLine("Transaction: " + transaction.Number + " ORELSE jump");
#endif
                overAllReadSet.Merge(transaction.ReadMap);
                index++;
            }

            return index;
        }


        private static void WaitOnReadset(JVTransaction transaction, ReadMap readMap)
        {

#if DEBUG
            Console.WriteLine("ENTERED WAIT ON RETRY: " + transaction.Number);
#endif

            if (readMap.Count == 0)
            {
                throw new STMInvalidRetryException();
            }

            var expectedEra = transaction.RetryLatch.Era;

            foreach (var kvpair in readMap)
            {
                kvpair.Key.RegisterRetryLatch(transaction.RetryLatch, kvpair.Value, expectedEra);
            }

            if (!transaction.ReadMap.Validate())
            {
                transaction.Abort();
                return;
            }

#if DEBUG
            Console.WriteLine("WAIT ON RETRY: " + transaction.Number);
#endif
            transaction.Await(expectedEra);
            transaction.RetryLatch.Reset();
#if DEBUG
            Console.WriteLine("AWOKEN: " + transaction.Number);
#endif
            transaction.Abort();
        }


        #endregion Retry



        #region GC

        public static void StartGC()
        {
            if (!GC_Status)
            {
                GC_Status = true;
                var gc_thread = new Thread(() =>
                {
                    var lasCleanedTo = ActiveTxnRecord.First;
                    while (GC_Status)
                    {
                        var rec = FindOldestRecordInUse();
                        lasCleanedTo = CleanUpTo(rec, lasCleanedTo);
                        Thread.Sleep(5);
                    }
                });

                gc_thread.IsBackground = true;
                gc_thread.Start();
            }
        }

        internal static ActiveTxnRecord FindOldestRecordInUse()
        {
            var alreadyCommitted = ActiveTxnRecord.LastCommitted;

            var minRequiredRec_1 = GetOldestRecord();
            if (minRequiredRec_1 == null)
            {
                return alreadyCommitted;
            }

            var minRequiredRec_2 = GetOldestRecord();
            return (minRequiredRec_2 != null) ? minRequiredRec_2 : minRequiredRec_1;
        }

        internal static ActiveTxnRecord GetOldestRecord()
        {
            return GetOldestRecord(null);
        }

        internal static ActiveTxnRecord GetOldestRecord(TxnContext upTo)
        {
            var context = TxnContext.Head;
            ActiveTxnRecord oldestRecord = null;
            TxnContext prevContext = null;
            while (context != upTo)
            {
                var contextOldest = context.OldestRequiredRecord;
                if (oldestRecord == null || (contextOldest != null && oldestRecord.TxNumber > contextOldest.TxNumber))
                {
                    oldestRecord = contextOldest;
                }

                if (!context.OwningThread.IsAlive && prevContext != null)
                {
                    prevContext.Next = context.Next;
                }

                prevContext = context;
                context = context.Next;
            }

            return oldestRecord;
        }

        internal static ActiveTxnRecord CleanUpTo(ActiveTxnRecord cleanTo, ActiveTxnRecord lastCleanTo)
        {
            if (cleanTo.TxNumber < lastCleanTo.TxNumber)
            {
                return lastCleanTo;
            }

            var rec = lastCleanTo;
            
            while (rec != cleanTo)
            {
                
                rec.Clean();
                if (!rec.Next.IsCommited)
                {
                    break;
                }

                rec = rec.Next;
            }


            return rec;
        }

        public static void StopGC()
        {
            GC_Status = false;
        }

        #endregion GC
    }

   
}
