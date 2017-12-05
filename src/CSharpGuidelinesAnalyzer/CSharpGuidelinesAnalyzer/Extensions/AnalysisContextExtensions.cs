using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class AnalysisContextExtensions
    {
        public static void SkipInvalid(this OperationAnalysisContext context, [NotNull] Action<OperationAnalysisContext> action)
        {
            if (!context.Operation.HasErrors(context.Compilation, context.CancellationToken))
            {
                action(context);
            }
        }

        public static void SkipInvalid(this OperationBlockAnalysisContext context,
            [NotNull] Action<OperationBlockAnalysisContext> action)
        {
            if (!context.OperationBlocks.Any(block => block.HasErrors(context.Compilation, context.CancellationToken)))
            {
                action(context);
            }
        }

        public static void SkipEmptyName(this SymbolAnalysisContext context, [NotNull] Action<SymbolAnalysisContext> action)
        {
            if (!string.IsNullOrEmpty(context.Symbol.Name))
            {
                action(context);
            }
        }

        public static void SkipEmptyName(this SyntaxNodeAnalysisContext context, [NotNull] Action<SymbolAnalysisContext> action)
        {
            SymbolAnalysisContext symbolContext = context.ToSymbolContext();

#pragma warning disable AV2310 // Code blocks should not contain inline comments
            // Bug workaround for https://github.com/dotnet/roslyn/issues/16209
#pragma warning restore AV2310 // Code blocks should not contain inline comments
            if (symbolContext.Symbol != null)
            {
                symbolContext.SkipEmptyName(_ => action(symbolContext));
            }
        }
    }
}
