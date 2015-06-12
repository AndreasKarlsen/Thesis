using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;
using Evaluation.Common;

namespace Evaluation.Library.Collections
{
    public class Queue<T> : ISTMQueue<T>
    {

        private readonly TMVar<Node> _head = new TMVar<Node>(null);
        private readonly TMVar<Node> _tail = new TMVar<Node>(null);

        public Queue()
        {
            var node = new Node(default(T));
            _head.Value = node;
            _tail.Value = node;
        }

        public void Enqueue(T value)
        {
            STMSystem.Atomic(() =>
            {
                var node = new Node(value);
                var curTail = _tail.Value;
                curTail.Next.Value = node;
                _tail.Value = node;
            });
        }


        public T Dequeue()
        {
            return STMSystem.Atomic(() =>
            {
                var node = _head.Value.Next.Value;

                if (node == null)
                {
                    STMSystem.Retry();
                }

                _head.Value = node;
                return node.Value;
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
