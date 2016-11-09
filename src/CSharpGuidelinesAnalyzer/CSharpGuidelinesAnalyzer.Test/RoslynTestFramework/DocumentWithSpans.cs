using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    /// <summary />
    internal class DocumentWithSpans
    {
        [NotNull]
        public Document Document { get; }

        [NotNull]
        public IList<TextSpan> TextSpans { get; }

        public DocumentWithSpans([NotNull] Document document, [NotNull] IList<TextSpan> textSpans)
        {
            Guard.NotNull(document, nameof(document));
            Guard.NotNull(textSpans, nameof(textSpans));

            Document = document;
            TextSpans = textSpans;
        }
    }
}