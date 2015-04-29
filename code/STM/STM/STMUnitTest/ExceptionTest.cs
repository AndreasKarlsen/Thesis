using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;

namespace STMUnitTest
{
    [TestClass]
    public class ExceptionTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExceptionInTransaction()
        {
            TMInt i = new TMInt(0);
            TMInt j = new TMInt(0);
            STMSystem.Atomic(() =>
            {
                i.Value = 1;
                throw new InvalidOperationException();
                j.Value = 1;
            });

        }

        [TestMethod]
        public void ExceptionInTransaction2()
        {
            TMInt i = new TMInt(0);
            TMInt j = new TMInt(0);
            try
            {
                STMSystem.Atomic(() =>
                {
                    i.Value = 1;
                    throw new InvalidOperationException();
                    j.Value = 1;
                });
            }
            catch (InvalidOperationException)
            {

            }

            Assert.AreEqual(1, i.Value);
            Assert.AreEqual(0, j.Value);
        }

       
    }
}
