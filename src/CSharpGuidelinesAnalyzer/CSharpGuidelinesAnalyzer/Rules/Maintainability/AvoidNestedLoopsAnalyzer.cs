using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidNestedLoopsAnalyzer : DiagnosticAnalyzer
{
    private const string Title = "Loop statement contains nested loop";
    private const string MessageFormat = "Loop statement contains nested loop";
    private const string Description = "Avoid nested loops.";

    public const string DiagnosticId = AnalyzerCategory.RulePrefix + "1532";

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true,
        Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly SyntaxKind[] LoopStatementKinds =
    [
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement,
        SyntaxKind.ForStatement,
        SyntaxKind.ForEachStatement,
        SyntaxKind.ForEachVariableStatement
    ];

    private static readonly LoopBodyLocator BodyLocator = new();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeLoopStatement, LoopStatementKinds);
    }

    private static void AnalyzeLoopStatement(SyntaxNodeAnalysisContext context)
    {
        StatementSyntax? loopBody = BodyLocator.Visit(context.Node);

        if (loopBody != null)
        {
            AnalyzeLoopBody(loopBody, context);
        }
    }

    private static void AnalyzeLoopBody(StatementSyntax loopBody, SyntaxNodeAnalysisContext context)
    {
        var walker = new LoopLocationWalker(context.CancellationToken);
        walker.Visit(loopBody);

        if (walker.LoopStatementLocation != null)
        {
            var diagnostic = Diagnostic.Create(Rule, walker.LoopStatementLocation);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private sealed class LoopBodyLocator : CSharpSyntaxVisitor<StatementSyntax?>
    {
        public override StatementSyntax VisitWhileStatement(WhileStatementSyntax node)
        {
            return node.Statement;
        }

        public override StatementSyntax VisitDoStatement(DoStatementSyntax node)
        {
            return node.Statement;
        }

        public override StatementSyntax? VisitForStatement(ForStatementSyntax node)
        {
            return node.Statement;
        }

        public override StatementSyntax? VisitForEachStatement(ForEachStatementSyntax node)
        {
            return node.Statement;
        }

        public override StatementSyntax? VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            return node.Statement;
        }
    }

    private sealed class LoopLocationWalker(CancellationToken cancellationToken) : CSharpSyntaxWalker
    {
        public Location? LoopStatementLocation { get; private set; }

        public override void Visit(SyntaxNode node)
        {
            cancellationToken.ThrowIfCancellationRequested();

            base.Visit(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            LoopStatementLocation = node.WhileKeyword.GetLocation();
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            LoopStatementLocation = node.DoKeyword.GetLocation();
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            LoopStatementLocation = node.ForKeyword.GetLocation();
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            LoopStatementLocation = node.ForEachKeyword.GetLocation();
        }

        public override void VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            LoopStatementLocation = node.ForEachKeyword.GetLocation();
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
        }
    }
}
