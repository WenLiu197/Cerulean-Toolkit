using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core.Helpers;

//TODO: 后续如果越来越多记得按类别拆分
/// <summary>
/// 格式化输出辅助类，集中管理各种表格、面板等输出逻辑。
/// </summary>
public static class PrintingHelper
{
    /// <summary>
    /// 输出所有配置项的表格
    /// </summary>
    /// <param name="config">配置对象</param>
    public static void PrintAllConfig(Config config)
    {
        var table = BuildTable();
        foreach (var (group, key, value) in EnumerateEntries(config))
        {
            table.AddRow(group, key, value);
        }
        AnsiConsole.Write(table);
    }

    /// <summary>
    /// 输出指定组别的配置项表格
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <param name="group">组名，不区分大小写</param>
    public static void PrintConfigWithGroup(Config config, string group)
    {
        var table = BuildTable();

        foreach (var (groupName, key, value) in EnumerateEntries(config))
        {
            if (string.Equals(group.Trim().ToLower(), groupName, StringComparison.OrdinalIgnoreCase))
            {
                table.AddRow(group, key, value);
            }
        }
        if (table.Rows.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]未找到组 '{group}', 可输入 'ctk cfg ls -a' 查看目前支持的配置[/]");
            // 本来想列个表，但一想，算了，反正有全列的命令
            return;
        }
        else
        {
            AnsiConsole.Write(table);
        }
    }

    private static Table BuildTable()
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.Title("[bold][yellow]程序配置[/][/]");
        table.AddColumn("[bold][yellow]组别[/][/]");
        table.AddColumn("[bold][yellow]配置项[/][/]");
        table.AddColumn("[bold][yellow]值[/][/]");
        return table;
    }

    /// <summary>
    /// 枚举所有配置组及其属性
    /// </summary>
    /// <param name="config">配置对象</param>
    /// <returns>三元组：(组名, 配置项名, 值的字符串表示)</returns>
    private static IEnumerable<(string Group, string Key, string Value)> EnumerateEntries(Config config)
    {
        // General 组
        yield return ("General", nameof(config.General.LogLevel), config.General.LogLevel);

        // Phigros 组
        yield return ("Phigros", nameof(config.Phigros.SessionToken), config.Phigros.SessionToken);
        yield return ("Phigros", nameof(config.Phigros.IsInternational), config.Phigros.IsInternational.ToString());

        // 未来新增组时，在这里追加 yield return 即可
    }
}