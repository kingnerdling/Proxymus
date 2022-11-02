using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxymusCore.Metrics
{
    public class BackendHostMetrics
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public MessageMetrics MessageMetrics { get; set; }
        public IEnumerable<BackendHostConnectionMetrics> BackendHostConnectionMetrics { get; set; }


    }
}