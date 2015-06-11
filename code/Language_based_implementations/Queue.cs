using System;
using System.Threading;
using System.Threading.Tasks;

namespace LanguagedBasedQueue
{
    public class Program
    {
        public static void Main()
        {
            var queue = new Queue<int>();

            var t1 = new Thread(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    queue.Enqueue(i);
                }

                for (var i = 0; i < 1000; i++)
                {
                    queue.Dequeue();
                }
            });


            var t2 = new Thread(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    queue.Enqueue(i);
                }

                for (var i = 0; i < 1000; i++)
                {
                    queue.Dequeue();
                }
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();
            Console.WriteLine("Done:");
        }
    }
    
    public class Queue<T>
    {

        private atomic Node _head;
        private atomic Node _tail;

        public Queue()
        {
            var node = new Node(default(T));
            _head = node;
            _tail = node;
        }

        public void Enqueue(T value)
        {
            atomic
            {
                var node = new Node(value);
                _tail.Next = node;
                _tail = node;
            }
        }


        public T Dequeue()
        {
            atomic
            {
                var node = _head.Next;

                if (node == null)
                {
                    retry;
                }

                _head = node;
                return node.Value;
            }
        }

        private class Node
        {
            public atomic Node Next { get; set; }
            public readonly T Value;

            public Node(T value)
            {
                Value = value;
            }

        }
    }
}