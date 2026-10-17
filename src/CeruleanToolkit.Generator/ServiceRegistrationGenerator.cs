using CeruleanToolkit.Generator.Diagnostics;
using CeruleanToolkit.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Text;

namespace CeruleanToolkit.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class ServiceRegistrationGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "CeruleanToolkit.Core.Attributes.ServiceAttribute",
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) => ExtractWithDiagnostics(ctx))
            .Where(static m => m is not null)!;

        var collected = provider.Collect();

        context.RegisterSourceOutput(collected, static (spc, results) =>
        {
            var validModels = new List<ServiceModel>();

            foreach (var result in results)
            {
                if (result is null)
                    continue;

                foreach (var diagnostic in result.Diagnostics)
                {
                    var descriptor = diagnostic.Id switch
                    {
                        "CTK001" => GeneratorDiagnostics.ServiceTypeMustBeInterface,
                        "CTK002" => GeneratorDiagnostics.ServiceMustImplementInterface,
                        "CTK003" => GeneratorDiagnostics.ServiceClassNameMustEndWithService,
                        "CTK004" => GeneratorDiagnostics.ServiceMustBeInstantiable,
                        "CTK005" => GeneratorDiagnostics.ServiceMustHavePublicConstructor,
                        "CTK006" => GeneratorDiagnostics.UnsupportedGenericService,
                        _ => null
                    };

                    if (descriptor is null)
                        continue;

                    object?[] messageArgs = diagnostic.Id switch
                    {
                        "CTK002" => new object?[] { diagnostic.Arg0, diagnostic.Arg1 },
                        "CTK006" => new object?[] { diagnostic.Arg0, diagnostic.Arg1, diagnostic.Arg2 },
                        _ => new object?[] { diagnostic.Arg0 }
                    };

                    spc.ReportDiagnostic(Diagnostic.Create(
                        descriptor,
                        diagnostic.Location?.ToLocation(),
                        messageArgs));
                }

                // 收集合法数据
                if (result.Model is not null)
                {
                    validModels.Add(result.Model);
                }
            }

            // 如果存在合法的 Service，生成注册代码
            if (validModels.Any())
            {
                spc.AddSource("Registration.g.cs", GenerateRegistrationCode(validModels));
            }
        });
    }

    private static ExtractionResult? ExtractWithDiagnostics(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;
        var attribute = ctx.Attributes[0];

        if (attribute.ConstructorArguments.Length == 0)
            return null;
        var typeArg = attribute.ConstructorArguments[0];

        if (typeArg.Value is not ITypeSymbol targetTypeSymbol)
            return null;

        var location = LocationInfo.From(ctx.TargetNode.GetLocation());
        string classDisplayName = classSymbol.ToDisplayString();
        string interfaceDisplayName = targetTypeSymbol.ToDisplayString();
        var diagnostics = ImmutableArray.CreateBuilder<ServiceDiagnostic>();

        // 校验接口。不是接口就无法继续做实现/泛型检查，直接返回。
        if (targetTypeSymbol.TypeKind != TypeKind.Interface)
        {
            diagnostics.Add(new ServiceDiagnostic("CTK001", location, interfaceDisplayName));
            return new ExtractionResult(null, diagnostics.ToImmutable());
        }

        // 命名约束：服务类必须以 Service 结尾
        if (!classSymbol.Name.EndsWith("Service", StringComparison.Ordinal))
        {
            diagnostics.Add(new ServiceDiagnostic("CTK003", location, classDisplayName));
        }

        // 可实例化约束：不能是抽象类、静态类或 file 局部类
        if (classSymbol.IsAbstract || classSymbol.IsStatic || classSymbol.IsFileLocal)
        {
            diagnostics.Add(new ServiceDiagnostic("CTK004", location, classDisplayName));
        }

        // 构造函数约束：依赖注入需要至少一个公共构造函数
        if (!classSymbol.InstanceConstructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic))
        {
            diagnostics.Add(new ServiceDiagnostic("CTK005", location, classDisplayName));
        }

        var interfaceType = targetTypeSymbol as INamedTypeSymbol;
        bool interfaceIsOpenGeneric = interfaceType is { IsUnboundGenericType: true };
        bool interfaceIsGeneric = interfaceType is { IsGenericType: true };
        bool serviceIsGeneric = classSymbol.IsGenericType;

        // 查找实现关系：开放泛型按原始定义比较，封闭泛型/非泛型按构造类型比较
        INamedTypeSymbol? implementedInterface = interfaceIsOpenGeneric
            ? classSymbol.AllInterfaces.FirstOrDefault(i =>
                SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceType!.OriginalDefinition))
            : classSymbol.AllInterfaces.FirstOrDefault(i =>
                SymbolEqualityComparer.Default.Equals(i, targetTypeSymbol));

        if (implementedInterface is null)
        {
            diagnostics.Add(new ServiceDiagnostic("CTK002", location, classDisplayName, interfaceDisplayName));
            return new ExtractionResult(null, diagnostics.ToImmutable());
        }

        // 泛型组合约束：
        // - 开放泛型接口 -> 泛型服务类，且泛型参数按位置一一对应
        // - 封闭泛型接口 -> 非泛型服务类
        // - 非泛型接口 -> 非泛型服务类
        if (interfaceIsOpenGeneric)
        {
            if (!serviceIsGeneric)
            {
                diagnostics.Add(new ServiceDiagnostic("CTK006", location, classDisplayName, interfaceDisplayName,
                    "接口是开放泛型，但服务类不是泛型类；请使用 FooService<T> : IFoo<T> 的形式"));
            }
            else if (classSymbol.ContainingType?.IsGenericType == true)
            {
                diagnostics.Add(new ServiceDiagnostic("CTK006", location, classDisplayName, interfaceDisplayName,
                    "不支持嵌套在泛型类型中的服务类"));
            }
            else
            {
                int expectedArity = interfaceType!.Arity;
                if (classSymbol.TypeParameters.Length != expectedArity)
                {
                    diagnostics.Add(new ServiceDiagnostic("CTK006", location, classDisplayName, interfaceDisplayName,
                        $"服务类有 {classSymbol.TypeParameters.Length} 个泛型参数，但接口需要 {expectedArity} 个"));
                }
                else
                {
                    for (int i = 0; i < expectedArity; i++)
                    {
                        if (implementedInterface.TypeArguments[i] is not ITypeParameterSymbol typeParameter ||
                            !SymbolEqualityComparer.Default.Equals(typeParameter, classSymbol.TypeParameters[i]))
                        {
                            diagnostics.Add(new ServiceDiagnostic("CTK006", location, classDisplayName,
                                interfaceDisplayName,
                                "服务类的泛型参数必须按相同顺序对应接口的泛型参数，例如 class FooService<T> : IFoo<T>"));
                            break;
                        }
                    }
                }
            }
        }
        else if (interfaceIsGeneric && serviceIsGeneric)
        {
            diagnostics.Add(new ServiceDiagnostic("CTK006", location, classDisplayName, interfaceDisplayName,
                "接口是封闭泛型，但服务类是泛型类；请改用开放泛型接口（如 IFoo<>）"));
        }
        else if (!interfaceIsGeneric && serviceIsGeneric)
        {
            diagnostics.Add(new ServiceDiagnostic("CTK006", location, classDisplayName, interfaceDisplayName,
                "服务类是泛型类，但接口不是泛型；泛型服务类只能注册到开放泛型接口"));
        }

        // 只要有任何一条约束不满足，就不生成注册代码
        if (diagnostics.Count > 0)
        {
            return new ExtractionResult(null, diagnostics.ToImmutable());
        }

        // 解析命名参数 (Lifetime, Platform)。
        // 不依赖枚举的底层数值，按枚举成员名映射；这样即使以后调整枚举顺序也不会静默注册错。
        // 默认值与 ServiceAttribute 的属性初始值保持一致：
        // Lifetime = ServiceLifetime.Transient，Platform = SupportedPlatform.All。
        string lifetimeMember = GetEnumMemberName(attribute, "Lifetime", "Transient");
        string lifetimeMethod = lifetimeMember switch
        {
            "Singleton" => "AddSingleton",
            "Scoped" => "AddScoped",
            "Transient" => "AddTransient",
            _ => "AddTransient"
        };

        string platformMember = GetEnumMemberName(attribute, "Platform", "All");
        string? platformSymbol = platformMember switch
        {
            "Windows" => "WINDOWS",
            "Linux" => "LINUX",
            "MacOS" => "MACOS",
            _ => null
        };

        var format = SymbolDisplayFormat.FullyQualifiedFormat;
        string interfaceName;
        string implementationName;
        bool useTypeOverload;

        if (interfaceIsOpenGeneric)
        {
            // 开放泛型只能用 typeof(IFoo<>) + typeof(FooService<>) 的 Type 重载注册，
            // 不能写成泛型方法 AddTransient<IFoo<>, FooService<>>()。
            interfaceName = interfaceType!.ToDisplayString(format);
            implementationName = classSymbol.ConstructUnboundGenericType().ToDisplayString(format);
            useTypeOverload = true;
        }
        else
        {
            interfaceName = targetTypeSymbol.ToDisplayString(format);
            implementationName = classSymbol.ToDisplayString(format);
            useTypeOverload = false;
        }

        var model = new ServiceModel(
            InterfaceFullName: interfaceName,
            ImplementationFullName: implementationName,
            LifetimeMethodName: lifetimeMethod,
            PlatformSymbol: platformSymbol,
            UseTypeOverload: useTypeOverload
        );

        return new ExtractionResult(model, ImmutableArray<ServiceDiagnostic>.Empty);
    }

    /// <summary>
    /// 从特性的命名参数中取出枚举成员名。
    /// 如果调用方没有显式指定该参数，则返回 <paramref name="defaultMemberName"/>。
    /// <para>
    /// 这样生成器就不依赖枚举的底层数值，调整枚举成员顺序不会静默改变注册行为。
    /// </para>
    /// </summary>
    private static string GetEnumMemberName(
        AttributeData attribute,
        string argumentName,
        string defaultMemberName)
    {
        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key != argumentName)
                continue;

            var typedValue = namedArg.Value;
            if (typedValue.Value is null || typedValue.Type is not INamedTypeSymbol enumType)
                return defaultMemberName;

            int numericValue = System.Convert.ToInt32(typedValue.Value);
            foreach (var member in enumType.GetMembers())
            {
                if (member is IFieldSymbol field &&
                    field.HasConstantValue &&
                    field.ConstantValue is not null &&
                    System.Convert.ToInt32(field.ConstantValue) == numericValue)
                {
                    return field.Name;
                }
            }

            // 未知/非法枚举值：回退到默认值，避免生成无法编译的代码。
            return defaultMemberName;
        }

        return defaultMemberName;
    }

    private static string GenerateRegistrationCode(List<ServiceModel> models)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine();
        sb.AppendLine("namespace CeruleanToolkit.Core;");
        sb.AppendLine();
        sb.AppendLine("public static class Registration");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddAllServices(this IServiceCollection services)");
        sb.AppendLine("    {");

        foreach (var model in models
            .OrderBy(m => m.InterfaceFullName, StringComparer.Ordinal)
            .ThenBy(m => m.ImplementationFullName, StringComparer.Ordinal))
        {
            if (model.PlatformSymbol is not null)
                sb.AppendLine($"#if {model.PlatformSymbol}");

            if (model.UseTypeOverload)
            {
                sb.AppendLine($"        services.{model.LifetimeMethodName}(typeof({model.InterfaceFullName}), typeof({model.ImplementationFullName}));");
            }
            else
            {
                sb.AppendLine($"        services.{model.LifetimeMethodName}<{model.InterfaceFullName}, {model.ImplementationFullName}>();");
            }

            if (model.PlatformSymbol is not null)
                sb.AppendLine("#endif");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }
}