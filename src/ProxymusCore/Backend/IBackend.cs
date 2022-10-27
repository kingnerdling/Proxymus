using ProxymusCore.Message;

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
    }
}