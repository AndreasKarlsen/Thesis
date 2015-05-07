using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;

namespace Evaluation.Locking
{
    public class NaiveLockingHashMap<K, V> : BaseHashMap<K, V>
    {
        private const int DefaultNrLocks = DefaultNrBuckets;
        protected readonly object _lock = new object();

        private LinkedList<Node>[] _buckets;
        private int _size;
        private int _threshold;

        public NaiveLockingHashMap()
            : this(DefaultNrBuckets)
        {

        }

        public NaiveLockingHashMap(int size)
            : this(size, DefaultNrLocks)
        {

        }

        private NaiveLockingHashMap(int size, int nrLocks)
        {
            if (size % nrLocks != 0)
            {
                throw new Exception("The intital size % nrbuckets must be equal to zero");
            }
            _buckets = MakeBuckets(size);
            _threshold = CalculateThreshold(size);
        }


        private LinkedList<Node>[] MakeBuckets(int nrBuckets)
        {
            var temp = new LinkedList<Node>[nrBuckets];
            for (var i = 0; i < nrBuckets; i++)
            {
                temp[i] = new LinkedList<Node>();
            }

            return temp;
        }

        private object[] MakeLocks(int nrLocks)
        {
            var temp = new object[nrLocks];
            for (var i = 0; i < nrLocks; i++)
            {
                temp[i] = new object();
            }

            return temp;
        }

        #region Utility

        private Node CreateNode(K key, V value)
        {
            return new Node(key, value);
        }

        private int GetBucketIndex(K key)
        {
            return GetBucketIndex(_buckets.Length, key);
        }

        private int GetBucketIndex(int hashCode)
        {
            return GetBucketIndex(_buckets.Length, hashCode);
        }

        private Node FindNode(int bucketIndex, K key)
        {
            return FindNode(_buckets[bucketIndex], key);
        }

        private Node FindNode(IEnumerable<Node> bucket, K key)
        {
            return bucket.FirstOrDefault(n => n.Key.Equals(key));
        }



        #endregion Utility

        public override int Count
        {
            get
            {
                lock (_lock)
                {
                    return _size;
                }
            }

            protected set
            {
                lock (_lock)
                {
                    _size = value;
                }
            }
        }

        public override bool ContainsKey(K key)
        {
            var hashCode = GetHashCode(key);
            lock (_lock)
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                return FindNode(bucket, key) != null;
            }
        }

        public override V Get(K key)
        {
            var hashCode = GetHashCode(key);
            lock (_lock)
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                var node = FindNode(bucket, key);

                if (node == null)
                {
                    //If node is null, key is not in map
                    throw new KeyNotFoundException("Key not found. Key: " + key);
                }

                return node.Value;
            }
        }

        public override void Add(K key, V value)
        {
            var hashCode = GetHashCode(key);
            lock (_lock)
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                var node = FindNode(bucket, key);

                if (node != null)
                {
                    //If node is not null, key exist in map. Update the value
                    node.Value = value;
                }
                else
                {
                    //Else insert the node
                    bucket.AddFirst(CreateNode(key, value));
                    _size++;
                    ResizeIfNeeded();
                }
            }
        }

        public override bool AddIfAbsent(K key, V value)
        {
            var hashCode = GetHashCode(key);
            lock (_lock)
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                var node = FindNode(bucket, key);

                if (node != null) return false;
                //If node is not in map insert new node
                bucket.AddFirst(CreateNode(key, value));
                _size++;
                ResizeIfNeeded();
                return true;
            }
        }

        public override bool Remove(K key)
        {
            var hashCode = GetHashCode(key);
            lock (_lock)
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                var node = FindNode(bucket, key);

                if (node != null)
                {
                    bucket.Remove(node);
                    _size--;
                    return true;
                }

                return false;
            }
        }

        private void ResizeIfNeeded()
        {
            if (_size >= _threshold)
            {
                //Construct new backing array
                var newBucketSize = _buckets.Length * 2;
                var newBuckets = MakeBuckets(newBucketSize);

                //For each key in the map rehash
                foreach (var bucket in _buckets)
                {
                    foreach (var node in bucket)
                    {
                        var bucketIndex = GetBucketIndex(newBucketSize, node.Key);
                        newBuckets[bucketIndex].AddFirst(node);
                    }
                }

                //Calculate new resize threshold and assign the rehashed backing array
                _threshold = CalculateThreshold(newBucketSize);
                _buckets = newBuckets;

            }
        }

        public override V this[K key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }

        public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            lock (_lock)
            {
                var kvPairs = from bucket in _buckets
                              from node in bucket
                              select new KeyValuePair<K, V>(node.Key, node.Value);

                return kvPairs.GetEnumerator();
            }
        }

        private class Node
        {
            public K Key { get; private set; }
            public V Value { get; internal set; }

            public Node(K key, V value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
