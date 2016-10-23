using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    public static class HelpLinkUris
    {
        private const string ClassDesign =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1000_ClassDesignGuidelines.md";

        private const string Documentation =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2300_DocumentationGuidelines.md";

        private const string Framework =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/2200_FrameworkGuidelines.md";

        private const string Maintainability =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1500_MaintainabilityGuidelines.md";

        private const string MemberDesign =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1100_MemberDesignGuidelines.md";

        private const string MiscellaneousDesign =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1200_MiscellaneousDesignGuidelines.md";

        private const string Naming =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Src/Guidelines/1700_NamingGuidelines.md";

        [NotNull]
        public static string GetForCategory([NotNull] string category, [NotNull] string ruleId)
        {
            Guard.NotNull(category, nameof(category));
            Guard.NotNull(ruleId, nameof(ruleId));

            string categoryLink = GetLinkForCategory(category);
            return categoryLink + "#" + ruleId.ToLowerInvariant();
        }

        [NotNull]
        private static string GetLinkForCategory([NotNull] string category)
        {
            switch (category)
            {
                case "Class Design":
                    return ClassDesign;

                case "Documentation":
                    return Documentation;

                case "Framework":
                    return Framework;

                case "Maintainability":
                    return Maintainability;

                case "Member Design":
                    return MemberDesign;

                case "Miscellaneous Design":
                    return MiscellaneousDesign;

                case "Naming":
                    return Naming;

                default:
                    throw new NotSupportedException($"Internal error: Unknown analyzer category {category}.");
            }
        }
    }
}