using CeruleanToolkit.Core.Attributes;
using CeruleanToolkit.Core.Helpers.Pringting;
using CeruleanToolkit.Core.Interfaces;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

[SubCommand(typeof(PhigrosCommand), Order = 2)]
internal sealed class GetUserInfoCommand : Command
{
    public GetUserInfoCommand(IPhigrosService phigrosService)
        : base("get-user-info", "获取用户基本信息")
    {
        Aliases.Add("usr-info");

        SetAction(async (parseResult, cancellationToken) =>
        {
            return PhigrosPrintingHelper.ShowUserInfo(await phigrosService.GetUserInfoAsync());
        });
    }
}
