using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            TMVar<string> s = new TMVar<string>("abc");
            TMVar<bool> b = new TMVar<bool>();
            TMVar<Person> p = new TMVar<Person>(new Person { Name = "Bo Hansen", Age = 57 });
        }

        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
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
    }
}
