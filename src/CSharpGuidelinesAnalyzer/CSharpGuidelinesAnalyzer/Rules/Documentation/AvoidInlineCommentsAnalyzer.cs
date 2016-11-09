using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Rules.Documentation
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AvoidInlineCommentsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "AV2310";

        private const string Title = "Code blocks should not contain inline comments";
        private const string MessageFormat = "Code blocks should not contain inline comments.";
        private const string Description = "Avoid inline comments.";
        private const string Category = "Documentation";

        [NotNull]
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Description, HelpLinkUris.GetForCategory(Category, DiagnosticId));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        [ItemNotNull]
        private static readonly ImmutableArray<string> ArrangeActAssertLines =
            new[] { "// Arrange", "// Act", "// Assert", "// Act and assert" }.ToImmutableArray();

        public override void Initialize([NotNull] AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCodeBlockAction(AnalyzeCodeBlock);
        }

        private void AnalyzeCodeBlock(CodeBlockAnalysisContext context)
        {
            SyntaxTrivia[] outerCommentTrivia =
                context.CodeBlock.GetLeadingTrivia()
                    .Concat(context.CodeBlock.GetTrailingTrivia())
                    .Where(IsComment)
                    .ToArray();

            foreach (SyntaxTrivia commentTrivia in context.CodeBlock.DescendantTrivia().Where(IsComment))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (outerCommentTrivia.Contains(commentTrivia))
                {
                    continue;
                }

                if (IsResharperSuppression(commentTrivia))
                {
                    continue;
                }

                if (IsArrangeActAssertUnitTestPattern(commentTrivia))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Rule, commentTrivia.GetLocation()));
            }
        }

        private static bool IsComment(SyntaxTrivia trivia)
        {
            return trivia.Kind() == SyntaxKind.SingleLineCommentTrivia ||
                trivia.Kind() == SyntaxKind.MultiLineCommentTrivia;
        }

        private bool IsResharperSuppression(SyntaxTrivia commentTrivia)
        {
            string text = commentTrivia.ToString();
            return text.Contains("// ReSharper disable ") || text.Contains("// ReSharper restore ");
        }

        private bool IsArrangeActAssertUnitTestPattern(SyntaxTrivia commentTrivia)
        {
            string text = commentTrivia.ToString();
            return ArrangeActAssertLines.Any(line => line.Equals(text));
        }
    }
}