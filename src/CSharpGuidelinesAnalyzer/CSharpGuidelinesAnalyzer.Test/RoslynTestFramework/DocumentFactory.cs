using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;

/// <summary />
internal static class DocumentFactory
{
    private static readonly CSharpCompilationOptions DefaultCompilationOptions = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
    private static readonly CSharpParseOptions DefaultParseOptions = new();

    public static Document ToDocument(string code, AnalyzerTestContext context)
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

    private static ParseOptions GetParseOptions(DocumentationMode documentationMode)
    {
        return DefaultParseOptions.WithDocumentationMode(documentationMode);
    }

    private static CompilationOptions GetCompilationOptions(AnalyzerTestContext context)
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
