using System.Collections.Immutable;

namespace CSharpGuidelinesAnalyzer.Settings;

public sealed class AnalyzerSettingsRegistry
{
    internal static readonly AnalyzerSettingsRegistry ImmutableEmpty = new(ImmutableDictionary<AnalyzerSettingKey, string>.Empty);

    private readonly IDictionary<AnalyzerSettingKey, string> settings;

    internal bool IsEmpty => !settings.Any();

    public AnalyzerSettingsRegistry()
        : this(new Dictionary<AnalyzerSettingKey, string>())
    {
    }

    private AnalyzerSettingsRegistry(IDictionary<AnalyzerSettingKey, string> settings)
    {
        this.settings = settings;
    }

    public void Add(string rule, string name, string? value)
    {
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(name);

        if (value != null)
        {
            var key = new AnalyzerSettingKey(rule, name);
            settings[key] = value;
        }
    }

    internal int? TryGetInt32(AnalyzerSettingKey key, int minValue, int maxValue)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (settings.ContainsKey(key) && !string.IsNullOrEmpty(settings[key]))
        {
            if (int.TryParse(settings[key], out int value) && value >= minValue && value <= maxValue)
            {
                return value;
            }

            throw new ArgumentOutOfRangeException($"Value for '{key}' in '{AnalyzerSettingsProvider.SettingsFileName}' must be in range {minValue}-{maxValue}.",
                (Exception?)null);
        }

        return null;
    }

    internal IEnumerable<KeyValuePair<AnalyzerSettingKey, string>> GetAll()
    {
        return settings;
    }
}
