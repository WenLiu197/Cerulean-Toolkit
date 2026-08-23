using CeruleanToolkit.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

internal class GetB30Command
{
    public static Command Create(IServiceProvider services)
    {
        var phigrosService = services.GetRequiredService<IPhigrosService>();

        var command = new Command("get-b30", "获取 B30 成绩");
        command.Aliases.Add("盖特比三零");

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            return await phigrosService.GetB30Async();
        });

        return command;
    }
}