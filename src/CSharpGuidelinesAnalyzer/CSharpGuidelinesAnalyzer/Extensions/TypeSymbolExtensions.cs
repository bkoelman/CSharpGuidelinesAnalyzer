using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary />
internal static class TypeSymbolExtensions
{
    public static bool IsBooleanOrNullableBoolean(this ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_Boolean || IsNullableBoolean(type);
    }

    public static ITypeSymbol UnwrapNullableValueType(this ITypeSymbol type)
    {
        if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            var namedType = (INamedTypeSymbol)type;

            if (namedType.TypeArguments[0] is INamedTypeSymbol innerType)
            {
                return innerType;
            }
        }

        return type;
    }

    public static bool IsNullableBoolean(this ITypeSymbol type)
    {
        Guard.NotNull(type, nameof(type));

        if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            var namedTypeSymbol = type as INamedTypeSymbol;

            if (namedTypeSymbol?.TypeArguments[0].SpecialType == SpecialType.System_Boolean)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsNullableEnumeration(this ITypeSymbol type)
    {
        Guard.NotNull(type, nameof(type));

        if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            var namedTypeSymbol = type as INamedTypeSymbol;
            ITypeSymbol? innerType = namedTypeSymbol?.TypeArguments[0];

            if (innerType?.BaseType is { SpecialType: SpecialType.System_Enum })
            {
                return true;
            }
        }

        return false;
    }

    public static bool ImplementsIEnumerable(this ITypeSymbol type)
    {
        Guard.NotNull(type, nameof(type));

        return type.AllInterfaces.Any(IsEnumerableInterface);
    }

    public static bool IsOrImplementsIEnumerable(this ITypeSymbol type)
    {
        Guard.NotNull(type, nameof(type));

        return IsEnumerableInterface(type) || type.AllInterfaces.Any(IsEnumerableInterface);
    }

    public static bool IsEnumerableInterface(this ITypeSymbol type)
    {
        Guard.NotNull(type, nameof(type));

        return type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
            type.SpecialType == SpecialType.System_Collections_IEnumerable;
    }
}
