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
    }
}
