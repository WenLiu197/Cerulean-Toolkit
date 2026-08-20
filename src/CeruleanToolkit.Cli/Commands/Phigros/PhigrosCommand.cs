using CeruleanToolkit.Cli.Commands.Config;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

/// <summary>
/// Phigros 命令
/// </summary>
internal class PhigrosCommand
{
    // TODO: 这里慢慢加，v0.1.0先写一部分
    /// <summary>
    /// 创建 <c>phigros</c> 命令实例, 并添加子命令
    /// </summary>
    /// <param name="serviceProvider">顾名思义</param>
    /// <returns>配置完成的命令实例</returns>
    public static Command Create(IServiceProvider serviceProvider)
    {

        var command = new Command("phigros", "Phigros 游戏相关命令");
        command.Aliases.Add("phi");
        command.Aliases.Add("pgr");

        return command;
    }
}
