using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Extensions;

internal static class NullableAwareSymbolExtensions
{
    // Workaround for https://github.com/dotnet/roslyn/issues/39166.
    // These properties are not nullable-aware in Roslyn; very dangerous!
    extension(ISymbol symbol)
    {
        public ISymbol? NullableContainingSymbol => symbol.ContainingSymbol;
        public IAssemblySymbol? NullableContainingAssembly => symbol.ContainingAssembly;
        public IModuleSymbol? NullableContainingModule => symbol.ContainingModule;
        public INamedTypeSymbol? NullableContainingType => symbol.ContainingType;
        public INamespaceSymbol? NullableContainingNamespace => symbol.ContainingNamespace;
    }
}
