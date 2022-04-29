using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test
{
    internal sealed class ParsedSourceCode
    {
        public AnalyzerTestContext TestContext { get; }

        public ParsedSourceCode(string sourceText, AnalyzerTestContext testContext)
        {
            Guard.NotNull(sourceText, nameof(sourceText));
            Guard.NotNull(testContext, nameof(testContext));

            var document = new FixableDocument(sourceText);
            TestContext = testContext.WithCode(document.SourceText, document.SourceSpans);
        }
    }
}
