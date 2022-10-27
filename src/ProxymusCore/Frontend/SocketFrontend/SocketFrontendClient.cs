using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ProxymusCore.Frontend.Client;
using ProxymusCore.Message;
using ProxymusCore.MessageProcessor;

namespace ProxymusCore.Frontend.SocketFrontend
{
    public class SocketFrontendClient : IClient
    {
        public Guid Id => Guid.NewGuid();
        public string Name { get; }
        public DateTime Created => DateTime.UtcNow;


        private readonly Socket _socket;
        private readonly int _bufferSize;
        private byte[] _receiveBuffer;
        private IMessageProcessor _messageProcessor;
        private Action<IMessage> _messageCallback;
        private Action<SocketFrontendClient> _disconnectCallback;


        public SocketFrontendClient(Socket socket, int bufferSize, IMessageProcessor messageProcessor, Action<IMessage> messageCallback, Action<SocketFrontendClient> disconnectCallback)
        {

            this._socket = socket ?? throw new ArgumentNullException(nameof(socket));
            this._messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));

            if (socket.RemoteEndPoint == null)
            {
                throw new NullReferenceException("Remote Endpoint Null");
            }

            IPEndPoint remoteIpEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            if (remoteIpEndPoint == null)
            {
                throw new NullReferenceException("Remote Endpoint Null");
            }

            this.Name = $"{remoteIpEndPoint.Address.ToString()}:{remoteIpEndPoint.Port}";
            this._bufferSize = bufferSize;
            this._receiveBuffer = new byte[_bufferSize];

            _messageCallback = messageCallback ?? throw new ArgumentNullException(nameof(messageCallback));
            _disconnectCallback = disconnectCallback ?? throw new ArgumentNullException(nameof(disconnectCallback));
            _socket.BeginReceive(_receiveBuffer, 0, _bufferSize, SocketFlags.None, Socket_Receive, null);
        }

        private void Socket_Receive(IAsyncResult ar)
        {
            if (!_socket.Connected)
            {
                Disconnect();
                return;
            }

            var intLen = _socket.EndReceive(ar);
            if (intLen == 0)
            {
                Disconnect();
            }
            var data = new byte[intLen];

            Array.Copy(_receiveBuffer, data, intLen);
            _messageProcessor.AddData(data);

            while (_messageProcessor.HasMessages)
            {
                var msgByte = _messageProcessor.NextMessage();
                if (_messageCallback != null && msgByte != null)
                {
                    var msg = new ClientMessage(this, msgByte);
                    _messageCallback(msg);
                }
            }
            _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, Socket_Receive, null);
        }

        private void Disconnect()
        {
            try
            {
                _socket.Disconnect(false);
            }
            catch (System.Exception)
            {
            }
            finally
            {
                if (_disconnectCallback != null)
                {
                    _disconnectCallback(this);
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}