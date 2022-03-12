using System.Linq;
using CSharpGuidelinesAnalyzer.Extensions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class NullCheckScanner
    {
        [NotNull]
        private readonly KnownSymbols knownSymbols;

        [CanBeNull]
        public IPropertySymbol NullableHasValueProperty => knownSymbols.NullableValueProperty;

        public NullCheckScanner([NotNull] Compilation compilation)
        {
            Guard.NotNull(compilation, nameof(compilation));

            knownSymbols = new KnownSymbols(compilation);
        }

        [CanBeNull]
        public NullCheckScanResult? ScanPropertyReference([NotNull] IPropertyReferenceOperation propertyReference)
        {
            Guard.NotNull(propertyReference, nameof(propertyReference));

            if (propertyReference.Property.OriginalDefinition.IsEqualTo(knownSymbols.NullableHasValueProperty) &&
                IsNullableValueType(propertyReference.Instance))
            {
                NullCheckOperand nullCheckOperand = GetParentNullCheckOperand(propertyReference);
                NullCheckOperand toggledOperand = nullCheckOperand.Toggle();

                return new NullCheckScanResult(propertyReference.Instance, NullCheckMethod.NullableHasValueMethod, toggledOperand);
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

        [CanBeNull]
        private NullCheckScanResult? AnalyzeDoubleArgumentInvocation([NotNull] IInvocationOperation invocation)
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

        [CanBeNull]
        private NullCheckMethod? TryGetNullCheckForDoubleArgumentInvocation([NotNull] IInvocationOperation invocation)
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

        private bool IsObjectReferenceEquals([NotNull] IInvocationOperation invocation)
        {
            return invocation.TargetMethod.IsEqualTo(knownSymbols.StaticObjectReferenceEqualsMethod);
        }

        private bool IsStaticObjectEquals([NotNull] IInvocationOperation invocation)
        {
            return invocation.TargetMethod.IsEqualTo(knownSymbols.StaticObjectEqualsMethod);
        }

        private bool IsEqualityComparerEquals([NotNull] IInvocationOperation invocation)
        {
            return invocation.TargetMethod.OriginalDefinition.IsEqualTo(knownSymbols.EqualityComparerEqualsMethod);
        }

        [CanBeNull]
        public NullCheckScanResult? ScanIsPattern([NotNull] IIsPatternOperation isPattern)
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

        [CanBeNull]
        public NullCheckScanResult? ScanBinaryOperator([NotNull] IBinaryOperation binaryOperator)
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

        [CanBeNull]
        private NullCheckOperand? TryGetBinaryOperatorNullCheckOperand([NotNull] IBinaryOperation binaryOperator)
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

        private NullCheckOperand GetParentNullCheckOperand([NotNull] IOperation operation)
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

        private static bool IsConstantNullOrDefault([NotNull] IOperation operation)
        {
            if (operation.ConstantValue.HasValue && operation.ConstantValue.Value == null)
            {
                return true;
            }

            return operation is IDefaultValueOperation;
        }

        [CanBeNull]
        private NullCheckScanResult? AnalyzeArguments(ArgumentsInfo info)
        {
            IOperation leftArgumentNoConversion = info.LeftArgument.SkipTypeConversions();
            IOperation rightArgumentNoConversion = info.RightArgument.SkipTypeConversions();

            ArgumentsInfo arguments = info.WithArguments(leftArgumentNoConversion, rightArgumentNoConversion);
            return InnerAnalyzeArguments(arguments);
        }

        [CanBeNull]
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

        private bool IsNullableValueType([CanBeNull] IOperation operation)
        {
            return operation != null && operation.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
        }

        private sealed class KnownSymbols
        {
            [CanBeNull]
            public IPropertySymbol NullableHasValueProperty { get; }

            [CanBeNull]
            public IPropertySymbol NullableValueProperty { get; }

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
                NullableValueProperty = ResolveNullableValueProperty(compilation);
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
            private static IPropertySymbol ResolveNullableValueProperty([NotNull] Compilation compilation)
            {
                INamedTypeSymbol nullableType = KnownTypes.SystemNullableT(compilation);
                return nullableType?.GetMembers("Value").OfType<IPropertySymbol>().FirstOrDefault();
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
                return objectType?.GetMembers("Equals").OfType<IMethodSymbol>().FirstOrDefault(method => method.IsStatic);
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

        private readonly struct ArgumentsInfo
        {
            [NotNull]
            public IOperation LeftArgument { get; }

            public bool IsLeftArgumentNull => IsConstantNullOrDefault(LeftArgument);

            [NotNull]
            public IOperation RightArgument { get; }

            public bool IsRightArgumentNull => IsConstantNullOrDefault(RightArgument);

            public NullCheckMethod NullCheckMethod { get; }

            public NullCheckOperand NullCheckOperand { get; }

            public ArgumentsInfo([NotNull] IOperation leftArgument, [NotNull] IOperation rightArgument, NullCheckMethod nullCheckMethod,
                NullCheckOperand nullCheckOperand)
            {
                LeftArgument = leftArgument;
                RightArgument = rightArgument;
                NullCheckMethod = nullCheckMethod;
                NullCheckOperand = nullCheckOperand;
            }

            public ArgumentsInfo WithArguments([NotNull] IOperation leftArgument, [NotNull] IOperation rightArgument)
            {
                return new ArgumentsInfo(leftArgument, rightArgument, NullCheckMethod, NullCheckOperand);
            }
        }
    }
}
