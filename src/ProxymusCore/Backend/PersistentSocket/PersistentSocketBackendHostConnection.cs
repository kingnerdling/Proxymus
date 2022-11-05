using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProxymusCore.Message;
using ProxymusCore.MessageProcessor;
using ProxymusCore.Metrics;


namespace ProxymusCore.Backend.PersistentSocket
{
    public class PersistentSocketBackendHostConnection : IBackendConnection
    {
        public Guid Id { get; }
        public PersistentSocketBackendHostConfiguration Configuration { get; }
        public bool IsConnected => _connected;
        public MessageMetrics MessageMetrics => _messageMetrics;
        public ClientMetrics ClientMetrics => _clientMetrics;
        public DateTime LastConnectionDate => _lastConnectionDate;

        private DateTime _lastConnectionDate;
        private MessageMetrics _messageMetrics = new MessageMetrics();
        private ClientMetrics _clientMetrics = new ClientMetrics();
        private bool _connected;
        private Func<IMessage> _newMessageCallback { get; }
        private readonly IMessageProcessor _messageProcessor;
        private readonly Action<IMessage> _processedMessageCallback;
        private IMessage? _currentMessage;
        private bool _isDisconnecting;
        private ILogger<PersistentSocketBackendHostConnection> _logger;
        private BackendHostConnectionSocket _socket;

        public PersistentSocketBackendHostConnection(ILogger<PersistentSocketBackendHostConnection> logger, PersistentSocketBackendHostConfiguration configuration, IMessageProcessor messageProcessor, Func<IMessage> newMessageCallback, Action<IMessage> processedMessageCallback)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Id = Guid.NewGuid();
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            this._newMessageCallback = newMessageCallback ?? throw new ArgumentNullException(nameof(newMessageCallback));
            this._processedMessageCallback = processedMessageCallback;
        }

        public void Start()
        {
            Task.Run(() => MonitorConnection());
            Task.Run(() => ProcessMessages());
        }


        public void Disconnect()
        {
            _isDisconnecting = true;
            _connected = false;
            if (_socket != null)
            {
                _socket.Disconnect();
                _socket = null;
            }
        }

        private void MonitorConnection()
        {
            while (!_isDisconnecting)
            {
                try
                {
                    if (_socket == null || !_socket.IsConnected())
                    {
                        _socket = new BackendHostConnectionSocket(Configuration);
                        _socket.Connect();

                        _connected = true;
                    }
                }
                catch (System.Exception)
                {
                    _connected = false;
                }
                Thread.Sleep(Configuration.ReconnectIntervalMs);
            }

        }

        private void ProcessMessages()
        {
            while (!_isDisconnecting)
            {
                try
                {
                    _currentMessage = _newMessageCallback();
                    _messageMetrics.NewMessage();
                    ProcessMessage();
                }
                catch (System.Exception)
                {
                    _currentMessage.Errored = true;
                    _processedMessageCallback(_currentMessage);
                }

            }
        }

        private void ProcessMessage()
        {
            _socket.Send(_currentMessage.RequestData);
            _clientMetrics.DataSent(_currentMessage.RequestData.Length);
            _logger.LogTrace($"{Id}: tx: {Convert.ToHexString(_currentMessage.RequestData)}");

            var timeoutTime = DateTime.UtcNow.AddMilliseconds(Configuration.ReceiveTimeoutMs);
            while (DateTime.UtcNow < timeoutTime)
            {
                var data = _socket.Receive();
                _messageProcessor.AddData(data);
                _clientMetrics.DataReceived(data.Length);
                _logger.LogTrace($"{Id}: rx: {Convert.ToHexString(data)}");

                while (_messageProcessor.HasMessages)
                {
                    var msgByte = _messageProcessor.NextMessage();
                    if (msgByte != null && _currentMessage != null)
                    {
                        _currentMessage.ResponseData = msgByte;
                        _currentMessage.ResponseDateTime = DateTime.UtcNow;
                        _messageMetrics.ProcessedMessage(_currentMessage);
                        _processedMessageCallback(_currentMessage);
                        return;
                    }
                }
            }
        }

    }
}