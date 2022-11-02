using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Message;
using ProxymusCore.Metrics;
using ProxymusCore.Router;

namespace ProxymusCore.Backend.PersistentSocket
{
    public class PersistentSocketBackend : IBackend
    {
        public Guid Id { get; }
        public string Name { get; }
        public PersistentSocketBackendConfiguration Configuration { get; }
        public IEnumerable<IBackendHost> Hosts => _hosts;
        public Action<IMessage>? ProcessedMessageCallback { get; set; }
        public IRouter Router { get; }
        public bool IsConnected => _hosts.Any(x => x.IsConnected);
        public bool IsMaxMessages => _hosts.All(x => x.IsMaxMessages);
        public MessageMetrics MessageMetrics => _messageMetrics;
        private MessageMetrics _messageMetrics = new MessageMetrics();
        private readonly IList<IBackendHost> _hosts;

        public PersistentSocketBackend(PersistentSocketBackendConfiguration configuration, IRouter router)
        {
            this.Id = Guid.NewGuid();
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Router = router ?? throw new ArgumentNullException(nameof(router));
            this.Name = configuration.Name == null ? "default" : configuration.Name;
            _hosts = new List<IBackendHost>();
            InitialiseHosts();
        }

        public void Start()
        {
            foreach (var host in _hosts)
            {
                host.Start();
            }
        }

        public void Stop()
        {
            foreach (var host in _hosts)
            {
                host.Stop();
            }
        }

        public void ProcessMessage(IMessage message)
        {
            _messageMetrics.NewMessage();
            var host = Router.Route(_hosts.ToArray());
            if (host != null)
            {
                host.ProcessMessage(message);
            }
            else
            {
                message.Errored = true;
                if (ProcessedMessageCallback != null)
                {
                    ProcessedMessageCallback(message);
                }
            }
        }

        private void InitialiseHosts()
        {
            foreach (var hostConfiguration in Configuration.HostConfigurations)
            {
                var host = new PersistentSocketBackendHost(hostConfiguration, Host_ProcessedMessageCallback);
                _hosts.Add(host);
            }
        }

        private void Host_ProcessedMessageCallback(IMessage message)
        {
            _messageMetrics.ProcessedMessage();
            if (ProcessedMessageCallback != null)
            {
                ProcessedMessageCallback(message);
            }
        }
    }
}