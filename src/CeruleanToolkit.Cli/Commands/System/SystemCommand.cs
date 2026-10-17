using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.System;

/// <summary>
/// System 命令
/// </summary>
internal sealed class SystemCommand : Command
{
    public SystemCommand(IServiceProvider services) : base("system", "一些系统工具命令")
    {
        Aliases.Add("sys");
        Aliases.Add("os");

#if WINDOWS
        var fixCursorSchemeCommand = services.GetRequiredService<FixCursorThemeCommand>();
        Subcommands.Add(fixCursorSchemeCommand);
#endif
    }

}