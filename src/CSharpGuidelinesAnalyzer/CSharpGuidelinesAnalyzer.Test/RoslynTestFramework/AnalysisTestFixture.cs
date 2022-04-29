using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable once CheckNamespace
namespace RoslynTestFramework
{
    public abstract class AnalysisTestFixture
    {
        [NotNull]
        private static readonly DocumentFactory DocumentFactory = new DocumentFactory();

        [NotNull]
        protected abstract string DiagnosticId { get; }

        [NotNull]
        protected abstract DiagnosticAnalyzer CreateAnalyzer();

        [NotNull]
        protected virtual CodeFixProvider CreateFixProvider()
        {
            throw new NotImplementedException();
        }

        protected void AssertDiagnostics([NotNull] AnalyzerTestContext context, [NotNull] [ItemNotNull] params string[] messages)
        {
            FrameworkGuard.NotNull(context, nameof(context));
            FrameworkGuard.NotNull(messages, nameof(messages));

            RunDiagnostics(context, messages);
        }

        [NotNull]
        private AnalysisResult RunDiagnostics([NotNull] AnalyzerTestContext context,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            AnalysisResult result = GetAnalysisResult(context, messages);

            VerifyDiagnosticCount(result);
            VerifyDiagnostics(result);

            return result;
        }

        [NotNull]
        private AnalysisResult GetAnalysisResult([NotNull] AnalyzerTestContext context, [NotNull] [ItemNotNull] string[] messages)
        {
            var document = DocumentFactory.ToDocument(context.SourceCode, context);

            IList<Diagnostic> diagnostics = GetSortedAnalyzerDiagnostics(document, context);
            return new AnalysisResult(diagnostics, context.SourceSpans, messages);
        }

        [NotNull]
        [ItemNotNull]
        private IList<Diagnostic> GetSortedAnalyzerDiagnostics([NotNull] Document document, [NotNull] AnalyzerTestContext context)
        {
            return EnumerateDiagnosticsForDocument(document, context).Where(diagnostic => diagnostic.Id == DiagnosticId)
                .OrderBy(diagnostic => diagnostic.Location.SourceSpan).ToImmutableArray();
        }

        [NotNull]
        [ItemNotNull]
        private IEnumerable<Diagnostic> EnumerateDiagnosticsForDocument([NotNull] Document document,
            [NotNull] AnalyzerTestContext context)
        {
            CompilationWithAnalyzers compilationWithAnalyzers =
                GetCompilationWithAnalyzers(document, context.ValidationMode, context.Options);

            SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

            return EnumerateAnalyzerDiagnostics(compilationWithAnalyzers, tree);
        }

        [NotNull]
        private CompilationWithAnalyzers GetCompilationWithAnalyzers([NotNull] Document document,
            TestValidationMode validationMode, [NotNull] AnalyzerOptions options)
        {
            DiagnosticAnalyzer analyzer = CreateAnalyzer();

            Compilation compilation = document.Project.GetCompilationAsync().Result;
            compilation = EnsureAnalyzerIsEnabled(analyzer, compilation);

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
        private static IEnumerable<Diagnostic> EnumerateAnalyzerDiagnostics([NotNull] CompilationWithAnalyzers compilationWithAnalyzers,
            [NotNull] SyntaxTree tree)
        {
            foreach (Diagnostic diagnostic in compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result)
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

        protected void AssertDiagnosticsWithCodeFixes([NotNull] FixProviderTestContext context,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            FrameworkGuard.NotNull(context, nameof(context));
            FrameworkGuard.NotNull(messages, nameof(messages));

            AnalysisResult analysisResult = RunDiagnostics(context.AnalyzerTestContext, messages);

            FixProviderTestContext fixContext = UpdateContextForComparisonMode(context);
            RunCodeFixesForFirstDiagnostic(analysisResult, fixContext);
        }

        protected void AssertDiagnosticsWithAllCodeFixes([NotNull] FixProviderTestContext context,
            [NotNull] [ItemNotNull] params string[] messages)
        {
            FrameworkGuard.NotNull(context, nameof(context));
            FrameworkGuard.NotNull(messages, nameof(messages));

            AnalysisResult analysisResult = RunDiagnostics(context.AnalyzerTestContext, messages);

            FixProviderTestContext fixContext = UpdateContextForComparisonMode(context);

            RunAllCodeFixesForDiagnostics(analysisResult.Diagnostics, fixContext);
        }

        [NotNull]
        private static FixProviderTestContext UpdateContextForComparisonMode([NotNull] FixProviderTestContext context)
        {
            if (context.CodeComparisonMode == TextComparisonMode.IgnoreWhitespaceDifferences)
            {
                ICollection<string> expectedCode = context.ExpectedCode
                    .Select(code => DocumentFactory.FormatSourceCode(code, context.AnalyzerTestContext)).ToArray();

                return context.WithExpectedCode(expectedCode);
            }

            return context;
        }

        private void RunCodeFixesForFirstDiagnostic([NotNull] AnalysisResult analysisResult,
            [NotNull] FixProviderTestContext context)
        {
            CodeFixProvider fixProvider = CreateFixProvider();

            Diagnostic firstDiagnostic = analysisResult.Diagnostics.FirstOrDefault();

            if (firstDiagnostic != null)
            {
                RunCodeFixesForDiagnostic(context, firstDiagnostic, fixProvider);
            }
        }

        private void RunCodeFixesForDiagnostic([NotNull] FixProviderTestContext context, [NotNull] Diagnostic diagnostic,
            [NotNull] CodeFixProvider fixProvider)
        {
            for (int index = 0; index < context.ExpectedCode.Count; index++)
            {
                var document =
                    DocumentFactory.ToDocument(context.AnalyzerTestContext.SourceCode, context.AnalyzerTestContext);

                ImmutableArray<CodeAction> codeFixes = GetCodeFixesForDiagnostic(diagnostic, document, fixProvider);
                codeFixes.Should().HaveSameCount(context.ExpectedCode);

                VerifyResultOfCodeFix(codeFixes[index], document, context, context.ExpectedCode[index]);
            }
        }

        [ItemNotNull]
        private ImmutableArray<CodeAction> GetCodeFixesForDiagnostic([NotNull] Diagnostic diagnostic, [NotNull] Document document,
            [NotNull] CodeFixProvider fixProvider)
        {
            ImmutableArray<CodeAction>.Builder builder = ImmutableArray.CreateBuilder<CodeAction>();

            var fixContext = new CodeFixContext(document, diagnostic, RegisterCodeFix, CancellationToken.None);
            fixProvider.RegisterCodeFixesAsync(fixContext).Wait();

            return builder.ToImmutable();

            void RegisterCodeFix(CodeAction codeAction, ImmutableArray<Diagnostic> _)
            {
                builder.Add(codeAction);
            }
        }

        private void RunAllCodeFixesForDiagnostics([NotNull] [ItemNotNull] IList<Diagnostic> diagnostics,
            [NotNull] FixProviderTestContext context)
        {
            CodeFixProvider fixProvider = CreateFixProvider();

            var diagnosticProvider = new SimpleDiagnosticProvider(diagnostics);

            for (int index = 0; index < context.ExpectedCode.Count; index++)
            {
                var document =
                    DocumentFactory.ToDocument(context.AnalyzerTestContext.SourceCode, context.AnalyzerTestContext);

                string equivalenceKey = context.EquivalenceKeysForFixAll[index];
                CodeAction codeFix = GetCodeFixForEquivalenceKey(equivalenceKey, document, fixProvider, diagnosticProvider);

                string expectedCode = context.ExpectedCode[index];
                VerifyResultOfCodeFix(codeFix, document, context, expectedCode);
            }
        }

        [NotNull]
        private static CodeAction GetCodeFixForEquivalenceKey([NotNull] string equivalenceKey, [NotNull] Document document,
            [NotNull] CodeFixProvider fixProvider, [NotNull] SimpleDiagnosticProvider diagnosticProvider)
        {
            var fixAllContext = new FixAllContext(document, fixProvider, FixAllScope.Document, equivalenceKey,
                fixProvider.FixableDiagnosticIds, diagnosticProvider, CancellationToken.None);

            FixAllProvider fixAllProvider = fixProvider.GetFixAllProvider();
            return fixAllProvider.GetFixAsync(fixAllContext).Result;
        }

        private static void VerifyResultOfCodeFix([NotNull] CodeAction fixAction, [NotNull] Document document,
            [NotNull] FixProviderTestContext context, [NotNull] string expectedCode)
        {
            bool formatOutputDocument = context.CodeComparisonMode == TextComparisonMode.IgnoreWhitespaceDifferences;

            string actualCode = ApplyCodeAction(fixAction, document, formatOutputDocument);
            actualCode.Should().Be(expectedCode);
        }

        [NotNull]
        private static string ApplyCodeAction([NotNull] CodeAction codeAction, [NotNull] Document document,
            bool formatOutputDocument)
        {
            ImmutableArray<CodeActionOperation> operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;

            operations.Should().HaveCount(1);

            CodeActionOperation operation = operations.Single();
            return GetTextForOperation(document, operation, formatOutputDocument);
        }

        [NotNull]
        private static string GetTextForOperation([NotNull] Document document, [NotNull] CodeActionOperation operation,
            bool formatOutputDocument)
        {
            Document newDocument = ApplyOperationToDocument(operation, document);

            return formatOutputDocument
                ? DocumentFactory.FormatDocument(newDocument)
                : newDocument.GetTextAsync().Result.ToString();
        }

        [NotNull]
        private static Document ApplyOperationToDocument([NotNull] CodeActionOperation operation, [NotNull] Document document)
        {
            Workspace workspace = document.Project.Solution.Workspace;
            operation.Apply(workspace, CancellationToken.None);

            return workspace.CurrentSolution.GetDocument(document.Id);
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

        private sealed class SimpleDiagnosticProvider : FixAllContext.DiagnosticProvider
        {
            [NotNull]
            [ItemNotNull]
            private readonly IEnumerable<Diagnostic> diagnostics;

            public SimpleDiagnosticProvider([NotNull] [ItemNotNull] IEnumerable<Diagnostic> diagnostics)
            {
                FrameworkGuard.NotNull(diagnostics, nameof(diagnostics));
                this.diagnostics = diagnostics;
            }

            [NotNull]
            [ItemNotNull]
            public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync([NotNull] Document document,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(diagnostics);
            }

            [NotNull]
            [ItemNotNull]
            public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync([NotNull] Project project,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(diagnostics);
            }

            [NotNull]
            [ItemNotNull]
            public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync([NotNull] Project project,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(diagnostics);
            }
        }
    }
}
