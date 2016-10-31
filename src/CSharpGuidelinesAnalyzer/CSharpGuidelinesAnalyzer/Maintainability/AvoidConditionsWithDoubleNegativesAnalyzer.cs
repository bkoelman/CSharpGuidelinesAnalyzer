using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidConditionsWithDoubleNegativesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1502";

        private const string Title = "Logical not operator is applied on a member which has a negation in its name.";

        private const string MessageFormat =
            "Logical not operator is applied on {0} '{1}', which has a negation in its name.";

        private const string Description = "Avoid conditions with double negatives.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> NegatingWords = new[] { "No", "Not" }.ToImmutableArray();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (AnalysisUtilities.SupportsOperations(startContext.Compilation))
                {
                    startContext.RegisterOperationAction(AnalyzeUnaryOperator, OperationKind.UnaryOperatorExpression);
                }
            });
        }

        private void AnalyzeUnaryOperator(OperationAnalysisContext context)
        {
            var unaryOperator = (IUnaryOperatorExpression) context.Operation;
            if (unaryOperator.UnaryOperationKind == UnaryOperationKind.BooleanLogicalNot)
            {
                IdentifierInfo identifierInfo = AnalysisUtilities.TryGetIdentifierInfo(unaryOperator.Operand);
                if (identifierInfo != null)
                {
                    if (AnalysisUtilities.GetFirstWordInSetFromIdentifier(identifierInfo.Name, NegatingWords, true) != null)
                    {
                        string kind = identifierInfo.Kind.ToLowerInvariant();
                        context.ReportDiagnostic(Diagnostic.Create(Rule, unaryOperator.Syntax.GetLocation(), kind,
                            identifierInfo.Name));
                    }
                }
            }
        }
    }
}