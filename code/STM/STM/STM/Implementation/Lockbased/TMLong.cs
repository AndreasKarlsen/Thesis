using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public class TMLong : TMVar<long>, Incrementable
    {

        public TMLong()
        {

        }

         public TMLong(long i)
             : base(i)
        {

        }

         #region Inc Dec
         public void Inc()
         {
             this.Value = this.Value + 1;
         }

         public void Dec()
         {
             this.Value = this.Value - 1;
         }

         public static TMLong operator ++(TMLong tmLong)
         {
             tmLong.Inc();
             return tmLong;
         }

         public static TMLong operator --(TMLong tmLong)
         {
             tmLong.Dec();
             return tmLong;
         }

         #endregion Inc Dec
    }
}
