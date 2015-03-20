using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Spring.Threading.AtomicTypes;
using STM.Implementation.Exceptions;

namespace STM.Interfaces
{
    public abstract class STMObject<T> : BaseSTMObject where T : class, ICopyable<T>, new() 
    {
        protected Type Type;
        protected AtomicReference<T> Value;

        protected STMObject(T value)
        {
            Type = typeof (T);
            Value = new AtomicReference<T>(value);
        }

        protected abstract T OpenRead();
        protected abstract T OpenWrite();
        public abstract bool Validate();

        public virtual void SetValue(T newValue)
        {
            throw new STMAccessViolationException("Attempted to set the value of a STM variable outside of a transaction.");
        }

        public virtual T GetValue()
        {
            throw new STMAccessViolationException("Attempted to get the value of a STM variable outside of a transaction.");
        }
    }
}
