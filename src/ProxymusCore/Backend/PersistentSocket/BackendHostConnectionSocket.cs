using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ProxymusCore.Backend.PersistentSocket
{
    public class BackendHostConnectionSocket
    {
        private PersistentSocketBackendHostConfiguration _configuration;
        private byte[] _buffer;
        private TcpClient _tcpClient;
        public BackendHostConnectionSocket(PersistentSocketBackendHostConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _buffer = new byte[_configuration.BufferSize];
        }

        public void Connect()
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(_configuration.IpAddress, _configuration.Port);
        }

        public void Disconnect()
        {
            _tcpClient.Close();
        }

        public void Send(byte[] data)
        {
            _tcpClient.GetStream().Write(data, 0, data.Length);
        }

        public byte[] Receive()
        {
            var intLen = _tcpClient.GetStream().Read(_buffer, 0, _buffer.Length);
            var data = new byte[intLen];
            Array.Copy(_buffer, 0, data, 0, intLen);
            return data;
        }

        public bool IsConnected()
        {
            return _tcpClient.Client.Connected;
        }
    }
}