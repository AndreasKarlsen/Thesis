using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class AtomicBool
    {
        private volatile bool _value;

        public bool Value { get { return _value; } set { _value = value; } }
    }
}
