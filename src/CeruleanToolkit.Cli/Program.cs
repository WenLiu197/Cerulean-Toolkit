using CeruleanToolkit.Cli.Commands.Config;
using CeruleanToolkit.Cli.Commands.Phigros;
using CeruleanToolkit.Cli.Commands.System;
using CeruleanToolkit.Core;
using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Interfaces;
using CeruleanToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Text;

// 服务注册
var configService = new ConfigService();
configService.Init();

var builder = Host.CreateApplicationBuilder();
LoggingHelper.Configure(builder.Logging, configService);
builder.Services.AddSingleton<IConfigService>(configService);
builder.Services.AddAllServices();

// 命令注册
builder.Services.AddTransient<SystemCommand>();
#if WINDOWS
builder.Services.AddTransient<FixCursorThemeCommand>();
#endif

using IHost host = builder.Build();

try
{
    Console.OutputEncoding = Encoding.UTF8;
    await host.StartAsync();

    var root = new RootCommand("Cerulean Toolkit —— 一个多功能工具包");

    var systemCommand = host.Services.GetRequiredService<SystemCommand>();

    root.Subcommands.Add(ConfigCommand.Create(host.Services));
    root.Subcommands.Add(PhigrosCommand.Create(host.Services));
    root.Subcommands.Add(systemCommand);

    return await root.Parse(args).InvokeAsync();
}
finally
{
    await host.StopAsync();
}