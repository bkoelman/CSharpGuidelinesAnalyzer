using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;

namespace CSharpGuidelinesAnalyzer.Test;

public abstract class CSharpGuidelinesAnalysisTestFixture : AnalysisTestFixture
{
    private protected async Task VerifyGuidelineDiagnosticAsync(ParsedSourceCode source, params string[] messages)
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNull(messages, nameof(messages));

        await AssertDiagnosticsAsync(source.TestContext, messages);
    }
}
