using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Threading.AtomicTypes;


namespace STM.Collections
{
    public class LockFreeSkipList<T> : ISkipList<T>
    {
        internal static int MAX_LEVEL = 5;
        private readonly SkipListNode head = new SkipListNode(int.MinValue);
        private readonly SkipListNode tail = new SkipListNode(int.MaxValue);

        public LockFreeSkipList()
        {
            for (var i = 0; i < head.Next.Length; i++)
            {
                head.Next[i]
                    = new AtomicMarkableReference<SkipListNode>(tail, false);
            }
        }

        public class SkipListNode
        {
            public readonly T Value;
            public readonly int Key;
            private int _topLevel;

            public readonly AtomicMarkableReference<SkipListNode>[] Next =
                new AtomicMarkableReference<SkipListNode>[MAX_LEVEL + 1];

            public SkipListNode(int key)
            {
                Key = key;
                _topLevel = MAX_LEVEL;
                InitNext();
            }

            public SkipListNode(T value, int height)
            {
                Value = value;
                Key = value.GetHashCode();
                _topLevel = height;
                InitNext();
            }

            private void InitNext()
            {
                for (var i = 0; i < Next.Length; i++)
                {
                    Next[i] = new AtomicMarkableReference<SkipListNode>(null, false);
                }
            }
        }
    }
}
