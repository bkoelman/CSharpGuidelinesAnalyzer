using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IfElseIfStatementsShouldFinishWithElseClauseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1537";

        private const string Title = "if-else-if construct should end with an unconditional else clause";
        private const string MessageFormat = "if-else-if construct should end with an unconditional else clause.";
        private const string Description = "Finish every if-else-if statement with an else-part.";
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

            context.RegisterCompilationStartAction(startContext =>
            {
                if (AnalysisUtilities.SupportsOperations(startContext.Compilation))
                {
                    startContext.RegisterOperationBlockAction(AnalyzeCodeBlock);
                }
            });
        }

        private void AnalyzeCodeBlock(OperationBlockAnalysisContext context)
        {
            var collector = new IfStatementCollector();
            collector.VisitBlocks(context.OperationBlocks);

            while (collector.CollectedStatements.Any())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                AnalyzeIfStatement(collector.CollectedStatements, context);
            }
        }

        private void AnalyzeIfStatement([NotNull] IDictionary<Location, IIfStatement> ifStatementsLeftToAnalyze,
            OperationBlockAnalysisContext context)
        {
            KeyValuePair<Location, IIfStatement> firstEntry = ifStatementsLeftToAnalyze.First();
            ifStatementsLeftToAnalyze.Remove(firstEntry.Key);

            IIfStatement ifStatement = firstEntry.Value;

            if (IsIfElseIf(ifStatement))
            {
                AnalyzeIfElseIfStatement(ifStatement, ifStatementsLeftToAnalyze, context);
            }
        }

        private bool IsIfElseIf([NotNull] IIfStatement ifStatement)
        {
            var ifElseStatement = ifStatement.IfFalseStatement as IIfStatement;
            return ifElseStatement != null;
        }

        private void AnalyzeIfElseIfStatement([NotNull] IIfStatement ifStatement,
            [NotNull] IDictionary<Location, IIfStatement> ifStatementsLeftToAnalyze,
            OperationBlockAnalysisContext context)
        {
            Location topIfKeywordLocation = GetLocation(ifStatement);

            while (true)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                IOperation falseBlock = ifStatement.IfFalseStatement;

                if (falseBlock == null)
                {
                    // no else clause
                    context.ReportDiagnostic(Diagnostic.Create(Rule, topIfKeywordLocation));

                    Remove(ifStatement, ifStatementsLeftToAnalyze);
                    break;
                }

                var ifElseStatement = falseBlock as IIfStatement;
                if (ifElseStatement != null)
                {
                    // else-if
                    Remove(ifElseStatement, ifStatementsLeftToAnalyze);

                    ifStatement = ifElseStatement;
                }
                else
                {
                    // unconditional else
                    break;
                }
            }
        }

        private void Remove([NotNull] IIfStatement ifStatementToRemove,
            [NotNull] IDictionary<Location, IIfStatement> ifStatements)
        {
            Location location = GetLocation(ifStatementToRemove);
            ifStatements.Remove(location);
        }

        [NotNull]
        private static Location GetLocation([NotNull] IIfStatement ifStatement)
        {
            return ((IfStatementSyntax) ifStatement.Syntax).IfKeyword.GetLocation();
        }

        private sealed class IfStatementCollector : OperationWalker
        {
            [NotNull]
            public IDictionary<Location, IIfStatement> CollectedStatements { get; } =
                new SortedDictionary<Location, IIfStatement>(LocationComparer.Default);

            public void VisitBlocks([ItemNotNull] ImmutableArray<IOperation> blocks)
            {
                foreach (IOperation block in blocks)
                {
                    Visit(block);
                }
            }

            public override void VisitIfStatement([NotNull] IIfStatement operation)
            {
                Location location = GetLocation(operation);
                CollectedStatements.Add(location, operation);

                base.VisitIfStatement(operation);
            }

            private sealed class LocationComparer : IComparer<Location>
            {
                [NotNull]
                public static readonly LocationComparer Default = new LocationComparer();

                public int Compare(Location x, Location y)
                {
                    return x.SourceSpan.Start.CompareTo(y.SourceSpan.Start);
                }
            }
        }
    }
}