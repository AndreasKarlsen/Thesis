using System;

namespace ACSharp
{
    class Account
    {
        public long Balance { get; set; }

        static void Main()
        {
            var salary = new Account(1000);
            var carLoan = new Account(-50000);
            TransferMoney(1000, salary, carLoan);
        }

        public atomic Account(long balance)
        {
            Balance = balance;
        }

        public static void TransferMoney(long amount, Account from, Account to)
        {
            atomic 
            {
                from.Subtract(amount);
                to.Add(amount);
            }
        }

        public void Add(long amount)
        {
            atomic
            {
                Balance += amount;
            }
        }

        public void Subtract(long amount)
        {
            atomic
            {
                if (Balance - amount >= 0)
                {
                    Balance -= amount;
                }
                else
                {
                    throw new Exception("Insufficient funds!");
                }
            }
        }
    }
}
