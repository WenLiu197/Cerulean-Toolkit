using System;

namespace CeruleanToolkit.Core.Attributes;

/// <summary>
/// 服务生命周期
/// </summary>
public enum ServiceLifetime
{
    Singleton,
    Transient,
    Scoped
}

/// <summary>
/// 支持的平台（用于条件编译）
/// </summary>
public enum SupportedPlatform
{
    All,
    Windows,
    Linux,
    MacOS
}

/// <summary>
/// 用于标记服务类，标记后会由源生成器生成自动注册拓展方法，支持条件编译
/// <para>
/// 服务类名必须以 Service 结尾，并且至少有一个 public 构造函数。
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ServiceAttribute : Attribute
{
    public ServiceAttribute(Type serviceType) => ServiceType = serviceType;

    public Type ServiceType { get; }
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
    public SupportedPlatform Platform { get; set; } = SupportedPlatform.All;
}
