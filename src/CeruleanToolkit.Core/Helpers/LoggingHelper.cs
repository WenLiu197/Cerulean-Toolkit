using CeruleanToolkit.Core.Interfaces;
using CeruleanToolkit.Core.Services;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core.Helpers;

/// <summary>
/// 管理主机日志需要的方法都在这儿~~~
/// </summary>
/// <remarks>
/// <para>
/// 过滤策略：
/// </para>
/// <list type="bullet">
/// <item><description>Debug 构建固定允许 <see cref="LogLevel.Debug"/> 及以上级别的日志，便于调试。</description></item>
/// <item><description>Release 构建的最低级别由配置的 <see cref="Config.General.logLevel"/> 决定（默认 <c>warning</c>），用户可自行调整以排错。</description></item>
/// </list>
/// </remarks>
public static class LoggingHelper
{
    private static IConfigService _configService = new ConfigService();
    private static Config _config = _configService.GetConfig();

    /// <summary>
    /// 配置日志过滤
    /// </summary>
    /// <param name="logging">日志构建器实例</param>
    /// <param name="config">配置文件实例</param>
    public static void Configure(ILoggingBuilder logging, Config config)
    {
        ArgumentNullException.ThrowIfNull(logging);
        ArgumentNullException.ThrowIfNull(config);
#if DEBUG
        logging.SetMinimumLevel(LogLevel.Debug);
#else
        logging.SetMinimumLevel(ParseLogLevel(config.General.LogLevel));
#endif
    }

    /// <summary>
    /// 将配置中的日志等级字符串解析为 <see cref="LogLevel"/>。
    /// </summary>
    /// <param name="level">日志等级字符串，不区分大小写。</param>
    /// <returns>
    /// 解析后的日志级别；无法识别或为空时回退为 <see cref="LogLevel.Warning"/>。
    /// </returns>
    private static LogLevel ParseLogLevel(string? level)
    {
        return level?.Trim().ToLowerInvariant() switch
        {
            "trace" => LogLevel.Trace,
            "debug" or "dbg" => LogLevel.Debug,
            "information" or "info" => LogLevel.Information,
            "warning" or "warn" => LogLevel.Warning,
            "error" => LogLevel.Error,
            "critical" or "fatal" => LogLevel.Critical,
            "none" => LogLevel.None,
            _ => SetDefaultLevel(level)
        };
    }

    private static LogLevel SetDefaultLevel(string? level)
    {
        AnsiConsole.MarkupLine($"[yellow]未知的日志等级: {level}, 已设置为默认配置: Level.Warning[/]");
        AnsiConsole.MarkupLine($"[yellow]支持的配置值:[/]\n");
        AnsiConsole.MarkupLine($"[yellow]trance:            LogLevel.Trace[/]");
        AnsiConsole.MarkupLine($"[yellow]debug, dbg:        LogLevel.Debug[/]");
        AnsiConsole.MarkupLine($"[yellow]information, info: LogLevel.Information[/]");
        AnsiConsole.MarkupLine($"[yellow]warning, warn:     LogLevel.Warning[/]");
        AnsiConsole.MarkupLine($"[yellow]error:             LogLevel.Error[/]");
        AnsiConsole.MarkupLine($"[yellow]critical, fatal:   LogLevel.Critical[/]");
        AnsiConsole.MarkupLine($"[yellow]none:              LogLevel.None[/]");
        _config.General.LogLevel = "warning";
        _configService.Save();
        return LogLevel.Warning;
    }
}
