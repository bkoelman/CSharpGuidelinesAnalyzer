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

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(startContext =>
            {
                var scanner = new NullCheckScanner(startContext.Compilation);

                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzePropertyReference(c, scanner)),
                    OperationKind.PropertyReference);

                startContext.RegisterOperationAction(c => c.SkipInvalid(_ => AnalyzeBinaryOperator(c, scanner)),
                    OperationKind.BinaryOperator);
            });
        }

        private void AnalyzePropertyReference(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var propertyReference = (IPropertyReferenceOperation)context.Operation;

            NullCheckScanResult? scanResult = scanner.ScanPropertyReference(propertyReference);
            if (scanResult != null && scanResult.Value.Kind == NullCheckKind.NullableHasValueMethod)
            {
                context.ReportDiagnostic(Diagnostic.Create(NullableHasValueRule, propertyReference.Syntax.GetLocation()));
            }
        }

        private void AnalyzeBinaryOperator(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var binaryOperator = (IBinaryOperation)context.Operation;

            if (!IsLogicalAnd(binaryOperator))
            {
                return;
            }

            IOperation leftTarget = TryGetTargetInNotNullCheck(binaryOperator.LeftOperand, scanner);
            if (leftTarget != null && !(leftTarget is IInvocationOperation))
            {
                if (binaryOperator.RightOperand is IBinaryOperation rightOperation &&
                    NumericComparisonOperators.Contains(rightOperation.OperatorKind))
                {
                    if (HaveSameTarget(leftTarget, rightOperation.LeftOperand, scanner) ||
                        HaveSameTarget(leftTarget, rightOperation.RightOperand, scanner))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(NullableComparisonRule, binaryOperator.Syntax.GetLocation()));
                    }
                }
            }
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
        private IOperation TryGetTargetInNotNullCheck([NotNull] IOperation operation, [NotNull] NullCheckScanner scanner)
        {
            IOperation targetOperation = SkipNotOperators(operation);

            var visitor = new NullCheckVisitor(scanner);
            visitor.Visit(targetOperation);

            return visitor.ScanResult != null && visitor.ScanResult.Value.IsInverted ? visitor.ScanResult.Value.Target : null;
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

        private bool HaveSameTarget([NotNull] IOperation leftOperation, [NotNull] IOperation rightOperation,
            [NotNull] NullCheckScanner scanner)
        {
            IOperation innerRightOperation = SkipNullableValueProperty(rightOperation, scanner.NullableHasValueProperty);

            return OperationEqualityComparer.Default.Equals(leftOperation, innerRightOperation);
        }

        [NotNull]
        private IOperation SkipNullableValueProperty([NotNull] IOperation operation,
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

        private sealed class NullCheckVisitor : OperationVisitor
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
