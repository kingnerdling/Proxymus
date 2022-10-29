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
        public int MaxClients { get; set; }
        public int MessageQueueLength { get; set; }
        public int IdleTimeout { get; set; }
        public int BufferSize { get; set; }
        public string MessageProcessor { get; set; }
    }
}