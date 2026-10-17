using CeruleanToolkit.Generator.Diagnostics;
using CeruleanToolkit.Generator.Helpers;
using CeruleanToolkit.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace CeruleanToolkit.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class CommandRegistrationGenerator : IIncrementalGenerator
{
    private const string CommandAttributeMetadataName = "CeruleanToolkit.Core.Attributes.CommandAttribute";
    private const string SubCommandAttributeMetadataName = "CeruleanToolkit.Core.Attributes.SubCommandAttribute";
    private const string CommandTypeMetadataName = "System.CommandLine.Command";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is ClassDeclarationSyntax,
            transform: static (ctx, _) => ExtractCommand(ctx))
            .Where(static result => result is not null)!;

        context.RegisterSourceOutput(provider.Collect(), static (spc, results) =>
        {
            bool hasAny = results.Any(static result => result is not null);
            if (!hasAny)
                return;

            var diagnostics = new List<GeneratorDiagnostic>();
            var models = new List<CommandModel>();

            foreach (var result in results)
            {
                if (result is null)
                    continue;

                diagnostics.AddRange(result.Diagnostics);
                if (result.Model is not null)
                    models.Add(result.Model);
            }

            var invalidTypes = ValidateRelationships(models, diagnostics);

            foreach (var diagnostic in diagnostics)
            {
                var descriptor = diagnostic.Id switch
                {
                    "CTK101" => GeneratorDiagnostics.CommandClassNameMustEndWithCommand,
                    "CTK102" => GeneratorDiagnostics.CommandMustInheritCommand,
                    "CTK103" => GeneratorDiagnostics.SubCommandParentMustBeCommand,
                    "CTK104" => GeneratorDiagnostics.CommandCannotBeRootAndSubCommand,
                    "CTK105" => GeneratorDiagnostics.CommandParentCycle,
                    "CTK106" => GeneratorDiagnostics.CommandMustBeInstantiable,
                    "CTK107" => GeneratorDiagnostics.CommandMustHavePublicConstructor,
                    "CTK108" => GeneratorDiagnostics.CommandParentMustBeMarked,
                    "CTK109" => GeneratorDiagnostics.SubCommandPlatformTooBroad,
                    _ => null
                };

                if (descriptor is null)
                    continue;

                object?[] messageArgs = diagnostic.Id switch
                {
                    "CTK103" => new object?[] { diagnostic.Arg0, diagnostic.Arg1 },
                    "CTK109" => new object?[] { diagnostic.Arg0, diagnostic.Arg1, diagnostic.Arg2 },
                    _ => new object?[] { diagnostic.Arg0 }
                };

                spc.ReportDiagnostic(Diagnostic.Create(
                    descriptor,
                    diagnostic.Location?.ToLocation(),
                    messageArgs));
            }

            var validModels = RemoveInvalidModels(models, invalidTypes);
            spc.AddSource("CommandRegistration.g.cs", GenerateRegistrationCode(validModels));
        });
    }

    private static CommandExtractionResult? ExtractCommand(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is not ClassDeclarationSyntax classDeclaration)
            return null;
        if (ctx.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
            return null;

        var commandAttribute = FindAttribute(classSymbol, CommandAttributeMetadataName);
        var subCommandAttribute = FindAttribute(classSymbol, SubCommandAttributeMetadataName);

        if (commandAttribute is null && subCommandAttribute is null)
            return null;

        var location = LocationInfo.From(ctx.Node.GetLocation());
        string displayName = classSymbol.ToDisplayString();
        var diagnostics = ImmutableArray.CreateBuilder<GeneratorDiagnostic>();

        // 命令类名必须以 Command 结尾
        if (!classSymbol.Name.EndsWith("Command", StringComparison.Ordinal))
        {
            diagnostics.Add(new GeneratorDiagnostic("CTK101", location, displayName));
        }

        // 必须继承 System.CommandLine.Command
        if (!InheritsFromCommand(classSymbol))
        {
            diagnostics.Add(new GeneratorDiagnostic("CTK102", location, displayName));
        }

        // 必须是可实例化的普通类
        if (classSymbol.IsAbstract || classSymbol.IsGenericType)
        {
            diagnostics.Add(new GeneratorDiagnostic("CTK106", location, displayName));
        }

        // DI 需要至少一个 public 构造函数
        if (!classSymbol.InstanceConstructors.Any(c =>
                c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic))
        {
            diagnostics.Add(new GeneratorDiagnostic("CTK107", location, displayName));
        }

        // 不能同时标根命令和子命令
        if (commandAttribute is not null && subCommandAttribute is not null)
        {
            diagnostics.Add(new GeneratorDiagnostic("CTK104", location, displayName));
            return new CommandExtractionResult(null, diagnostics.ToImmutable());
        }

        AttributeData attribute = commandAttribute ?? subCommandAttribute!;
        string platformMember = EnumMemberHelper.GetMemberName(attribute, "Platform", "All");
        string? platformSymbol = ToPlatformSymbol(platformMember);

        string? parentTypeFullName = null;
        string? parentDisplayName = null;

        if (subCommandAttribute is not null)
        {
            var parentSymbol = subCommandAttribute.ConstructorArguments.Length > 0
                ? subCommandAttribute.ConstructorArguments[0].Value as INamedTypeSymbol
                : null;

            if (parentSymbol is null)
            {
                diagnostics.Add(new GeneratorDiagnostic("CTK103", location, displayName, "<unknown>"));
            }
            else
            {
                parentTypeFullName = parentSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                parentDisplayName = parentSymbol.ToDisplayString();

                bool parentIsCommand = InheritsFromCommand(parentSymbol);
                bool parentIsMarked =
                    FindAttribute(parentSymbol, CommandAttributeMetadataName) is not null ||
                    FindAttribute(parentSymbol, SubCommandAttributeMetadataName) is not null;

                if (!parentIsCommand)
                {
                    diagnostics.Add(new GeneratorDiagnostic("CTK103", location, displayName, parentDisplayName));
                }
                else if (!parentIsMarked)
                {
                    diagnostics.Add(new GeneratorDiagnostic("CTK108", location, parentDisplayName));
                }
            }
        }

        if (diagnostics.Count > 0)
        {
            return new CommandExtractionResult(null, diagnostics.ToImmutable());
        }

        var model = new CommandModel(
            TypeFullName: classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            DisplayName: displayName,
            ParentTypeFullName: parentTypeFullName,
            ParentDisplayName: parentDisplayName,
            Order: GetOrder(attribute),
            PlatformSymbol: platformSymbol,
            PlatformMember: platformMember,
            Location: location);

        return new CommandExtractionResult(model, ImmutableArray<GeneratorDiagnostic>.Empty);
    }

    private static HashSet<string> ValidateRelationships(
        List<CommandModel> models,
        List<GeneratorDiagnostic> diagnostics)
    {
        var modelsByType = models.ToDictionary(m => m.TypeFullName, StringComparer.Ordinal);
        var invalidTypes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var model in models)
        {
            if (model.ParentTypeFullName is null)
                continue;

            if (!modelsByType.TryGetValue(model.ParentTypeFullName, out var parent))
            {
                diagnostics.Add(new GeneratorDiagnostic(
                    "CTK103",
                    model.Location,
                    model.DisplayName,
                    model.ParentDisplayName ?? model.ParentTypeFullName));
                invalidTypes.Add(model.TypeFullName);
                continue;
            }

            // 父命令是特定平台时，子命令不能是 All 或其他平台
            if (parent.PlatformSymbol is not null && model.PlatformSymbol != parent.PlatformSymbol)
            {
                diagnostics.Add(new GeneratorDiagnostic(
                    "CTK109",
                    model.Location,
                    model.DisplayName,
                    model.PlatformMember,
                    parent.PlatformMember));
                invalidTypes.Add(model.TypeFullName);
            }
        }

        var reportedCycles = new HashSet<string>(StringComparer.Ordinal);

        foreach (var model in models)
        {
            if (reportedCycles.Contains(model.TypeFullName))
                continue;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            CommandModel? current = model;

            while (current is not null)
            {
                if (!seen.Add(current.TypeFullName))
                {
                    var cycle = string.Join(
                        " -> ",
                        seen.Select(type => modelsByType[type].DisplayName)
                            .Append(current.DisplayName));
                    diagnostics.Add(new GeneratorDiagnostic("CTK105", model.Location, cycle));

                    foreach (var type in seen)
                    {
                        reportedCycles.Add(type);
                        invalidTypes.Add(type);
                    }

                    break;
                }

                if (current.ParentTypeFullName is null)
                    break;

                if (!modelsByType.TryGetValue(current.ParentTypeFullName, out var parent))
                    break;

                current = parent;
            }
        }

        return invalidTypes;
    }

    private static List<CommandModel> RemoveInvalidModels(
        List<CommandModel> models,
        HashSet<string> invalidTypes)
    {
        var modelsByType = models.ToDictionary(m => m.TypeFullName, StringComparer.Ordinal);
        bool changed = true;

        // 父命令无效时，整棵子树都不生成
        while (changed)
        {
            changed = false;

            foreach (var model in models)
            {
                if (invalidTypes.Contains(model.TypeFullName))
                    continue;

                if (model.ParentTypeFullName is not null &&
                    (!modelsByType.ContainsKey(model.ParentTypeFullName) ||
                     invalidTypes.Contains(model.ParentTypeFullName)))
                {
                    invalidTypes.Add(model.TypeFullName);
                    changed = true;
                }
            }
        }

        return models.Where(m => !invalidTypes.Contains(m.TypeFullName)).ToList();
    }

    private static string GenerateRegistrationCode(List<CommandModel> models)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using System.CommandLine;");
        sb.AppendLine();
        sb.AppendLine("namespace CeruleanToolkit.Cli.Commands;");
        sb.AppendLine();
        sb.AppendLine("public static class CommandRegistration");
        sb.AppendLine("{");
        sb.AppendLine("    public static IServiceCollection AddAllCommands(this IServiceCollection services)");
        sb.AppendLine("    {");

        var childrenByParent = models
            .Where(m => m.ParentTypeFullName is not null)
            .GroupBy(m => m.ParentTypeFullName!, StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(m => m.Order).ThenBy(m => m.TypeFullName, StringComparer.Ordinal).ToList(),
                StringComparer.Ordinal);

        var roots = models
            .Where(m => m.ParentTypeFullName is null)
            .OrderBy(m => m.Order)
            .ThenBy(m => m.TypeFullName, StringComparer.Ordinal)
            .ToList();

        // 按命令树顺序注册，生成结果更易读
        var orderedModels = new List<CommandModel>();

        void Collect(CommandModel model)
        {
            orderedModels.Add(model);

            if (!childrenByParent.TryGetValue(model.TypeFullName, out var children))
                return;

            foreach (var child in children)
                Collect(child);
        }

        foreach (var root in roots)
            Collect(root);

        foreach (var model in orderedModels)
        {
            if (model.PlatformSymbol is not null)
                sb.AppendLine($"#if {model.PlatformSymbol}");

            sb.AppendLine($"        services.AddTransient<{model.TypeFullName}>();");

            if (model.PlatformSymbol is not null)
                sb.AppendLine("#endif");
        }

        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    public static RootCommand BuildRootCommand(this IServiceProvider services, string description)");
        sb.AppendLine("    {");
        sb.AppendLine("        var root = new RootCommand(description);");

        int variableIndex = 0;

        foreach (var root in roots)
        {
            if (root.PlatformSymbol is not null)
                sb.AppendLine($"#if {root.PlatformSymbol}");

            string rootVariable = AppendCommand(
                sb,
                root,
                "        ",
                root.PlatformSymbol,
                childrenByParent,
                ref variableIndex);

            sb.AppendLine($"        root.Subcommands.Add({rootVariable});");

            if (root.PlatformSymbol is not null)
                sb.AppendLine("#endif");
        }

        sb.AppendLine("        return root;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string AppendCommand(
        StringBuilder sb,
        CommandModel model,
        string indent,
        string? effectivePlatform,
        Dictionary<string, List<CommandModel>> childrenByParent,
        ref int variableIndex)
    {
        string variable = $"cmd{variableIndex++}";
        sb.AppendLine($"{indent}var {variable} = services.GetRequiredService<{model.TypeFullName}>();");

        if (!childrenByParent.TryGetValue(model.TypeFullName, out var children))
            return variable;

        foreach (var child in children)
        {
            bool wrap = child.PlatformSymbol is not null && child.PlatformSymbol != effectivePlatform;

            if (wrap)
                sb.AppendLine($"#if {child.PlatformSymbol}");

            string? childEffective = child.PlatformSymbol ?? effectivePlatform;
            string childVariable = AppendCommand(
                sb,
                child,
                indent,
                childEffective,
                childrenByParent,
                ref variableIndex);

            sb.AppendLine($"{indent}{variable}.Subcommands.Add({childVariable});");

            if (wrap)
                sb.AppendLine("#endif");
        }

        return variable;
    }

    private static AttributeData? FindAttribute(INamedTypeSymbol type, string metadataName)
    {
        return type.GetAttributes().FirstOrDefault(attribute =>
            attribute.AttributeClass?.ToDisplayString() == metadataName);
    }

    private static bool InheritsFromCommand(INamedTypeSymbol type)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == CommandTypeMetadataName)
                return true;
        }

        return false;
    }

    private static int GetOrder(AttributeData attribute)
    {
        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key == "Order" && namedArg.Value.Value is int order)
                return order;
        }

        return 0;
    }

    private static string? ToPlatformSymbol(string platformMember)
    {
        return platformMember switch
        {
            "Windows" => "WINDOWS",
            "Linux" => "LINUX",
            "MacOS" => "MACOS",
            _ => null
        };
    }
}
