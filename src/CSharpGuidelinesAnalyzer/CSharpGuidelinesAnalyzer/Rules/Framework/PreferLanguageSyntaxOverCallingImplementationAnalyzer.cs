using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer.Rules.Framework;

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

    private static readonly AnalyzerCategory Category = AnalyzerCategory.Framework;

    private static readonly DiagnosticDescriptor NullableHasValueRule = new(DiagnosticId, Title, NullableHasValueMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    private static readonly DiagnosticDescriptor NullableComparisonRule = new(DiagnosticId, Title, NullableComparisonMessageFormat, Category.DisplayName,
        DiagnosticSeverity.Warning, true, Description, Category.GetHelpLinkUri(DiagnosticId));

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(NullableHasValueRule, NullableComparisonRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(RegisterCompilationStart);
    }

    private static void RegisterCompilationStart(CompilationStartAnalysisContext startContext)
    {
        var scanner = new NullCheckScanner(startContext.Compilation);

        startContext.SafeRegisterOperationAction(context => AnalyzePropertyReference(context, scanner), OperationKind.PropertyReference);
        startContext.SafeRegisterOperationAction(context => AnalyzeBinaryOperator(context, scanner), OperationKind.BinaryOperator);
    }

    private static void AnalyzePropertyReference(OperationAnalysisContext context, NullCheckScanner scanner)
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

    private static void AnalyzeBinaryOperator(OperationAnalysisContext context, NullCheckScanner scanner)
    {
        var binaryOperator = (IBinaryOperation)context.Operation;

        if (DoReportForNullableComparison(binaryOperator, scanner))
        {
            Location location = binaryOperator.Syntax.GetLocation();

            var diagnostic = Diagnostic.Create(NullableComparisonRule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool DoReportForNullableComparison(IBinaryOperation binaryOperator, NullCheckScanner scanner)
    {
        if (binaryOperator.OperatorKind == BinaryOperatorKind.ConditionalAnd)
        {
            IOperation? leftTarget = TryGetTargetInNotNullCheck(binaryOperator.LeftOperand, scanner);

            if (leftTarget != null && leftTarget is not IInvocationOperation)
            {
                if (DoReportForMatchingRightOperandInNullableComparison(binaryOperator.RightOperand, scanner, leftTarget))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool DoReportForMatchingRightOperandInNullableComparison(IOperation rightOperand, NullCheckScanner scanner, IOperation leftTarget)
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

    private static IOperation? TryGetTargetInNotNullCheck(IOperation operation, NullCheckScanner scanner)
    {
        IOperation targetOperation = SkipNotOperators(operation);

        var visitor = new NullCheckVisitor(scanner);
        visitor.Visit(targetOperation);

        return visitor.ScanResult is { Operand: NullCheckOperand.IsNotNull } ? visitor.ScanResult.Value.Target : null;
    }

    private static IOperation SkipNotOperators(IOperation operation)
    {
        IOperation currentOperation = operation;

        while (currentOperation is IUnaryOperation { OperatorKind: UnaryOperatorKind.Not } unaryOperation)
        {
            currentOperation = unaryOperation.Operand;
        }

        return currentOperation;
    }

    private static bool HaveSameTarget(IOperation leftOperation, IOperation rightOperation, NullCheckScanner scanner)
    {
        IOperation innerRightOperation = SkipNullableValueProperty(rightOperation, scanner.NullableHasValueProperty);

        return OperationEqualityComparer.Default.Equals(leftOperation, innerRightOperation);
    }

    private static IOperation SkipNullableValueProperty(IOperation operation, IPropertySymbol? nullableHasValueProperty)
    {
        if (nullableHasValueProperty != null && operation.SkipTypeConversions() is IPropertyReferenceOperation propertyReference)
        {
            if (propertyReference.Property.OriginalDefinition.IsEqualTo(nullableHasValueProperty))
            {
                return propertyReference.Instance!;
            }
        }

        return operation;
    }

    private static bool IsEqualityComparisonWithNullableType(IBinaryOperation binaryOperator)
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

    private static bool IsNullableValueType(IOperation operation)
    {
        return operation.Type?.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    private sealed class NullCheckVisitor : ExplicitOperationVisitor
    {
        private readonly NullCheckScanner scanner;

        public NullCheckScanResult? ScanResult { get; private set; }

        public NullCheckVisitor(NullCheckScanner scanner)
        {
            ArgumentNullException.ThrowIfNull(scanner);
            this.scanner = scanner;
        }

        public override void VisitPropertyReference(IPropertyReferenceOperation operation)
        {
            NullCheckScanResult? scanResult = scanner.ScanPropertyReference(operation);
            SetScanResult(scanResult);

            base.VisitPropertyReference(operation);
        }

        public override void VisitInvocation(IInvocationOperation operation)
        {
            NullCheckScanResult? scanResult = scanner.ScanInvocation(operation);
            SetScanResult(scanResult);

            base.VisitInvocation(operation);
        }

        public override void VisitIsPattern(IIsPatternOperation operation)
        {
            NullCheckScanResult? scanResult = scanner.ScanIsPattern(operation);
            SetScanResult(scanResult);

            base.VisitIsPattern(operation);
        }

        public override void VisitBinaryOperator(IBinaryOperation operation)
        {
            NullCheckScanResult? scanResult = scanner.ScanBinaryOperator(operation);
            SetScanResult(scanResult);

            base.VisitBinaryOperator(operation);
        }

        private void SetScanResult(NullCheckScanResult? scanResult)
        {
            if (scanResult != null)
            {
                ScanResult = scanResult;
            }
        }
    }
}
