using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CSharpGuidelinesAnalyzer;

/// <summary>
/// A walker that skips compiler-generated / implicitly computed operations.
/// </summary>
internal abstract class ExplicitOperationWalker : OperationWalker
{
    public override void Visit([CanBeNull] IOperation operation)
    {
        if (operation is { IsImplicit: false })
        {
            base.Visit(operation);
        }
    }
}