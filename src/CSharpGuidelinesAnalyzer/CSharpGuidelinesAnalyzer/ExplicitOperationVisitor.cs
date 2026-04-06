using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer;

/// <summary>
/// A visitor that skips compiler-generated / implicitly computed operations.
/// </summary>
internal class ExplicitOperationVisitor : OperationVisitor
{
    public override void Visit(IOperation? operation)
    {
        if (operation is { IsImplicit: false })
        {
            operation.Accept(this);
        }
    }
}

/// <summary>
/// A visitor that skips compiler-generated / implicitly computed operations.
/// </summary>
internal abstract class ExplicitOperationVisitor<TArgument, TResult> : OperationVisitor<TArgument?, TResult?>
{
    public override TResult? Visit(IOperation? operation, TArgument? argument)
    {
        if (operation is { IsImplicit: false })
        {
            return operation.Accept(this, argument);
        }

        return default;
    }
}
