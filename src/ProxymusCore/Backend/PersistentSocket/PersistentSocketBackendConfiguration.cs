using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxymusCore.Backend.PersistentSocket
{
    public class PersistentSocketBackendConfiguration
    {
        public string Name { get; set; }
        public IEnumerable<PersistentSocketBackendHostConfiguration> HostConfigurations { get; set; }

    }
}