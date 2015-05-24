using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class VBoxBody<T> : BaseVBoxBody
    {
        public readonly T Value;
        public readonly int Version;
        public VBoxBody<T> Next;
        public VBoxBody<T> Previous;

        internal VBoxBody(T value, int version)
        {
            Value = value;
            Version = version;
        }

        internal VBoxBody(T value, int version, VBoxBody<T> next)
            : this(value, version)
        {
            Next = next;
            next.Previous = this;
        }

        internal override void Clean()
        {
            Next = null;
        }
    }
}
