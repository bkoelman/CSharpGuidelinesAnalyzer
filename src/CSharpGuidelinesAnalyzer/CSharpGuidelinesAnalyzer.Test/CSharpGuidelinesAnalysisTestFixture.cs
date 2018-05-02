using System.Reflection;
using JetBrains.Annotations;
using RoslynTestFramework;

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

            AssertDiagnostics(source.TestContext, messages);
        }
    }
}
