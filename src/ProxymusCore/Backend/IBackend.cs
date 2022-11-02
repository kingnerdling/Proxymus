using ProxymusCore.Message;
using ProxymusCore.Metrics;

namespace ProxymusCore.Backend
{
    public interface IBackend
    {
        public Guid Id { get; }
        public string Name { get; }
        public IEnumerable<IBackendHost> Hosts { get; }
        public void Start();
        public void Stop();
        public void ProcessMessage(IMessage message);
        public Action<IMessage> ProcessedMessageCallback { get; set; }
        public bool IsConnected { get; }
        public MessageMetrics MessageMetrics { get; }
        public bool IsMaxMessages { get; }
    }
}