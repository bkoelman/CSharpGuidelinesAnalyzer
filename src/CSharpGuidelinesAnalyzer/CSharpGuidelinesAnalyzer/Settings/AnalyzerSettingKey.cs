using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Settings
{
    internal sealed class AnalyzerSettingKey
    {
        [NotNull]
        public string Rule { get; }

        [NotNull]
        public string Name { get; }

        public AnalyzerSettingKey([NotNull] string rule, [NotNull] string name)
        {
            Guard.NotNullNorWhiteSpace(rule, nameof(rule));
            Guard.NotNullNorWhiteSpace(name, nameof(name));

            Rule = rule;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is AnalyzerSettingKey other && other.Rule == Rule && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Rule.GetHashCode() ^ Name.GetHashCode();
        }

        public override string ToString()
        {
            return Rule + ":" + Name;
        }
    }
}
