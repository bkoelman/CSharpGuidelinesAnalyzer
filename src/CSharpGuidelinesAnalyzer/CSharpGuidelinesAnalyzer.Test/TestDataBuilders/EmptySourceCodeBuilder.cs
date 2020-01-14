using System.Linq;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal sealed class EmptySourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        private string text = string.Empty;

        public EmptySourceCodeBuilder()
            : base(Enumerable.Empty<string>())
        {
        }

        protected override string GetSourceCode()
        {
            return text;
        }

        [NotNull]
        public EmptySourceCodeBuilder WithCode([NotNull] string code)
        {
            Guard.NotNull(code, nameof(code));

            text += code;
            return this;
        }
    }
}
