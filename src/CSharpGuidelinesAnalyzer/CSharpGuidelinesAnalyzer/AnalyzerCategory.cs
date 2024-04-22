using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer;

internal sealed class AnalyzerCategory
{
    public const string RulePrefix = "AV";

    [NotNull]
    public static readonly AnalyzerCategory ClassDesign = new("Class Design");

    [NotNull]
    public static readonly AnalyzerCategory MemberDesign = new("Member Design");

    [NotNull]
    public static readonly AnalyzerCategory MiscellaneousDesign = new("Miscellaneous Design");

    [NotNull]
    public static readonly AnalyzerCategory Maintainability = new("Maintainability");

    [NotNull]
    public static readonly AnalyzerCategory Naming = new("Naming");

    [NotNull]
    public static readonly AnalyzerCategory Performance = new("Performance");

    [NotNull]
    public static readonly AnalyzerCategory Framework = new("Framework");

    [NotNull]
    public static readonly AnalyzerCategory Documentation = new("Documentation");

    [NotNull]
    public static readonly AnalyzerCategory Layout = new("Layout");

    [NotNull]
    public string DisplayName { get; }

    private AnalyzerCategory([NotNull] string displayName)
    {
        DisplayName = displayName;
    }

    [NotNull]
    public string GetHelpLinkUri([NotNull] string ruleId)
    {
        Guard.NotNullNorWhiteSpace(ruleId, nameof(ruleId));

        if (!ruleId.StartsWith(RulePrefix, StringComparison.Ordinal) || ruleId.Length != 6)
        {
            throw new InvalidOperationException($"Rule '{ruleId}' does not match the format {RulePrefix}nnnn.");
        }

        string ruleNumber = ruleId.Substring(2);
        return $"https://github.com/dennisdoomen/CSharpGuidelines/blob/5.7.0/_rules/{ruleNumber}.md";
    }
}
