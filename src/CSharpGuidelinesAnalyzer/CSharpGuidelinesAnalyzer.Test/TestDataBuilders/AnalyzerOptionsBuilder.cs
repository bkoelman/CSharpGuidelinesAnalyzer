using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

internal sealed class AnalyzerOptionsBuilder : ITestDataBuilder<AnalyzerOptions>
{
    private AdditionalText? xmlSettings;
    private AnalyzerConfigOptionsProvider? editorConfigSettings;

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

    public AnalyzerOptionsBuilder ForXmlSettings(XmlSettingsBuilder builder)
    {
        xmlSettings = builder.Build();
        return this;
    }

    public AnalyzerOptionsBuilder ForXmlText(string content)
    {
        xmlSettings = XmlSettingsBuilder.FromContent(content);
        return this;
    }

    public AnalyzerOptionsBuilder ForEditorConfig(EditorConfigSettingsBuilder builder)
    {
        editorConfigSettings = builder.Build();
        return this;
    }
}
