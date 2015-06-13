using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Exceptions;
using STM.Implementation.Lockbased;
using Spring.Threading.AtomicTypes;

namespace STM.Implementation.JVSTM
{
    public class WriteMap : Dictionary<BaseVBox,object>// HashMap<BaseVBox, object>
    {
        protected AtomicBoolean[] _bucketsDone;
        protected BaseVBoxBody[] _commitedBodies;
        protected KeyValuePair<BaseVBox, object>[] _buckets;

        public WriteMap()
        {
            
        }

        public WriteMap(WriteMap other)
        {
            foreach (var item in other)
            {
                Add(item.Key, item.Value);
            }
        }

        public void Merge(WriteMap other)
        {
            foreach (var kvpair in other)
            {
                Add(kvpair.Key, kvpair.Value);
            }
        }

        public bool Validate(ReadMap readMap)
        {
            foreach (var writePair in this)
            {
                if (readMap.ContainsKey(writePair.Key))
                {
                    return false;
                }
            }

            return true;
        }
        
        public void PrepareCommit()
        {
            _buckets = new KeyValuePair<BaseVBox, object>[Count];
            var j = 0;
            foreach (var item in this)
            {
                _buckets[j] = item;
                j++;
            }
            /*
            if (Count < 5)
            {
                _buckets = new List<KeyValuePair<BaseVBox,object>>[1];
                var list = new List<KeyValuePair<BaseVBox, object>>();
                foreach (var item in this)
	            {
		            list.Add(item);
	            }
                _buckets[0] = list;
            }else
	        {
                _buckets = new List<KeyValuePair<BaseVBox,object>>[5];
                for (int i = 0; i < 5; i++)
			    {
			        _buckets[i] = new List<KeyValuePair<BaseVBox,object>>();
			    }
                int j = 0;
                foreach (var item in this)
	            {
		            _buckets[j % 5].Add(item);
                    j++;
	            }
                _commitedBodies = new LinkedList<BaseVBoxBody>[5];
	        }*/

            _bucketsDone = new AtomicBoolean[_buckets.Length];
            for (int i = 0; i < _bucketsDone.Length; i++)
            {
                _bucketsDone[i] = new AtomicBoolean();
            }
            _commitedBodies = new BaseVBoxBody[_buckets.Length];
        }

        public void HelpWriteBack(int newTxNumber)
        {
            int finalBucket = LocalRandom.Random.Next(_buckets.Length);
            int currBucket = finalBucket;
            do
            {
                if (!_bucketsDone[currBucket].Value)
                {
                    _commitedBodies[currBucket] = WriteBackBucket(currBucket, newTxNumber);
                    _bucketsDone[currBucket].Value = true;
                }
                currBucket = (currBucket + 1) % _buckets.Length;
            } while (currBucket != finalBucket);
        }

        public BaseVBoxBody WriteBackBucket(int bucket, int newTxNumber) {
            var node = _buckets[bucket];

            return node.Key.Commit(node.Value, newTxNumber);
            /*
            foreach (var item in node)
            {
                var body = item.Key.Commit(item.Value, newTxNumber);
                newBodies.AddFirst(body);
            }*/
            /*
            while (node != null)
	        {
                var body = node.Key.Commit(node.Value, newTxNumber);
                newBodies.AddFirst(body);
                node = node.Next;
	        }

            return newBodies;
             * */
        }

        public void Clean()
        {
            for (int i = 0; i < _commitedBodies.Length; i++)
            {
                var item = _commitedBodies[i];
                item.Clean();
            }
        }
    }
}
