using ProxymusCore.Frontend.Client;
using ProxymusCore.Message;
using ProxymusCore.Metrics;

namespace ProxymusCore.Frontend
{
    public interface IFrontend
    {
        public Guid Id { get; }
        public string Name { get; }
        public IEnumerable<IClient> Clients { get; }
        public void Start();
        public void Stop();
        public void Pause();
        public bool IsListening { get; }
        public void New_Message(Action<IMessage> newMessageCallback);
        public MessageMetrics MessageMetrics { get; }
        public ClientMetrics ClientMetrics { get; }
    }
}