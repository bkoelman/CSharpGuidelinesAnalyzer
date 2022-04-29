using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
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

        private static readonly Lazy<ImmutableHashSet<MetadataReference>> DefaultReferencesLazy = new Lazy<ImmutableHashSet<MetadataReference>>(
            () => ReferenceAssemblies.Net.Net60.ResolveAsync(null, CancellationToken.None).Result.ToImmutableHashSet(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        public string SourceCode { get; }

        public IList<TextSpan> SourceSpans { get; }

        public string FileName { get; }

        public string AssemblyName { get; }

        public ImmutableHashSet<MetadataReference> References { get; }

        public DocumentationMode DocumentationMode { get; }

        public OutputKind OutputKind { get; }

        public TreatWarningsAsErrors WarningsAsErrors { get; }

        public TestValidationMode ValidationMode { get; }

        public AnalyzerOptions Options { get; }

        public AnalyzerTestContext(string sourceCode, IList<TextSpan> sourceSpans, AnalyzerOptions options)
            : this(sourceCode, sourceSpans, DefaultFileName, DefaultAssemblyName, DefaultReferencesLazy.Value, DefaultDocumentationMode, DefaultOutputKind,
                TreatWarningsAsErrors.None, DefaultTestValidationMode, options)
        {
            FrameworkGuard.NotNull(sourceCode, nameof(sourceCode));
            FrameworkGuard.NotNull(sourceSpans, nameof(sourceSpans));
            FrameworkGuard.NotNull(options, nameof(options));
        }

#pragma warning disable AV1561 // Signature contains too many parameters
        private AnalyzerTestContext(string sourceCode, IList<TextSpan> sourceSpans, string fileName, string assemblyName,
            ImmutableHashSet<MetadataReference> references, DocumentationMode documentationMode, OutputKind outputKind, TreatWarningsAsErrors warningsAsErrors,
            TestValidationMode validationMode, AnalyzerOptions options)
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

        public AnalyzerTestContext WithCode(string sourceCode, IList<TextSpan> sourceSpans)
        {
            FrameworkGuard.NotNull(sourceCode, nameof(sourceCode));
            FrameworkGuard.NotNull(sourceSpans, nameof(sourceSpans));

            return new AnalyzerTestContext(sourceCode, sourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        public AnalyzerTestContext InFileNamed(string fileName)
        {
            FrameworkGuard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            return new AnalyzerTestContext(SourceCode, SourceSpans, fileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        public AnalyzerTestContext InAssemblyNamed(string assemblyName)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, assemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        public AnalyzerTestContext WithReferences(IEnumerable<MetadataReference> references)
        {
            FrameworkGuard.NotNull(references, nameof(references));

            ImmutableHashSet<MetadataReference> referenceSet = references.ToImmutableHashSet();

            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName,
                referenceSet, DocumentationMode, OutputKind, WarningsAsErrors,
                ValidationMode, Options);
        }

        public AnalyzerTestContext WithDocumentationMode(DocumentationMode mode)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References, mode,
                OutputKind, WarningsAsErrors, ValidationMode, Options);
        }

        public AnalyzerTestContext WithOutputKind(OutputKind outputKind)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, outputKind, WarningsAsErrors, ValidationMode, Options);
        }

        public AnalyzerTestContext CompileWithWarningsAsErrors(TreatWarningsAsErrors warningsAsErrors)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, warningsAsErrors, ValidationMode, Options);
        }

        public AnalyzerTestContext InValidationMode(TestValidationMode validationMode)
        {
            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, validationMode, Options);
        }

        public AnalyzerTestContext WithOptions(AnalyzerOptions options)
        {
            FrameworkGuard.NotNull(options, nameof(options));

            return new AnalyzerTestContext(SourceCode, SourceSpans, FileName, AssemblyName, References,
                DocumentationMode, OutputKind, WarningsAsErrors, ValidationMode, options);
        }
    }
}
