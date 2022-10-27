using Microsoft.AspNetCore.Builder;
using ProxymusCore.Proxy;
using ProxymusService;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var proxy = ProxyFactory.Build(hostContext.Configuration.GetSection("Proxy"));
        services.AddSingleton<IProxy>(proxy);
        services.AddHostedService<Worker>();

    })
    .Build();

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/metrics", () => "Hello World!");
host.RunAsync();
app.Run();



