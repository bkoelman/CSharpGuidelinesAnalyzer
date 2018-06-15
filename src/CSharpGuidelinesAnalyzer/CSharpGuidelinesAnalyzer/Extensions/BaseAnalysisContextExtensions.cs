using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Extensions
{
    internal static class BaseAnalysisContextExtensions
    {
        public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this SyntaxNodeAnalysisContext context, [NotNull] TTarget target)
        {
            return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken,
                context.ReportDiagnostic, target);
        }

        public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this SymbolAnalysisContext context, [NotNull] TTarget target)
        {
            return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken,
                context.ReportDiagnostic, target);
        }

        public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this CompilationAnalysisContext context,
            [NotNull] TTarget target)
        {
            return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken,
                context.ReportDiagnostic, target);
        }

        public static BaseAnalysisContext<TTarget> Wrap<TTarget>(this OperationAnalysisContext context, [NotNull] TTarget target)
        {
            return new BaseAnalysisContext<TTarget>(context.Compilation, context.Options, context.CancellationToken,
                context.ReportDiagnostic, target);
        }
    }
}
