using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    public abstract class AnalysisTestFixture
    {
        [NotNull]
        protected abstract string DiagnosticId { get; }

        [NotNull]
        protected abstract DiagnosticAnalyzer CreateAnalyzer();

        protected void AssertDiagnostics([NotNull] AnalyzerTestContext context,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(messages, nameof(messages));

            RunDiagnostics(context, messages);
        }

        private void RunDiagnostics([NotNull] AnalyzerTestContext context,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            DocumentWithSpans documentWithSpans = TestHelpers.GetDocumentAndSpansFromMarkup(context);

            ImmutableArray<Diagnostic> diagnostics =
                GetDiagnosticsForDocument(documentWithSpans.Document, context.ValidationMode,
                        context.DiagnosticsMustBeInSourceTree)
                    .OrderBy(d => d.Location.SourceSpan)
                    .Where(d => d.Id == DiagnosticId)
                    .ToImmutableArray();
            ImmutableArray<TextSpan> spans = documentWithSpans.TextSpans.OrderBy(s => s).ToImmutableArray();

            if (context.DiagnosticsMustBeInSourceTree)
            {
                diagnostics.Should().HaveCount(spans.Length);
            }

            messages.Should().HaveCount(diagnostics.Length);

            for (int index = 0; index < diagnostics.Length; index++)
            {
                Diagnostic diagnostic = diagnostics[index];

                if (context.DiagnosticsMustBeInSourceTree)
                {
                    diagnostic.Location.IsInSource.Should().BeTrue();

                    TextSpan span = spans[index];
                    diagnostic.Location.SourceSpan.Should().Be(span);
                }

                diagnostic.GetMessage().Should().Be(messages[index]);
            }
        }

        [NotNull]
        [ItemNotNull]
        private ICollection<Diagnostic> GetDiagnosticsForDocument([NotNull] Document document,
            TestValidationMode validationMode, bool diagnosticsMustBeInSourceTree)
        {
            ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray.Create(CreateAnalyzer());
            Compilation compilation = document.Project.GetCompilationAsync().Result;
            CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

            ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(CancellationToken.None);
            if (validationMode != TestValidationMode.AllowCompileErrors)
            {
                ValidateCompileErrors(compilerDiagnostics);
            }

            SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

            ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
            foreach (Diagnostic analyzerDiagnostic in compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result)
            {
                Location location = analyzerDiagnostic.Location;
                if (!diagnosticsMustBeInSourceTree || (location.IsInSource && location.SourceTree == tree))
                {
                    builder.Add(analyzerDiagnostic);
                }
            }

            return builder.ToImmutable();
        }

        private void ValidateCompileErrors([ItemNotNull] ImmutableArray<Diagnostic> compilerDiagnostics)
        {
            bool hasErrors = compilerDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            hasErrors.Should().BeFalse("test should have no compile errors");
        }
    }
}