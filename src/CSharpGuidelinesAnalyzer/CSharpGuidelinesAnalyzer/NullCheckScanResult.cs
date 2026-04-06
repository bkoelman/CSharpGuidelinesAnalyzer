using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer;

internal struct NullCheckScanResult
{
    public IOperation Target { get; }

    public NullCheckMethod Method { get; }

    public NullCheckOperand Operand { get; }

    public NullCheckScanResult(IOperation target, NullCheckMethod method, NullCheckOperand operand)
    {
        Guard.NotNull(target, nameof(target));

        Target = target;
        Method = method;
        Operand = operand;
    }
}
