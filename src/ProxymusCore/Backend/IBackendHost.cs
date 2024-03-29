using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Message;
using ProxymusCore.Metrics;

namespace ProxymusCore.Backend
{
    public interface IBackendHost
    {
        public Guid Id { get; }
        public string Name { get; }
        public IEnumerable<IBackendConnection> Connections { get; }
        public void Start();
        public void Stop();
        public void ProcessMessage(IMessage message);
        public bool IsConnected { get; }
        public bool IsMaxMessages { get; }
        public MessageMetrics MessageMetrics { get; }

    }
}