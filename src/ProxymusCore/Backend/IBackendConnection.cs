using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Metrics;

namespace ProxymusCore.Backend
{
    public interface IBackendConnection
    {
        public Guid Id { get; }
        public bool IsConnected { get; }
        public MessageMetrics MessageMetrics { get; }
        public ClientMetrics ClientMetrics { get; }
        public DateTime LastConnectionDate { get; }
    }
}