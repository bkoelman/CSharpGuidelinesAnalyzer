using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    internal struct NullCheckScanResult
    {
        [NotNull]
        public IOperation Target { get; }

        public NullCheckKind Kind { get; }

        public bool IsInverted { get; }

        public NullCheckScanResult([NotNull] IOperation target, NullCheckKind kind, bool isInverted)
        {
            Guard.NotNull(target, nameof(target));

            Target = target;
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
