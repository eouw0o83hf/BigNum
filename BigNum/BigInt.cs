using System;
using System.Collections.Generic;
using System.Linq;

namespace BigNum
{
    public class BigInt : IComparable
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

            // We're storing BCDs in each array member.
            // The "+0.01" is to take care of the Log edge case where the input is an
            // exact Log10 match, and it bumps it up to the next size
            _bytes = new byte[(int)Math.Ceiling(Math.Log10(input + .01))];
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

        public int CompareTo(object obj)
        {
            // First: can we even compare these?
            var bigInt = obj as BigInt;
            if (bigInt == null) throw new ArgumentException("Argument must be non-null and explicitly convertible to BigInt", "obj");

            // Second: can we just use the sign?
            if (!_negative && bigInt._negative)
            {
                return 1;
            }
            if (_negative && !bigInt._negative)
            {
                return -1;
            }

            // Third: can we just use the length of the number?
            if (_bytes.Length > bigInt._bytes.Length)
            {
                return _negative ? -1 : 1;
            }
            if (_bytes.Length < bigInt._bytes.Length)
            {
                return _negative ? 1 : -1;
            }

            // Fourth: compare number components
            // Note at this point, the arrays are guaranteed to be the same length
            // and the negative statuses are equivalent
            for (var i = _bytes.Length - 1; i >= 0; --i)
            {
                if (_bytes[i] > bigInt._bytes[i])
                {
                    return _negative ? -1 : 1;
                }
                if (_bytes[i] < bigInt._bytes[i])
                {
                    return _negative ? 1 : -1;
                }
            }

            // Fifth: No comparison remains. These numbers are equal
            return 0;
        }

        #endregion

        #region Arithmetic

        public BigInt Add(BigInt target)
        {
            if (_negative)
            {
                if (!target._negative)
                {
                    return _subtractCore(target._bytes, _bytes, true);
                }
                return _addCore(_bytes, target._bytes, true);
            }
            
            if (target._negative)
            {
                return _subtractCore(_bytes, target._bytes, false);
            }
            return _addCore(_bytes, target._bytes, false);
        }

        public BigInt Subtract(BigInt target)
        {
            return Add(new BigInt(!target._negative, target._bytes));
        }

        private static BigInt _subtractCore(IList<byte> minuend, IList<byte> subtrahend, bool outputIsNegative)
        {
            // todo: if subtrahend > minuend, swap pointers and flip negative

            var accumulator = new List<byte>();
            var max = Math.Max(minuend.Count, subtrahend.Count);

            var carry = false;
            for (var i = 0; i < max; ++i)
            {
                var input0 = minuend.Count > i ? minuend[i] : 0;
                var input1 = subtrahend.Count > i ? subtrahend[i] : 0;
                var input2 = carry ? 1 : 0;

                var difference = input0 - input1 - input2;

                carry = difference < 0;

                if (carry)
                {
                    if (i < max - 1)
                    {
                        difference += 10;
                    }
                    else
                    {
                        difference = -difference;
                        outputIsNegative = !outputIsNegative;
                    }
                }

                accumulator.Add((byte)(difference));
            }

            return new BigInt(outputIsNegative, accumulator.ToArray());
        }

        private static BigInt _addCore(IList<byte> addend1, IList<byte> addend2, bool outputIsNegative)
        {
            var accumulator = new List<byte>();
            var max = Math.Max(addend1.Count, addend2.Count);

            var carry = false;
            for (var i = 0; i < max; ++i)
            {
                var input0 = addend1.Count > i ? addend1[i] : 0;
                var input1 = addend2.Count > i ? addend2[i] : 0;
                var input2 = carry ? 1 : 0;

                var sum = (byte)(input0 + input1 + input2);

                carry = sum >= 10;
                if (carry) sum -= 10;

                accumulator.Add(sum);
            }

            if (carry)
            {
                accumulator.Add(1);
            }

            return new BigInt(outputIsNegative, accumulator.ToArray());
        }

        #endregion
    }
}
