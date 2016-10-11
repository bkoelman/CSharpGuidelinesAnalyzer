using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CSharpGuidelinesAnalyzer
{
    public static class AnalysisUtilities
    {
        public static bool SupportsOperations([NotNull] Compilation compilation)
        {
            IReadOnlyDictionary<string, string> features = compilation.SyntaxTrees.FirstOrDefault()?.Options.Features;
            return features != null && features.ContainsKey("IOperation") && features["IOperation"] == "true";
        }

        public static bool IsNullableBoolean([NotNull] ITypeSymbol typeSymbol)
        {
            if (typeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                var namedTypeSymbol = typeSymbol as INamedTypeSymbol;

                if (namedTypeSymbol?.TypeArguments[0].SpecialType == SpecialType.System_Boolean)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
