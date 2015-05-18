using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class BaseMap<K,V> : Dictionary<K,V>
    {
        public BaseMap()
        {
            
        }

        public BaseMap(IDictionary<K, V> other) : base(other)
        {
            
        }

        public bool Contains(K stmObject)
        {
            return ContainsKey(stmObject);
        }

        public void Put(K stmObject, V value)
        {
            this[stmObject] = value;
        }

        public void PutIfAbsent(K stmObject, V value)
        {
            if (!ContainsKey(stmObject))
            {
                this[stmObject] = value;
            }
        }




    }
}
