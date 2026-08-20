using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace CeruleanToolkit.Cli.Commands.Config;

internal class ListConfigCommand
{
    private Table _table = new Table();
    public static Command Create(IServiceProvider serviceProvider)
    {
        var configService = serviceProvider.GetRequiredService<IConfigService>();

        Option<bool> fullOption = new("--all", ["-a"])
        {
            Description = "输出完整配置（默认，可不指定）"
        };

        Option<string> groupOption = new("--group", ["-g"])
        {
            Description = "按配置组输出，可选值：general、phigros"
        };

        var command = new Command("list-config", "列出当前配置")
    {
        fullOption,
        groupOption
    };
        command.Aliases.Add("list");
        command.Aliases.Add("ls");

        command.SetAction((parseResult) =>
        {
            string? group = parseResult.GetValue(groupOption);

            if (string.IsNullOrEmpty(group))
            {
                PrintingHelper.PrintAllConfig(configService.GetConfig());
            }
            else
            {
                PrintingHelper.PrintConfigWithGroup(configService.GetConfig(), group);
            }
        });

        return command;
    }
}
