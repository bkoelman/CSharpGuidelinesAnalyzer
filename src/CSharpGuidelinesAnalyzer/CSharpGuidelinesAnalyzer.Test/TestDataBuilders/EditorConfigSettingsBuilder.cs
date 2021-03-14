using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    internal sealed class EditorConfigSettingsBuilder : ITestDataBuilder<AnalyzerConfigOptionsProvider>
    {
        [NotNull]
        private readonly FakeAnalyzerConfigOptionsProvider provider = new FakeAnalyzerConfigOptionsProvider();

        public AnalyzerConfigOptionsProvider Build()
        {
            return provider;
        }

        [NotNull]
        public EditorConfigSettingsBuilder Including([NotNull] string rule, [NotNull] string name, [CanBeNull] string value)
        {
            string key = string.Join(".", "dotnet_diagnostic", rule, name);
            provider.Add(key, value);

            return this;
        }

        private sealed class FakeAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            [NotNull]
            private readonly FakeAnalyzerConfigOptions options = new FakeAnalyzerConfigOptions();

            [NotNull]
            public override AnalyzerConfigOptions GlobalOptions => options;

            public void Add([NotNull] string key, [CanBeNull] string value)
            {
                options.Settings.Add(key, value);
            }

            [NotNull]
            public override AnalyzerConfigOptions GetOptions([NotNull] SyntaxTree tree)
            {
                return options;
            }

            [NotNull]
            public override AnalyzerConfigOptions GetOptions([NotNull] AdditionalText textFile)
            {
                return options;
            }

            private sealed class FakeAnalyzerConfigOptions : AnalyzerConfigOptions
            {
                [NotNull]
                public IDictionary<string, string> Settings { get; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

                public override bool TryGetValue([NotNull] string key, [CanBeNull] out string value)
                {
                    return Settings.TryGetValue(key, out value);
                }
            }
        }
    }
}
