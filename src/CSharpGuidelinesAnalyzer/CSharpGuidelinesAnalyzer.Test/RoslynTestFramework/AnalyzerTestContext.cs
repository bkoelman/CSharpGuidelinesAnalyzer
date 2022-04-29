using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    public sealed class AnalyzerTestContext
    {
        private const string DefaultFileName = "TestDocument";
        private const string DefaultAssemblyName = "TestProject";
        private const DocumentationMode DefaultDocumentationMode = DocumentationMode.None;
        private const OutputKind DefaultOutputKind = OutputKind.DynamicallyLinkedLibrary;
        private const TestValidationMode DefaultTestValidationMode = TestValidationMode.AllowCompileWarnings;

        [NotNull]
        [ItemNotNull]
        private static readonly Lazy<ImmutableHashSet<MetadataReference>> DefaultReferencesLazy =
            new Lazy<ImmutableHashSet<MetadataReference>>(ResolveDefaultReferences, LazyThreadSafetyMode.PublicationOnly);

        [NotNull]
        public string SourceCode { get; }

        [NotNull]
        public IList<TextSpan> SourceSpans { get; }

        [NotNull]
        public string FileName { get; }

        [NotNull]
        public string AssemblyName { get; }

        [NotNull]
        [ItemNotNull]
        public ImmutableHashSet<MetadataReference> References { get; }

        public DocumentationMode DocumentationMode { get; }

        public OutputKind OutputKind { get; }

        public TreatWarningsAsErrors WarningsAsErrors { get; }

        public TestValidationMode ValidationMode { get; }

        [NotNull]
        public AnalyzerOptions Options { get; }

        public AnalyzerTestContext([NotNull] string sourceCode, [NotNull] IList<TextSpan> sourceSpans, [NotNull] AnalyzerOptions options)
            : this(sourceCode, sourceSpans, DefaultFileName, DefaultAssemblyName, DefaultReferencesLazy.Value, DefaultDocumentationMode, DefaultOutputKind,
                TreatWarningsAsErrors.None, DefaultTestValidationMode, options)
        {
            FrameworkGuard.NotNull(sourceCode, nameof(sourceCode));
            FrameworkGuard.NotNull(sourceSpans, nameof(sourceSpans));
            FrameworkGuard.NotNull(options, nameof(options));
        }

#pragma warning disable AV1561 // Signature contains too many parameters
        private AnalyzerTestContext([NotNull] string sourceCode, [NotNull] IList<TextSpan> sourceSpans, [NotNull] string fileName,
            [NotNull] string assemblyName, [NotNull] [ItemNotNull] ImmutableHashSet<MetadataReference> references, DocumentationMode documentationMode,
            OutputKind outputKind, TreatWarningsAsErrors warningsAsErrors, TestValidationMode validationMode, [NotNull] AnalyzerOptions options)
        {
            SourceCode = sourceCode;
            SourceSpans = sourceSpans;
            FileName = fileName;
            AssemblyName = assemblyName;
            References = references;
            DocumentationMode = documentationMode;
            OutputKind = outputKind;
            WarningsAsErrors = warningsAsErrors;
            ValidationMode = validationMode;
            Options = options;
        }
#pragma warning restore AV1561 // Signature contains too many parameters

        [NotNull]
        [ItemNotNull]
        private static ImmutableHashSet<MetadataReference> ResolveDefaultReferences()
        {
            string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            if (assemblyPath == null)
            {
                throw new InvalidOperationException("Failed to locate assembly for System.Object.");
            }

            // Bug workaround for test runner in VS2019, which fails to load ValueTask due to missing dependency.
            Assembly netStandardAssembly =
                Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");

            return ImmutableHashSet.Create(new MetadataReference[]
            {
                CreateReferenceFromFile(assemblyPath, "mscorlib.dll"),
                CreateReferenceFromFile(assemblyPath, "System.dll"),
                CreateReferenceFromFile(assemblyPath, "System.Core.dll"),
                CreateReferenceFromFile(assemblyPath, "System.Runtime.dll"),
                MetadataReference.CreateFromFile(netStandardAssembly.Location)
            });
        }

        [NotNull]
        private static PortableExecutableReference CreateReferenceFromFile([NotNull] string assemblyPath, [NotNull] string assemblyFileName)
        {
            FrameworkGuard.NotNull(assemblyPath, nameof(assemblyPath));
            FrameworkGuard.NotNull(assemblyFileName, nameof(assemblyFileName));

            string filePath = Path.Combine(assemblyPath, assemblyFileName);
            return MetadataReference.CreateFromFile(filePath);
        }

        [NotNull]
        public AnalyzerTestContext WithCode([NotNull] string sourceCode, [NotNull] IList<TextSpan> sourceSpans)
        {
            FrameworkGuard.NotNull(sourceCode, nameof(sourceCode));
            FrameworkGuard.NotNull(sourceSpans, nameof(sourceSpans));

            return new AnalyzerTestContext(sourceCode, sourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext InFileNamed([NotNull] string fileName)
        {
            FrameworkGuard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            return new AnalyzerTestContext(SourceCode, SourceSpans, fileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext InAssemblyNamed([NotNull] string assemblyName)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, assemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext WithReferences([NotNull] [ItemNotNull] IEnumerable<MetadataReference> references)
        {
            FrameworkGuard.NotNull(references, nameof(references));

            ImmutableHashSet<MetadataReference> referenceSet = references.ToImmutableHashSet();

            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName,
                referenceSet, DocumentationMode, OutputKind, WarningsAsErrors,
                ValidationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext WithDocumentationMode(DocumentationMode mode)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References, mode,
                OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext WithOutputKind(OutputKind outputKind)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, outputKind, WarningsAsErrors, ValidationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext CompileWithWarningsAsErrors(TreatWarningsAsErrors warningsAsErrors)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, warningsAsErrors, ValidationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext InValidationMode(TestValidationMode validationMode)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, validationMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext WithOptions([NotNull] AnalyzerOptions options)
        {
            FrameworkGuard.NotNull(options, nameof(options));

            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, options);
        }
    }
}
