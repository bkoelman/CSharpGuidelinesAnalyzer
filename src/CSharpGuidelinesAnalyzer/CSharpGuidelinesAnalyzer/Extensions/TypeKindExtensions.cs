using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary />
internal static class TypeKindExtensions
{
    [NotNull]
    public static string Format(this TypeKind typeKind)
    {
        return typeKind == TypeKind.Struct ? "Struct" : typeKind.ToString();
    }
}
