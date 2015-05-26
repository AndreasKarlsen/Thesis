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

        private readonly object _sizeLock = new object();

        private readonly object[] _locks;
        private Node[] _buckets;
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


        private Node[] MakeBuckets(int nrBuckets)
        {
            return new Node[nrBuckets]; ;
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

        private int GetBucketIndex(int hashCode)
        {
            return GetBucketIndex(_buckets.Length, hashCode);
        }


        private Node FindNode(Node node, K key)
        {
            while (node != null && !key.Equals(node.Key))
                node = node.Next;
            return node;
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

        private void InsertInBucket(Node[] buckets, Node node, int index)
        {
            InsertInBucket(buckets, node, buckets[index], index);
        }

        private void InsertInBucket(Node node, int index)
        {
            InsertInBucket(node, _buckets[index], index);
        }

        private void InsertInBucket(Node node, Node curNode, int index)
        {
            InsertInBucket(_buckets, node, curNode, index);
        }

        private void InsertInBucket(Node[] buckets, Node node, Node curNode, int index)
        {
            if (curNode != null)
            {
                node.Next = curNode;
            }
            buckets[index] = node;
        }

        #endregion Utility

        public override int Count {
            get {
                lock (_sizeLock)
                {
                    return _size;
                }
            }

            protected set{
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
                var index = GetBucketIndex(hashCode);
                var bucket = _buckets[index];
                var node = FindNode(bucket, key);

                if (node != null)
                {
                    //If node is not null, key exist in map. Update the value
                    node.Value = value;
                }
                else
                {
                    //Else insert the node
                    InsertInBucket(CreateNode(key, value),bucket,index);
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
                var index = GetBucketIndex(hashCode);
                var bucket = _buckets[index];
                var node = FindNode(bucket, key);

                if (node != null) return false;
                //If node is not in map insert new node
                InsertInBucket(CreateNode(key, value), bucket, index);
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
                var index = GetBucketIndex(hashCode);
                var bucket = _buckets[index];

                return RemoveNode(key, bucket, index);
            }
        }

        private bool RemoveNode(K key, Node node, int index)
        {
            if (node == null)
            {
                return false;
            }
            
            if (node.Key.Equals(key))
            {
                lock (_sizeLock)
                {
                    _size--;
                }
                _buckets[index] = node.Next;
                return true;
            }


            while (node.Next != null && !key.Equals(node.Next.Key))
                node = node.Next;

            //node.Next == null || node.Next.Key == key
            if (node.Next == null) return false;

            lock (_sizeLock)
            {
                _size--;
            }
            node.Next = node.Next.Next;
            return true;
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
                    for (var i = 0; i < _buckets.Length; i++)
                    {
                        var node = _buckets[i];
                        while (node != null)
                        {
                            var bucketIndex = GetBucketIndex(newBucketSize, node.Key);
                            InsertInBucket(newBuckets,CreateNode(node.Key,node.Value),bucketIndex);
                            node = node.Next;
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
                var list = new List<KeyValuePair<K,V>>(_size);
                for (var i = 0; i < _buckets.Length; i++)
                {
                    var node = _buckets[i];
                    while (node != null)
                    {
                        list.Add(new KeyValuePair<K, V>(node.Key, node.Value));
                        node = node.Next;
                    }
                }

                return list.GetEnumerator();
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
            public Node Next { get; internal set; }

            public Node(K key, V value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}
