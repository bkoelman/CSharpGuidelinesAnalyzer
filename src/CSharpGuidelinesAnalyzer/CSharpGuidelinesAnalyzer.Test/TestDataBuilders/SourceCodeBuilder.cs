using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    public abstract class SourceCodeBuilder : ITestDataBuilder<ParsedSourceCode>
    {
        [NotNull]
        [ItemNotNull]
        private readonly HashSet<string> namespaceImports = new HashSet<string> { "System" };

        [CanBeNull]
        private string headerText;

        [NotNull]
        private string sourceFilename = DefaultFilename;

        [NotNull]
        [ItemNotNull]
        private readonly HashSet<MetadataReference> references = new HashSet<MetadataReference>(DefaultReferences);

        [NotNull]
        private string codeNamespaceImportExpected = string.Empty;

        private TestValidationMode validationMode;

        [NotNull]
        protected abstract string GetSourceCode();

        public const string DefaultFilename = "Test.cs";

        [NotNull]
        [ItemNotNull]
        public static readonly ImmutableHashSet<MetadataReference> DefaultReferences =
            ImmutableHashSet.Create(new MetadataReference[]
            {
                /* mscorlib.dll */
                MetadataReference.CreateFromFile(typeof (object).Assembly.Location),
                /* System.dll */
                MetadataReference.CreateFromFile(typeof (Component).Assembly.Location)
            });

        public ParsedSourceCode Build()
        {
            string sourceText = GetCompleteSourceText();

            IList<string> nestedTypes = new string[0];

            return new ParsedSourceCode(sourceText, sourceFilename, ImmutableHashSet.Create(references.ToArray()),
                nestedTypes, codeNamespaceImportExpected, true, validationMode);
        }

        [NotNull]
        private string GetCompleteSourceText()
        {
            var sourceBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(headerText))
            {
                sourceBuilder.AppendLine(headerText);
            }

            bool hasNamespaceImportsAtTop = false;
            foreach (string ns in namespaceImports)
            {
                sourceBuilder.AppendLine($"using {ns};");
                hasNamespaceImportsAtTop = true;
            }

            if (!string.IsNullOrEmpty(codeNamespaceImportExpected))
            {
                sourceBuilder.AppendLine("<import/>");
                hasNamespaceImportsAtTop = true;
            }

            if (hasNamespaceImportsAtTop)
            {
                sourceBuilder.AppendLine();
            }

            sourceBuilder.Append(GetSourceCode());

            return sourceBuilder.ToString();
        }

        internal void _Using([NotNull] string codeNamespace)
        {
            namespaceImports.Add(codeNamespace);
        }

        internal void _WithReference([NotNull] Assembly assembly)
        {
            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        internal void _WithReference([NotNull] MetadataReference reference)
        {
            references.Add(reference);
        }

        internal void _Named([NotNull] string filename)
        {
            sourceFilename = filename;
        }

        internal void _ExpectingImportForNamespace([NotNull] string codeNamespace)
        {
            codeNamespaceImportExpected = codeNamespace;
        }

        internal void _AllowingCompileErrors()
        {
            validationMode = TestValidationMode.AllowCompileErrors;
        }

        internal void _WithHeader([NotNull] string text)
        {
            headerText = text;
        }
    }

    public static class SourceCodeBuilderExtensions
    {
        [NotNull]
        public static TBuilder Using<TBuilder>([NotNull] this TBuilder source, [CanBeNull] string codeNamespace)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            if (!string.IsNullOrWhiteSpace(codeNamespace))
            {
                source._Using(codeNamespace);
            }

            return source;
        }

        [NotNull]
        public static TBuilder WithHeader<TBuilder>([NotNull] this TBuilder source, [NotNull] string text)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(text, nameof(text));

            source._WithHeader(text);
            return source;
        }

        [NotNull]
        public static TBuilder WithReference<TBuilder>([NotNull] this TBuilder source, [NotNull] Assembly assembly)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(assembly, nameof(assembly));

            source._WithReference(assembly);
            return source;
        }

        [NotNull]
        public static TBuilder WithReferenceToExternalAssemblyFor<TBuilder>([NotNull] this TBuilder source,
            [NotNull] string code) where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(code, nameof(code));

            MetadataReference reference = GetInMemoryAssemblyReferenceForCode(code);
            source._WithReference(reference);
            return source;
        }

        [NotNull]
        private static MetadataReference GetInMemoryAssemblyReferenceForCode([NotNull] string code,
            [NotNull] [ItemNotNull] params MetadataReference[] references)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
            CSharpCompilation compilation =
                CSharpCompilation.Create("TempAssembly", new[] { tree })
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            PortableExecutableReference msCorLib = MetadataReference.CreateFromFile(typeof (object).Assembly.Location);
            compilation = compilation.AddReferences(msCorLib);
            compilation = compilation.AddReferences(references);

            return compilation.ToMetadataReference();
        }

        [NotNull]
        public static TBuilder Named<TBuilder>([NotNull] this TBuilder source, [NotNull] string filename)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(filename, nameof(filename));

            source._Named(filename);
            return source;
        }

        [NotNull]
        public static TBuilder ExpectingImportForNamespace<TBuilder>([NotNull] this TBuilder source,
            [NotNull] string expectedImportText) where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(expectedImportText, nameof(expectedImportText));

            source._ExpectingImportForNamespace(expectedImportText);
            return source;
        }

        [NotNull]
        public static TBuilder AllowingCompileErrors<TBuilder>([NotNull] this TBuilder source)
            where TBuilder : SourceCodeBuilder
        {
            Guard.NotNull(source, nameof(source));

            source._AllowingCompileErrors();
            return source;
        }
    }
}