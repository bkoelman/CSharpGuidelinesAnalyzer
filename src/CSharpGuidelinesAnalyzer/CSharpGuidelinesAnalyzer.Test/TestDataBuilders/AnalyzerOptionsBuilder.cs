using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    internal sealed class AnalyzerOptionsBuilder : ITestDataBuilder<AnalyzerOptions>
    {
        [CanBeNull]
        private AdditionalText xmlSettings;

        [CanBeNull]
        private AnalyzerConfigOptionsProvider editorConfigSettings;

        public AnalyzerOptions Build()
        {
            ImmutableArray<AdditionalText> textArray = xmlSettings == null
                ? ImmutableArray<AdditionalText>.Empty
                : new[]
                {
                    xmlSettings
                }.ToImmutableArray();

            return editorConfigSettings != null ? new AnalyzerOptions(textArray, editorConfigSettings) : new AnalyzerOptions(textArray);
        }

        [NotNull]
        public AnalyzerOptionsBuilder ForXmlSettings([NotNull] XmlSettingsBuilder builder)
        {
            xmlSettings = builder.Build();
            return this;
        }

        [NotNull]
        public AnalyzerOptionsBuilder ForXmlText([NotNull] string content)
        {
            xmlSettings = XmlSettingsBuilder.FromContent(content);
            return this;
        }

        [NotNull]
        public AnalyzerOptionsBuilder ForEditorConfig([NotNull] EditorConfigSettingsBuilder builder)
        {
            editorConfigSettings = builder.Build();
            return this;
        }
    }
}
