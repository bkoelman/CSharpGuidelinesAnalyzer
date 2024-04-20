using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer;

/// <summary>
/// A visitor that skips compiler-generated / implicitly computed operations.
/// </summary>
internal class ExplicitOperationVisitor : OperationVisitor
{
    public override void Visit([CanBeNull] IOperation operation)
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
internal abstract class ExplicitOperationVisitor<TArgument, TResult> : OperationVisitor<TArgument, TResult>
{
    [CanBeNull]
    public override TResult Visit([CanBeNull] IOperation operation, [CanBeNull] TArgument argument)
    {
        if (operation is { IsImplicit: false })
        {
            return operation.Accept(this, argument);
        }

        return default;
    }
}
