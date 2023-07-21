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
        _proxy.Start();
    }
}
