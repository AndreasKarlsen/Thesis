using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STM.Implementation.Common;

namespace STM.Implementation.JVSTM
{
    public class JVTransaction
    { 
        private static volatile int lastCommitted = 0;
        private static readonly object _commitLock = new object();

        public int Number { get; private set; }
        internal JVTransaction Parent { get; private set; }
        internal ReadMap ReadMap { get; private set; }
        internal WriteMap WriteMap { get; private set; }
        internal readonly IRetryLatch RetryLatch = new RetryLatch();


        private JVTransaction(int id) : this(id,null,new ReadMap(), new WriteMap())
        {

        }

        private JVTransaction(int id, JVTransaction parent)
            : this(id, parent, new ReadMap(), new WriteMap())
        {

        }

        private JVTransaction(int id, JVTransaction parent, ReadMap readMap, WriteMap writeMap)
        {
            Number = id;
            Parent = parent;
            ReadMap = readMap;
            WriteMap = writeMap;
        }

        public static JVTransaction StartNew()
        {
            return new JVTransaction(lastCommitted);
        }

        public bool Commit()
        {
            lock (_commitLock)
            {
                var newNumber = lastCommitted + 1;
                if (WriteMap.Count != 0)
                {
                    var valid = ReadMap.Validate();
                    if (!valid)
                    {
                        return false;
                    }

                    foreach (var kvpair in WriteMap)
                    {
                        kvpair.Key.Install(kvpair.Value, newNumber);
                    }
                }

                lastCommitted = newNumber;
                return true;
            }
        }

        public void Await(int expectedEra)
        {
            RetryLatch.Await(expectedEra);
        }

    }
}
