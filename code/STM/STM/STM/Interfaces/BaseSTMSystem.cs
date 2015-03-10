using System;
using STM.Exceptions;

namespace STM.Interfaces
{
    public abstract class BaseSTMSystem
    {
        protected abstract bool OnValidate();

        protected virtual void OnCommit() {  }
        protected virtual void OnAbort() {  }

        public virtual void Retry()
        {
            throw new STMRetryException();
        }
        public abstract T Atomic<T>(Func<T> stmAction);
        public abstract void Atomic(Action stmAction);
    }
}