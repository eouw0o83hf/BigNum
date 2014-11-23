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

        #region Addition

        [Test]
        public void Add_Identity()
        {
            var num1 = new BigInt(0);
            var num2 = new BigInt(812);
            var expected = new BigInt(812);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Add_ZeroAndZero()
        {
            var num1 = new BigInt(0);
            var num2 = new BigInt(0);
            var expected = new BigInt(0);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Add_SingleDigits()
        {
            var num1 = new BigInt(3);
            var num2 = new BigInt(5);
            var expected = new BigInt(8);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Add_CarryToNewDigit()
        {
            var num1 = new BigInt(8);
            var num2 = new BigInt(5);
            var expected = new BigInt(13);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Add_JaggedDigits()
        {
            var num1 = new BigInt(13);
            var num2 = new BigInt(5);
            var expected = new BigInt(18);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Add_MultipleCarry()
        {
            var num1 = new BigInt(987654321);
            var num2 = new BigInt(123456789);
            var expected = new BigInt(1111111110);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Add_SomeCarrySomeDontJagged()
        {
            var num1 = new BigInt(84928);
            var num2 = new BigInt(418011);
            var expected = new BigInt(502939);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Add_NegativeToNegative()
        {
            var num1 = new BigInt(-4912);
            var num2 = new BigInt(-32942);
            var expected = new BigInt(-37854);

            Assert.That(num1.Add(num2), Is.EqualTo(expected));
        }

        #endregion

        #region Subtraction

        [Test]
        public void Subtract_SingleDigitPositive()
        {
            var num1 = new BigInt(8);
            var num2 = new BigInt(5);
            var expected = new BigInt(3);

            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Subtract_SimpleDoubleDigit()
        {
            var num1 = new BigInt(28);
            var num2 = new BigInt(17);
            var expected = new BigInt(11);

            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Subtract_SimpleDoubleDigitCarry()
        {
            var num1 = new BigInt(28);
            var num2 = new BigInt(9);
            var expected = new BigInt(19);

            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Subtract_BigCarryAndNoCarry()
        {
            var num1 = new BigInt(481932045);
            var num2 = new BigInt(32940027);
            var expected = new BigInt(448992018);

            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
        }

        #endregion
    }
}
