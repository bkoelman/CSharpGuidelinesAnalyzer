using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpGuidelinesAnalyzer.Documentation
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
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description,
            helpLinkUri: HelpLinkUris.GetForCategory(Category));

        [ItemNotNull]
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

            SyntaxTrivia[] allCommentTrivia = context.CodeBlock.DescendantTrivia().Where(IsComment).ToArray();
            foreach (SyntaxTrivia commentTrivia in allCommentTrivia)
            {
                if (outerCommentTrivia.Contains(commentTrivia))
                {
                    continue;
                }

                if (IsResharperSuppression(commentTrivia))
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
    }
}