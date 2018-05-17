using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using RoslynTestFramework;

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

            source.Editor.UpdateTestContext(context => context.WithReferences(context.References.Add(reference)));

            return source;
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
        public static TBuilder CompileAtWarningLevel<TBuilder>([NotNull] this TBuilder source, int warningLevel)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.Editor.UpdateTestContext(context => context.CompileAtWarningLevel(warningLevel));

            return source;
        }

        [NotNull]
        public static TBuilder CompileWithWarningAsError<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.Editor.UpdateTestContext(context => context.CompileWithWarningAsError(true));

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
        public static TBuilder AllowingDiagnosticsOutsideSourceTree<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source.Editor.UpdateTestContext(context => context.AllowingDiagnosticsOutsideSourceTree());

            return source;
        }
    }
}
