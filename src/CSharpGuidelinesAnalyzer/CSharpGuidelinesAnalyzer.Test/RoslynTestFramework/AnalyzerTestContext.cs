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

        [NotNull]
        [ItemNotNull]
        private static readonly ImmutableList<MetadataReference> DefaultReferences =
            ImmutableList.Create(new MetadataReference[]
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
        [ItemNotNull]
        public ImmutableList<MetadataReference> References { get; }

        [CanBeNull]
        public AnalyzerOptions Options { get; }

        private AnalyzerTestContext([NotNull] string markupCode, [NotNull] string languageName,
            [NotNull] string fileName, [CanBeNull] AnalyzerOptions options,
            [NotNull] [ItemNotNull] ImmutableList<MetadataReference> references)
        {
            MarkupCode = markupCode;
            LanguageName = languageName;
            FileName = fileName;
            Options = options;
            References = references;
        }

        public AnalyzerTestContext([NotNull] string markupCode, [NotNull] string languageName,
            [CanBeNull] AnalyzerOptions options)
            : this(markupCode, languageName, DefaultFileName, options, DefaultReferences)
        {
            Guard.NotNull(markupCode, nameof(markupCode));
            Guard.NotNull(languageName, nameof(languageName));
        }

        [NotNull]
        public AnalyzerTestContext WithFileName([NotNull] string fileName)
        {
            Guard.NotNull(fileName, nameof(fileName));

            return new AnalyzerTestContext(MarkupCode, LanguageName, fileName, Options, References);
        }

        [NotNull]
        public AnalyzerTestContext WithReferences([NotNull] [ItemNotNull] IEnumerable<MetadataReference> references)
        {
            Guard.NotNull(references, nameof(references));

            ImmutableList<MetadataReference> referenceList = ImmutableList.CreateRange(references);
            return new AnalyzerTestContext(MarkupCode, LanguageName, FileName, Options, referenceList);
        }
    }
}