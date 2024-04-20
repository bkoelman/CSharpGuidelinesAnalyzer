using System;
using System.Collections.Immutable;
using System.Threading;
using JetBrains.Annotations;
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

    [NotNull]
    private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

    [NotNull]
    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    [NotNull]
    private static readonly SyntaxKind[] LoopStatementKinds =
    [
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement,
        SyntaxKind.ForStatement,
        SyntaxKind.ForEachStatement,
        SyntaxKind.ForEachVariableStatement
    ];

    [NotNull]
    private static readonly Action<SyntaxNodeAnalysisContext> AnalyzeLoopStatementAction = AnalyzeLoopStatement;

    [NotNull]
    private static readonly LoopBodyLocator BodyLocator = new();

    [ItemNotNull]
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize([NotNull] AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeLoopStatementAction, LoopStatementKinds);
    }

    private static void AnalyzeLoopStatement(SyntaxNodeAnalysisContext context)
    {
        StatementSyntax loopBody = BodyLocator.Visit(context.Node);

        if (loopBody != null)
        {
            AnalyzeLoopBody(loopBody, context);
        }
    }

    private static void AnalyzeLoopBody([NotNull] StatementSyntax loopBody, SyntaxNodeAnalysisContext context)
    {
        var walker = new LoopLocationWalker(context.CancellationToken);
        walker.Visit(loopBody);

        if (walker.LoopStatementLocation != null)
        {
            var diagnostic = Diagnostic.Create(Rule, walker.LoopStatementLocation);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private sealed class LoopBodyLocator : CSharpSyntaxVisitor<StatementSyntax>
    {
        [NotNull]
        public override StatementSyntax VisitWhileStatement([NotNull] WhileStatementSyntax node)
        {
            return node.Statement;
        }

        [NotNull]
        public override StatementSyntax VisitDoStatement([NotNull] DoStatementSyntax node)
        {
            return node.Statement;
        }

        [CanBeNull]
        public override StatementSyntax VisitForStatement([NotNull] ForStatementSyntax node)
        {
            return node.Statement;
        }

        [CanBeNull]
        public override StatementSyntax VisitForEachStatement([NotNull] ForEachStatementSyntax node)
        {
            return node.Statement;
        }

        [CanBeNull]
        public override StatementSyntax VisitForEachVariableStatement([NotNull] ForEachVariableStatementSyntax node)
        {
            return node.Statement;
        }
    }

    private sealed class LoopLocationWalker : CSharpSyntaxWalker
    {
        private CancellationToken cancellationToken;

        [CanBeNull]
        public Location LoopStatementLocation { get; private set; }

        public LoopLocationWalker(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public override void Visit([NotNull] SyntaxNode node)
        {
            cancellationToken.ThrowIfCancellationRequested();

            base.Visit(node);
        }

        public override void VisitWhileStatement([NotNull] WhileStatementSyntax node)
        {
            LoopStatementLocation = node.WhileKeyword.GetLocation();
        }

        public override void VisitDoStatement([NotNull] DoStatementSyntax node)
        {
            LoopStatementLocation = node.DoKeyword.GetLocation();
        }

        public override void VisitForStatement([NotNull] ForStatementSyntax node)
        {
            LoopStatementLocation = node.ForKeyword.GetLocation();
        }

        public override void VisitForEachStatement([NotNull] ForEachStatementSyntax node)
        {
            LoopStatementLocation = node.ForEachKeyword.GetLocation();
        }

        public override void VisitForEachVariableStatement([NotNull] ForEachVariableStatementSyntax node)
        {
            LoopStatementLocation = node.ForEachKeyword.GetLocation();
        }

        public override void VisitLocalFunctionStatement([NotNull] LocalFunctionStatementSyntax node)
        {
        }

        public override void VisitSimpleLambdaExpression([NotNull] SimpleLambdaExpressionSyntax node)
        {
        }

        public override void VisitParenthesizedLambdaExpression([NotNull] ParenthesizedLambdaExpressionSyntax node)
        {
        }

        public override void VisitAnonymousMethodExpression([NotNull] AnonymousMethodExpressionSyntax node)
        {
        }
    }
}