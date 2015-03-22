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
             this.Value = this.Value + 1;
         }

         public void Dec()
         {
             this.Value = this.Value - 1;
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
