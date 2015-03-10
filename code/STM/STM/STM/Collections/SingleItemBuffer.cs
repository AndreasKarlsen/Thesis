using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Lockbased;

namespace STM.Collections
{
    public class SingleItemBuffer<T>
    {
        private readonly RefLockObject<T> _item;
        private readonly RefLockObject<bool> _full;

        public bool IsFull => _full.GetValue();

        public SingleItemBuffer(T initial)
        {
            _item = new RefLockObject<T>(initial);
            _full = new RefLockObject<bool>(true);
        }

        public SingleItemBuffer()
        {
            _item = new RefLockObject<T>(default(T));
            _full = new RefLockObject<bool>(false);
        }

        public T GetValue()
        {
            return LockSTMSystem.Atomic(() =>
            {
                if (!_full.GetValue())
                {
                    LockSTMSystem.Retry();
                }

                _full.SetValue(false);
                return _item.GetValue();
            });
        }

        public void SetValue(T value)
        {
            LockSTMSystem.Atomic(() =>
            {
                if (_full.GetValue())
                {
                    LockSTMSystem.Retry();
                }

                _full.SetValue(true);
                _item.SetValue(value);
            });
        }
    }
}
