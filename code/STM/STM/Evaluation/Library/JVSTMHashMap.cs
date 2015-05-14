using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;
using STM.Implementation.JVSTM;
using STM.Implementation.Lockbased;

namespace Evaluation.Library
{
    public class JVSTMHashMap<K,V> : BaseHashMap<K,V>
    {
        //TMVar to (array of TMVars to (ImmutableList of nodes) )
        private readonly VBox<VBox<ImmutableList<Node>>[]> _buckets;
        private readonly VBox<int> _threshold;
        private readonly VBox<int> _size = new VBox<int>(0);

        public JVSTMHashMap() : this(DefaultNrBuckets)
        {
            
        }

        public JVSTMHashMap(int nrBuckets)
        {
            _buckets = new VBox<VBox<ImmutableList<Node>>[]>(MakeBuckets(nrBuckets));
            _threshold = new VBox<int>(CalculateThreshold(nrBuckets));
        }

        /// <summary>
        /// Creates and initializes the backing array
        /// </summary>
        /// <param name="nrBuckets"></param>
        /// <returns></returns>
        private VBox<ImmutableList<Node>>[] MakeBuckets(int nrBuckets)
        {
            var temp = new VBox<ImmutableList<Node>>[nrBuckets];
            for (var i = 0; i < nrBuckets; i++)
            {
                temp[i] = new VBox<ImmutableList<Node>>(ImmutableList.Create<Node>()); 
            }

            return temp;
        }


        #region Utility

        private Node CreateNode(K key, V value)
        {
            return new Node(key,value);
        }

        private int GetBucketIndex(JVTransaction transaction,K key)
        {
            return GetBucketIndex(_buckets.Read(transaction).Length, key);
        }

        private Node FindNode(JVTransaction transaction, K key)
        {
            return FindNode(transaction, key, GetBucketIndex(transaction, key));
        }

        private Node FindNode(JVTransaction transaction, K key, int bucketIndex)
        {
            return FindNode(key, _buckets.Read(transaction)[bucketIndex].Read(transaction));
        }

        private Node FindNode(K key, ImmutableList<Node> chain)
        {
            return chain.Find(n => n.Key.Equals(key));
        }

        #endregion Utility

        public override bool ContainsKey(K key)
        {
            var notCommitted = true;
            Node node = null;
            while (notCommitted)
            {
                var transaction = JVTransaction.Start();
                node = FindNode(transaction, key);
                notCommitted = !transaction.Commit();
            }

            return node != null;
        }

        public override V Get(K key)
        {
            var notCommitted = true;
            var value = default(V);
            while (notCommitted)
            {
                var transaction = JVTransaction.Start();
                var node = FindNode(transaction, key);
                if (node == null)
                {
                    throw new KeyNotFoundException("Key not found. Key: " + key);
                }
                value = node.Value.Read(transaction);
                notCommitted = !transaction.Commit();
            }

            return value;
        }

        public override void Add(K key, V value)
        {
            var notCommitted = true;
            while (notCommitted)
            {
                var transaction = JVTransaction.Start();
                var bucketIndex = GetBucketIndex(transaction, key);
                var bucketVar = _buckets.Read(transaction)[bucketIndex];
                var node = FindNode(key, bucketVar.Read(transaction));

                if (node != null)
                {
                    //If node is not null key exist in map. Update the value
                    node.Value.Put(transaction, value);
                }
                else
                {
                    //Else insert the node
                    bucketVar.Put(transaction, bucketVar.Read(transaction).Add(CreateNode(key, value)));
                    _size.Put(transaction,_size.Read(transaction)+1);
                    ResizeIfNeeded(transaction);
                }

                notCommitted = !transaction.Commit();
            }
        }

        public override bool AddIfAbsent(K key, V value)
        {

            var notCommitted = true;
            var result = false;
            while (notCommitted)
            {
                var transaction = JVTransaction.Start();
                var bucketIndex = GetBucketIndex(transaction, key);
                var bucketVar = _buckets.Read(transaction)[bucketIndex];
                var node = FindNode(key, bucketVar.Read(transaction));

                if (node == null)
                {
                    //If node is not found key does not exist so insert
                    bucketVar.Put(transaction, bucketVar.Read(transaction).Add(CreateNode(key, value)));
                    _size.Put(transaction, _size.Read(transaction) + 1);
                    ResizeIfNeeded(transaction);
                    result = true;
                }
                else
                {
                    result = false;
                }

                notCommitted = !transaction.Commit();
            }

            return result;
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
                foreach (var node in bucket.Read(transaction))
                {
                    var bucketIndex = GetBucketIndex(newBucketSize, node.Key);
                    newBuckets[bucketIndex].Put(transaction,newBuckets[bucketIndex].Read(transaction).Add(node));
                }
            }

            //Calculate new resize threshold and assign the rehashed backing array
            _threshold.Put(transaction, CalculateThreshold(newBucketSize));
            _buckets.Put(transaction, newBuckets);
        }

        public override bool Remove(K key)
        {
            var notCommitted = true;
            var result = false;
            while (notCommitted)
            {
                var transaction = JVTransaction.Start();
                var bucketIndex = GetBucketIndex(transaction, key);
                var bucketVar = _buckets.Read(transaction)[bucketIndex];
                var node = FindNode(key, bucketVar.Read(transaction));

                if (node != null)
                {
                    //If node is not found key does not exist so insert
                    bucketVar.Put(transaction, bucketVar.Read(transaction).Remove(node));
                    _size.Put(transaction, _size.Read(transaction) - 1);
                    result = true;
                }
                else
                {
                    result = false;
                }

                notCommitted = !transaction.Commit();
            }

            return result;
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
                    foreach (var node in bucket.Read(transaction))
                    {
                        list.Add(new KeyValuePair<K, V>(node.Key, node.Value.Read(transaction)));
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
                var result =  _size.Read(transaction);
                transaction.Commit();
                return result;
            }
        }
        private class Node
        {
            public K Key { get; private set; }
            public VBox<V> Value { get; private set; }

            public Node(K key, V value)
            {
                Key = key;
                Value = new VBox<V>(value);
            }
        }

       
    }
}
