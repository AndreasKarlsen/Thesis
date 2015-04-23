using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM.Collections;

namespace Evaluation.Language
{
    public class SantaClausProblem
    {
        public const int NR_REINDEER = 9;
        public const int NR_ELFS = 6;
        public const int MAX_ELFS = 3;

        public static void Main()
        {
            var rBuffer = new Queue<Reindeer>();
            var eBuffer = new Queue<Elf>();
            
            var santa = new Santa(rBuffer, eBuffer);
            santa.Start();

            for (int i = 0; i < SantaClausProblem.NR_REINDEER; i++)
            {
                var reindeer = new Reindeer(i, rBuffer);
                reindeer.Start();
            }

            for (int i = 0; i < SantaClausProblem.NR_ELFS; i++)
            {
                var elf = new Elf(i, eBuffer);
                elf.Start();
            }

            System.Console.WriteLine("Press any key to terminate...");
            System.Console.ReadKey();
        }
    }

    public class Elf
    {
        private Random randomGen = new Random(Guid.NewGuid().GetHashCode());
        public int ID { get; private set; }
        private Queue<Elf> _buffer;
        private atomic bool _waitingToAsk = false;
        private atomic bool _questionAsked = false;

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

                    atomic
                    {
                        if (_buffer.Count == SantaClausProblem.MAX_ELFS)
                        {
                            retry;
                        }

                        _buffer.Enqueue(this);
                        _waitingToAsk = true;
                    }

                    Console.WriteLine("Elf {0} at the door", ID);
                    
                    //Waiting on santa
                    atomic
                    {
                        if (_waitingToAsk)
                        {
                            retry;
                        }
                    }

                    //Asking question

                    //Done asking
                    atomic
                    {
                        if (!_questionAsked)
                        {
                            retry;
                        }

                        _questionAsked = false;
                    }
                }
            });
        }

        public void AskQuestion()
        {
            _waitingToAsk = false;
        }

        public void BackToWork()
        {
            _questionAsked = true;
        }
    }


    public class Reindeer
    {
        private readonly Random randomGen = new Random(Guid.NewGuid().GetHashCode());
        public int ID { get; private set; }

        private Queue<Reindeer> reindeerBuffer;
        private atomic bool _workingForSanta = false;
        private atomic bool _waitingOnSanta = false;

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

                    atomic
                    {
                        reindeerBuffer.Enqueue(this);
                        _waitingOnSanta = true;
                    }

                    Console.WriteLine("Reindeer {0} is back",ID);

                    //Wait for santa to be ready
                    atomic
                    {
                        if (_waitingOnSanta)
                        {
                            retry;
                        }
                    }

                    //Delivering presents

                    //Wait to be released by santa
                    atomic
                    {
                        if (_workingForSanta)
                        {
                            retry;
                        }
                    } 
                }
            });
        }

        public void HelpDeliverPresents()
        {
            atomic
            {
                _waitingOnSanta = false;
                _workingForSanta = true;
            }
           
        }

        public void ReleaseReindeer()
        {
            _workingForSanta = false;
        }
    }

    public class Santa
    {
        private readonly Queue<Reindeer> _rBuffer;
        private readonly Queue<Elf> _eBuffer;

        public Santa(Queue<Reindeer> rBuffer, Queue<Elf> eBuffer)
        {
            _rBuffer = rBuffer;
            _eBuffer = eBuffer;
        }

        private WakeState SleepUntilAwoken()
        {
            atomic
            {
                if (_rBuffer.Count != SantaClausProblem.NR_REINDEER)
                {
                    retry;
                }

                return WakeState.ReindeerBack;
            }
            orelse
            {
                
                if (_eBuffer.Count != SantaClausProblem.MAX_ELFS)
                {
                    retry;
                }

                return WakeState.ElfsIncompetent;
            }
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

        private void HandleReindeer()
        {
            Console.WriteLine("All reindeer are back!");

            //Setup the sleigh
            atomic
            {
                foreach (var reindeer in _rBuffer)
                {
                    reindeer.HelpDeliverPresents();
                }
            }

            //Deliver presents
            Console.WriteLine("Santa delivering presents");
            Thread.Sleep(100);

            //Release reindeer
            atomic
            {
                while (_rBuffer.Count != 0)
                {
                    var reindeer = _rBuffer.Dequeue();
                    reindeer.ReleaseReindeer();
                }
            }

            Console.WriteLine("Reindeer released");
        }

        private void HandleElfs()
        {
            Console.WriteLine("3 elfs at the door!");
            atomic
            {
                foreach (var elf in _eBuffer)
                {
                    elf.AskQuestion();
                }
            }

            //Answer questions
            Thread.Sleep(100);

            //Back to work incompetent elfs!
            atomic
            {
                for (int i = 0; i < SantaClausProblem.MAX_ELFS; i++)
                {
                    var elf = _eBuffer.Dequeue();
                    elf.BackToWork();
                }
            }

            Console.WriteLine("Elfs helped");
        }

    }

    public enum WakeState
    {
        ReindeerBack,
        ElfsIncompetent
    }
}