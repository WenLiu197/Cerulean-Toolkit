using CeruleanToolkit.Core.Interfaces;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core.Services;

public sealed class PhigrosService : IPhigrosService
{
    private readonly IConfigService _configService;

    /// <summary>
    /// 通过构造函数注入 IConfigService
    /// </summary>
    /// <param name="configService">配置服务实例</param>
    public PhigrosService(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <summary>
    /// 直接使用 sessionToken 进行绑定
    /// </summary>
    /// <returns></returns>
    public int BindWithSessionToken()
    {
        // 获取 sessionToken 并校验
        string sessionToken = AnsiConsole.Prompt(new TextPrompt<string>("请输入 25 字符的 [yellow]sessionToken[/]: ")
            .Secret()
            .Validate(text =>
            {
                if (text.Trim().Length != 25)
                    return ValidationResult.Error("[red]sessionToken 长度错误，请检查后重新输入[/]");

                return ValidationResult.Success();
            }));

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            AnsiConsole.MarkupLine("[red]输入无效，操作已取消[/]");
            return 1;
        }

        _configService.GetConfig().Phigros.SessionToken = sessionToken;
        _configService.Save();
        AnsiConsole.MarkupLine("[green]sessionToken绑定成功~~~[/]");
        return 0;
    }
}
