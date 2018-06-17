using System;
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
    public sealed class ClauseInSwitchStatementShouldHaveBlockAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1535";

        private const string Title = "Missing block in case or default clause of switch statement";
        private const string MessageFormat = "Missing block in case or default clause of switch statement.";
        private const string Description = "Always add a block after the keywords if, else, do, while, for, foreach and case.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Maintainability;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [NotNull]
        private static readonly Action<OperationAnalysisContext> AnalyzeSwitchCaseAction =
            context => context.SkipInvalid(AnalyzeSwitchCase);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterOperationAction(AnalyzeSwitchCaseAction, OperationKind.SwitchCase);
        }

        private static void AnalyzeSwitchCase(OperationAnalysisContext context)
        {
            var switchCase = (ISwitchCaseOperation)context.Operation;

            if (switchCase.Body.Length > 0)
            {
                if (!(switchCase.Body[0] is IBlockOperation))
                {
                    ReportAtLastClause(switchCase, context);
                }
            }
        }

        private static void ReportAtLastClause([NotNull] ISwitchCaseOperation switchCase, OperationAnalysisContext context)
        {
            ICaseClauseOperation lastClause = switchCase.Clauses.Last();

            Location location = lastClause.TryGetLocationForKeyword();
            if (location != null)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }
        }
    }
}
