namespace NetS.Core.Utilities.Encoders
{
    public abstract class DataEncoder
    {
        internal DataEncoder()
        {
            
        }

        public static bool IsSpace(char c)
        {
            switch (c)
            {
                case '\t':
                case '\n':
                case '\v':
                case '\f':
                case '\r':
                case ' ':
                    return true;
                default:
                    return false;
            }   
        }

        public string Encode(byte[] data)
        {
            return this.Encode(data, 0, data.Length);
        }

        public abstract string Encode(byte[] data, int offset, int count);

        public abstract byte[] Decode(string encoded);
    }
}
