using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary />
internal static class TypeKindExtensions
{
    public static string Format(this TypeKind typeKind)
    {
        return typeKind == TypeKind.Struct ? "Struct" : typeKind.ToString();
    }
}
