using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    internal struct NullCheckScanResult
    {
        [NotNull]
        public IOperation Operation { get; }

        public NullCheckKind Kind { get; }

        public bool IsInverted { get; }

        public NullCheckScanResult([NotNull] IOperation operation, NullCheckKind kind, bool isInverted)
        {
            Guard.NotNull(operation, nameof(operation));

            Operation = operation;
            Kind = kind;
            IsInverted = isInverted;
        }
    }

    internal enum NullCheckKind
    {
        EqualityOperator,
        IsPattern,
        NullableHasValueMethod,
        NullableEqualsMethod,
        StaticObjectEqualsMethod,
        StaticObjectReferenceEqualsMethod,
        EqualityComparerEqualsMethod
    }
}
