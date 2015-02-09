using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STM.Collections
{
    public class SkipList<T> : ISkipList<T>
    {
        private const int MaxLevel = 5;
        private readonly SkipListNode head = new SkipListNode(int.MinValue);
        private readonly SkipListNode tail = new SkipListNode(int.MaxValue);

        public SkipList()
        {
            for (var i = 0; i < head.Next.Length; i++)
            {
                head.Next[i]= tail;
            }
        }

        public class SkipListNode
        {
            public readonly object LockObject = new object();
            public readonly T Value;
            public readonly int Key;
            public int TopLevel { get; private set; }

            public readonly SkipListNode[] Next =new SkipListNode[MaxLevel + 1];

            public SkipListNode(int key)
            {
                Key = key;
                TopLevel = MaxLevel;
            }

            public SkipListNode(T value, int height)
            {
                Value = value;
                Key = value.GetHashCode();
                TopLevel = height;
            }

        }
    }
}
