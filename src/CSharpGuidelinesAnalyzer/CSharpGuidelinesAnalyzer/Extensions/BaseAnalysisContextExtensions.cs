using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions;

internal static class BaseAnalysisContextExtensions
{
    public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this SyntaxNodeAnalysisContext context, TTarget target)
    {
        return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken, context.ReportDiagnostic, target);
    }

    public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this SymbolAnalysisContext context, TTarget target)
    {
        return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken, context.ReportDiagnostic, target);
    }

    public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this CompilationAnalysisContext context, TTarget target)
    {
        return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken, context.ReportDiagnostic, target);
    }

    public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this OperationAnalysisContext context, TTarget target)
    {
        return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken, context.ReportDiagnostic, target);
    }
}
