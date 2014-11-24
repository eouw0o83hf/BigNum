﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BigNum
{
    /// <summary>
    /// A straightforward, immutable, arbitrarily-sized integer
    /// </summary>
    public class BigInt : IComparable
    {
        #region Immutable State

        // Little-Endian BCDs
        private readonly byte[] _bytes;
        private readonly bool _negative;

        #endregion

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

        public BigInt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be empty", "input");
            }

            if (input[0] == '-')
            {
                _negative = true;
                input = input.Substring(1);
            }

            if (!input.All(char.IsDigit))
            {
                throw new ArgumentException("Input must contain only numeric characters", "input");
            }

            _bytes = input
                        .Select(a => (byte) (a - '0'))  // Easy char conversion
                        .Reverse()                      // Little-endian
                        .ToArray();
        }

        private BigInt(bool negative, byte[] bytes)
        {
            _bytes = bytes;
            if (bytes.Length == 0 || (bytes.Length == 1 && bytes[0] == 0))
            {
                return;
            }
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

            return CompareTo(_bytes, bigInt._bytes, _negative);
        }

        private static int CompareTo(IList<byte> left, IList<byte> right, bool isNegative = false)
        {
            // Third: can we just use the length of the number?
            if (left.Count > right.Count)
            {
                return isNegative ? -1 : 1;
            }
            if (left.Count < right.Count)
            {
                return isNegative ? 1 : -1;
            }

            // Fourth: compare number components
            // Note at this point, the arrays are guaranteed to be the same length
            // and the negative statuses are equivalent
            for (var i = left.Count - 1; i >= 0; --i)
            {
                if (left[i] > right[i])
                {
                    return isNegative ? -1 : 1;
                }
                if (left[i] < right[i])
                {
                    return isNegative ? 1 : -1;
                }
            }

            // Fifth: No comparison remains. These numbers are equal
            return 0;
        }

        #endregion

        #region Add/Subtract

        public BigInt Add(BigInt target)
        {
            // There are four cases here: one for each positive/negative combination.
            // In the case where they match, we can just use a simple addition algorithm
            // and handle the sign. If they don't match, we have to use a subtraction
            // algorithm, which involves somewhat more complex sign handling.

            if (_negative)
            {
                if (!target._negative)
                {
                    return _subtractCore(_bytes, target._bytes, true);
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
            // Subtraction is just negative addition
            return Add(new BigInt(!target._negative, target._bytes));
        }

        private static BigInt _addCore(IList<byte> addend1, IList<byte> addend2, bool outputIsNegative)
        {
            var accumulator = new List<byte>();
            var max = Math.Max(addend1.Count, addend2.Count);

            var carry = false;
            for (var i = 0; i < max; ++i)
            {
                // Sum up each addend and the carry, if appropriate
                var input0 = addend1.Count > i ? addend1[i] : 0;
                var input1 = addend2.Count > i ? addend2[i] : 0;
                var input2 = carry ? 1 : 0;

                var sum = (byte)(input0 + input1 + input2);

                // Handle each number and carry
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

        private static BigInt _subtractCore(IList<byte> minuend, IList<byte> subtrahend, bool outputIsNegative)
        {
            // We need to assert that the larger argument is first
            if(CompareTo(minuend, subtrahend) < 0)
            {
                var temp = minuend;
                minuend = subtrahend;
                subtrahend = temp;
                outputIsNegative = !outputIsNegative;
            }

            var accumulator = new List<byte>();
            var max = Math.Max(minuend.Count, subtrahend.Count);

            var carry = false;
            for (var i = 0; i < max; ++i)
            {
                // This is the inverse of addition, just subtract all the components
                var input0 = minuend.Count > i ? minuend[i] : 0;
                var input1 = subtrahend.Count > i ? subtrahend[i] : 0;
                var input2 = carry ? 1 : 0;

                var difference = input0 - input1 - input2;

                carry = difference < 0;

                // The trick is that a carry is handled differently if it's
                // the end of the number or a middle case
                if (carry)
                {
                    if (i < max - 1)
                    {
                        // If it's the middle, just carry from the next diigt
                        difference += 10;
                    }
                    else
                    {
                        // If it's at the end, we've run off the edge of the number
                        // and we need to flip the sign
                        difference = -difference;
                        outputIsNegative = !outputIsNegative;
                    }
                }

                accumulator.Add((byte)(difference));
            }

            while (accumulator.Last() == 0 && accumulator.Count > 1)
            {
                accumulator.RemoveAt(accumulator.Count - 1);
            }

            // Any sign flipping has been handled and we're good to go
            return new BigInt(outputIsNegative, accumulator.ToArray());
        }

        #endregion

        #region Multiplication/Divison

        public BigInt Multiply(BigInt target)
        {
            var bytes = new List<byte>();

            for (var i = 0; i < _bytes.Length; ++i)
            {
                for (var j = 0; j < target._bytes.Length; ++j)
                {
                    while (bytes.Count < i + j + 1) bytes.Add(0);

                    var product = _bytes[i] * target._bytes[j];

                    for (var k = 0; product > 0; product /= 10, ++k)
                    {
                        while (bytes.Count < i + j + k + 1) bytes.Add(0);
                        product += bytes[i + j + k];
                        bytes[i + j + k] = (byte)(product % 10);
                    }
                }
            }

            while (bytes.Last() == 0) bytes.RemoveAt(bytes.Count);

            return new BigInt(_negative ^ target._negative, bytes.ToArray());
        }

        public BigInt Divide(BigInt target)
        {
            if (target.Equals(Zero))
            {
                throw new DivideByZeroException();
            }

            if (CompareTo(_bytes, target._bytes) < 0)
            {
                return Zero;
            }

            var outputSign = target._negative ? !_negative : _negative;

            if (target._bytes.SequenceEqual(One._bytes))
            {
                return new BigInt(outputSign, _bytes);
            }

            if (target._bytes.SequenceEqual(_bytes))
            {
                return new BigInt(outputSign, One._bytes);
            }

            // At this point, the numerator is greater than the denominator and the
            // denominator is not 0. Since division relies on most-significant figure
            // first, we'll be working in reverse of the normal procedures.
            var bigEndianOutputList = new List<byte>();
           
            throw new NotImplementedException();

            bigEndianOutputList.Reverse();
            return new BigInt(outputSign, bigEndianOutputList.ToArray());
        }

        #endregion

        #region Operators

        public static bool operator <(BigInt left, BigInt right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(BigInt left, BigInt right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(BigInt left, BigInt right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(BigInt left, BigInt right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static BigInt operator +(BigInt left, BigInt right)
        {
            return left.Add(right);
        }

        public static BigInt operator -(BigInt left, BigInt right)
        {
            return left.Subtract(right);
        }

        public static BigInt operator *(BigInt left, BigInt right)
        {
            return left.Multiply(right);
        }

        public static BigInt operator /(BigInt left, BigInt right)
        {
            return left.Divide(right);
        }

        #endregion

        #region Absolutes

        public static readonly BigInt Zero = new BigInt(0);
        public static readonly BigInt One = new BigInt(1);

        #endregion
    }
}
