using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using CSharpGuidelinesAnalyzer.Utilities;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
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

        [NotNull]
        protected abstract CodeFixProvider CreateFixProvider();

        protected void AssertDiagnostics([NotNull] AnalyzerTestContext context)
        {
            Guard.NotNull(context, nameof(context));

            RunDiagnostics(context);
        }

        [ItemNotNull]
        private ImmutableArray<Diagnostic> RunDiagnostics([NotNull] AnalyzerTestContext context)
        {
            DocumentWithSpans documentWithSpans = TestHelpers.GetDocumentAndSpansFromMarkup(context.MarkupCode,
                context.LanguageName, context.References, context.FileName);

            ImmutableArray<Diagnostic> diagnostics =
                GetDiagnosticsForDocument(documentWithSpans.Document, context.Options)
                    .OrderBy(d => d.Location.SourceSpan)
                    .Where(d => d.Id == DiagnosticId)
                    .ToImmutableArray();
            ImmutableArray<TextSpan> spans = documentWithSpans.TextSpans.OrderBy(s => s).ToImmutableArray();

            diagnostics.Should().HaveCount(spans.Length);

            for (int index = 0; index < diagnostics.Length; index++)
            {
                Diagnostic diagnostic = diagnostics[index];
                TextSpan span = spans[index];

                diagnostic.Location.IsInSource.Should().BeTrue();
                diagnostic.Location.SourceSpan.Should().Be(span);
            }

            return diagnostics;
        }

        [ItemNotNull]
        private ImmutableArray<Diagnostic> GetDiagnosticsForDocument([NotNull] Document document,
            [CanBeNull] AnalyzerOptions options)
        {
            ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray.Create(CreateAnalyzer());
            Compilation compilation = document.Project.GetCompilationAsync().Result;
            CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, options,
                CancellationToken.None);

            ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(CancellationToken.None);
            ValidateCompilerDiagnostics(compilerDiagnostics);

            SyntaxTree tree = document.GetSyntaxTreeAsync().Result;

            ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();
            foreach (Diagnostic analyzerDiagnostic in compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result)
            {
                Location location = analyzerDiagnostic.Location;
                if (location.IsInSource && location.SourceTree == tree)
                {
                    builder.Add(analyzerDiagnostic);
                }
            }

            return builder.ToImmutable();
        }

        private void ValidateCompilerDiagnostics([ItemNotNull] ImmutableArray<Diagnostic> compilerDiagnostics)
        {
            bool hasErrors = compilerDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
            hasErrors.Should().BeFalse();
        }

        protected void AssertDiagnosticsWithCodeFixes([NotNull] FixProviderTestContext context)
        {
            Guard.NotNull(context, nameof(context));

            ImmutableArray<Diagnostic> diagnostics = RunDiagnostics(context.AnalyzerTestContext);

            IList<string> expectedCode = TestHelpers.RemoveMarkupFrom(context.Expected,
                context.AnalyzerTestContext.LanguageName, context.ReformatExpected,
                context.AnalyzerTestContext.References, context.AnalyzerTestContext.FileName);
            context = context.WithExpected(expectedCode);

            CodeFixProvider fixProvider = CreateFixProvider();
            foreach (Diagnostic diagnostic in diagnostics)
            {
                RunCodeFixes(context, diagnostic, fixProvider);
            }
        }

        private void RunCodeFixes([NotNull] FixProviderTestContext context, [NotNull] Diagnostic diagnostic,
            [NotNull] CodeFixProvider fixProvider)
        {
            for (int index = 0; index < context.Expected.Count; index++)
            {
                Document document =
                    TestHelpers.GetDocumentAndSpansFromMarkup(context.AnalyzerTestContext.MarkupCode,
                        context.AnalyzerTestContext.LanguageName, context.AnalyzerTestContext.References,
                        context.AnalyzerTestContext.FileName).Document;

                ImmutableArray<CodeAction> codeFixes = GetCodeFixesForDiagnostic(diagnostic, document, fixProvider);
                codeFixes.Should().HaveCount(context.Expected.Count);

                Verify.CodeAction(codeFixes[index], document, context.Expected[index]);
            }
        }

        [ItemNotNull]
        private ImmutableArray<CodeAction> GetCodeFixesForDiagnostic([NotNull] Diagnostic diagnostic,
            [NotNull] Document document, [NotNull] CodeFixProvider fixProvider)
        {
            ImmutableArray<CodeAction>.Builder builder = ImmutableArray.CreateBuilder<CodeAction>();
            Action<CodeAction, ImmutableArray<Diagnostic>> registerCodeFix = (a, _) => builder.Add(a);

            var context = new CodeFixContext(document, diagnostic, registerCodeFix, CancellationToken.None);
            fixProvider.RegisterCodeFixesAsync(context).Wait();

            return builder.ToImmutable();
        }
    }
}