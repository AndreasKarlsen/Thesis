using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;
using Evaluation.Common;

namespace Evaluation.Library
{
    public class STMHashMapInternalList<K,V> : BaseHashMap<K,V>
    {
         //TMVar to (array of TMVars to (ImmutableList of nodes) )
        private readonly TMVar<TMVar<Node>[]> _buckets = new TMVar<TMVar<Node>[]>(); 
        private readonly TMInt _threshold = new TMInt();
        private TMInt _size = new TMInt();

        public STMHashMapInternalList() : this(DefaultNrBuckets)
        {
            
        }

        public STMHashMapInternalList(int nrBuckets)
        {
            _buckets.Value = MakeBuckets(nrBuckets);
            _threshold.Value = CalculateThreshold(nrBuckets);
        }

        /// <summary>
        /// Creates and initializes the backing array
        /// </summary>
        /// <param name="nrBuckets"></param>
        /// <returns></returns>
        private TMVar<Node>[] MakeBuckets(int nrBuckets)
        {
            var temp = new TMVar<Node>[nrBuckets];
            for (var i = 0; i < nrBuckets; i++)
            {
                temp[i] = new TMVar<Node>(); 
            }

            return temp;
        }


        #region Utility

        private Node CreateNode(K key, V value)
        {
            return new Node(key,value);
        }

        private int GetBucketIndex(K key)
        {
            return GetBucketIndex(_buckets.Value.Length, key);
        }

        private Node FindNode(K key)
        {
            return FindNode(key, GetBucketIndex(key));
        }

        private Node FindNode(K key, int bucketIndex)
        {
            return FindNode(key, _buckets.Value[bucketIndex].Value);
        }

        private Node FindNode(K key, Node node)
        {
            while (node != null && !key.Equals(node.Key))
                node = node.Next.Value;
            return node;
        }

        private void InsertInBucket(TMVar<Node> bucketVar, Node node)
        {
            var curNode = bucketVar.Value;
            if (curNode != null)
            {
                node.Next.Value = curNode;
            }
            bucketVar.Value = node;
        }

        #endregion Utility

        public override bool ContainsKey(K key)
        {
            return STMSystem.Atomic(() => FindNode(key) != null);
        }

        public override V Get(K key)
        {
            return STMSystem.Atomic(() =>
            {
                var node = FindNode(key);
                if (node == null)
                {
                    //If node == null key is not present in dictionary
                    throw new KeyNotFoundException("Key not found. Key: " + key);
                }

                return node.Value.Value;
            });
        }

        public override void Add(K key, V value)
        {
            STMSystem.Atomic(() =>
            {
                var bucketIndex = GetBucketIndex(key);
                //TMVar wrapping the immutable chain list
                var bucketVar = _buckets.Value[bucketIndex];
                var node = FindNode(key, bucketVar.Value);
                
                if (node != null)
                {
                    //If node is not null key exist in map. Update the value
                    node.Value.Value = value;
                }
                else
                {
                    //Else insert the node
                    InsertInBucket(bucketVar, CreateNode(key,value));
                    _size++;
                    ResizeIfNeeded();
                }
            });
        }

        public override bool AddIfAbsent(K key, V value)
        {
            return STMSystem.Atomic(() =>
            {
                var bucketIndex = GetBucketIndex(key);
                //TMVar wrapping the immutable chain list
                var bucketVar = _buckets.Value[bucketIndex];
                var node = FindNode(key, bucketVar.Value);

                if (node == null)
                {
                    //If node is not found key does not exist so insert
                    InsertInBucket(bucketVar, CreateNode(key,value));
                    _size++;
                    ResizeIfNeeded();
                    return true;
                }

                return false;
            });
        }

        private void ResizeIfNeeded()
        {
            if (_size.Value >= _threshold.Value)
            {
                Resize();
            }
        }

        private void Resize()
        {
            //Construct new backing array
            var newBucketSize = _buckets.Value.Length * 2;
            var newBuckets = MakeBuckets(newBucketSize);

            //For each key in the map rehash
            for (var i = 0; i < _buckets.Value.Length; i++)
            {
                var bucket = _buckets.Value[i];
                var node = bucket.Value;
                while (node != null)
                {
                    var bucketIndex = GetBucketIndex(newBucketSize, node.Key);
                    InsertInBucket(newBuckets[bucketIndex], CreateNode(node.Key, node.Value));
                    node = node.Next.Value;
                }
            }

            //Calculate new resize threshold and assign the rehashed backing array
            _threshold.Value = CalculateThreshold(newBucketSize);
            _buckets.Value = newBuckets;
        }

        public override bool Remove(K key)
        {
            return STMSystem.Atomic(() =>
            {
                var bucketIndex = GetBucketIndex(key);
                //TMVar wrapping the immutable chain list
                var bucketVar = _buckets.Value[bucketIndex];
                var firstNode = bucketVar.Value;

                return RemoveNode(key, firstNode, bucketVar);
            });
        }

        private bool RemoveNode(K key, Node node, TMVar<Node> bucketVar)
        {
            if (node == null)
            {
                return false;
            }

            if (node.Key.Equals(key))
            {
                _size--;
                bucketVar.Value = node.Next;
                return true;
            }

            while (node.Next != null && !key.Equals(node.Next.Value.Key))
                node = node.Next.Value;

            //node.Next == null || node.Next.Key == key
            if (node.Next == null) return false;

            _size--;
            node.Next.Value = node.Next.Value.Next;
            return true;
        }

        public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return STMSystem.Atomic(() =>
            {
                var list = new List<KeyValuePair<K, V>>(_size.Value);
                for (var i = 0; i < _buckets.Value.Length; i++)
                {
                    var bucket = _buckets.Value[i];
                    var node = bucket.Value;
                    while (node != null)
                    {
                        list.Add(new KeyValuePair<K, V>(node.Key, node.Value));
                        node = node.Next.Value;
                    }
                }
                return list.GetEnumerator();
            }); 
        }


        public override V this[K key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }

        public override int Count
        {
            get { return _size.Value; }
        }
        private class Node
        {
            public K Key { get; private set; }
            public TMVar<V> Value { get; private set; }
            public TMVar<Node> Next { get; private set; }

            public Node(K key, V value)
            {
                Key = key;
                Value = new TMVar<V>(value);
                Next = new TMVar<Node>();
            }
        }

    }
}
