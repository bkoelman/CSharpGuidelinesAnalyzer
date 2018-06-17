using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class IfElseIfConstructShouldFinishWithElseClauseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1537";

        private const string Title = "If-else-if construct should end with an unconditional else clause";
        private const string MessageFormat = "If-else-if construct should end with an unconditional else clause.";
        private const string Description = "Finish every if-else-if statement with an else clause.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [NotNull]
        private static readonly Action<OperationBlockAnalysisContext> AnalyzeCodeBlockAction =
            context => context.SkipInvalid(AnalyzeCodeBlock);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationBlockAction(AnalyzeCodeBlockAction);
        }

        private static void AnalyzeCodeBlock(OperationBlockAnalysisContext context)
        {
            var collector = new IfStatementCollector();
            collector.VisitBlocks(context.OperationBlocks);

            var analyzer = new IfStatementAnalyzer(collector.CollectedIfStatements, context);
            analyzer.Analyze();
        }

        private sealed class IfStatementCollector : ExplicitOperationWalker
        {
            [NotNull]
            public IDictionary<Location, IConditionalOperation> CollectedIfStatements { get; } =
                new SortedDictionary<Location, IConditionalOperation>(LocationComparer.Default);

            public void VisitBlocks([ItemNotNull] ImmutableArray<IOperation> blocks)
            {
                foreach (IOperation block in blocks)
                {
                    Visit(block);
                }
            }

            public override void VisitConditional([NotNull] IConditionalOperation operation)
            {
                if (operation.IsStatement())
                {
                    Location location = operation.TryGetLocationForKeyword();
                    if (location != null)
                    {
                        CollectedIfStatements.Add(location, operation);
                    }
                }

                base.VisitConditional(operation);
            }

            private sealed class LocationComparer : IComparer<Location>
            {
                [NotNull]
                public static readonly LocationComparer Default = new LocationComparer();

                public int Compare(Location x, Location y)
                {
                    if (ReferenceEquals(x, y))
                    {
                        return 0;
                    }

                    return x == null ? -1 : y == null ? 1 : x.SourceSpan.Start.CompareTo(y.SourceSpan.Start);
                }
            }
        }

        private sealed class IfStatementAnalyzer
        {
            [NotNull]
            private readonly IDictionary<Location, IConditionalOperation> ifStatementsLeftToAnalyze;

            private OperationBlockAnalysisContext context;

            public IfStatementAnalyzer([NotNull] IDictionary<Location, IConditionalOperation> ifStatementsToAnalyze,
                OperationBlockAnalysisContext context)
            {
                Guard.NotNull(ifStatementsToAnalyze, nameof(ifStatementsToAnalyze));

                ifStatementsLeftToAnalyze = ifStatementsToAnalyze;
                this.context = context;
            }

            public void Analyze()
            {
                while (ifStatementsLeftToAnalyze.Any())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    AnalyzeTopIfStatement();
                }
            }

            private void AnalyzeTopIfStatement()
            {
                IConditionalOperation ifStatement = ConsumeNextIfStatement();

                if (IsIfElseIfConstruct(ifStatement))
                {
                    var analyzer = new IfElseIfConstructAnalyzer(this, ifStatement);
                    analyzer.Analyze();
                }
            }

            [NotNull]
            private IConditionalOperation ConsumeNextIfStatement()
            {
                KeyValuePair<Location, IConditionalOperation> entry = ifStatementsLeftToAnalyze.First();
                ifStatementsLeftToAnalyze.Remove(entry.Key);

                return entry.Value;
            }

            private bool IsIfElseIfConstruct([NotNull] IConditionalOperation ifStatement)
            {
                return ifStatement.WhenFalse is IConditionalOperation;
            }

            private sealed class IfElseIfConstructAnalyzer
            {
                [NotNull]
                private readonly IfStatementAnalyzer owner;

                [CanBeNull]
                private readonly Location topIfKeywordLocation;

                [NotNull]
                private IConditionalOperation ifStatement;

                public IfElseIfConstructAnalyzer([NotNull] IfStatementAnalyzer owner,
                    [NotNull] IConditionalOperation topIfStatement)
                {
                    this.owner = owner;

                    topIfKeywordLocation = topIfStatement.TryGetLocationForKeyword();
                    ifStatement = topIfStatement;
                }

                public void Analyze()
                {
                    if (topIfKeywordLocation != null)
                    {
                        while (true)
                        {
                            owner.context.CancellationToken.ThrowIfCancellationRequested();

                            IOperation falseBlock = ifStatement.WhenFalse;
                            if (!AnalyzeFalseBlock(falseBlock))
                            {
                                break;
                            }
                        }
                    }
                }

                private bool AnalyzeFalseBlock([CanBeNull] IOperation falseBlock)
                {
                    if (falseBlock == null)
                    {
                        return HandleMissingElseClause();
                    }

                    return !(falseBlock is IConditionalOperation ifElseStatement)
                        ? HandleUnconditionalElse()
                        : HandleElseIf(ifElseStatement);
                }

                private bool HandleMissingElseClause()
                {
                    owner.context.ReportDiagnostic(Diagnostic.Create(Rule, topIfKeywordLocation));
                    Remove(ifStatement, owner.ifStatementsLeftToAnalyze);
                    return false;
                }

                private bool HandleUnconditionalElse()
                {
                    return false;
                }

                private bool HandleElseIf([NotNull] IConditionalOperation ifElseStatement)
                {
                    Remove(ifElseStatement, owner.ifStatementsLeftToAnalyze);
                    ifStatement = ifElseStatement;
                    return true;
                }

                private void Remove([NotNull] IConditionalOperation ifStatementToRemove,
                    [NotNull] IDictionary<Location, IConditionalOperation> ifStatements)
                {
                    Location location = ifStatementToRemove.TryGetLocationForKeyword();
                    ifStatements.Remove(location);
                }
            }
        }
    }
}
