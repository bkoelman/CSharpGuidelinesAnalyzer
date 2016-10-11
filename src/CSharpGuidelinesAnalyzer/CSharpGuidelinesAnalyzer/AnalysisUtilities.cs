using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        [NotNull]
        [ItemNotNull]
        public static List<string> ExtractWords([NotNull] string identifierName)
        {
            var words = new List<string>();

            var builder = new StringBuilder();
            foreach (char ch in identifierName)
            {
                if (char.IsUpper(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) ||
                    char.IsDigit(ch) || char.IsSymbol(ch))
                {
                    FlushBuilder(words, builder);

                    if (!char.IsUpper(ch))
                    {
                        continue;
                    }
                }

                builder.Append(ch);
            }

            FlushBuilder(words, builder);

            return words;
        }

        private static void FlushBuilder([NotNull][ItemNotNull] List<string> words, [NotNull] StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                words.Add(builder.ToString());
                builder.Clear();
            }
        }
    }
}
