using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using STM.Implementation.Lockbased;
using STM;

namespace STMUnitTest
{
    [TestClass]
    public class TMIntTest
    {
        private TMInt _tmInt;
        [TestInitialize]
        public void Setup()
        {
            _tmInt = new TMInt(5);
        }

        [TestMethod]
        public void TMIntPlusTest()
        {
            STMSystem.Atomic(() =>
            {
                _tmInt.Value = _tmInt + 5;
            });

            Assert.AreEqual<int>(10, _tmInt.Value);
        }

        [TestMethod]
        public void TMIntMinusTest()
        {
            STMSystem.Atomic(() =>
            {
                _tmInt.Value = _tmInt - 5;
            });

            Assert.AreEqual<int>(0, _tmInt.Value);
        }

        [TestMethod]
        public void TMIntTimesTest()
        {
            STMSystem.Atomic(() =>
            {
                _tmInt.Value = _tmInt * 5;
            });

            Assert.AreEqual<int>(25, _tmInt.Value);
        }

        [TestMethod]
        public void TMIntDivTest()
        {
            STMSystem.Atomic(() =>
            {
                _tmInt.Value = _tmInt / 5;
            });
            Assert.AreEqual<int>(1, _tmInt.Value);
        }

        [TestMethod]
        public void TMIntModTest()
        {
            STMSystem.Atomic(() =>
            {
                _tmInt.Value = _tmInt % 3;
            });
            Assert.AreEqual<int>(2, _tmInt.Value);
        }

        [TestMethod]
        public void TMIntEqTest()
        {
            Assert.IsTrue(_tmInt == 5);
            Assert.IsFalse(_tmInt == 6);
        }

        [TestMethod]
        public void TMIntNotEqTest()
        {
            Assert.IsFalse(_tmInt != 5);
            Assert.IsTrue(_tmInt != 6);
        }

        [TestMethod]
        public void TMIntLessTest()
        {
            Assert.IsTrue(!(_tmInt < 5));
        }

        [TestMethod]
        public void TMIntGreaterTest()
        {
            Assert.IsTrue(!(_tmInt > 5));
        }

        [TestMethod]
        public void TMIntLessEqTest()
        {
            Assert.IsTrue(_tmInt <= 5);
            Assert.IsTrue(_tmInt <= 6);
        }

        [TestMethod]
        public void TMIntGreaterEqTest()
        {
            Assert.IsTrue(_tmInt >= 5);
            Assert.IsTrue(_tmInt >= 4);
        }

        [TestMethod]
        public void TMIntInc()
        {
            _tmInt++;
            Assert.IsTrue(_tmInt == 6);
            Assert.IsTrue(++_tmInt == 7);
        }

        [TestMethod]
        public void TMIntDec()
        {
            _tmInt--;
            Assert.IsTrue(_tmInt == 4);
            Assert.IsTrue(--_tmInt == 3);
        }

        public void TMIntWithDoubleTest()
        {
            double d = 2.0;
            Assert.IsTrue((_tmInt * d) == 10);
        }
    }
}
