using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Layout;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseRegionsAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Region should be removed";
    private const string MessageFormat = "Region should be removed";
    private const string Description = "Do not use #region.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2407";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Layout;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.RegionDirectiveTrivia);
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var trivia = (RegionDirectiveTriviaSyntax)context.Node;

        Location location = trivia.GetLocation();

        var diagnostic = Diagnostic.Create(Rule, location);
        context.ReportDiagnostic(diagnostic);
    }
}
