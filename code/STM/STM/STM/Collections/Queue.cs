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
        private readonly RefLockObject<Node> _head = new RefLockObject<Node>(null);
        private readonly RefLockObject<Node> _tail = new RefLockObject<Node>(null);
        private readonly RefLockObject<int> _size = new RefLockObject<int>(0);

        public int Count => _size.GetValue();

        public void Enqueue(T value)
        {
            LockSTMSystem.Atomic(() =>
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
            return LockSTMSystem.Atomic(() =>
            {
                if (_head.GetValue() == null)
                {
                    LockSTMSystem.Retry();
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
            public readonly RefLockObject<Node> Next = new RefLockObject<Node>(null);
            public readonly T Value;

            public Node(T value)
            {
                Value = value;
            }

        }
    }
}
