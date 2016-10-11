using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace CSharpGuidelinesAnalyzer.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypesShouldHaveASinglePurposeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1000";

        private const string Title = "Type contains the word 'and'";
        private const string MessageFormat = "Type {0} contains the word 'and'.";
        private const string Description = "A class or interface should have a single purpose.";
        private const string Category = "Class Design";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        }

        private void AnalyzeType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol)context.Symbol;

            if (IdentifierNameContainsAnyOf(type.Name, "And", "and"))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0], type.Name));
            }
        }

        private static bool IdentifierNameContainsAnyOf([NotNull] string identiferName, 
            [NotNull][ItemNotNull] params string[] wordsToFind)
        {
            var wordsInText = ExtractWords(identiferName);

            foreach (var wordToFind in wordsToFind)
            {
                if (wordsInText.Contains(wordToFind))
                {
                    return true;
                }
            }
            
            return false;
        }

        [NotNull]
        [ItemNotNull]
        private static List<string> ExtractWords([NotNull] string identifierName)
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