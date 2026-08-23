using CeruleanToolkit.Core.Helpers.Pringting;
using CeruleanToolkit.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

internal class GetUserInfoCommand
{
    public static Command Create(IServiceProvider services)
    {
        var configService = services.GetRequiredService<IConfigService>();
        var phigrosService = services.GetRequiredService<IPhigrosService>();

        var command = new Command("get-user-info", "获取用户基本信息");
        command.Aliases.Add("usr-info");

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            return PhigrosPrintingHelper.ShowUserInfo(await phigrosService.GetUserInfoAsync());
        });

        return command;
    }
}