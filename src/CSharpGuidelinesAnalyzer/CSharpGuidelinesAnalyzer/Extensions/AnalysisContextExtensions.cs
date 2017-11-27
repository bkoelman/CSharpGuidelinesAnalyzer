using System;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    /// <summary />
    internal static class AnalysisContextExtensions
    {
        public static void RegisterConditionalOperationAction([NotNull] this AnalysisContext context,
            [NotNull] Action<OperationAnalysisContext> action, [NotNull] params OperationKind[] operationKinds)
        {
            RegisterConditionalOperationAction(context, action, operationKinds.ToImmutableArray());
        }

        public static void RegisterConditionalOperationAction([NotNull] this AnalysisContext context,
            [NotNull] Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(action, nameof(action));
            Guard.NotNullNorEmpty(operationKinds, nameof(operationKinds));

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationAction(action, operationKinds);
                }
            });
        }

        public static void RegisterConditionalOperationBlockAction([NotNull] this AnalysisContext context,
            [NotNull] Action<OperationBlockAnalysisContext> action)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(action, nameof(action));

            context.RegisterCompilationStartAction(startContext =>
            {
                if (startContext.Compilation.SupportsOperations())
                {
                    startContext.RegisterOperationBlockAction(action);
                }
            });
        }

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
