using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class AnalyzerCategory
    {
        private const string CommitHash = "1a90b74c95225a0f7e4d0e7a341fd027e17c2320";

        [NotNull]
        public string DisplayName { get; }

        [NotNull]
        private readonly string helpCategoryUri;

        [NotNull]
        public static readonly AnalyzerCategory ClassDesign =
            new AnalyzerCategory("Class Design", "1000_ClassDesignGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory MemberDesign =
            new AnalyzerCategory("Member Design", "1100_MemberDesignGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory MiscellaneousDesign =
            new AnalyzerCategory("Miscellaneous Design", "1200_MiscellaneousDesignGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Maintainability =
            new AnalyzerCategory("Maintainability", "1500_MaintainabilityGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Naming = new AnalyzerCategory("Naming", "1700_NamingGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Framework = new AnalyzerCategory("Framework", "2200_FrameworkGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Documentation =
            new AnalyzerCategory("Documentation", "2300_DocumentationGuidelines.md");

        private AnalyzerCategory([NotNull] string displayName, [NotNull] string documentName)
        {
            DisplayName = displayName;
            helpCategoryUri = $"https://github.com/dennisdoomen/CSharpGuidelines/blob/{CommitHash}/_pages/{documentName}";
        }

        [NotNull]
        public string GetHelpLinkUri([NotNull] string ruleId)
        {
            Guard.NotNullNorWhiteSpace(ruleId, nameof(ruleId));

            return helpCategoryUri + "#" + ruleId.ToLowerInvariant();
        }
    }
}
