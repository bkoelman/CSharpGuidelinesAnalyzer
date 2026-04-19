using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer;

internal sealed class DiagnosticCollector : IDisposable
{
    private readonly Action<Diagnostic> reportDiagnostic;

    private HashSet<Diagnostic>? diagnostics;

    public ICollection<Diagnostic> Diagnostics
    {
        get
        {
            if (diagnostics == null)
            {
                return ImmutableArray<Diagnostic>.Empty;
            }

            return diagnostics;
        }
    }

    public DiagnosticCollector(Action<Diagnostic> reportDiagnostic)
    {
        ArgumentNullException.ThrowIfNull(reportDiagnostic);

        diagnostics = null;
        this.reportDiagnostic = reportDiagnostic;
    }

    public void Add(Diagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);

        diagnostics ??= [];
        diagnostics.Add(diagnostic);
    }

    public void Dispose()
    {
        foreach (Diagnostic diagnostic in Diagnostics)
        {
            reportDiagnostic(diagnostic);
        }
    }
}
