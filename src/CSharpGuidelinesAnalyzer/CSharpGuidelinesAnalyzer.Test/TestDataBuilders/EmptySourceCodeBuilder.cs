using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal sealed class EmptySourceCodeBuilder : SourceCodeBuilder
    {
        [NotNull]
        private string text = string.Empty;

        protected override string GetSourceCode()
        {
            return text;
        }

        public EmptySourceCodeBuilder()
            : base(false)
        {
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
