using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Settings
{
    public sealed class AnalyzerSettingsRegistry
    {
        [NotNull]
        private readonly IDictionary<AnalyzerSettingKey, string> settings;

        [NotNull]
        internal static readonly AnalyzerSettingsRegistry ImmutableEmpty =
            new AnalyzerSettingsRegistry(ImmutableDictionary<AnalyzerSettingKey, string>.Empty);

        internal bool IsEmpty => !settings.Any();

        public AnalyzerSettingsRegistry()
            : this(new Dictionary<AnalyzerSettingKey, string>())
        {
        }

        private AnalyzerSettingsRegistry([NotNull] IDictionary<AnalyzerSettingKey, string> settings)
        {
            this.settings = settings;
        }

        public void Add([NotNull] string rule, [NotNull] string name, [CanBeNull] string value)
        {
            Guard.NotNull(rule, nameof(rule));
            Guard.NotNull(name, nameof(name));

            var key = new AnalyzerSettingKey(rule, name);
            settings[key] = value;
        }

        [CanBeNull]
        internal int? TryGetInt32([NotNull] AnalyzerSettingKey key, int minValue, int maxValue)
        {
            Guard.NotNull(key, nameof(key));

            if (settings.ContainsKey(key) && int.TryParse(settings[key], out int value))
            {
                if (value < minValue || value > maxValue)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Value for {key} configuration setting must be in range {minValue}-{maxValue}.", (Exception)null);
                }

                return value;
            }

            return null;
        }

        [NotNull]
        internal IEnumerable<KeyValuePair<AnalyzerSettingKey, string>> GetAll()
        {
            return settings;
        }
    }
}
