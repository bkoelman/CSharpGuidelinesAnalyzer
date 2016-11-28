using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CSharpGuidelinesAnalyzer.Test.RoslynTestFramework;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal abstract class SourceCodeBuilder : ITestDataBuilder<ParsedSourceCode>
    {
        [NotNull]
        private AnalyzerTestContext testContext = new AnalyzerTestContext(string.Empty, LanguageNames.CSharp)
            .WithReferences(DefaultReferences);

        [NotNull]
        [ItemNotNull]
        private static readonly ImmutableHashSet<MetadataReference> DefaultReferences =
            ImmutableHashSet.Create(new MetadataReference[]
            {
                MsCorLibAssembly,
                SystemAssembly
            });

        [NotNull]
        private static PortableExecutableReference SystemAssembly
            => MetadataReference.CreateFromFile(typeof(Component).Assembly.Location);

        [NotNull]
        private static PortableExecutableReference MsCorLibAssembly
            => MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        /// <summary>
        /// Intended for internal use.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        internal readonly HashSet<string> NamespaceImports = new HashSet<string> { "System" };

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
            if (NamespaceImports.Any())
            {
                foreach (string namespaceImport in NamespaceImports)
                {
                    sourceBuilder.AppendLine($"using {namespaceImport};");
                }

                sourceBuilder.AppendLine();
            }
        }

        [NotNull]
        protected abstract string GetSourceCode();
    }
}
