using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotHideInheritedMembersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1010";

        private const string Title = "Member hides inherited member";
        private const string MessageFormat = "'{0}' hides inherited member.";
        private const string Description = "Don't hide inherited members with the new keyword.";
        private const string Category = "Class Design";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> SymbolKinds =
            new[] { SymbolKind.Property, SymbolKind.Method, SymbolKind.Event }.ToImmutableArray();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(AnalyzeMember, SymbolKinds);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (AnalysisUtilities.IsPropertyOrEventAccessor(context.Symbol))
            {
                return;
            }

            if (!context.Symbol.IsOverride && AnalysisUtilities.HidesBaseMember(context.Symbol, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0],
                    context.Symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }
    }
}