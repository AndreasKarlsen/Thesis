using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evaluation.Common;
using STM.Implementation.Lockbased;
using STM.Collections;

namespace Evaluation.Library.SantaClausImpl
{
    public class Reindeer : IStartable
    {
        private Random randomGen = new Random(Guid.NewGuid().GetHashCode());
        public int ID { get; private set; }

        private Queue<Reindeer> reindeerBuffer;
        private TMVar<bool> workingForSanta = new TMVar<bool>(false);

        public Reindeer(int id, Queue<Reindeer> buffer)
        {
            ID = id;
            reindeerBuffer = buffer;
        }
        
        public Task Start()
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100 * randomGen.Next(10));

                    STMSystem.Atomic(() =>
                    {
                        reindeerBuffer.Enqueue(this);
                        workingForSanta.Value = true;
                    });

                    Console.WriteLine("Reindeer {0} is back",ID);

                    //Wait to be released by santa
                    STMSystem.Atomic(() =>
                    {
                        if (workingForSanta)
                        {
                            STMSystem.Retry();
                        }
                    });   
                }
            });
        }

        public void ReleaseReindeer()
        {
            workingForSanta.Value = false;
        }
    }
}
