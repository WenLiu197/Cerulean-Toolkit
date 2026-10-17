using CeruleanToolkit.Cli.Commands;
using CeruleanToolkit.Core;
using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Interfaces;
using CeruleanToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

// 服务注册
var configService = new ConfigService();
configService.Init();

var builder = Host.CreateApplicationBuilder();
LoggingHelper.Configure(builder.Logging, configService);
builder.Services.AddSingleton<IConfigService>(configService);
builder.Services.AddAllServices();
builder.Services.AddAllCommands();

using IHost host = builder.Build();

try
{
    Console.OutputEncoding = Encoding.UTF8;
    await host.StartAsync();

    var root = host.Services.BuildRootCommand("Cerulean Toolkit —— 一个多功能工具包");
    return await root.Parse(args).InvokeAsync();
}
finally
{
    await host.StopAsync();
}
