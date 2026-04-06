using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;

namespace CSharpGuidelinesAnalyzer.Test;

public abstract class CSharpGuidelinesAnalysisTestFixture : AnalysisTestFixture
{
    private protected async Task VerifyGuidelineDiagnosticAsync(ParsedSourceCode source, params string[] messages)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(messages);

        await AssertDiagnosticsAsync(source.TestContext, messages);
    }
}
