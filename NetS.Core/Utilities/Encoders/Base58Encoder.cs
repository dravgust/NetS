using System;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace NetS.Core.Utilities.Encoders
{
    /// <summary>
    /// Base58Check Encoding / Decoding (Bitcoin-style)
    /// </summary>
    /// <remarks>
    /// See here for more details: https://en.bitcoin.it/wiki/Base58Check_encoding
    /// </remarks>
    public class Base58CheckEncoder : DataEncoder
    {
        private static readonly Base58Encoder InternalEncoder = new Base58Encoder();
        private const int CHECK_SUM_SIZE = 4;

        /// <summary>
        /// Encodes data with a 4-byte checksum
        /// </summary>
        /// <param name="data">Data to be encoded</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override string Encode(byte[] data, int offset, int count)
        {
            Guard.NotNull(data, nameof(data));
            if (data.Length == 0 || count == 0)
                return string.Empty;

            return InternalEncoder.Encode(AddCheckSum(data));
        }

        /// <summary>
        /// Decodes data in Base58Check format (with 4 byte checksum)
        /// </summary>
        /// <param name="encoded">Data to be decoded</param>
        /// <returns>Returns decoded data if valid; throws FormatException if invalid</returns>
        public override byte[] Decode(string encoded)
        {
            var dataWithCheckSum = InternalEncoder.Decode(encoded);
            var dataWithoutCheckSum = VerifyAndRemoveCheckSum(dataWithCheckSum);

            if (dataWithoutCheckSum == null)
                throw new FormatException("Base58 checksum is invalid");

            return dataWithoutCheckSum;
        }

        private static byte[] AddCheckSum(byte[] data)
        {
            var checkSum = GetCheckSum(data);
            var dataWithCheckSum = ArrayUtils.ConcatArrays(data, checkSum);

            return dataWithCheckSum;
        }
        
        //Returns null if the checksum is invalid
        private static byte[] VerifyAndRemoveCheckSum(byte[] data)
        {
            var result = ArrayUtils.SubArray(data, 0, data.Length - CHECK_SUM_SIZE);
            var givenCheckSum = ArrayUtils.SubArray(data, data.Length - CHECK_SUM_SIZE);
            var correctCheckSum = GetCheckSum(result);

            return givenCheckSum.SequenceEqual(correctCheckSum) ? result : null;
        }

        private static byte[] GetCheckSum(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash1 = sha256.ComputeHash(data);
                var hash2 = sha256.ComputeHash(hash1);

                var result = new byte[CHECK_SUM_SIZE];
                Buffer.BlockCopy(hash2, 0, result, 0, result.Length);

                return result;
            }
        }
    }

    public class Base58Encoder : DataEncoder
    {
        private const string DIGITS = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        /// <summary>
        /// Encodes data in plain Base58.
        /// </summary>
        /// <param name="data">The data to be encoded</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override string Encode(byte[] data, int offset, int count)
        {
            Guard.NotNull(data, nameof(data));
            if (data.Length == 0 || count == 0)
                return string.Empty;

            var input = new byte[count];
            // Decode byte[] to BigInteger
            var intData = input.Aggregate<byte, BigInteger>(0, (current, t) => current * 256 + t);

            // Encode BigInteger to Base58 string
            var result = string.Empty;
            while (intData > 0)
            {
                var remainder = (int) (intData % 58);
                intData /= 58;
                result = DIGITS[remainder] + result;
            }

            // Append `1` for each leading 0 byte
            for (var i = 0; i < input.Length && input[i] == 0; i++)
            {
                result = '1' + result;
            }

            return result;
        }

        /// <summary>
        /// Decodes data in plain Base58.
        /// </summary>
        /// <param name="encoded">Data to be decoded</param>
        /// <returns>Returns decoded data if valid; throws FormatException if invalid</returns>
        public override byte[] Decode(string encoded)
        {
            Guard.NotNull(encoded, nameof(encoded));

            // Decode Base58 string to BigInteger
            BigInteger intData = 0;
            for (var i = 0; i < encoded.Length; i++)
            {
                var digit = DIGITS.IndexOf(encoded[i]); //slow
                if(digit < 0)
                    throw new FormatException(string.Format("Invalid Base58 character `{0}` at position {1}", encoded[i], i));
                intData = intData * 58 + digit;
            }

            // Encode BigInteger to byte[]
            // Leading zero bytes get encoded as leading `1` characters
            var leadingZeroCount = encoded.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte) 0, leadingZeroCount);
            var byteWithoutLeadingZeros = intData.ToByteArray()
                .Reverse() // to big endian
                .SkipWhile(b => b == 0); // stip sign byte
            var result = leadingZeros.Concat(byteWithoutLeadingZeros).ToArray();

            return result;
        }
    }
}
