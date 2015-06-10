using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Common;

namespace STM.Implementation.JVSTM
{
    public abstract class BaseVBox
    {
        internal abstract bool Validate(BaseVBoxBody readBody);

        internal abstract BaseVBoxBody Install(object value, int version);

        internal abstract void RegisterRetryLatch(IRetryLatch latch, BaseVBoxBody expectedBody, int expectedEra);

        internal abstract BaseVBoxBody Commit(object value, int version);

    }
}
