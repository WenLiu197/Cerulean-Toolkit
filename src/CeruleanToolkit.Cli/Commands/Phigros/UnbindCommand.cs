using CeruleanToolkit.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace CeruleanToolkit.Cli.Commands.Phigros;

internal class UnbindCommand
{
    /// <summary>
    /// 解除绑定账号命令
    /// </summary>
    /// <param name="services">顾名思义</param>
    /// <returns>配置完成的命令实例</returns>
    public static Command Create(IServiceProvider services)
    {
        var phigrosService = services.GetRequiredService<IPhigrosService>();
        var configService = services.GetRequiredService<IConfigService>();

        var command = new Command("unbind", "解绑账号命令");

        command.SetAction(parseResult =>
        {
            return phigrosService.Unbind(configService.GetConfig());
        });

        return command;
    }
}