using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

internal sealed class EditorConfigSettingsBuilder : ITestDataBuilder<AnalyzerConfigOptionsProvider>
{
    private readonly FakeAnalyzerConfigOptionsProvider provider = new();

    public AnalyzerConfigOptionsProvider Build()
    {
        return provider;
    }

    public EditorConfigSettingsBuilder Including(string rule, string name, string? value)
    {
        string key = string.Join(".", "dotnet_diagnostic", rule, name);
        provider.Add(key, value);

        return this;
    }

    private sealed class FakeAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly FakeAnalyzerConfigOptions options = new();

        public override AnalyzerConfigOptions GlobalOptions => options;

        public void Add(string key, string? value)
        {
            options.Settings.Add(key, value);
        }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return options;
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return options;
        }

        private sealed class FakeAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            public IDictionary<string, string?> Settings { get; } = new Dictionary<string, string?>(StringComparer.InvariantCultureIgnoreCase);

            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            {
                return Settings.TryGetValue(key, out value);
            }
        }
    }
}
