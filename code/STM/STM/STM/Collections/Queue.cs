using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;

namespace STM.Collections
{
    public class Queue<T> : IEnumerable<T>
    {
        private readonly TMVar<Node> _head = new TMVar<Node>(null);
        private readonly TMVar<Node> _tail = new TMVar<Node>(null);
        private readonly TMVar<int> _size = new TMVar<int>(0);

        public int Count { get { return _size.GetValue(); } }

        public void Enqueue(T value)
        {
            STMSystem.Atomic(() =>
            {
#if DEBUG
                Console.WriteLine("Enqueue by: " + Transaction.LocalTransaction.ID);
#endif
                var node = new Node(value);
                if (_size.Value == 0)
                {
                    _head.Value = node;
                    _tail.Value = node;
                }
                else
                {
                    var curTail = _tail.Value;
                    curTail.Next.Value = node;
                    _tail.Value = node;
                }
                _size.Value = _size.Value + 1;
            });
        }

        public T Dequeue()
        {
            return STMSystem.Atomic(() =>
            {
#if DEBUG
                Console.WriteLine("Dequeue by: "+Transaction.LocalTransaction.ID);
#endif
                if (_size.Value == 0)
                {
                    STMSystem.Retry();
                }

                var oldHead = _head.Value;
                var newHead = oldHead.Next.Value;
                var value = oldHead.Value;

                _head.Value = newHead;
                if (newHead == null)
                {
                    _tail.Value = null;
                }
                _size.Value = _size.Value - 1;

                return value;
            });
        }

        private class Node
        {
            public readonly TMVar<Node> Next = new TMVar<Node>(null);
            public readonly T Value;

            public Node(T value)
            {
                Value = value;
            }

        }

        public IEnumerator<T> GetEnumerator()
        {
            var item = _head;
            for (int i = 0; i < Count; i++)
            {
                yield return item.Value.Value;
                item = item.Value.Next;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
