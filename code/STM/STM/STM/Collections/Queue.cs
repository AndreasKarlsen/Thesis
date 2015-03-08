using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;

namespace STM.Collections
{
    public class Queue<T>
    {
        private readonly TMVar<Node> _head = new TMVar<Node>(null);
        private readonly TMVar<Node> _tail = new TMVar<Node>(null);
        private TMVar<int> _size = new TMVar<int>(0);

        public int Count
        {
            get
            {
                return STMSystem.Atomic(() =>
                {
                    return _size.GetValue();
                });
            }
        }

        public void Enqueue(T value)
        {
            STMSystem.Atomic(() =>
            {
                var node = new Node(value);
                if (_tail.GetValue() == null)
                {

                    _head.SetValue(node);
                    _tail.SetValue(node);
                }
                else
                {
                    _tail.GetValue().Next.SetValue(node);
                    _tail.SetValue(node);
                }
                _size.SetValue(_size.GetValue() + 1);
            });
        }

        public T Dequeue()
        {
            return STMSystem.Atomic(() =>
            {
                if (_head.GetValue() == null)
                {
                    STMSystem.Retry();
                }

                var oldHead = _head.GetValue();
                var newHead = oldHead.Next.GetValue();
                var value = oldHead.Value;
                
                _head.SetValue(newHead);
                if (newHead == null)
                {
                    _tail.SetValue(null);
                }
                _size.SetValue(_size.GetValue() - 1);

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
    }
}
