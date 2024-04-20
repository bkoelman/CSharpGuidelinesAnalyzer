using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions;

internal static class AccessibilityExtensions
{
    [NotNull]
    public static string ToText(this Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.NotApplicable:
            {
                return string.Empty;
            }
            case Accessibility.ProtectedAndInternal:
            {
                return "Private protected";
            }
            case Accessibility.ProtectedOrInternal:
            {
                return "Protected internal";
            }
            default:
            {
                return accessibility.ToString();
            }
        }
    }
}