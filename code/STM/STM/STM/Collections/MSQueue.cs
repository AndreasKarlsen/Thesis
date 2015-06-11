using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STM.Collections
{
    public class MSQueue<T>
    {
        private Node _head;
        private Node _tail;

        public MSQueue()
        {
            _head = _tail = new Node(default(T));
        }

        public bool Dequeue(out T item)
        {
            while (true)
            {
                var  first = _head;
                var last = _tail;
                var next = first.Next;
                if (first == _head)
                {
                    if (first == last)
                    {
                        if (next == null)
                        {
                            item = default(T);
                            return false;
                        }
                        else
                        {
                            Interlocked.CompareExchange(ref _tail, next, last);
                        }
                    }
                    else
                    {
                        var result = next.Value;
                        if (Interlocked.CompareExchange(ref _head, next, first) == first)
                        {
                            item = result;
                            return true;
                        }
                    }

                }
            } 
        }

        public void Enqueue(T item)
        {
            var node = new Node(item);
            while (true)
            {
                var last = _tail;
                var next = last.Next;
                if (last == _tail)
                {
                    if (next == null)
                    {
                        if (Interlocked.CompareExchange(ref last.Next, node, next) == next)
                        {
                            Interlocked.CompareExchange(ref _tail, node, last);
                            return;
                        }
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref _tail, next, last);
                    }
                } 
            }
        }

        private class Node
        {
            public Node Next;
            public readonly T Value;

            public Node(T value)
            {
                Value = value;
            }

        }
    }
}
