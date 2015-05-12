using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class BaseMap<K,V> : Dictionary<K,V>
    {

        public bool Contains(K stmObject)
        {
            return ContainsKey(stmObject);
        }

        public void Put(K stmObject, V value)
        {
            this[stmObject] = value;
        }


    }
}
