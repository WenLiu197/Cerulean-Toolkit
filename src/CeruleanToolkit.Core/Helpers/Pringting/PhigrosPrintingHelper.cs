using CeruleanToolkit.Core.Services;
using PhiLibrary197.CloudSave;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core.Helpers.Pringting;

public class PhigrosPrintingHelper
{
    public static int ShowUserInfo((string NickName, string Rks, string Intro, string Data, string Challenge, bool ShowIntro) info)
    {
        if (info.ShowIntro)
        {
            AnsiConsole.MarkupLine($"[yellow]昵称: [/]{info.NickName ?? "(guest)"}");
            AnsiConsole.MarkupLine($"[yellow]简介: [/]{info.Intro}");
            AnsiConsole.MarkupLine($"[yellow]Rks: [/]{info.Rks}");
            AnsiConsole.MarkupLine($"[yellow]课题模式: [/]{info.Challenge}");
            AnsiConsole.MarkupLine($"[yellow]Data: [/]{info.Data}");
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine($"[yellow]昵称: [/]{info.NickName ?? "(guest)"}");
            AnsiConsole.MarkupLine($"[yellow]Rks: [/]{info.Rks}");
            AnsiConsole.MarkupLine($"[yellow]课题模式: [/]{info.Challenge}");
            AnsiConsole.MarkupLine($"[yellow]Data: [/]{info.Data}");
            return 0;
        }
    }

    public static int ShowB30((List<CompleteScore> Phis, List<CompleteScore> Others) score, PhigrosService service)
    {
        // 输出 B30 中的 P 位
        AnsiConsole.Write(new Rule("[bold yellow]Phi 3[/]"));
        var p3Table = BuildScoreTable(TableType.Phi);
        byte i = 0;
        foreach (var p in score.Phis.Take(3))
        {
            i++;
            var name = p.NameOrDefault;
            var difficulty = service.ParseDifficulty(p.Score.Difficulty);
            var chatConstant = p.ChartConstant.ToString();
            var rks = Math.Round(p.Rks, 1).ToString();
            var status = service.ParseStatus(p.Score.Status);
            var chartScore = p.Score.Score.ToString();
            var acc = p.Score.Accuracy;

            p3Table.AddRow($"P{i}", name, difficulty, chatConstant, rks, status, chartScore, $"{acc}%");
        }
        AnsiConsole.Write(p3Table);

        // 输出 B30 中的 B27 和 Over Flow
        AnsiConsole.Write(new Rule("[bold DeepSkyBlue1]Best 27[/]"));
        var b27Table = BuildScoreTable(TableType.B27);
        var overFlowTable = BuildScoreTable(TableType.Others);
        byte j = 0;
        foreach (var s in score.Others.Take(33))
        {
            j++;
            var name = s.NameOrDefault;
            var difficulty = service.ParseDifficulty(s.Score.Difficulty);
            var chatConstant = s.ChartConstant.ToString();
            var rks = Math.Round(s.Rks, 1).ToString();
            var status = service.ParseStatus(s.Score.Status);
            var chartScore = s.Score.Score.ToString();
            var acc = Math.Round(s.Score.Accuracy, 2);

            if (j <= 27)
            {
                // B30
                b27Table.AddRow($"#{j}", name, difficulty, chatConstant, rks, status, chartScore, $"{acc}%");
            }
            else
            {
                // Over Flow
                overFlowTable.AddRow($"#{j}", name, difficulty, chatConstant, rks, status, chartScore, $"{acc}%");
            }
        }
        AnsiConsole.Write(b27Table);
        AnsiConsole.Write(new Rule("[bold]Over Flow[/]"));
        AnsiConsole.Write(overFlowTable);

        return 0;
    }

    /// <summary>
    /// 构建成绩相关的表格
    /// </summary>
    /// <param name="type">表格类型</param>
    private static Table BuildScoreTable(TableType type)
    {
        var table = new Table().RoundedBorder();

        switch (type)
        {
            case TableType.Phi:
                table.BorderColor(Color.Yellow);
                break;
            case TableType.B27:
                table.BorderColor(Color.DeepSkyBlue1);
                break;
            case TableType.Others:
                table.BorderColor(Color.White);
                break;
            default:
                table.BorderColor(Color.White);
                break;
        }

        table.AddColumn("序号", col => col.Centered());
        table.AddColumn("曲名");
        table.AddColumn("难度", col => col.Centered());
        table.AddColumn("定数", col => col.Centered());
        table.AddColumn("单曲 Rks", col => col.Centered());
        table.AddColumn("评级", col => col.Centered());
        table.AddColumn("分数", col => col.Centered());
        table.AddColumn("Acc", col => col.Centered());

        return table;
    }

    enum TableType
    {
        Phi = 0,
        B27 = 1,
        Others = 2
    }
}
