using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Common;
using STM.Implementation.JVSTM;

namespace Evaluation.Library.Collections
{
    public class JVSTMHashMapInternalList<K,V> : BaseHashMap<K,V>
    {
                //TMVar to (array of TMVars to (ImmutableList of nodes) )
        private readonly VBox<VBox<Node>[]> _buckets;
        private readonly VBox<int> _threshold;
        private readonly VBox<int> _size = new VBox<int>(0);

        public JVSTMHashMapInternalList()
            : this(DefaultNrBuckets)
        {

        }

        public JVSTMHashMapInternalList(int nrBuckets)
        {
            _buckets = new VBox<VBox<Node>[]>(MakeBuckets(nrBuckets));
            _threshold = new VBox<int>(CalculateThreshold(nrBuckets));
        }

        /// <summary>
        /// Creates and initializes the backing array
        /// </summary>
        /// <param name="nrBuckets"></param>
        /// <returns></returns>
        private VBox<Node>[] MakeBuckets(int nrBuckets)
        {
            var temp = new VBox<Node>[nrBuckets];
            for (var i = 0; i < nrBuckets; i++)
            {
                temp[i] = new VBox<Node>();
            }

            return temp;
        }


        #region Utility

        private Node CreateNode(K key, V value)
        {
            return new Node(key, value);
        }

        private int GetBucketIndex(JVTransaction transaction, K key)
        {
            return GetBucketIndex(_buckets.Read(transaction).Length, key);
        }

        private Node FindNode(JVTransaction transaction, K key)
        {
            return FindNode(transaction, key, GetBucketIndex(transaction, key));
        }

        private Node FindNode(JVTransaction transaction, K key, int bucketIndex)
        {
            return FindNode(transaction, key, _buckets.Read(transaction)[bucketIndex].Read(transaction));
        }

        private Node FindNode(JVTransaction transaction, K key, Node node)
        {
            while (node != null && !key.Equals(node.Key))
                node = node.Next.Read(transaction);
            return node;
        }

        private void InsertInBucket(JVTransaction transaction, VBox<Node> bucketVar, Node node)
        {
            var curNode = bucketVar.Read(transaction);
            if (curNode != null)
            {
                node.Next.Put(transaction,curNode);
            }
            bucketVar.Put(transaction,node);
        }

        #endregion Utility

        public override bool ContainsKey(K key)
        {
            return JVSTMSystem.Atomic(transaction => FindNode(transaction, key) != null);
        }

        public override V Get(K key)
        {
            return JVSTMSystem.Atomic((transaction) =>
            {
                var node = FindNode(transaction, key);
                if (node == null)
                {
                    throw new KeyNotFoundException("Key not found. Key: " + key);
                }
                return node.Value.Read(transaction);
            });
        }

        public override void Add(K key, V value)
        {
            JVSTMSystem.Atomic(transaction =>
            {
                var bucketIndex = GetBucketIndex(transaction, key);
                var bucketVar = _buckets.Read(transaction)[bucketIndex];
                var node = FindNode(transaction,key, bucketVar.Read(transaction));

                if (node != null)
                {
                    //If node is not null key exist in map. Update the value
                    node.Value.Put(transaction, value);
                }
                else
                {
                    //Else insert the node
                    InsertInBucket(transaction, bucketVar, CreateNode(key, value));
                    
                    //_size.Commute(transaction, i => i + 1);
                    _size.Put(transaction, _size.Read(transaction) + 1);
                    ResizeIfNeeded(transaction);
                }

            });
        }

        public override bool AddIfAbsent(K key, V value)
        {
            return JVSTMSystem.Atomic(transaction =>
            {
                var bucketIndex = GetBucketIndex(transaction, key);
                var bucketVar = _buckets.Read(transaction)[bucketIndex];
                var node = FindNode(transaction, key, bucketVar.Read(transaction));

                if (node == null)
                {
                    //If node is not found key does not exist so insert
                    InsertInBucket(transaction, bucketVar, CreateNode(key, value));
                    //_size.Commute(transaction,i => i + 1);
                    _size.Put(transaction, _size.Read(transaction) + 1);
                    ResizeIfNeeded(transaction);
                    return true;
                }
                else
                {
                    return false;
                }
            });
            
        }

        private void ResizeIfNeeded(JVTransaction transaction)
        {
            if (_size.Read(transaction) >= _threshold.Read(transaction))
            {
                Resize(transaction);
            }
        }

        private void Resize(JVTransaction transaction)
        {
            //Construct new backing array
            var newBucketSize = _buckets.Read(transaction).Length * 2;
            var newBuckets = MakeBuckets(newBucketSize);

            //For each key in the map rehash
            for (var i = 0; i < _buckets.Read(transaction).Length; i++)
            {
                var bucket = _buckets.Read(transaction)[i];
                var node = bucket.Read(transaction);
                while (node != null)
                {
                    var bucketIndex = GetBucketIndex(newBucketSize, node.Key);
                    InsertInBucket(transaction, newBuckets[bucketIndex],CreateNode(node.Key,node.Value.Read(transaction)));
                    node = node.Next.Read(transaction);
                }
            }

            //Calculate new resize threshold and assign the rehashed backing array
            _threshold.Put(transaction, CalculateThreshold(newBucketSize));
            _buckets.Put(transaction, newBuckets);
        }

        public override bool Remove(K key)
        {
            return JVSTMSystem.Atomic(transaction =>
            {
                var bucketIndex = GetBucketIndex(transaction, key);
                var bucketVar = _buckets.Read(transaction)[bucketIndex];
                var firstNode = bucketVar.Read(transaction);

                return RemoveNode(transaction, key, firstNode,bucketVar);
            });

        }

        private bool RemoveNode(JVTransaction transaction, K key, Node node, VBox<Node> bucketVar)
        {
            if (node == null)
            {
                return false;
            }

            if (node.Key.Equals(key))
            {
                _size.Put(transaction, _size.Read(transaction) - 1);
                bucketVar.Put(transaction,node.Next.Read(transaction));
                return true;
            }

            while (node.Next != null && !key.Equals(node.Next.Read(transaction).Key))
                node = node.Next.Read(transaction);

            //node.Next == null || node.Next.Key == key
            if (node.Next == null) return false;

            _size.Put(transaction, _size.Read(transaction) - 1);
            node.Next.Put(transaction, node.Next.Read(transaction).Next.Read(transaction));
            return true;
        }


        public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            var notCommitted = true;
            IEnumerator<KeyValuePair<K, V>> result = null;
            while (notCommitted)
            {
                var transaction = JVTransaction.Start();
                var list = new List<KeyValuePair<K, V>>(_size.Read(transaction));
                for (var i = 0; i < _buckets.Read(transaction).Length; i++)
                {
                    var bucket = _buckets.Read(transaction)[i];
                    var node = bucket.Read(transaction);
                    while (node != null)
                    {
                        list.Add(new KeyValuePair<K, V>(node.Key, node.Value.Read(transaction)));
                        node = node.Next.Read(transaction);
                    }
                }
                result = list.GetEnumerator();

                notCommitted = !transaction.Commit();
            }

            return result;
        }


        public override V this[K key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }

        public override int Count
        {
            get
            {
                var transaction = JVTransaction.Start();
                var result = _size.Read(transaction);
                transaction.Commit();
                return result;
            }
        }
        private class Node
        {
            public K Key { get; private set; }
            public VBox<V> Value { get; private set; }
            public VBox<Node> Next { get; private set; }

            public Node(K key, V value)
            {
                Key = key;
                Value = new VBox<V>(value);
                Next = new VBox<Node>();
            }
        }
    }
}
