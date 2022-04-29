namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

/// <summary />
internal sealed class EmptySourceCodeBuilder : SourceCodeBuilder
{
    private string text = string.Empty;

    public EmptySourceCodeBuilder()
        : base(Enumerable.Empty<string>())
    {
    }

    protected override string GetSourceCode()
    {
        return text;
    }

    public EmptySourceCodeBuilder WithCode(string code)
    {
        Guard.NotNull(code, nameof(code));

        text += code;
        return this;
    }
}
