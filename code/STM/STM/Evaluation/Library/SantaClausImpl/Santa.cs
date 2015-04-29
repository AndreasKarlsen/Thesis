using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;
using STM.Collections;
using STM.Implementation.Lockbased;

namespace Evaluation.Library.SantaClausImpl
{
    public class Santa : IStartable
    {
        private readonly Queue<Reindeer> _rBuffer;
        private readonly Queue<Elf> _eBuffer;

        public Santa(Queue<Reindeer> rBuffer, Queue<Elf> eBuffer)
        {
            _rBuffer = rBuffer;
            _eBuffer = eBuffer;
        }

        public Task Start()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    var wakestate = SleepUntilAwoken();

                    switch (wakestate)
                    {
                        case WakeState.ReindeerBack:
                            HandleReindeer();
                            break;
                        case WakeState.ElfsIncompetent:
                            HandleElfs();
                            break;
                    }
                }
            });
        }

        private void HandleElfs()
        {
            Console.WriteLine("3 elfs at the door!");
            STMSystem.Atomic(() =>
            {
                foreach (var elf in _eBuffer)
                {
                    elf.AskQuestion();
                }
            });

            //Answer questions
            Thread.Sleep(100);

            //Back to work incompetent elfs!
            STMSystem.Atomic(() =>
            {
                for (int i = 0; i < SCStats.MAX_ELFS; i++)
                {
                    var elf = _eBuffer.Dequeue();
                    elf.BackToWork();
                }
            });

            Console.WriteLine("Elfs helped");
        }

        private void HandleReindeer()
        {
            Console.WriteLine("All reindeer are back!");

            //Call reindeer from the warming hut
            STMSystem.Atomic(() =>
            {
                foreach (var reindeer in _rBuffer)
                {
                    reindeer.CallToSleigh();
                }
            });

            //Setup the sleigh
            STMSystem.Atomic(() =>
            {
                foreach (var reindeer in _rBuffer)
                {
                    reindeer.HelpDeliverPresents();
                }
            });

            //Deliver presents
            Console.WriteLine("Santa delivering presents");
            Thread.Sleep(100);

            //Release reindeer
            STMSystem.Atomic(() =>
            {
                while (_rBuffer.Count != 0)
                {
                    var reindeer = _rBuffer.Dequeue();
                    reindeer.ReleaseReindeer();
                }
            });

            Console.WriteLine("Reindeer released");
        }

        private WakeState SleepUntilAwoken()
        {
            return STMSystem.Atomic(() =>
            {
                if (_rBuffer.Count != SCStats.NR_REINDEER)
                {
                    STMSystem.Retry();
                }

                return WakeState.ReindeerBack;
            },
                () =>
                {
                    if (_eBuffer.Count != SCStats.MAX_ELFS)
                    {
                        STMSystem.Retry();
                    }

                    return WakeState.ElfsIncompetent;
                });
        }
    }
}
