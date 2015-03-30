using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;
namespace Evaluation.Locking.SantaClausImpl
{
    public class LockingReindeer : IStartable
    {
        private Random _randomGen = new Random(Guid.NewGuid().GetHashCode());
        public int ID { get; private set; }

        private Queue<LockingReindeer> _reindeerBuffer;
        private Semaphore _santaHandle;
        private Semaphore _sleigh;
        private Semaphore _doneDelivering;


        public LockingReindeer(int id, Queue<LockingReindeer> buffer, Semaphore santaHandle, Semaphore sleigh, Semaphore doneDelivering)
        {
            ID = id;
            _reindeerBuffer = buffer;
            _santaHandle = santaHandle;
            _sleigh = sleigh;
            _doneDelivering = doneDelivering;
        }
        
        public Task Start()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    //Yey vaication
                    Thread.Sleep(100 * _randomGen.Next(10));

                    lock (_reindeerBuffer)
                    {
                        _reindeerBuffer.Enqueue(this);
                        if (_reindeerBuffer.Count == SCStats.NR_REINDEER)
                        {
                            _santaHandle.Release();
                        }
                    }

                    //Console.WriteLine("Reindeer {0} is back",ID);

                    //Wait for santa to be ready
                    _sleigh.WaitOne();

                    //Delivering presents

                    //Wait for delivery to be done
                    _doneDelivering.WaitOne();
                    
                }
            });
        }

        public void ReleaseReindeer()
        {
            
        }
    }
}
