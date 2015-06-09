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
        }

        public void HelpWriteBack(int newTxNumber)
        {
            int finalBucket = LocalRandom.Random.Next(_buckets.Length);
            int currBucket = finalBucket;
            do
            {
                if (!_bucketsDone[currBucket].Value)
                {
                    //this.bodiesPerBucket[currBucket] =
                    WriteBackBucket(currBucket, newTxNumber);
                    _bucketsDone[currBucket].Value = true;
                }
                currBucket = (currBucket + 1) % _buckets.Length;
            } while (currBucket != finalBucket);
        }

        public void WriteBackBucket(int bucket, int newTxNumber) {

            var node = _buckets[bucket];
            while (node != null)
	        {
                node.Key.Commit(node.Value, newTxNumber);
                node = node.Next;
	        }
        }
    }
}
