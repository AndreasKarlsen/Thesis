using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STM.Implementation.Common;
using STM.Interfaces;

namespace STM.Implementation.Lockbased
{
    public class TMUint : TMVar<uint>, Incrementable
    {
        public TMUint()
        {

        }

        public TMUint(uint i)
            : base(i)
        {

        }

        #region Inc Dec

        public void Inc()
        {
            if (Transaction.LocalTransaction.Status == TransactionStatus.Committed)
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
            if (Transaction.LocalTransaction.Status == TransactionStatus.Committed)
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

        public static TMUint operator ++(TMUint tmuint)
        {
            tmuint.Inc();
            return tmuint;
        }

        public static TMUint operator --(TMUint tmuint)
        {
            tmuint.Dec();
            return tmuint;
        }

        #endregion Inc Dec
    }
}
