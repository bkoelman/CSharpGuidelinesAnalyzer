using System.Collections.Immutable;
using System.Text;
using System.Threading;
using CSharpGuidelinesAnalyzer.Settings;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    internal sealed class AnalyzerSettingsBuilder : ITestDataBuilder<AnalyzerSettingsRegistry>
    {
        [NotNull]
        private readonly AnalyzerSettingsRegistry registry = new AnalyzerSettingsRegistry();

        public AnalyzerSettingsRegistry Build()
        {
            return registry;
        }

        [NotNull]
        public AnalyzerSettingsBuilder Including([NotNull] string rule, [NotNull] string name, [CanBeNull] string value)
        {
            registry.Add(rule, name, value);
            return this;
        }

        [NotNull]
        public static AnalyzerOptions ToOptions([NotNull] AnalyzerSettingsRegistry registry)
        {
            Guard.NotNull(registry, nameof(registry));

            string content = AnalyzerSettingsProvider.ToFileContent(registry);
            return ToOptions(content);
        }

        [NotNull]
        public static AnalyzerOptions ToOptions([NotNull] string settingsText)
        {
            Guard.NotNull(settingsText, nameof(settingsText));

            AdditionalText additionalText = new FakeAdditionalText(settingsText);
            return new AnalyzerOptions(ImmutableArray.Create(additionalText));
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
            public override SourceText GetText(CancellationToken cancellationToken = new CancellationToken())
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
