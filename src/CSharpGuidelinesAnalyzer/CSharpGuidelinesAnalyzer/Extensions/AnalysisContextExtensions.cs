using System;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary>
/// Replaces the built-in registration methods to not run on broken code.
/// </summary>
internal static class AnalysisContextExtensions
{
    public static void SafeRegisterOperationAction([NotNull] this AnalysisContext analysisContext, [NotNull] Action<OperationAnalysisContext> action,
        [NotNull] params OperationKind[] operationKinds)
    {
        analysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationAction([NotNull] this AnalysisContext analysisContext, [NotNull] Action<OperationAnalysisContext> action,
        ImmutableArray<OperationKind> operationKinds)
    {
        analysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationBlockAction([NotNull] this AnalysisContext analysisContext, [NotNull] Action<OperationBlockAnalysisContext> action)
    {
        analysisContext.RegisterOperationBlockAction(context => SkipInvalid(context, action));
    }

    public static void SafeRegisterSymbolAction([NotNull] this AnalysisContext analysisContext, [NotNull] Action<SymbolAnalysisContext> action,
        [NotNull] params SymbolKind[] symbolKinds)
    {
        analysisContext.RegisterSymbolAction(context => SkipEmptyName(context, action), symbolKinds);
    }

    public static void SafeRegisterSymbolAction([NotNull] this AnalysisContext analysisContext, [NotNull] Action<SymbolAnalysisContext> action,
        ImmutableArray<SymbolKind> symbolKinds)
    {
        analysisContext.RegisterSymbolAction(context => SkipEmptyName(context, action), symbolKinds);
    }

    public static void SafeRegisterSyntaxNodeAction([NotNull] this AnalysisContext analysisContext, [NotNull] Action<SymbolAnalysisContext> action,
        [NotNull] params SyntaxKind[] syntaxKinds)
    {
        analysisContext.RegisterSyntaxNodeAction(context => SkipEmptyName(context, action), syntaxKinds);
    }

    public static void SafeRegisterSyntaxNodeAction([NotNull] this AnalysisContext analysisContext, [NotNull] Action<SymbolAnalysisContext> action,
        ImmutableArray<SyntaxKind> syntaxKinds)
    {
        analysisContext.RegisterSyntaxNodeAction(context => SkipEmptyName(context, action), syntaxKinds);
    }

    public static void SafeRegisterOperationAction([NotNull] this CompilationStartAnalysisContext compilationStartAnalysisContext,
        [NotNull] Action<OperationAnalysisContext> action, [NotNull] params OperationKind[] operationKinds)
    {
        compilationStartAnalysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationAction([NotNull] this CompilationStartAnalysisContext compilationStartAnalysisContext,
        [NotNull] Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
    {
        compilationStartAnalysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationBlockAction([NotNull] this CompilationStartAnalysisContext compilationStartAnalysisContext,
        [NotNull] Action<OperationBlockAnalysisContext> action)
    {
        compilationStartAnalysisContext.RegisterOperationBlockAction(context => SkipInvalid(context, action));
    }

    public static void SafeRegisterSymbolAction([NotNull] this CompilationStartAnalysisContext compilationStartAnalysisContext,
        [NotNull] Action<SymbolAnalysisContext> action, [NotNull] params SymbolKind[] symbolKinds)
    {
        compilationStartAnalysisContext.RegisterSymbolAction(context => SkipEmptyName(context, action), symbolKinds);
    }

    public static void SafeRegisterSymbolAction([NotNull] this CompilationStartAnalysisContext compilationStartAnalysisContext,
        [NotNull] Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
    {
        compilationStartAnalysisContext.RegisterSymbolAction(context => SkipEmptyName(context, action), symbolKinds);
    }

    public static void SafeRegisterSyntaxNodeAction([NotNull] this CompilationStartAnalysisContext compilationStartAnalysisContext,
        [NotNull] Action<SymbolAnalysisContext> action, [NotNull] params SyntaxKind[] syntaxKinds)
    {
        compilationStartAnalysisContext.RegisterSyntaxNodeAction(context => SkipEmptyName(context, action), syntaxKinds);
    }

    public static void SafeRegisterSyntaxNodeAction([NotNull] this CompilationStartAnalysisContext compilationStartAnalysisContext,
        [NotNull] Action<SymbolAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
    {
        compilationStartAnalysisContext.RegisterSyntaxNodeAction(context => SkipEmptyName(context, action), syntaxKinds);
    }

    private static void SkipInvalid(OperationAnalysisContext context, [NotNull] Action<OperationAnalysisContext> action)
    {
        if (!context.Operation.HasErrors(context.Compilation, context.CancellationToken))
        {
            action(context);
        }
    }

    private static void SkipInvalid(OperationBlockAnalysisContext context, [NotNull] Action<OperationBlockAnalysisContext> action)
    {
        if (!context.OperationBlocks.Any(block => block.HasErrors(context.Compilation, context.CancellationToken)))
        {
            action(context);
        }
    }

    private static void SkipEmptyName(SymbolAnalysisContext context, [NotNull] Action<SymbolAnalysisContext> action)
    {
        if (!string.IsNullOrEmpty(context.Symbol.Name))
        {
            action(context);
        }
    }

    private static void SkipEmptyName(SyntaxNodeAnalysisContext context, [NotNull] Action<SymbolAnalysisContext> action)
    {
        SymbolAnalysisContext symbolContext = context.ToSymbolContext();
        SkipEmptyName(symbolContext, _ => action(symbolContext));
    }
}
