using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Common;

namespace Evaluation.Locking.Collections
{
    public class Queue<T> : IQueue<T>
    {
        protected readonly object HeadLock = new object();
        protected readonly object TailLock = new object();
        private Node _head;
        private Node _tail;

        public Queue()
        {
            _head = new Node(default(T));
            _tail = _head;
        }

        public void Enqueue(T item)
        {
            var node = new Node(item);
            lock (TailLock)
            {
                _tail.Next = node;
                _tail = node;
            }
        }

        public bool Dequeue(out T item)
        {
            lock (HeadLock)
            {
                var newHead = _head.Next;
                if (newHead == null)
                {
                    item = default(T);
                    return false;
                }

                _head = newHead;
                item = newHead.Value;
                return true;
            }
        }

        private class Node
        {
            public Node Next { get; set; }
            public T Value { get; private set; }

            public Node(T value)
            {
                Value = value;
            }

        }
    }
}
