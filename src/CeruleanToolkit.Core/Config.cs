using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core
{
    /// <summary>
    /// 表示应用配置的根对象，对应 <c>config.json</c> 文件的整体结构~~~
    /// <para>
    /// <i>懂得都懂~~~</i>
    /// </para>
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// 获取或设置通用配置节。
        /// </summary>
        public GeneralConfig General { get; set; } = new();

        /// <summary>
        /// 获取或设置 Phigros 相关配置节。
        /// </summary>
        public PhigrosConfig Phigros { get; set; } = new();
    }

    /// <summary>
    /// 通用配置节
    /// </summary>
    public sealed class GeneralConfig
    {
        /// <summary>
        /// 获取或设置用户显示的日志等级。
        /// </summary>
        /// <value>
        /// 日志等级字符串（如 <c>"warning"</c>、<c>"information"</c>），
        /// 默认 <c>"warning"</c>，即仅显示 warning 及以上级别的日志。
        /// </value>
        public string LogLevel { get; set; } = "warning";
    }

    // TODO: 后续继续开发 Phigros 相关命令，v0.1.0 先开发获取成绩的命令

    /// <summary>
    /// 表示 Phigros 相关配置节。
    /// </summary>
    public sealed class PhigrosConfig
    {
        /// <summary>
        /// 获取或设置 Phigros 会话令牌。
        /// </summary>
        /// <value>用于 Phigros 查分接口鉴权的会话令牌，默认为空字符串。</value>
        public string SessionToken { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置是否为国际服。
        /// </summary>
        /// <value><see langword="true"/> 表示国际服，<see langword="false"/> 表示国服。</value>
        public bool IsInternational { get; set; }
    }

}
