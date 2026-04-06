namespace CSharpGuidelinesAnalyzer;

internal struct IdentifierName
{
    public string ShortName { get; }

    public string LongName { get; }

    public IdentifierName(string shortName, string longName)
    {
        Guard.NotNullNorWhiteSpace(shortName, nameof(shortName));
        Guard.NotNullNorWhiteSpace(longName, nameof(longName));

        ShortName = shortName;
        LongName = longName;
    }
}
