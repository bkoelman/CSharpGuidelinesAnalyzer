using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class AnalyzerCategory
    {
        private const string CommitHash = "4ad2ebe71296a1d6e70d2f22f7b997edcd9e257d";

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

            if (!ruleId.StartsWith("AV", StringComparison.Ordinal) || ruleId.Length != 6)
            {
                throw new InvalidOperationException($"Rule '{ruleId}' does not match the format AVnnnn.");
            }

            string ruleNumber = ruleId.Substring(2);
            return $"https://github.com/dennisdoomen/CSharpGuidelines/blob/{CommitHash}/_rules/{ruleNumber}.md";

        }
    }
}
