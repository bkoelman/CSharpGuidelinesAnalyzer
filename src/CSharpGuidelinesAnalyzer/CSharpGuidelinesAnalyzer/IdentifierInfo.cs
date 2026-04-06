using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer;

internal sealed class IdentifierInfo
{
    public IdentifierName Name { get; }

    public ITypeSymbol Type { get; }

    public IdentifierInfo(IdentifierName name, ITypeSymbol type)
    {
        Guard.NotNull(type, nameof(type));

        Name = name;
        Type = type;
    }
}
