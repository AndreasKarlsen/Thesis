using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Exceptions
{
    public class STMAbortException : STMException
    {
        public STMAbortException()
        {
            
        }

        public STMAbortException(string message)
            : base(message)
        {

        }
    }
}
