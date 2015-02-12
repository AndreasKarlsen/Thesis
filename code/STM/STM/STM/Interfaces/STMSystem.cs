using System;

namespace STM.Interfaces
{
    public abstract class STMSystem
    {
        protected abstract bool OnValidate();

        protected virtual void OnCommit() {  }
        protected virtual void OnAbort() {  }

        public abstract T Atomic<T>(Func<T> stmAction);
        public abstract void Atomic(Action stmAction);
    }
}