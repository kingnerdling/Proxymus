
namespace ProxymusCore.Frontend.SocketFrontend
{
    public class SocketFrontendConfiguration
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int MessageQueueLength { get; set; }
        public int IdleTimeout { get; set; }
        public int MaxClients { get; set; }
        public int BufferSize { get; set; }
        public string MessageProcessor { get; set; }
    }
}