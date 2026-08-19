using CeruleanToolkit.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var builder = Host.CreateApplicationBuilder();
// TODO: 后续配置 DI 与日志过滤
using IHost host = builder.Build();

try
{
    await host.StartAsync();

    var root = new RootCommand("Cerulean Toolkit —— 一个多功能工具包");
    return await root.Parse(args).InvokeAsync();
}
finally
{
    await host.StopAsync();
}