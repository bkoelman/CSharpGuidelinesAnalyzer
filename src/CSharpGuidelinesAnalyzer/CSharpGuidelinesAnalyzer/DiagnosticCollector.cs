using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer
{
    internal sealed class DiagnosticCollector : IDisposable
    {
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
                    return ImmutableArray<Diagnostic>.Empty;
                }

                return diagnostics;
            }
        }

        [NotNull]
        private readonly Action<Diagnostic> reportDiagnostic;

        public DiagnosticCollector([NotNull] Action<Diagnostic> reportDiagnostic)
        {
            Guard.NotNull(reportDiagnostic, nameof(reportDiagnostic));

            diagnostics = null;
            this.reportDiagnostic = reportDiagnostic;
        }

        public void Add([NotNull] Diagnostic diagnostic)
        {
            Guard.NotNull(diagnostic, nameof(diagnostic));

            if (diagnostics == null)
            {
                diagnostics = new List<Diagnostic>();
            }

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
}
