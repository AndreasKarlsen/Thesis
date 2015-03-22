using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.Exceptions
{
    public class STMMaxAttemptException : STMException
    {
        public STMMaxAttemptException()
        {
            
        }

        public STMMaxAttemptException(string message) : base(message)
        {
            
        }
    }
}
