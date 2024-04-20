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

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        return new AdhocWorkspace()
            .AddProject(context.AssemblyName, LanguageNames.CSharp)
            .WithParseOptions(parseOptions)
            .WithCompilationOptions(compilationOptions)
            .AddMetadataReferences(context.References)
            .AddDocument(context.FileName, code);

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore
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
