using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public class TMUInt : TMVar<uint>, Incrementable
    {
        public TMUInt()
        {

        }

        public TMUInt(uint i)
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

        public static TMUInt operator ++(TMUInt tmuint)
        {
            tmuint.Inc();
            return tmuint;
        }

        public static TMUInt operator --(TMUInt tmuint)
        {
            tmuint.Dec();
            return tmuint;
        }

        #endregion Inc Dec
    }
}
