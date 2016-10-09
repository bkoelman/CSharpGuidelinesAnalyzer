using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace CSharpGuidelinesAnalyzer.Maintainability
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidExplicitBooleanComparisonsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV1525";

        private const string Title = "Expression contains explicit comparison to true or false";
        private const string MessageFormat = "Expression contains explicit comparison to {0}.";
        private const string Description = "Don't make explicit comparisons to true or false.";
        private const string Category = "Maintainability";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                if (SupportsOperations(startContext.Compilation))
                {
                    startContext.RegisterOperationAction(AnalyzeExpression, OperationKind.BinaryOperatorExpression);
                }
            });
        }

        private bool SupportsOperations([NotNull] Compilation compilation)
        {
            IReadOnlyDictionary<string, string> features = compilation.SyntaxTrees.FirstOrDefault()?.Options.Features;
            return features != null && features.ContainsKey("IOperation") && features["IOperation"] == "true";
        }

        private void AnalyzeExpression(OperationAnalysisContext context)
        {
            var expression = (IBinaryOperatorExpression) context.Operation;

            if (expression.BinaryOperationKind == BinaryOperationKind.BooleanEquals ||
                expression.BinaryOperationKind == BinaryOperationKind.BooleanNotEquals)
            {
                if (expression.LeftOperand.Kind == OperationKind.LiteralExpression)
                {
                    AnalyzeOperand(expression.LeftOperand, context);
                }

                if (expression.RightOperand.Kind == OperationKind.LiteralExpression)
                {
                    AnalyzeOperand(expression.RightOperand, context);
                }
            }
        }

        private void AnalyzeOperand([NotNull] IOperation operand, OperationAnalysisContext context)
        {
            if (IsTrueOrFalseConstant(operand))
            {
                ReportDiagnostic(context, operand, operand.ConstantValue.Value.ToString());
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