using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ProxymusCore.Message;


namespace ProxymusCore.MessageProcessor
{
    public class HeaderLengthMessageProcessor : IMessageProcessor
    {
        public bool HasMessages { get { return _messageQueue.Count > 0; } }

        private List<byte> _buffer = new List<byte>();
        private ConcurrentQueue<byte[]> _messageQueue;

        public HeaderLengthMessageProcessor()
        {
            _messageQueue = new ConcurrentQueue<byte[]>();
        }
        public void AddData(byte[] data)
        {
            _buffer.AddRange(data);

            while (true)
            {
                if (_buffer.Count < 2)
                {
                    return;
                }

                var msglen = BitConverter.ToInt16(_buffer.Take(2).ToArray(), 0);
                msglen += 2;

                if (_buffer.Count < msglen)
                {
                    return;
                }

                var message = _buffer.Take(msglen);
                _messageQueue.Enqueue(data);
                _buffer.RemoveRange(0, msglen);
            }
        }

        public byte[] NextMessage()
        {
            if (_messageQueue.TryDequeue(out var msg))
            {
                return msg;
            }
            throw new KeyNotFoundException("No Message Available");
        }
    }
}