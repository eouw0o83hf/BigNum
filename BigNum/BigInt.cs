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
        
        public BigInt(long input)
        {
            if (input == 0)
            {
                _bytes = new byte[] { 0 };
                return;
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
            var charArray = _bytes
                                .Reverse()
                                .Select(a => (char) (a + '0'))
                                .ToArray();
            return new string(charArray);
        }
    }
}
