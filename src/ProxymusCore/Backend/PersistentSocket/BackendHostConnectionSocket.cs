using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ProxymusCore.Backend.PersistentSocket
{
    public class BackendHostConnectionSocket
    {
        private PersistentSocketBackendHostConfiguration _config;
        private byte[] _buffer;
        private TcpClient _tcpClient;
        public BackendHostConnectionSocket(PersistentSocketBackendHostConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _buffer = new byte[config.BufferSize];
            _tcpClient = new TcpClient();
        }

        public async void Connect()
        {
            await _tcpClient.ConnectAsync(_config.IpAddress, _config.Port);
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
    }
}