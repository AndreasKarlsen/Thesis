using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Implementation.Exceptions;
using STM.Interfaces;

namespace STM.Implementation.Obstructionfree
{

    public class FreeStmSystem : BaseSTMSystem
    {
        private static readonly Lazy<FreeStmSystem> LazySystem = new Lazy<FreeStmSystem>(true);

        protected override bool OnValidate()
        {
            Transaction me = Transaction.LocalTransaction;
            return me.Status != Transaction.TransactionStatus.Aborted;
        }

        public static BaseSTMSystem GeInstance()
        {
            return LazySystem.Value;
        }

        public override  T Atomic<T>(Func<T> stmAction)
        {
            var result = default(T);
            while (true)
            {
                var me = Transaction.StartTransaction();
                Transaction.LocalTransaction = me;
                try
                {
                    result = stmAction();
                }
                catch (ThreadAbortException){}
                catch (STMException) { }
                catch (Exception ex)
                {
                    Debug.Assert(false,"STM fail: "+ex.Message);
                }

                if (OnValidate())
                {
                    if (me.Commit())
                    {
                        OnCommit();
                        return result;
                    }
                }
                me.Abort();
                OnAbort();
            }
        }

        public override void Atomic(Action stmAction)
        {
            Atomic(() =>
            {
                stmAction();
                return true;
            });
        }
    }
}
