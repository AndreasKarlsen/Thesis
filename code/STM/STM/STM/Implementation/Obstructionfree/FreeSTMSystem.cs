using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Exceptions;
using STM.Interfaces;

namespace STM.Implementation.Obstructionfree
{

    public class FreeSTMSystem : STMSystem
    {
        private static readonly Lazy<FreeSTMSystem> LazySystem = new Lazy<FreeSTMSystem>(true);

        protected override bool OnValidate()
        {
            Transaction me = Transaction.GetLocal();
            return me.GetStatus() != Transaction.Status.Aborted;
        }

        public static STMSystem GeInstance()
        {
            return LazySystem.Value;
        }

        public override  T Atomic<T>(Func<T> stmAction)
        {
            T result = default(T);
            while (true)
            {
                var me = new Transaction();
                Transaction.SetLocal(me);
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
