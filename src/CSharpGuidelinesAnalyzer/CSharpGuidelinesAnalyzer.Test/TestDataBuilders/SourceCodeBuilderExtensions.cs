using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal static class SourceCodeBuilderExtensions
    {
        [NotNull]
        public static TBuilder Using<TBuilder>([NotNull] this TBuilder source, [CanBeNull] string codeNamespace)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            if (!string.IsNullOrWhiteSpace(codeNamespace))
            {
                source.Editor.IncludeNamespaceImport(codeNamespace);
            }

            return source;
        }

        [NotNull]
        public static TBuilder InFileNamed<TBuilder>([NotNull] this TBuilder source, [NotNull] string fileName)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            source.Editor.UpdateTestContext(context => context.InFileNamed(fileName));

            return source;
        }

        [NotNull]
        public static TBuilder InAssemblyNamed<TBuilder>([NotNull] this TBuilder source, [NotNull] string assemblyName)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNullNorWhiteSpace(assemblyName, nameof(assemblyName));

            source.Editor.UpdateTestContext(context => context.InAssemblyNamed(assemblyName));

            return source;
        }

        [NotNull]
        public static TBuilder WithReference<TBuilder>([NotNull] this TBuilder source, [NotNull] Assembly assembly)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(assembly, nameof(assembly));

            PortableExecutableReference reference = MetadataReference.CreateFromFile(assembly.Location);

            source.Editor.UpdateTestContext(context =>
            {
                ImmutableHashSet<MetadataReference> references = context.References.Add(reference);
                return context.WithReferences(references);
            });

            return source;
        }

        [NotNull]
        public static TBuilder WithReferenceToExternalAssemblyFor<TBuilder>([NotNull] this TBuilder source, [NotNull] string code)
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

        [NotNull]
        private static Stream GetInMemoryAssemblyStreamForCode([NotNull] string code, [NotNull] string assemblyName,
            [NotNull] [ItemNotNull] params MetadataReference[] references)
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

        private static void ValidateCompileErrors([NotNull] EmitResult emitResult)
        {
            Diagnostic[] compilerErrors = emitResult.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
            compilerErrors.Should().BeEmpty("external assembly should not have compile errors");
            emitResult.Success.Should().BeTrue();
        }

        [NotNull]
        public static TBuilder WithDocumentationComments<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.Editor.UpdateTestContext(context => context.WithDocumentationMode(DocumentationMode.Diagnose));

            return source;
        }

        [NotNull]
        public static TBuilder WithOutputKind<TBuilder>([NotNull] this TBuilder source, OutputKind outputKind)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.Editor.UpdateTestContext(context => context.WithOutputKind(outputKind));

            return source;
        }

        [NotNull]
        public static TBuilder CompileWithWarningAsError<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.Editor.UpdateTestContext(context => context.CompileWithWarningsAsErrors(TreatWarningsAsErrors.All));

            return source;
        }

        [NotNull]
        public static TBuilder AllowingCompileErrors<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.Editor.UpdateTestContext(context => context.InValidationMode(TestValidationMode.AllowCompileErrors));

            return source;
        }

        [NotNull]
        public static TBuilder WithOptions<TBuilder>([NotNull] this TBuilder source, [NotNull] AnalyzerOptionsBuilder builder)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(builder, nameof(builder));

            AnalyzerOptions options = builder.Build();
            source.Editor.UpdateTestContext(context => context.WithOptions(options));

            return source;
        }
    }
}
