using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class IdentifierInfo
    {
        public IdentifierName Name { get; }

        [NotNull]
        public ITypeSymbol Type { get; }

        [NotNull]
        public string Kind { get; }

        public IdentifierInfo(IdentifierName name, [NotNull] ITypeSymbol type, [NotNull] string kind)
        {
            Guard.NotNull(type, nameof(type));
            Guard.NotNullNorWhiteSpace(kind, nameof(kind));

            Name = name;
            Type = type;
            Kind = kind;
        }
    }
}
