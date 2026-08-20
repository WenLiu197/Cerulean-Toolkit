using CeruleanToolkit.Cli.Commands.Config;
using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Interfaces;
using CeruleanToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text;

var configService = new ConfigService();
configService.Init();

var builder = Host.CreateApplicationBuilder();
LoggingHelper.Configure(builder.Logging, configService);
builder.Services.AddSingleton<IConfigService>(configService);
using IHost host = builder.Build();

try
{
    Console.OutputEncoding = Encoding.UTF8;
    await host.StartAsync();

    var root = new RootCommand("Cerulean Toolkit —— 一个多功能工具包");
    root.Subcommands.Add(ConfigCommand.Create(host.Services));

    return await root.Parse(args).InvokeAsync();
}
finally
{
    await host.StopAsync();
}