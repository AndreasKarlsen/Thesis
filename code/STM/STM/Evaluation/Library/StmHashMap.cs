using System;
using System.Collections.Generic;
using Evaluation.Common;
using STM.Implementation.Lockbased;
using System.Collections.Immutable;

namespace Evaluation.Library
{
    public class StmHashMap<K,V> : BaseHashMap<K,V>
    {
        //TMVar to (array of TMVars to (ImmutableList of nodes) )
        private readonly TMVar<TMVar<ImmutableList<Node>>[]> _buckets = new TMVar<TMVar<ImmutableList<Node>>[]>(); 
        private readonly TMInt _threshold = new TMInt();
        private TMInt _size = new TMInt();

        public StmHashMap() : this(DefaultNrBuckets)
        {
            
        }

        public StmHashMap(int nrNuckets)
        {
            _buckets.Value = MakeBuckets(nrNuckets);
            _threshold.Value = CalulateThreshold(nrNuckets);
        }

        /// <summary>
        /// Creates and initializes the backing array
        /// </summary>
        /// <param name="nrBuckets"></param>
        /// <returns></returns>
        private TMVar<ImmutableList<Node>>[] MakeBuckets(int nrBuckets)
        {
            var temp = new TMVar<ImmutableList<Node>>[nrBuckets];
            for (var i = 0; i < nrBuckets; i++)
            {
                temp[i] = new TMVar<ImmutableList<Node>>(ImmutableList.Create<Node>()); 
            }

            return temp;
        }


        #region Utility

        private Node CreatNode(K key, V value)
        {
            return new Node(key,value);
        }

        private int GetBucketIndex(K key)
        {
            return GetBucketIndex(_buckets.Value.Length, key);
        }

        private int GetBucketIndex(int length, K key)
        {
            var hasCode = GetHashCode(key);
            return hasCode % length;
        }

        private Node FindNode(K key)
        {
            return FindNode(key, GetBucketIndex(key));
        }

        private Node FindNode(K key, int bucketIndex)
        {
            return FindNode(key, _buckets.Value[bucketIndex].Value);
        }

        private Node FindNode(K key, ImmutableList<Node> chain)
        {
            return chain.Find(n => n.Key.Equals(key));
        }

        #endregion Utility

        public override bool ContainsKey(K key)
        {
            return FindNode(key) != null;
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
                    //Else inser the node
                    bucketVar.Value = bucketVar.Value.Add(CreatNode(key, value));
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
                    bucketVar.Value = bucketVar.Value.Add(CreatNode(key, value));
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
                foreach (var node in bucket.Value)
                {
                    var bucketIndex = GetBucketIndex(newBucketSize, node.Key);
                    newBuckets[bucketIndex].Value = newBuckets[bucketIndex].Value.Add(node);
                }
            }

            //Calculate new resize threashold and assign the rehashed backing array
            _threshold.Value = CalulateThreshold(newBucketSize);
            _buckets.Value = newBuckets;
        }

        public override bool Remove(K key)
        {
            return STMSystem.Atomic(() =>
            {
                var bucketIndex = GetBucketIndex(key);
                //TMVar wrapping the immutable chain list
                var bucketVar = _buckets.Value[bucketIndex];
                var node = FindNode(key, bucketVar.Value);

                if (node != null)
                {
                    //If node is not found key does not exist so insert
                    bucketVar.Value = bucketVar.Value.Remove(node);
                    _size--;
                    return true;
                }

                return false;
            });
        }

        public override V this[K key]
        {
            get { return Get(key); }
            set { Add(key, value); }
        }

        public override int Size
        {
            get { return _size.Value; }
        }
        private class Node
        {
            public K Key { get; private set; }
            public TMVar<V> Value { get; private set; }

            public Node(K key, V value)
            {
                Key = key;
                Value = new TMVar<V>(value);
            }
        }
    }
}
