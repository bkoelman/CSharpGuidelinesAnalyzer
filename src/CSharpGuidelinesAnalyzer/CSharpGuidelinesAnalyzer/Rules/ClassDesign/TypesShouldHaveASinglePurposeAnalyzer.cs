using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TypesShouldHaveASinglePurposeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1000";

        private const string Title = "Type contains the word 'and'";
        private const string MessageFormat = "Type '{0}' contains the word 'and'.";
        private const string Description = "A class or interface should have a single purpose.";
        private const string Category = "Class Design";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> WordsBlacklist = ImmutableArray.Create("And");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeType), SymbolKind.NamedType);
        }

        private void AnalyzeType(SymbolAnalysisContext context)
        {
            var type = (INamedTypeSymbol) context.Symbol;

            if (type.Name.GetFirstWordInSetFromIdentifier(WordsBlacklist, TextMatchMode.AllowLowerCaseMatch) != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, type.Locations[0], type.Name));
            }
        }
    }
}