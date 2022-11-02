using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxymusCore.Backend.PersistentSocket
{
    public class PersistentSocketBackendHostConfiguration
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int ClientCount { get; set; }
        public int MaxMessageCount { get; set; }
        public int IdleTimeout { get; set; }
        public int BufferSize { get; set; }
        public string MessageProcessor { get; set; }
        public int ReconnectInterval { get; set; }
    }
}