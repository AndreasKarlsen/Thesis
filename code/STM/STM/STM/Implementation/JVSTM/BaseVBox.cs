using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public abstract class BaseVBox
    {
        internal abstract bool Validate(BaseVBoxBody readBody);

        internal abstract void Install(object value, int version);
    }
}
