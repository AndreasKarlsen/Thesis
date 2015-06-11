namespace Evaluation.Common
{
    public interface IQueue<T>
    {
        void Enqueue(T item);
        bool Dequeue(out T item);
    }
}