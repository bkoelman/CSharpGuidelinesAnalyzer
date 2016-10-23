using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CaseClausesInSwitchStatementsShouldHaveBracesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1535";

        private const string Title = "Missing block in case statement.";
        private const string MessageFormat = "Missing block in case statement.";

        private const string Description =
            "Always add a block after keywords such as if, else, while, for, foreach and case.";

        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category, DiagnosticId));

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
                    startContext.RegisterOperationAction(AnalyzeSwitchCase, OperationKind.SwitchCase);
                }
            });
        }

        private void AnalyzeSwitchCase(OperationAnalysisContext context)
        {
            var switchCase = (ISwitchCase) context.Operation;

            if (switchCase.IsInvalid)
            {
                return;
            }

            if (switchCase.Body.Length > 0)
            {
                var block = switchCase.Body[0] as IBlockStatement;
                if (block == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, switchCase.Syntax.GetLocation()));
                }
            }
        }
    }
}