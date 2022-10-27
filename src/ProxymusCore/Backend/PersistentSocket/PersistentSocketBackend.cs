using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Message;


namespace ProxymusCore.Backend.PersistentSocket
{
    public class PersistentSocketBackend : IBackend
    {
        public Guid Id => Guid.NewGuid();
        public string Name { get; }
        public PersistentSocketBackendConfiguration Configuration { get; }
        public IEnumerable<IBackendHost> Hosts => _hosts;

        private readonly IList<IBackendHost> _hosts;

        public PersistentSocketBackend(PersistentSocketBackendConfiguration configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.Name = configuration.Name;
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
            var host = _hosts[new Random().Next(_hosts.Count - 1)];
            host.ProcessMessage(message);
        }

        private void InitialiseHosts()
        {
            foreach (var hostConfiguration in Configuration.HostConfigurations)
            {
                var host = new PersistentSocketBackendHost(hostConfiguration);
                _hosts.Add(host);
            }
        }
    }
}