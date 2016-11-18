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

            source.TestContext = source.TestContext.InFileNamed(filename);
            return source;
        }

        [NotNull]
        public static TBuilder InAssemblyNamed<TBuilder>([NotNull] this TBuilder source, [NotNull] string assemblyName)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNullNorWhiteSpace(assemblyName, nameof(assemblyName));

            source.TestContext = source.TestContext.InAssemblyNamed(assemblyName);
            return source;
        }

        [NotNull]
        public static TBuilder WithReference<TBuilder>([NotNull] this TBuilder source, [NotNull] Assembly assembly)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(assembly, nameof(assembly));

            PortableExecutableReference reference = MetadataReference.CreateFromFile(assembly.Location);
            source.TestContext = source.TestContext.WithReferences(source.TestContext.References.Add(reference));

            return source;
        }

        [NotNull]
        public static TBuilder AllowingCompileErrors<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.TestContext = source.TestContext.InValidationMode(TestValidationMode.AllowCompileErrors);
            return source;
        }

        [NotNull]
        public static TBuilder CompileAtWarningLevel<TBuilder>([NotNull] this TBuilder source, int warningLevel)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.TestContext = source.TestContext.CompileAtWarningLevel(warningLevel);
            return source;
        }

        [NotNull]
        public static TBuilder AllowingDiagnosticsOutsideSourceTree<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.TestContext = source.TestContext.AllowingDiagnosticsOutsideSourceTree();
            return source;
        }
    }
}