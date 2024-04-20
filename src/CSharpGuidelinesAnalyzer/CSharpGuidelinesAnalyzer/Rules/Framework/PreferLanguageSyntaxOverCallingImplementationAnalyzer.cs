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
        private const string Title = "Prefer language syntax over explicit calls to underlying implementations";
        private const string NullableHasValueMessageFormat = "Replace call to Nullable<T>.HasValue with null check";
        private const string NullableComparisonMessageFormat = "Remove null check in numeric comparison";
        private const string Description = "Prefer language syntax over explicit calls to underlying implementations.";

        public const string DiagnosticId = AnalyzerCategory.RulePrefix + "2202";

        private static readonly ImmutableArray<BinaryOperatorKind> NumericComparisonOperators = new[]
        {
            BinaryOperatorKind.Equals,
            BinaryOperatorKind.LessThan,
            BinaryOperatorKind.LessThanOrEqual,
            BinaryOperatorKind.GreaterThan,
            BinaryOperatorKind.GreaterThanOrEqual
        }.ToImmutableArray();

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

        [NotNull]
        private static readonly DiagnosticDescriptor NullableHasValueRule = new DiagnosticDescriptor(DiagnosticId, Title, NullableHasValueMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly DiagnosticDescriptor NullableComparisonRule = new DiagnosticDescriptor(DiagnosticId, Title, NullableComparisonMessageFormat,
            Category.DisplayName, DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [NotNull]
        private static readonly Action<CompilationStartAnalysisContext> RegisterCompilationStartAction = RegisterCompilationStart;

        [NotNull]
        private static readonly Action<OperationAnalysisContext, NullCheckScanner> AnalyzePropertyReferenceAction = (context, scanner) =>
            context.SkipInvalid(_ => AnalyzePropertyReference(context, scanner));

        [NotNull]
        private static readonly Action<OperationAnalysisContext, NullCheckScanner> AnalyzeBinaryOperatorAction = (context, scanner) =>
            context.SkipInvalid(_ => AnalyzeBinaryOperator(context, scanner));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NullableHasValueRule, NullableComparisonRule);

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(RegisterCompilationStartAction);
        }

        private static void RegisterCompilationStart([NotNull] CompilationStartAnalysisContext startContext)
        {
            var scanner = new NullCheckScanner(startContext.Compilation);

            startContext.RegisterOperationAction(context => AnalyzePropertyReferenceAction(context, scanner), OperationKind.PropertyReference);
            startContext.RegisterOperationAction(context => AnalyzeBinaryOperatorAction(context, scanner), OperationKind.BinaryOperator);
        }

        private static void AnalyzePropertyReference(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var propertyReference = (IPropertyReferenceOperation)context.Operation;

            NullCheckScanResult? scanResult = scanner.ScanPropertyReference(propertyReference);

            if (scanResult is { Method: NullCheckMethod.NullableHasValueMethod })
            {
                if (propertyReference.Parent is IBinaryOperation binaryOperator && DoReportForNullableComparison(binaryOperator, scanner))
                {
                    return;
                }

                Location location = propertyReference.Syntax.GetLocation();

                var diagnostic = Diagnostic.Create(NullableHasValueRule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void AnalyzeBinaryOperator(OperationAnalysisContext context, [NotNull] NullCheckScanner scanner)
        {
            var binaryOperator = (IBinaryOperation)context.Operation;

            if (DoReportForNullableComparison(binaryOperator, scanner))
            {
                Location location = binaryOperator.Syntax.GetLocation();

                var diagnostic = Diagnostic.Create(NullableComparisonRule, location);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool DoReportForNullableComparison([NotNull] IBinaryOperation binaryOperator, [NotNull] NullCheckScanner scanner)
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

        private static bool DoReportForMatchingRightOperandInNullableComparison([NotNull] IOperation rightOperand, [NotNull] NullCheckScanner scanner,
            [NotNull] IOperation leftTarget)
        {
            if (rightOperand is IBinaryOperation rightOperation && NumericComparisonOperators.Contains(rightOperation.OperatorKind))
            {
                if (HaveSameTarget(leftTarget, rightOperation.LeftOperand, scanner) || HaveSameTarget(leftTarget, rightOperation.RightOperand, scanner))
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
            if (binaryOperation is { OperatorKind: BinaryOperatorKind.And, Syntax: BinaryExpressionSyntax binaryExpressionSyntax })
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

            return visitor.ScanResult is { Operand: NullCheckOperand.IsNotNull } ? visitor.ScanResult.Value.Target : null;
        }

        [NotNull]
        private static IOperation SkipNotOperators([NotNull] IOperation operation)
        {
            IOperation currentOperation = operation;

            while (currentOperation is IUnaryOperation { OperatorKind: UnaryOperatorKind.Not } unaryOperation)
            {
                currentOperation = unaryOperation.Operand;
            }

            return currentOperation;
        }

        private static bool HaveSameTarget([NotNull] IOperation leftOperation, [NotNull] IOperation rightOperation, [NotNull] NullCheckScanner scanner)
        {
            IOperation innerRightOperation = SkipNullableValueProperty(rightOperation, scanner.NullableHasValueProperty);

            return OperationEqualityComparer.Default.Equals(leftOperation, innerRightOperation);
        }

        [NotNull]
        private static IOperation SkipNullableValueProperty([NotNull] IOperation operation, [CanBeNull] IPropertySymbol nullableHasValueProperty)
        {
            if (nullableHasValueProperty != null && operation.SkipTypeConversions() is IPropertyReferenceOperation propertyReference)
            {
                if (propertyReference.Property.OriginalDefinition.IsEqualTo(nullableHasValueProperty))
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
            return operatorKind == BinaryOperatorKind.Equals;
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
