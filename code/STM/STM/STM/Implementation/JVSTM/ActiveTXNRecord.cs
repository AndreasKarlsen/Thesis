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
        public readonly int TxNumber;
        public BaseVBoxBody[] BodiesToClean;
        public int Running = 0;
        public volatile ActiveTxnRecord Next = null;
        public static volatile ActiveTxnRecord First = new ActiveTxnRecord(0);

        internal ActiveTxnRecord(int txNumber)
        {
            TxNumber = txNumber;
            BodiesToClean = null;
        }

        internal ActiveTxnRecord(int txNumber, BaseVBoxBody[] bodies)
        {
            TxNumber = txNumber;
            BodiesToClean = bodies;
            Running = 1;
        }

        internal static void InsertNewRecord(ActiveTxnRecord record)
        {
            First.Next = record;
            First = record;
        }

        internal static ActiveTxnRecord StartTransaction()
        {
            var rec = First;
            while (true)
            {
                Interlocked.Increment(ref rec.Running);
                if (rec.Next == null)
                {
                    // if there is no next yet, then it’s because the rec
                    // is the most recent one and we may return its number
                    return rec;
                }
                else
                {
                    // a more recent record exists, so backoff
                    Interlocked.Decrement(ref rec.Running);
                    // and try again with the new one
                    rec = rec.Next;
                }
            }
        }

        internal void FinishTransaction()
        {
            if (Interlocked.Decrement(ref Running) == 0)
            {/*
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
                }*/
            }
        }

        internal bool Clean()
        {
            var toClean = Interlocked.Exchange(ref BodiesToClean, null);
            // the toClean may be null because more
            // than one thread may race into this method
            // yet, because of the atomic getAndSet above,
            // only one will actually clean the bodies
            if (toClean != null)
            {
                foreach (var body in toClean)
                {
                    body.Clean();
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
