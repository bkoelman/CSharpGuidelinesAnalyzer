using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class AnalysisContextExtensions
    {
        public static void SkipInvalid(this OperationAnalysisContext context,
            [NotNull] Action<OperationAnalysisContext> action)
        {
            if (!context.Operation.IsInvalid)
            {
                action(context);
            }
        }

        public static void SkipInvalid(this OperationBlockAnalysisContext context,
            [NotNull] Action<OperationBlockAnalysisContext> action)
        {
            if (!context.OperationBlocks.Any(block => block.IsInvalid))
            {
                action(context);
            }
        }

        public static void SkipEmptyName(this SymbolAnalysisContext context,
            [NotNull] Action<SymbolAnalysisContext> action)
        {
            if (!string.IsNullOrEmpty(context.Symbol.Name))
            {
                action(context);
            }
        }

        public static void SkipEmptyName(this SyntaxNodeAnalysisContext context,
            [NotNull] Action<SymbolAnalysisContext> action)
        {
            SymbolAnalysisContext symbolContext = context.ToSymbolContext();
            symbolContext.SkipEmptyName(_ => action(symbolContext));
        }
    }
}