using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    public sealed class AnalyzerTestContext
    {
        private const string DefaultFileName = "TestDocument";
        private const string DefaultAssemblyName = "TestProject";
        private const DocumentationMode DefaultDocumentationMode = DocumentationMode.None;
        private const OperationFeature DefaultOperationFeature = OperationFeature.Disabled;
        private const TestValidationMode DefaultTestValidationMode = TestValidationMode.AllowCompileWarnings;

        [NotNull]
        [ItemNotNull]
        private static readonly ImmutableHashSet<MetadataReference> DefaultReferences =
            ImmutableHashSet.Create(new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).GetTypeInfo().Assembly.Location)
            });

        [NotNull]
        private static readonly AnalyzerOptions DefaultAnalyzerOptions = new AnalyzerOptions(new ImmutableArray<AdditionalText>())
            ;

        [NotNull]
        public string MarkupCode { get; }

        [NotNull]
        public string LanguageName { get; }

        [NotNull]
        public string FileName { get; }

        [NotNull]
        public string AssemblyName { get; }

        [NotNull]
        [ItemNotNull]
#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type
        public ImmutableHashSet<MetadataReference> References { get; }
#pragma warning restore AV1130 // Return type in method signature should be a collection interface instead of a concrete type

        public DocumentationMode DocumentationMode { get; }

        public OperationFeature OperationFeature { get; }

        [CanBeNull]
        public int? CompilerWarningLevel { get; }

        public TestValidationMode ValidationMode { get; }

        public DiagnosticsCaptureMode DiagnosticsCaptureMode { get; }

        [NotNull]
        public AnalyzerOptions Options { get; }

#pragma warning disable AV1561 // Method or constructor contains more than three parameters
#pragma warning disable AV1500 // Member contains more than seven statements
        private AnalyzerTestContext([NotNull] string markupCode, [NotNull] string languageName, [NotNull] string fileName,
            [NotNull] string assemblyName, [NotNull] [ItemNotNull] ImmutableHashSet<MetadataReference> references,
            DocumentationMode documentationMode, OperationFeature operationFeature, [CanBeNull] int? compilerWarningLevel,
            TestValidationMode validationMode, DiagnosticsCaptureMode diagnosticsCaptureMode, [NotNull] AnalyzerOptions options)
        {
            MarkupCode = markupCode;
            LanguageName = languageName;
            FileName = fileName;
            AssemblyName = assemblyName;
            References = references;
            DocumentationMode = documentationMode;
            OperationFeature = operationFeature;
            CompilerWarningLevel = compilerWarningLevel;
            ValidationMode = validationMode;
            DiagnosticsCaptureMode = diagnosticsCaptureMode;
            Options = options;
        }
#pragma warning restore AV1500 // Member contains more than seven statements
#pragma warning restore AV1561 // Method or constructor contains more than three parameters

        public AnalyzerTestContext([NotNull] string markupCode, [NotNull] string languageName)
            : this(markupCode, languageName, DefaultFileName, DefaultAssemblyName, DefaultReferences, DefaultDocumentationMode,
                DefaultOperationFeature, null, DefaultTestValidationMode, DiagnosticsCaptureMode.RequireInSourceTree,
                DefaultAnalyzerOptions)
        {
            Guard.NotNull(markupCode, nameof(markupCode));
            Guard.NotNullNorWhiteSpace(languageName, nameof(languageName));
        }

        [NotNull]
        public AnalyzerTestContext WithMarkupCode([NotNull] string markupCode)
        {
            Guard.NotNull(markupCode, nameof(markupCode));

            return new AnalyzerTestContext(markupCode, LanguageName, FileName, AssemblyName, References, DocumentationMode,
                OperationFeature, CompilerWarningLevel, ValidationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext InFileNamed([NotNull] string fileName)
        {
            Guard.NotNullNorWhiteSpace(fileName, nameof(fileName));

            return new AnalyzerTestContext(MarkupCode, LanguageName, fileName, AssemblyName, References, DocumentationMode,
                OperationFeature, CompilerWarningLevel, ValidationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext InAssemblyNamed([NotNull] string assemblyName)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, assemblyName, References, DocumentationMode,
                OperationFeature, CompilerWarningLevel, ValidationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext WithReferences([NotNull] [ItemNotNull] IEnumerable<MetadataReference> references)
        {
            Guard.NotNull(references, nameof(references));

            ImmutableList<MetadataReference> referenceList = ImmutableList.CreateRange(references);

            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, referenceList.ToImmutableHashSet(),
                DocumentationMode, OperationFeature, CompilerWarningLevel, ValidationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext WithDocumentationMode(DocumentationMode mode)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References, mode, OperationFeature,
                CompilerWarningLevel, ValidationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext WithOperationFeature(OperationFeature feature)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References, DocumentationMode,
                feature, CompilerWarningLevel, ValidationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext CompileAtWarningLevel(int warningLevel)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References, DocumentationMode,
                OperationFeature, warningLevel, ValidationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext InValidationMode(TestValidationMode validationMode)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References, DocumentationMode,
                OperationFeature, CompilerWarningLevel, validationMode, DiagnosticsCaptureMode, Options);
        }

        [NotNull]
        public AnalyzerTestContext AllowingDiagnosticsOutsideSourceTree()
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References, DocumentationMode,
                OperationFeature, CompilerWarningLevel, ValidationMode, DiagnosticsCaptureMode.AllowOutsideSourceTree, Options);
        }
    }
}
