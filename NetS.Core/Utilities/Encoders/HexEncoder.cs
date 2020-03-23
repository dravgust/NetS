using System;
using System.Linq;

namespace NetS.Core.Utilities.Encoders
{
    public class HexEncoder : DataEncoder
    {
        private static readonly string[] HexTbl = Enumerable.Range(0, 256).Select(v => v.ToString("x2")).ToArray();

        private static readonly int[] HexValueArray;

        public bool Space { get; set; }

        static HexEncoder()
        {
            var hexDigits = new char[22]
            {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                'a',
                'b',
                'c',
                'd',
                'e',
                'f',
                'A',
                'B',
                'C',
                'D',
                'E',
                'F'
            };
            var hexValues = new byte[22]
            {
                (byte) 0,
                (byte) 1,
                (byte) 2,
                (byte) 3,
                (byte) 4,
                (byte) 5,
                (byte) 6,
                (byte) 7,
                (byte) 8,
                (byte) 9,
                (byte) 10,
                (byte) 11,
                (byte) 12,
                (byte) 13,
                (byte) 14,
                (byte) 15,
                (byte) 10,
                (byte) 11,
                (byte) 12,
                (byte) 13,
                (byte) 14,
                (byte) 15
            };
            HexEncoder.HexValueArray = new int[hexDigits.Max() + 1];
            for (var i = 0; i < HexEncoder.HexValueArray.Length; i++)
            {
                var index = Array.IndexOf(hexDigits, (char)i);
                var hexValue = -1;
                if (index != -1)
                {
                    hexValue = hexValues[index];
                }
                HexEncoder.HexValueArray[i] = hexValue;
            }
        }

        public override string Encode(byte[] data, int offset, int count)
        {
            Guard.NotNull(data, nameof(data));

            int pos = 0;
            var space = (Space ? Math.Max((count - 1), 0) : 0);
            var s = new char[2 * count + space];
            for (var i = offset; i < offset + count; i++)
            {
                if (Space && i != 0)
                    s[pos++] = ' ';
                var c = HexTbl[data[i]];
                s[pos++] = c[0];
                s[pos++] = c[1];
            }
            return new string(s);
        }

        public override byte[] Decode(string encoded)
        {
            Guard.NotNull(encoded, nameof(encoded));

            if(encoded.Length % 2 == 1)
                throw new FormatException("Invalid Hex String");

            var result = new byte[encoded.Length / 2];
            for (int i = 0, j = 0; i < encoded.Length; i += 2, j++)
            {
                var a = GetHexValue(encoded[i]);
                var b = GetHexValue(encoded[i + 1]);
                if(a == -1 || b == -1)
                    throw new FormatException("Invalid Hex String");

                result[j] = (byte) (((uint) a << 4) | (uint) b);
            }
            return result;
        }

        public bool IsValid(string str)
        {
            return str.ToCharArray().All(c => GetHexValue(c) != -1) && str.Length % 2 == 0;
        }

        public static int GetHexValue(char c)
        {
            return c + 1 <= HexValueArray.Length ? HexValueArray[c] : -1;
        }

        public static bool IsWellFormed(string str)
        {
            try
            {
                Encoders.Hex.Decode(str);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
