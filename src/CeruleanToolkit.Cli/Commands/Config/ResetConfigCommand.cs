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
// 三个选项大抵是够了
/// <summary>
/// 表示将配置恢复为默认值的命令, 重置配置命令
/// </summary>
/// <remarks>
/// <para>
/// 重置范围由<b>选项</b>决定，三个选项互斥：
/// </para>
/// <list type="bullet">
/// <item><description><c>--all</c>（<c>-a</c>）：重置全部配置，为默认行为，可不指定。</description></item>
/// <item><description><c>--group</c>（<c>-g</c>）：重置指定配置组（<c>general</c>、<c>phigros</c>）。</description></item>
/// <item><description><c>--item</c>（<c>-i</c>）：重置指定配置项（如 <c>phigros.sessionToken</c>）。</description></item>
/// </list>
/// </remarks>
internal static class ResetConfigCommand
{
    /// <summary>
    /// 创建 <c>reset-config</c> 命令实例。
    /// </summary>
    /// <param name="services">用于解析命令依赖的服务提供程序。</param>
    /// <returns>配置完成的命令实例。</returns>
    public static Command Create(IServiceProvider services)
    {
        var configService = services.GetRequiredService<IConfigService>();

        Option<bool> allOption = new("--all", ["-a"])
        {
            Description = "重置全部配置（默认行为，可不指定）",
        };

        Option<string> groupOption = new("--group", ["-g"])
        {
            Description = "重置指定配置组，每次只能跟一个，可选值：general、phigros",
        };

        Option<string> itemOption = new("--item", ["-i"])
        {
            Description = "重置指定配置项，如 phigros.sessionToken"
        };

        var command = new Command("reset-config", "恢复默认配置")
        {
            allOption,
            groupOption,
            itemOption,
        };
        //command.Options.Add(allOption);
        //command.Options.Add(groupOption);
        //command.Options.Add(groupOption);
        command.Aliases.Add("reset");
        command.SetAction(parseResult =>
        {
            // 统计用户使用了几个选项
            int specifiedCount = 0;

            if (parseResult.GetValue(allOption))
                specifiedCount++;

            string? group = parseResult.GetValue(groupOption);
            if (!string.IsNullOrEmpty(group))
                specifiedCount++;

            string? item = parseResult.GetValue(itemOption);
            if (!string.IsNullOrEmpty(item))
                specifiedCount++;

            if (specifiedCount > 1)
            {
                AnsiConsole.MarkupLine("[red]--all、--group、--item 只能指定其一哦~~~[/]");
                return 1;
            }

            // 根据指定的选项执行对应逻辑
            if (!string.IsNullOrEmpty(group))
                return ResetGroup(configService, group!);

            if (!string.IsNullOrEmpty(item))
                return ResetItem(configService, item!);

            // 默认是全部输出
            return ResetAll(configService);

            // 重构前的代码，留着，做第可读性、难维护的反面教材
            //bool allSpecified = parseResult.GetValue(allOption);
            //bool groupSpecified = !string.IsNullOrEmpty(group);
            //bool itemSpecified = !string.IsNullOrEmpty(item);

            //if ((allSpecified && (groupSpecified || itemSpecified))
            //    || (groupSpecified && itemSpecified))
            //{
            //    AnsiConsole.MarkupLine("[red]--all、--group、--item 只能指定其一[/]");
            //    return 1;
            //}

            //if (groupSpecified)
            //{
            //    return ResetGroup(configService, group!);
            //}

            //if (itemSpecified)
            //{
            //    return ResetItem(configService, item!);
            //}

            //ResetAll(configService);
            //return 0;
        });

        return command;
    }

    /// <summary>
    /// 将全部配置恢复为默认值并保存。
    /// </summary>
    /// <param name="configService">配置服务。</param>
    /// <returns>恒为 <c>0</c>。</returns>
    private static int ResetAll(IConfigService configService)
    {
        AppConfig config = configService.GetConfig();
        config.General = new GeneralConfig();
        config.Phigros = new PhigrosConfig();

        configService.Save();
        AnsiConsole.MarkupLine("[green]已重置全部配置[/]");
        return 0;
    }

    /// <summary>
    /// 将指定配置组恢复为默认值并保存。
    /// </summary>
    /// <param name="configService">配置服务。</param>
    /// <param name="group">配置组名称，不区分大小写。</param>
    /// <returns>重置成功返回 <c>0</c>；配置组不存在返回 <c>1</c>。</returns>
    private static int ResetGroup(IConfigService configService, string group)
    {
        AppConfig config = configService.GetConfig();

        switch (group.Trim().ToLowerInvariant())
        {
            case "general":
                config.General = new GeneralConfig();
                break;

            case "phigros":
                config.Phigros = new PhigrosConfig();
                break;

            default:
                AnsiConsole.MarkupLine(
                    $"[red]未知的配置组：'{Markup.Escape(group)}'（可选：general、phigros）[/]");
                return 1;
        }

        configService.Save();
        AnsiConsole.MarkupLine($"[green]已重置配置组 {Markup.Escape(group.Trim().ToLowerInvariant())}[/]");
        return 0;
    }

    // TODO: 同样的，需要改，但以目前的能力够呛

    /// <summary>
    /// 将指定配置项恢复为默认值并保存。
    /// </summary>
    /// <param name="configService">配置服务。</param>
    /// <param name="item">点分路径形式的配置项名称。</param>
    /// <returns>重置成功返回 <c>0</c>；配置项不存在返回 <c>1</c>。</returns>
    private static int ResetItem(IConfigService configService, string item)
    {
        AppConfig config = configService.GetConfig();

        switch (item.Trim().ToLowerInvariant())
        {
            case "general.loglevel":
                config.General.LogLevel = "warning";
                break;

            case "phigros.sessiontoken":
                config.Phigros.SessionToken = string.Empty;
                break;

            case "phigros.isinternational":
                config.Phigros.IsInternational = false;
                break;

            default:
                AnsiConsole.MarkupLine(
                    $"[red]未知的配置项：'{Markup.Escape(item)}'（可选：general.logLevel、phigros.sessionToken、phigros.isInternational）[/]");
                return 1;
        }

        configService.Save();
        AnsiConsole.MarkupLine($"[green]已重置配置项 {Markup.Escape(item.Trim().ToLowerInvariant())}[/]");
        return 0;
    }
}