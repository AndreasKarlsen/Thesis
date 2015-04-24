using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Evaluation.Common
{
    public class HashMap<K,V> : IHashMap<K,V>
    {
        private const int DefaultNrBuckets = 16;
        private const double LoadFactor = 0.75D;

        private Node[] _buckets;
        private int _threshold;

        public HashMap() : this(DefaultNrBuckets)
        {
            
        }

        public HashMap(int nrNuckets)
        {
            _buckets = MakeBuckets(nrNuckets);
            _threshold = CalulateThreshold(nrNuckets);
        }

        private int CalulateThreshold(int nrNuckets)
        {
            return (int)(nrNuckets * LoadFactor);
        }

    #region Utility

        private Node[] MakeBuckets(int nrBuckets)
        {
            return new Node[nrBuckets];
        }

        public bool ContainsKey(K key)
        {
            var bucket = GetBucket(key);
            return SearchBucket(bucket, key) != null;
        }

        private int GetHashCode(K key)
        {
            var hashCode = key.GetHashCode();
            return hashCode < 0 ? 0 - hashCode : hashCode;
        }

        private int GetBucketIndex(K key)
        {
            return GetBucketIndex(_buckets, key);
        }

        private int GetBucketIndex(Node[] buckets, K key)
        {
            var hasCode = GetHashCode(key);
            return hasCode % buckets.Length;
        }

        private Node GetBucket(K key)
        {
            var bucketIndex = GetBucketIndex(key);
            return _buckets[bucketIndex];
        }

        private Node GetBucket(int index)
        {
            return _buckets[index];
        }

        private void InsertInBucket(Node[] buckets, Node node, int index)
        {
            InsertInBucket(buckets, node, buckets[index], index, false);
        }

        private void InsertInBucket(Node node, int index)
        {
            InsertInBucket(node, _buckets[index], index);
        }

        private void InsertInBucket(Node node, Node curNode, int index)
        {
            InsertInBucket(_buckets,node,curNode,index,true);
        }

        private void InsertInBucket(Node[] buckets, Node node, Node curNode, int index, bool checkCondition)
        {
            if (curNode != null)
            {
                node.Next = curNode;
            }
            buckets[index] = node;
            

            if (checkCondition)
            {
                Size++;
                ResizeIfNeeded();
            }
           
        }

        private Node CreateNode(K key, V value)
        {
            return new Node(key,value);
        }

    #endregion Utility

        public V Get(K key)
        {
            var bucket = GetBucket(key);
            var node = SearchBucket(bucket, key);
            if (node == null)
            {
                throw new KeyNotFoundException("Key: "+key);
            }

            return node.Value;
        }

        public void Add(K key, V value)
        {
            var bucketIndex = GetBucketIndex(key);
            InsertInBucket(CreateNode(key, value), bucketIndex);
        }

        public bool AddIfAbsent(K key, V value)
        {
            var bucketIndex = GetBucketIndex(key);
            var bucket = GetBucket(bucketIndex);
            var node = SearchBucket(bucket, key);

            if (node != null) return false;

            InsertInBucket(CreateNode(key,value), bucket, bucketIndex);
            return true;
        }

        public bool Remove(K key)
        {
            var bucketIndex = GetBucketIndex(key);
            var node = GetBucket(bucketIndex);
            

            if (node == null)
            {
                return false;
            }
            else if (node.Key.Equals(key))
            {
                Size--;
                _buckets[bucketIndex] = node.Next;
                return true;
            }
            else
            {
                while (node.Next != null && !key.Equals(node.Next.Key))
                    node = node.Next;

                //node.Next == null || node.Next.Key == key
                if (node.Next == null) return false;

                Size--;
                node.Next = node.Next.Next;
                return true;
            }

        }

        private void ResizeIfNeeded()
        {
            if (Size >= _threshold)
            {
                Resize();
            }
        }

        private void Resize()
        {
            var newBucketSize = _buckets.Length * 2;
            var newBuckets = MakeBuckets(newBucketSize);
            for (int i = 0; i < _buckets.Length; i++)
            {
                var node = _buckets[i];
                while (node != null)
                {
                    var bucketIndex = GetBucketIndex(newBuckets, node.Key);
                    InsertInBucket(newBuckets, CreateNode(node.Key, node.Value), bucketIndex);

                    node = node.Next;
                }
            }

            _threshold = CalulateThreshold(newBucketSize);
            _buckets = newBuckets;
        }


        public V this[K key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }

        public int Size { get; private set; }

        private class Node
        {
            public Node(K key, V value)
            {
                this.Key = key;
                this.Value = value;
            }

            public K Key { get; set; }
            public V Value { get; set; }
            public Node Next { get; set; }
        }

        private static Node SearchBucket(Node node, K key) {
            while (node != null && !key.Equals(node.Key))
                node = node.Next;
            return node;
        }
    }
}
