using System;
using System.IO;

namespace NetS.Core.Utilities
{
    public class ApplicationStream : IDisposable
    {
        public Stream Inner
        {
            get;
        }

        public bool IsBigEndian
        {
            get;
            set;
        }

        protected ApplicationStream(Stream inner)
        {
            Inner = inner;
        }

        protected ApplicationStream(byte[] bytes) : this(new MemoryStream(bytes))
        {
            
        }

        public void Write(byte data)
        {
            WriteByte(data);
        }

        public void Read(out byte data)
        {
            ReadByte(out data);
        }

        public void Write(bool data)
        {
            var b = data ? (byte)1 : (byte)0;
            WriteByte(b);
        }

        public void Read(out bool data)
        {
            ReadByte(out byte b);
            data = b == 0;
        }

        public void Read(out int data)
        {
            ReadNumber(out long l, sizeof(int));
            data = (int)l;
        }

        public void Write(int data)
        {
            var l = (long)data;
            WriteNumber(l, sizeof(int));
        }

        public void Read(out uint data)
        {
            ReadNumber(out ulong ul, sizeof(uint));
            data = (uint)ul;
        }

        public void Write(uint data)
        {
            var ul = (ulong)data;
            WriteNumber(ul, sizeof(uint));
        }

        public void Read(ref byte[] arr)
        {
            ReadBytes(ref arr);
        }

        public void Write(byte[] arr)
        {
            WriteBytes(arr);
        }

        public void Read(ref byte[] arr, int offset, int count)
        {
            ReadBytes(ref arr, offset, count);
        }

        public void Write(byte[] arr, int offset, int count)
        {
            WriteBytes(arr, offset, count);
        }

        protected void ReadNumber(out long value, int size)
        {
            ReadNumber(out ulong uvalue, size);
            value = unchecked((long)uvalue);
        }

        protected void WriteNumber(long value, int size)
        {
            var uvalue = unchecked((ulong)value);
            WriteNumber(uvalue, size);
        }

        protected void ReadNumber(out ulong value, int size)
        {
            var bytes = new byte[size];

            ReadBytes(ref bytes);

            if (IsBigEndian) Array.Reverse(bytes);

            ulong valueTemp = 0;
            for (var i = 0; i < bytes.Length; i++)
            {
                var @ulong = (ulong)bytes[i];
                valueTemp += @ulong << (i * 8);
            }
            value = valueTemp;
        }

        protected void WriteNumber(ulong value, int size)
        {
            var bytes = new byte[size];
            for (var i = 0; i < size; i++)
            {
                bytes[i] = (byte)(value >> i * 8);
            }

            if (IsBigEndian) Array.Reverse(bytes);

            WriteBytes(bytes);
        }

        protected void WriteByte(byte data)
        {
            Inner.WriteByte(data);
        }

        protected void ReadByte(out byte data)
        {
            var read = Inner.ReadByte();
            if(read == -1)
                throw new EndOfStreamException("No more byte to read");
            data = (byte) read;
        }

        protected void ReadBytes(ref byte[] data, int offset = 0, int count = -1)
        {
            Guard.NotNull(data, nameof(data));

            if (data.Length == 0) return;
            count = count == -1 ? data.Length : count;
            if (count == 0) return;

            var read = Inner.Read(data, offset, count);
            if (read == 0)
                throw new EndOfStreamException("No more byte to read");
        }

        protected void WriteBytes(byte[] data, int offset = 0, int count = -1)
        {
            Guard.NotNull(data, nameof(data));
            if (data.Length == 0) return;
            count = count == -1 ? data.Length : count;
            if (count == 0) return;

            Inner.Write(data, offset, count);
        }

        public void Dispose()
        {
            Inner.Dispose();
        }
    }
}
