using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Message;
using ProxymusCore.MessageProcessor;

namespace ProxymusCore.Backend.PersistentSocket
{
    public class PersistentSocketBackendHost : IBackendHost
    {
        public Guid Id => Guid.NewGuid();
        public string Name { get; }
        public PersistentSocketBackendHostConfiguration Configuration { get; }

        public IEnumerable<IBackendConnection> Connections => Connections;
        private readonly IList<PersistentSocketBackendHostConnection> _connections;

        private BlockingCollection<IMessage> _messageQueue;

        public PersistentSocketBackendHost(PersistentSocketBackendHostConfiguration configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.Name = configuration.Name;
            _connections = new List<PersistentSocketBackendHostConnection>();
            _messageQueue = new BlockingCollection<IMessage>();
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
            _messageQueue.Add(message);
        }

        private void InitialiseConnections()
        {
            for (int i = 0; i < Configuration.MaxClients; i++)
            {
                var messageProcessor = (IMessageProcessor)Activator.CreateInstance(Type.GetType(Configuration.MessageProcessor));
                var connection = new PersistentSocketBackendHostConnection(
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

        }

        private IMessage HostConnection_NewMessageCallback()
        {
            return _messageQueue.Take();
        }
    }
}