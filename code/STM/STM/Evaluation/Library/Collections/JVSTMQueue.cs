using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Common;
using STM.Implementation.JVSTM;

namespace Evaluation.Library.Collections
{
    public class JVSTMQueue<T> : ISTMQueue<T>
    {
        private readonly VBox<Node> _head = new VBox<Node>(null);
        private readonly VBox<Node> _tail = new VBox<Node>(null);

        public JVSTMQueue()
        {
            var node = new Node(default(T));
            JVSTMSystem.Atomic((t) =>
            {
                _head.Put(t,node);
                _tail.Put(t,node);
            });
        }

        public void Enqueue(T value)
        {
            JVSTMSystem.Atomic((t) =>
            {
                var node = new Node(value);
                var curTail = _tail.Read(t);
                curTail.Next.Put(t,node);
                _tail.Put(t,node);
            });
        }


        public T Dequeue()
        {
            return JVSTMSystem.Atomic((t) =>
            {
                var node = _head.Read(t).Next.Read(t);

                if (node == null)
                {
                    JVSTMSystem.Retry();
                }

                _head.Put(t, node);
                return node.Value;
            });
        }

        private class Node
        {
            public readonly VBox<Node> Next = new VBox<Node>(null);
            public readonly T Value;

            public Node(T value)
            {
                Value = value;
            }

        }
    }
}
