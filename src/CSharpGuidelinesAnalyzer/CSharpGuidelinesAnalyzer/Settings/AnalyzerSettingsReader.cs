using System;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Settings;

internal sealed class AnalyzerSettingsReader(AnalyzerOptions options, CancellationToken cancellationToken)
{
    private const string EditorConfigFileName = ".editorconfig";

    private readonly AnalyzerConfigOptionsProviderShim analyzerConfigOptionsProvider = new(options);

    private readonly AnalyzerSettingsRegistry settingsRegistry = AnalyzerSettingsProvider.LoadSettings(options, cancellationToken);

    internal int? TryGetInt32(SyntaxTree syntaxTree, AnalyzerSettingKey key, int minValue, int maxValue)
    {
        Guard.NotNull(syntaxTree, nameof(syntaxTree));
        Guard.NotNull(key, nameof(key));

        string keyName = GetEditorConfigKeyName(key);
        string? textValue = TryGetValue(syntaxTree, keyName);

        if (textValue != null)
        {
            if (int.TryParse(textValue, out int value) && value >= minValue && value <= maxValue)
            {
                return value;
            }

            throw new ArgumentOutOfRangeException(
                $"Value for '{keyName.ToLowerInvariant()}' in '{EditorConfigFileName}' must be in range {minValue}-{maxValue}.", (Exception?)null);
        }

        return settingsRegistry.TryGetInt32(key, minValue, maxValue);
    }

    private static string GetEditorConfigKeyName(AnalyzerSettingKey key)
    {
        return string.Join(".", "dotnet_diagnostic", key.Rule, key.NameInSnakeCase);
    }

    private string? TryGetValue(SyntaxTree syntaxTree, string keyPath)
    {
        return analyzerConfigOptionsProvider.TryGetOptionValue(syntaxTree, keyPath, out string? value) ? value : null;
    }

    private sealed class AnalyzerConfigOptionsProviderShim(AnalyzerOptions options)
    {
        private static readonly PropertyInfo? AnalyzerConfigOptionsProviderProperty =
            typeof(AnalyzerOptions).GetRuntimeProperty("AnalyzerConfigOptionsProvider");

        private static readonly MethodInfo? GetOptionsMethod =
            AnalyzerConfigOptionsProviderProperty?.PropertyType.GetRuntimeMethod("GetOptions", [typeof(SyntaxTree)]);

        private static readonly MethodInfo? TryGetValueMethod = GetOptionsMethod?.ReturnType.GetRuntimeMethod("TryGetValue", [
            typeof(string),
            typeof(string).MakeByRefType()
        ]);

        private readonly object? providerInstance = AnalyzerConfigOptionsProviderProperty?.GetValue(options);

        public bool TryGetOptionValue(SyntaxTree syntaxTree, string key, out string? value)
        {
            if (providerInstance != null && GetOptionsMethod != null && TryGetValueMethod != null)
            {
                object options = GetOptionsMethod.Invoke(providerInstance, [syntaxTree]);

                object?[] parameters =
                [
                    key,
                    null
                ];

                bool succeeded = (bool)TryGetValueMethod.Invoke(options, parameters);

                if (succeeded)
                {
                    value = (string?)parameters[1];
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
