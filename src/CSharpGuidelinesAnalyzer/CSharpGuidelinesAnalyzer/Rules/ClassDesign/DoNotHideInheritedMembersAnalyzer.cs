using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.ClassDesign
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotHideInheritedMembersAnalyzer : GuidelineAnalyzer
    {
        public const string DiagnosticId = "AV1010";

        private const string Title = "Member hides inherited member";
        private const string MessageFormat = "'{0}' hides inherited member.";
        private const string Description = "Don't suppress compiler warnings using the new keyword.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.ClassDesign;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.Name, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private static readonly ImmutableArray<SymbolKind> MemberSymbolKinds =
            ImmutableArray.Create(SymbolKind.Property, SymbolKind.Method, SymbolKind.Event);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(c => c.SkipEmptyName(AnalyzeMember), MemberSymbolKinds);
        }

        private void AnalyzeMember(SymbolAnalysisContext context)
        {
            if (context.Symbol.IsPropertyOrEventAccessor())
            {
                return;
            }

            if (!context.Symbol.IsOverride && context.Symbol.HidesBaseMember(context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Symbol.Locations[0],
                    context.Symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)));
            }
        }
    }
}
