using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigNum
{
    public class BigInt
    {
        // Little-Endian BCDs
        private readonly byte[] _bytes;
        private readonly bool _negative;

        #region In, Out

        public BigInt(long input)
        {
            // Main edge case
            if (input == 0)
            {
                _bytes = new byte[] { 0 };
                return;
            }

            // If it's negative, we'll track it with a bool, then
            // funnel it through the same logic as a positive
            if (input < 0)
            {
                _negative = true;
                input = -input;
            }

            // We're storing BCDs in each array member
            _bytes = new byte[(int)Math.Ceiling(Math.Log10(input))];
            for (var i = 0; input > 0; input /= 10, ++i)
            {
                _bytes[i] = (byte)(input % 10);
            }
        }

        private BigInt(bool negative, byte[] bytes)
        {
            _bytes = bytes;
            _negative = negative;
        }

        public override string ToString()
        {
            // Put a negative sign in the first position if we need it
            var negativeSign = _negative ? new [] { '-' } : Enumerable.Empty<char>();

            // Bytes are little-endian, so we have to go in reverse order.
            // Adding '0' to the digit is an ASCII hack that gets the int value for ASCII '0',
            // then adds an offset for the current digit.
            var digits = _bytes
                            .Reverse()
                            .Select(a => (char) (a + '0'));

            // Concat() instead of Union() because we need repeated digits to be preserved
            return new string(negativeSign.Concat(digits).ToArray());
        }

        #endregion

        #region Equivalence

        public override bool Equals(object obj)
        {
            var bigInt = obj as BigInt;
            if (bigInt == null) return false;

            return _negative == bigInt._negative && _bytes.SequenceEqual(bigInt._bytes);
        }

        public override int GetHashCode()
        {
            // Naive approach, but this will probably only rarely get used
            return ToString().GetHashCode();
        }

        #endregion

        #region Arithmetic

        public BigInt Add(BigInt target)
        {
            var accumulator = new List<byte>();
            var max = Math.Max(_bytes.Length, target._bytes.Length);

            var carry = false;
            for (var i = 0; i < max; ++i)
            {
                var input0 = _bytes.Length > i ? _bytes[i] : 0;
                var input1 = target._bytes.Length > i ? target._bytes[i] : 0;
                var input2 = carry ? 1 : 0;

                var sum = (byte) (input0 + input1 + input2);

                carry = sum >= 10;
                if (carry) sum -= 10;

                accumulator.Add(sum);
            }

            if (carry)
            {
                accumulator.Add(1);
            }

            return new BigInt(false, accumulator.ToArray());
        }

        #endregion
    }
}
