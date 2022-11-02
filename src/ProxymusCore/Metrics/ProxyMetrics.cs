using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxymusCore.Metrics
{
    public class ProxyMetrics
    {
        public Guid Id { get; }
        public string Name { get; }
        public MessageMetrics MessageMetrics { get; }

        public ProxyMetrics(Guid id, string name, MessageMetrics messageMetrics)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MessageMetrics = messageMetrics ?? throw new ArgumentNullException(nameof(messageMetrics));
        }
    }
}