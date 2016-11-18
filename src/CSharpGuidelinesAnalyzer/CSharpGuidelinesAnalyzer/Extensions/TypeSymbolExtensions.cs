using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class TypeSymbolExtensions
    {
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

        public static bool IsNullableEnum([NotNull] this ITypeSymbol type)
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
    }
}