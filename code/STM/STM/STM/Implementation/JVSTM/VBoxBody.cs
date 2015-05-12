using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    class VBoxBody<T> : BaseVBoxBody
    {
        public readonly T Value;
        public readonly int Version;
        public readonly VBoxBody<T> Next;

        public VBoxBody(T value, int version, VBoxBody<T> next)
        {
            Value = value;
            Version = version;
            Next = next;
        }
    }
}
