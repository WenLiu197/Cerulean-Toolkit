using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.System;

/// <summary>
/// System 命令
/// </summary>
internal class SystemCommand
{
    /// <summary>
    /// 创建 <c>system</c> 命令实例, 并添加子命令
    /// </summary>
    /// <param name="services">顾名思义</param>
    /// <returns>配置完成的命令实例</returns>
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("system", "一些系统工具命令");
        command.Aliases.Add("sys");
        command.Aliases.Add("os");

#if WINDOWS
        command.Subcommands.Add(FixCursorSchemeCommand.Create(services));
#endif

        return command;
    }
}
