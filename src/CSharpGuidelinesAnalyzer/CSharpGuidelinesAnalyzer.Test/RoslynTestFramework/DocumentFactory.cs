using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    /// <summary />
    internal static class DocumentFactory
    {
        [NotNull]
        private static readonly CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

        [NotNull]
        private static readonly CSharpParseOptions DefaultParseOptions = new CSharpParseOptions();

        [NotNull]
        public static Document ToDocument([NotNull] string code, [NotNull] AnalyzerTestContext context)
        {
            ParseOptions parseOptions = GetParseOptions(context.DocumentationMode);
            CompilationOptions compilationOptions = GetCompilationOptions(context);

            Document document = new AdhocWorkspace()
                .AddProject(context.AssemblyName, LanguageNames.CSharp)
                .WithParseOptions(parseOptions)
                .WithCompilationOptions(compilationOptions)
                .AddMetadataReferences(context.References)
                .AddDocument(context.FileName, code);

            return document;
        }

        [NotNull]
        private static ParseOptions GetParseOptions(DocumentationMode documentationMode)
        {
            return DefaultParseOptions.WithDocumentationMode(documentationMode);
        }

        [NotNull]
        private static CompilationOptions GetCompilationOptions([NotNull] AnalyzerTestContext context)
        {
            CompilationOptions options = DefaultCompilationOptions;

            options = options.WithOutputKind(context.OutputKind);

            if (context.WarningsAsErrors == TreatWarningsAsErrors.All)
            {
                options = options.WithGeneralDiagnosticOption(ReportDiagnostic.Error);
            }

            return options;
        }
    }
}
