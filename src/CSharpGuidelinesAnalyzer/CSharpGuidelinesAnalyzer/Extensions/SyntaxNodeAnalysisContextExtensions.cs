using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    public static class SyntaxNodeAnalysisContextExtensions
    {
        public static SymbolAnalysisContext ToSymbolContext(this SyntaxNodeAnalysisContext syntaxContext)
        {
            ISymbol symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node);
            return SyntaxToSymbolContext(syntaxContext, symbol);
        }

        private static SymbolAnalysisContext SyntaxToSymbolContext(SyntaxNodeAnalysisContext syntaxContext,
            [NotNull] ISymbol symbol)
        {
            Guard.NotNull(symbol, nameof(symbol));

            return new SymbolAnalysisContext(symbol, syntaxContext.SemanticModel.Compilation, syntaxContext.Options,
                syntaxContext.ReportDiagnostic, x => true, syntaxContext.CancellationToken);
        }
    }
}