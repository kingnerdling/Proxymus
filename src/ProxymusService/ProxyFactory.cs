using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Backend;
using ProxymusCore.Backend.PersistentSocket;
using ProxymusCore.Frontend;
using ProxymusCore.Frontend.SocketFrontend;
using ProxymusCore.Proxy;

namespace ProxymusService
{
    public static class ProxyFactory
    {
        public static IProxy Build(IConfiguration configuration)
        {
            var name = configuration.GetSection("Name").Value;
            var messageQueueLength = int.Parse(configuration.GetSection("MessageQueueLength").Value);
            var frontend = BuildFrontend(configuration.GetSection("Frontend"));
            var backend = BuildBackend(configuration.GetSection("Backend"));

            return new Proxy(name, frontend, backend, messageQueueLength);
        }

        private static IBackend BuildBackend(IConfigurationSection configuration)
        {
            var type = configuration.GetSection("Type").Value;
            var name = configuration.GetSection("Name").Value;
            switch (type.ToLower())
            {
                case "persistentsocketbackend":
                    var socketBackendConfiguration = configuration.Get<PersistentSocketBackendConfiguration>();
                    var socketBackend = new PersistentSocketBackend(socketBackendConfiguration);
                    return socketBackend;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown backen type: {type}");
            }
        }

        private static IFrontend BuildFrontend(IConfiguration configuration)
        {
            var type = configuration.GetSection("Type").Value;

            switch (type.ToLower())
            {
                case "socket":
                    var socketFrontendConfiguration = configuration.GetSection("Configuration").Get<SocketFrontendConfiguration>();
                    var socketFrontend = new SocketFrontend(socketFrontendConfiguration);
                    return socketFrontend;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown frontend type: {type}");
            }
        }
    }
}