using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
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
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> NegatingWords = ImmutableArray.Create("No", "Not");

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterConditionalOperationAction(c => c.SkipInvalid(AnalyzeUnaryOperator),
                OperationKind.UnaryOperatorExpression);
        }

        private void AnalyzeUnaryOperator(OperationAnalysisContext context)
        {
            var unaryOperator = (IUnaryOperatorExpression) context.Operation;

            if (IsOperatorNot(unaryOperator))
            {
                IdentifierInfo identifierInfo = unaryOperator.Operand.TryGetIdentifierInfo();
                if (identifierInfo != null && ContainsNegatingWord(identifierInfo))
                {
                    string kind = identifierInfo.Kind.ToLowerInvariant();
                    context.ReportDiagnostic(Diagnostic.Create(Rule, unaryOperator.Syntax.GetLocation(), kind,
                        identifierInfo.Name.ShortName));
                }
            }
        }

        private static bool IsOperatorNot([NotNull] IUnaryOperatorExpression unaryOperator)
        {
            return unaryOperator.UnaryOperationKind == UnaryOperationKind.BooleanLogicalNot;
        }

        private static bool ContainsNegatingWord([NotNull] IdentifierInfo info)
        {
            string name = info.Name.ShortName;
            return name.GetFirstWordInSetFromIdentifier(NegatingWords, TextMatchMode.AllowLowerCaseMatch) != null;
        }
    }
}