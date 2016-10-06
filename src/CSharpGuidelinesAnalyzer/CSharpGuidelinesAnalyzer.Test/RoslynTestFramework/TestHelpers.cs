using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    internal static class TestHelpers
    {
        [NotNull]
        public static DocumentWithSpans GetDocumentAndSpansFromMarkup([NotNull] string markupCode,
            [NotNull] string languageName, [NotNull] [ItemNotNull] ImmutableList<MetadataReference> references,
            [NotNull] string fileName)
        {
            Guard.NotNull(markupCode, nameof(markupCode));
            Guard.NotNull(languageName, nameof(languageName));
            Guard.NotNull(references, nameof(references));
            Guard.NotNull(fileName, nameof(fileName));

            string code;
            IList<TextSpan> spans;
            GetCodeAndSpansFromMarkup(markupCode, out code, out spans);

            Document document = GetDocument(code, languageName, references, fileName);
            return new DocumentWithSpans(document, spans);
        }

        private static void GetCodeAndSpansFromMarkup([NotNull] string markupCode, [NotNull] out string code,
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
            [NotNull] [ItemNotNull] ImmutableList<MetadataReference> references, [NotNull] string fileName)
        {
            return new AdhocWorkspace()
                .AddProject("TestProject", languageName)
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithParseOptions(new CSharpParseOptions()
                    .WithFeatures(new[]
                    {
                        new KeyValuePair<string, string>("IOperation", "true")
                    }))
                .AddMetadataReferences(references)
                .AddDocument(fileName, code);
        }

        [NotNull]
        [ItemNotNull]
        public static IList<string> RemoveMarkupFrom([NotNull] [ItemNotNull] IList<string> expected,
            [NotNull] string language, bool reformat,
            [NotNull] [ItemNotNull] ImmutableList<MetadataReference> references, [NotNull] string fileName)
        {
            Guard.NotNull(expected, nameof(expected));
            Guard.NotNull(language, nameof(language));
            Guard.NotNull(references, nameof(references));
            Guard.NotNull(fileName, nameof(fileName));

            return expected.Select(text => RemoveMarkupFrom(text, language, reformat, references, fileName)).ToList();
        }

        [NotNull]
        private static string RemoveMarkupFrom([NotNull] string expected, [NotNull] string language, bool reformat,
            [NotNull] [ItemNotNull] ImmutableList<MetadataReference> references, [NotNull] string fileName)
        {
            Document document = GetDocumentAndSpansFromMarkup(expected, language, references, fileName).Document;
            SyntaxNode syntaxRoot = document.GetSyntaxRootAsync().Result;

            if (reformat)
            {
                SyntaxNode formattedSyntaxRoot = Formatter.Format(syntaxRoot, document.Project.Solution.Workspace);
                return formattedSyntaxRoot.ToFullString();
            }

            return syntaxRoot.ToFullString();
        }
    }
}