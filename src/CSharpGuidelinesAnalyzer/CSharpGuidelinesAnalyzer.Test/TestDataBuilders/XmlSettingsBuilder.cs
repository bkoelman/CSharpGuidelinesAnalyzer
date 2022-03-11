using System.Text;
using System.Threading;
using CSharpGuidelinesAnalyzer.Settings;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    internal sealed class XmlSettingsBuilder : ITestDataBuilder<AdditionalText>
    {
        [NotNull]
        private readonly AnalyzerSettingsRegistry registry = new AnalyzerSettingsRegistry();

        public AdditionalText Build()
        {
            string content = AnalyzerSettingsProvider.ToFileContent(registry);
            return new FakeAdditionalText(content);
        }

        [NotNull]
        public XmlSettingsBuilder Including([NotNull] string rule, [NotNull] string name, [CanBeNull] string value)
        {
            registry.Add(rule, name, value);
            return this;
        }

        [NotNull]
        public static AdditionalText FromContent([NotNull] string content)
        {
            return new FakeAdditionalText(content);
        }

        private sealed class FakeAdditionalText : AdditionalText
        {
            [NotNull]
            private readonly SourceText sourceText;

            [NotNull]
            public override string Path { get; } = AnalyzerSettingsProvider.SettingsFileName;

            public FakeAdditionalText([NotNull] string content)
            {
                sourceText = new FakeSourceText(content, AnalyzerSettingsProvider.CreateEncoding());
            }

            [NotNull]
#pragma warning disable AV1554 // Method contains optional parameter in type hierarchy
            public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
#pragma warning restore AV1554 // Method contains optional parameter in type hierarchy
            {
                return sourceText;
            }

            private sealed class FakeSourceText : SourceText
            {
                [NotNull]
                private readonly string content;

                [NotNull]
                public override Encoding Encoding { get; }

                public override int Length => content.Length;

                public override char this[int position] => content[position];

                public FakeSourceText([NotNull] string content, [NotNull] Encoding encoding)
                {
                    Guard.NotNull(content, nameof(content));
                    Guard.NotNull(encoding, nameof(encoding));

                    this.content = content;
                    Encoding = encoding;
                }

                public override void CopyTo(int sourceIndex, [NotNull] char[] destination, int destinationIndex, int count)
                {
                    content.CopyTo(sourceIndex, destination, destinationIndex, count);
                }
            }
        }
    }
}
