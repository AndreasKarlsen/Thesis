using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    internal abstract class BaseCommute
    {
        public abstract void Perform(int version);
    }
}
