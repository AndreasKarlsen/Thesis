using System;
using System.Threading;

namespace STM.Collections
{
    public class Queue<T>
    {
        private atomic Node _head = null;
        private atomic Node _tail = null;
        private atomic int _size = 0;

        public int Count
        {
            get
            {
                return _size;
            }
        }

        public void Enqueue(T value)
        {
            atomic{
                var node = new Node(value);
                if (_tail == null)
                {
                    _head = node;
                    _head = node;
                }
                else
                {
                    _tail.Next = node;
                    _tail = node;
                }
                _size++;
            }
        }

        public T Dequeue()
        {
            atomic{
                if (_size == 0)
                {
                    retry;
                }

                var oldHead = _head;

                _head = _old.Next;
                if (newHead == null)
                {
                    _head = null;
                }
                _size--;

                return oldHead.Value;
            }
        }

        private class Node
        {
            public atomic Node Next = null;
            public readonly T Value;

            public Node(T value)
            {
                Value = value;
            }

        }
    }
}