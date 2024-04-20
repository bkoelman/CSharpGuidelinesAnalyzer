using System.Text;
using CSharpGuidelinesAnalyzer.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

internal sealed class XmlSettingsBuilder : ITestDataBuilder<AdditionalText>
{
    private readonly AnalyzerSettingsRegistry registry = new();

    public AdditionalText Build()
    {
        string content = AnalyzerSettingsProvider.ToFileContent(registry);
        return new FakeAdditionalText(content);
    }

    public XmlSettingsBuilder Including(string rule, string name, string? value)
    {
        registry.Add(rule, name, value);
        return this;
    }

    public static AdditionalText FromContent(string content)
    {
        return new FakeAdditionalText(content);
    }

    private sealed class FakeAdditionalText(string content) : AdditionalText
    {
        private readonly SourceText sourceText = new FakeSourceText(content, AnalyzerSettingsProvider.CreateEncoding());

        public override string Path { get; } = AnalyzerSettingsProvider.SettingsFileName;

        public override SourceText GetText(CancellationToken cancellationToken = new())
        {
            return sourceText;
        }

        private sealed class FakeSourceText : SourceText
        {
            private readonly string content;

            public override Encoding Encoding { get; }

            public override int Length => content.Length;

            public override char this[int position] => content[position];

            public FakeSourceText(string content, Encoding encoding)
            {
                Guard.NotNull(content, nameof(content));
                Guard.NotNull(encoding, nameof(encoding));

                this.content = content;
                Encoding = encoding;
            }

            public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
            {
                content.CopyTo(sourceIndex, destination, destinationIndex, count);
            }
        }
    }
}
