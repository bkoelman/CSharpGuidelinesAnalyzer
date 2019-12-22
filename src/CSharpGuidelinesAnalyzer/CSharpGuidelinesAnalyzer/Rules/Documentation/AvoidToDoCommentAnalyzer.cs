using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace CSharpGuidelinesAnalyzer.Rules.Documentation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidToDoCommentAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2318";

        private const string Title = "Work-tracking TODO comment should be removed";
        private const string MessageFormat = "Work-tracking TODO comment should be removed.";
        private const string Description = "Don't use comments for tracking work to be done later.";

        [NotNull]
        private static readonly AnalyzerCategory Category = AnalyzerCategory.Documentation;

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category.DisplayName, DiagnosticSeverity.Info, true, Description, Category.GetHelpLinkUri(DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private const string TodoCommentToken = "TODO";

        [NotNull]
        private static readonly Action<SyntaxTreeAnalysisContext> AnalyzeTodoCommentsAction = AnalyzeTodoComments;

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxTreeAction(AnalyzeTodoCommentsAction);
        }

        private static void AnalyzeTodoComments(SyntaxTreeAnalysisContext context)
        {
            SourceText text = context.Tree.GetText(context.CancellationToken);
            SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);

            var analyzer = new TodoCommentAnalyzer(text, context);

            foreach (SyntaxTrivia trivia in root.DescendantTrivia())
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                analyzer.Analyze(trivia);
            }
        }

        private sealed class TodoCommentAnalyzer
        {
            private const string SingleLineCommentPrefix = "//";
            private static readonly int MultiLineCommentPostfixLength = "*/".Length;

            [NotNull]
            private readonly SourceText text;

            private SyntaxTreeAnalysisContext context;

            public TodoCommentAnalyzer([NotNull] SourceText text, SyntaxTreeAnalysisContext context)
            {
                Guard.NotNull(text, nameof(text));

                this.text = text;
                this.context = context;
            }

            public void Analyze(SyntaxTrivia trivia)
            {
                if (PreprocessorHasSingleLineComment(trivia))
                {
                    ProcessSingleLineCommentAfterPreprocessor(trivia);
                }
                else if (IsSingleLineComment(trivia))
                {
                    ProcessCommentOnSingleOrMultipleLines(trivia, 0);
                }
                else if (IsMultilineComment(trivia))
                {
                    ProcessCommentOnSingleOrMultipleLines(trivia, MultiLineCommentPostfixLength);
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                    // No action required.
                }
            }

            private void ProcessSingleLineCommentAfterPreprocessor(SyntaxTrivia trivia)
            {
                string message = trivia.ToString();

                int index = message.IndexOf(SingleLineCommentPrefix, StringComparison.Ordinal);
                int start = trivia.FullSpan.Start + index;

                ReportTodoCommentFromSingleLine(message.Substring(index), start);
            }

            private void ReportTodoCommentFromSingleLine([NotNull] string message, int start)
            {
                int index = GetCommentStartingIndex(message);
                if (index >= message.Length)
                {
                    return;
                }

                if (!StartsWithTodoCommentToken(message, index) || HasIdentifierCharacterAfterTodoCommentToken(message, index))
                {
                    return;
                }

                var location = Location.Create(context.Tree, TextSpan.FromBounds(start + index, start + message.Length));
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }

            private static bool StartsWithTodoCommentToken([NotNull] string message, int index)
            {
                return string.Compare(message, index, TodoCommentToken, 0, TodoCommentToken.Length,
                    StringComparison.OrdinalIgnoreCase) == 0;
            }

            private static bool HasIdentifierCharacterAfterTodoCommentToken([NotNull] string message, int index)
            {
                return message.Length > index + TodoCommentToken.Length &&
                    SyntaxFacts.IsIdentifierPartCharacter(message[index + TodoCommentToken.Length]);
            }

            private void ProcessCommentOnSingleOrMultipleLines(SyntaxTrivia trivia, int postfixLength)
            {
                TextSpan fullSpan = trivia.FullSpan;
                string fullString = trivia.ToFullString();

                TextLine startLine = text.Lines.GetLineFromPosition(fullSpan.Start);
                TextLine endLine = text.Lines.GetLineFromPosition(fullSpan.End);

                if (startLine.LineNumber == endLine.LineNumber)
                {
                    ProcessCommentOnSingleLine(fullString, fullSpan, postfixLength);
                }
                else
                {
                    ProcessCommentOnMultipleLines(fullSpan, new LineRange(startLine, endLine), postfixLength);
                }
            }

            private void ProcessCommentOnSingleLine([NotNull] string fullString, TextSpan fullSpan, int postfixLength)
            {
                string message = postfixLength == 0 ? fullString : fullString.Substring(0, fullSpan.Length - postfixLength);
                ReportTodoCommentFromSingleLine(message, fullSpan.Start);
            }

            private void ProcessCommentOnMultipleLines(TextSpan fullSpan, LineRange range, int postfixLength)
            {
                ProcessFirstLine(fullSpan, range.StartLine);
                ProcessNextLines(range.StartLine, range.EndLine);
                ProcessLastLine(fullSpan, range.EndLine, postfixLength);
            }

            private void ProcessFirstLine(TextSpan fullSpan, TextLine startLine)
            {
                string firstMessage = text.ToString(TextSpan.FromBounds(fullSpan.Start, startLine.End));
                ReportTodoCommentFromSingleLine(firstMessage, fullSpan.Start);
            }

            private void ProcessNextLines(TextLine startLine, TextLine endLine)
            {
                for (int lineNumber = startLine.LineNumber + 1; lineNumber < endLine.LineNumber; lineNumber++)
                {
                    TextLine line = text.Lines[lineNumber];
                    string nextMessage = line.ToString();
                    ReportTodoCommentFromSingleLine(nextMessage, line.Start);
                }
            }

            private void ProcessLastLine(TextSpan fullSpan, TextLine endLine, int postfixLength)
            {
                int length = fullSpan.End - endLine.Start;
                if (length >= postfixLength)
                {
                    length -= postfixLength;
                }

                string lastMessage = text.ToString(new TextSpan(endLine.Start, length));
                ReportTodoCommentFromSingleLine(lastMessage, endLine.Start);
            }

            private int GetCommentStartingIndex([NotNull] string message)
            {
                for (int index = 0; index < message.Length; index++)
                {
                    char ch = message[index];
                    if (!SyntaxFacts.IsWhitespace(ch) && ch != '*' && ch != '/')
                    {
                        return index;
                    }
                }

                return message.Length;
            }

            private bool PreprocessorHasSingleLineComment(SyntaxTrivia trivia)
            {
                SyntaxKind kind = trivia.Kind();

                return kind != SyntaxKind.RegionDirectiveTrivia && SyntaxFacts.IsPreprocessorDirective(kind) &&
                    trivia.ToString().IndexOf(SingleLineCommentPrefix, StringComparison.Ordinal) > 0;
            }

            private bool IsSingleLineComment(SyntaxTrivia trivia)
            {
                SyntaxKind kind = trivia.Kind();

                return kind == SyntaxKind.SingleLineCommentTrivia || kind == SyntaxKind.SingleLineDocumentationCommentTrivia;
            }

            private bool IsMultilineComment(SyntaxTrivia trivia)
            {
                SyntaxKind kind = trivia.Kind();

                return kind == SyntaxKind.MultiLineCommentTrivia || kind == SyntaxKind.MultiLineDocumentationCommentTrivia;
            }

            private struct LineRange
            {
                public TextLine StartLine { get; }
                public TextLine EndLine { get; }

                public LineRange(TextLine startLine, TextLine endLine)
                {
                    StartLine = startLine;
                    EndLine = endLine;
                }
            }
        }
    }
}
