using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PerformanceTestModel
{
    public class TestRunner
    {
        private const int NR_ITERATIONS = 7;

        public static void RunTest(string testName, ITestable test, ResultWriter writer)
        {
            Console.WriteLine("{0} start", testName);
            var times = new long[NR_ITERATIONS];
            for (int n = 0; n < NR_ITERATIONS; n++)
            {
                test.Setup();
                Console.WriteLine("{0} iteration {1} start", testName, n);
                var stopwatch = Stopwatch.StartNew();
                test.Perform();
                stopwatch.Stop();
                times[n] = stopwatch.ElapsedMilliseconds;
                Console.WriteLine("{0} iteration {1} end", testName, n);
            }
            Console.WriteLine("{0} end", testName);

            long low = long.MaxValue, high = long.MinValue;
            int lowIndex = 0, highIndex = 0;
            for (int i = 0; i < NR_ITERATIONS; i++)
            {
                var time = times[i];
                if (time > high)
                {
                    high = time;
                    highIndex = i;
                }

                if (time < low)
                {
                    low = time;
                    lowIndex = i;
                }
            }

            var finalTimes = new List<long>();
            for (int i = 0; i < NR_ITERATIONS; i++)
            {
                if (i != lowIndex && i != highIndex)
                {
                    finalTimes.Add(times[i]);
                }
            }

            var t1 = Math.Pow(4, 2);
            var mean = (double) finalTimes.Sum() / (double)finalTimes.Count;
            var stdv = Math.Sqrt(finalTimes.Sum(t => (t - mean)* (t-mean)) / (finalTimes.Count-1));
            writer.WriteResult(mean, stdv, testName);
        }
    }

    public interface ITestable
    {
        void Setup();
        double Perform();
    }
}
