using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test
{
    public sealed class ParsedSourceCode
    {
        [NotNull]
        public AnalyzerTestContext TestContext { get; }

        public ParsedSourceCode([NotNull] AnalyzerTestContext testContext)
        {
            Guard.NotNull(testContext, nameof(testContext));

            TestContext = testContext;
        }
    }
}
