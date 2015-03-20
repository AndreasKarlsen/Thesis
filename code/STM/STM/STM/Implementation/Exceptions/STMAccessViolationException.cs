using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.Exceptions
{
    public class STMAccessViolationException : STMException
    {
        public STMAccessViolationException(string message) : base(message)
        {
            
        }
    }
}
