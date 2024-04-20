using System.Collections.Immutable;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;

public abstract class AnalysisTestFixture
{
    protected abstract string DiagnosticId { get; }

    protected abstract DiagnosticAnalyzer CreateAnalyzer();

    protected async Task AssertDiagnosticsAsync(AnalyzerTestContext context, params string[] messages)
    {
        FrameworkGuard.NotNull(context, nameof(context));
        FrameworkGuard.NotNull(messages, nameof(messages));

        await RunDiagnosticsAsync(context, messages);
    }

    private async Task RunDiagnosticsAsync(AnalyzerTestContext context, params string[] messages)
    {
        AnalysisResult result = await GetAnalysisResultAsync(context, messages);

        VerifyDiagnosticCount(result);
        VerifyDiagnostics(result);
    }

    private async Task<AnalysisResult> GetAnalysisResultAsync(AnalyzerTestContext context, string[] messages)
    {
        var document = DocumentFactory.ToDocument(context.SourceCode, context);

        IList<Diagnostic> diagnostics = await GetSortedAnalyzerDiagnosticsAsync(document, context);
        return new AnalysisResult(diagnostics, context.SourceSpans, messages);
    }

    private async Task<IList<Diagnostic>> GetSortedAnalyzerDiagnosticsAsync(Document document, AnalyzerTestContext context)
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

    private async IAsyncEnumerable<Diagnostic> EnumerateDiagnosticsForDocumentAsync(Document document, AnalyzerTestContext context)
    {
        CompilationWithAnalyzers compilationWithAnalyzers = await GetCompilationWithAnalyzersAsync(document, context.ValidationMode, context.Options);
        SyntaxTree? tree = await document.GetSyntaxTreeAsync();

        await foreach (Diagnostic diagnostic in EnumerateAnalyzerDiagnosticsAsync(compilationWithAnalyzers, tree!))
        {
            yield return diagnostic;
        }
    }

    private async Task<CompilationWithAnalyzers> GetCompilationWithAnalyzersAsync(Document document, TestValidationMode validationMode, AnalyzerOptions options)
    {
        DiagnosticAnalyzer analyzer = CreateAnalyzer();

        Compilation? compilation = await document.Project.GetCompilationAsync();
        compilation = EnsureAnalyzerIsEnabled(analyzer, compilation!);

        ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(CancellationToken.None);

        if (validationMode != TestValidationMode.AllowCompileErrors)
        {
            ValidateCompileErrors(compilerDiagnostics);
        }

        ImmutableArray<DiagnosticAnalyzer> analyzers = [analyzer];
        return compilation.WithAnalyzers(analyzers, options);
    }

    private static Compilation EnsureAnalyzerIsEnabled(DiagnosticAnalyzer analyzer, Compilation compilation)
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

    private void ValidateCompileErrors(ImmutableArray<Diagnostic> compilerDiagnostics)
    {
        Diagnostic[] compilerErrors = compilerDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        compilerErrors.Should().BeEmpty("test should not have compile errors");
    }

    private static async IAsyncEnumerable<Diagnostic> EnumerateAnalyzerDiagnosticsAsync(CompilationWithAnalyzers compilationWithAnalyzers, SyntaxTree tree)
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

    private static void ThrowForCrashingAnalyzer(Diagnostic diagnostic)
    {
        if (diagnostic.Id == "AD0001")
        {
            string message = diagnostic.GetMessage(CultureInfo.InvariantCulture);
            throw new Exception(message);
        }
    }

    private static void VerifyDiagnosticCount(AnalysisResult result)
    {
        result.DiagnosticsWithLocation.Should().HaveSameCount(result.SpansExpected);
        result.Diagnostics.Should().HaveSameCount(result.MessagesExpected);
    }

    private static void VerifyDiagnostics(AnalysisResult result)
    {
        VerifyDiagnosticMessages(result);
        VerifyDiagnosticLocations(result);
    }

    private static void VerifyDiagnosticMessages(AnalysisResult result)
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

    private static void VerifyDiagnosticLocations(AnalysisResult result)
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
        public IList<Diagnostic> Diagnostics { get; }

        public IList<Diagnostic> DiagnosticsWithLocation => Diagnostics.Where(diagnostic => diagnostic.Location.IsInSource).ToArray();

        public IList<TextSpan> SpansExpected { get; }

        public IList<string> MessagesExpected { get; }

        public AnalysisResult(IList<Diagnostic> diagnostics, IList<TextSpan> spansExpected, IList<string> messagesExpected)
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
