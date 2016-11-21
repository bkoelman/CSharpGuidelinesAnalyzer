using System.Collections.Immutable;
using System.Reflection;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

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
            Guard.NotNullNorWhiteSpace(codeNamespace, nameof(codeNamespace));

            source.NamespaceImports.Add(codeNamespace);
            return source;
        }

        [NotNull]
        public static TBuilder InFileNamed<TBuilder>([NotNull] this TBuilder source, [NotNull] string filename)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNullNorWhiteSpace(filename, nameof(filename));

            return source.UpdateTestContext(source.TestContext.InFileNamed(filename));
        }

        [NotNull]
        public static TBuilder InAssemblyNamed<TBuilder>([NotNull] this TBuilder source, [NotNull] string assemblyName)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNullNorWhiteSpace(assemblyName, nameof(assemblyName));

            return source.UpdateTestContext(source.TestContext.InAssemblyNamed(assemblyName));
        }

        [NotNull]
        public static TBuilder WithReference<TBuilder>([NotNull] this TBuilder source, [NotNull] Assembly assembly)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(assembly, nameof(assembly));

            PortableExecutableReference reference = MetadataReference.CreateFromFile(assembly.Location);
            ImmutableHashSet<MetadataReference> references = source.TestContext.References.Add(reference);

            return source.UpdateTestContext(source.TestContext.WithReferences(references));
        }

        [NotNull]
        public static TBuilder WithDocumentationComments<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            return source.UpdateTestContext(source.TestContext.WithDocumentationMode(DocumentationMode.Diagnose));
        }

        [NotNull]
        public static TBuilder AllowingCompileErrors<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            return source.UpdateTestContext(source.TestContext.InValidationMode(TestValidationMode.AllowCompileErrors));
        }

        [NotNull]
        public static TBuilder CompileAtWarningLevel<TBuilder>([NotNull] this TBuilder source, int warningLevel)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            return source.UpdateTestContext(source.TestContext.CompileAtWarningLevel(warningLevel));
        }

        [NotNull]
        public static TBuilder AllowingDiagnosticsOutsideSourceTree<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            return source.UpdateTestContext(source.TestContext.AllowingDiagnosticsOutsideSourceTree());
        }

        [NotNull]
        private static TBuilder UpdateTestContext<TBuilder>([NotNull] this TBuilder source,
            [NotNull] AnalyzerTestContext testContext)
            where TBuilder : SourceCodeBuilder
        {
            source.TestContext = testContext;
            return source;
        }
    }
}