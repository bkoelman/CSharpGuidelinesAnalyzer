using System;

namespace CSharpGuidelinesAnalyzer;

internal sealed class AnalyzerCategory
{
    public const string RulePrefix = "AV";

    public static readonly AnalyzerCategory ClassDesign = new("Class Design");

    public static readonly AnalyzerCategory MemberDesign = new("Member Design");

    public static readonly AnalyzerCategory MiscellaneousDesign = new("Miscellaneous Design");

    public static readonly AnalyzerCategory Maintainability = new("Maintainability");

    public static readonly AnalyzerCategory Naming = new("Naming");

    public static readonly AnalyzerCategory Performance = new("Performance");

    public static readonly AnalyzerCategory Framework = new("Framework");

    public static readonly AnalyzerCategory Documentation = new("Documentation");

    public static readonly AnalyzerCategory Layout = new("Layout");

    public string DisplayName { get; }

    private AnalyzerCategory(string displayName)
    {
        DisplayName = displayName;
    }

    public string GetHelpLinkUri(string ruleId)
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
