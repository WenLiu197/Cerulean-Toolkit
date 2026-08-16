using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var builder = Host.CreateApplicationBuilder();
// TODO: 后续配置 DI 与日志过滤
using IHost host = builder.Build();

try
{
    await host.StartAsync();


}
finally
{
    await host.StopAsync();
}