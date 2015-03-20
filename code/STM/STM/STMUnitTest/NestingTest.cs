using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;

namespace STMUnitTest
{
    [TestClass]
    public class NestingTest
    {
        [TestMethod]
        public void NestingOrElseTest()
        {
            var buffer1 = new STM.Collections.Queue<int>();
            var buffer2 = new STM.Collections.Queue<int>();

            buffer2.Enqueue(10);

            var result = STMSystem.Atomic(() =>
            {
                if (buffer1.Count == 0)
                {
                    STMSystem.Retry();
                }

                return buffer1.Dequeue();
            },
            () =>
            {
                if (buffer2.Count == 0)
                {
                    STMSystem.Retry();
                }

                return buffer2.Dequeue();
            });

            Assert.IsTrue(result == 10);
            Assert.IsTrue(buffer1.Count == 0);
            Assert.IsTrue(buffer2.Count == 0);
        }

        [TestMethod]
        public void NestingOrElseTest2()
        {
            var tm1 = new TMVar<int>(1);
            var tm2 = new TMVar<int>(2);

            STMSystem.Atomic(() =>
            {
                tm1.Value = 10;

                STMSystem.Atomic(() =>
                {
                    tm1.Value = 20;
                });

                var temp = tm1.Value;

                tm2.Value = temp*temp;
            });

            Assert.IsTrue(tm2.Value == 400);
        }

        [TestMethod]
        public void NestingOrElseTest3()
        {
            var tm1 = new TMVar<int>(1);
            var tm2 = new TMVar<int>(2);

            STMSystem.Atomic(() =>
            {
                tm1.Value = 10;

                STMSystem.Atomic(() =>
                {
                    if (tm1.Value == 1)
                    {
                        STMSystem.Retry();
                    }
                    tm1.Value = 20;
                },
                    () =>
                    {
                        tm1.Value = 50;
                    });

                var temp = tm1.Value;

                tm2.Value = temp * temp;
            });

            Assert.IsTrue(tm2.Value == 2500);
        }

        [TestMethod]
        public void NestingEnclosingWriteTest()
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

            Assert.AreEqual<string>(result, "abcdef");
        }
    }
}
