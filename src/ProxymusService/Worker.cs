using ProxymusCore.Backend.PersistentSocket;
using ProxymusCore.Frontend.SocketFrontend;
using ProxymusCore.MessageProcessor;
using ProxymusCore.Proxy;

namespace ProxymusService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IProxy _proxy;

    public Worker(ILogger<Worker> logger, IProxy proxy)
    {
        _logger = logger;
        _proxy = proxy;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // var socketFrontendConfiguration = new SocketFrontendConfiguration("Frontend", "*", 5555, 100, 60000, 1000, 4096);
        // var persistentSocketBackendConfiguration = new PersistentSocketBackendConfiguration("Backend1",
        //  new List<PersistentSocketBackendHostConfiguration>{
        //     new PersistentSocketBackendHostConfiguration("host1", "127.0.0.1",5001,8,10,6000,4096)
        //  });

        // var proxy = new Proxy("test",
        //     new SocketFrontend(socketFrontendConfiguration, typeof(HeaderLengthMessageProcessor)),
        //     new PersistentSocketBackend(persistentSocketBackendConfiguration, typeof(HeaderLengthMessageProcessor)), 100);

        _proxy.Start();
    }
}
