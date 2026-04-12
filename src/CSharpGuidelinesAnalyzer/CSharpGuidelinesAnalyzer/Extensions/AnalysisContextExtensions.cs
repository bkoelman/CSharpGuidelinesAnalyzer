using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions;

/// <summary>
/// Replaces the built-in registration methods to not run on broken code.
/// </summary>
internal static class AnalysisContextExtensions
{
    private static readonly ImmutableArray<SyntaxKind> ExtraParameterContainerSyntaxKinds =
    [
        SyntaxKind.LocalFunctionStatement,
        SyntaxKind.SimpleLambdaExpression,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.AnonymousMethodExpression
    ];

    public static void SafeRegisterOperationAction(this AnalysisContext analysisContext, Action<OperationAnalysisContext> action,
        params OperationKind[] operationKinds)
    {
        analysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationAction(this AnalysisContext analysisContext, Action<OperationAnalysisContext> action,
        ImmutableArray<OperationKind> operationKinds)
    {
        analysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationBlockAction(this AnalysisContext analysisContext, Action<OperationBlockAnalysisContext> action)
    {
        analysisContext.RegisterOperationBlockAction(context => SkipInvalid(context, action));
    }

    public static void SafeRegisterSymbolAction(this AnalysisContext analysisContext, Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
    {
        ImmutableArray<SymbolKind> symbolKindArray = ImmutableArray.Create(symbolKinds);
        analysisContext.SafeRegisterSymbolAction(action, symbolKindArray);
    }

    public static void SafeRegisterSymbolAction(this AnalysisContext analysisContext, Action<SymbolAnalysisContext> action,
        ImmutableArray<SymbolKind> symbolKinds)
    {
        if (symbolKinds.Contains(SymbolKind.Parameter))
        {
            // Workaround for https://github.com/dotnet/roslyn/issues/35770

            analysisContext.RegisterSyntaxNodeAction(context =>
            {
                if (context.Node is LocalFunctionStatementSyntax localFunctionSyntax)
                {
                    IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(localFunctionSyntax);
                    SafeRegisterParametersAction(context, methodSymbol, action);
                }
                else if (context.Node is LambdaExpressionSyntax lambdaSyntax)
                {
                    var methodSymbol = context.SemanticModel.GetSymbolInfo(lambdaSyntax).Symbol as IMethodSymbol;
                    SafeRegisterParametersAction(context, methodSymbol, action);
                }
                else if (context.Node is AnonymousMethodExpressionSyntax anonymousMethodSyntax)
                {
                    var methodSymbol = context.SemanticModel.GetSymbolInfo(anonymousMethodSyntax).Symbol as IMethodSymbol;
                    SafeRegisterParametersAction(context, methodSymbol, action);
                }
            }, ExtraParameterContainerSyntaxKinds);
        }

        analysisContext.RegisterSymbolAction(context => SkipEmptyName(context, action), symbolKinds);
    }

    private static void SafeRegisterParametersAction(SyntaxNodeAnalysisContext syntaxContext, IMethodSymbol? methodSymbol, Action<SymbolAnalysisContext> action)
    {
        foreach (IParameterSymbol parameter in methodSymbol?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty)
        {
            if (!parameter.IsImplicitlyDeclared)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var symbolContext = new SymbolAnalysisContext(parameter, syntaxContext.Compilation, syntaxContext.Options, syntaxContext.ReportDiagnostic,
                    _ => true, syntaxContext.CancellationToken);
#pragma warning restore CS0618 // Type or member is obsolete

                Action<SymbolAnalysisContext> safeAction = context => SkipEmptyName(context, action);
                safeAction(symbolContext);
            }
        }
    }

    public static void SafeRegisterOperationAction(this CompilationStartAnalysisContext compilationStartAnalysisContext,
        Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
    {
        compilationStartAnalysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationAction(this CompilationStartAnalysisContext compilationStartAnalysisContext,
        Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
    {
        compilationStartAnalysisContext.RegisterOperationAction(context => SkipInvalid(context, action), operationKinds);
    }

    public static void SafeRegisterOperationBlockAction(this CompilationStartAnalysisContext compilationStartAnalysisContext,
        Action<OperationBlockAnalysisContext> action)
    {
        compilationStartAnalysisContext.RegisterOperationBlockAction(context => SkipInvalid(context, action));
    }

    public static void SafeRegisterSymbolAction(this CompilationStartAnalysisContext compilationStartAnalysisContext, Action<SymbolAnalysisContext> action,
        params SymbolKind[] symbolKinds)
    {
        compilationStartAnalysisContext.RegisterSymbolAction(context => SkipEmptyName(context, action), symbolKinds);
    }

    public static void SafeRegisterSymbolAction(this CompilationStartAnalysisContext compilationStartAnalysisContext, Action<SymbolAnalysisContext> action,
        ImmutableArray<SymbolKind> symbolKinds)
    {
        compilationStartAnalysisContext.RegisterSymbolAction(context => SkipEmptyName(context, action), symbolKinds);
    }

    private static void SkipInvalid(OperationAnalysisContext context, Action<OperationAnalysisContext> action)
    {
        if (!context.Operation.HasErrors(context.Compilation, context.CancellationToken))
        {
            action(context);
        }
    }

    private static void SkipInvalid(OperationBlockAnalysisContext context, Action<OperationBlockAnalysisContext> action)
    {
        if (!context.OperationBlocks.Any(block => block.HasErrors(context.Compilation, context.CancellationToken)))
        {
            action(context);
        }
    }

    private static void SkipEmptyName(SymbolAnalysisContext context, Action<SymbolAnalysisContext> action)
    {
        if (!string.IsNullOrEmpty(context.Symbol.Name))
        {
            action(context);
        }
    }
}
