using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxymusCore.Metrics
{
    public class ClientMetrics
    {
        public int TotalClients => _totalClients;
        public int BytesReceived => _bytesReceived;
        public int BytesSent => _bytesSent;

        private int _totalClients;
        private int _bytesReceived;
        private int _bytesSent;

        public void NewClient()
        {
            Interlocked.Increment(ref _totalClients);
        }
        public void DataReceived(int count)
        {
            Interlocked.Add(ref _bytesReceived, count);
        }
        public void DataSent(int count)
        {
            Interlocked.Add(ref _bytesSent, count);
        }
    }
}
