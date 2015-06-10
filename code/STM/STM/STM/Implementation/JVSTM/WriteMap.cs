using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Exceptions;
using STM.Implementation.Lockbased;

namespace STM.Implementation.JVSTM
{
    public class WriteMap : HashMap<BaseVBox, object>
    {
        protected AtomicBool[] _bucketsDone;
        protected LinkedList<BaseVBoxBody>[] _commitedBodies;

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
            _bucketsDone = new AtomicBool[_buckets.Length];

            for (int i = 0; i < _bucketsDone.Length; i++)
            {
                _bucketsDone[i] = new AtomicBool();
            }

            _commitedBodies = new LinkedList<BaseVBoxBody>[_buckets.Length];
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

        public LinkedList<BaseVBoxBody> WriteBackBucket(int bucket, int newTxNumber) {
            var newBodies = new LinkedList<BaseVBoxBody>();
            var node = _buckets[bucket];
            while (node != null)
	        {
                var body = node.Key.Commit(node.Value, newTxNumber);
                newBodies.AddFirst(body);
                node = node.Next;
	        }

            return newBodies;
        }

        public void Clean()
        {
            for (int i = 0; i < _commitedBodies.Length; i++)
            {
                var list = _commitedBodies[i];
                foreach (var item in list)
                {
                    item.Clean();
                }
            }
        }
    }
}
