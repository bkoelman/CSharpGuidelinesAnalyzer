using System.Collections.Immutable;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders;

/// <summary />
internal static class SourceCodeBuilderExtensions
{
    public static TBuilder Using<TBuilder>(this TBuilder source, string? codeNamespace)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));

        if (!string.IsNullOrWhiteSpace(codeNamespace))
        {
            source.Editor.IncludeNamespaceImport(codeNamespace);
        }

        return source;
    }

    public static TBuilder InFileNamed<TBuilder>(this TBuilder source, string fileName)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

        source.Editor.UpdateTestContext(context => context.InFileNamed(fileName));

        return source;
    }

    public static TBuilder InAssemblyNamed<TBuilder>(this TBuilder source, string assemblyName)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNullNorWhiteSpace(assemblyName, nameof(assemblyName));

        source.Editor.UpdateTestContext(context => context.InAssemblyNamed(assemblyName));

        return source;
    }

    public static TBuilder WithReferenceToExternalAssemblyFor<TBuilder>(this TBuilder source, string code)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));
        Guard.NotNull(code, nameof(code));

        Stream assemblyStream = GetInMemoryAssemblyStreamForCode(code, "TempAssembly");
        PortableExecutableReference reference = MetadataReference.CreateFromStream(assemblyStream);

        source.Editor.UpdateTestContext(context =>
        {
            ImmutableHashSet<MetadataReference> references = context.References.Add(reference);
            return context.WithReferences(references);
        });

        return source;
    }

    private static Stream GetInMemoryAssemblyStreamForCode(string code, string assemblyName, params MetadataReference[] references)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
        ImmutableArray<SyntaxTree> trees = ImmutableArray.Create(tree);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, trees).WithOptions(options);
        compilation = compilation.AddReferences(SourceCodeBuilder.DefaultTestContext.References);
        compilation = compilation.AddReferences(references);

        var stream = new MemoryStream();

        EmitResult emitResult = compilation.Emit(stream);
        ValidateCompileErrors(emitResult);

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    private static void ValidateCompileErrors(EmitResult emitResult)
    {
        Diagnostic[] compilerErrors = emitResult.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        compilerErrors.Should().BeEmpty("external assembly should not have compile errors");
        emitResult.Success.Should().BeTrue();
    }

    public static TBuilder WithDocumentationComments<TBuilder>(this TBuilder source)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));

        source.Editor.UpdateTestContext(context => context.WithDocumentationMode(DocumentationMode.Diagnose));

        return source;
    }

    public static TBuilder WithOutputKind<TBuilder>(this TBuilder source, OutputKind outputKind)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));

        source.Editor.UpdateTestContext(context => context.WithOutputKind(outputKind));

        return source;
    }

    public static TBuilder CompileWithWarningAsError<TBuilder>(this TBuilder source)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));

        source.Editor.UpdateTestContext(context => context.CompileWithWarningsAsErrors(TreatWarningsAsErrors.All));

        return source;
    }

    public static TBuilder AllowingCompileErrors<TBuilder>(this TBuilder source)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(source, nameof(source));

        source.Editor.UpdateTestContext(context => context.InValidationMode(TestValidationMode.AllowCompileErrors));

        return source;
    }

    public static TBuilder WithOptions<TBuilder>(this TBuilder source, AnalyzerOptionsBuilder builder)
        where TBuilder : SourceCodeBuilder
    {
        Guard.NotNull(builder, nameof(builder));

        AnalyzerOptions options = builder.Build();
        source.Editor.UpdateTestContext(context => context.WithOptions(options));

        return source;
    }
}
