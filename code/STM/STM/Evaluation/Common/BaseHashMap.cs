using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluation.Common
{
    public abstract class BaseHashMap<K,V> : IMap<K,V>
    {
        protected const int DefaultNrBuckets = 16;
        protected const double LoadFactor = 0.75D;

        public abstract bool ContainsKey(K key);
        public abstract V Get(K key);
        public abstract void Add(K key, V value);
        public abstract bool AddIfAbsent(K key, V value);
        public abstract bool Remove(K k);
        public abstract V this[K key] { get; set; }
        public virtual int Count {  get; protected set; }

        protected int CalculateThreshold(int nrBuckets)
        {
            return (int)(nrBuckets * LoadFactor);
        }

        protected int GetHashCode(K key)
        {
            var hashCode = key.GetHashCode();
            return hashCode < 0 ? 0 - hashCode : hashCode;
        }

        protected int GetBucketIndex(int length, K key)
        {
            return GetBucketIndex(length, GetHashCode(key));
        }

        protected int GetBucketIndex(int length, int hashCode)
        {
            return hashCode % length;
        }


        public abstract IEnumerator<KeyValuePair<K, V>> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
