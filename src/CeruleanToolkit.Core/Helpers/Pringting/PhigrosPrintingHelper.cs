using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core.Helpers.Pringting;

public class PhigrosPrintingHelper
{
    public static int ShowUserInfo((string NickName, string Rks, string Intro, string Data, string Challenge) info)
    {
        // 获取云存档实例
        AnsiConsole.MarkupLine($"[yellow]昵称: [/]{info.NickName ?? "(guest)"}");
        AnsiConsole.MarkupLine($"[yellow]简介: [/]{info.Intro}");
        AnsiConsole.MarkupLine($"[yellow]Rks: [/]{info.Rks}");
        AnsiConsole.MarkupLine($"[yellow]课题模式: [/]{info.Challenge}");
        AnsiConsole.MarkupLine($"[yellow]Data: [/]{info.Data}");
        return 0;
    }
}
