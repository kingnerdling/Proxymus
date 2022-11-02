
namespace ProxymusCore.Metrics
{
    public class BackendMetrics
    {
        public Guid Id { get; }
        public string Name { get; }
        public MessageMetrics MessageMetrics { get; }
        public IEnumerable<BackendHostMetrics> BackendHostsMetrics { get; }

        public BackendMetrics(Guid id, string name, MessageMetrics messageMetrics, IEnumerable<BackendHostMetrics> backendHostsMetrics)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MessageMetrics = messageMetrics ?? throw new ArgumentNullException(nameof(messageMetrics));
            BackendHostsMetrics = backendHostsMetrics ?? throw new ArgumentNullException(nameof(backendHostsMetrics));
        }
    }
}