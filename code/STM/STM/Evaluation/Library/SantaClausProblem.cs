using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Library.SantaClausImpl;
using Evaluation.Common;
using STM.Collections;

namespace Evaluation.Library
{
    public class SantaClausProblem
    {
        public static void Start()
        {
            var rBuffer = new Queue<Reindeer>();
            var eBuffer = new Queue<Elf>();
            var santa = new Santa(rBuffer,eBuffer);
            santa.Start();

            for (int i = 0; i < SCStats.NR_REINDEER ; i++)
            {
                var reindeer = new Reindeer(i, rBuffer);
                reindeer.Start();
            }
            /*
            for (int i = 0; i < SCStats.NR_ELFS; i++)
            {
                var elf = new Elf(i, eBuffer);
                elf.Start();
            }*/
        }
    }
}
