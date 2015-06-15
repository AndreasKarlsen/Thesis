using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;

namespace STM.Implementation.JVSTM
{
    internal class ActiveTxnRecord
    {
        private static readonly int STATUS_COMMITTED = 1; 
        private static readonly int STATUS_VALID = 0;


        public readonly int TxNumber;
        public BaseVBoxBody[] BodiesToClean;
        public int Running = 0;
        public volatile ActiveTxnRecord Next = null;
        public static ActiveTxnRecord First = new ActiveTxnRecord(0);
        public static volatile ActiveTxnRecord LastCommitted = First;
        public WriteMap WriteMap;
        private volatile int _status;

        public bool IsCommited { get { return _status == STATUS_COMMITTED; } }
        public void SetCommitted()
        {
            _status = STATUS_COMMITTED;
        }

        internal ActiveTxnRecord(int txNumber)
        {
            TxNumber = txNumber;
            SetCommitted();
        }

        internal ActiveTxnRecord(int txNumber, BaseVBoxBody[] bodies)
        {
            TxNumber = txNumber;
            BodiesToClean = bodies;
            Running = 1;
        }
        
        internal ActiveTxnRecord(int txNumber, WriteMap bodies)
        {
            TxNumber = txNumber;
            WriteMap = bodies;
            Running = 1;
        }

        internal static void InsertNewRecord(ActiveTxnRecord record)
        {
            LastCommitted.Next = record;
            LastCommitted = record;
        }

        internal static ActiveTxnRecord StartTransaction()
        {
            var rec = LastCommitted;
            while (true)
            {
                //Interlocked.Increment(ref rec.Running);
                if (rec.Next == null || !rec.Next.IsCommited)
                {
                    TxnContext.LocalContext.OldestRequiredRecord = rec;
                    // if there is no next yet, then it’s because the rec
                    // is the most recent one and we may return its number
                    return rec;
                }
                else
                {
                    // a more recent record exists, so backoff
                    //Interlocked.Decrement(ref rec.Running);
                    // and try again with the new one
                    rec = rec.Next;
                }
            }
        }

        internal void FinishTransaction()
        {
            if (Interlocked.Decrement(ref Running) == 0)
            {
                // when running reachs 0 maybe
                // it is time to clean our successor
                var rec = this;
                while (true)
                {
                    // it is crucial that we test the next field first,
                    // because only after having the next non-null,
                    // do we have the guarantee that no transactions
                    // may start for this record
                    if ((rec.Next != null)
                        && (BodiesToClean == null)
                        && (Running == 0))
                    {
                        if (rec.Next.Clean())
                        {
                            // if we cleaned up, move to the next
                            rec = rec.Next;
                            // and repeat the test
                            continue;
                        }
                    }
                    break;
                }
            }
        }

        internal bool Clean()
        {
            if (WriteMap != null)
            {
                WriteMap.Clean();
                WriteMap = null;
            }

            return true;
            /*
            var toClean = Interlocked.Exchange(ref WriteMap, null);
            // the toClean may be null because more
            // than one thread may race into this method
            // yet, because of the atomic getAndSet above,
            // only one will actually clean the bodies
            if (toClean != null)
            {
                toClean.Clean();
                return true;
            }
            else
            {
                return false;
            }*/
        }

        internal bool TrySetNext(ActiveTxnRecord record)
        {
            return Interlocked.CompareExchange<ActiveTxnRecord>(ref this.Next,record,null) == null;
        }

        internal static void FinishCommit(ActiveTxnRecord recToCommit) {
            recToCommit.SetCommitted();
            LastCommitted = recToCommit;
        }
    }
}
