using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class AnalyzerCategory
    {
        public const string RulePrefix = "AV";

        [NotNull]
        public static readonly AnalyzerCategory ClassDesign = new AnalyzerCategory("Class Design");

        [NotNull]
        public static readonly AnalyzerCategory MemberDesign = new AnalyzerCategory("Member Design");

        [NotNull]
        public static readonly AnalyzerCategory MiscellaneousDesign = new AnalyzerCategory("Miscellaneous Design");

        [NotNull]
        public static readonly AnalyzerCategory Maintainability = new AnalyzerCategory("Maintainability");

        [NotNull]
        public static readonly AnalyzerCategory Naming = new AnalyzerCategory("Naming");

        [NotNull]
        public static readonly AnalyzerCategory Framework = new AnalyzerCategory("Framework");

        [NotNull]
        public static readonly AnalyzerCategory Documentation = new AnalyzerCategory("Documentation");

        [NotNull]
        public static readonly AnalyzerCategory Layout = new AnalyzerCategory("Layout");

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
            return $"https://github.com/dennisdoomen/CSharpGuidelines/blob/5.6.0/_rules/{ruleNumber}.md";
        }
    }
}
