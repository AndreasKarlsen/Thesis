using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using STM.Implementation.Exceptions;

namespace STM.Implementation.JVSTM
{
    public static class JVSTMSystem
    {
        /// <summary>
        /// Max attempts. Set to same as Clojure
        /// </summary>
        internal static readonly int MAX_ATTEMPTS = 10000;

        #region Atomic

        public static T Atomic<T>(Func<JVTransaction,T> stmAction)
        {
            return AtomicInternal(new List<Func<JVTransaction,T>> { stmAction });
        }
        /*
        public static T Atomic<T>(Func<T> stmAction, params Func<T>[] orElses)
        {
            var atomics = new List<Func<T>>(orElses.Length + 1) { stmAction };
            atomics.AddRange(orElses);
            return AtomicInternal(atomics);
        }*/

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

        /*
        public static void Atomic(Action stmAction, params Action[] orElses)
        {
            var atomics = new List<Func<bool>>(orElses.Length + 1)
            {
                () =>
                {
                    stmAction();
                    return true;
                }
            };

            //Convert orElseblocks from Action to Func<bool>
            atomics.AddRange(orElses.Select<Action, Func<bool>>(orElseBlcok =>
                () =>
                {
                    orElseBlcok();
                    return true;
                }
            ));

            AtomicInternal(atomics);
        }*/

        private static T AtomicInternal<T>(IList<Func<JVTransaction,T>> stmActions)
        {
            Debug.Assert(stmActions.Count > 0);
            var result = default(T);
            var index = 0;
            var nrAttempts = 0;

            while (true)
            {
                var stmAction = stmActions[index];

                var transaction = JVTransaction.StartNew();

                try
                {
                    //Execute transaction body
                    result = stmAction(transaction);

                    if (transaction.Commit())
                    {
                        return result;
                    }

                }
                catch (Exception) //Catch non stm related exceptions which occurs in transactions
                {
                    //Throw exception of transaction can commit
                    //Else abort and rerun transaction
                    if (transaction.Commit())
                    {
                        throw;
                    }
                }

                nrAttempts++;
                //transaction.Abort();


                if (nrAttempts == MAX_ATTEMPTS)
                {
                    throw new STMMaxAttemptException("Fatal error: max attempts reached");
                }

            }
        }


        #endregion Atomic   

    }
}
