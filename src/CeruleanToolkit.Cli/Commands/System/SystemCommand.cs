using CeruleanToolkit.Core.Attributes;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.System;

/// <summary>
/// System 命令
/// </summary>
[Command(Order = 2)]
internal sealed class SystemCommand : Command
{
    public SystemCommand()
        : base("system", "一些系统工具命令")
    {
        Aliases.Add("sys");
        Aliases.Add("os");
    }
}
