using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;

namespace Evaluation.Locking.SantaClausImpl
{
    public class LockingSanta : IStartable
    {
        private Queue<LockingReindeer> _rBuffer;
        private Queue<LockingElf> _eBuffer;
        private Semaphore _santaHandle;
        private Semaphore _sleigh;
        private Semaphore _warmingHut;
        private Semaphore _reindeerDone;
        private Semaphore _elfsWaiting;
        private Semaphore _elfsDone;

        public LockingSanta(Queue<LockingReindeer> rBuffer, Queue<LockingElf> eBuffer, Semaphore santaHandle, 
            Semaphore sleigh, Semaphore warmingHut, Semaphore reindeerDone, Semaphore elfsWaiting, Semaphore elfsDone)
        {
            _rBuffer = rBuffer;
            _eBuffer = eBuffer;
            _santaHandle = santaHandle;
            _sleigh = sleigh;
            _warmingHut = warmingHut;
            _reindeerDone = reindeerDone;
            _elfsWaiting = elfsWaiting;
            _elfsDone = elfsDone;
        }

        public Task Start()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    //Santa is resting
                    _santaHandle.WaitOne();

                    var wakeState = WakeState.ElfsIncompetent;

                    lock (_rBuffer)
                    {
                        if (_rBuffer.Count == SCStats.NR_REINDEER)
                        {
                            wakeState = WakeState.ReindeerBack;    
                        }
                    }

                    switch (wakeState)
                    {
                        case WakeState.ReindeerBack:
                            Console.WriteLine("All reindeer are back!");

                            //Release reindeers from warming hut
                            _warmingHut.Release(SCStats.NR_REINDEER);

                            //Setup the sleigh
                            _sleigh.Release(SCStats.NR_REINDEER);

                            //Deliver presents
                            Console.WriteLine("Santa delivering presents");
                            Thread.Sleep(100);

                            //Release reindeer
                            _rBuffer.Clear();
                            _reindeerDone.Release(SCStats.NR_REINDEER);
                            Console.WriteLine("Reindeer released");
                            break;
                        case WakeState.ElfsIncompetent:
                            Console.WriteLine("3 elfs at the door!");

                            _elfsWaiting.Release(SCStats.MAX_ELFS);

                            //Answer questions
                            Thread.Sleep(100);

                            //Back to work incompetent elfs!
                            _eBuffer.Clear();
                            _elfsDone.Release(SCStats.MAX_ELFS);

                            Console.WriteLine("Elfs helped");
                            break;
                    }
                    

                }
            });
        }
        
    }
}
