using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;
using STM.Implementation.Lockbased;

namespace STM
{
    public class Transaction
    {
        public enum TransactionStatus { Aborted, Active, Committed };

        //Possibly use int or class ref instead of enum for compare and swap
        private TransactionStatus _transactionStatus;
        public string ID { get; private set; }
        public int ReadStamp { get; internal set; }
        public WriteSet WriteSet { get; private set; }
        public ReadSet ReadSet { get; private set; }
        public Transaction Parent { get; internal set; }

        public bool IsNested
        {
            get { return Parent != null; }
        }

        #region Construction

        internal static Transaction StartTransaction()
        {
            var transaction = new Transaction(TransactionStatus.Active) {ReadStamp = VersionClock.TimeStamp};
            Local.Value = transaction;
#if DEBUG
            Console.WriteLine("STARTED: "+transaction.ID);
#endif
            return transaction;
        }

        internal static Transaction StartNestedTransaction(Transaction parent)
        {
            var transaction = new Transaction(TransactionStatus.Active) { ReadStamp = VersionClock.TimeStamp };
            Local.Value = transaction;
            transaction.Parent = parent;
#if DEBUG
            Console.WriteLine("STARTED NESTED: " + transaction.ID);
#endif
            return transaction;
        }

        private Transaction(TransactionStatus status)
        {
            Init(status);
        }

        private void Init(TransactionStatus status)
        {
#if DEBUG
            ID = Guid.NewGuid().ToString();
#endif
            _transactionStatus = status;
            ReadSet = new ReadSet();
            WriteSet = new WriteSet();
        }

        #endregion Construction

        #region Nesting

        public void MergeWithParent()
        {
            Debug.Assert(IsNested);

            Parent.ReadSet.Merge(ReadSet);
            Parent.WriteSet.Merge(WriteSet);
        }

        #endregion Nesting

        #region Local

        /// <summary>
        /// Thread local storage. Each thread gets has its own value
        /// </summary>
        private static readonly ThreadLocal<Transaction> Local = new ThreadLocal<Transaction>(() => new Transaction(TransactionStatus.Committed));

        public static Transaction LocalTransaction
        {
            get { return Local.Value; }
            set { Local.Value = value; }
        }

        #endregion Local

        #region Status

        public TransactionStatus Status {
            get { return _transactionStatus; }
            set { _transactionStatus = value; }
        }



        public bool Commit()
        {  
            /*
            bool commited = false;
            lock (LockObject)
            {
                if (_transactionStatus == TransactionStatus.Active)
                {
                    _transactionStatus = TransactionStatus.Committed;
#if DEBUG
                    Console.WriteLine("Transaction: " + ID + " commited");
#endif
                    commited = true;
                }
            }
            //Interlocked.CompareExchange(ref _transactionStatus, Status.Committed, Status.Active);
            return commited;
             * 
             */

#if DEBUG
                    Console.WriteLine("COMMITTED: "+ID);
#endif

            _transactionStatus = TransactionStatus.Committed;

            return true;
        }

        public void Abort()
        {
            /*
            lock (LockObject)
            {
                _transactionStatus = TransactionStatus.Aborted;
#if DEBUG
                Console.WriteLine("Transaction: " + ID + " aborted");
#endif
            }*/

            _transactionStatus = TransactionStatus.Aborted;
#if DEBUG
            Console.WriteLine("ABORTED: "+ID);
#endif
            WriteSet.Clear();
            ReadSet.Clear();

        }

        #endregion Status

    }
}
