using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;
using STM.Implementation.Lockbased;
using STM.Util;

namespace STM
{
    public class Transaction
    {
        public enum TransactionStatus { Aborted, Active, Committed };

        //Possibly use int or class ref instead of enum for compare and swap
        private TransactionStatus _transactionStatus;
        public long ID { get; private set; }
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
            var transaction = new Transaction(TransactionStatus.Active, VersionClock.TimeStamp);
          
#if DEBUG
            Console.WriteLine("STARTED: "+transaction.ID);
#endif
            Local.Value = transaction;
            return transaction;
        }

        internal static Transaction StartNestedTransaction(Transaction parent)
        {
            var transaction = new Transaction(TransactionStatus.Active, VersionClock.TimeStamp, parent);
#if DEBUG
            Console.WriteLine("STARTED NESTED: " + transaction.ID);
#endif
            Local.Value = transaction;
            return transaction;
        }

        private Transaction(TransactionStatus status, int readStmp)
        {
            Init(status, readStmp);
            WriteSet = new WriteSet();
        }

        private Transaction(TransactionStatus status,  int readStmp, Transaction parent)
        {
            Init(status, readStmp);
            Parent = parent;
            WriteSet = new WriteSet(parent.WriteSet);
        }


        private void Init(TransactionStatus status, int readStmp)
        {
#if DEBUG
            ID = IDGenerator.NextID;
#endif
            _transactionStatus = status;
            ReadStamp = readStmp;
            ReadSet = new ReadSet();
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
        private static readonly ThreadLocal<Transaction> Local = new ThreadLocal<Transaction>(() => new Transaction(TransactionStatus.Committed,VersionClock.TimeStamp));

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

        #endregion Status

        #region Commit

        public bool Commit()
        {
            if (!IsNested)
            {
                int writeStamp;
                if (!Validate(out writeStamp))
                    return false;

                HandleCommit(writeStamp);

            }
            else
            {
                if (!ValidateReadset())
                    return false;

                MergeWithParent();
                Transaction.LocalTransaction = Parent;
            }

#if DEBUG
            Console.WriteLine("COMMITTED: "+ID);
#endif

            Status = TransactionStatus.Committed;
            return true;

            
            if (!Validate(out writeStamp)) 
                return false;
            
            
            if (IsNested)
            {
                WriteSet.Unlock();
                MergeWithParent();
                Transaction.LocalTransaction = Parent;
            }
            else
            {
                HandleCommit(writeStamp);
            }


#if DEBUG
            Console.WriteLine("COMMITTED: "+ID);
#endif

            Status = TransactionStatus.Committed;
            return true;
        }

        public void Abort()
        {
            _transactionStatus = TransactionStatus.Aborted;
#if DEBUG
            Console.WriteLine("ABORTED: "+ID);
#endif
            WriteSet.Clear();
            ReadSet.Clear();

        }

        private void HandleCommit(int writeStamp)
        {
            foreach (var entry in WriteSet)
            {
                var lo = entry.Key;
                var value = entry.Value;
                lo.Commit(value, writeStamp);
            }

            WriteSet.Unlock();
        }


        public bool Validate(out int writeStamp)
        {
            writeStamp = -1;

            if (Status == TransactionStatus.Aborted 
                || !WriteSet.TryLock(STMSystem.TIME_OUT))
                return false;

            if (ValidateReadset())
            {
                writeStamp = VersionClock.IncrementClock();
                return true;
            }

            WriteSet.Unlock();
            return false;
        }


        public bool ValidateReadset()
        {
            return ReadSet.Validate(this);
        }


        #endregion Commit
    }
}
