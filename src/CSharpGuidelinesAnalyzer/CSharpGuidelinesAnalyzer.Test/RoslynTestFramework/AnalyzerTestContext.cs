using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    public sealed class AnalyzerTestContext
    {
        private const string DefaultFileName = "TestDocument";
        private const string DefaultAssemblyName = "TestProject";
        private const TestValidationMode DefaultTestValidationMode = TestValidationMode.AllowCompileWarnings;

        [NotNull]
        [ItemNotNull]
        private static readonly ImmutableHashSet<MetadataReference> DefaultReferences =
            ImmutableHashSet.Create(new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof (object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof (Enumerable).GetTypeInfo().Assembly.Location)
            });

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
        public ImmutableHashSet<MetadataReference> References { get; }

        [CanBeNull]
        public int? CompilerWarningLevel { get; }

        public TestValidationMode ValidationMode { get; }

        public bool DiagnosticsMustBeInSourceTree { get; }

        private AnalyzerTestContext([NotNull] string markupCode, [NotNull] string languageName,
            [NotNull] string fileName, [NotNull] string assemblyName,
            [NotNull] [ItemNotNull] ImmutableHashSet<MetadataReference> references,
            [CanBeNull] int? compilerWarningLevel, TestValidationMode validationMode, bool diagnosticsMustBeInSourceTree)
        {
            MarkupCode = markupCode;
            LanguageName = languageName;
            FileName = fileName;
            AssemblyName = assemblyName;
            References = references;
            CompilerWarningLevel = compilerWarningLevel;
            ValidationMode = validationMode;
            DiagnosticsMustBeInSourceTree = diagnosticsMustBeInSourceTree;
        }

        public AnalyzerTestContext([NotNull] string markupCode, [NotNull] string languageName)
            : this(
                markupCode, languageName, DefaultFileName, DefaultAssemblyName, DefaultReferences, null,
                DefaultTestValidationMode, true)
        {
            Guard.NotNull(markupCode, nameof(markupCode));
            Guard.NotNull(languageName, nameof(languageName));
        }

        [NotNull]
        public AnalyzerTestContext WithMarkupCode([NotNull] string markupCode)
        {
            Guard.NotNull(markupCode, nameof(markupCode));

            return new AnalyzerTestContext(markupCode, LanguageName, FileName, AssemblyName, References,
                CompilerWarningLevel, ValidationMode, DiagnosticsMustBeInSourceTree);
        }

        [NotNull]
        public AnalyzerTestContext InFileNamed([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            return new AnalyzerTestContext(MarkupCode, LanguageName, fileName, AssemblyName, References,
                CompilerWarningLevel, ValidationMode, DiagnosticsMustBeInSourceTree);
        }

        [NotNull]
        public AnalyzerTestContext InAssemblyNamed([NotNull] string assemblyName)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, assemblyName, References,
                CompilerWarningLevel, ValidationMode, DiagnosticsMustBeInSourceTree);
        }

        [NotNull]
        public AnalyzerTestContext WithReferences([NotNull] [ItemNotNull] IEnumerable<MetadataReference> references)
        {
            Guard.NotNull(references, nameof(references));

            ImmutableList<MetadataReference> referenceList = ImmutableList.CreateRange(references);

            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName,
                referenceList.ToImmutableHashSet(), CompilerWarningLevel, ValidationMode, DiagnosticsMustBeInSourceTree);
        }

        [NotNull]
        public AnalyzerTestContext CompileAtWarningLevel(int warningLevel)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References, warningLevel,
                ValidationMode, DiagnosticsMustBeInSourceTree);
        }

        [NotNull]
        public AnalyzerTestContext InValidationMode(TestValidationMode validationMode)
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References,
                CompilerWarningLevel, validationMode, DiagnosticsMustBeInSourceTree);
        }

        [NotNull]
        public AnalyzerTestContext AllowingDiagnosticsOutsideSourceTree()
        {
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, AssemblyName, References,
                CompilerWarningLevel, ValidationMode, false);
        }
    }
}