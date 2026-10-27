using CeruleanToolkit.Core.Helpers.Pringting;
using PhiLibrary197.CloudSave;
using PhiLibrary197.Scores;

namespace CeruleanToolkit.Core.Services;

public sealed partial class PhigrosService
{
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
            case ScoreStatus.Bugged:
                parsedStatus = "[red]BUG[/]";
                break;
            case ScoreStatus.NotFc:
                parsedStatus = "[dim]-[/]";
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
        using var save = GetSave(config);
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
    /// 获取 B30 成绩
    /// </summary>
    public async Task<int> GetB30Async()
    {
        PhigrosPrintingHelper.ShowUserInfo(await GetUserInfoAsync());
        var constantMap = _songDatabase.ToConstantMap();
        var nameMap = _songDatabase.ToNameMap();
        using var save = GetSave(_configService.GetConfig());
        SaveContext ctx = await save.GetSaveContextAsync(_saveIndexLatest);
        GameRecord record = ctx.ReadGameRecord();
        var (phis, sorted, _) = record.GetSortedListForRks(constantMap, nameMap);

        // 新 API 的 OtherScores 包含 Phi，B30 展示时去掉已在 Phi 3 中显示的部分
        var otherScores = sorted.Where(score => !phis.Contains(score)).ToList();
        PhigrosPrintingHelper.ShowB30((phis, otherScores), this);
        return 0;
    }
}
