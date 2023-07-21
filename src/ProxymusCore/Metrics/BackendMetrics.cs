
namespace ProxymusCore.Metrics
{
    public class BackendMetrics
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public MessageMetrics MessageMetrics { get; set; }
        public IEnumerable<BackendHostMetrics> BackendHostsMetrics { get; set; }
    }
}