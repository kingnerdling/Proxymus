using System.Collections.Concurrent;
using ProxymusCore.Message;

namespace ProxymusCore.Metrics
{
    public class MessageMetrics
    {
        public int Received => _received;
        public int Processed => _processed;
        public int Errored => _errored;
        public int Current => _received - _processed;
        public double AverageProcessingTime => _averageProcessingTime;
        public double AverageProcessed => _totalProcessedInRange;
        public DateTime MetricTime => SetTime();



        private int _received;
        private int _processed;
        private int _errored;
        private double _averageProcessingTime;
        private int _totalProcessedInRange;
        private int _totalProcessingTimeInRange;

        private DateTime _currentTime;

        private object _lock;

        public MessageMetrics()
        {
            _lock = new object();
        }
        public void NewMessage()
        {
            Interlocked.Increment(ref _received);
        }

        public void ProcessedMessage(IMessage message)
        {
            SetTime();
            Interlocked.Increment(ref _processed);
            if (!message.Errored)
            {
                Interlocked.Increment(ref _totalProcessedInRange);
                Interlocked.Add(ref _totalProcessingTimeInRange, Convert.ToInt32(message.ResponseDateTime.Subtract(message.RequestDateTime).TotalMilliseconds));
                Interlocked.Exchange(ref _averageProcessingTime, _totalProcessingTimeInRange / _totalProcessedInRange);
            }
            else
            {
                Interlocked.Increment(ref _errored);
            }
        }

        private DateTime SetTime()
        {
            if (DateTime.UtcNow > _currentTime.AddSeconds(10))
            {
                lock (_lock)
                {
                    _currentTime = DateTime.UtcNow;
                }

                Interlocked.Exchange(ref _totalProcessedInRange, 0);
                Interlocked.Exchange(ref _totalProcessingTimeInRange, 0);
                Interlocked.Exchange(ref _averageProcessingTime, 0);
            }
            return _currentTime;
        }
    }
}