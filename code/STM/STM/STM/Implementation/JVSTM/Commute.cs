using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STM.Implementation.JVSTM
{
    internal class Commute<T> : BaseCommute
    {
        public Func<T,T> Action { get; private set; }
        public VBox<T> VBox { get; private set; }

        public Commute(Func<T, T> action, VBox<T> vBox)
        {
            Action = action;
            VBox = vBox;
        }

        public override void Perform(int version)
        {
            VBox.Install(Action(VBox.ReadCommute()), version);
        }
    }
}
