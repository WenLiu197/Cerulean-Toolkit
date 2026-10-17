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

    public static readonly DiagnosticDescriptor CommandClassNameMustEndWithCommand = new(
        id: "CTK101",
        title: "命令类名必须以 Command 结尾",
        messageFormat: "命令类 '{0}' 的类名必须以 'Command' 结尾，例如 FooCommand",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandMustInheritCommand = new(
        id: "CTK102",
        title: "命令类必须继承 System.CommandLine.Command",
        messageFormat: "命令类 '{0}' 必须继承 System.CommandLine.Command",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SubCommandParentMustBeCommand = new(
        id: "CTK103",
        title: "子命令的父类型必须是命令类型",
        messageFormat: "子命令 '{0}' 的父类型 '{1}' 必须是命令类型（继承 System.CommandLine.Command）",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandCannotBeRootAndSubCommand = new(
        id: "CTK104",
        title: "不能同时标记 Command 和 SubCommand",
        messageFormat: "类 '{0}' 不能同时标记 [Command] 和 [SubCommand]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandParentCycle = new(
        id: "CTK105",
        title: "命令父子关系存在循环",
        messageFormat: "命令父子关系存在循环：{0}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandMustBeInstantiable = new(
        id: "CTK106",
        title: "命令类必须可实例化",
        messageFormat: "命令类 '{0}' 不能是 abstract 或开放泛型",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandMustHavePublicConstructor = new(
        id: "CTK107",
        title: "命令类必须有公共构造函数",
        messageFormat: "命令类 '{0}' 必须至少有一个 public 构造函数，依赖注入才能创建它",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CommandParentMustBeMarked = new(
        id: "CTK108",
        title: "父类型没有标记为命令",
        messageFormat: "父类型 '{0}' 必须标记 [Command] 或 [SubCommand]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SubCommandPlatformTooBroad = new(
        id: "CTK109",
        title: "子命令平台不能比父命令更宽",
        messageFormat: "子命令 '{0}' 的平台 '{1}' 不能比父命令 '{2}' 的平台更宽",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
