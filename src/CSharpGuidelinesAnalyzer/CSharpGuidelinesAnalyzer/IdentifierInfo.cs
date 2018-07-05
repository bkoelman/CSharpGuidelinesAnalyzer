using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class IdentifierInfo
    {
        public IdentifierName Name { get; }

        [NotNull]
        public ITypeSymbol Type { get; }

        public IdentifierInfo(IdentifierName name, [NotNull] ITypeSymbol type)
        {
            Guard.NotNull(type, nameof(type));

            Name = name;
            Type = type;
        }
    }
}
