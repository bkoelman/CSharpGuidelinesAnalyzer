using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class OperationAnalysisContextExtensions
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
    }
}