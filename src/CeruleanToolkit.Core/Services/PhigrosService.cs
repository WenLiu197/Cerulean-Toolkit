using CeruleanToolkit.Core.Helpers;
using CeruleanToolkit.Core.Helpers.Pringting;
using CeruleanToolkit.Core.Interfaces;
using PhiLibrary197.CloudSave;
using PhiLibrary197.CloudSave.HttpModels;
using PhiLibrary197.CloudSave.Login;
using Spectre.Console;
using System.Net;

namespace CeruleanToolkit.Core.Services;

/// <summary>
/// 所有 phigros 相关命令的业务逻辑
/// <para>
/// <i>好难……</i>
/// </para>
/// <para>
/// <b>声明: </b>本项目使用的 PhiLibrary197 NuGet 为 <i>WenLiu97(本仓库所有者)</i> 自己魔改的 Native AOT 友好版，该 NuGet 版权归 github@yt6983138 所有，详情见 README.md
/// </para>
/// </para>
/// </summary>
public sealed class PhigrosService : IPhigrosService
{
    private readonly IConfigService _configService;

    private static readonly string _installPath = InstallPathHelper.GetEffectivePath();
    private static readonly string _difficultyPath = Path.Combine(_installPath, "Info", "difficulty.tsv");
    private static readonly string _infoPath = Path.Combine(_installPath, "Info", "info.tsv");
    private SongDatabase _songDatabase = SongDatabase.Load(_difficultyPath, _infoPath);
    private const int _saveIndexLatest = 0;

    /// <summary>
    /// 通过构造函数注入 IConfigService
    /// </summary>
    /// <param name="configService">配置服务实例</param>
    public PhigrosService(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <summary>
    /// 获取云存档
    /// </summary>
    /// <param name="config">应用配置实例</param>
    /// <returns>云存档</returns>
    private static Save GetSave(AppConfig config)
    {
        try
        {
            return new Save(config.Phigros.SessionToken, config.Phigros.IsInternational);
        }
        catch (Exception ex)
        {
            throw new Exception("还没有绑定账户 (配置项 'Phigros.SessionToken' 为空或无效)", ex);
        }

    }

    /// <summary>
    /// 顾名思义，就是返回上下文，搭配 <see cref="PhigrosService.GetSave(AppConfig config)"/> 食用更佳
    /// </summary>
    /// <param name="save">自己看~~~</param>
    /// <returns>返回存档上下文</returns>
    //private static async Task<SaveContext> GetSaveContextAsync(Save save)
    //{
    //    SaveInfo saveInfo = (await save.GetSaveInfoFromCloudAsync()).Results[_saveIndexLatest];
    //    byte[] rawZip = await save.GetSaveZipAsync(saveInfo.Simplify());
    //    SaveContext ctx = await SaveContext.FromZipAsync(rawZip, saveInfo, save.Decrypt);
    //    return ctx;
    //}

    /// <summary>
    /// 解析课题模式成绩
    /// </summary>
    private string ParseChallengeModeRank(Challenge challenge)
    {
        if (challenge.HasEverDone)
        {
            string rank = challenge.Rank.ToString() switch
            {
                "Green" => "[#00FA9A]绿",
                "Blue" => "[#00BFFF]蓝",
                "Red" => "[red]红",
                "Gold" => "[yellow]金",
                "Rainbow" => "[#FF69B4]彩",
                _ => "NAN"
            };

            string level = challenge.Level.ToString();

            return $"{rank} {level}[/]";
        }
        return "Null";
    }

    public string ParseStatus(ScoreStatus status)
    {
        string parsedStatus = string.Empty;
        switch (status)
        {
            case ScoreStatus.Fc:
                parsedStatus = "[bold DeepSkyBlue1]V[/]";
                break;
            case ScoreStatus.Phi:
                parsedStatus = "[bold yellow]φ[/]";
                break;
            case ScoreStatus.Vu:
                parsedStatus = "[bold]V[/]";
                break;
            case ScoreStatus.S:
                parsedStatus = "[bold]S[/]";
                break;
            case ScoreStatus.A:
                parsedStatus = "[bold]A[/]";
                break;
            case ScoreStatus.B:
                parsedStatus = "[bold]B[/]";
                break;
            case ScoreStatus.C:
                parsedStatus = "[bold]C[/]";
                break;
            case ScoreStatus.False:
                parsedStatus = "[bold]F[/]";
                break;
            default:
                parsedStatus = "?";
                break;
        }

        return parsedStatus;
    }

    public string ParseDifficulty(Difficulty difficulty)
    {
        string parsedDifficulty = string.Empty;
        switch (difficulty)
        {
            case Difficulty.EZ:
                parsedDifficulty = "[#00FA9A]EZ[/]";
                break;
            case Difficulty.HD:
                parsedDifficulty = "[#00BFFF]HD[/]";
                break;
            case Difficulty.IN:
                parsedDifficulty = "[red]IN[/]";
                break;
            case Difficulty.AT:
                parsedDifficulty = "[white]AT[/]";
                break;
            case Difficulty.Legacy:
                parsedDifficulty = "[#FF69B4]Legacy[/]";
                break;
            case Difficulty.SP:
                parsedDifficulty = "[#FF69B4n]SP[/]";
                break;
            default:
                parsedDifficulty = "[white]?[/]";
                break;
        }
        return parsedDifficulty;
    }

    /// <summary>
    /// 获取基本用户信息
    /// </summary>
    /// <returns>存储了用户信息的元组</returns>
    public async Task<(string NickName, string Rks, string Intro, string Data, string Challenge, bool ShowIntro)> GetUserInfoAsync(bool showIntro = true)
    {
        var config = _configService.GetConfig();
        var save = GetSave(config);
        var info = await save.GetPlayerInfoAsync();
        var context = await save.GetSaveContextAsync(_saveIndexLatest);
        var record = context.ReadGameRecord();
        var progress = context.ReadGameProgress();
        var userInfo = context.ReadGameUserInfo();
        Dictionary<ChartConstantKey, float> constantMap = _songDatabase.ToConstantMap();
        Dictionary<string, string> nameMap = _songDatabase.ToNameMap();
        string rks = Math.Round(record.GetSortedListForRks(constantMap, nameMap).Rks, 4).ToString();
        string data = progress.Money.ToString();
        string challenge = ParseChallengeModeRank(progress.ChallengeModeRank);
        return (info.NickName ?? "Guest", rks, userInfo.Intro, data, challenge, showIntro);
    }

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

    /// <summary>
    /// 获取 B30 成绩
    /// </summary>
    public async Task<int> GetB30Async()
    {
        PhigrosPrintingHelper.ShowUserInfo(await GetUserInfoAsync());
        var constantMap = _songDatabase.ToConstantMap();
        var nameMap = _songDatabase.ToNameMap();
        var save = GetSave(_configService.GetConfig());
        SaveContext ctx = await save.GetSaveContextAsync(_saveIndexLatest);
        GameRecord record = ctx.ReadGameRecord();
        Summary summary = ctx.ReadSummary();
        var (phis, otherScores, _) = record.GetSortedListForRks(constantMap, nameMap);
        PhigrosPrintingHelper.ShowB30((phis, otherScores), this);
        return 0;
    }
}