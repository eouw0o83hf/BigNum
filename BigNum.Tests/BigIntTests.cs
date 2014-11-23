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
    }
}
