using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Exceptions
{
    public class STMException : Exception
    {
        public STMException()
        {
            
        }

        public STMException(string message) : base(message)
        {
                
        }
    }
}
