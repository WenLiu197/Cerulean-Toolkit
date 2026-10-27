using CeruleanToolkit.Core.Helpers.Pringting;
using PhiLibrary197.Authentication;
using PhiLibrary197.CloudSave;
using Spectre.Console;

namespace CeruleanToolkit.Core.Services;

public sealed partial class PhigrosService
{
    /// <summary>
    /// 直接使用 sessionToken 进行绑定，包括对 sessionToken 有效性的判断
    /// </summary>
    public async Task<int> BindWithSessionTokenAsync()
    {
        AppConfig config = _configService.GetConfig();

        if (string.IsNullOrEmpty(config.Phigros.SessionToken))
        {
            // 获取 sessionToken 并校验字符串长度
            string sessionToken = AnsiConsole.Prompt(new TextPrompt<string>("请输入 25 字符的 [yellow]sessionToken[/]: ")
                .Secret()
                .Validate(text =>
                {
                    if (text.Trim().Length != 25)
                        return ValidationResult.Error("[red]sessionToken 长度错误，请检查后重新输入[/]");

                    return ValidationResult.Success();
                }));

            AnsiConsole.MarkupLine("[blue]正在验证 sessionToken 有效性...[/]");

            try
            {
                using Save testSave = new(sessionToken.Trim(), config.Phigros.IsInternational);

                // 发起 HTTP 请求
                // 如果 token 无效，抛出 HttpRequestException
                await testSave.GetSaveContextAsync(_saveIndexLatest);

                // 验证通过后保存到配置
                config.Phigros.SessionToken = sessionToken;
                _configService.Save();

                AnsiConsole.MarkupLine("[green]sessionToken 绑定成功~~~\n[/]");
                AnsiConsole.MarkupLine("[yellow]请检查您的个人信息:\n[/]");
                PhigrosPrintingHelper.ShowUserInfo(await GetUserInfoAsync());
                return 0;
            }
            catch (HttpRequestException ex)
            {
                // Token 无效 / 无权限 / 网络错误
                AnsiConsole.MarkupLine($"[red]sessionToken 验证失败：{ex.Message}[/]");
                AnsiConsole.MarkupLine("[red]请检查 sessionToken 是否正确，或稍后重试（可能是网络问题）[/]");
                return 1;
            }
            catch (Exception ex)
            {
                // 其他异常（解密失败、格式错误等）
                AnsiConsole.MarkupLine($"[red]验证过程中发生错误：{ex.Message}[/]");
                return 1;
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]您以绑定以下账号，请使用 'phigros unbind' 解绑后再试\n[/]");
            PhigrosPrintingHelper.ShowUserInfo(await GetUserInfoAsync());
            return 1;
        }
    }

    /// <summary>
    /// 通过 TapTap 回调监听登录
    /// </summary>
    public async Task<int> BindWithTaptapAsync()
    {
        var config = _configService.GetConfig();

        if (string.IsNullOrEmpty(config.Phigros.SessionToken))
        {
            // Save 的 isInternational 与登录 API 的 useChinaEndpoint 方向相反
            bool useChinaEndpoint = !config.Phigros.IsInternational;

            using TapTapCallbackServer server = TapTapHelper.StartCallbackLogin(
                port: 0,
                useChinaEndpoint: useChinaEndpoint);

            AnsiConsole.MarkupLine($"请访问以登录: [blue]{server.BeginUrl}[/]");
            AnsiConsole.MarkupLine("[yellow]当提示 “正在打开 Phigros” 时即可返回~[/]");

            try
            {
                TapTapTokenData token = await server.WaitForTokenAsync();
                TapTapProfileData profile = await TapTapHelper.GetProfile(
                    token.Data,
                    useChinaEndpoint: useChinaEndpoint);

                // 用 TapTap 数据登录 LeanCloud 并保存 sessionToken
                string sessionToken = await LCHelper.LoginAndGetToken(
                    new LCCombinedAuthData(profile.Data, token.Data),
                    useChinaEndpoint: useChinaEndpoint);

                config.Phigros.SessionToken = sessionToken;
                _configService.Save();

                AnsiConsole.MarkupLine("\n[green]登录成功![/]\n");
                AnsiConsole.MarkupLine("[yellow]请检查您的个人信息:[/]\n");
                PhigrosPrintingHelper.ShowUserInfo(await GetUserInfoAsync());
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"发生错误：{ex.Message}");
                return 1;
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]您已经绑定以下账号，请使用 'phigros unbind' 解绑后再试\n[/]");
            PhigrosPrintingHelper.ShowUserInfo(await GetUserInfoAsync());
            return 1;
        }
    }

    public int Unbind(AppConfig config)
    {
        if (string.IsNullOrEmpty(config.Phigros.SessionToken))
        {
            AnsiConsole.MarkupLine("[red]您还没有绑定账号哦~[/]");
            return 1;
        }
        config.Phigros.SessionToken = string.Empty;
        _configService.Save();
        AnsiConsole.MarkupLine("[green]解绑成功~[/]");
        return 0;
    }
}
