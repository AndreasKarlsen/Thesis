using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    public class VBox<T> : BaseVBox
    {
        private volatile VBoxBody<T> _body;

        public VBox() : this(default(T))
        {
            
        }

        public VBox(T value)
        {
            _body = new VBoxBody<T>(value, 0, null);
        }

        public T Read(JVTransaction transaction)
        {
            if (transaction.WriteMap.Contains(this))
            {
                return (T)transaction.WriteMap[this];
            }

            var body = _body;
            var tNumber = transaction.Number;
            while (body.Version > tNumber)
            {
                body = body.Next;
            }

            transaction.ReadMap.Put(this,body);

            return body.Value;
        }

        public void Put(JVTransaction transaction, T value)
        {
            transaction.WriteMap.Put(this,value);
        }

        internal override bool Validate(BaseVBoxBody readBody)
        {
            return _body == readBody;
        }

        internal override void Install(object value, int version)
        {
            _body = new VBoxBody<T>((T)value,version,_body);
        }
    }
}
