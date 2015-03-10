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
        private readonly TMVar<T> _item;
        private readonly TMVar<bool> _full;

        public bool IsFull => _full.GetValue();

        public SingleItemBuffer(T initial)
        {
            _item = new TMVar<T>(initial);
            _full = new TMVar<bool>(true);
        }

        public SingleItemBuffer()
        {
            _item = new TMVar<T>(default(T));
            _full = new TMVar<bool>(false);
        }

        public T GetValue()
        {
            return STMSystem.Atomic(() =>
            {
                if (!_full.GetValue())
                {
                    STMSystem.Retry();
                }

                _full.SetValue(false);
                return _item.GetValue();
            });
        }

        public void SetValue(T value)
        {
            STMSystem.Atomic(() =>
            {
                if (_full.GetValue())
                {
                    STMSystem.Retry();
                }

                _full.SetValue(true);
                _item.SetValue(value);
            });
        }
    }
}
