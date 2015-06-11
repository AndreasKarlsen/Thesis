using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PerformanceTest
{
    public class TestRunner
    {
        private const int NR_ITERATIONS = 7;

        public static void RunTest(Testable test)
        {
            var times = new long[NR_ITERATIONS];
            for (int n = 0; n < 7; n++)
            {
                var stopwatch = Stopwatch.StartNew();
            }
        }
    }

    public abstract class Testable
    {
        public virtual void Setup() { }
        public abstract double Call();
    }
}
