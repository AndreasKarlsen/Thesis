using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;
using STM.Implementation.Exceptions;

namespace STMUnitTest
{
    [TestClass]
    public class VariousTest
    {
        [TestMethod]
        [ExpectedException(typeof(STMMaxAttemptException))]
        public void MaxAttemptsTest()
        {
            STMSystem.Atomic(() =>
            {
                throw new STMAbortException();
            });
        }
    }
}
