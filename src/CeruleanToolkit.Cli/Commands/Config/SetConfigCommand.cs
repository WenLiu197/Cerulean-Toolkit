using CeruleanToolkit.Core;
using CeruleanToolkit.Core.Attributes;
using CeruleanToolkit.Core.Interfaces;
using Spectre.Console;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Config;

/// <summary>
/// 对应 <c><b>set-config</b></c>, 更改配置命令
/// </summary>
/// <remarks>
/// <para>
/// 别名: set
/// </para>
/// </remarks>
[SubCommand(typeof(ConfigCommand), Order = 1)]
internal sealed class SetConfigCommand : Command
{
    public SetConfigCommand(IConfigService configService)
        : base("set-config", "更改程序配置")
    {
        Argument<string> projectArgument = new("key")
        {
            Description = "配置项路径，如 phigros.isInternational"
        };

        Argument<string> valueArgument = new("value")
        {
            Description = "配置项的新值"
        };

        Add(projectArgument);
        Add(valueArgument);

        SetAction(parseResult =>
        {
            string project = parseResult.GetValue(projectArgument) ?? string.Empty;
            string value = parseResult.GetValue(valueArgument) ?? string.Empty;
            return SetValue(configService, project, value);
        });

        Aliases.Add("set");
    }

    private static int SetValue(IConfigService configService, string project, string value)
    {
        AppConfig config = configService.GetConfig();

        switch (project.Trim().ToLowerInvariant())
        {
            case "general.loglevel":
                config.General.LogLevel = value;
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
                    $"[red]未知的配置项：'{Markup.Escape(project)}'（可选：general.logLevel、phigros.isInternational）[/]");
                return 1;
        }

        configService.Save();
        AnsiConsole.MarkupLine($"[green]已更新 {Markup.Escape(project.Trim().ToLowerInvariant())}[/]");
        return 0;
    }
}
