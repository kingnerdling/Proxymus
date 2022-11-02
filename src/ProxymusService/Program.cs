using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using ProxymusCore.Metrics;
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
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();

app.MapGet("/metrics", () => Results.Ok(Metrics.Create(proxy)));
host.RunAsync();
app.Run();