using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using STM;
using STM.Collections;
using STM.Interfaces;
using STM.Implementation.Obstructionfree;
using STM.Implementation.Lockbased;

namespace STMTester.LibraryExamples
{
    class TransactionExamples
    {
        public static void AtomicExmples()
        {
            STMSystem.Atomic(() =>
            {
                //Transaction body
            });

            var result = STMSystem.Atomic<int>(() =>
            {
                //Transaction body
                return 1;
            });

            STMSystem.Atomic(() =>
            {
                //Transaction body
            },
            () =>
            {
                //orelse body
            });
        }

        public static void TMVarExample()
        {
            TMVar<string> tmString = new TMVar<string>("abc");
            TMVar<bool> tmBool = new TMVar<bool>();
            TMVar<Person> tmPerson = new TMVar<Person>(new Person("Bo Hansen", 57));
            TMInt tmInt = new TMInt(12);
           

            STMSystem.Atomic(() =>
            {
                if (tmBool && tmString == "abc")
                {
                    tmInt++;
                }
            });
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public Person(string name, int age)
            {
                Name = name;
                Age = age;
            }
        }

        public static void RetryExample(STM.Collections.Queue<Person> buffer)
        {
            Person result = STMSystem.Atomic(() =>
            {
                if (buffer.Count == 0)
                      STMSystem.Retry(); //Initiates retry

                return buffer.Dequeue();
            });

            //Work with the result

            STMSystem.Retry(); //Has no effect
        }

        public static void NestingExample()
        {
            TMVar<string> s = new TMVar<string>(string.Empty);
            var result = STMSystem.Atomic(() =>
            {
                s.Value = "abc";
                STMSystem.Atomic(() =>
                {
                    s.Value = s + "def";
                });

                return s.Value;
            });

            STM.Collections.Queue<Person> buffer1 = new STM.Collections.Queue<Person>();
            STM.Collections.Queue<Person> buffer2 = new STM.Collections.Queue<Person>();

            STMSystem.Atomic(() =>
            {
                var item = buffer1.Dequeue();
                buffer2.Enqueue(item);
            });
        }

        private static void DinningPhilosophersTest()
        {
            var fork1 = new TMVar<bool>(true);
            var fork2 = new TMVar<bool>(true);
            var fork3 = new TMVar<bool>(true);
            var fork4 = new TMVar<bool>(true);
            var fork5 = new TMVar<bool>(true);

            StartPhilosopher(fork1, fork2);
            StartPhilosopher(fork2, fork3);
            StartPhilosopher(fork3, fork4);
            StartPhilosopher(fork4, fork5);
            StartPhilosopher(fork5, fork1);
        }

        private static void StartPhilosopher( TMVar<bool> left, TMVar<bool> right)
        {
            var t1 = new Thread(() =>
            {
                while (true)
                {
                    STMSystem.Atomic(() =>
                    {
                        if (!left || !right)
                        {
                            STMSystem.Retry();
                        }

                        left.Value = false;
                        right.Value = false;
                    });

                    Thread.Sleep(100);

                    STMSystem.Atomic(() =>
                    {
                        left.Value = true;
                        right.Value = true;
                    });

                    Thread.Sleep(100);
                }
            });

            t1.Start();
        }
    }
}
