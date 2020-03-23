using System;
using System.Collections.Generic;

namespace NetS.Core.Utilities
{
    public delegate void OnMessageDetected(byte[] message);
    public delegate void OnException(Exception exception);

    public abstract class DataReader
    {
        private const int MAX_MESSAGE_LENGTH = int.MaxValue;

        private readonly byte[] _startMarker;
        private readonly byte[] _endMarker;
        private readonly List<byte> _buffer;

        protected event OnException OnException;
        protected event OnMessageDetected OnMessage;

        public bool HasMessageHandlers => this.OnMessage != null;

        protected DataReader(byte[] endMarker = null, byte[] startMarker = null)
        {
            _startMarker = startMarker ?? new byte[0];
            _endMarker = endMarker ?? new[] { (byte)'\r', (byte)'\n' };

            _buffer = new List<byte>();
        }

        public void Push(byte[] data, int offset = 0, int length = -1)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (data.Length == 0) return;
            length = length == -1 ? data.Length : length;

            try
            {
                for (var i = 0; i < length; i++)
                {
                    this.PushNext(data[i]);
                }
            }
            catch (Exception e)
            {
                OnException?.Invoke(e);
            }
        }

        private void PushNext(byte data)
        {
            if (_startMarker.Length > 0)
            {
                if (_buffer.Count < _startMarker.Length)
                {
                    if (data != _startMarker[_buffer.Count])
                    {
                        _buffer.Clear();

                        if (data != _startMarker[0])
                            return;
                    }
                }
                else
                {
                    if (data == _startMarker[_startMarker.Length - 1])
                    {
                        var check = 1;
                        if (_startMarker.Length > 1)
                        {
                            for (var i = _startMarker.Length - 2; i >= 0; i--, check++)
                            {
                                if (_buffer[_buffer.Count - check] != _startMarker[i])
                                    break;
                            }
                        }
                        if (check == _startMarker.Length)
                        {
                            _buffer.Clear();
                            _buffer.AddRange(_startMarker);
                            return;
                        }
                    }
                }
            }

            _buffer.Add(data);

            if (_buffer.Count >= _endMarker.Length + _startMarker.Length)
            {
                if (data == _endMarker[_endMarker.Length - 1])
                {
                    var check = 1;
                    if (_endMarker.Length > 1)
                    {
                        for (var i = _endMarker.Length - 2; i >= 0; i--, check++)
                        {
                            if (_buffer[_buffer.Count - 1 - check] != _endMarker[i])
                                break;
                        }
                    }
                    if (check == _endMarker.Length)
                    {
                        this.OnMessage?.Invoke(_buffer.ToArray());
                        _buffer.Clear();
                    }
                }
                else if (_buffer.Count >= MAX_MESSAGE_LENGTH)
                {
                    _buffer.Clear();
                }
            }
        }
    }
}
