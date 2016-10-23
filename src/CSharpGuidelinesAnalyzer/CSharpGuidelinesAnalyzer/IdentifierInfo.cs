using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    public sealed class IdentifierInfo
    {
        [NotNull]
        public string Name { get; }

        [NotNull]
        public string LongName { get; }

        [NotNull]
        public ITypeSymbol Type { get; }

        [NotNull]
        public string Kind { get; }

        public IdentifierInfo([NotNull] string name, [NotNull] string longName, [NotNull] ITypeSymbol type,
            [NotNull] string kind)
        {
            Guard.NotNull(name, nameof(name));
            Guard.NotNull(longName, nameof(longName));
            Guard.NotNull(type, nameof(type));
            Guard.NotNull(kind, nameof(kind));

            Name = name;
            LongName = longName;
            Type = type;
            Kind = kind;
        }
    }
}