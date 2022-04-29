using System.Threading.Tasks;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test
{
    public abstract class CSharpGuidelinesAnalysisTestFixture : AnalysisTestFixture
    {
        private protected async Task VerifyGuidelineDiagnosticAsync([NotNull] ParsedSourceCode source, [NotNull] [ItemNotNull] params string[] messages)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(messages, nameof(messages));

            await AssertDiagnosticsAsync(source.TestContext, messages);
        }
    }
}
