using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace STM.Implementation.JVSTM
{
    internal class TxnContext
    {
        public static volatile TxnContext Head;

        private TxnContext (Thread thread, TxnContext next)
	    {
            OwningThread = thread;
            Next = next;
	    }

        private static ThreadLocal<TxnContext> LOCAL = new ThreadLocal<TxnContext>(() =>
        {
            TxnContext prev;
            TxnContext context;
            do
            {
                prev = Head;
                context = new TxnContext(Thread.CurrentThread, prev);
                
            } while (prev != Interlocked.CompareExchange(ref Head,context,prev));
            return context;
        });

        public static TxnContext LocalContext { get { return LOCAL.Value; } }

        public volatile ActiveTxnRecord OldestRequiredRecord;
        public readonly Thread OwningThread;
        public TxnContext Next;
    }
}
