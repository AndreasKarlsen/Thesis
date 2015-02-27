    using System;
    using System.Threading;

    public class RaceCondition
    {
        public static atomic int number = 10;

        public static int Main(string[] args)
        {
            Thread t1 = new Thread ( () => {
                atomic{
                    if (number == 10 )           
                        number = number * 3;
                }
             });
            Thread t2 = new Thread( () => {
                atomic{
                    number = 20;
                }
            });
            t1.Start(); t2.Start();
            t1.Join(); t2.Start();
            int result;
            atomic{
			    result = number;          
            }
            Console.WriteLine("Result is: "+result);
            return 0;
        }
    }