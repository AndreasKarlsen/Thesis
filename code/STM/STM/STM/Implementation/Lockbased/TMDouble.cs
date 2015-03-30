using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public class TMDouble : TMVar<double>, Incrementable
    {
        public TMDouble()
        {

        }

        public TMDouble(double d)
             : base(d)
        {

        }

         #region Inc Dec
         public void Inc()
         {
             if (Transaction.LocalTransaction.Status == Transaction.TransactionStatus.Active)
             {
                 STMSystem.Atomic(() =>
                 {
                     this.Value = this.Value + 1;
                 });
             }
             else
             {
                 this.Value = this.Value + 1;
             }
         }

         public void Dec()
         {
             if (Transaction.LocalTransaction.Status == Transaction.TransactionStatus.Active)
             {
                 STMSystem.Atomic(() =>
                 {
                     this.Value = this.Value - 1;
                 });
             }
             else
             {
                 this.Value = this.Value - 1;
             }
         }

         public static TMDouble operator ++(TMDouble tmDouble)
         {
             tmDouble.Inc();
             return tmDouble;
         }

         public static TMDouble operator --(TMDouble tmDouble)
         {
             tmDouble.Dec();
             return tmDouble;
         }

         #endregion Inc Dec
    }
}
