using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using STM.Implementation.Common;
using STM.Implementation.Lockbased;
using STM.Implementation.Exceptions;

namespace STM.Implementation.JVSTM
{
    public class JVTransaction
    {
        /// <summary>
        /// Thread local storage. Each thread gets has its own value
        /// </summary>
        private static readonly ThreadLocal<JVTransaction> Local = new ThreadLocal<JVTransaction>(() => new JVTransaction(TransactionStatus.Committed));

        internal static JVTransaction LocalTransaction
        {
            get { return Local.Value; }
            set { Local.Value = value; }
        }

        private static volatile int _lastCommitted = 0;
        private static readonly object CommitLock = new object();

        public int Number { get; private set; }
        internal JVTransaction Parent { get; private set; }
        internal ReadMap ReadMap { get; private set; }
        internal WriteMap WriteMap { get; private set; }
        internal IList<BaseCommute> Commutes { get; private set; }
        internal readonly IRetryLatch RetryLatch = new RetryLatch();
        public TransactionStatus Status { get; internal set; }
        private readonly ActiveTxnRecord _txnRecord;
        private ActiveTxnRecord _commitTxnRecord;

        internal bool IsNested
        {
            get { return Parent != null; }
        }


        private JVTransaction(ActiveTxnRecord txnRecord) : this(TransactionStatus.Active, txnRecord,null,new ReadMap(), new WriteMap())
        {

        }

        private JVTransaction()
            : this(TransactionStatus.Active, ActiveTxnRecord.StartTransaction(), null, new ReadMap(), new WriteMap())
        {

        }

         private JVTransaction(TransactionStatus status)
            : this(status, 0, null, new ReadMap(), new WriteMap())
        {

        }

         private JVTransaction(TransactionStatus status, int id, JVTransaction parent, ReadMap readMap, WriteMap writeMap)
         {
             Number = id;
             Parent = parent;
             ReadMap = readMap;
             WriteMap = writeMap;
             Status = status;
             //Commutes = new List<BaseCommute>();
         }

        private JVTransaction(TransactionStatus status, ActiveTxnRecord txnRecord, JVTransaction parent, ReadMap readMap, WriteMap writeMap)
        {
            Number = txnRecord.TxNumber;
            Parent = parent;
            ReadMap = readMap;
            WriteMap = writeMap;
            Status = status;
            _txnRecord = txnRecord;
            //Commutes = new List<BaseCommute>();
        }

        public static JVTransaction Start()
        {
            return new JVTransaction(ActiveTxnRecord.StartTransaction());
        }

        public static JVTransaction StartNested(JVTransaction parent)
        {
            return new JVTransaction(TransactionStatus.Active, parent.Number, parent, new ReadMap(), new WriteMap(parent.WriteMap));
        }

        public bool Commit()
        {
            return CommitLockFree();


            if (WriteMap.Count == 0)
            {
                Status = TransactionStatus.Committed;
                _txnRecord.FinishTransaction();
                return true;
            }

            bool result;
            ActiveTxnRecord commitRecord = null;
            lock (CommitLock)
            {
                var newNumber = _lastCommitted + 1;

                var valid = ReadMap.Validate();
                if (!valid)
                {
                    result = false;
                }
                else
                {
                    if (IsNested)
                    {
                        Parent.ReadMap.Merge(ReadMap);
                        Parent.WriteMap.Merge(WriteMap);
                    }
                    else
                    {
                        var bodies = new BaseVBoxBody[WriteMap.Count];
                        var i = 0;
                        foreach (var kvpair in WriteMap)
                        {
                            bodies[i] = kvpair.Key.Install(kvpair.Value, newNumber);
                            i++;
                        }

                        commitRecord = new ActiveTxnRecord(newNumber, bodies);
                        ActiveTxnRecord.InsertNewRecord(commitRecord);
                    }

                    _lastCommitted = newNumber;
                    Status = TransactionStatus.Committed;
                    result = true;
                }
            }
            
            if (result && !IsNested)
            {
                _txnRecord.FinishTransaction();

                if (commitRecord != null)
                {
                    Interlocked.Decrement(ref commitRecord.Running);
                }
            }

            return result;;
        }

        public bool CommitLockFree()
        {
            if (WriteMap.Count == 0)
            {
                Status = TransactionStatus.Committed;
                //_txnRecord.FinishTransaction();
                return true;
            }

            if (IsNested)
            {
                Parent.ReadMap.Merge(ReadMap);
                Parent.WriteMap.Merge(WriteMap);
                Status = TransactionStatus.Committed;
                return true;
            }

            try
            {
                WriteMap.PrepareCommit();
                ValidateCommitAndEnqueue();
                EnsureCommitStatus();
                Status = TransactionStatus.Committed;

                //_txnRecord.FinishTransaction();
                //Interlocked.Decrement(ref _commitTxnRecord.Running);

                return true;
            }
            catch (STMCommitException)
            {
                return false;
            }

        }

        public void ValidateCommitAndEnqueue()
        {
            ActiveTxnRecord lastValid = _txnRecord;
            do
            {
                lastValid = Validate(lastValid);
                _commitTxnRecord = new ActiveTxnRecord(lastValid.TxNumber + 1,
                this.WriteMap);
            } while (!lastValid.TrySetNext(_commitTxnRecord));
        }

        private ActiveTxnRecord Validate(ActiveTxnRecord record)
        {
            var next = record.Next;
            if (next != null)
            {
                return ValidateInternal(next);
            }

            return record;
 
        }

        private ActiveTxnRecord ValidateInternal(ActiveTxnRecord record)
        {
            var valid = record.WriteMap.Validate(this.ReadMap);
            if (!valid)
            {
                throw new STMCommitException();
            }

            var next = record.Next;
            if (next != null)
            {
                return Validate(next);
            }

            return record;
        }

        private void EnsureCommitStatus()
        {
            ActiveTxnRecord recToCommit = ActiveTxnRecord.First.Next;
            while (recToCommit != null && recToCommit.TxNumber <=  _commitTxnRecord.TxNumber)
            {
                if (!recToCommit.IsCommited)
                {
                    var writeMap = recToCommit.WriteMap;
                    writeMap.HelpWriteBack(recToCommit.TxNumber);
                    ActiveTxnRecord.FinishCommit(recToCommit);
                }
                recToCommit = recToCommit.Next;
            }
        }


        public void Abort()
        {
            Status = TransactionStatus.Aborted;
            if (_txnRecord != null)
            {
                //Interlocked.Decrement(ref _txnRecord.Running);
            } 
        }

        public void Await(int expectedEra)
        {
            RetryLatch.Await(expectedEra);
        }

    }
}
