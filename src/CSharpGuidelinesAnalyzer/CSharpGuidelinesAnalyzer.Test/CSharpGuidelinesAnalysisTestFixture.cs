using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;
using System.Reflection;

[assembly: AssemblyTrademark("-")]
[assembly: AssemblyCopyright("-")]

namespace CSharpGuidelinesAnalyzer.Test
{
    public abstract class CSharpGuidelinesAnalysisTestFixture : AnalysisTestFixture
    {
        protected void VerifyGuidelineDiagnostic([NotNull] ParsedSourceCode source,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(messages, nameof(messages));

            AssertDiagnostics(source.TestContext, messages);
        }
    }
}
