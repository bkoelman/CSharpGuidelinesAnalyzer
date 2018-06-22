using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RoslynTestFramework;

namespace CSharpGuidelinesAnalyzer.Test.TestDataBuilders
{
    /// <summary />
    internal abstract class SourceCodeBuilder : ITestDataBuilder<ParsedSourceCode>
    {
        [NotNull]
        private static readonly AnalyzerTestContext DefaultTestContext = new AnalyzerTestContext(string.Empty,
            Array.Empty<TextSpan>(), LanguageNames.CSharp, new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty));

        [NotNull]
        private AnalyzerTestContext testContext = DefaultTestContext;

        [NotNull]
        [ItemNotNull]
        private readonly HashSet<string> namespaceImports;

        [NotNull]
        internal readonly CodeEditor Editor;

        [ItemNotNull]
        protected static readonly ImmutableArray<string> DefaultNamespaceImports = new[]
        {
            "System"
        }.ToImmutableArray();

        protected SourceCodeBuilder([NotNull] [ItemNotNull] IEnumerable<string> implicitNamespaceImports)
        {
            namespaceImports = new HashSet<string>(implicitNamespaceImports);
            Editor = new CodeEditor(this);
        }

        public ParsedSourceCode Build()
        {
            string sourceText = GetCompleteSourceText();

            return new ParsedSourceCode(sourceText, testContext);
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

        [NotNull]
        protected string GetLinesOfCode([NotNull] [ItemNotNull] IEnumerable<string> codeBlocks)
        {
            Guard.NotNull(codeBlocks, nameof(codeBlocks));

            var builder = new StringBuilder();
            AppendCodeBlocks(codeBlocks, builder);

            return builder.ToString();
        }

        private static void AppendCodeBlocks([NotNull] [ItemNotNull] IEnumerable<string> codeBlocks,
            [NotNull] StringBuilder builder)
        {
            bool isInFirstBlock = true;
            foreach (string codeBlock in codeBlocks)
            {
                if (isInFirstBlock)
                {
                    isInFirstBlock = false;
                }
                else
                {
                    builder.AppendLine();
                }

                AppendCodeBlock(codeBlock.TrimEnd(), builder);
            }
        }

        private static void AppendCodeBlock([NotNull] string codeBlock, [NotNull] StringBuilder builder)
        {
            bool isOnFirstLineInBlock = true;
            foreach (string line in GetLinesInText(codeBlock))
            {
                if (isOnFirstLineInBlock)
                {
                    if (line.Trim().Length == 0)
                    {
                        continue;
                    }

                    isOnFirstLineInBlock = false;
                }

                builder.AppendLine(line);
            }
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> GetLinesInText([NotNull] string text)
        {
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        internal sealed class CodeEditor
        {
            [NotNull]
            private readonly SourceCodeBuilder owner;

            public CodeEditor([NotNull] SourceCodeBuilder owner)
            {
                Guard.NotNull(owner, nameof(owner));
                this.owner = owner;
            }

            public void UpdateTestContext([NotNull] Func<AnalyzerTestContext, AnalyzerTestContext> change)
            {
                Guard.NotNull(change, nameof(change));

                owner.testContext = change(owner.testContext);
            }

            public void IncludeNamespaceImport([NotNull] string codeNamespace)
            {
                owner.namespaceImports.Add(codeNamespace);
            }
        }
    }
}
