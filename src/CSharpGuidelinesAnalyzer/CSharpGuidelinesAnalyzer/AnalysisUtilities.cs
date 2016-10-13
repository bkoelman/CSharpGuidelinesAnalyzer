using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

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
                if (char.IsUpper(ch) || char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || char.IsDigit(ch) ||
                    char.IsSymbol(ch))
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

        private static void FlushBuilder([NotNull] [ItemNotNull] List<string> words, [NotNull] StringBuilder builder)
        {
            if (builder.Length > 0)
            {
                words.Add(builder.ToString());
                builder.Clear();
            }
        }

        public static bool HidesBaseMember([NotNull] ISymbol member, CancellationToken cancellationToken)
        {
            SyntaxNode syntax = member.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken);

            var method = syntax as MethodDeclarationSyntax;
            var propertyEventIndexer = syntax as BasePropertyDeclarationSyntax;

            EventFieldDeclarationSyntax eventField = member is IEventSymbol
                ? syntax.FirstAncestorOrSelf<EventFieldDeclarationSyntax>() as EventFieldDeclarationSyntax
                : null;

            return ContainsNewModifier(method?.Modifiers ?? propertyEventIndexer?.Modifiers ?? eventField?.Modifiers);
        }    

        private static bool ContainsNewModifier([CanBeNull] SyntaxTokenList? modifiers)
        {
            return modifiers != null && modifiers.Value.Any(m => m.Kind() == SyntaxKind.NewKeyword);
        }

        public static bool IsPropertyOrEventAccessor([CanBeNull] ISymbol symbol)
        {
            var method = symbol as IMethodSymbol;
            switch (method?.MethodKind)
            {
                case MethodKind.PropertyGet:
                case MethodKind.PropertySet:
                case MethodKind.EventAdd:
                case MethodKind.EventRemove:
                    return true;
                default: return false;
            }
        }
    }
}