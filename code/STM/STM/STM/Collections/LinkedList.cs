using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;

namespace STM.Collections
{
    public class LinkedList<T> : IEnumerable<T>
    {
        private readonly TMVar<Node> _head = new TMVar<Node>(null);
        private TMInt _size = new TMInt(0);

        public int Count { get { return _size.GetValue(); } }

        public void Add(T item)
        {
            STMSystem.Atomic(() =>
            {
                _head.Value = new Node(item, _head);
                _size++;
            });
        }

        public T FirstWhere(Predicate<T> condition)
        {
            return STMSystem.Atomic<T>(() =>
            {
                var node = _head.Value;
                while (node != null)
                {
                    if (condition(node.Item))
                    {
                        return node.Item;
                    }

                    node = node.Next.Value;
                }

                return default(T);
            });
        }

        public bool Contains(T item)
        {
            return STMSystem.Atomic(() =>
            {
                var node = _head.Value;
                while (node != null)
                {
                    if (node.Item.Equals(item))
                    {
                        return true;
                    }

                    node = node.Next.Value;
                }

                return false;
            });
        }

        public bool Remove(T item)
        {
            return STMSystem.Atomic(() =>
            {
                Node prevNode = null;
                var node = _head.Value;
                while (node != null)
                {
                    if (node.Item.Equals(item))
                    {
                        if (prevNode != null)
                        {
                            prevNode.Next.Value = node.Next.Value;
                        }
                        _size--;
                        return true;
                    }

                    prevNode = node;
                    node = node.Next.Value;
                }

                return false;
            });
        }


        private class Node
        {
            public readonly TMVar<Node> Next = new TMVar<Node>(null);
            public readonly T Item;

            public Node(T item)
            {
                Item = item;
            }

            public Node(T item, Node next) 
            {
                Item = item;
                Next.Value = next;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var node = _head.Value;
            while (node != null)
            {
                yield return node.Item;
                node = node.Next.Value;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
