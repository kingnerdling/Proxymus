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

        private ConcurrentQueue<byte[]> _messageQueue;

        public HeaderLengthMessageProcessor()
        {
            _messageQueue = new ConcurrentQueue<byte[]>();
        }
        public void AddData(byte[] data)
        {
            _messageQueue.Enqueue(data);
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