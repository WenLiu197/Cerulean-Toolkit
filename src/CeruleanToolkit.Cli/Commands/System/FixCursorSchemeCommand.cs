#if WINDOWS
using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Interfaces.System;
using Spectre.Console;
using System.CommandLine;

namespace CeruleanToolkit.Cli.Commands.System;

/// <summary>
/// 修复 Windows 11 的光标方案 Bug，给巨硬擦屁股用的
/// </summary>
internal class FixCursorSchemeCommand : Command
{
    public FixCursorSchemeCommand(ICursorFixService cursorFixService)
        : base("fix-cursor-theme", "修复 Windows 11 的光标方案 Bug，如果你的光标变成了超大 Windows (默认)，不妨试试这个命令")
    {
        this.Aliases.Add("fct");

        this.SetAction(_ =>
        {
            try
            {
                cursorFixService.ApplyCurrentCursorTheme();
                AnsiConsole.MarkupLine($"[green]光标方案已应用。[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]应用失败：{ex.Message}[/]");
            }
        });
    }
}
#endif