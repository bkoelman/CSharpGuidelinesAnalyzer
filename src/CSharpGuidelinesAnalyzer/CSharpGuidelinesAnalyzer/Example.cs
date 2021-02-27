using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    public sealed class Example
    {
        [NotNull]
        public string GetText([NotNull] string text)
        {
            return text;
        }
    }
}
