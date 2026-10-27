using CeruleanToolkit.Core.Attributes;
using CeruleanToolkit.Core.Interfaces;
using PhiLibrary197.Songs;

namespace CeruleanToolkit.Core.Services;

/// <summary>
/// 所有 phigros 相关命令的业务逻辑
/// <para>
/// <i>好难……</i>
/// </para>
/// <para>
/// <b>声明: </b>本项目使用的 PhiLibrary197 NuGet 为 <i>WenLiu97(本仓库所有者)</i> 自己魔改的 Native AOT 友好版，该 NuGet 版权归 github@yt6983138 所有，详情见 README.md
/// </para>
/// </summary>
[Service(typeof(IPhigrosService), Lifetime = ServiceLifetime.Transient)]
public sealed partial class PhigrosService : IPhigrosService
{
    private readonly IConfigService _configService;
    private readonly SongDatabase _songDatabase = SongDatabase.LoadFromBuiltIn();
    private const int _saveIndexLatest = 0;

    /// <summary>
    /// 通过构造函数注入 IConfigService
    /// </summary>
    /// <param name="configService">配置服务实例</param>
    public PhigrosService(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }
}
