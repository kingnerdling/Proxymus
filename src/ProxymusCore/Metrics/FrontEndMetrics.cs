using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Frontend.Client;

namespace ProxymusCore.Metrics
{
    public class FrontendMetrics
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ClientMetrics ClientMetrics { get; set; }
        public MessageMetrics MessageMetrics { get; set; }
        public IEnumerable<IClient> Clients { get; set; }
        public bool IsListening { get; set; }
    }
}