using ProxymusCore.Frontend.Client;
using ProxymusCore.Message;
using ProxymusCore.MessageProcessor;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
namespace ProxymusCore.Frontend.SocketFrontend
{
    public class SocketFrontend : IFrontend
    {
        public Guid Id => Guid.NewGuid();
        public SocketFrontendConfiguration Configuration { get; }
        public IEnumerable<IClient> Clients => _clients.Select(x => x.Value);
        public bool IsListening { get; }

        private Dictionary<Guid, IClient> _clients => new Dictionary<Guid, IClient>();

        public int MessageCount => throw new NotImplementedException();

        public string Name { get; }

        private Socket _listener;
        private Action<IMessage>? _newMessageCallback;
        public SocketFrontend(SocketFrontendConfiguration configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(Configuration));
            this.Name = configuration.Name;
            // this._messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()
        {
            if (Configuration.IpAddress == "*")
            {
                _listener.Bind(new IPEndPoint(IPAddress.Any, Configuration.Port));
            }
            else
            {
                _listener.Bind(new IPEndPoint(IPAddress.Parse(Configuration.IpAddress), Configuration.Port));
            }
            _listener.Listen(Configuration.MessageQueueLength);
            _listener.BeginAccept(Accept_Callback, null);
        }

        private void Accept_Callback(IAsyncResult ar)
        {
            var socket = _listener.EndAccept(ar);
            var messageProcessor = (IMessageProcessor)Activator.CreateInstance(Type.GetType(Configuration.MessageProcessor));
            var client = new SocketFrontendClient(socket, Configuration.BufferSize, messageProcessor, messageCallback, disconnectCallback);

            _clients.Add(client.Id, client);
            _listener.BeginAccept(Accept_Callback, null);
        }

        private void messageCallback(IMessage msg)
        {
            if (_newMessageCallback != null)
            {
                _newMessageCallback(msg);
            }
        }

        private void disconnectCallback(IClient client)
        {
            _clients.Remove(client.Id);
        }

        public void Stop()
        {
            foreach (var client in _clients)
            {
                client.Value.Dispose();
            }

            _clients.Clear();
        }

        public void New_Message(Action<IMessage> newMessageCallback)
        {
            _newMessageCallback = newMessageCallback;
        }
    }
}