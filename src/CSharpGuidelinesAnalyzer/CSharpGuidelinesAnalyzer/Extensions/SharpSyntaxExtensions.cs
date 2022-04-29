using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    internal static class SharpSyntaxExtensions
    {
        [CanBeNull]
        public static BaseTypeDeclarationSyntax TryGetContainingTypeDeclaration([NotNull] this SyntaxNode syntax)
        {
            Guard.NotNull(syntax, nameof(syntax));

            SyntaxNode parent = syntax.Parent;

            while (parent != null)
            {
                if (parent is BaseTypeDeclarationSyntax typeSyntax)
                {
                    return typeSyntax;
                }

                parent = parent.Parent;
            }

            return null;
        }
    }
}
