using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer;

internal sealed class NullCheckScanner
{
    private readonly KnownSymbols knownSymbols;

    public IPropertySymbol? NullableHasValueProperty => knownSymbols.NullableValueProperty;

    public NullCheckScanner(Compilation compilation)
    {
        Guard.NotNull(compilation, nameof(compilation));

        knownSymbols = new KnownSymbols(compilation);
    }

    public NullCheckScanResult? ScanPropertyReference(IPropertyReferenceOperation propertyReference)
    {
        Guard.NotNull(propertyReference, nameof(propertyReference));

        if (propertyReference.Property.OriginalDefinition.IsEqualTo(knownSymbols.NullableHasValueProperty) && IsNullableValueType(propertyReference.Instance))
        {
            NullCheckOperand nullCheckOperand = GetParentNullCheckOperand(propertyReference);
            NullCheckOperand toggledOperand = nullCheckOperand.Toggle();

            return new NullCheckScanResult(propertyReference.Instance, NullCheckMethod.NullableHasValueMethod, toggledOperand);
        }

        return null;
    }

    public NullCheckScanResult? ScanInvocation(IInvocationOperation invocation)
    {
        Guard.NotNull(invocation, nameof(invocation));

        if (invocation.TargetMethod != null)
        {
            if (invocation.Arguments.Length == 1)
            {
                return AnalyzeSingleArgumentInvocation(invocation);
            }

            if (invocation.Arguments.Length == 2)
            {
                return AnalyzeDoubleArgumentInvocation(invocation);
            }
        }

        return null;
    }

    private NullCheckScanResult? AnalyzeSingleArgumentInvocation(IInvocationOperation invocation)
    {
        if (invocation.Instance != null)
        {
            bool isNullableEquals = invocation.TargetMethod.OriginalDefinition.IsEqualTo(knownSymbols.NullableEqualsMethod);

            if (isNullableEquals)
            {
                NullCheckOperand nullCheckOperand = GetParentNullCheckOperand(invocation);
                var info = new ArgumentsInfo(invocation.Instance, invocation.Arguments[0].Value, NullCheckMethod.NullableEqualsMethod, nullCheckOperand);

                return AnalyzeArguments(info);
            }
        }

        return null;
    }

    private NullCheckScanResult? AnalyzeDoubleArgumentInvocation(IInvocationOperation invocation)
    {
        NullCheckMethod? nullCheckMethod = TryGetNullCheckForDoubleArgumentInvocation(invocation);

        if (nullCheckMethod != null)
        {
            IArgumentOperation leftArgument = invocation.Arguments[0];
            IArgumentOperation rightArgument = invocation.Arguments[1];

            NullCheckOperand nullCheckOperand = GetParentNullCheckOperand(invocation);
            var info = new ArgumentsInfo(leftArgument.Value, rightArgument.Value, nullCheckMethod.Value, nullCheckOperand);

            return AnalyzeArguments(info);
        }

        return null;
    }

    private NullCheckMethod? TryGetNullCheckForDoubleArgumentInvocation(IInvocationOperation invocation)
    {
        if (IsObjectReferenceEquals(invocation))
        {
            return NullCheckMethod.StaticObjectReferenceEqualsMethod;
        }

        if (IsStaticObjectEquals(invocation))
        {
            return NullCheckMethod.StaticObjectEqualsMethod;
        }

        if (IsEqualityComparerEquals(invocation))
        {
            return NullCheckMethod.EqualityComparerEqualsMethod;
        }

        return null;
    }

    private bool IsObjectReferenceEquals(IInvocationOperation invocation)
    {
        return invocation.TargetMethod.IsEqualTo(knownSymbols.StaticObjectReferenceEqualsMethod);
    }

    private bool IsStaticObjectEquals(IInvocationOperation invocation)
    {
        return invocation.TargetMethod.IsEqualTo(knownSymbols.StaticObjectEqualsMethod);
    }

    private bool IsEqualityComparerEquals(IInvocationOperation invocation)
    {
        return invocation.TargetMethod.OriginalDefinition.IsEqualTo(knownSymbols.EqualityComparerEqualsMethod);
    }

    public NullCheckScanResult? ScanIsPattern(IIsPatternOperation isPattern)
    {
        Guard.NotNull(isPattern, nameof(isPattern));

        if (isPattern.Pattern is IConstantPatternOperation constantPattern)
        {
            if (IsConstantNullOrDefault(constantPattern.Value) && IsNullableValueType(isPattern.Value))
            {
                NullCheckOperand nullCheckOperand = GetParentNullCheckOperand(isPattern);

                return new NullCheckScanResult(isPattern.Value, NullCheckMethod.IsPattern, nullCheckOperand);
            }
        }

        return null;
    }

    public NullCheckScanResult? ScanBinaryOperator(IBinaryOperation binaryOperator)
    {
        Guard.NotNull(binaryOperator, nameof(binaryOperator));

        NullCheckOperand? operatorNullCheckOperand = TryGetBinaryOperatorNullCheckOperand(binaryOperator);

        if (operatorNullCheckOperand == null)
        {
            return null;
        }

        NullCheckOperand parentNullCheckOperand = GetParentNullCheckOperand(binaryOperator);
        NullCheckOperand nullCheckOperandCombined = parentNullCheckOperand.CombineWith(operatorNullCheckOperand.Value);
        var info = new ArgumentsInfo(binaryOperator.LeftOperand, binaryOperator.RightOperand, NullCheckMethod.EqualityOperator, nullCheckOperandCombined);

        return AnalyzeArguments(info);
    }

    private NullCheckOperand? TryGetBinaryOperatorNullCheckOperand(IBinaryOperation binaryOperator)
    {
        if (binaryOperator.OperatorKind == BinaryOperatorKind.Equals)
        {
            return NullCheckOperand.IsNull;
        }

        if (binaryOperator.OperatorKind == BinaryOperatorKind.NotEquals)
        {
            return NullCheckOperand.IsNotNull;
        }

        return null;
    }

    private NullCheckOperand GetParentNullCheckOperand(IOperation operation)
    {
        var operand = NullCheckOperand.IsNull;

        IOperation currentOperation = operation.Parent;

        while (currentOperation is IUnaryOperation { OperatorKind: UnaryOperatorKind.Not })
        {
            operand = operand.Toggle();
            currentOperation = currentOperation.Parent;
        }

        return operand;
    }

    private static bool IsConstantNullOrDefault(IOperation operation)
    {
        if (operation.ConstantValue is { HasValue: true, Value: null })
        {
            return true;
        }

        return operation is IDefaultValueOperation;
    }

    private NullCheckScanResult? AnalyzeArguments(ArgumentsInfo info)
    {
        IOperation leftArgumentNoConversion = info.LeftArgument.SkipTypeConversions();
        IOperation rightArgumentNoConversion = info.RightArgument.SkipTypeConversions();

        ArgumentsInfo arguments = info.WithArguments(leftArgumentNoConversion, rightArgumentNoConversion);
        return InnerAnalyzeArguments(arguments);
    }

    private NullCheckScanResult? InnerAnalyzeArguments(ArgumentsInfo info)
    {
        if (info.IsRightArgumentNull)
        {
            if (!info.IsLeftArgumentNull && IsNullableValueType(info.LeftArgument))
            {
                return new NullCheckScanResult(info.LeftArgument, info.NullCheckMethod, info.NullCheckOperand);
            }
        }
        else
        {
            if (info.IsLeftArgumentNull && IsNullableValueType(info.RightArgument))
            {
                return new NullCheckScanResult(info.RightArgument, info.NullCheckMethod, info.NullCheckOperand);
            }
        }

        return null;
    }

    private bool IsNullableValueType(IOperation? operation)
    {
        return operation != null && operation.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }

    private sealed class KnownSymbols
    {
        public IPropertySymbol? NullableHasValueProperty { get; }

        public IPropertySymbol? NullableValueProperty { get; }

        public IMethodSymbol? StaticObjectReferenceEqualsMethod { get; }

        public IMethodSymbol? StaticObjectEqualsMethod { get; }

        public IMethodSymbol? NullableEqualsMethod { get; }

        public IMethodSymbol? EqualityComparerEqualsMethod { get; }

        public KnownSymbols(Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            NullableHasValueProperty = ResolveNullableHasValueProperty(compilation);
            NullableValueProperty = ResolveNullableValueProperty(compilation);
            StaticObjectReferenceEqualsMethod = ResolveObjectReferenceEquals(compilation);
            StaticObjectEqualsMethod = ResolveStaticObjectEquals(compilation);
            NullableEqualsMethod = ResolveNullableEquals(compilation);
            EqualityComparerEqualsMethod = ResolveEqualityComparerEquals(compilation);
        }

        private static IPropertySymbol? ResolveNullableHasValueProperty(Compilation compilation)
        {
            INamedTypeSymbol? nullableType = KnownTypes.SystemNullableT(compilation);
            return nullableType?.GetMembers("HasValue").OfType<IPropertySymbol>().FirstOrDefault();
        }

        private static IPropertySymbol? ResolveNullableValueProperty(Compilation compilation)
        {
            INamedTypeSymbol? nullableType = KnownTypes.SystemNullableT(compilation);
            return nullableType?.GetMembers("Value").OfType<IPropertySymbol>().FirstOrDefault();
        }

        private IMethodSymbol? ResolveObjectReferenceEquals(Compilation compilation)
        {
            INamedTypeSymbol? objectType = KnownTypes.SystemObject(compilation);
            return objectType?.GetMembers("ReferenceEquals").OfType<IMethodSymbol>().FirstOrDefault();
        }

        private IMethodSymbol? ResolveStaticObjectEquals(Compilation compilation)
        {
            INamedTypeSymbol? objectType = KnownTypes.SystemObject(compilation);
            return objectType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault(method => method.IsStatic);
        }

        private IMethodSymbol? ResolveNullableEquals(Compilation compilation)
        {
            INamedTypeSymbol? nullableType = KnownTypes.SystemNullableT(compilation);
            return nullableType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault();
        }

        private IMethodSymbol? ResolveEqualityComparerEquals(Compilation compilation)
        {
            INamedTypeSymbol? equalityComparerType = KnownTypes.SystemCollectionsGenericEqualityComparerT(compilation);
            return equalityComparerType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault();
        }
    }

    private readonly struct ArgumentsInfo(
        IOperation leftArgument, IOperation rightArgument, NullCheckMethod nullCheckMethod, NullCheckOperand nullCheckOperand)
    {
        public IOperation LeftArgument { get; } = leftArgument;

        public bool IsLeftArgumentNull => IsConstantNullOrDefault(LeftArgument);

        public IOperation RightArgument { get; } = rightArgument;

        public bool IsRightArgumentNull => IsConstantNullOrDefault(RightArgument);

        public NullCheckMethod NullCheckMethod { get; } = nullCheckMethod;

        public NullCheckOperand NullCheckOperand { get; } = nullCheckOperand;

        public ArgumentsInfo WithArguments(IOperation leftArgument, IOperation rightArgument)
        {
            return new ArgumentsInfo(leftArgument, rightArgument, NullCheckMethod, NullCheckOperand);
        }
    }
}
