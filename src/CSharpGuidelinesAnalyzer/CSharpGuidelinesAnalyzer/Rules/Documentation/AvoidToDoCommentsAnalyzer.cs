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
    public sealed class AvoidToDoCommentsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2318";

        private const string Title = "Work tracking comment should be removed";
        private const string MessageFormat = "Work tracking comment should be removed.";
        private const string Description = "Don't use comments for tracking work to be done later.";
        private const string Category = "Documentation";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private const string TodoCommentToken = "TODO";

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            var commentAnalyzer = new TodoCommentAnalyzer();

            context.RegisterSyntaxTreeAction(c => commentAnalyzer.AnalyzeTodoComments(c));
        }

        private sealed class TodoCommentAnalyzer
        {
            // This class is a simplified variant, based on Roslyn's internal class:
            // Microsoft.CodeAnalysis.Editor.CSharp.TodoComments.CSharpTodoCommentService

            private static readonly int MultiLineCommentPostfixLength = "*/".Length;
            private const string SingleLineCommentPrefix = "//";

            public void AnalyzeTodoComments(SyntaxTreeAnalysisContext context)
            {
                SourceText text = context.Tree.GetText(context.CancellationToken);
                SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);

                foreach (SyntaxTrivia trivia in root.DescendantTrivia())
                {
                    context.CancellationToken.ThrowIfCancellationRequested();

                    if (ContainsComments(trivia))
                    {
                        ReportTodoComments(text, trivia, context);
                    }
                }
            }

            private bool ContainsComments(SyntaxTrivia trivia)
            {
                return PreprocessorHasComment(trivia) || IsSingleLineComment(trivia) || IsMultilineComment(trivia);
            }

            private void ReportTodoComments([NotNull] SourceText text, SyntaxTrivia trivia,
                SyntaxTreeAnalysisContext context)
            {
                if (PreprocessorHasComment(trivia))
                {
                    string message = trivia.ToString();

                    int index = message.IndexOf(SingleLineCommentPrefix, StringComparison.Ordinal);
                    int start = trivia.FullSpan.Start + index;

                    ReportTodoCommentInfoFromSingleLine(message.Substring(index), start, context);
                    return;
                }

                if (IsSingleLineComment(trivia))
                {
                    ProcessMultilineComment(text, trivia, 0, context);
                    return;
                }

                if (IsMultilineComment(trivia))
                {
                    ProcessMultilineComment(text, trivia, MultiLineCommentPostfixLength, context);
                    return;
                }

                throw new Exception("ExceptionUtilities.Unreachable");
            }

            private void ReportTodoCommentInfoFromSingleLine([NotNull] string message, int start,
                SyntaxTreeAnalysisContext context)
            {
                int index = GetCommentStartingIndex(message);
                if (index >= message.Length)
                {
                    return;
                }

                if (
                    string.Compare(message, index, TodoCommentToken, 0, TodoCommentToken.Length,
                        StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return;
                }

                if ((message.Length > index + TodoCommentToken.Length) &&
                    SyntaxFacts.IsIdentifierPartCharacter(message[index + TodoCommentToken.Length]))
                {
                    // they wrote something like:
                    //   todoboo
                    // instead of:
                    //   todo
                    return;
                }

                Location location = Location.Create(context.Tree,
                    TextSpan.FromBounds(start + index, start + message.Length));
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }

            private void ProcessMultilineComment([NotNull] SourceText text, SyntaxTrivia trivia, int postfixLength,
                SyntaxTreeAnalysisContext context)
            {
                TextSpan fullSpan = trivia.FullSpan;
                string fullString = trivia.ToFullString();

                TextLine startLine = text.Lines.GetLineFromPosition(fullSpan.Start);
                TextLine endLine = text.Lines.GetLineFromPosition(fullSpan.End);

                // single line multiline comments
                if (startLine.LineNumber == endLine.LineNumber)
                {
                    string message = postfixLength == 0
                        ? fullString
                        : fullString.Substring(0, fullSpan.Length - postfixLength);
                    ReportTodoCommentInfoFromSingleLine(message, fullSpan.Start, context);
                    return;
                }

                // multiline 
                string startMessage = text.ToString(TextSpan.FromBounds(fullSpan.Start, startLine.End));
                ReportTodoCommentInfoFromSingleLine(startMessage, fullSpan.Start, context);

                for (int lineNumber = startLine.LineNumber + 1; lineNumber < endLine.LineNumber; lineNumber++)
                {
                    TextLine line = text.Lines[lineNumber];
                    string message = line.ToString();

                    ReportTodoCommentInfoFromSingleLine(message, line.Start, context);
                }

                int length = fullSpan.End - endLine.Start;
                if (length >= postfixLength)
                {
                    length -= postfixLength;
                }

                string endMessage = text.ToString(new TextSpan(endLine.Start, length));
                ReportTodoCommentInfoFromSingleLine(endMessage, endLine.Start, context);
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

            private bool PreprocessorHasComment(SyntaxTrivia trivia)
            {
                return trivia.Kind() != SyntaxKind.RegionDirectiveTrivia &&
                    SyntaxFacts.IsPreprocessorDirective(trivia.Kind()) &&
                    trivia.ToString().IndexOf(SingleLineCommentPrefix, StringComparison.Ordinal) > 0;
            }

            private bool IsSingleLineComment(SyntaxTrivia trivia)
            {
                return trivia.Kind() == SyntaxKind.SingleLineCommentTrivia ||
                    trivia.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia;
            }

            private bool IsMultilineComment(SyntaxTrivia trivia)
            {
                return trivia.Kind() == SyntaxKind.MultiLineCommentTrivia ||
                    trivia.Kind() == SyntaxKind.MultiLineDocumentationCommentTrivia;
            }
        }
    }
}