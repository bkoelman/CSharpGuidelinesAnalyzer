using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidMembersWithMoreThanSevenStatementsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1500";

        private const string Title = "Member contains more than seven statements";
        private const string MessageFormat = "{0} '{1}' contains {2} statements, which exceeds the maximum of seven statements.";
        private const string Description = "Methods should not exceed 7 statements.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category,
            DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterConditionalOperationBlockAction(c => c.SkipInvalid(AnalyzeCodeBlock));
        }

        private void AnalyzeCodeBlock(OperationBlockAnalysisContext context)
        {
            var statementWalker = new StatementWalker();
            statementWalker.VisitBlocks(context.OperationBlocks);

            context.CancellationToken.ThrowIfCancellationRequested();

            if (statementWalker.StatementCount > 7)
            {
                ReportMember(context, statementWalker.StatementCount);
            }
        }

        private static void ReportMember(OperationBlockAnalysisContext context, int statementCount)
        {
            ISymbol containingMember = context.OwningSymbol.GetContainingMember();

            string memberName = containingMember.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            Location location = containingMember.Locations[0];
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, containingMember.Kind, memberName, statementCount));
        }

        private sealed class StatementWalker : OperationWalker
        {
            public int StatementCount { get; private set; }

            public void VisitBlocks([ItemNotNull] ImmutableArray<IOperation> blocks)
            {
                foreach (IOperation block in blocks)
                {
                    Visit(block);
                }
            }

            public override void VisitBranchStatement([NotNull] IBranchStatement operation)
            {
                StatementCount++;
                base.VisitBranchStatement(operation);
            }

            public override void VisitEmptyStatement([NotNull] IEmptyStatement operation)
            {
                StatementCount++;
                base.VisitEmptyStatement(operation);
            }

            public override void VisitExpressionStatement([NotNull] IExpressionStatement operation)
            {
                StatementCount++;
                base.VisitExpressionStatement(operation);
            }

            public override void VisitLambdaExpression([NotNull] ILambdaExpression operation)
            {
                if (!IsBodyCompilerGenerated(operation))
                {
                    Visit(operation.Body);
                }
            }

            private static bool IsBodyCompilerGenerated([NotNull] ILambdaExpression operation)
            {
                // Workaround for https://github.com/dotnet/roslyn/issues/10214
                var lambdaExpressionSyntax = operation.Syntax as LambdaExpressionSyntax;
                var expressionSyntax = lambdaExpressionSyntax?.Body as ExpressionSyntax;
                return expressionSyntax != null;
            }

            public override void VisitFixedStatement([NotNull] IFixedStatement operation)
            {
                // Note: fixed statements must always occur in combination with a declaration 
                // expression statement. So to allow eight fixed statements, we do not count these.
                base.VisitFixedStatement(operation);
            }

            public override void VisitForEachLoopStatement([NotNull] IForEachLoopStatement operation)
            {
                StatementCount++;
                base.VisitForEachLoopStatement(operation);
            }

            public override void VisitForLoopStatement([NotNull] IForLoopStatement operation)
            {
                StatementCount++;
                Visit(operation.Body);
            }

            public override void VisitIfStatement([NotNull] IIfStatement operation)
            {
                StatementCount++;
                base.VisitIfStatement(operation);
            }

            public override void VisitInvalidStatement([NotNull] IInvalidStatement operation)
            {
                StatementCount++;
                base.VisitInvalidStatement(operation);
            }

            public override void VisitLabelStatement([NotNull] ILabelStatement operation)
            {
                StatementCount++;
                base.VisitLabelStatement(operation);
            }

            public override void VisitLockStatement([NotNull] ILockStatement operation)
            {
                StatementCount++;
                base.VisitLockStatement(operation);
            }

            public override void VisitReturnStatement([NotNull] IReturnStatement operation)
            {
                StatementCount++;
                base.VisitReturnStatement(operation);
            }

            public override void VisitSwitchStatement([NotNull] ISwitchStatement operation)
            {
                StatementCount++;
                base.VisitSwitchStatement(operation);
            }

            public override void VisitThrowStatement([NotNull] IThrowStatement operation)
            {
                StatementCount++;
                base.VisitThrowStatement(operation);
            }

            public override void VisitTryStatement([NotNull] ITryStatement operation)
            {
                StatementCount++;
                base.VisitTryStatement(operation);
            }

            public override void VisitUsingStatement([NotNull] IUsingStatement operation)
            {
                StatementCount++;
                base.VisitUsingStatement(operation);
            }

            public override void VisitVariableDeclarationStatement([NotNull] IVariableDeclarationStatement operation)
            {
                StatementCount++;
                base.VisitVariableDeclarationStatement(operation);
            }

            public override void VisitWhileUntilLoopStatement([NotNull] IWhileUntilLoopStatement operation)
            {
                StatementCount++;
                base.VisitWhileUntilLoopStatement(operation);
            }

            public override void VisitYieldBreakStatement([NotNull] IReturnStatement operation)
            {
                StatementCount++;
                base.VisitYieldBreakStatement(operation);
            }
        }
    }
}
