using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.Exceptions
{
    public class STMRetryException : STMException
    {
        public STMRetryException()
        {

        }

        public STMRetryException(string message)
            : base(message)
        {
                
        }
    }
}
