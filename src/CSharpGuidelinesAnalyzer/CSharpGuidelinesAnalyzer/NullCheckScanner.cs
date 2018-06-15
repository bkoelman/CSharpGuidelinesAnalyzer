﻿using System.Linq;
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

            if (propertyReference.Property.OriginalDefinition.Equals(knownSymbols.NullableHasValueProperty) &&
                IsNullableValueType(propertyReference.Instance))
            {
                NullCheckOperand nullCheckOperand = GetParentNullCheckOperand(propertyReference);

                return new NullCheckScanResult(propertyReference.Instance, NullCheckMethod.NullableHasValueMethod,
                    nullCheckOperand.Toggle());
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
                    NullCheckOperand nullCheckOperand = GetParentNullCheckOperand(invocation);

                    return AnalyzeArguments(invocation.Instance, invocation.Arguments[0].Value,
                        NullCheckMethod.NullableEqualsMethod, nullCheckOperand);
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

                return AnalyzeArguments(leftArgument.Value, rightArgument.Value, nullCheckMethod.Value, nullCheckOperand);
            }

            return null;
        }

        [CanBeNull]
        private NullCheckMethod? TryGetNullCheckForDoubleArgumentInvocation([NotNull] IInvocationOperation invocation)
        {
            bool isObjectReferenceEquals = invocation.TargetMethod.Equals(knownSymbols.StaticObjectReferenceEqualsMethod);
            if (isObjectReferenceEquals)
            {
                return NullCheckMethod.StaticObjectReferenceEqualsMethod;
            }

            bool isStaticObjectEquals = invocation.TargetMethod.Equals(knownSymbols.StaticObjectEqualsMethod);
            if (isStaticObjectEquals)
            {
                return NullCheckMethod.StaticObjectEqualsMethod;
            }

            bool isEqualityComparerEquals =
                invocation.TargetMethod.OriginalDefinition.Equals(knownSymbols.EqualityComparerEqualsMethod);
            if (isEqualityComparerEquals)
            {
                return NullCheckMethod.EqualityComparerEqualsMethod;
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

            return AnalyzeArguments(binaryOperator.LeftOperand, binaryOperator.RightOperand, NullCheckMethod.EqualityOperator,
                nullCheckOperandCombined);
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
            while (currentOperation is IUnaryOperation unaryOperation && unaryOperation.OperatorKind == UnaryOperatorKind.Not)
            {
                operand = operand.Toggle();
                currentOperation = currentOperation.Parent;
            }

            return operand;
        }

        [CanBeNull]
        private NullCheckScanResult? AnalyzeArguments([NotNull] IOperation leftArgument, [NotNull] IOperation rightArgument,
            NullCheckMethod nullCheckMethod, NullCheckOperand nullCheckOperand)
        {
            IOperation leftArgumentNoConversion = leftArgument.SkipTypeConversions();
            IOperation rightArgumentNoConversion = rightArgument.SkipTypeConversions();

            return InnerAnalyzeArguments(leftArgumentNoConversion, rightArgumentNoConversion, nullCheckMethod, nullCheckOperand);
        }

        [CanBeNull]
        private NullCheckScanResult? InnerAnalyzeArguments([NotNull] IOperation leftArgument, [NotNull] IOperation rightArgument,
            NullCheckMethod nullCheckMethod, NullCheckOperand nullCheckOperand)
        {
            bool leftIsNull = IsConstantNullOrDefault(leftArgument);
            bool rightIsNull = IsConstantNullOrDefault(rightArgument);

            if (rightIsNull)
            {
                if (!leftIsNull && IsNullableValueType(leftArgument))
                {
                    return new NullCheckScanResult(leftArgument, nullCheckMethod, nullCheckOperand);
                }
            }
            else
            {
                if (leftIsNull && IsNullableValueType(rightArgument))
                {
                    return new NullCheckScanResult(rightArgument, nullCheckMethod, nullCheckOperand);
                }
            }

            return null;
        }

        private bool IsConstantNullOrDefault([NotNull] IOperation operation)
        {
            if (operation.ConstantValue.HasValue && operation.ConstantValue.Value == null)
            {
                return true;
            }

            return operation is IDefaultValueOperation;
        }

        private bool IsNullableValueType([NotNull] IOperation operation)
        {
            return operation.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
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
