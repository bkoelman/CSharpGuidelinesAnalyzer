using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test
{
    internal sealed class ParsedSourceCode
    {
        [NotNull]
        public AnalyzerTestContext TestContext { get; }

        public ParsedSourceCode([NotNull] string sourceText, [NotNull] AnalyzerTestContext testContext)
        {
            Guard.NotNull(sourceText, nameof(sourceText));
            Guard.NotNull(testContext, nameof(testContext));

            var document = new FixableDocument(sourceText);
            TestContext = testContext.WithCode(document.SourceText, document.SourceSpans);
        }
    }
}
