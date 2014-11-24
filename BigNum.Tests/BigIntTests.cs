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
        public void Initialize_LogEdgeCase()
        {
            var sut = new BigInt(10);
            Assert.That(sut.ToString(), Is.EqualTo("10"));
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

        [Test]
        public void InitializeFromString_Invalid()
        {
            Assert.Throws<ArgumentException>(() => new BigInt("-42803rjfoiew"));
        }

        [Test]
        public void InitializeFromString_HandlesNegative()
        {
            var num = new BigInt("-40");
            Assert.That(num.ToString(), Is.EqualTo("-40"));
        }

        [Test]
        public void InitializeFromString_BiggerThanLong_Positive()
        {
            var num = new BigInt("48025029359374092839702839709283029835823023058319294");
            Assert.That(num.ToString(), Is.EqualTo("48025029359374092839702839709283029835823023058319294"));
        }

        [Test]
        public void InitializeFromString_BiggerThanLong_Negative()
        {
            var num = new BigInt("-48025029359374092839702839709283029835823023058319294");
            Assert.That(num.ToString(), Is.EqualTo("-48025029359374092839702839709283029835823023058319294"));
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

        #region Comparison

        [Test]
        public void CompareTo_NonBigInt()
        {
            var num1 = new BigInt(0);
            var num2 = DateTime.Now;
            Assert.Throws<ArgumentException>(() => num1.CompareTo(num2));
        }

        [Test]
        public void CompareTo_Sign_Positive()
        {
            var num1 = new BigInt(5);
            var num2 = new BigInt(-5);
            Assert.That(num1.CompareTo(num2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_Sign_Negative()
        {
            var num1 = new BigInt(-5);
            var num2 = new BigInt(5);
            Assert.That(num1.CompareTo(num2), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_Length_Shorter_Positive()
        {
            var num1 = new BigInt(100);
            var num2 = new BigInt(90);
            Assert.That(num1.CompareTo(num2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_Length_Shorter_Negative()
        {
            var num1 = new BigInt(-100);
            var num2 = new BigInt(-90);
            Assert.That(num1.CompareTo(num2), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_Length_Longer_Positive()
        {
            var num1 = new BigInt(90);
            var num2 = new BigInt(100);
            Assert.That(num1.CompareTo(num2), Is.LessThan(0));
        }

        [Test]
        public void CompareTo_Length_Longer_Negative()
        {
            var num1 = new BigInt(-90);
            var num2 = new BigInt(-100);
            Assert.That(num1.CompareTo(num2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_Value_Greater_Positive()
        {
            var num1 = new BigInt(4823);
            var num2 = new BigInt(4803);
            Assert.That(num1.CompareTo(num2), Is.GreaterThan(0));
        }

        [Test]
        public void CompareTo_Value_Greater_Negative()
        {
            var num1 = new BigInt(-4803);
            var num2 = new BigInt(-4823);
            Assert.That(num1.CompareTo(num2), Is.GreaterThan(0));
        }

        [Test]
        public void Compareto_Value_Less_Positive()
        {
            var num1 = new BigInt(4803);
            var num2 = new BigInt(4823);
            Assert.That(num1.CompareTo(num2), Is.LessThan(0));
        }

        [Test]
        public void Compareto_Value_Less_Negative()
        {
            var num1 = new BigInt(-4823);
            var num2 = new BigInt(-4803);
            Assert.That(num1.CompareTo(num2), Is.LessThan(0));
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

        [Test]
        public void Add_BiggerThanLong()
        {
            var num1 = new BigInt("48025029359374092839702839709283029835823023058319294");
            var num2 = new BigInt("10000000000000000000000000000000000000000000000000001");
            var expected = new BigInt("58025029359374092839702839709283029835823023058319295");
            Assert.That(num1 + num2, Is.EqualTo(expected));
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
        public void Subtract_SingleDigitToNegative()
        {
            var num1 = new BigInt(5);
            var num2 = new BigInt(8);
            var expected = new BigInt(-3);

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
        public void Subtract_SimpleDoubleDigitNegative()
        {
            var num1 = new BigInt(17);
            var num2 = new BigInt(28);
            var expected = new BigInt(-11);

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

        [Test]
        public void Subtract_PositiveFromNegative()
        {
            var num1 = new BigInt(-58728);
            var num2 = new BigInt(48);
            var expected = new BigInt(-58776);

            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Subtract_NegativeToPositive()
        {
            var num1 = new BigInt(-20);
            var num2 = new BigInt(-34);
            var expected = new BigInt(14);

            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Subtract_PositiveToNegative()
        {
            var num1 = new BigInt(20);
            var num2 = new BigInt(34);
            var expected = new BigInt(-14);

            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Subtract_Catchall()
        {
            var signs = new[] { -1, 1 };
            for (var i = 0; i < 1000; ++i)
            {
                for (var j = 0; j < 100; ++j)
                {
                    foreach (var iSign in signs)
                    {
                        foreach (var jSign in signs)
                        {
                            var num1 = new BigInt(i * iSign);
                            var num2 = new BigInt(j * jSign);
                            var expected = new BigInt((i * iSign) - (j * jSign));
                            Assert.That(num1.Subtract(num2), Is.EqualTo(expected));
                        }
                    }
                }
            }
        }

        #endregion

        #region Multiplication

        [Test]
        public void Multiplication_SingleDigit()
        {
            var num1 = new BigInt(2);
            var num2 = new BigInt(4);
            var expected = new BigInt(8);
            Assert.That(num1.Multiply(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_SingleDigit_LeftNegative()
        {
            var num1 = new BigInt(-2);
            var num2 = new BigInt(4);
            var expected = new BigInt(-8);
            Assert.That(num1.Multiply(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_SingleDigit_RightNegative()
        {
            var num1 = new BigInt(2);
            var num2 = new BigInt(-4);
            var expected = new BigInt(-8);
            Assert.That(num1.Multiply(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_SingleDigit_BothNegative()
        {
            var num1 = new BigInt(-2);
            var num2 = new BigInt(-4);
            var expected = new BigInt(8);
            Assert.That(num1.Multiply(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_JaggedTwoDigit()
        {
            var num1 = new BigInt(12);
            var num2 = new BigInt(7);
            var expected = new BigInt(84);
            Assert.That(num1.Multiply(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_TwoDigit()
        {
            var num1 = new BigInt(12);
            var num2 = new BigInt(12);
            var expected = new BigInt(144);
            Assert.That(num1.Multiply(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_ThreeDigit()
        {
            var num1 = new BigInt(482);
            var num2 = new BigInt(976);
            var expected = new BigInt(470432);
            Assert.That(num1.Multiply(num2), Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_BiggerThanLong()
        {
            var num1 = new BigInt(9223372036854775806);
            var num2 = new BigInt(9223372036854775806);
            var expected = new BigInt("85070591730234615828950163710522949636");
            Assert.That(num1 * num2, Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_OutrageouslyLong()
        {
            var num1 = new BigInt("85070591730234615828950163710522949636");
            var expected = new BigInt("7237005577332262207696084827656313479035278819920499616848843295441792532496");
            Assert.That(num1 * num1, Is.EqualTo(expected));
        }

        [Test]
        public void Multiplication_OutrageouslyLongAndJagged()
        {
            var num1 = new BigInt("85070591730234615828950163710522949636");
            var num2 = new BigInt("7237005577332262207696084827656313479035278819920499616848843295441792532496");
            var expected = new BigInt("615656346818663736890864863094402685240458120927376293923527047632440653142966908917270607696787020784216301371456");
            Assert.That(num1 * num2, Is.EqualTo(expected));
        }

        #endregion

        #region Division

        [Test]
        public void Division_RightGreater_Zero()
        {
            var num1 = new BigInt(582039);
            var num2 = new BigInt(4802580293);
            var expected = new BigInt(0);
            Assert.That(num1 / num2, Is.EqualTo(expected));
        }

        #endregion
    }
}
