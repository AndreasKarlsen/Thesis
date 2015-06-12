using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Common;

namespace STMTester
{
    public class InstrumentedDictionary<K, V> : BaseHashMap<K, V>
    {
        private readonly IDictionary<K, V> _dictionary;

        public InstrumentedDictionary()
        {
            _dictionary = new ConcurrentDictionary<K, V>();
        } 

        public override bool ContainsKey(K key)
        {
            return _dictionary.ContainsKey(key);
        }

        public override V Get(K key)
        {
            V value;
            _dictionary.TryGetValue(key, out value);
            return value;
        }

        public override void Add(K key, V value)
        {
            _dictionary[key] = value;
        }

        public override bool AddIfAbsent(K key, V value)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(K k)
        {
            return _dictionary.Remove(k);
        }

        public override V this[K key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}
