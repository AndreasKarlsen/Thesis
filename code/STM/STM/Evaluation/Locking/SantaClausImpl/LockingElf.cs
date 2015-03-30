using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;

namespace Evaluation.Locking.SantaClausImpl
{
    public class LockingElf : IStartable
    {
        private Random _randomGen = new Random(Guid.NewGuid().GetHashCode());
        public int ID { get; private set; }
        private Queue<LockingElf> _buffer;
        private Semaphore _maxElfs;
        private Semaphore _santaHandle;
        private Semaphore _waitingToAsk;
        private Semaphore _doneAsking;

        public LockingElf(int id, Queue<LockingElf> buffer, Semaphore santaHandle, Semaphore maxElfs, Semaphore waitingToAsk, Semaphore doneWaiting)
        {
            _buffer = buffer;
            ID = id;
            _maxElfs = maxElfs;
            _santaHandle = santaHandle;
            _waitingToAsk = waitingToAsk;
            _doneAsking = doneWaiting;
        }

        public Task Start()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100 * _randomGen.Next(21));

                    //Only a fixed amount of elfs can go to santa at a time
                    _maxElfs.WaitOne();

                    lock (_buffer)
                    {
                        _buffer.Enqueue(this);
                        if (_buffer.Count == SCStats.MAX_ELFS)
                        {
                            _santaHandle.Release();
                        }
                    }

                    Console.WriteLine("Elf {0} at the door", ID);

                    //Wait for santa to be ready
                    _waitingToAsk.WaitOne();

                    //Asking questions

                    _doneAsking.WaitOne();

                    //Allow a new elf to visit santa
                    _maxElfs.Release();
                }
            });
        }

        public void AskQuestion()
        {
            _waitingToAsk.Release();
        }
    }
}
