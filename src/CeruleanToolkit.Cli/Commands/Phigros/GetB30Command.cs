using CeruleanToolkit.Core.Attributes;
using CeruleanToolkit.Core.Interfaces;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

[SubCommand(typeof(PhigrosCommand), Order = 3)]
internal sealed class GetB30Command : Command
{
    public GetB30Command(IPhigrosService phigrosService)
        : base("get-b30", "获取 B30 成绩")
    {
        Aliases.Add("盖特比三零");

        SetAction(async (parseResult, cancellationToken) =>
        {
            return await phigrosService.GetB30Async();
        });
    }
}
