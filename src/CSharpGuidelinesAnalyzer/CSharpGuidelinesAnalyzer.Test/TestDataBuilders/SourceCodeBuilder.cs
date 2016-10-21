using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    public abstract class SourceCodeBuilder : ITestDataBuilder<ParsedSourceCode>
    {
        [NotNull]
        private AnalyzerTestContext testContext = new AnalyzerTestContext(string.Empty, LanguageNames.CSharp)
            .WithReferences(DefaultReferences);

        [NotNull]
        [ItemNotNull]
        private static readonly ImmutableHashSet<MetadataReference> DefaultReferences =
            ImmutableHashSet.Create(new MetadataReference[]
            {
                /* mscorlib.dll */
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location),
                /* System.dll */
                MetadataReference.CreateFromFile(typeof (Component).Assembly.Location)
            });

        [NotNull]
        [ItemNotNull]
        private readonly HashSet<string> namespaceImports = new HashSet<string> { "System" };

        [NotNull]
        public AnalyzerTestContext TestContext
        {
            get
            {
                return testContext;
            }
            set
            {
                Guard.NotNull(value, nameof(value));
                testContext = value;
            }
        }

        public ParsedSourceCode Build()
        {
            string sourceText = GetCompleteSourceText();
            TestContext = TestContext.WithMarkupCode(sourceText);

            return new ParsedSourceCode(TestContext);
        }

        [NotNull]
        private string GetCompleteSourceText()
        {
            var sourceBuilder = new StringBuilder();

            WriteNamespaceImports(sourceBuilder);

            sourceBuilder.Append(GetSourceCode());

            return sourceBuilder.ToString();
        }

        private void WriteNamespaceImports([NotNull] StringBuilder sourceBuilder)
        {
            if (namespaceImports.Any())
            {
                foreach (string namespaceImport in namespaceImports)
                {
                    sourceBuilder.AppendLine($"using {namespaceImport};");
                }

                sourceBuilder.AppendLine();
            }
        }

        [NotNull]
        protected abstract string GetSourceCode();

        internal void _Using([NotNull] string codeNamespace)
        {
            namespaceImports.Add(codeNamespace);
        }
    }

    public static class SourceCodeBuilderExtensions
    {
        [NotNull]
        public static TBuilder Using<TBuilder>([NotNull] this TBuilder source, [CanBeNull] string codeNamespace)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNullNorWhiteSpace(codeNamespace, nameof(codeNamespace));

            source._Using(codeNamespace);
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