using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        protected async Task AssertDiagnosticsAsync([NotNull] AnalyzerTestContext context, [NotNull] [ItemNotNull] params string[] messages)
        {
            FrameworkGuard.NotNull(context, nameof(context));
            FrameworkGuard.NotNull(messages, nameof(messages));

            await RunDiagnosticsAsync(context, messages);
        }

        private async Task RunDiagnosticsAsync([NotNull] AnalyzerTestContext context,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            AnalysisResult result = await GetAnalysisResultAsync(context, messages);

            VerifyDiagnosticCount(result);
            VerifyDiagnostics(result);
        }

        [NotNull]
        [ItemNotNull]
        private async Task<AnalysisResult> GetAnalysisResultAsync([NotNull] AnalyzerTestContext context, [NotNull] [ItemNotNull] string[] messages)
        {
            var document = DocumentFactory.ToDocument(context.SourceCode, context);

            IList<Diagnostic> diagnostics = await GetSortedAnalyzerDiagnosticsAsync(document, context);
            return new AnalysisResult(diagnostics, context.SourceSpans, messages);
        }

        [NotNull]
        [ItemNotNull]
        private async Task<IList<Diagnostic>> GetSortedAnalyzerDiagnosticsAsync([NotNull] Document document, [NotNull] AnalyzerTestContext context)
        {
            var diagnostics = new List<Diagnostic>();

            await foreach (Diagnostic diagnostic in EnumerateDiagnosticsForDocumentAsync(document, context))
            {
                if (diagnostic.Id == DiagnosticId)
                {
                    diagnostics.Add(diagnostic);
                }
            }

            return diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceSpan).ToImmutableArray();
        }

        [NotNull]
        [ItemNotNull]
        private async IAsyncEnumerable<Diagnostic> EnumerateDiagnosticsForDocumentAsync([NotNull] Document document,
            [NotNull] AnalyzerTestContext context)
        {
            CompilationWithAnalyzers compilationWithAnalyzers =
                await GetCompilationWithAnalyzersAsync(document, context.ValidationMode, context.Options);

            SyntaxTree tree = await document.GetSyntaxTreeAsync();

            await foreach (Diagnostic diagnostic in EnumerateAnalyzerDiagnosticsAsync(compilationWithAnalyzers, tree!))
            {
                yield return diagnostic;
            }
        }

        [NotNull]
        [ItemNotNull]
        private async Task<CompilationWithAnalyzers> GetCompilationWithAnalyzersAsync([NotNull] Document document,
            TestValidationMode validationMode, [NotNull] AnalyzerOptions options)
        {
            DiagnosticAnalyzer analyzer = CreateAnalyzer();

            Compilation compilation = await document.Project.GetCompilationAsync();
            compilation = EnsureAnalyzerIsEnabled(analyzer, compilation!);

            ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(CancellationToken.None);

            if (validationMode != TestValidationMode.AllowCompileErrors)
            {
                ValidateCompileErrors(compilerDiagnostics);
            }

            ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray.Create(analyzer);
            return compilation.WithAnalyzers(analyzers, options);
        }

        [NotNull]
        private static Compilation EnsureAnalyzerIsEnabled([NotNull] DiagnosticAnalyzer analyzer,
            [NotNull] Compilation compilation)
        {
            ImmutableDictionary<string, ReportDiagnostic> diagnosticOptions = compilation.Options.SpecificDiagnosticOptions;

            foreach (DiagnosticDescriptor descriptor in analyzer.SupportedDiagnostics)
            {
                if (!descriptor.IsEnabledByDefault)
                {
                    diagnosticOptions = diagnosticOptions.Add(descriptor.Id, ReportDiagnostic.Warn);
                }
            }

            CompilationOptions compilationWithSpecificOptions = compilation.Options.WithSpecificDiagnosticOptions(diagnosticOptions);
            return compilation.WithOptions(compilationWithSpecificOptions);
        }

        private void ValidateCompileErrors([ItemNotNull] ImmutableArray<Diagnostic> compilerDiagnostics)
        {
            Diagnostic[] compilerErrors = compilerDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .ToArray();

            compilerErrors.Should().BeEmpty("test should not have compile errors");
        }

        [NotNull]
        [ItemNotNull]
        private static async IAsyncEnumerable<Diagnostic> EnumerateAnalyzerDiagnosticsAsync([NotNull] CompilationWithAnalyzers compilationWithAnalyzers,
            [NotNull] SyntaxTree tree)
        {
            foreach (Diagnostic diagnostic in await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync())
            {
                ThrowForCrashingAnalyzer(diagnostic);

                if (diagnostic.Location.IsInSource)
                {
                    diagnostic.Location.SourceTree.Should().Be(tree);
                }

                yield return diagnostic;
            }
        }

        private static void ThrowForCrashingAnalyzer([NotNull] Diagnostic diagnostic)
        {
            if (diagnostic.Id == "AD0001")
            {
                string message = diagnostic.Descriptor.Description.ToString();
                throw new Exception(message);
            }
        }

        private static void VerifyDiagnosticCount([NotNull] AnalysisResult result)
        {
            result.DiagnosticsWithLocation.Should().HaveSameCount(result.SpansExpected);
            result.Diagnostics.Should().HaveSameCount(result.MessagesExpected);
        }

        private static void VerifyDiagnostics([NotNull] AnalysisResult result)
        {
            VerifyDiagnosticMessages(result);
            VerifyDiagnosticLocations(result);
        }

        private static void VerifyDiagnosticMessages([NotNull] AnalysisResult result)
        {
            int messageIndex = 0;

            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                string messageActual = diagnostic.GetMessage();
                string messageExpected = result.MessagesExpected[messageIndex];
                messageActual.Should().Be(messageExpected);

                messageIndex++;
            }
        }

        private static void VerifyDiagnosticLocations([NotNull] AnalysisResult result)
        {
            int spanIndex = 0;

            foreach (Diagnostic diagnostic in result.DiagnosticsWithLocation)
            {
                TextSpan locationSpanExpected = result.SpansExpected[spanIndex];
                diagnostic.Location.SourceSpan.Should().Be(locationSpanExpected);

                spanIndex++;
            }
        }

        private sealed class AnalysisResult
        {
            [NotNull]
            [ItemNotNull]
            public IList<Diagnostic> Diagnostics { get; }

            [NotNull]
            [ItemNotNull]
            public IList<Diagnostic> DiagnosticsWithLocation => Diagnostics.Where(diagnostic => diagnostic.Location.IsInSource).ToArray();

            [NotNull]
            public IList<TextSpan> SpansExpected { get; }

            [NotNull]
            [ItemNotNull]
            public IList<string> MessagesExpected { get; }

            public AnalysisResult([NotNull] [ItemNotNull] IList<Diagnostic> diagnostics, [NotNull] IList<TextSpan> spansExpected,
                [NotNull] [ItemNotNull] IList<string> messagesExpected)
            {
                FrameworkGuard.NotNull(diagnostics, nameof(diagnostics));
                FrameworkGuard.NotNull(spansExpected, nameof(spansExpected));
                FrameworkGuard.NotNull(messagesExpected, nameof(messagesExpected));

                Diagnostics = diagnostics;
                SpansExpected = spansExpected;
                MessagesExpected = messagesExpected;
            }
        }
    }
}
