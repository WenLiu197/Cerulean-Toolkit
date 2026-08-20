using CeruleanToolkit.Core;
using CeruleanToolkit.Core.Interfaces;
using CeruleanToolkit.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace CeruleanToolkit.Cli.Commands.Config;

/// <summary>
/// 对应 <c><b>set-config</b></c>
/// </summary>
/// <remarks>
/// <para>
/// 别名: set
/// </para>
/// </remarks>
internal static class SetConfigCommand
{
    public static Command Create(IServiceProvider servicesProvider)
    {
        var configService = servicesProvider.GetRequiredService<IConfigService>();

        Argument<string> projectArgument = new("project")
        {
            Description = "配置项路径，如 phigros.isInternational"
        };

        Argument<string> valueArgument = new("value")
        {
            Description = "配置项的新值"
        };

        var command = new Command("set-config", "更改程序配置")
        {
            projectArgument,
            valueArgument
        };

        command.SetAction(parseResult =>
        {
            string project = parseResult.GetValue(projectArgument) ?? string.Empty;
            string value = parseResult.GetValue(valueArgument) ?? string.Empty;
            return SetValue(configService, project, value);
        });
        command.Aliases.Add("set");

        return command;
    }

    private static int SetValue(IConfigService configService, string project, string value)
    {
        AppConfig config = configService.GetConfig();

        switch (project.Trim().ToLowerInvariant())
        {
            case "general.loglevel":
                config.General.LogLevel = value;
                break;

            case "phigros.sessiontoken":
                config.Phigros.SessionToken = value;
                break;

            case "phigros.isinternational":
                if (!bool.TryParse(value, out bool isInternational))
                {
                    AnsiConsole.MarkupLine($"[red]'{Markup.Escape(value)}' 不是有效的布尔值（true/false）[/]");
                    return 1;
                }

                config.Phigros.IsInternational = isInternational;
                break;

            default:
                AnsiConsole.MarkupLine(
                    $"[red]未知的配置项：'{Markup.Escape(project)}'（可选：general.logLevel、phigros.sessionToken、phigros.isInternational）[/]");
                return 1;
        }

        configService.Save();
        AnsiConsole.MarkupLine($"[green]已更新 {Markup.Escape(project.Trim().ToLowerInvariant())}[/]");
        return 0;
    }
}
