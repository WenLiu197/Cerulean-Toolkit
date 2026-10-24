using CeruleanToolkit.Core.Helpers.Pringting;
using CeruleanToolkit.Core.Interfaces;
using PhiLibrary197.CloudSave;
using PhiLibrary197.CloudSave.Login;
using Spectre.Console;
using System.Net;

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

            // 理论上不会触发，只做防御性编程
            if (string.IsNullOrWhiteSpace(sessionToken))
            {
                AnsiConsole.MarkupLine("[red]输入无效，操作已取消[/]");
                return 1;
            }

            AnsiConsole.MarkupLine("[blue]正在验证 sessionToken 有效性...[/]");

            try
            {
                {
                    Save testSave = new(sessionToken.Trim(), config.Phigros.IsInternational);

                    // 发起 HTTP 请求
                    // 如果 token 无效，抛出 HttpRequestException
                    await testSave.GetSaveContextAsync(_saveIndexLatest);
                }

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
            // 生成回调登录数据
            CallbackLoginData login = TapTapHelper.GenerateCallbackLoginUrl(
                "http://localhost:8080/callback");

            // 引导用户访问，在浏览器里完成授权
            Console.MarkupLine($"请访问以登录: {login.BeginUrl}\n");
            AnsiConsole.MarkupLine($"[yellow]当提示 “正在打开 Phigros” 时即可返回~[/]");

            // 第一次写这种监听器………………希望不会出什么差错
            using var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();

            try
            {
                // 等待 TapTap 的回调
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                // 检查是否是 /callback 路径（以防万一……还是写上吧……防御性编程这一块）
                if (request.Url?.AbsolutePath != "/callback")
                {
                    Console.Error.WriteLine($"接收到意外的路径：{request.Url?.AbsolutePath}");
                    return 1;
                }

                // 从查询字符串中提取 code
                string? code = request.QueryString["code"];

                if (string.IsNullOrEmpty(code))
                {
                    Console.Error.WriteLine("回调中缺少 code 参数");
                    return 1;
                }

                // 使用 code 交换 Token
                TapTapTokenData token = await TapTapHelper.HandleCallbackLogin(login, code);
                TapTapProfileData profile = await TapTapHelper.GetProfile(token.Data);

                // 用 TapTap 数据登录 LeanCloud 并保存 sessionToken
                string sessionToken = await LCHelper.LoginAndGetToken(
                    new LCCombinedAuthData(profile.Data, token.Data));
                config.Phigros.SessionToken = sessionToken;
                _configService.Save();

                AnsiConsole.MarkupLine("\n[green]登录成功![/]\n");
                AnsiConsole.MarkupLine("[yellow]请检查您的个人信息:[/]\n");
                PhigrosPrintingHelper.ShowUserInfo(await GetUserInfoAsync());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"发生错误：{ex.Message}");
            }
            finally
            {
                listener.Stop();   // 停止监听，释放资源
            }
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]您以绑定以下账号，请使用 'phigros unbind' 解绑后再试\n[/]");
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
