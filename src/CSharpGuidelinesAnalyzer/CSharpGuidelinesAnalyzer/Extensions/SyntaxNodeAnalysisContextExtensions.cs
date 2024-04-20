using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary />
internal static class SyntaxNodeAnalysisContextExtensions
{
    public static SymbolAnalysisContext ToSymbolContext(this SyntaxNodeAnalysisContext syntaxContext)
    {
        ISymbol symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node);
        return SyntaxToSymbolContext(syntaxContext, symbol);
    }

    private static SymbolAnalysisContext SyntaxToSymbolContext(SyntaxNodeAnalysisContext syntaxContext, [CanBeNull] ISymbol symbol)
    {
        return new SymbolAnalysisContext(symbol, syntaxContext.SemanticModel.Compilation, syntaxContext.Options, syntaxContext.ReportDiagnostic, _ => true,
            syntaxContext.CancellationToken);
    }
}