using Microsoft.CodeAnalysis;

namespace CeruleanToolkit.Generator.Diagnostics;

internal static class GeneratorDiagnostics
{
    private const string Category = "CeruleanToolkit.SourceGenerator";

    public static readonly DiagnosticDescriptor ServiceTypeMustBeInterface = new(
        id: "CTK001",
        title: "ServiceType 必须是接口",
        messageFormat: "类型 '{0}' 必须是接口（Interface）",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ServiceMustImplementInterface = new(
        id: "CTK002",
        title: "未实现指定的服务接口",
        messageFormat: "类 '{0}' 必须实现接口 '{1}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ServiceClassNameMustEndWithService = new(
        id: "CTK003",
        title: "服务类名必须以 Service 结尾",
        messageFormat: "服务类 '{0}' 的类名必须以 'Service' 结尾，例如 FooService",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ServiceMustBeInstantiable = new(
        id: "CTK004",
        title: "服务类必须可实例化",
        messageFormat: "服务类 '{0}' 不能是 abstract、static 或 file 局部类，依赖注入无法创建它",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ServiceMustHavePublicConstructor = new(
        id: "CTK005",
        title: "服务类必须有公共构造函数",
        messageFormat: "服务类 '{0}' 必须至少有一个 public 构造函数，依赖注入才能创建它",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedGenericService = new(
        id: "CTK006",
        title: "不支持的服务泛型组合",
        messageFormat: "服务类 '{0}' 无法注册到接口 '{1}'：{2}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
