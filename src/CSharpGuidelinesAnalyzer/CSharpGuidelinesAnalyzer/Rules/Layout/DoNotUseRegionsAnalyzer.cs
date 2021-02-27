using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Layout
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotUseRegionsAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "Region should be removed";
        private const string MessageFormat = "Region should be removed.";
        private const string Description = "Do not use #region.";

        public const string DiagnosticId = "AV2407";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Layout;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category.DisplayName,
            DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeSyntaxNodeAction = AnalyzeSyntaxNode;

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNodeAction, SyntaxKind.RegionDirectiveTrivia);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var trivia = (RegionDirectiveTriviaSyntax)context.Node;

            Location location = trivia.GetLocation();
            context.ReportDiagnostic(Diagnostic.Create(Rule, location));
        }
    }
}
