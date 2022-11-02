using ProxymusCore.Message;

namespace ProxymusCore.Metrics
{
    public class MessageMetrics
    {
        public int Received => _received;
        public int Processed => _processed;
        public int Errored => _errored;

        private int _received;
        private int _processed;
        private int _errored;
        private double _averageProcessTime;

        public void NewMessage()
        {
            Interlocked.Increment(ref _received);
        }

        public void ProcessedMessage()
        {
            Interlocked.Increment(ref _processed);
        }
        public void ErroredMessage()
        {
            Interlocked.Increment(ref _errored);
        }
    }
}