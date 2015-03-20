using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;
using STM;

namespace STMUnitTest
{
    [TestClass]
    public class TMVarTest
    {
        private TMVar<string> _tmString;
        [TestInitialize]
        public void Setup()
        {
            _tmString = SetupInternal();
        }

        private TMVar<string> SetupInternal()
        {
            return new TMVar<string>("abc");
        }

        [TestMethod]
        public void TMVarEqTest()
        {
            Assert.IsTrue(_tmString == "abc");
            Assert.IsFalse(_tmString == "def");
        }

        [TestMethod]
        public void TMVarNotEqTest()
        {
            Assert.IsFalse(_tmString != "abc");
            Assert.IsTrue(_tmString != "def");
        }

        [TestMethod]
        public void TMVarEqTest2()
        {
            var tmvar2 = SetupInternal();
            var tmvar3 = new TMVar<string>("def");
            Assert.IsTrue(_tmString == tmvar2);
            Assert.IsFalse(_tmString == tmvar3);
        }

        [TestMethod]
        public void TMVarNotEqTest2()
        {
            var tmvar2 = SetupInternal();
            var tmvar3 = new TMVar<string>("def");
            Assert.IsFalse(_tmString != tmvar2);
            Assert.IsTrue(_tmString != tmvar3);
        }
    }
}
