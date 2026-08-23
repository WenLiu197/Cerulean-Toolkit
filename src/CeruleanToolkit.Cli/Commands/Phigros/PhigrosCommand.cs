using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

/// <summary>
/// Phigros 命令
/// </summary>
internal class PhigrosCommand
{
    /// <summary>
    /// 创建 <c>phigros</c> 命令实例, 并添加子命令
    /// </summary>
    /// <param name="services">顾名思义</param>
    /// <returns>配置完成的命令实例</returns>
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("phigros", "Phigros 游戏相关命令");
        command.Aliases.Add("phi");
        command.Aliases.Add("pgr");

        command.Subcommands.Add(BindCommand.Create(services));
        command.Subcommands.Add(UnbindCommand.Create(services));
        command.Subcommands.Add(GetUserInfoCommand.Create(services));
        command.Subcommands.Add(GetB30Command.Create(services));

        return command;
    }
}
