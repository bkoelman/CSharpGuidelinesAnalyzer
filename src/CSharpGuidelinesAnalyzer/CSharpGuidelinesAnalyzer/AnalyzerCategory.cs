using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class AnalyzerCategory
    {
        [NotNull]
        public string Name { get; }

        [NotNull]
        private readonly string helpCategoryUri;

        [NotNull]
        public static readonly AnalyzerCategory ClassDesign =
            new AnalyzerCategory("Class Design", "https://csharpcodingguidelines.com/class-design-guidelines/");

        [NotNull]
        public static readonly AnalyzerCategory MemberDesign =
            new AnalyzerCategory("Member Design", "https://csharpcodingguidelines.com/member-design-guidelines/");

        [NotNull]
        public static readonly AnalyzerCategory MiscellaneousDesign =
            new AnalyzerCategory("Miscellaneous Design", "https://csharpcodingguidelines.com/misc-design-guidelines/");

        [NotNull]
        public static readonly AnalyzerCategory Maintainability = new AnalyzerCategory("Maintainability",
            "https://csharpcodingguidelines.com/maintainability-guidelines/");

        [NotNull]
        public static readonly AnalyzerCategory Naming =
            new AnalyzerCategory("Naming", "https://csharpcodingguidelines.com/naming-guidelines/");

        [NotNull]
        public static readonly AnalyzerCategory Framework = new AnalyzerCategory("Framework",
            "https://csharpcodingguidelines.com/framework-guidelines/");

        [NotNull]
        public static readonly AnalyzerCategory Documentation = new AnalyzerCategory("Documentation",
            "https://csharpcodingguidelines.com/documentation-guidelines/");

        private AnalyzerCategory([NotNull] string name, [NotNull] string categoryUri)
        {
            Name = name;
            helpCategoryUri = categoryUri;
        }

        [NotNull]
        public string GetHelpLinkUri([NotNull] string ruleId)
        {
            Guard.NotNullNorWhiteSpace(ruleId, nameof(ruleId));

            return helpCategoryUri + "#" + ruleId.ToLowerInvariant();
        }
    }
}
