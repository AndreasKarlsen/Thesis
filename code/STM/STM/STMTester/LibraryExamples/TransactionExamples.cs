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
    }
}
