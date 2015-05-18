using System.Threading;

namespace STM.Implementation.Common
{
    public class RetryLatch : IRetryLatch
    {
        private volatile int _era = int.MinValue;
        private volatile bool _isOpen = false;
        private readonly object _lockObject = new object();

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public int Era
        {
            get { return _era; }
        }

        public void Open(int expectedEra)
        {
            if (_isOpen || expectedEra != _era)
            {
                return;
            }

            lock (_lockObject)
            {
                if (_isOpen || expectedEra != _era)
                {
                    return;
                }

                _isOpen = true;
                Monitor.PulseAll(_lockObject);
            }
        }


        public void Await(int expectedEra)
        {
            if (_isOpen || expectedEra != _era)
            {
                return;
            }

            lock (_lockObject)
            {
                while (!_isOpen && _era == expectedEra)
                {
                    Monitor.Wait(_lockObject);
                }
            }

        }

        public void Reset()
        {
            lock (_lockObject)
            {
                if (!_isOpen)
                {
                    Monitor.PulseAll(_lockObject);
                }
                else
                {
                    _isOpen = false;
                }
                _era++;
            }
        }
    }
}