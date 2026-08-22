using CeruleanToolkit.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace CeruleanToolkit.Cli.Commands.Phigros;

internal class BindCommand
{
    /// <summary>
    /// 绑定账号命令，获取 sessionToken 并保存到程序配置中
    /// </summary>
    /// <param name="services">顾名思义</param>
    /// <returns>配置完成的命令实例</returns>
    public static Command Create(IServiceProvider services)
    {
        var phigrosService = services.GetRequiredService<IPhigrosService>();

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

        var command = new Command("bind", "绑定账号命令，获取 sessionToken 并保存到程序配置中")
        {
            sessionTokenOption,
            taptapOption
        };

        command.SetAction(async (parseResult, cancellationToken) =>
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

        return command;
    }
}
