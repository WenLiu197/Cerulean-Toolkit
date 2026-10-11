using CeruleanToolkit.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace CeruleanToolkit.Cli.Commands.System;

internal class FixCursorSchemeCommand
{
    /// <summary>
    /// 修复 Windows 11 的光标方案 Bug，给巨硬擦屁股用的
    /// </summary>
    /// <param name="services">顾名思义</param>
    /// <returns>配置完成的命令实例</returns>
    public static Command Create(IServiceProvider services)
    {

        var command = new Command("fix-cursor-scheme", "修复 Windows 11 的光标方案 Bug，如果你的光标变成了超大 Windows (默认)，不妨试试这个命令");

        command.SetAction(async (parseResult, cancellationToken) =>
        {
           

            //return await phigrosService.BindWithSessionTokenAsync();
        });

        return command;
    }
}
