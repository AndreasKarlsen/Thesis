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
        private readonly Queue<LockingReindeer> _rBuffer;
        private readonly Queue<LockingElf> _eBuffer;
        private readonly SemaphoreSlim _santaHandle;
        private readonly SemaphoreSlim _sleigh;
        private readonly SemaphoreSlim _warmingHut;
        private readonly SemaphoreSlim _reindeerDone;
        private readonly SemaphoreSlim _elfsWaiting;
        private readonly SemaphoreSlim _elfsDone;

        public LockingSanta(Queue<LockingReindeer> rBuffer, Queue<LockingElf> eBuffer, SemaphoreSlim santaHandle,
            SemaphoreSlim sleigh, SemaphoreSlim warmingHut, SemaphoreSlim reindeerDone, SemaphoreSlim elfsWaiting, SemaphoreSlim elfsDone)
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
                    _santaHandle.Wait();

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
