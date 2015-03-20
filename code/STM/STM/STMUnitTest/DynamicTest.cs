using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;

namespace STMUnitTest
{
    [TestClass]
    public class DynamicTest
    {
        [TestMethod]
        public void TestDynamicTypeInTMVar()
        {
            const string stringForTest = "abc";
            var dyna = new TMVar<dynamic>(0);
            STMSystem.Atomic(() => dyna.Value = stringForTest);
            Assert.AreEqual(stringForTest, dyna.Value);
        }
    }
}
