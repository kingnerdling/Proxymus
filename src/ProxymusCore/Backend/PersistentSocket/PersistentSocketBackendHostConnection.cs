using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
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

        public PersistentSocketBackendHostConnection(PersistentSocketBackendHostConfiguration configuration, IMessageProcessor messageProcessor, Func<IMessage> newMessageCallback, Action<IMessage> processedMessageCallback)
        {
            this.Id = Guid.NewGuid();
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            this._newMessageCallback = newMessageCallback ?? throw new ArgumentNullException(nameof(newMessageCallback));
            this._processedMessageCallback = processedMessageCallback;
            this._receiveBuffer = new byte[Configuration.BufferSize];
        }

        public void Start()
        {
            Connect();
        }

        public void Connect()
        {
            _tcpClient = new TcpClient();
            _tcpClient.BeginConnect(Configuration.IpAddress, Configuration.Port, TcpClient_Connect, null);
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
                        Thread.Sleep(Configuration.ReconnectInterval);
                        Connect();
                        return;
                    }
                    _clientMetrics.NewClient();
                    _lastConnectionDate = DateTime.UtcNow;
                    _connected = true;
                    _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, TcpClient_Read, null);
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
                    if (!_tcpClient.Connected)
                    {
                        Close();
                        return;
                    }

                    var intLen = _tcpClient.GetStream().EndRead(ar);
                    var data = new byte[intLen];
                    Array.Copy(_receiveBuffer, data, intLen);
                    _messageProcessor.AddData(data);
                    _clientMetrics.DataReceived(intLen);
                    while (_messageProcessor.HasMessages)
                    {
                        var msgByte = _messageProcessor.NextMessage();
                        if (msgByte != null && _currentMessage != null)
                        {
                            _currentMessage.ResponseData = msgByte;
                            _messageMetrics.ProcessedMessage();
                            _processedMessageCallback(_currentMessage);
                        }
                    }
                    _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, TcpClient_Read, null);
                }
            }
            catch (System.Exception)
            {
                Close();
                return;
            }
        }

        private void Close()
        {
            _connected = false;
            if (_tcpClient != null)
            {
                _tcpClient.Close();
                _tcpClient.Dispose();
                _tcpClient = null;
            }

            if (!_isDisconnecting)
            {
                Connect();
            }
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
                _clientMetrics.DataSent(_currentMessage.RequestData.Length);
                _tcpClient.GetStream().WriteAsync(_currentMessage.RequestData, 0, _currentMessage.RequestData.Length);
            }
        }
    }
}