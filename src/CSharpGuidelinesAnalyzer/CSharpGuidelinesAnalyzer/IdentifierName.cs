using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer;

internal struct IdentifierName
{
    [NotNull]
    public string ShortName { get; }

    [NotNull]
    public string LongName { get; }

    public IdentifierName([NotNull] string shortName, [NotNull] string longName)
    {
        Guard.NotNullNorWhiteSpace(shortName, nameof(shortName));
        Guard.NotNullNorWhiteSpace(longName, nameof(longName));

        ShortName = shortName;
        LongName = longName;
    }
}
