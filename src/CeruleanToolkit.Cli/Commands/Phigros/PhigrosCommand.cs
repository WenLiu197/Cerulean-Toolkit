using CeruleanToolkit.Core.Attributes;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

/// <summary>
/// Phigros 命令
/// </summary>
[Command(Order = 1)]
internal sealed class PhigrosCommand : Command
{
    public PhigrosCommand()
        : base("phigros", "Phigros 游戏相关命令")
    {
        Aliases.Add("phi");
        Aliases.Add("pgr");
    }
}
