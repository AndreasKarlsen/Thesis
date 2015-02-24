using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Exceptions
{
    public class STMInvalidRetryException : STMException
    {
        public STMInvalidRetryException() : base("Attempted to execute a retry on a empty readset.")
        {

        }
    }
}
