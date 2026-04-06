using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;

namespace CSharpGuidelinesAnalyzer.Test;

internal sealed class ParsedSourceCode
{
    public AnalyzerTestContext TestContext { get; }

    public ParsedSourceCode(string sourceText, AnalyzerTestContext testContext)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        ArgumentNullException.ThrowIfNull(testContext);

        var document = new FixableDocument(sourceText);
        TestContext = testContext.WithCode(document.SourceText, document.SourceSpans);
    }
}
