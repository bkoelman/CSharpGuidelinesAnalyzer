using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class NullCheckScanner
    {
        [NotNull]
        private readonly KnownSymbols knownSymbols;

        public NullCheckScanner([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            knownSymbols = new KnownSymbols(compilation);
        }

        [CanBeNull]
        public NullCheckScanResult? ScanPropertyReference([NotNull] IPropertyReferenceOperation propertyReference)
        {
            Guard.NotNull(propertyReference, nameof(propertyReference));

            if (propertyReference.Property.OriginalDefinition.Equals(knownSymbols.NullableHasValueProperty) &&
                IsNullableValueType(propertyReference.Instance))
            {
                return new NullCheckScanResult(propertyReference.Instance, NullCheckKind.NullableHasValueMethod, false);
            }

            return null;
        }

        [CanBeNull]
        public NullCheckScanResult? ScanInvocation([NotNull] IInvocationOperation invocation)
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

        [CanBeNull]
        private NullCheckScanResult? AnalyzeSingleArgumentInvocation([NotNull] IInvocationOperation invocation)
        {
            if (invocation.Instance != null)
            {
                bool isNullableEquals = invocation.TargetMethod.OriginalDefinition.Equals(knownSymbols.NullableEqualsMethod);
                if (isNullableEquals)
                {
                    bool isInverted = IsInverted(invocation);

                    return AnalyzeArguments(invocation.Instance, invocation.Arguments[0].Value,
                        NullCheckKind.NullableEqualsMethod, isInverted);
                }
            }

            return null;
        }

        [CanBeNull]
        private NullCheckScanResult? AnalyzeDoubleArgumentInvocation([NotNull] IInvocationOperation invocation)
        {
            NullCheckKind? nullCheckKind = TryGetNullCheckForDoubleArgumentInvocation(invocation);
            if (nullCheckKind != null)
            {
                IArgumentOperation leftArgument = invocation.Arguments[0];
                IArgumentOperation rightArgument = invocation.Arguments[1];

                bool isInverted = IsInverted(invocation);

                return AnalyzeArguments(leftArgument.Value, rightArgument.Value, nullCheckKind.Value, isInverted);
            }

            return null;
        }

        [CanBeNull]
        private NullCheckKind? TryGetNullCheckForDoubleArgumentInvocation([NotNull] IInvocationOperation invocation)
        {
            bool isObjectReferenceEquals = invocation.TargetMethod.Equals(knownSymbols.StaticObjectReferenceEqualsMethod);
            if (isObjectReferenceEquals)
            {
                return NullCheckKind.StaticObjectReferenceEqualsMethod;
            }

            bool isStaticObjectEquals = invocation.TargetMethod.Equals(knownSymbols.StaticObjectEqualsMethod);
            if (isStaticObjectEquals)
            {
                return NullCheckKind.StaticObjectEqualsMethod;
            }

            bool isEqualityComparerEquals =
                invocation.TargetMethod.OriginalDefinition.Equals(knownSymbols.EqualityComparerEqualsMethod);
            if (isEqualityComparerEquals)
            {
                return NullCheckKind.EqualityComparerEqualsMethod;
            }

            return null;
        }

        [CanBeNull]
        public NullCheckScanResult? ScanIsPattern([NotNull] IIsPatternOperation isPattern)
        {
            Guard.NotNull(isPattern, nameof(isPattern));

            if (isPattern.Pattern is IConstantPatternOperation constantPattern)
            {
                if (IsConstantNullOrDefault(constantPattern.Value) && IsNullableValueType(isPattern.Value))
                {
                    bool isInverted = IsInverted(isPattern);

                    return new NullCheckScanResult(isPattern.Value, NullCheckKind.IsPattern, isInverted);
                }
            }

            return null;
        }

        [CanBeNull]
        public NullCheckScanResult? ScanBinaryOperator([NotNull] IBinaryOperation binaryOperator)
        {
            Guard.NotNull(binaryOperator, nameof(binaryOperator));

            bool isInverted;
            if (binaryOperator.OperatorKind == BinaryOperatorKind.Equals)
            {
                isInverted = false;
            }
            else if (binaryOperator.OperatorKind == BinaryOperatorKind.NotEquals)
            {
                isInverted = true;
            }
            else
            {
                return null;
            }

            return AnalyzeArguments(binaryOperator.LeftOperand, binaryOperator.RightOperand, NullCheckKind.EqualityOperator,
                isInverted);
        }

        private bool IsInverted([NotNull] IOperation operation)
        {
            bool isInverted = false;

            IOperation currentOperation = operation.Parent;
            while (currentOperation is IUnaryOperation unaryOperation && unaryOperation.OperatorKind == UnaryOperatorKind.Not)
            {
                isInverted = !isInverted;
                currentOperation = currentOperation.Parent;
            }

            return isInverted;
        }

        [CanBeNull]
        private NullCheckScanResult? AnalyzeArguments([NotNull] IOperation leftArgument, [NotNull] IOperation rightArgument,
            NullCheckKind nullCheckKind, bool isInverted)
        {
            bool leftIsNull = IsConstantNullOrDefault(leftArgument);
            bool rightIsNull = IsConstantNullOrDefault(rightArgument);

            if (rightIsNull)
            {
                if (!leftIsNull && IsNullableValueType(leftArgument))
                {
                    return new NullCheckScanResult(leftArgument, nullCheckKind, isInverted);
                }
            }
            else
            {
                if (leftIsNull && IsNullableValueType(rightArgument))
                {
                    return new NullCheckScanResult(rightArgument, nullCheckKind, isInverted);
                }
            }

            return null;
        }

        private bool IsConstantNullOrDefault([NotNull] IOperation operation)
        {
            IOperation source = operation is IConversionOperation conversion ? conversion.Operand : operation;

            if (source.ConstantValue.HasValue && source.ConstantValue.Value == null)
            {
                return true;
            }

            return source is IDefaultValueOperation;
        }

        private bool IsNullableValueType([NotNull] IOperation operation)
        {
            IOperation source = operation is IConversionOperation conversion ? conversion.Operand : operation;

            return source.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }

        private sealed class KnownSymbols
        {
            [CanBeNull]
            public IPropertySymbol NullableHasValueProperty { get; }

            [CanBeNull]
            public IMethodSymbol StaticObjectReferenceEqualsMethod { get; }

            [CanBeNull]
            public IMethodSymbol StaticObjectEqualsMethod { get; }

            [CanBeNull]
            public IMethodSymbol NullableEqualsMethod { get; }

            [CanBeNull]
            public IMethodSymbol EqualityComparerEqualsMethod { get; }

            public KnownSymbols([NotNull] Compilation compilation)
            {
                Guard.NotNull(compilation, nameof(compilation));

                NullableHasValueProperty = ResolveNullableHasValueProperty(compilation);
                StaticObjectReferenceEqualsMethod = ResolveObjectReferenceEquals(compilation);
                StaticObjectEqualsMethod = ResolveStaticObjectEquals(compilation);
                NullableEqualsMethod = ResolveNullableEquals(compilation);
                EqualityComparerEqualsMethod = ResolveEqualityComparerEquals(compilation);
            }

            [CanBeNull]
            private static IPropertySymbol ResolveNullableHasValueProperty([NotNull] Compilation compilation)
            {
                INamedTypeSymbol nullableType = KnownTypes.SystemNullableT(compilation);
                return nullableType?.GetMembers("HasValue").OfType<IPropertySymbol>().FirstOrDefault();
            }

            [CanBeNull]
            private IMethodSymbol ResolveObjectReferenceEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol objectType = KnownTypes.SystemObject(compilation);
                return objectType?.GetMembers("ReferenceEquals").OfType<IMethodSymbol>().FirstOrDefault();
            }

            [CanBeNull]
            private IMethodSymbol ResolveStaticObjectEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol objectType = KnownTypes.SystemObject(compilation);
                return objectType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault(m => m.IsStatic);
            }

            [CanBeNull]
            private IMethodSymbol ResolveNullableEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol nullableType = KnownTypes.SystemNullableT(compilation);
                return nullableType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault();
            }

            [CanBeNull]
            private IMethodSymbol ResolveEqualityComparerEquals([NotNull] Compilation compilation)
            {
                INamedTypeSymbol equalityComparerType = KnownTypes.SystemCollectionsGenericEqualityComparerT(compilation);
                return equalityComparerType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault();
            }
        }
    }
}
