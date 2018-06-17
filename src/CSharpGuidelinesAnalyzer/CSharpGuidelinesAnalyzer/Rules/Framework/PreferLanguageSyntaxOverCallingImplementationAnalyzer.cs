using System;
using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Framework
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PreferLanguageSyntaxOverCallingImplementationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2202";

        private const string Title = "Prefer language syntax over explicit calls to underlying implementations";

        private const string NullableHasValueMessageFormat = "Replace call to Nullable<T>.HasValue with null check.";
        private const string NullableComparisonMessageFormat = "Remove null check in numeric comparison.";

        private const string Description = "Prefer language syntax over explicit calls to underlying implementations.";

        private static readonly ImmutableArray<BinaryOperatorKind> NumericComparisonOperators = new[]
        {
            BinaryOperatorKind.Equals,
            BinaryOperatorKind.NotEquals,
            BinaryOperatorKind.LessThan,
            BinaryOperatorKind.LessThanOrEqual,
            BinaryOperatorKind.GreaterThan,
            BinaryOperatorKind.GreaterThanOrEqual
        }.ToImmutableArray();

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor NullableHasValueRule = new DiagnosticDescriptor(DiagnosticId, Title,
            NullableHasValueMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor NullableComparisonRule = new DiagnosticDescriptor(DiagnosticId, Title,
            NullableComparisonMessageFormat, Category.DisplayName, DiagnosticSeverity.Warning, true, Description,
            Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(NullableHasValueRule, NullableComparisonRule);

        [NotNull]
        private static readonly Action<OperationAnalysisContext, NullCheckScanner> AnalyzePropertyReferenceAction =
            (context, scanner) => context.SkipInvalid(_ => AnalyzePropertyReference(context, scanner));

        [NotNull]
        private static readonly Action<OperationAnalysisContext, NullCheckScanner> AnalyzeBinaryOperatorAction =
            (context, scanner) => context.SkipInvalid(_ => AnalyzeBinaryOperator(context, scanner));

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var scanner = new NullCheckScanner(startContext.Compilation);

                startContext.RegisterOperationAction(c => AnalyzePropertyReferenceAction(c, scanner),
                    OperationKind.PropertyReference);

                startContext.RegisterOperationAction(c => AnalyzeBinaryOperatorAction(c, scanner), OperationKind.BinaryOperator);
            });
        }

        private static void AnalyzePropertyReference(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var propertyReference = (IPropertyReferenceOperation)context.Operation;

            NullCheckScanResult? scanResult = scanner.ScanPropertyReference(propertyReference);
            if (scanResult != null && scanResult.Value.Method == NullCheckMethod.NullableHasValueMethod)
            {
                if (propertyReference.Parent is IBinaryOperation binaryOperator &&
                    DoReportForNullableComparison(binaryOperator, scanner))
                {
                    return;
                }

                context.ReportDiagnostic(Diagnostic.Create(NullableHasValueRule, propertyReference.Syntax.GetLocation()));
            }
        }

        private static void AnalyzeBinaryOperator(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var binaryOperator = (IBinaryOperation)context.Operation;

            if (DoReportForNullableComparison(binaryOperator, scanner))
            {
                context.ReportDiagnostic(Diagnostic.Create(NullableComparisonRule, binaryOperator.Syntax.GetLocation()));
            }
        }

        private static bool DoReportForNullableComparison([NotNull] IBinaryOperation binaryOperator,
            [NotNull] NullCheckScanner scanner)
        {
            if (IsLogicalAnd(binaryOperator))
            {
                IOperation leftTarget = TryGetTargetInNotNullCheck(binaryOperator.LeftOperand, scanner);
                if (leftTarget != null && !(leftTarget is IInvocationOperation))
                {
                    if (DoReportForMatchingRightOperandInNullableComparison(binaryOperator.RightOperand, scanner, leftTarget))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool DoReportForMatchingRightOperandInNullableComparison([NotNull] IOperation rightOperand,
            [NotNull] NullCheckScanner scanner, [NotNull] IOperation leftTarget)
        {
            if (rightOperand is IBinaryOperation rightOperation &&
                NumericComparisonOperators.Contains(rightOperation.OperatorKind))
            {
                if (HaveSameTarget(leftTarget, rightOperation.LeftOperand, scanner) ||
                    HaveSameTarget(leftTarget, rightOperation.RightOperand, scanner))
                {
                    if (!IsEqualityComparisonWithNullableType(rightOperation))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsLogicalAnd([NotNull] IBinaryOperation binaryOperation)
        {
            if (binaryOperation.OperatorKind == BinaryOperatorKind.ConditionalAnd)
            {
                return true;
            }

            // Bug workaround for https://github.com/dotnet/roslyn/issues/27209
            if (binaryOperation.OperatorKind == BinaryOperatorKind.And &&
                binaryOperation.Syntax is BinaryExpressionSyntax binaryExpressionSyntax)
            {
                if (binaryExpressionSyntax.OperatorToken.IsKind(SyntaxKind.AmpersandAmpersandToken))
                {
                    return true;
                }
            }

            return false;
        }

        [CanBeNull]
        private static IOperation TryGetTargetInNotNullCheck([NotNull] IOperation operation, [NotNull] NullCheckScanner scanner)
        {
            IOperation targetOperation = SkipNotOperators(operation);

            var visitor = new NullCheckVisitor(scanner);
            visitor.Visit(targetOperation);

            return visitor.ScanResult != null && visitor.ScanResult.Value.Operand == NullCheckOperand.IsNotNull
                ? visitor.ScanResult.Value.Target
                : null;
        }

        [NotNull]
        private static IOperation SkipNotOperators([NotNull] IOperation operation)
        {
            IOperation currentOperation = operation;
            while (currentOperation is IUnaryOperation unaryOperation && unaryOperation.OperatorKind == UnaryOperatorKind.Not)
            {
                currentOperation = unaryOperation.Operand;
            }

            return currentOperation;
        }

        private static bool HaveSameTarget([NotNull] IOperation leftOperation, [NotNull] IOperation rightOperation,
            [NotNull] NullCheckScanner scanner)
        {
            IOperation innerRightOperation = SkipNullableValueProperty(rightOperation, scanner.NullableHasValueProperty);

            return OperationEqualityComparer.Default.Equals(leftOperation, innerRightOperation);
        }

        [NotNull]
        private static IOperation SkipNullableValueProperty([NotNull] IOperation operation,
            [CanBeNull] IPropertySymbol nullableHasValueProperty)
        {
            if (nullableHasValueProperty != null && operation is IPropertyReferenceOperation propertyReference)
            {
                if (propertyReference.Property.OriginalDefinition.Equals(nullableHasValueProperty))
                {
                    return propertyReference.Instance;
                }
            }

            return operation;
        }

        private static bool IsEqualityComparisonWithNullableType([NotNull] IBinaryOperation binaryOperator)
        {
            if (IsEqualityComparison(binaryOperator.OperatorKind))
            {
                if (IsNullableValueType(binaryOperator.LeftOperand) && IsNullableValueType(binaryOperator.RightOperand))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsEqualityComparison(BinaryOperatorKind operatorKind)
        {
            return operatorKind == BinaryOperatorKind.Equals || operatorKind == BinaryOperatorKind.NotEquals;
        }

        private static bool IsNullableValueType([NotNull] IOperation operation)
        {
            return operation.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }

        private sealed class NullCheckVisitor : ExplicitOperationVisitor
        {
            [NotNull]
            private readonly NullCheckScanner scanner;

            [CanBeNull]
            public NullCheckScanResult? ScanResult { get; private set; }

            public NullCheckVisitor([NotNull] NullCheckScanner scanner)
            {
                Guard.NotNull(scanner, nameof(scanner));
                this.scanner = scanner;
            }

            public override void VisitPropertyReference([NotNull] IPropertyReferenceOperation operation)
            {
                NullCheckScanResult? scanResult = scanner.ScanPropertyReference(operation);
                SetScanResult(scanResult);

                base.VisitPropertyReference(operation);
            }

            public override void VisitInvocation([NotNull] IInvocationOperation operation)
            {
                NullCheckScanResult? scanResult = scanner.ScanInvocation(operation);
                SetScanResult(scanResult);

                base.VisitInvocation(operation);
            }

            public override void VisitIsPattern([NotNull] IIsPatternOperation operation)
            {
                NullCheckScanResult? scanResult = scanner.ScanIsPattern(operation);
                SetScanResult(scanResult);

                base.VisitIsPattern(operation);
            }

            public override void VisitBinaryOperator([NotNull] IBinaryOperation operation)
            {
                NullCheckScanResult? scanResult = scanner.ScanBinaryOperator(operation);
                SetScanResult(scanResult);

                base.VisitBinaryOperator(operation);
            }

            private void SetScanResult([CanBeNull] NullCheckScanResult? scanResult)
            {
                if (scanResult != null)
                {
                    ScanResult = scanResult;
                }
            }
        }
    }
}
