using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Exceptions;
using STM.Implementation.Lockbased;

namespace STM.Implementation.JVSTM
{
    public class WriteMap : BaseMap<BaseVBox, object>
    {
        public WriteMap()
        {
            
        }

        public WriteMap(WriteMap other) : base(other)
        {
            
        }

        public void Merge(WriteMap other)
        {
            foreach (var kvpair in other)
            {
                Put(kvpair.Key, kvpair.Value);
            }
        }
    }
}
