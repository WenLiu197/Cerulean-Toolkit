using CeruleanToolkit.Core.Attributes;
using CeruleanToolkit.Core.Interfaces;
using Spectre.Console;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.Phigros;

[SubCommand(typeof(PhigrosCommand), Order = 0)]
internal sealed class BindCommand : Command
{
    public BindCommand(IPhigrosService phigrosService)
        : base("bind", "绑定账号命令，获取 sessionToken 并保存到程序配置中")
    {
        Option<bool> sessionTokenOption = new("--session-token", ["-s"])
        {
            Description = "直接输入 sessionToken 绑定（默认行为，可不指定）",
            Arity = ArgumentArity.Zero
        };

        Option<bool> taptapOption = new("--taptap", ["-t"])
        {
            Description = "通过 TapTap 绑定",
            Arity = ArgumentArity.Zero
        };

        Add(sessionTokenOption);
        Add(taptapOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            int specifiedCount = 0;

            if (parseResult.GetValue(sessionTokenOption))
                specifiedCount++;

            if (parseResult.GetValue(taptapOption))
                specifiedCount++;

            if (specifiedCount > 1)
            {
                AnsiConsole.MarkupLine("[red]--session-token、--taptap 只能指定其一哦~~~[/]");
                return 1;
            }

            // 根据指定的选项执行对应逻辑
            if (parseResult.GetValue(sessionTokenOption))
                return await phigrosService.BindWithSessionTokenAsync();

            if (parseResult.GetValue(taptapOption))
                return await phigrosService.BindWithTaptapAsync();

            return await phigrosService.BindWithSessionTokenAsync();
        });
    }
}
