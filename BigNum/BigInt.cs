using System;
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

            return _compareTo(_bytes, bigInt._bytes, _negative);
        }

        private static int _compareTo(IList<byte> left, IList<byte> right, bool isNegative = false)
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
            if(_compareTo(minuend, subtrahend) < 0)
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

            // Any sign flipping has been handled and we're good to go
            return new BigInt(outputIsNegative, _trimZeros(accumulator));
        }

        #endregion

        #region Multiply/Divide

        public BigInt Multiply(BigInt target)
        {
            return new BigInt(_negative ^ target._negative, _multiplyCore(_bytes, target._bytes));
        }

        /// <summary>
        /// Simple unsigned byte-wise multiplication of two byte arrays
        /// </summary>
        private static byte[] _multiplyCore(IList<byte> left, IList<byte> right)
        {
            var bytes = new List<byte>();

            // Since multiplication is a commutative operation, we just need to
            // iterate through all possible number pairs and multiply them, then
            // add them together.
            for (var i = 0; i < left.Count; ++i)
            {
                for (var j = 0; j < right.Count; ++j)
                {
                    while (bytes.Count < i + j + 1) bytes.Add(0);

                    var product = left[i] * right[j];

                    // The only trick is that we have to (a) add the product to the
                    // existing output and (b) place it at the correct location. To
                    // apply it, we'll add in the least significant digit of the product,
                    // then whittle it down digit by digit until it's gone
                    for (var k = 0; product > 0; product /= 10, ++k)
                    {
                        while (bytes.Count < i + j + k + 1) bytes.Add(0);

                        // [i + j + k] will end up being the position that we're targeting
                        // as k works its way toward the more significant digits
                        product += bytes[i + j + k];
                        bytes[i + j + k] = (byte)(product % 10);
                    }
                }
            }

            // Remove any leading zeros
            return bytes
                .AsEnumerable()
                .Reverse()
                .SkipWhile(a => a == 0)
                .Reverse()
                .ToArray();
        }

        public BigInt Divide(BigInt target)
        {
            return _divideCore(this, target).Item1;
        }

        /// <summary>
        /// Divides left by right, returning [quotient, remainder]
        /// </summary>
        private static Tuple<BigInt, BigInt> _divideCore(BigInt left, BigInt right)
        {

            // Here's the tough one. There are two main phases: edge cases and actual division.

            // 1: Handle all of the edge cases

            // Divide by 0
            if (right.Equals(Zero))
            {
                throw new DivideByZeroException();
            }

            // Denominator > Numerator
            if (_compareTo(left._bytes, right._bytes) < 0)
            {
                return new Tuple<BigInt, BigInt>(Zero, left);
            }

            // At this point we need to start caring about signs since
            // the output will be signed
            var outputSign = right._negative ? !left._negative : left._negative;

            // Divide by 1
            if (right._bytes.SequenceEqual(One._bytes))
            {
                return new Tuple<BigInt, BigInt>(new BigInt(outputSign, left._bytes), Zero);
            }

            // Divide by Self
            if (right._bytes.SequenceEqual(left._bytes))
            {
                return new Tuple<BigInt, BigInt>(new BigInt(outputSign, One._bytes), Zero);
            }

            // At this point, the numerator is greater than the denominator and the
            // denominator is not 0. Since division relies on most-significant figure
            // first, we'll be working in reverse of the normal procedures.
            var bigEndianOutputList = new List<byte>();

            // This is going to get modified as we go along - as long division, instead
            // of trailing down and down, we're doing in-place replacements so that
            // we just keep pulling out the divided amount every time a quotient component
            // is determined. This is little-endian like the inputs.
            var accumulator = left._bytes;

            // Since we can only ever have 10 outputs, we can just enumerate them
            // and then lookup against this mapping instead of re-multiplying every time.
            // We'll still be doing modifications and comparisons, but this is the
            // expensive bit.
            var quotientMemo = Enumerable
                .Range(0, 10)
                // Saving this off as an anonymously-typed list so it can be given
                // a guaranteed, static order
                .OrderByDescending(a => a)
                .Select(a => new
                {
                    QuotientComponent = (byte)a,
                    Value = _multiplyCore(right._bytes, new[] { (byte)a }).DefaultIfEmpty((byte)0).ToArray()
                })
                .ToList();

            // Work our way across the numerator, dividing out components from the
            // denominator at every digit. There are some initial edge cases that
            // we could skip with cleverness, but cleverness is not the exercise
            for (var i = 0; i < left._bytes.Length; ++i)
            {
                // Run through all of the possible component quotients and
                // quit as soon as one of them works
                foreach (var q in quotientMemo)
                {
                    // The concept here is to add zeros to the result (which is akin to
                    // placing the multiplied value in the lefmost position during long
                    // division on paper) and see if it's less than or equal to the value
                    // in the accumulator (which is akin to the current bottom-most sub-
                    // total in long division). If it's not, then the quotient component
                    // is too high. If it is, then it's our current match and gets applied
                    var subtractor = Enumerable
                        // The offset is awkward here, but this is the number of places
                        // between the start of the numerator and the current position
                                        .Repeat((byte)0, left._bytes.Length - i - 1)
                        // Since q.Value is little-endian, adding zeroes first is just
                        // power-of-ten left-shifting by that number of places
                                        .Concat(q.Value)
                        // Make it comparable. ToList() is a little better to call
                        // than ToArray(), and we only need an IList for the comparison,
                        // so we're not doing an array here 
                        // Reference: http://stackoverflow.com/questions/1105990/is-it-better-to-call-tolist-or-toarray-in-linq-queries
                                        .ToList();

                    if (_compareTo(subtractor, accumulator) <= 0)
                    {
                        bigEndianOutputList.Add(q.QuotientComponent);
                        // Make sure to update the accumulator's state by subtracting out
                        // the value we just arrived at
                        accumulator = _subtractCore(accumulator, subtractor, false)._bytes;
                        break;
                    }
                }
            }

            var finalOutput = bigEndianOutputList
                // Right now it's big-endian, so leading zeros are discarded
                // befor ethe little-endian conversion
                                .SkipWhile(a => a == 0)
                // If the result is 0, we'll have an empty set. We need an
                // explicit 0 value though, so supply that
                                .DefaultIfEmpty((byte)0)
                // Switch to little endian for the response
                                .Reverse()
                // For the response object
                                .ToArray();

            return new Tuple<BigInt, BigInt>(new BigInt(outputSign, finalOutput), new BigInt(outputSign, accumulator));
        }

        public BigInt Modulus(BigInt target)
        {
            return _divideCore(this, target).Item2;
        }

        #endregion

        #region Powers and Roots

        public BigInt Power(BigInt power)
        {
            if (power < Zero)
            {
                throw new ArgumentException("Don't yet support negative powers", "power");
            }

            // Exponentiaion to the 0th power is an edge case for negatives
            if (power.Equals(Zero))
            {
                return new BigInt(_negative, One._bytes);
            }

            // Since the zero case has been taken care of, just start with 1 as the seed
            // and start multiplying!
            var accumulator = One;

            for (var i = One; i <= power; ++i)
            {
                accumulator *= this;

                if (i > 1 && i < power)
                {
                    var division = _divideCore(power, i);
                    if (division.Item2.Equals(Zero))
                    {
                        return accumulator.Power(division.Item1);
                    }
                }
            }

            return accumulator;
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

        public static BigInt operator %(BigInt left, BigInt right)
        {
            return left.Modulus(right);
        }

        public static BigInt operator ++(BigInt target)
        {
            return target + One;
        }

        public static BigInt operator --(BigInt target)
        {
            return target - One;
        }

        public static BigInt operator &(BigInt left, BigInt right)
        {
            return _bitwiseCompare(left, right, (a, b) => (byte)(a & b));
        }

        public static BigInt operator |(BigInt left, BigInt right)
        {
            return _bitwiseCompare(left, right, (a, b) => (byte)(a | b));
        }

        private static BigInt _bitwiseCompare(BigInt left, BigInt right, Func<byte, byte, byte> compare)
        {
            var maxIndex = Math.Max(left._bytes.Length, right._bytes.Length);
            var output = Enumerable.Repeat((byte)0, maxIndex).ToList();
            for (var i = 0; i < maxIndex; ++i)
            {
                output[i] = compare(left._bytes.ElementAtOrDefault(i), right._bytes.ElementAtOrDefault(i));
            }

            return new BigInt(left._negative, _trimZeros(output));
        }

        // The shifting operations here aren't particularly elegant, but they get the job done simply
        public static BigInt operator <<(BigInt left, int right)
        {
            var factor = (int)Math.Pow(2, right);
            return left * new BigInt(factor);
        }

        public static BigInt operator >>(BigInt left, int right)
        {
            var factor = (int)Math.Pow(2, right);
            return left / new BigInt(factor);
        }

        public static BigInt operator -(BigInt target)
        {
            if (target.Equals(Zero))
            {
                return target;
            }

            return new BigInt(!target._negative, target._bytes);
        }

        public static BigInt operator ^(BigInt root, BigInt power)
        {
            return root.Power(power);
        }

        public static implicit operator BigInt(int input)
        {
            return new BigInt(input);
        }

        #endregion

        #region Static References

        public static readonly BigInt Zero = new BigInt(0);
        public static readonly BigInt One = new BigInt(1);

        #endregion

        #region Helpers

        private static byte[] _trimZeros(IEnumerable<byte> bytes)
        {
            return bytes
                .Reverse()
                .SkipWhile(a => a == 0)
                .Reverse()
                .DefaultIfEmpty((byte) 0)
                .ToArray();
        }

        #endregion
    }
}
