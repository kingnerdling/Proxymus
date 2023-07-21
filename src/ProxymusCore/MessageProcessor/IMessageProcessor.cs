using ProxymusCore.Message;

namespace ProxymusCore.MessageProcessor
{
    public interface IMessageProcessor
    {
        public bool HasMessages { get; }
        public byte[] NextMessage();
        public void AddData(byte[] data);
    }
}