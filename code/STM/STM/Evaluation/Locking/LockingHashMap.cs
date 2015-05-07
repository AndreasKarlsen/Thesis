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
    public class LockingHashMap<K,V> : BaseHashMap<K,V>
    {
        private const int DefaultNrLocks = DefaultNrBuckets;

        private readonly object _resizeLock = new object();
        private readonly object _sizeLock = new object();

        private readonly object[] _locks;
        private LinkedList<Node>[] _buckets;
        private int _size;
        private int _threshold;

        public LockingHashMap() : this(DefaultNrBuckets)
        {
            
        }

        public LockingHashMap(int size) : this(size, DefaultNrLocks)
        {
           
        }

        private LockingHashMap(int size, int nrLocks)
        {
            if (size % nrLocks != 0)
            {
                throw new Exception("The intital size % nrbuckets must be equal to zero");
            }
            _buckets = MakeBuckets(size);
            _locks = MakeLocks(nrLocks);
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

        private int GetLockIndex(int hashCode)
        {
            return hashCode % _locks.Length;
        }

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

        private Node FindNode(LinkedList<Node> bucket, K key)
        {
            return bucket.FirstOrDefault(n => n.Key.Equals(key));
        }

        private void LockAll()
        {
            foreach (var lo in _locks)
            {
                Monitor.Enter(lo);
            }
        }

        private void UnlockAll()
        {
            foreach (var lo in _locks)
            {
                Monitor.Exit(lo);
            }
        }

        #endregion Utility

        public override int Count {
            get {
                lock (_sizeLock)
                {
                    return _size;
                }
            }

            protected set
            {
                lock (_sizeLock)
                {
                    _size = value;
                }
            } 
        }

        public override bool ContainsKey(K key)
        {
            var hashCode = GetHashCode(key);
            lock (_locks[GetLockIndex(hashCode)])
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                return FindNode(bucket, key) != null;
            }
        }

        public override V Get(K key)
        {
            var hashCode = GetHashCode(key);
            lock (_locks[GetLockIndex(hashCode)])
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                var node = FindNode(bucket, key);

                if (node == null)
                {
                    //If node is null, key is not in map
                    throw new KeyNotFoundException("Key not found. Key: "+key);
                }

                return node.Value;
            }
        }

        public override void Add(K key, V value)
        {
            var hashCode = GetHashCode(key);
            lock (_locks[GetLockIndex(hashCode)])
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
                    lock (_sizeLock)
                    {
                        _size++;
                    }
                }
            }

            ResizeIfNeeded();
        }

        public override bool AddIfAbsent(K key, V value)
        {
            var hashCode = GetHashCode(key);
            lock (_locks[GetLockIndex(hashCode)])
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                var node = FindNode(bucket, key);

                if (node != null) return false;
                //If node is not in map insert new node
                bucket.AddFirst(CreateNode(key, value));
                lock (_sizeLock)
                {
                    _size++;
                }
                ResizeIfNeeded();
                return true;
            }

            
        }

        public override bool Remove(K key)
        {
            var hashCode = GetHashCode(key);
            lock (_locks[GetLockIndex(hashCode)])
            {
                var bucket = _buckets[GetBucketIndex(hashCode)];
                var node = FindNode(bucket, key);

                if (node != null)
                {
                    bucket.Remove(node);
                    lock (_sizeLock)
                    {
                        _size--;
                    }
                    return true;
                }

                return false;
            }
        }

        private void ResizeIfNeeded()
        {
            if (ResizeCondtion())
            {
                LockAll();
                try
                {
                    if (!ResizeCondtion())
                    {
                        return;
                    }
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
                finally
                {
                    UnlockAll();
                }
            }
            /*
            // If the lock can not be acquired imedietly then some other thread is checking the resize condition or resizing the array
            // In that case there is not need to proceed as that other thread will resize the array if it is needed
            if (Monitor.TryEnter(_resizeLock, 0))
            {
                try
                {
                    bool condition;
                    lock (_sizeLock)
                    {
                        condition = _size >= _threshold;
                    }

                    if (condition)
                    {
                        LockAll();
                        try
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
                        finally
                        {
                            UnlockAll();
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_resizeLock);
                }
            }*/
        }

        private bool ResizeCondtion()
        {
            lock (_sizeLock)
            {
                return _size >= _threshold;
            }
        }

        public override V this[K key]
        {
            get { return Get(key); }
            set { Add(key,value); }
        }

        public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            LockAll();
            try
            {
                var kvPairs = from bucket in _buckets
                    from node in bucket
                    select new KeyValuePair<K, V>(node.Key, node.Value);

                return kvPairs.ToList().GetEnumerator();
            }
            finally
            {
                UnlockAll();
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
