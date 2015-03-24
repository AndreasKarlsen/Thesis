using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evaluation.Common;
using STM.Collections;
using STM.Implementation.Lockbased;

namespace Evaluation.Library.SantaClausImpl
{
    public class Elf : IStartable
    {
        private Random randomGen = new Random(Guid.NewGuid().GetHashCode());
        public int ID { get; private set; }
        private Queue<Elf> _buffer;
        private TMVar<bool> _waitingToAsk = new TMVar<bool>(false);

        public Elf(int id, Queue<Elf> buffer)
        {
            _buffer = buffer;
            ID = id;
        }

        public Task Start()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    randomGen.Next(100 * randomGen.Next(21));

                    STMSystem.Atomic(() =>
                    {
                        if (_buffer.Count == SCStats.MAX_ELFS)
                        {
                            STMSystem.Retry();
                        }

                        _buffer.Enqueue(this);
                        _waitingToAsk.Value = true;
                    });

                    Console.WriteLine("Elf {0} at the door",ID);

                    STMSystem.Atomic(() =>
                    {
                        if (_waitingToAsk)
                        {
                            STMSystem.Retry();
                        }
                    });
                    
                }
            });
        }

        public void AskQuestion()
        {
            _waitingToAsk.Value = false;
        }
    }
}
