using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotChangeLoopVariableAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Loop variable should not be written to in loop body";
    private const string MessageFormat = "Loop variable '{0}' should not be written to in loop body";
    private const string Description = "Don't change a loop variable inside a for loop.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1530";

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
    }

    private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
    {
        var forStatementSyntax = (ForStatementSyntax)context.Node;

        if (forStatementSyntax.Declaration != null)
        {
            foreach (VariableDeclaratorSyntax variableDeclaratorSyntax in forStatementSyntax.Declaration.Variables)
            {
                AnalyzeLoopVariable(context, variableDeclaratorSyntax, forStatementSyntax.Statement);
            }
        }
    }

    private static void AnalyzeLoopVariable(SyntaxNodeAnalysisContext context, [NotNull] VariableDeclaratorSyntax variableDeclaratorSyntax,
        [NotNull] StatementSyntax statementSyntax)
    {
        ISymbol variableSymbol = context.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax);

        if (variableSymbol != null)
        {
            DataFlowAnalysis dataFlowAnalysis = context.SemanticModel.SafeAnalyzeDataFlow(statementSyntax);

            if (dataFlowAnalysis != null)
            {
                if (dataFlowAnalysis.WrittenInside.Contains(variableSymbol))
                {
                    Location location = variableDeclaratorSyntax.Identifier.GetLocation();

                    var diagnostic = Diagnostic.Create(Rule, location, variableSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
