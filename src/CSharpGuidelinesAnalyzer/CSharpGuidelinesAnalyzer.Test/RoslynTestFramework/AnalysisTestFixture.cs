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
        private static readonly DocumentFactory DocumentFactory = new DocumentFactory();

        [NotNull]
        protected abstract string DiagnosticId { get; }

        [NotNull]
        protected abstract DiagnosticAnalyzer CreateAnalyzer();

        protected void AssertDiagnostics([NotNull] AnalyzerTestContext context, [NotNull] [ItemNotNull] params string[] messages)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(messages, nameof(messages));

            RunDiagnostics(context, messages);
        }

        private void RunDiagnostics([NotNull] AnalyzerTestContext context, [NotNull] [ItemNotNull] params string[] messages)
        {
            AnalysisResult result = GetAnalysisResult(context, messages);

            VerifyDiagnosticCount(result, context.DiagnosticsCaptureMode);
            VerifyDiagnostics(result, context);
        }

        [NotNull]
        private AnalysisResult GetAnalysisResult([NotNull] AnalyzerTestContext context, [NotNull] [ItemNotNull] string[] messages)
        {
            DocumentWithSpans documentWithSpans = DocumentFactory.GetDocumentWithSpansFromMarkup(context);

            IList<Diagnostic> diagnostics = GetSortedAnalyzerDiagnostics(context, documentWithSpans);
            ImmutableArray<TextSpan> spans = documentWithSpans.TextSpans.OrderBy(s => s).ToImmutableArray();

            return new AnalysisResult(diagnostics, spans, messages);
        }

        [NotNull]
        [ItemNotNull]
        private IList<Diagnostic> GetSortedAnalyzerDiagnostics([NotNull] AnalyzerTestContext context,
            [NotNull] DocumentWithSpans documentWithSpans)
        {
            IEnumerable<Diagnostic> diagnostics =
                EnumerateDiagnosticsForDocument(documentWithSpans.Document, context.ValidationMode,
                    context.DiagnosticsCaptureMode).Where(d => d.Id == DiagnosticId);

            if (context.DiagnosticsCaptureMode == DiagnosticsCaptureMode.RequireInSourceTree)
            {
                diagnostics = diagnostics.OrderBy(d => d.Location.SourceSpan);
            }

            return diagnostics.ToImmutableArray();
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<Diagnostic> EnumerateDiagnosticsForDocument([NotNull] Document document,
            TestValidationMode validationMode, DiagnosticsCaptureMode diagnosticsCaptureMode)
        {
            CompilationWithAnalyzers compilationWithAnalyzers = GetCompilationWithAnalyzers(document, validationMode);

            SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

            return EnumerateAnalyzerDiagnostics(compilationWithAnalyzers, tree, diagnosticsCaptureMode);
        }

        [NotNull]
        private CompilationWithAnalyzers GetCompilationWithAnalyzers([NotNull] Document document,
            TestValidationMode validationMode)
        {
            ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray.Create(CreateAnalyzer());
            Compilation compilation = document.Project.GetCompilationAsync().Result;

            ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(CancellationToken.None);
            if (validationMode != TestValidationMode.AllowCompileErrors)
            {
                ValidateCompileErrors(compilerDiagnostics);
            }

            return compilation.WithAnalyzers(analyzers);
        }

        private void ValidateCompileErrors([ItemNotNull] ImmutableArray<Diagnostic> compilerDiagnostics)
        {
            bool hasErrors = compilerDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            hasErrors.Should().BeFalse("test should have no compile errors");
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<Diagnostic> EnumerateAnalyzerDiagnostics(
            [NotNull] CompilationWithAnalyzers compilationWithAnalyzers, [NotNull] SyntaxTree tree,
            DiagnosticsCaptureMode diagnosticsCaptureMode)
        {
            foreach (Diagnostic analyzerDiagnostic in compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result)
            {
                Location location = analyzerDiagnostic.Location;

                if (diagnosticsCaptureMode == DiagnosticsCaptureMode.AllowOutsideSourceTree ||
                    LocationIsInSourceTree(location, tree))
                {
                    yield return analyzerDiagnostic;
                }
            }
        }

        private static bool LocationIsInSourceTree([NotNull] Location location, [CanBeNull] SyntaxTree tree)
        {
            return location.IsInSource && location.SourceTree == tree;
        }

        private static void VerifyDiagnosticCount([NotNull] AnalysisResult result, DiagnosticsCaptureMode captureMode)
        {
            if (captureMode == DiagnosticsCaptureMode.RequireInSourceTree)
            {
                result.Diagnostics.Should().HaveSameCount(result.Spans);
            }

            result.Diagnostics.Should().HaveSameCount(result.Messages);
        }

        private static void VerifyDiagnostics([NotNull] AnalysisResult result, [NotNull] AnalyzerTestContext context)
        {
            for (int index = 0; index < result.Diagnostics.Count; index++)
            {
                Diagnostic diagnostic = result.Diagnostics[index];

                if (context.DiagnosticsCaptureMode == DiagnosticsCaptureMode.RequireInSourceTree)
                {
                    VerifyDiagnosticLocation(diagnostic, result.Spans[index]);
                }

                diagnostic.GetMessage().Should().Be(result.Messages[index]);
            }
        }

        private static void VerifyDiagnosticLocation([NotNull] Diagnostic diagnostic, TextSpan span)
        {
            diagnostic.Location.IsInSource.Should().BeTrue();
            diagnostic.Location.SourceSpan.Should().Be(span);
        }

        private sealed class AnalysisResult
        {
            [NotNull]
            [ItemNotNull]
            public IList<Diagnostic> Diagnostics { get; }

            [NotNull]
            public IList<TextSpan> Spans { get; }

            [NotNull]
            [ItemNotNull]
            public IList<string> Messages { get; }

            public AnalysisResult([NotNull] [ItemNotNull] IList<Diagnostic> diagnostics, [NotNull] IList<TextSpan> spans,
                [NotNull] [ItemNotNull] IList<string> messages)
            {
                Guard.NotNull(diagnostics, nameof(diagnostics));
                Guard.NotNull(spans, nameof(spans));
                Guard.NotNull(messages, nameof(messages));

                Diagnostics = diagnostics;
                Spans = spans;
                Messages = messages;
            }
        }
    }
}
