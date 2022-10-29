using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProxymusCore.Backend;
using ProxymusCore.Backend.PersistentSocket;
using ProxymusCore.Frontend;
using ProxymusCore.Frontend.SocketFrontend;
using ProxymusCore.Proxy;
using ProxymusCore.Router;

namespace ProxymusService
{
    public static class ProxyFactory
    {
        public static IProxy Build(IConfiguration configuration)
        {
            var name = configuration.GetRequiredSection("Name").Value;
            var messageQueueLength = int.Parse(configuration.GetRequiredSection("MessageQueueLength").Value);
            var frontend = BuildFrontend(configuration.GetRequiredSection("Frontend"));
            var backend = BuildBackend(configuration.GetRequiredSection("Backend"));

            return new Proxy(name, frontend, backend, messageQueueLength);
        }

        private static IBackend BuildBackend(IConfigurationSection configuration)
        {
            var type = configuration.GetRequiredSection("Type").Value;
            var name = configuration.GetRequiredSection("Name").Value;
            switch (type.ToLower())
            {
                case "proxymuscore.messageprocessor.persistentsocketbackend":
                    var socketBackendConfiguration = configuration.Get<PersistentSocketBackendConfiguration>();
                    var router = BuildRouter(configuration.GetRequiredSection("Router"));
                    var socketBackend = new PersistentSocketBackend(socketBackendConfiguration, router);
                    return socketBackend;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown backend type: {type}");
            }
        }

        private static IRouter BuildRouter(IConfiguration configuration)
        {
            var type = configuration.GetRequiredSection("Type").Value;
            switch (type.ToLower())
            {
                case "proxymuscore.router.roundrobinrouter":
                    return new RoundRobinRouter();
                default:
                    throw new ArgumentOutOfRangeException($"Unknown router : {type}");
            }
        }

        private static IFrontend BuildFrontend(IConfiguration configuration)
        {
            var type = configuration.GetRequiredSection("Type").Value;

            switch (type.ToLower())
            {
                case "proxymuscore.frontend.socketfrontend":
                    var socketFrontendConfiguration = configuration.GetRequiredSection("Configuration").Get<SocketFrontendConfiguration>();
                    var socketFrontend = new SocketFrontend(socketFrontendConfiguration);
                    return socketFrontend;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown frontend type: {type}");
            }
        }
    }
}