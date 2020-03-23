namespace NetS.Core.Utilities.Encoders
{
    public static class Extensions
    {
        public static string ToBase58String(this byte[] data)
        {
            return Encoders.Base58.Encode(data);
        }

        public static byte[] FromBase58String(this string base58String)
        {
            return Encoders.Base58.Decode(base58String);
        }

        public static string ToBase58CheckString(this byte[] data)
        {
            return Encoders.Base58Check.Encode(data);
        }

        public static byte[] FromBase58CheckString(this string base58CheckString)
        {
            return Encoders.Base58Check.Decode(base58CheckString);
        }

        public static string ToBase64String(this byte[] data)
        {
            return Encoders.Base64.Encode(data);
        }

        public static byte[] FromBase64String(this string baseBase64String)
        {
            return Encoders.Base64.Decode(baseBase64String);
        }

        public static string ToHexString(this byte[] data)
        {
            return Encoders.Hex.Encode(data);
        }

        public static byte[] FromHexString(this string hexString)
        {
            return Encoders.Hex.Decode(hexString);
        }
    }

    public static class Encoders
    {
        public static readonly Base58Encoder Base58 = new Base58Encoder();
        public static readonly Base58CheckEncoder Base58Check = new Base58CheckEncoder();
        public static readonly Base64Encoder Base64 = new Base64Encoder();
        public static readonly HexEncoder Hex = new HexEncoder();
    }
}
