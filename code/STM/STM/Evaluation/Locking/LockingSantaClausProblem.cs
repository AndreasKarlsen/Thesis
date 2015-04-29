using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Locking.SantaClausImpl;
using Evaluation.Common;

namespace Evaluation.Locking
{
    public class LockingSantaClausProblem
    {
        public static void Start()
        {
            var santaHandle = new Semaphore(0,2); //First two elves wait on this outside Santa's shop
            var sleigh = new Semaphore(0,SCStats.NR_REINDEER);
            var warmingHut = new Semaphore(0, SCStats.NR_REINDEER);
            var reindeerDone = new Semaphore(0, SCStats.NR_REINDEER);
            var elfWaiting = new Semaphore(0, SCStats.MAX_ELFS);
            var elfDone = new Semaphore(0, SCStats.MAX_ELFS);
            var maxElfs = new Semaphore(SCStats.MAX_ELFS, SCStats.MAX_ELFS);
            var rBuffer = new Queue<LockingReindeer>(); //Number of reindeer back
            var eBuffer = new Queue<LockingElf>(); //Number of elfs back
            var santa = new LockingSanta(rBuffer,eBuffer,santaHandle,sleigh, warmingHut,reindeerDone,elfWaiting, elfDone);
            santa.Start();

            for (int i = 0; i < SCStats.NR_REINDEER ; i++)
            {
                var reindeer = new LockingReindeer(i, rBuffer, santaHandle, sleigh, warmingHut, reindeerDone);
                reindeer.Start();
            }

            
            for (int i = 0; i < SCStats.NR_ELFS; i++)
            {
                var elf = new LockingElf(i, eBuffer,santaHandle,maxElfs,elfWaiting,elfDone);
                elf.Start();
            }
            
        }
    }
}
