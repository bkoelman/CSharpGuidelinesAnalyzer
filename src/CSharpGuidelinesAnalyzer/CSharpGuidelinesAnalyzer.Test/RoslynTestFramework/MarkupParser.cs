using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    /// <summary />
    internal class MarkupParser
    {
        [NotNull]
        private static readonly CSharpCompilationOptions DefaultCompilationOptions =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

        [NotNull]
        private static readonly CSharpParseOptions DefaultParseOptions =
            new CSharpParseOptions().WithFeatures(new[] { new KeyValuePair<string, string>("IOperation", "true") });

        [NotNull]
        public DocumentWithSpans GetDocumentWithSpansFromMarkup([NotNull] AnalyzerTestContext context)
        {
            Guard.NotNull(context, nameof(context));

            string code;
            IList<TextSpan> spans;
            GetCodeWithSpansFromMarkup(context.MarkupCode, out code, out spans);

            Document document = GetDocument(code, context.LanguageName, context.FileName, context.AssemblyName,
                context.References, context.CompilerWarningLevel);
            return new DocumentWithSpans(document, spans);
        }

        private static void GetCodeWithSpansFromMarkup([NotNull] string markupCode, [NotNull] out string code,
            [NotNull] out IList<TextSpan> spans)
        {
            var codeBuilder = new StringBuilder();
            var textSpans = new List<TextSpan>();

            int offset = 0;
            int start = markupCode.IndexOf("[|", offset, StringComparison.Ordinal);
            while (start != -1)
            {
                codeBuilder.Append(markupCode.Substring(offset, start - offset));

                int end = markupCode.IndexOf("|]", start + 2, StringComparison.Ordinal);
                if (end == -1)
                {
                    throw new Exception("Missing |] in source.");
                }

                codeBuilder.Append(markupCode.Substring(start + 2, end - start - 2));

                int shift = textSpans.Count * 4;
                textSpans.Add(TextSpan.FromBounds(start - shift, end - 2 - shift));

                offset = end + 2;
                start = markupCode.IndexOf("[|", offset, StringComparison.Ordinal);
            }

            int extra = markupCode.IndexOf("|]", offset, StringComparison.Ordinal);
            if (extra != -1)
            {
                throw new Exception("Additional |] in source.");
            }

            codeBuilder.Append(markupCode.Substring(offset));

            spans = textSpans;
            code = codeBuilder.ToString();
        }

        [NotNull]
        private static Document GetDocument([NotNull] string code, [NotNull] string languageName,
            [NotNull] string fileName, [NotNull] string assemblyName,
            [NotNull] [ItemNotNull] ImmutableHashSet<MetadataReference> references,
            [CanBeNull] int? compilerWarningLevel)
        {
            CSharpCompilationOptions compilationOptions = compilerWarningLevel != null
                ? DefaultCompilationOptions.WithWarningLevel(compilerWarningLevel.Value)
                : DefaultCompilationOptions;

            return new AdhocWorkspace()
                .AddProject(assemblyName, languageName)
                .WithCompilationOptions(compilationOptions)
                .WithParseOptions(DefaultParseOptions)
                .AddMetadataReferences(references)
                .AddDocument(fileName, code);
        }
    }
}