using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using STM.Implementation.Common;
using STM.Implementation.Lockbased;

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
        internal readonly IRetryLatch RetryLatch = new RetryLatch();
        public TransactionStatus Status { get; internal set; }

        internal bool IsNested
        {
            get { return Parent != null; }
        }


        private JVTransaction(int id) : this(TransactionStatus.Active, id,null,new ReadMap(), new WriteMap())
        {

        }

        private JVTransaction()
            : this(TransactionStatus.Active, _lastCommitted, null, new ReadMap(), new WriteMap())
        {

        }

         private JVTransaction(TransactionStatus status)
            : this(status, _lastCommitted, null, new ReadMap(), new WriteMap())
        {

        }

        private JVTransaction(int id, JVTransaction parent)
            : this(TransactionStatus.Active, id, parent, new ReadMap(), new WriteMap())
        {

        }

        private JVTransaction(TransactionStatus status, int id, JVTransaction parent, ReadMap readMap, WriteMap writeMap)
        {
            Number = id;
            Parent = parent;
            ReadMap = readMap;
            WriteMap = writeMap;
            Status = status;
        }

        public static JVTransaction Start()
        {
            return new JVTransaction(_lastCommitted);
        }

        public static JVTransaction StartNested(JVTransaction parent)
        {
            return new JVTransaction(TransactionStatus.Active, _lastCommitted, parent, new ReadMap(), new WriteMap(parent.WriteMap));
        }

        public bool Commit()
        {
            lock (CommitLock)
            {
                var newNumber = _lastCommitted + 1;
                if (WriteMap.Count != 0)
                {
                    var valid = ReadMap.Validate();
                    if (!valid)
                    {
                        return false;
                    }

                    if (IsNested)
                    {
                        Parent.ReadMap.Merge(ReadMap);
                        Parent.WriteMap.Merge(WriteMap);
                    }
                    else
                    {
                        foreach (var kvpair in WriteMap)
                        {
                            kvpair.Key.Install(kvpair.Value, newNumber);
                        }
                    }
                }

                _lastCommitted = newNumber;
                return true;
            }
        }

        public void Await(int expectedEra)
        {
            RetryLatch.Await(expectedEra);
        }

    }
}
