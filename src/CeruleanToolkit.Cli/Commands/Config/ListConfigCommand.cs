using CeruleanToolkit.Core.Attributes;
using CeruleanToolkit.Core.Helpers.Pringting;
using CeruleanToolkit.Core.Interfaces;
using Spectre.Console;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Config;

/// <summary>
/// 对应 <c><b>list-config</b></c>, 列出配置命令
/// </summary>
/// <remarks>
/// <para>
/// 别名:
/// <para>
/// <list type="bullet">list</list>
/// <list type="bullet">ls</list>
/// </para>
/// </para>
/// </remarks>
[SubCommand(typeof(ConfigCommand), Order = 0)]
internal sealed class ListConfigCommand : Command
{
    public ListConfigCommand(IConfigService configService)
        : base("list-config", "列出当前配置")
    {
        Option<bool> allOption = new("--all", ["-a"])
        {
            Description = "输出完整配置（默认，可不指定）"
        };

        Option<string> groupOption = new("--group", ["-g"])
        {
            Description = "按配置组输出，可选值：general、phigros"
        };

        Add(allOption);
        Add(groupOption);

        Aliases.Add("list");
        Aliases.Add("ls");

        SetAction(parseResult =>
        {
            // 统计用户使用了几个选项
            int specifiedCount = 0;

            if (parseResult.GetValue(allOption))
                specifiedCount++;

            string? group = parseResult.GetValue(groupOption);
            if (!string.IsNullOrEmpty(group))
                specifiedCount++;

            // 互斥校验：最多只能指定一个
            if (specifiedCount > 1)
            {
                AnsiConsole.MarkupLine("[red]--all、--group 只能指定其一哦~~~[/]");
                return 1;
            }

            if (string.IsNullOrEmpty(group))
            {
                return ConfigPrintingHelper.PrintAllConfig(configService.GetConfig());
            }
            else
            {
                return ConfigPrintingHelper.PrintConfigWithGroup(configService.GetConfig(), group);
            }
        });
    }
}
