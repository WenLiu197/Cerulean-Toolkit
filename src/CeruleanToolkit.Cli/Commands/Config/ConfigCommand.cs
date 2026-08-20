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
/// 各类配置命令的父命令
/// </summary>
internal class ConfigCommand
{
    /// <summary>
    /// 创建 <c>config</c> 命令实例, 并添加子命令
    /// </summary>
    /// <param name="serviceProvider">顾名思义</param>
    /// <returns>配置完成的命令实例</returns>
    public static Command Create(IServiceProvider serviceProvider)
    {

        var command = new Command("config", "配置管理");
        command.Aliases.Add("cfg");
        command.Subcommands.Add(ListConfigCommand.Create(serviceProvider));

        return command;
    }
}