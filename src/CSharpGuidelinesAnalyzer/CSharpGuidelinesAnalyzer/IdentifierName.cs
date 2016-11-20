using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    public struct IdentifierName
    {
        [NotNull]
        public string ShortName { get; }

        [NotNull]
        public string LongName { get; }

        public IdentifierName([NotNull] string shortName, [NotNull] string longName)
        {
            Guard.NotNull(shortName, nameof(shortName));
            Guard.NotNull(longName, nameof(longName));

            ShortName = shortName;
            LongName = longName;
        }
    }
}