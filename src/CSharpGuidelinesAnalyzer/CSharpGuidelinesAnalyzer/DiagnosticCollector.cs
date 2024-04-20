using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer;

internal sealed class DiagnosticCollector : IDisposable
{
    [NotNull]
    private readonly Action<Diagnostic> reportDiagnostic;

    [CanBeNull]
    [ItemNotNull]
    private List<Diagnostic> diagnostics;

    [NotNull]
    [ItemNotNull]
    public ICollection<Diagnostic> Diagnostics
    {
        get
        {
            if (diagnostics == null)
            {
                return [];
            }

            return diagnostics;
        }
    }

    public DiagnosticCollector([NotNull] Action<Diagnostic> reportDiagnostic)
    {
        Guard.NotNull(reportDiagnostic, nameof(reportDiagnostic));

        diagnostics = null;
        this.reportDiagnostic = reportDiagnostic;
    }

    public void Add([NotNull] Diagnostic diagnostic)
    {
        Guard.NotNull(diagnostic, nameof(diagnostic));

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