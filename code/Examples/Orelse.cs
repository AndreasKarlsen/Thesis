public class Consumer<T>
{
    private Buffer<T> _buffer1;
    private Buffer<T> _buffer2;

    public T ConsumeItem()
    {
        atomic{
            if(_buffer1.Count == 0)
                retry;

            return _buffer1.Get();
        }orelse{
            if(_buffer2.Count == 0)
                retry;

            return _buffer2.Get();
        }
    }
}