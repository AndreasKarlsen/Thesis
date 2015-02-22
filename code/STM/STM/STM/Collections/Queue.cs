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

        public void EnQueue(T value)
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
            });
        }

        public T DeQueue()
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
