using ProxymusCore.Router;
using ProxymusCore.Backend;
using ProxymusCore.Frontend;
using System.Collections.Concurrent;
using ProxymusCore.Message;

namespace ProxymusCore.Proxy
{
    public interface IProxy
    {
        public Guid Id { get; }
        public string Name { get; }
        public int MessageQueueLength { get; }
        public IFrontend Frontend { get; }
        public IBackend Backend { get; }
        public IEnumerable<IMessage> MessageQueue { get; }
        public void Start();
        public void Stop();
    }
}