namespace CSharpGuidelinesAnalyzer;

internal struct IdentifierName
{
    public string ShortName { get; }

    public string LongName { get; }

    public IdentifierName(string shortName, string longName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortName);
        ArgumentException.ThrowIfNullOrWhiteSpace(longName);

        ShortName = shortName;
        LongName = longName;
    }
}
