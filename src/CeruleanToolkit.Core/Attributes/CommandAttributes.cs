using System;

namespace CeruleanToolkit.Core.Attributes;

/// <summary>
/// 标记根命令。命令类必须继承 <c>System.CommandLine.Command</c>，
/// 并在构造函数中通过 <c>base(name, description)</c> 设置名称和描述。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// 同级命令中的排序值，越小越靠前。
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 注册该命令的目标平台，默认为所有平台。
    /// </summary>
    public SupportedPlatform Platform { get; set; } = SupportedPlatform.All;
}

/// <summary>
/// 标记子命令，并指定它的父命令类型。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class SubCommandAttribute : Attribute
{
    public SubCommandAttribute(Type parent) => Parent = parent;

    /// <summary>
    /// 父命令类型。
    /// </summary>
    public Type Parent { get; }

    /// <summary>
    /// 同级命令中的排序值，越小越靠前。
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 注册该命令的目标平台，默认为所有平台。
    /// 子命令的平台不能比父命令更宽。
    /// </summary>
    public SupportedPlatform Platform { get; set; } = SupportedPlatform.All;
}
// 大肥鲸鱼太好用了