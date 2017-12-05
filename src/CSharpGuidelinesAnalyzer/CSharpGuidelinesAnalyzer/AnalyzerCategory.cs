using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class AnalyzerCategory
    {
        [NotNull]
        public string Name { get; }

        [NotNull]
        public string HelpLinkUri { get; }

        [NotNull]
        public static readonly AnalyzerCategory ClassDesign = new AnalyzerCategory("Class Design",
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1000_ClassDesignGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Documentation = new AnalyzerCategory("Documentation",
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2300_DocumentationGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Framework = new AnalyzerCategory("Framework",
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2200_FrameworkGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Maintainability = new AnalyzerCategory("Maintainability",
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory MemberDesign = new AnalyzerCategory("Member Design",
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1100_MemberDesignGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory MiscellaneousDesign = new AnalyzerCategory("Miscellaneous Design",
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1200_MiscellaneousDesignGuidelines.md");

        [NotNull]
        public static readonly AnalyzerCategory Naming = new AnalyzerCategory("Naming",
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md");

        private AnalyzerCategory([NotNull] string name, [NotNull] string helpLinkUri)
        {
            Name = name;
            HelpLinkUri = helpLinkUri;
        }
    }
}
