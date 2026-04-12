using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Settings;

internal sealed class AnalyzerSettingsReader(AnalyzerOptions options, CancellationToken cancellationToken)
{
    private const string EditorConfigFileName = ".editorconfig";

    private readonly AnalyzerSettingsRegistry settingsRegistry = AnalyzerSettingsProvider.LoadSettings(options, cancellationToken);

    internal int? TryGetInt32(SyntaxTree syntaxTree, AnalyzerSettingKey key, int minValue, int maxValue)
    {
        ArgumentNullException.ThrowIfNull(syntaxTree);
        ArgumentNullException.ThrowIfNull(key);

        string keyName = GetEditorConfigKeyName(key);
        string? textValue = TryGetOptionValue(syntaxTree, keyName);

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

    private string? TryGetOptionValue(SyntaxTree syntaxTree, string key)
    {
        AnalyzerConfigOptions analyzerConfigOptions = options.AnalyzerConfigOptionsProvider.GetOptions(syntaxTree);
        return analyzerConfigOptions.TryGetValue(key, out string? value) ? value : null;
    }
}
