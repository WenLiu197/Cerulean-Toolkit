using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Interfaces;
using CeruleanToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var builder = Host.CreateApplicationBuilder();
var configService = new ConfigService();
LoggingHelper.Configure(builder.Logging, configService.GetConfig());
builder.Services.AddSingleton<IConfigService, ConfigService>();
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