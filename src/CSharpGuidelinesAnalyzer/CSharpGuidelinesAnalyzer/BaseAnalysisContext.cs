using System;
using System.Threading;
using JetBrains.Annotations;
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
    [NotNull]
    private readonly Action<Diagnostic> reportDiagnosticCallback;

    [NotNull]
    public Compilation Compilation { get; }

    [NotNull]
    public AnalyzerOptions Options { get; }

    public CancellationToken CancellationToken { get; }

    [NotNull]
    public TTarget Target { get; }

    public BaseAnalysisContext([NotNull] Compilation compilation, [NotNull] AnalyzerOptions options, CancellationToken cancellationToken,
        [NotNull] Action<Diagnostic> reportDiagnostic, [NotNull] TTarget target)
    {
        Guard.NotNull(compilation, nameof(compilation));
        Guard.NotNull(options, nameof(options));
        Guard.NotNull((object)target, nameof(target));

        Compilation = compilation;
        Options = options;
        CancellationToken = cancellationToken;
        reportDiagnosticCallback = reportDiagnostic;
        Target = target;
    }

    public void ReportDiagnostic([NotNull] Diagnostic diagnostic)
    {
        reportDiagnosticCallback(diagnostic);
    }

    public BaseAnalysisContext<TOther> WithTarget<TOther>([NotNull] TOther target)
    {
        return new BaseAnalysisContext<TOther>(Compilation, Options, CancellationToken, reportDiagnosticCallback, target);
    }
}
