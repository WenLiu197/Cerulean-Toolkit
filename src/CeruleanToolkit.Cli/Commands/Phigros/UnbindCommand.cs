using CeruleanToolkit.Core.Attributes;
using CeruleanToolkit.Core.Interfaces;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

[SubCommand(typeof(PhigrosCommand), Order = 1)]
internal sealed class UnbindCommand : Command
{
    public UnbindCommand(IPhigrosService phigrosService, IConfigService configService)
        : base("unbind", "解绑账号命令")
    {
        SetAction(parseResult =>
        {
            return phigrosService.Unbind(configService.GetConfig());
        });
    }
}
