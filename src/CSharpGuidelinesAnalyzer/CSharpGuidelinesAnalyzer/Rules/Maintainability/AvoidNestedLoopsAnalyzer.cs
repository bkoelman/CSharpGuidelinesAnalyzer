using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidNestedLoopsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1532";

        private const string Title = "Loop statement contains nested loop";
        private const string MessageFormat = "Loop statement contains nested loop.";
        private const string Description = "Avoid nested loops.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterConditionalOperationAction(c => c.SkipInvalid(AnalyzeLoopStatement),
                OperationKind.LoopStatement);
        }

        private void AnalyzeLoopStatement(OperationAnalysisContext context)
        {
            var loopStatement = (ILoopStatement) context.Operation;

            var walker = new LoopBodyWalker();
            walker.Visit(loopStatement.Body);

            context.CancellationToken.ThrowIfCancellationRequested();

            if (walker.LoopStatementLocation != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, walker.LoopStatementLocation));
            }
        }

        private sealed class LoopBodyWalker : OperationWalker
        {
            [CanBeNull]
            public Location LoopStatementLocation { get; private set; }

            public override void VisitWhileUntilLoopStatement([NotNull] IWhileUntilLoopStatement operation)
            {
                LoopStatementLocation = operation.GetLocationForKeyword();
            }

            public override void VisitForLoopStatement([NotNull] IForLoopStatement operation)
            {
                LoopStatementLocation = operation.GetLocationForKeyword();
            }

            public override void VisitForEachLoopStatement([NotNull] IForEachLoopStatement operation)
            {
                LoopStatementLocation = operation.GetLocationForKeyword();
            }
        }
    }
}