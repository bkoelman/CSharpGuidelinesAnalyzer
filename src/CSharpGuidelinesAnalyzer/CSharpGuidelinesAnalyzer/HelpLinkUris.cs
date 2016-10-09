using System;
using JetBrains.Annotations;

namespace CSharpGuidelinesAnalyzer
{
    public static class HelpLinkUris
    {
        private const string Naming =
            "https://github.com/dennisdoomen/CSharpGuidelines/blob/master/Guidelines/1700_NamingGuidelines.md";

        [NotNull]
        public static string GetForCategory([NotNull] string category)
        {
            Guard.NotNull(category, nameof(category));

            switch (category)
            {
                case "Naming":
                    return Naming;

                default:
                    throw new NotSupportedException($"Internal error: Unknown analyzer category {category}.");
            }
        }
    }
}