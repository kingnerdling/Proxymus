using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Proxy;

namespace ProxymusCore.Metrics
{
    public class Metrics
    {
        public ProxyMetrics ProxyMetrics { get; }
        public FrontendMetrics FrontendMetrics { get; }
        public BackendMetrics BackendMetrics { get; }
        public Metrics(ProxyMetrics proxyMetrics, FrontendMetrics frontendMetrics, BackendMetrics backendMetrics)
        {
            ProxyMetrics = proxyMetrics ?? throw new ArgumentNullException(nameof(proxyMetrics));
            FrontendMetrics = frontendMetrics ?? throw new ArgumentNullException(nameof(frontendMetrics));
            BackendMetrics = backendMetrics ?? throw new ArgumentNullException(nameof(backendMetrics));
        }

        public static Metrics Create(IProxy proxy)
        {
            var backendHostMetrics = new List<BackendHostMetrics>();
            foreach (var backendHost in proxy.Backend.Hosts)
            {
                var backendHostConnectionMetrics = new List<BackendHostConnectionMetrics>();
                foreach (var backendHostConnection in backendHost.Connections)
                {
                    backendHostConnectionMetrics.Add(new BackendHostConnectionMetrics
                    {
                        Id = backendHostConnection.Id,
                        ClientMetrics = backendHostConnection.ClientMetrics,
                        MessageMetrics = backendHostConnection.MessageMetrics,
                        IsConnected = backendHostConnection.IsConnected,
                        LastConnectionDate = backendHostConnection.LastConnectionDate
                    });
                }

                backendHostMetrics.Add(new BackendHostMetrics
                {
                    Id = backendHost.Id,
                    Name = backendHost.Name,
                    BackendHostConnectionMetrics = backendHostConnectionMetrics,
                    MessageMetrics = backendHost.MessageMetrics
                });
            }

            var backendMetrics = new BackendMetrics
            {
                Id = proxy.Backend.Id,
                Name = proxy.Backend.Name,
                MessageMetrics = proxy.Backend.MessageMetrics,
                BackendHostsMetrics = backendHostMetrics
            };

            var frontendMetrics = new FrontendMetrics
            {
                ClientMetrics = proxy.Frontend.ClientMetrics,
                Clients = proxy.Frontend.Clients,
                Id = proxy.Frontend.Id,
                MessageMetrics = proxy.Frontend.MessageMetrics,
                Name = proxy.Frontend.Name,
                IsListening = proxy.Frontend.IsListening,
                ClientCount = proxy.Frontend.Clients.Count()
            };

            var proxyMetrics = new ProxyMetrics
            {
                Id = proxy.Id,
                Name = proxy.Name,
                MessageMetrics = proxy.MessageMetrics
            };

            return new Metrics(proxyMetrics, frontendMetrics, backendMetrics);
        }
    }
}