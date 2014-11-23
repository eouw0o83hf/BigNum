using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BigNum.Tests
{
    [TestFixture]
    public class BigIntTests
    {
        #region Initialization / Stringification

        [Test]
        public void InitializeAndString_0()
        {
            var sut = new BigInt(0);
            Assert.That(sut.ToString(), Is.EqualTo("0"));
        }

        [Test]
        public void InitializeAndString_SeveralDigits()
        {
            var sut = new BigInt(8429750293);
            Assert.That(sut.ToString(), Is.EqualTo("8429750293"));
        }

        [Test]
        public void InitializeAndString_Negative()
        {
            var sut = new BigInt(-38202430);
            Assert.That(sut.ToString(), Is.EqualTo("-38202430"));
        }

        #endregion

        #region Equivalence

        [Test]
        public void Equals_Zero()
        {
            var num1 = new BigInt(0);
            var num2 = new BigInt(0);
            Assert.That(num1, Is.EqualTo(num2));
        }

        [Test]
        public void Equals_Positive()
        {
            var num1 = new BigInt(582392503);
            var num2 = new BigInt(582392503);
            Assert.That(num1, Is.EqualTo(num2));
        }

        [Test]
        public void Equals_Negative()
        {
            var num1 = new BigInt(-49203823);
            var num2 = new BigInt(-49203823);
            Assert.That(num1, Is.EqualTo(num2));
        }

        [Test]
        public void NotEqual_DifferentType()
        {
            var num1 = new BigInt(-49203823);
            var num2 = DateTime.MaxValue;
            Assert.That(num1, Is.Not.EqualTo(num2));
        }

        [Test]
        public void NotEqual_PosNeg()
        {
            var num1 = new BigInt(49203823);
            var num2 = new BigInt(-49203823);
            Assert.That(num1, Is.Not.EqualTo(num2));
        }

        [Test]
        public void NotEqual_RepeatedDigit()
        {
            var num1 = new BigInt(49203815);
            var num2 = new BigInt(49203823);
            Assert.That(num1, Is.Not.EqualTo(num2));
        }

        [Test]
        public void NotEqual_DifferentLengths()
        {
            var num1 = new BigInt(49203815);
            var num2 = new BigInt(4920381);
            Assert.That(num1, Is.Not.EqualTo(num2));
        }

        [Test]
        public void HashEquivalence_NotEqual()
        {
            var num1 = new BigInt(49203823);
            var num2 = new BigInt(-49203823);
            Assert.That(num1.GetHashCode(), Is.Not.EqualTo(num2.GetHashCode()));
        }

        [Test]
        public void HashEquivalence_Equal()
        {
            var num1 = new BigInt(49203823);
            var num2 = new BigInt(49203823);
            Assert.That(num1.GetHashCode(), Is.EqualTo(num2.GetHashCode()));
        }

        #endregion
    }
}
