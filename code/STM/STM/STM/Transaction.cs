using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;

namespace STM
{
    public class Transaction
    {
        public readonly object LockObject = new object();
        public enum Status : int { Aborted, Active, Committed };
        public static Transaction Committed = new Transaction(Status.Committed);
        //Possibly use int or class ref instead of enum for compare and swap
        private Status _transactionStatus;
        public string ID { get; private set; }

        /// <summary>
        /// Thread local storage. Each thread gets has its own value
        /// </summary>
        private static readonly ThreadLocal<Transaction> Local = new ThreadLocal<Transaction>(() => new Transaction(Status.Committed));

        public static Transaction GetLocal()
        {
            return Local.Value;
        }

        public static void SetLocal(Transaction value)
        {
            Local.Value = value;
        }

        public Transaction()
        {
            ID = Guid.NewGuid().ToString();
            _transactionStatus = Status.Active;
        }

        public Transaction(Status status)
        {
            ID = Guid.NewGuid().ToString();
            _transactionStatus = status;
        }

        public Status GetStatus()
        {
            Status temp;
            lock (LockObject)
            {
                temp = _transactionStatus;
            }
            return temp;
        }

        public bool Commit()
        {
            bool commited = false;
            lock (LockObject)
            {
                if (_transactionStatus == Status.Active)
                {
                    _transactionStatus = Status.Committed;
#if DEBUG
                    Console.WriteLine("Transaction: " + ID + " commited");
#endif
                    commited = true;
                }
            }
            //Interlocked.CompareExchange(ref _transactionStatus, Status.Committed, Status.Active);
            return commited;
        }

        public void Abort()
        {
            lock (LockObject)
            {
                _transactionStatus = Status.Aborted;
#if DEBUG
                Console.WriteLine("Transaction: " + ID + " aborted");
#endif
            }
        }

    }
}
