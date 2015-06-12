using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class ReadMap : BaseMap<BaseVBox, BaseVBoxBody>
    {
        public bool Validate()
        {
            foreach (var kvpair in this)
            {
                if (!kvpair.Key.Validate(kvpair.Value))
                {
                    return false;
                }
            }

            return true;

            //return this.All(kvPair => kvPair.Key.Validate(kvPair.Value));
        }

        public void Merge(ReadMap other)
        {
            foreach (var kvpair in other)
            {
                PutIfAbsent(kvpair.Key,kvpair.Value);
            }
        }
    }
}
