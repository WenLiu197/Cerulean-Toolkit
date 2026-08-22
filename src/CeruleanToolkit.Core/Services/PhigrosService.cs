using CeruleanToolkit.Core.Helpers.Pringting;
using CeruleanToolkit.Core.Interfaces;
using PhiLibrary197.CloudSave;
using PhiLibrary197.CloudSave.HttpModels;
using Spectre.Console;

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
    private static readonly string _difficultyPath = $@"{Environment.CurrentDirectory}\Info\difficulty.tsv";
    private static readonly string _infoPath = $@"{Environment.CurrentDirectory}\Info\info.tsv";
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
    private static Save GetSave(AppConfig config) => new Save(
        config.Phigros.SessionToken,
        config.Phigros.IsInternational);

    /// <summary>
    /// 顾名思义，就是返回上下文，搭配 <see cref="PhigrosService.GetSave(AppConfig config)"/> 食用更佳
    /// </summary>
    /// <param name="save">自己看~~~</param>
    /// <returns>返回存档上下文</returns>
    private static async Task<SaveContext> GetSaveContextAsync(Save save)
    {
        SaveInfo saveInfo = (await save.GetSaveInfoFromCloudAsync()).Results[_saveIndexLatest];
        byte[] rawZip = await save.GetSaveZipAsync(saveInfo.Simplify());
        SaveContext ctx = await SaveContext.FromZipAsync(rawZip, saveInfo, save.Decrypt);
        return ctx;
    }

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

    /// <summary>
    /// 获取基本用户信息
    /// </summary>
    /// <param name="save">存档</param>
    /// <returns>存储了用户信息的元组</returns>
    public async Task<(string NickName, string Rks, string Intro, string Data, string Challenge)> GetUserInfo(Save save)
    {

        var info = await save.GetPlayerInfoAsync();
        var context = await GetSaveContextAsync(save);
        var record = context.ReadGameRecord();
        var progress = context.ReadGameProgress();
        var userInfo = context.ReadGameUserInfo();
        Dictionary<ChartConstantKey, float> constantMap = _songDatabase.ToConstantMap();
        Dictionary<string, string> nameMap = _songDatabase.ToNameMap();
        string rks = Math.Round(record.GetSortedListForRks(constantMap, nameMap).Rks, 4).ToString();
        string data = progress.Money.ToString();
        string challenge = ParseChallengeModeRank(progress.ChallengeModeRank);
        return (info.NickName ?? "Guest", rks, userInfo.Intro, data, challenge);
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
                    await GetSaveContextAsync(testSave);
                }

                // 验证通过后保存到配置
                config.Phigros.SessionToken = sessionToken;
                _configService.Save();

                AnsiConsole.MarkupLine("[green]sessionToken 绑定成功~~~\n[/]");
                AnsiConsole.MarkupLine("[yellow]请检查您的个人信息:\n[/]");
                PhigrosPrintingHelper.ShowUserInfo(await GetUserInfo(GetSave(config)));
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
            PhigrosPrintingHelper.ShowUserInfo(await GetUserInfo(GetSave(config)));
            return 1;
        }
    }

    public int Unbind(AppConfig config)
    {
        if (string.IsNullOrEmpty(config.Phigros.SessionToken))
        {
            AnsiConsole.MarkupLine("[red]您还没有绑定账号哦~[/]");
        }
        config.Phigros.SessionToken = string.Empty;
        _configService.Save();
        return 0;
    }
}
