using ProxymusCore.Frontend.Client;
using ProxymusCore.Message;
using ProxymusCore.MessageProcessor;
using ProxymusCore.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
namespace ProxymusCore.Frontend.SocketFrontend
{
    public class SocketFrontend : IFrontend
    {
        public Guid Id { get; }
        public SocketFrontendConfiguration Configuration { get; }
        public IEnumerable<IClient> Clients => _clients.Select(x => x.Value);
        public bool IsListening => _isListening;
        public string Name { get; }
        public MessageMetrics MessageMetrics => _messageMetrics;
        public ClientMetrics ClientMetrics => _clientMetrics;

        private MessageMetrics _messageMetrics;
        private ClientMetrics _clientMetrics;
        private ConcurrentDictionary<Guid, IClient> _clients;
        private Socket _listener;
        private Action<IMessage>? _newMessageCallback;
        private bool _isListening;

        public SocketFrontend(SocketFrontendConfiguration configuration)
        {
            this.Id = Guid.NewGuid();
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(Configuration));
            this.Name = configuration.Name;
            this._messageMetrics = new MessageMetrics();
            this._clientMetrics = new ClientMetrics();
            this._clients = new ConcurrentDictionary<Guid, IClient>();
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            StartListener();
        }

        public void Stop()
        {
            StopListener();
            foreach (var client in _clients)
            {
                client.Value.Dispose();
            }

            _clients.Clear();
        }

        public void Pause()
        {
            StopListener();
        }

        public void New_Message(Action<IMessage> newMessageCallback)
        {
            _newMessageCallback = newMessageCallback;
        }

        private void Accept_Callback(IAsyncResult ar)
        {
            _clientMetrics.NewClient();
            if (_listener != null)
            {
                var socket = _listener.EndAccept(ar);
                var messageProcessor = (IMessageProcessor)Activator.CreateInstance(Type.GetType(Configuration.MessageProcessor));
                var client = new SocketFrontendClient(socket, Configuration.BufferSize, messageProcessor, messageCallback, disconnectCallback);

                _clients.TryAdd(client.Id, client);
                _listener.BeginAccept(Accept_Callback, null);
            }
        }

        private void messageCallback(IMessage message)
        {
            _messageMetrics.NewMessage();
            _clientMetrics.DataReceived(message.RequestData.Length);

            if (_newMessageCallback != null)
            {
                _newMessageCallback(message);
            }
        }

        private void disconnectCallback(IClient client)
        {
            _clients.Remove(client.Id, out _);
            if (_listener == null && (_clients.Count < Configuration.MaxClientCount))
            {
                StartListener();
            }
        }

        private void StartListener()
        {
            if (_listener == null)
            {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            if (Configuration.IpAddress == "*")
            {
                _listener.Bind(new IPEndPoint(IPAddress.Any, Configuration.Port));
            }
            else
            {
                _listener.Bind(new IPEndPoint(IPAddress.Parse(Configuration.IpAddress), Configuration.Port));
            }

            _listener.Listen(Configuration.MaxClientQueueCount);
            _listener.BeginAccept(Accept_Callback, null);
            _isListening = true;
        }

        private void StopListener()
        {
            if (_listener != null)
            {
                _isListening = false;
                _listener.Close();
                _listener.Dispose();
                _listener = null;
            }
        }
    }
}