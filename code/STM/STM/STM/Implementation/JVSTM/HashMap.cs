using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class HashMap<K,V>
    {
        protected const int DefaultNrBuckets = 16;
        protected const double LoadFactor = 0.75D;
        public int Count { get; protected set; }

        protected Node[] _buckets;
        private int _threshold;

        public HashMap() : this(DefaultNrBuckets)
        {
            
        }

        public HashMap(int nrBuckets)
        {
            _buckets = MakeBuckets(nrBuckets);
            _threshold = CalculateThreshold(nrBuckets);
        }

    #region Utility


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

        private Node[] MakeBuckets(int nrBuckets)
        {
            return new Node[nrBuckets];
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
                Count++;
                ResizeIfNeeded();
            }
           
        }

        private Node CreateNode(K key, V value)
        {
            return new Node(key,value);
        }

    #endregion Utility

        public bool ContainsKey(K key)
        {
            var bucket = GetBucket(key);
            return SearchBucket(bucket, key) != null;
        }

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
            var bucket = GetBucket(bucketIndex);
            var node = SearchBucket(bucket, key);

            if (node != null)
            {
                node.Value = value;
            }
            else
            {
                InsertInBucket(CreateNode(key, value), bucket, bucketIndex);
            }
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
                Count--;
                _buckets[bucketIndex] = node.Next;
                return true;
            }
            else
            {
                while (node.Next != null && !key.Equals(node.Next.Key))
                    node = node.Next;

                //node.Next == null || node.Next.Key == key
                if (node.Next == null) return false;

                Count--;
                node.Next = node.Next.Next;
                return true;
            }

        }

        private void ResizeIfNeeded()
        {
            if (Count >= _threshold)
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

            _threshold = CalculateThreshold(newBucketSize);
            _buckets = newBuckets;
        }


        public V this[K key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            for (int i = 0; i < _buckets.Length; i++)
            {
                var node = _buckets[i];
                while (node != null)
                {
                    yield return new KeyValuePair<K, V>(node.Key,node.Value);
                    node = node.Next;
                }
            }
        }

        protected class Node
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
