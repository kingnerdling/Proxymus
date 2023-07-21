using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxymusCore.Message;
using ProxymusCore.MessageProcessor;
using ProxymusCore.Metrics;

namespace ProxymusCore.Backend.PersistentSocket
{
    public class PersistentSocketBackendHost : IBackendHost
    {
        public Guid Id { get; }
        public string Name { get; }
        public PersistentSocketBackendHostConfiguration Configuration { get; }
        public IEnumerable<IBackendConnection> Connections => _connections;
        public bool IsConnected => _connections.Any(x => x.IsConnected);
        public bool IsMaxMessages => _messageQueue.Count() >= Configuration.MaxMessageCount;
        public MessageMetrics MessageMetrics => _messageMetrics;
        private MessageMetrics _messageMetrics = new MessageMetrics();

        private readonly IList<PersistentSocketBackendHostConnection> _connections;
        private readonly BlockingCollection<IMessage> _messageQueue;
        private readonly ILogger<PersistentSocketBackendHost> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly Action<IMessage> _processedMessageCallback;

        public PersistentSocketBackendHost(ILoggerFactory loggerFactory, PersistentSocketBackendHostConfiguration configuration, Action<IMessage> processedMessageCallback)
        {
            this.Id = Guid.NewGuid();

            this._loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this._logger = loggerFactory.CreateLogger<PersistentSocketBackendHost>();
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.Name = configuration.Name;
            _connections = new List<PersistentSocketBackendHostConnection>();
            _messageQueue = new BlockingCollection<IMessage>();
            _processedMessageCallback = processedMessageCallback ?? throw new ArgumentNullException(nameof(processedMessageCallback));
            InitialiseConnections();
        }

        public void Start()
        {
            foreach (var connection in _connections)
            {
                connection.Start();
            }
        }

        public void Stop()
        {
            foreach (var connection in _connections)
            {
                connection.Disconnect();
            }
        }

        public void ProcessMessage(IMessage message)
        {
            _messageMetrics.NewMessage();
            _messageQueue.Add(message);
        }

        private void InitialiseConnections()
        {
            for (int i = 0; i < Configuration.ClientCount; i++)
            {
                var messageProcessor = (IMessageProcessor)Activator.CreateInstance(Type.GetType(Configuration.MessageProcessor));
                var connection = new PersistentSocketBackendHostConnection(_loggerFactory.CreateLogger<PersistentSocketBackendHostConnection>(),
                    Configuration,
                    messageProcessor,
                    HostConnection_NewMessageCallback,
                     HostConnection_ProcessedMessageCallback
                    );

                _connections.Add(connection);
            }
        }

        private void HostConnection_ProcessedMessageCallback(IMessage message)
        {
            _messageMetrics.ProcessedMessage(message);
            _processedMessageCallback(message);
        }

        private IMessage HostConnection_NewMessageCallback()
        {
            return _messageQueue.Take();
        }
    }
}