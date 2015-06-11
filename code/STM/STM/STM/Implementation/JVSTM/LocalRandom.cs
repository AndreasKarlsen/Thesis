using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace STM.Implementation.JVSTM
{
    public class LocalRandom
    {
        private static readonly ThreadLocal<Random> Local = new ThreadLocal<Random>(() => new Random((int)DateTime.Now.Ticks));

        internal static Random Random
        {
            get { return Local.Value; }
            set { Local.Value = value; }
        }
    }
}
