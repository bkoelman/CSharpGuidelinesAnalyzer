using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer;

/// <summary>
/// Contains data that is shared by various analysis contexts.
/// </summary>
/// <typeparam name="TTarget">
/// The target for analysis. Typically a syntax node, symbol or operation.
/// </typeparam>
internal readonly struct BaseAnalysisContext<TTarget>
{
    private readonly Action<Diagnostic> reportDiagnosticCallback;

    public Compilation Compilation { get; }

    public AnalyzerOptions Options { get; }

    public CancellationToken CancellationToken { get; }

    public TTarget Target { get; }

    public BaseAnalysisContext(Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken, Action<Diagnostic> reportDiagnostic,
        TTarget target)
    {
        ArgumentNullException.ThrowIfNull(compilation);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(target);

        Compilation = compilation;
        Options = options;
        CancellationToken = cancellationToken;
        reportDiagnosticCallback = reportDiagnostic;
        Target = target;
    }

    public void ReportDiagnostic(Diagnostic diagnostic)
    {
        reportDiagnosticCallback(diagnostic);
    }

    public BaseAnalysisContext<TOther> WithTarget<TOther>(TOther target)
    {
        return new BaseAnalysisContext<TOther>(Compilation, Options, CancellationToken, reportDiagnosticCallback, target);
    }
}
