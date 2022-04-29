using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Test.RoslynTestFramework
{
    internal sealed class FixableDocument
    {
        // Supported markers:
        //
        //   [|demo|] marks the span "demo"
        //   [+demo+] expects "demo" will be inserted
        //   [-demo-] expects "demo" will be removed
        //   [*before##after*] expects "before" will be replaced with "after"
        //
        // Example:
        // Input:    [|ab|][|c|][+INS+]def[-DEL-]ghi[*YYY##ZZZ*]jkl
        // Source:   abcdefDELghiYYYjkl
        // Expected: abcINSdefghiZZZjkl
        // Spans:    [0..2], [2..3]

        [NotNull]
        [ItemNotNull]
        private readonly IList<TextBlock> blocks;

        [NotNull]
        public string SourceText
        {
            get
            {
                IEnumerable<string> beforeBlocks = blocks.Select(block => block.TextBefore);
                return string.Concat(beforeBlocks);
            }
        }

        [NotNull]
        public IList<TextSpan> SourceSpans { get; }

        public FixableDocument([NotNull] string text)
        {
            FrameworkGuard.NotNull(text, nameof(text));

            var parser = new MarkupParser(text);
            parser.Parse();

            blocks = parser.TextBlocks;
            SourceSpans = parser.TextSpans.OrderBy(span => span).ToImmutableArray();
        }

        private sealed class MarkupParser
        {
            private const string SpanOpenText = "[";
            private const string SpanCloseText = "]";
            private const string ReplaceSeparator = "##";
            private const int SpanTextLength = 2;

            [NotNull]
            private static readonly char[] SpanKinds =
            {
                '|',
                '+',
                '-',
                '*'
            };

            [NotNull]
            [ItemNotNull]
            private static readonly string[] ReplaceSeparatorArray =
            {
                ReplaceSeparator
            };

            [NotNull]
            private readonly string markupCode;

            [NotNull]
            [ItemNotNull]
            public IList<TextBlock> TextBlocks { get; } = new List<TextBlock>();

            [NotNull]
            public IList<TextSpan> TextSpans { get; } = new List<TextSpan>();

            public MarkupParser([NotNull] string markupCode)
            {
                FrameworkGuard.NotNull(markupCode, nameof(markupCode));
                this.markupCode = markupCode;
            }

            public void Parse()
            {
                TextBlocks.Clear();
                TextSpans.Clear();

                ParseMarkupCode();

                CalculateSpans();
            }

            private void ParseMarkupCode()
            {
                var stateMachine = new ParseStateMachine(this);
                stateMachine.Run();
            }

            private void CalculateSpans()
            {
                int offset = 0;

                foreach (TextBlock block in TextBlocks)
                {
                    int blockLength = block.TextBefore.Length;

                    if (block is MarkedTextBlock)
                    {
                        TextSpan span = TextSpan.FromBounds(offset, offset + blockLength);
                        TextSpans.Add(span);
                    }

                    offset += blockLength;
                }
            }

            private (int spanStartIndex, char spanKind) GetNextSpanStart(int offset)
            {
                int startIndex = offset;

                while (true)
                {
                    int index = markupCode.IndexOf(SpanOpenText, startIndex, StringComparison.Ordinal);

                    (int, char)? result = TryGetNextSpanStartForIndex(index);

                    if (result != null)
                    {
                        return result.Value;
                    }

                    startIndex = index + 1;
                }
            }

            [CanBeNull]
            private (int spanStartIndex, char spanKind)? TryGetNextSpanStartForIndex(int index)
            {
                if (index == -1 || index >= markupCode.Length - 1)
                {
                    return (-1, '?');
                }

                char spanKind = markupCode[index + 1];

                if (SpanKinds.Contains(spanKind))
                {
                    return (index, spanKind);
                }

                return null;
            }

            private int GetNextSpanEnd(int start, char spanKind)
            {
                string spanEndText = spanKind + SpanCloseText;

                int index = markupCode.IndexOf(spanEndText, start + SpanTextLength, StringComparison.Ordinal);

                if (index == -1)
                {
                    throw new Exception($"Missing '{spanEndText}' in source.");
                }

                return index;
            }

            private void AppendCodeBlock(int offset, int length)
            {
                if (length > 0)
                {
                    string text = markupCode.Substring(offset, length);
                    var block = new StaticTextBlock(text);
                    TextBlocks.Add(block);
                }
            }

            private void AppendTextSpan(int spanStartIndex, int spanEndIndex, char spanKind)
            {
                string spanInnerText = markupCode.Substring(spanStartIndex + SpanTextLength,
                    spanEndIndex - spanStartIndex - SpanTextLength);

                TextBlock textBlock = TryCreateTextBlockForSpan(spanInnerText, spanKind);

                if (textBlock != null)
                {
                    TextBlocks.Add(textBlock);
                }
            }

            [CanBeNull]
            private TextBlock TryCreateTextBlockForSpan([NotNull] string spanInnerText, char spanKind)
            {
                switch (spanKind)
                {
                    case '|':
                    {
                        return new MarkedTextBlock(spanInnerText);
                    }
                    case '+':
                    {
                        return new InsertedTextBlock(spanInnerText);
                    }
                    case '-':
                    {
                        return new DeletedTextBlock(spanInnerText);
                    }
                    case '*':
                    {
                        return CreateReplacedTextBlock(spanInnerText);
                    }
                }

                return null;
            }

            [NotNull]
            private static ReplacedTextBlock CreateReplacedTextBlock([NotNull] string spanInnerText)
            {
                string[] parts = spanInnerText.Split(ReplaceSeparatorArray, StringSplitOptions.None);

                if (parts.Length == 1)
                {
                    throw new Exception($"Missing '{ReplaceSeparator}' in source.");
                }

                if (parts.Length > 2)
                {
                    throw new Exception($"Multiple '{ReplaceSeparator}' in source.");
                }

                return new ReplacedTextBlock(parts[0], parts[1]);
            }

            private void AppendLastCodeBlock(int offset)
            {
                if (HasMoreText(offset))
                {
                    AssertLastSpanIsClosed(offset);

                    string text = markupCode.Substring(offset);

                    if (text.Length > 0)
                    {
                        var block = new StaticTextBlock(text);
                        TextBlocks.Add(block);
                    }
                }
            }

            private bool HasMoreText(int offset)
            {
                return markupCode.Length - offset > 0;
            }

            private void AssertLastSpanIsClosed(int offset)
            {
                foreach (char spanKind in SpanKinds)
                {
                    string spanEndText = spanKind + SpanCloseText;

                    int index = markupCode.IndexOf(spanEndText, offset, StringComparison.Ordinal);

                    if (index != -1)
                    {
                        throw new Exception($"Additional '{spanEndText}' found in source.");
                    }
                }
            }

            private struct ParseStateMachine
            {
                [NotNull]
                private readonly MarkupParser parser;

                private int offset;
                private int spanStartIndex;
                private int spanEndIndex;
                private char spanKind;

                private bool HasFoundSpanStart => spanStartIndex != -1;

                public ParseStateMachine([NotNull] MarkupParser parser)
                {
                    FrameworkGuard.NotNull(parser, nameof(parser));

                    this.parser = parser;
                    offset = -1;
                    spanStartIndex = -1;
                    spanEndIndex = -1;
                    spanKind = '?';
                }

                public void Run()
                {
                    LocateFirstSpanStart();

                    LoopOverText();

                    AppendCodeBlockAfterSpanEnd();
                }

                private void LocateFirstSpanStart()
                {
                    offset = 0;
                    (spanStartIndex, spanKind) = parser.GetNextSpanStart(offset);
                }

                private void LoopOverText()
                {
                    while (HasFoundSpanStart)
                    {
                        AppendCodeBlockBeforeSpanStart();

                        LocateNextSpanEnd();

                        AppendTextSpan();

                        LocateNextSpanStart();
                    }
                }

                private void AppendCodeBlockBeforeSpanStart()
                {
                    int length = spanStartIndex - offset;
                    parser.AppendCodeBlock(offset, length);
                }

                private void LocateNextSpanEnd()
                {
                    spanEndIndex = parser.GetNextSpanEnd(spanStartIndex, spanKind);
                }

                private void AppendTextSpan()
                {
                    parser.AppendTextSpan(spanStartIndex, spanEndIndex, spanKind);
                }

                private void LocateNextSpanStart()
                {
                    offset = spanEndIndex + SpanTextLength;
                    (spanStartIndex, spanKind) = parser.GetNextSpanStart(offset);
                }

                private void AppendCodeBlockAfterSpanEnd()
                {
                    parser.AppendLastCodeBlock(offset);
                }
            }
        }

        private abstract class TextBlock
        {
            [NotNull]
            public string TextBefore { get; }

            [NotNull]
            public string TextAfter { get; }

            protected TextBlock([NotNull] string textBefore, [NotNull] string textAfter)
            {
                FrameworkGuard.NotNull(textBefore, nameof(textBefore));
                FrameworkGuard.NotNull(textAfter, nameof(textAfter));

                TextBefore = textBefore;
                TextAfter = textAfter;
            }
        }

        private sealed class StaticTextBlock : TextBlock
        {
            public StaticTextBlock([NotNull] string text)
                : base(text, text)
            {
            }

            public override string ToString()
            {
                return TextBefore;
            }
        }

        private sealed class MarkedTextBlock : TextBlock
        {
            public MarkedTextBlock([NotNull] string textToMark)
                : base(textToMark, textToMark)
            {
            }

            public override string ToString()
            {
                return "|" + TextAfter;
            }
        }

        private sealed class InsertedTextBlock : TextBlock
        {
            public InsertedTextBlock([NotNull] string textToInsert)
                : base(string.Empty, textToInsert)
            {
            }

            public override string ToString()
            {
                return "+" + TextAfter;
            }
        }

        private sealed class DeletedTextBlock : TextBlock
        {
            public DeletedTextBlock([NotNull] string textToDelete)
                : base(textToDelete, string.Empty)
            {
            }

            public override string ToString()
            {
                return "-" + TextBefore;
            }
        }

        private sealed class ReplacedTextBlock : TextBlock
        {
            public ReplacedTextBlock([NotNull] string textBefore, [NotNull] string textAfter)
                : base(textBefore, textAfter)
            {
            }

            public override string ToString()
            {
                return TextBefore + "=>" + TextAfter;
            }
        }
    }
}
