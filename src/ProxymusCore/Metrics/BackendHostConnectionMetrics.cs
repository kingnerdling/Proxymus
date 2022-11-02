using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxymusCore.Metrics
{
    public class BackendHostConnectionMetrics
    {
        public Guid Id { get; set; }
        public ClientMetrics ClientMetrics { get; set; }
        public DateTime LastConnectionDate { get; set; }
        public MessageMetrics MessageMetrics { get; set; }
        public bool IsConnected { get; set; }
    }
}