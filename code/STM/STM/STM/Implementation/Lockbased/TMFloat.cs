using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public class TMFloat : TMVar<float> , Incrementable
    {
        public TMFloat()
        {

        }

         public TMFloat(float f)
             : base(f)
        {

        }

         #region Inc Dec
         public void Inc()
         {
             if (Transaction.LocalTransaction.Status == Transaction.TransactionStatus.Committed)
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
             if (Transaction.LocalTransaction.Status == Transaction.TransactionStatus.Committed)
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

         public static TMFloat operator ++(TMFloat tmFloat)
         {
             tmFloat.Inc();
             return tmFloat;
         }

         public static TMFloat operator --(TMFloat tmFloat)
         {
             tmFloat.Dec();
             return tmFloat;
         }

         #endregion Inc Dec
    }
}
