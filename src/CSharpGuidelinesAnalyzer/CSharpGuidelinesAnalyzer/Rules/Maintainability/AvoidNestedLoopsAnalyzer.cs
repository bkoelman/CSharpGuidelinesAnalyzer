using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidNestedLoopsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1532";

        private const string Title = "Loop statement contains nested loop";
        private const string MessageFormat = "Loop statement contains nested loop.";
        private const string Description = "Avoid nested loops.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(c => c.SkipInvalid(AnalyzeLoopStatement), OperationKind.Loop);
        }

        private void AnalyzeLoopStatement(OperationAnalysisContext context)
        {
            var loopStatement = (ILoopOperation)context.Operation;

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

            public override void VisitWhileLoop([NotNull] IWhileLoopOperation operation)
            {
                LoopStatementLocation = operation.GetLocationForKeyword();
            }

            public override void VisitForLoop([NotNull] IForLoopOperation operation)
            {
                LoopStatementLocation = operation.GetLocationForKeyword();
            }

            public override void VisitForEachLoop([NotNull] IForEachLoopOperation operation)
            {
                LoopStatementLocation = operation.GetLocationForKeyword();
            }
        }
    }
}
