using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class TypeSymbolExtensions
    {
        public static bool IsBooleanOrNullableBoolean([NotNull] this ITypeSymbol type)
        {
            return type.SpecialType == SpecialType.System_Boolean || IsNullableBoolean(type);
        }

        public static bool IsNullableBoolean([NotNull] this ITypeSymbol type)
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

        public static bool IsNullableEnumeration([NotNull] this ITypeSymbol type)
        {
            Guard.NotNull(type, nameof(type));

            if (type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                var namedTypeSymbol = type as INamedTypeSymbol;
                ITypeSymbol innerType = namedTypeSymbol?.TypeArguments[0];

                if (innerType?.BaseType != null && innerType.BaseType.SpecialType == SpecialType.System_Enum)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ImplementsIEnumerable([NotNull] this ITypeSymbol type)
        {
            Guard.NotNull(type, nameof(type));

            return type.AllInterfaces.Any(IsEnumerableInterface);
        }

        public static bool IsOrImplementsIEnumerable([NotNull] this ITypeSymbol type)
        {
            Guard.NotNull(type, nameof(type));

            return IsEnumerableInterface(type) || type.AllInterfaces.Any(IsEnumerableInterface);
        }

        public static bool IsEnumerableInterface([NotNull] this ITypeSymbol type)
        {
            Guard.NotNull(type, nameof(type));

            return type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
                type.SpecialType == SpecialType.System_Collections_IEnumerable;
        }
    }
}
