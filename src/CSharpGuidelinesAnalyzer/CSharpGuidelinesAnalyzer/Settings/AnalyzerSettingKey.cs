using System;
using System.Text;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer.Settings;

internal sealed class AnalyzerSettingKey
{
    [NotNull]
    [ItemNotNull]
    private readonly Lazy<string> lazyNameInSnakeCase;

    [NotNull]
    public string Rule { get; }

    [NotNull]
    public string Name { get; }

    [NotNull]
    public string NameInSnakeCase => lazyNameInSnakeCase.Value;

    public AnalyzerSettingKey([NotNull] string rule, [NotNull] string name)
    {
        Guard.NotNullNorWhiteSpace(rule, nameof(rule));
        Guard.NotNullNorWhiteSpace(name, nameof(name));

        Rule = rule;
        Name = name;

        lazyNameInSnakeCase = new Lazy<string>(GetNameInSnakeCase);
    }

    [NotNull]
    private string GetNameInSnakeCase()
    {
        var builder = new StringBuilder();

        for (int index = 0; index < Name.Length; index++)
        {
            char ch = Name[index];

            if (char.IsUpper(ch))
            {
                if (index > 0)
                {
                    builder.Append('_');
                }

                char lowerCaseChar = char.ToLowerInvariant(ch);
                builder.Append(lowerCaseChar);
                continue;
            }

            builder.Append(ch);
        }

        return builder.ToString();
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