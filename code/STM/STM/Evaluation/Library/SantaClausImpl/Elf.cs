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
    public class Elf : IStartable
    {
        private Random randomGen = new Random(Guid.NewGuid().GetHashCode());
        public int ID { get; private set; }
        private Queue<Elf> _buffer;
        private TMVar<bool> _waitingToAsk = new TMVar<bool>(false);
        private TMVar<bool> _questionAsked = new TMVar<bool>(false);

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
                    Thread.Sleep(100 * randomGen.Next(21));

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
                    //Waiting on santa
                    STMSystem.Atomic(() =>
                    {
                        if (_waitingToAsk)
                        {
                            STMSystem.Retry();
                        }
                    });

                    //Asking question

                    //Done asking
                    STMSystem.Atomic(() =>
                    {
                        if (!_questionAsked)
                        {
                            STMSystem.Retry();
                        }

                        _questionAsked.Value = false;
                    });
                    
                }
            });
        }

        public void AskQuestion()
        {
            _waitingToAsk.Value = false;
        }

        public void BackToWork()
        {
            _questionAsked.Value = true;
        }
    }
}
