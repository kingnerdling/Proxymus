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
        private readonly byte[] _receiveBuffer;
        private readonly IMessageProcessor _messageProcessor;
        private readonly Action<IMessage> _processedMessageCallback;
        private TcpClient? _tcpClient;
        private IMessage? _currentMessage;
        private bool _isDisconnecting;
        private ILogger<PersistentSocketBackendHostConnection> _logger;
        private AutoResetEvent _resetEvent;
        private object _lock;
        public PersistentSocketBackendHostConnection(ILogger<PersistentSocketBackendHostConnection> logger, PersistentSocketBackendHostConfiguration configuration, IMessageProcessor messageProcessor, Func<IMessage> newMessageCallback, Action<IMessage> processedMessageCallback)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.Id = Guid.NewGuid();
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            this._newMessageCallback = newMessageCallback ?? throw new ArgumentNullException(nameof(newMessageCallback));
            this._processedMessageCallback = processedMessageCallback;
            this._receiveBuffer = new byte[Configuration.BufferSize];
            this._resetEvent = new AutoResetEvent(false);
            this._lock = new object();
            _tcpClient = new TcpClient();
        }

        public void Start()
        {
            Connect();
        }

        public void Connect()
        {
            lock (_lock)
            {
                Close();
                if (!_isDisconnecting)
                {
                    _tcpClient = new TcpClient();
                    _tcpClient.BeginConnect(Configuration.IpAddress, Configuration.Port, TcpClient_Connect, null);
                }
            }
        }

        private void TcpClient_Connect(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                if (_tcpClient != null)
                {
                    if (!_tcpClient.Connected)
                    {
                        _connected = false;
                        Thread.Sleep(Configuration.ReconnectIntervalMs);
                        Connect();
                        return;
                    }
                    _clientMetrics.NewClient();
                    _lastConnectionDate = DateTime.UtcNow;
                    _connected = true;
                    _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, TcpClient_Read, null);
                    _logger.LogInformation($"{Id}: Connected");
                    Task.Run(() => Process());
                }
            }
        }

        private void TcpClient_Read(IAsyncResult ar)
        {
            try
            {
                if (_tcpClient != null)
                {

                    var intLen = _tcpClient.GetStream().EndRead(ar);
                    if (intLen <= 0)
                    {
                        Connect();
                        return;
                    }
                    var data = new byte[intLen];
                    Array.Copy(_receiveBuffer, data, intLen);
                    _messageProcessor.AddData(data);
                    _clientMetrics.DataReceived(intLen);
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
                        }
                    }
                    _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, TcpClient_Read, _currentMessage.Id);
                    _resetEvent.Set();
                }
                else
                {
                    Connect();
                }
            }
            catch (System.Exception)
            {
                Connect();
            }
        }

        private void Close()
        {
            try
            {
                _connected = false;
                if (_tcpClient != null)
                {
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }
            }
            catch (System.Exception)
            {
            }

            _logger.LogInformation($"{Id}: Disconnected");
        }

        public void Disconnect()
        {
            _isDisconnecting = true;
            Close();
        }

        private void Process()
        {
            while (_tcpClient != null && _tcpClient.Connected)
            {
                _currentMessage = _newMessageCallback();
                _messageMetrics.NewMessage();
                if (!_tcpClient.Connected)
                {
                    _currentMessage.Errored = true;
                    _processedMessageCallback(_currentMessage);
                    Connect();
                    return;
                }

                _tcpClient.GetStream().Write(_currentMessage.RequestData, 0, _currentMessage.RequestData.Length);

                _clientMetrics.DataSent(_currentMessage.RequestData.Length);
                _logger.LogTrace($"{Id}: tx: {Convert.ToHexString(_currentMessage.RequestData)}");
                if (!_resetEvent.WaitOne(Configuration.ReceiveTimeoutMs))
                {
                    _currentMessage.Errored = true;
                    _processedMessageCallback(_currentMessage);
                    Close();
                    return;
                }

            }
        }
    }
}