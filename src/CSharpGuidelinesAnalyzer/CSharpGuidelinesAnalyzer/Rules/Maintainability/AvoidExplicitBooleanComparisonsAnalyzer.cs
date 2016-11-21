using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Rules.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidExplicitBooleanComparisonsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1525";

        private const string Title = "Expression contains explicit comparison to 'true' or 'false'";
        private const string MessageFormat = "Expression contains explicit comparison to '{0}'.";
        private const string Description = "Don't make explicit comparisons to true or false.";
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

            context.RegisterConditionalOperationAction(c => c.SkipInvalid(AnalyzeExpression),
                OperationKind.BinaryOperatorExpression);
        }

        private void AnalyzeExpression(OperationAnalysisContext context)
        {
            var expression = (IBinaryOperatorExpression) context.Operation;

            if (expression.BinaryOperationKind == BinaryOperationKind.BooleanEquals ||
                expression.BinaryOperationKind == BinaryOperationKind.BooleanNotEquals)
            {
                if (IsLiteralExpression(expression.LeftOperand))
                {
                    AnalyzeOperand(expression.LeftOperand, context);
                }

                if (IsLiteralExpression(expression.RightOperand))
                {
                    AnalyzeOperand(expression.RightOperand, context);
                }
            }
        }

        private static bool IsLiteralExpression([NotNull] IOperation operand)
        {
            return operand.Kind == OperationKind.LiteralExpression;
        }

        private void AnalyzeOperand([NotNull] IOperation operand, OperationAnalysisContext context)
        {
            if (IsTrueOrFalseConstant(operand))
            {
                ReportDiagnostic(context, operand, operand.ConstantValue.Value.ToString().ToLowerInvariant());
            }
        }

        private bool IsTrueOrFalseConstant([NotNull] IOperation operand)
        {
            if (operand.ConstantValue.HasValue)
            {
                string value = operand.ConstantValue.Value.ToString();
                if (value == true.ToString() || value == false.ToString())
                {
                    return true;
                }
            }

            return false;
        }

        private static void ReportDiagnostic(OperationAnalysisContext context, [NotNull] IOperation operation,
            [NotNull] string name)
        {
            Diagnostic diagnostic = Diagnostic.Create(Rule, operation.Syntax.GetLocation(), name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}