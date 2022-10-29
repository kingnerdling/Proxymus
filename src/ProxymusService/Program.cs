using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ProxymusCore.Proxy;
using ProxymusService;

IProxy proxy = null; ;
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        proxy = ProxyFactory.Build(hostContext.Configuration.GetSection("Proxy"));
        services.AddSingleton<IProxy>(proxy);
        services.AddHostedService<Worker>();

    })
    .Build();

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/metrics", () => Results.Ok(proxy.Metrics));
host.RunAsync();
app.Run();



