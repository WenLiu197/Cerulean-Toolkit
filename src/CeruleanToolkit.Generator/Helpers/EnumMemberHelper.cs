using Microsoft.CodeAnalysis;

namespace CeruleanToolkit.Generator.Helpers;

internal static class EnumMemberHelper
{
    /// <summary>
    /// 从特性的命名参数中取出枚举成员名。
    /// 如果调用方没有显式指定该参数，则返回 <paramref name="defaultMemberName"/>。
    /// <para>
    /// 这样生成器就不依赖枚举的底层数值，调整枚举成员顺序不会静默改变注册行为。
    /// </para>
    /// </summary>
    public static string GetMemberName(
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
}
