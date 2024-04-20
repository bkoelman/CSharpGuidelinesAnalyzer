using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer;

internal struct NullCheckScanResult
{
    [NotNull]
    public IOperation Target { get; }

    public NullCheckMethod Method { get; }

    public NullCheckOperand Operand { get; }

    public NullCheckScanResult([NotNull] IOperation target, NullCheckMethod method, NullCheckOperand operand)
    {
        Guard.NotNull(target, nameof(target));

        Target = target;
        Method = method;
        Operand = operand;
    }
}