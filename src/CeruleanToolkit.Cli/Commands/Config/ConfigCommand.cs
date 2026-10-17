using CeruleanToolkit.Core.Attributes;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Config;

/// <summary>
/// 各类配置命令的父命令
/// </summary>
[Command(Order = 0)]
internal sealed class ConfigCommand : Command
{
    public ConfigCommand()
        : base("config", "配置管理")
    {
        Aliases.Add("cfg");
    }
}
